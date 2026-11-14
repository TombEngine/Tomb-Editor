#nullable enable

using System.Collections.Generic;
using System.Linq;
using TombLib.Wad;

namespace SoundTool.ViewModels;

/// <summary>Describes a sound playback mode together with the hint shown to the user.</summary>
public sealed record LoopBehaviourOption(WadSoundLoopBehaviour Value, string Name, string Hint)
{
    public static IReadOnlyList<LoopBehaviourOption> All { get; } =
    [
        new(WadSoundLoopBehaviour.None, "None", "Play in any case"),
        new(WadSoundLoopBehaviour.OneShotWait, "One shot wait", "Ignore if already playing"),
        new(WadSoundLoopBehaviour.OneShotRewound, "One shot rewound", "Rewind if already playing"),
        new(WadSoundLoopBehaviour.Looped, "Loop", "Loop until stopped by engine")
    ];

    public static string GetHint(WadSoundLoopBehaviour value) => All.FirstOrDefault(option => option.Value == value)?.Hint ?? string.Empty;
}
