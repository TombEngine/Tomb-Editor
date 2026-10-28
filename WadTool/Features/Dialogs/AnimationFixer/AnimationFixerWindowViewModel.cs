#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using TombLib.Graphics;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.AnimationFixer;

/// <summary>
/// Mirrors the legacy <c>FormAnimationFixer</c>'s three-way <c>DialogResult</c>
/// (OK / Ignore / Cancel), which a WPF <see cref="bool"/>? dialog result can't express.
/// </summary>
public enum AnimationFixerOutcome
{
    /// <summary>User cancelled the dialog or declined the legacy-engine warning (legacy <c>Cancel</c>).</summary>
    Cancelled,

    /// <summary>User confirmed, but no properties were selected or there was nothing to fix (legacy <c>Ignore</c>).</summary>
    NothingFixed,

    /// <summary>At least one animation was fixed and an undo entry was pushed (legacy <c>OK</c>).</summary>
    Fixed
}

public partial class AnimationFixerWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly AnimationEditor _editor;
    private readonly List<AnimationNode> _animations;
    private readonly ILocalizationService _localization;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private bool _fixEndFrame;
    [ObservableProperty] private bool _fixNextAnimation;
    [ObservableProperty] private bool _fixNextFrame;
    [ObservableProperty] private bool _fixStateChangeRanges;
    [ObservableProperty] private bool _fixStateChangeNextAnimation;
    [ObservableProperty] private bool _fixStateChangeNextFrame;
    [ObservableProperty] private bool _restoreAnimationName;

    /// <summary>Comma-separated indices of the animations that were changed (read by the call site).</summary>
    public string ChangedAnimations { get; private set; } = string.Empty;

    public AnimationFixerOutcome Outcome { get; private set; } = AnimationFixerOutcome.Cancelled;

    /// <summary>Exposed so the window can persist its placement into the legacy config slots.</summary>
    public Configuration Configuration => _editor.Tool.Configuration;

    public AnimationFixerWindowViewModel(AnimationEditor editor, List<AnimationNode> animations,
        ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _animations = animations;
        _localization = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
    }

    private bool FixAnimation(AnimationNode animation)
    {
        bool anyChange = false;

        var wadAnim = animation.WadAnimation;
        var maxFrames = wadAnim.GetRealNumberOfFrames();

        if (FixEndFrame &&
            wadAnim.EndFrame >= wadAnim.GetRealNumberOfFrames())
        {
            wadAnim.EndFrame = (ushort)(wadAnim.GetRealNumberOfFrames() - 1);
            anyChange = true;
        }

        if (FixNextAnimation &&
            wadAnim.NextAnimation >= _editor.Animations.Count)
        {
            wadAnim.NextAnimation %= (ushort)_editor.Animations.Count;
            anyChange = true;
        }

        if (FixNextFrame &&
            wadAnim.NextFrame > _editor.Animations[wadAnim.NextAnimation].WadAnimation.EndFrame)
        {
            wadAnim.NextFrame = _editor.Animations[wadAnim.NextAnimation].WadAnimation.EndFrame;
            anyChange = true;
        }

        if (RestoreAnimationName)
        {
            wadAnim.Name = TrCatalog.GetAnimationName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, (uint)animation.Index);
            anyChange = true;
        }

        foreach (var sch in wadAnim.StateChanges)
            foreach (var disp in sch.Dispatches)
            {
                if (FixStateChangeRanges)
                {
                    if (disp.OutFrame > maxFrames)
                    {
                        disp.OutFrame = (ushort)(maxFrames > 0 ? maxFrames - 1 : maxFrames);
                        anyChange = true;
                    }

                    if (disp.InFrame > disp.OutFrame)
                    {
                        disp.InFrame = disp.OutFrame;
                        anyChange = true;
                    }
                }

                if (FixStateChangeNextAnimation &&
                    disp.NextAnimation >= _editor.Animations.Count)
                {
                    disp.NextAnimation = (ushort)(_editor.Animations.Count - 1);
                    anyChange = true;
                }

                if (FixStateChangeNextFrame &&
                    disp.NextAnimation < _editor.Animations.Count && disp.NextLowFrame > _editor.Animations[disp.NextAnimation].WadAnimation.EndFrame)
                {
                    disp.NextLowFrame = _editor.Animations[disp.NextAnimation].WadAnimation.EndFrame;
                    anyChange = true;
                }
            }

        if (anyChange)
        {
            if (string.IsNullOrEmpty(ChangedAnimations))
                ChangedAnimations = animation.Index.ToString();
            else
                ChangedAnimations += ", " + animation.Index.ToString();
        }

        return anyChange;
    }

    [RelayCommand]
    private void Confirm()
    {
        if (_animations is not null && _animations.Count > 0 &&
            UserDialogs.Show(WPFUtils.GetWindowOwner(),
                                _localization["LegacyEngineWarningMessage"], _localization["LegacyEngineWarningTitle"],
                                DialogButtons.OKCancel, DialogIcon.Warning) == DialogChoice.OK)
        {
            var undoList = new List<UndoRedoInstance>();

            foreach (var anim in _animations)
            {
                undoList.Add(new AnimationUndoInstance(_editor, anim));

                if (FixAnimation(anim))
                    _editor.Tool.AnimationEditorAnimationChanged(anim, false);
                else
                    undoList.RemoveAt(undoList.Count - 1);
            }

            if (undoList.Count > 0)
            {
                _editor.Tool.UndoManager.Push(undoList);
                Outcome = AnimationFixerOutcome.Fixed;
                DialogResult = true;
            }
            else
            {
                Outcome = AnimationFixerOutcome.NothingFixed;
                DialogResult = false;
            }
        }
        else
        {
            Outcome = AnimationFixerOutcome.Cancelled;
            DialogResult = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Outcome = AnimationFixerOutcome.Cancelled;
        DialogResult = false;
    }
}
