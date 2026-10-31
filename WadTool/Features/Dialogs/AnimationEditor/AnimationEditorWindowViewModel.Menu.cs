#nullable enable

using CommunityToolkit.Mvvm.Input;
using LevelFileDialog = TombLib.WPF.Services.LevelFileDialog;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;
using TombLib.Services;
using WadTool.Features.Dialogs.AnimationFixer;
using WadTool.Features.Dialogs.ReplaceAnimCommands;

namespace WadTool.Features.Dialogs.AnimEditor;

// Menu bar and timeline context menu operations.
public partial class AnimationEditorWindowViewModel
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private bool CanUndo() => _undoPossible;

    private bool CanRedo() => _redoPossible;

    private bool CanSave() => !_isSaved;

    // ---- Frames ----

    [RelayCommand]
    private void InsertFramesAfterCurrent()
    {
        if (_timeline is null || _editor.CurrentAnim is null)
            return;

        var input = InputBox.Show(Owner, "Add frames", "How many frames to add?", "1");

        if (input is null)
            return;

        if (!int.TryParse(input, out int framesCount) || framesCount <= 0)
        {
            ShowPopup("You must insert a number greater than 1", PopupType.Error);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        for (int i = 0; i < framesCount; i++)
            AddNewFrame(_timeline.Value + 1, false);
    }

    [RelayCommand]
    private void DeleteEveryNthFrame()
    {
        if (_timeline is null || _editor.CurrentAnim is null)
            return;

        var input = InputBox.Show(Owner, "Delete every nth frame", "Enter number", "2");

        if (input is null)
            return;

        if (!int.TryParse(input, out int step) || step <= 1)
        {
            ShowPopup("You must insert a number greater than 1", PopupType.Error);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        var keyFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames;

        for (int i = keyFrames.Count - 1; i >= 0; i -= step)
            keyFrames.RemoveAt(i);

        SetSaved(false);
        RefreshAfterResample();
    }

    [RelayCommand]
    private void CalculateCurrentFrameBoundingBox()
    {
        if (_timeline is not null)
            CalculateKeyframeBoundingBox(_timeline.Value, false, true);
    }

    [RelayCommand]
    private void DeleteCurrentFrameBoundingBox()
    {
        if (_timeline is not null)
            CalculateKeyframeBoundingBox(_timeline.Value, true, true);
    }

    // ---- Resampling ----

    [RelayCommand]
    private void ResampleAnimation()
    {
        if (_editor.CurrentAnim is null)
            return;

        var input = InputBox.Show(Owner, "Resample animation", "Enter resample multiplier (speed)", "2");

        if (input is null)
            return;

        if (!int.TryParse(input, out int multiplier) || multiplier <= 1)
        {
            ShowPopup("You must insert a number greater than 1", PopupType.Error);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        InterpolateAnimation(multiplier - 1, true);
        RefreshAfterResample();
    }

    [RelayCommand]
    private void ResampleAnimationToFramerate()
    {
        if (_editor.CurrentAnim is null)
            return;

        var animation = _editor.CurrentAnim.WadAnimation;

        if (animation.FrameRate < 2)
        {
            ShowPopup("Animation is already at framerate 1. You need other framerate to resample.", PopupType.Error);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        InterpolateAnimation(animation.FrameRate - 1, false);
        animation.FrameRate = 1;
        RefreshAfterResample();
    }

    private void InterpolateAnimation(int numFrames, bool fixAnimCommands)
    {
        var animation = _editor.CurrentAnim.WadAnimation;
        int stepCount = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

        if (stepCount <= 0 || numFrames <= 0)
            return;

        for (int i = 0; i < stepCount; i++)
        {
            int startFrame = i * (numFrames + 1);
            InterpolateFrames(startFrame, startFrame + 1, numFrames, false);
        }

        if (fixAnimCommands)
        {
            foreach (var command in animation.AnimCommands.Where(command => command.Type >= WadAnimCommandType.PlaySound))
                command.Parameter1 *= (short)(numFrames + 1);
        }
    }

    private void RefreshAfterResample()
    {
        if (_timeline is null || _editor.CurrentAnim is null)
            return;

        SetSaved(false);
        SelectAnimation(_editor.CurrentAnim);
        _timeline.Highlight();
    }

    // ---- Fixing and anim commands ----

    [RelayCommand]
    private void FixSelectedAnimations() => FixAnimations(AnimationFixScope.Selected);

    [RelayCommand]
    private void FixAllAnimations() => FixAnimations(AnimationFixScope.All);

    [RelayCommand]
    private void FixCurrentAnimation() => FixAnimations(AnimationFixScope.Current);

    private void FixAnimations(AnimationFixScope scope)
    {
        var animations = scope switch
        {
            AnimationFixScope.Current => _editor.CurrentAnim is null ? new List<AnimationNode>() : new List<AnimationNode> { _editor.CurrentAnim },
            AnimationFixScope.Selected => _selectedAnimItems.Select(item => item.Node).ToList(),
            _ => _editor.Animations.ToList()
        };

        if (animations.Count == 0)
            return;

        var vm = new AnimationFixerWindowViewModel(_editor, animations);
        var dialog = new AnimationFixerWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();

        if (vm.Outcome == AnimationFixerOutcome.NothingFixed)
        {
            ShowPopup("No properties were selected or there was nothing to fix.\nNo changes were made.", PopupType.Info);
        }
        else if (vm.Outcome == AnimationFixerOutcome.Fixed)
        {
            var message = vm.ChangedAnimations.Length < 50 ? "Animations (" + vm.ChangedAnimations + ")" : "Multiple animations";
            ShowPopup(message + " were fixed.\nPlease save your wad under new name and thoroughly test it.", PopupType.Warning);
            SelectAnimation(_editor.CurrentAnim);
        }
    }

    [RelayCommand]
    private void FindReplaceAnimCommands()
    {
        var vm = new ReplaceAnimCommandsWindowViewModel(_editor);
        var dialog = new ReplaceAnimCommandsWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();

        if (vm.EditingWasDone)
            SetSaved(false);

        _timeline?.Invalidate();
    }

    // ---- Batch import and export ----

    [RelayCommand]
    private void BatchImport()
    {
        if (_editor.CurrentAnim is null)
            return;

        var path = LevelFileDialog.BrowseFolder(Owner, null, null, "Specify folder for batch import", null);

        if (path is null)
            return;

        var undoList = new List<UndoRedoInstance>();
        var updateSelection = false;
        var importCount = 0;
        var errorCount = 0;
        bool? generateMissingAnimations = null;

        foreach (var file in Directory.GetFiles(path).Where(file => Path.GetExtension(file) == ".anim"))
        {
            // Files without a number in their name are ignored.
            if (!int.TryParse(Regex.Match(Path.GetFileName(file), @"\d+").Value, out int number))
                continue;

            try
            {
                var animation = WadActions.ImportAnimationFromXml(_editor.Tool, file);

                if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
                    throw new InvalidDataException("Incorrect amount of bones in animation imported from file " + file);

                if (_editor.Animations.Count <= number)
                {
                    generateMissingAnimations ??= UserDialogs.Show(Owner, "Batch contains animations with IDs spanning beyond existing animation count.\n" +
                        "Fill missing slots with empty animations (Yes) or skip missing slots (No)?", "Missing animation slots detected",
                        DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.Yes;

                    if (!generateMissingAnimations.Value)
                        throw new InvalidDataException("Animation index is out of range for animation file " + file);

                    while (_editor.Animations.Count <= number)
                        _editor.Animations.Add(new AnimationNode(new WadAnimation { Name = "Empty animation " + _editor.Animations.Count }, new Animation(), _editor.Animations.Count));
                }

                undoList.Add(new AnimationUndoInstance(_editor, _editor.Animations[number]));

                _editor.Animations[number].WadAnimation = animation;
                _editor.Animations[number].DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);

                if (number == _editor.CurrentAnim.Index)
                    updateSelection = true;

                importCount++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Batch import of animation file '{File}' failed.", file);
                errorCount++;
            }
        }

        _editor.Tool.UndoManager.Push(undoList);

        if (errorCount > 0)
            ShowPopup((importCount == 0 ? "No animations were imported.\n" : "Successfully imported " + importCount + " animations.\n") +
                errorCount + " animations were ignored. Check log file for details.", PopupType.Warning);
        else
            ShowPopup("Successfully imported " + importCount + " animations.", PopupType.Info);

        RebuildAnimationsList();
        SetSaved(false);

        if (!updateSelection || _timeline is null)
            return;

        SelectAnimation(_editor.CurrentAnim);
        _timeline.Value = 0;
        _panel?.Invalidate();
    }

    [RelayCommand]
    private void BatchExportSelected() => BatchExport(_selectedAnimItems.Select(item => item.Node));

    [RelayCommand]
    private void BatchExportAll() => BatchExport(_editor.Animations);

    private void BatchExport(IEnumerable<AnimationNode> animations)
    {
        if (_editor.Animations.Count == 0)
        {
            ShowPopup("No animations present. Nothing to export.", PopupType.Error);
            return;
        }

        var path = LevelFileDialog.BrowseFolder(Owner, null, null, "Specify folder to save animations", null);

        if (path is null)
            return;

        var exportCount = 0;
        var errorCount = 0;

        foreach (var animation in animations.ToList())
        {
            try
            {
                var fileName = Path.Combine(path, animation.Index.ToString("D4") + "_" + animation.WadAnimation.Name + ".anim");

                if (!WadActions.ExportAnimationToXml(_editor.Moveable, animation.WadAnimation, fileName))
                    throw new IOException("There was an error batch-exporting animation " + animation.WadAnimation.Name + ".");

                exportCount++;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Batch export of animation '{Name}' failed.", animation.WadAnimation.Name);
                errorCount++;
            }
        }

        if (errorCount > 0)
            ShowPopup("Successfully exported " + exportCount + " animations.\n" + errorCount + " animations were ignored. Check log file for details.", PopupType.Warning);
        else
            ShowPopup("Successfully exported " + exportCount + " animations.", PopupType.Info);
    }

    // ---- Timeline context menu ----

    [RelayCommand]
    private void MarkIn() => MarkSelection(true);

    [RelayCommand]
    private void MarkOut() => MarkSelection(false);

    [RelayCommand]
    private void SelectAll() => SelectAllFrames();

    [RelayCommand]
    private void ClearSelection() => _timeline?.ResetSelection();

    [RelayCommand]
    private void CreateAnimCommand()
    {
        if (_timeline?.Animation is null)
            return;

        EditAnimCommands(new WadAnimCommand { Type = WadAnimCommandType.PlaySound, Parameter1 = (short)_timeline.FrameIndex });
    }

    [RelayCommand]
    private void CreateStateChange()
    {
        if (_timeline?.Animation is null)
            return;

        WadStateChange? stateChange = null;

        if (!_timeline.SelectionIsEmpty)
        {
            stateChange = new WadStateChange();
            stateChange.Dispatches.Add(new WadAnimDispatch
            {
                InFrame = (ushort)_timeline.SelectionStartFrameIndex,
                OutFrame = (ushort)_timeline.SelectionEndFrameIndex,
                NextAnimation = 0,
                NextLowFrame = 0
            });
        }

        EditStateChanges(stateChange);
    }
}
