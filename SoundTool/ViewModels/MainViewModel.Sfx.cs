#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using TombLib.LevelData;
using TombLib.Wad;

namespace SoundTool.ViewModels;

public partial class MainViewModel
{
    private const string SfxFilter = "Tomb Raider 2/3 MAIN.SFX|*.sfx";

    [RelayCommand]
    private void CompileTr2() => Compile(TRVersion.Game.TR2);

    [RelayCommand]
    private void CompileTr3() => Compile(TRVersion.Game.TR3);

    private void Compile(TRVersion.Game version)
    {
        if (Sounds.Count == 0)
        {
            Warn("No sounds present. Nothing to compile.");
            return;
        }

        var samplePath = _dialogs.SelectFolder("Choose the folder containing all samples", _configuration.SoundTool_LastMainSFXSamplePath);
        if (samplePath is null)
            return;

        var filename = _dialogs.SaveFile("Save MAIN.SFX", SfxFilter, _configuration.SoundTool_LastMainSFXPath);
        if (filename is null)
            return;

        Run(() => {
            var settings = new LevelSettings { GameVersion = version };
            settings.WadSoundPaths.Clear();
            settings.WadSoundPaths.Add(new WadSoundPath(samplePath));
            var samples = WadSample.CompileSamples(CreateArchive().SoundInfos, settings, true, out bool missing);
            using (var stream = File.Create(filename))
                foreach (var sample in samples.Values)
                    stream.Write(sample.Data, 0, sample.Data.Length);

            _configuration.SoundTool_LastMainSFXSamplePath = samplePath;
            _configuration.SoundTool_LastMainSFXPath = filename;
            _messages.ShowInformation("MAIN.SFX compiled successfully." + (missing ? "\nSome samples were not found and will not play in game." : string.Empty));
        });
    }

    [RelayCommand]
    private void Unpack()
    {
        if (Sounds.Count == 0)
        {
            Warn("No sounds present. Nothing to unpack.");
            return;
        }

        var filename = _dialogs.OpenFile("Choose MAIN.SFX", SfxFilter, _configuration.SoundTool_LastMainSFXPath);
        if (filename is null)
            return;

        var directory = _dialogs.SelectFolder("Choose the destination sample folder", _configuration.SoundTool_LastMainSFXSamplePath);
        if (directory is null)
            return;

        Run(() => {
            var result = ExtractSamples(filename, directory);
            _configuration.SoundTool_LastMainSFXPath = filename;
            _configuration.SoundTool_LastMainSFXSamplePath = directory;
            _messages.ShowInformation($"MAIN.SFX unpacked successfully.\nExtracted {result.Samples} samples in {result.Sounds} sound infos.");
        });
    }

    private (int Samples, int Sounds) ExtractSamples(string filename, string directory)
    {
        int sampleCount = 0;
        int soundCount = 0;
        using var reader = new BinaryReader(File.OpenRead(filename));

        foreach (var sound in Sounds.OrderBy(sound => sound.Id))
        {
            if (!sound.Indexed)
                continue;

            foreach (var sample in sound.Samples)
            {
                if (reader.BaseStream.Length - reader.BaseStream.Position < 8 || reader.ReadInt32() != 0x46464952)
                    throw new InvalidDataException("Unexpected or missing RIFF header. Check the sound catalog and MAIN.SFX file.");

                int size = reader.ReadInt32();
                if (size == 0)
                    continue;

                if (size < 4 || size > reader.BaseStream.Length - reader.BaseStream.Position)
                    throw new InvalidDataException("Invalid RIFF size in MAIN.SFX.");

                reader.BaseStream.Position -= 8;
                var data = reader.ReadBytes(checked(size + 8));
                var destination = Path.GetFullPath(Path.Combine(directory, sample.FileName));
                var root = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
                if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Sample filename points outside the destination folder.");

                var parent = Path.GetDirectoryName(destination);
                if (parent is not null)
                    Directory.CreateDirectory(parent);

                File.WriteAllBytes(destination, data);
                sampleCount++;
            }

            soundCount++;
        }

        if (reader.BaseStream.Position != reader.BaseStream.Length)
            throw new InvalidDataException("MAIN.SFX is longer than expected. Check that the sound catalog matches.");

        return (sampleCount, soundCount);
    }
}
