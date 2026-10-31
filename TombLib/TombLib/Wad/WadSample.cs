using NAudio.Wave;
using NAudio.Flac;
using NAudio.Vorbis;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using TombLib.Utils;
using System.Xml.Serialization;
using TombLib.LevelData;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public class WadSample : IEquatable<WadSample>
    {
        // Samples are loaded only at compile phase, in all other cases we just need paths, with the only exception of 
        // TR1-2-3-4-5 files that have samples embedded in levels or in MAIN.SFX, in this case we have them in memory 
        // and we can provide the ability to save them on disk.
        [XmlIgnore]
        public bool IsLoaded => Data != null && Data.Length > 0;

        // Properties for correct encoding of samples.
        [XmlIgnore]
        private const int ChannelCountOffset = 22;
        [XmlIgnore]
        private const int SampleRateOffset = 24;
        [XmlIgnore]
        private const int SampleBytesPerSecondOffset = 28;
        [XmlIgnore]
        private const int BitsPerSampleOffset = 34;

        // RAW sample data, with hash
        [XmlIgnore]
        public byte[] Data { get; }
        [XmlIgnore]
        public Hash Hash { get; }

        // Path of this sample. This must be absolute path and this is the only field that must be serialized to XML.
        public string FileName { get; set; }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Only for XML serialization!
        public WadSample() { }

        public WadSample(string filename)
        {
            FileName = filename;
        }

        public WadSample(string filename, byte[] data)
        {
            if (CheckSampleDataForFormat(data) < 0)
                throw new NotSupportedException("Sample data is of an unsupported format.");
            Data = data;
            Hash = Hash.FromByteArray(Data);
            FileName = filename;
        }

        public static int CheckSampleDataForFormat(byte[] data, WaveFormat format = null)
        {
            // Based on: https://de.wikipedia.org/wiki/RIFF_WAVE
            // Just a very simple, cheap but strict checking routine to make sure
            // we don't get strange wave files in an *.wad2 or the game itself
            // that we then (or other tools) have to support later on.
            try
            {
                uint riffSignature = BitConverter.ToUInt32(data, 0);
                uint fileSize = BitConverter.ToUInt32(data, 4);
                uint waveSignature = BitConverter.ToUInt32(data, 8);
                if (riffSignature != 0x46464952)
                    return -1;
                if (fileSize != (data.Length - 8))
                    return -1;
                if (waveSignature != 0x45564157)
                    return -1;

                uint fmt_Signature = BitConverter.ToUInt32(data, 12);
                uint fmtLength = BitConverter.ToUInt32(data, 16);
                ushort formatTag = BitConverter.ToUInt16(data, 20);
                ushort channelCount = BitConverter.ToUInt16(data, ChannelCountOffset);
                uint sampleRate = BitConverter.ToUInt32(data, SampleRateOffset);
                uint bytesPerSecond = BitConverter.ToUInt32(data, SampleBytesPerSecondOffset);
                ushort blockAlign = BitConverter.ToUInt16(data, 32);
                ushort bitsPerSample = BitConverter.ToUInt16(data, BitsPerSampleOffset);

                if (fmt_Signature != 0x20746D66)
                    return -1;
                if (fmtLength != 16) // File generated with NAudio have a 18 bit header. Tomb Raider does not support this!
                    return -1;
                if (formatTag != 1) // We want default PCM
                    return -1;
                if (bytesPerSecond != (sampleRate * blockAlign))
                    return -1;
                if (blockAlign != ((bitsPerSample * channelCount) / 8))
                    return -1;

                if (format != null)
                {
                    if (channelCount != format.Channels)
                        return -1;
                    if (sampleRate != format.SampleRate)
                        return -1;
                    if (bitsPerSample != format.BitsPerSample)
                        return -1;
                }

                uint dataSignature = BitConverter.ToUInt32(data, 20 + (int)fmtLength);
                uint dataLength = BitConverter.ToUInt32(data, 24 + (int)fmtLength);
                if (dataSignature != 0x61746164)
                    return -1;
                if (dataLength != (data.Length - (28 + (int)fmtLength)))
                    return (int)dataLength + (28 + (int)fmtLength);
                return data.Length;
            }
            catch
            {
                return -1;
            }
        }

        [XmlIgnore]
        public TimeSpan Duration
        {
            get
            {
                using (var stream = new MemoryStream(Data))
                using (var wfr = new WaveFileReader(stream))
                    return wfr.TotalTime;
            }
        }

        [XmlIgnore]
        public uint SampleRate => BitConverter.ToUInt32(Data, SampleRateOffset);
        [XmlIgnore]
        public uint ChannelCount => BitConverter.ToUInt16(Data, ChannelCountOffset);
        [XmlIgnore]
        public uint BitsPerSample => BitConverter.ToUInt16(Data, BitsPerSampleOffset);

        public static byte[] ConvertSampleFormat(byte[] data, int sampleRate, int bitsPerSample)
        {
            var targetFormat = new WaveFormat(sampleRate, bitsPerSample, 1);
            if (CheckSampleDataForFormat(data, targetFormat) > 0)
                return data;

            // Use NAudio now to convert the audio data
            using (var inStream = new MemoryStream(data, false))
            using (var anyWaveStream = CreateReader(inStream, data))
            {
                // If loaded file is already in PCM or IEEE format, use it.
                // Otherwise, run a conversion through ACM because media foundation resampler can't convert
                // the file automatically for whatever reason.

                WaveStream sourceStream;
                if (anyWaveStream.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ||
                    anyWaveStream.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    sourceStream = anyWaveStream;
                else
                {
                    var resampleFormat = new WaveFormat(anyWaveStream.WaveFormat.SampleRate, 16, 
                                                        anyWaveStream.WaveFormat.Channels);
                    sourceStream = new WaveFormatConversionStream(resampleFormat, anyWaveStream);
                }

                var resampler = new MediaFoundationResampler(sourceStream, targetFormat) { ResamplerQuality = 60 };

                try // The pcm stream may need to get disposed.
                {
                    using (var outStream = new MemoryStream())
                    {
                        // NAudio's WaveFileWriter produces incompatible files with an extra 2 bytes in the fmt header.
                        // This breaks the sounds in the TR4 engine.
                        // To work around the issue, write the wave header ourselves...
                        BinaryWriter writer = new BinaryWriter(outStream);
                        writer.Write((uint)0x46464952); // riff
                        writer.Write((uint)0);
                        writer.Write((uint)0x45564157); // wave

                        // fmt chunk
                        writer.Write((uint)0x20746D66); // fmt
                        writer.Write((uint)16);
                        writer.Write((ushort)targetFormat.Encoding);
                        writer.Write((ushort)targetFormat.Channels);
                        writer.Write((uint)targetFormat.SampleRate);
                        writer.Write((uint)targetFormat.AverageBytesPerSecond);
                        writer.Write((ushort)targetFormat.BlockAlign);
                        writer.Write((ushort)targetFormat.BitsPerSample);

                        // data chunk
                        writer.Write(0x61746164); // data
                        writer.Write((uint)0); // placeholder
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) != 0)
                                outStream.Write(buffer, 0, bytesRead);
                        }

                        // Fix up file sizes
                        writer.BaseStream.Position = 4;
                        writer.Write((uint)(writer.BaseStream.Length - 8));
                        writer.BaseStream.Position = 40;
                        writer.Write((uint)(writer.BaseStream.Length - 44));

                        return outStream.ToArray();
                    }
                }
                finally
                {
                    (resampler as IDisposable)?.Dispose();
                    (sourceStream as IDisposable)?.Dispose();
                }
            }
        }

        private static WaveStream CreateReader(MemoryStream dataStream, byte[] data)
        {
            uint startWord = BitConverter.ToUInt32(data, 0);
            if (startWord == 0x46464952) // "riff"
                return new WaveFileReader(dataStream);
            else if (startWord == 0x5367674f) // "OggS"
                return new VorbisWaveReader(dataStream);
            else if (startWord == 0x43614C66) // "fLaC"
                return new FlacReader(dataStream);
            else if (startWord == 0x4D524F46) // "FORM"
                return new FlacReader(dataStream);

            // Mp3 files are hard to identify,
            // so let's just give that a shot if it's obviously not one of the others.
            return new Mp3FileReaderBase(dataStream, format => new AcmMp3FrameDecompressor(format));
        }

        public byte[] CompressToMsAdpcm(uint overwriteSampleRate, out int uncompressedSize) =>
            CompressToMsAdpcm(Data, overwriteSampleRate, out uncompressedSize);
        public static byte[] CompressToMsAdpcm(byte[] data, uint overwriteSampleRate, out int uncompressedSize)
        {
            WaveFormat pcmFormat = new WaveFormat((int)overwriteSampleRate, 16, 1);
            AdpcmWaveFormat adpcmFormat = new AdpcmWaveFormat((int)overwriteSampleRate, 1);

            using (var inStream = new MemoryStream(data))
            using (var anyWaveStream = new WaveFileReader(inStream))
            using (var pcmStream = new RawSourceWaveStream(anyWaveStream, pcmFormat))
            {
                int sampleSize = ((pcmStream.WaveFormat.BitsPerSample * pcmStream.WaveFormat.Channels) / 8);
                int uncompressedSampleCount = (int)(pcmStream.Length / sampleSize);
                uncompressedSampleCount = AlignTo(uncompressedSampleCount, adpcmFormat.SamplesPerBlock);
                uncompressedSize = uncompressedSampleCount * 2; // Time 2 because 16 bit mono samples (2 byte per sample)

                // We have to align the wave data to the wave block size
                // otherise NAudio will just cut off some samples!
                using (var alignedPcmStream = new AlignStream { _baseStream = pcmStream, _extendedLengthInBytes = uncompressedSize })
                using (var adpcmStream = new WaveFormatConversionStream(adpcmFormat, alignedPcmStream))
                using (var outStream = new MemoryStream())
                {
                    using (WaveFileWriter outWaveFileformat = new WaveFileWriter(outStream, adpcmFormat))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = adpcmStream.Read(buffer, 0, buffer.Length)) != 0)
                            outWaveFileformat.Write(buffer, 0, bytesRead);
                    }
                    return outStream.ToArray();
                }
            }
        }

        private class AlignStream : WaveStream
        {
            public long _extendedLengthInBytes;
            public WaveStream _baseStream;
            private long _currentPosition = 0;

            public override long Length => _extendedLengthInBytes;
            public override long Position
            {
                get { return _currentPosition; }
                set { throw new NotImplementedException(); }
            }
            public override WaveFormat WaveFormat => _baseStream.WaveFormat;
            public override int Read(byte[] buffer, int offset, int count)
            {
                long baseStreamLength = _baseStream.Length;

                // Read base stream
                int readInTotal = 0;
                if (_currentPosition < baseStreamLength)
                {
                    int read = _baseStream.Read(buffer, offset, count);
                    offset += read;
                    count -= read;
                    _currentPosition += read;
                    readInTotal += read;
                }

                // Extend base stream with new samples
                if (_currentPosition >= baseStreamLength)
                {
                    int extraByteCount = (int)(_extendedLengthInBytes - _currentPosition);
                    int read = Math.Min(extraByteCount, count);
                    Array.Clear(buffer, offset, read);
                    offset += read;
                    count -= read;
                    _currentPosition += read;
                    readInTotal += read;
                }

                return readInTotal;
            }
        }

        private static int AlignTo(int value, int alignment)
        {
            return ((value + alignment - 1) / alignment) * alignment;
        }

        public static bool operator ==(WadSample first, WadSample second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : (first.Hash == second.Hash));
        public static bool operator !=(WadSample first, WadSample second) => !(first == second);
        public bool Equals(WadSample other) => Hash == other.Hash;
        public override bool Equals(object other) => (other is WadSample) && Hash == ((WadSample)other).Hash;
        public override int GetHashCode() { return Hash.GetHashCode(); }

        public static string LookupSound(string soundName, bool ignoreMissingSounds, string wadPath, List<string> wadSoundPaths)
        {
            if (!Path.HasExtension(soundName))
                soundName += ".wav";

            foreach (var wadSoundPath in wadSoundPaths)
            {
                string realPath = Path.Combine(wadPath ?? "", wadSoundPath ?? "", soundName);
                if (File.Exists(realPath))
                    return realPath;
            }

            if (ignoreMissingSounds)
            {
                logger.Warn("Sound '" + soundName + "' not found.");
                return null;
            }
            throw new FileNotFoundException("Sound not found", soundName);
        }

        public static readonly WadSample NullSample = new WadSample("", new byte[] {
            0x52, 0x49, 0x46, 0x46, 0x28, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45,
            0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
            0x22, 0x56, 0x00, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x02, 0x00, 0x10, 0x00,
            0x64, 0x61, 0x74, 0x61, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        });

        public static WadSample ReadSound(string soundName, bool ignoreMissingSounds, string wadPath, List<string> wadSoundPaths)
        {
            string path = LookupSound(soundName, ignoreMissingSounds, wadPath, wadSoundPaths);
            if (string.IsNullOrEmpty(path))
                return NullSample;

            // Try opening sound
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] result = new byte[checked((int)fileStream.Length)];
                    fileStream.Read(result, 0, result.GetLength(0));
                    return new WadSample(path, result);
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load sound '" + soundName + "'.");
                if (ignoreMissingSounds)
                    return NullSample;
                throw;
            }
        }

        public static SortedDictionary<int, WadSample> CompileSamples(List<WadSoundInfo> soundMap, LevelSettings settings, bool onlyIndexed, IProgressReporter reporter)
        {
            bool temp;
            return CompileSamples(soundMap, settings, onlyIndexed, reporter, out temp);
        }

        public static SortedDictionary<int, WadSample> CompileSamples(List<WadSoundInfo> soundMap, LevelSettings settings, bool onlyIndexed, out bool missing)
        {
            return CompileSamples(soundMap, settings, onlyIndexed, null, out missing);
        }

        public static SortedDictionary<int, WadSample> CompileSamples(List<WadSoundInfo> soundMap, LevelSettings settings, bool onlyIndexed, IProgressReporter reporter, out bool samplesMissing)
        {
            var samples = new List<WadSample>();
            foreach (var soundInfo in soundMap)
            {
                if (onlyIndexed && !soundInfo.Indexed)
                    continue;
                foreach (var sample in soundInfo.Samples)
                    samples.Add(sample);
            }

            var loadedSamples = new SortedDictionary<int, WadSample>();

            // Set up maximum buffer sizes and sample rate
            int maxBufferLength     = TrCatalog.GetLimit(settings.GameVersion, Limit.SoundSampleSize) * 1024;
            int supportedSampleRate = TrCatalog.GetLimit(settings.GameVersion, Limit.SoundSampleRate);
            int supportedBitness    = TrCatalog.GetLimit(settings.GameVersion, Limit.SoundBitsPerSample);

            // Pre-generate recursive list of all subdirectories to search to speed-up process
            var pathList = settings.GetRecursiveListOfSoundPaths();

            var missing = false;
            for (int i = 0; i < samples.Count; i++)
            {
                WadSample currentSample = NullSample;
                try
                {
                    string samplePath = WadSounds.TryGetSamplePath(pathList, samples[i].FileName);

                    // If sample was found, then load it...
                    if (!string.IsNullOrEmpty(samplePath))
                    {
                        using (var stream = new FileStream(samplePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var buffer = new byte[stream.Length];
                            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                                throw new EndOfStreamException();

                            if (settings.EnableCustomSampleRate)
                                currentSample = new WadSample(samplePath, ConvertSampleFormat(buffer, settings.CustomSampleRate, supportedBitness));
                            else
                                currentSample = new WadSample(samplePath, ConvertSampleFormat(buffer, supportedSampleRate, supportedBitness));

                            if (!settings.EnableCustomSampleRate && currentSample.SampleRate != supportedSampleRate)
                                reporter?.ReportWarn("Sample " + samplePath + " has a sample rate of " + currentSample.SampleRate + " which is unsupported for this engine version.");

                            if (currentSample.ChannelCount > 1)
                                reporter?.ReportWarn("Sample " + samplePath + " isn't mono. Only mono samples are supported. Crashes may occur.");

                            if (currentSample.BitsPerSample != supportedBitness)
                                reporter?.ReportWarn("Sample " + samplePath + " is not " + supportedBitness + "-bit sample and is not supported in this game version. Crashes may occur.");

                            if (buffer.Length > maxBufferLength)
                                reporter?.ReportWarn("Sample " + samplePath + " is more than " + maxBufferLength / 1024 + " kbytes long. It may be too big for this game version, crashes may occur.");
                        }
                    }
                    // ... otherwise output null sample
                    else
                    {
                        currentSample = WadSample.NullSample;
                        logger.Warn(new FileNotFoundException(), "Unable to find sample '" + samplePath + "'");
                        missing = true;
                    }
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Unable to read file '" + samples[i].FileName + "' from provided location.");
                }

                loadedSamples.Add(i, currentSample);
            }

            if (samples.Count == 0 && settings.GameVersion != TRVersion.Game.TombEngine)
                reporter?.ReportWarn("All samples are missing. This may cause crashes. Make sure you have specified correct sample path.");
            else if (missing)
                reporter?.ReportWarn("Some samples are missing. Make sure sample paths are specified correctly. Check level settings for details.");

            samplesMissing = missing;
            return loadedSamples;
        }

        public static readonly IEnumerable<FileFormat> FileFormatsToRead = new[]
            {
                new FileFormat("WAVE file", "wav", "wave"),
                new FileFormat("MP3 file", "mp3"),
                new FileFormat("Free Lossless Audio Codec file", "flac"),
                new FileFormat("Ogg Vorbis file", "ogg"),
                new FileFormat("Audio Interchange file", "aiff", "aif", "aifc")
            };
    }
}
