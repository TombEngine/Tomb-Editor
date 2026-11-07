using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Wad
{
    public static class WadSoundPlayer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int maxChannels = 5;

        private static Random _rng = new Random();

        private static int[] _indices = new int[maxChannels];
        private static WaveOutEvent[] _channels = new WaveOutEvent[maxChannels];

        static WadSoundPlayer()
        {
            for (int i = 0; i < maxChannels; i++)
            {
                _indices[i] = -1;
                _channels[i] = null;
            }
        }

        public static int GetFreeChannel()
        {
            for (int i = 0; i < maxChannels; i++)
                if (_channels[i] == null) return i;

            for (int i = 0; i < maxChannels; i++)
                if (_channels[i].PlaybackState != PlaybackState.Playing)
                {
                    StopSample(i);
                    return i;
                }

            StopSample(0);
            return 0; // Just return first array index, we don't need smart queuing for previews
        }

        public static int FindPlayingIndex(int index)
        {
            for (int i = 0; i < maxChannels; i++)
            {
                if (_indices[i] == index && _channels[i] != null &&
                    _channels[i].PlaybackState == PlaybackState.Playing)
                    return i;
            }
            return -1;
        }

        public static void PlaySoundInfo(Level level, WadSoundInfo soundInfo)
        {
            if (soundInfo == null || soundInfo.Samples == null || soundInfo.Samples.Count == 0)
                return;

            int channelIndex = FindPlayingIndex(soundInfo.Id);

            if (channelIndex >= 0)
            {
                switch (soundInfo.LoopBehaviour)
                {
                    case WadSoundLoopBehaviour.Looped:
                    case WadSoundLoopBehaviour.OneShotWait:
                        return;

                    case WadSoundLoopBehaviour.OneShotRewound:
                        StopSample(channelIndex);
                        break;

                    case WadSoundLoopBehaviour.None:
                        channelIndex = GetFreeChannel();
                        break;
                }
            }
            else
                channelIndex = GetFreeChannel();

            _indices[channelIndex] = soundInfo.Id;

            // Figure out the precise play parameters
            int sampleIndex;
            int loopCount = soundInfo.LoopBehaviour == WadSoundLoopBehaviour.Looped ? 3 : 1;
            float pan = 0.0f;
            float volume = soundInfo.Volume / 100.0f;
            float pitch = (soundInfo.PitchFactor / 100.0f) + 1.0f;
            float chance = soundInfo.Chance == 0 ? 1.0f : soundInfo.Chance / 100.0f;

            lock (_rng)
            {
                if (chance != 1.0f && _rng.NextDouble() > chance)
                    return;

                sampleIndex = _rng.Next(0, soundInfo.Samples.Count);
                if (sampleIndex == soundInfo.Samples.Count)
                    sampleIndex = soundInfo.Samples.Count - 1;

                if (!soundInfo.DisablePanning)
                    pan = (float)((_rng.NextDouble() - 0.5f) * 1.6);
                if (soundInfo.RandomizePitch)
                    pitch += (float)(_rng.NextDouble() * (6000.0 / 65536.0));
                if (soundInfo.RandomizeVolume)
                    volume -= (float)_rng.NextDouble() * 0.125f;
            }

            PlaySample(level, soundInfo.Samples[sampleIndex], channelIndex, volume, pitch, pan, loopCount);
        }

        public static void PlaySoundtrack(Level level, string path)
        {
            path = Path.Combine(level.Settings.MakeAbsolute(level.Settings.GameDirectory), WadSounds.AudioFolder, path);

            WaveStream reader = null;
            var disposables = new List<IDisposable>();

            try
            {
                foreach (WadSounds.SoundtrackFormat ext in Enum.GetValues(typeof(WadSounds.SoundtrackFormat)))
                {
                    var potentialPath = Path.ChangeExtension(path, ext.ToString());
                    if (File.Exists(potentialPath))
                    {
                        switch (ext)
                        {
                            case WadSounds.SoundtrackFormat.Wav:
                                reader = disposables.AddAndReturn(new WaveFileReader(potentialPath));
                                break;

                            case WadSounds.SoundtrackFormat.Mp3:
                                reader = disposables.AddAndReturn(new Mp3FileReaderBase(potentialPath, format => new AcmMp3FrameDecompressor(format)));
                                break;

                            case WadSounds.SoundtrackFormat.Ogg:
                                reader = disposables.AddAndReturn(new VorbisWaveReader(potentialPath));
                                break;

                            default:
                                return;
                        }

                        break;
                    }
                }

                var sourceStream = ResampleWaveStream(reader);
                SetupChannelForPlayback(GetFreeChannel(), sourceStream.ToSampleProvider(), disposables);
            }
            catch (Exception ex)
            {
                // Clean up in case of a problem
                foreach (IDisposable disposable in disposables)
                    disposable?.Dispose();

                _logger.Error("Error while playing soundtrack " + path + ", exception: " + ex);
            }
        }

        public static void PlaySample(Level level, WadSample sample, int channel, float volume = 1.0f, float pitch = 1.0f, float pan = 0.0f, int loopCount = 1)
        {
            if (volume <= 0.0f || loopCount <= 0)
                return;

            var disposables = new List<IDisposable>();
            try
            {
                // Load data.
                // If waveform data is loaded into memory, use it to play sound, otherwise find wav file on disk.

                WaveFileReader waveStream;
                MemoryStream memoryStream;

                if (sample.IsLoaded)
                {
                    memoryStream = disposables.AddAndReturn(new MemoryStream(sample.Data, false));
                    waveStream = disposables.AddAndReturn(new WaveFileReader(memoryStream));
                }
                else
                {
                    var path = WadSounds.TryGetSamplePath(level.Settings.GetRecursiveListOfSoundPaths(), sample.FileName);
                    if (path == null)
                        return;
                    else
                        waveStream = disposables.AddAndReturn(new WaveFileReader(path));
                }

                // Resample if necessary
                var sourceStream = ResampleWaveStream(waveStream);

                // Apply looping
                ISampleProvider sampleStream;
                if (loopCount <= 1)
                    sampleStream = sourceStream.ToSampleProvider();
                else
                    sampleStream = new RepeatedStream(sourceStream) { LoopCount = loopCount };

                // Apply pitch
                if (pitch != 1.0f)
                    sampleStream = new PitchedStream { Source = sampleStream, Pitch = pitch };

                // Apply volume
                if (volume != 1.0f)
                    sampleStream = new VolumeSampleProvider(sampleStream) { Volume = volume };

                // Add some silence to make sure the audio plays out.
                const int latencyInMilliseconds = 200;
                int latencyInSamples = (sampleStream.WaveFormat.SampleRate * latencyInMilliseconds * 2) / 1000;
                sampleStream = new OffsetSampleProvider(sampleStream) { LeadOutSamples = sampleStream.WaveFormat.Channels * latencyInSamples };

                SetupChannelForPlayback(channel, sampleStream, disposables);
            }
            catch (Exception ex)
            {
                // Clean up in case of a problem
                foreach (IDisposable disposable in disposables)
                    disposable?.Dispose();

                _logger.Error("Error while playing sample " + sample + ", exception: " + ex);
            }
        }

        private static WaveStream ResampleWaveStream(WaveStream originalStream)
        {
            if (originalStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ||
                originalStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                return originalStream;
            }
            else
            {
                var resampleFormat = new WaveFormat(originalStream.WaveFormat.SampleRate, 16,
                                                    originalStream.WaveFormat.Channels);
                return new WaveFormatConversionStream(resampleFormat, originalStream);
            }
        }

        private static void SetupChannelForPlayback(int channel, ISampleProvider stream, List<IDisposable> disposables)
        {
            _channels[channel] = disposables.AddAndReturn(new WaveOutEvent());

            // HACK: In some user setups, default device (-1) is incorrectly identified by NAudio and results in exception.
            // Try to fall back to first available device if default device throws an exception.

            try
            {
                _channels[channel].Init(stream);
            }
            catch
            {
                _channels[channel].DeviceNumber = 0;
                _channels[channel].Init(stream);
            }

            // Setup stop event
            _channels[channel].PlaybackStopped += (s, e) =>
            {
                foreach (IDisposable disposable in disposables)
                    disposable?.Dispose();
                _channels[channel] = null;
                _indices[channel] = -1;
            };

            _channels[channel].Play();
        }

        public static void StopSample(int index = -1)
        {
            for (int i = 0; i < maxChannels; i++)
                if (index == -1 || index == i)
                    if (_channels[i] != null)
                    {
                        _channels[i].Stop();
                        _channels[i].Dispose();
                    }
        }

        private class RepeatedStream : ISampleProvider
        {
            public int LoopCount;
            private ISampleProvider _provider;
            private WaveStream _waveStream;

            public RepeatedStream(WaveStream waveStream)
            {
                _waveStream = waveStream;
                _provider = waveStream.ToSampleProvider();
            }

            public WaveFormat WaveFormat => _provider.WaveFormat;

            public int Read(float[] buffer, int offset, int count)
            {
                int read = 0;
                while (LoopCount > 0)
                {
                    read += _provider.Read(buffer, offset + read, count - read);
                    if (read >= count)
                        return read;

                    LoopCount--;
                    _waveStream.Seek(0, SeekOrigin.Begin);
                }
                return read;
            }
        }

        private class PitchedStream : ISampleProvider
        {
            public ISampleProvider Source;
            public float Pitch;

            public WaveFormat WaveFormat
            {
                get
                {
                    WaveFormat sourceFormat = Source.WaveFormat;
                    float newSampleRate = Math.Max(40, Math.Min(int.MaxValue, sourceFormat.SampleRate * Pitch + 0.5f));
                    return WaveFormat.CreateIeeeFloatWaveFormat((int)newSampleRate, sourceFormat.Channels);
                }
            }

            public int Read(float[] buffer, int offset, int count) => Source.Read(buffer, offset, count);
        }
    }
}
