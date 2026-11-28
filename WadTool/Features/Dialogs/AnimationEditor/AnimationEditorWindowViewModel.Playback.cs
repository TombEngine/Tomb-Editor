#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Playback, sound preview and frame navigation.</summary>
public partial class AnimationEditorWindowViewModel
{
    // ---- Transport toggles ----

    partial void OnChainPlaybackChanged(bool value)
    {
        _editor.Tool.Configuration.AnimationEditor_ChainPlayback = value;

        // Translate global event.
        if (_allowUpdate)
            _editor.Tool.TogglePlayback(IsPlaying, value);
    }

    [RelayCommand]
    private void ToggleSound()
    {
        if (_editor.Tool.ReferenceLevel is null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        _editor.Tool.Configuration.AnimationEditor_SoundPreview = !_editor.Tool.Configuration.AnimationEditor_SoundPreview;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview;
    }

    [RelayCommand]
    private void CycleSoundCondition()
    {
        if (_editor.Tool.ReferenceLevel is null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        var t = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        bool isTEN = IsTombEngine;
        _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = t switch
        {
            SoundPreviewType.Land => SoundPreviewType.LandWithMaterial,
            SoundPreviewType.LandWithMaterial => SoundPreviewType.Water,
            SoundPreviewType.Water => isTEN ? SoundPreviewType.Quicksand : SoundPreviewType.Land,
            SoundPreviewType.Quicksand when isTEN => SoundPreviewType.Underwater,
            _ => SoundPreviewType.Land
        };
        UpdateSoundCondition();
    }

    private void UpdateSoundCondition()
    {
        var previewType = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        SoundConditionLabel = previewType.ToString();
        SoundConditionToolTip = "Sound preview type: " + previewType.ToString().SplitCamelcase();
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();

    private void UpdateStatusLabel()
    {
        if (_timeline is null || _editor.CurrentAnim is null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        {
            StatusText = string.Empty;
            return;
        }

        StatusText = "Frame: " + _frameCount + " / " + Math.Max(_editor.GetRealNumberOfFrames() - 1, 0) +
                     "   Keyframe: " + _timeline.Value + " / " + Math.Max(_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1, 0);

        if (!_timeline.SelectionIsEmpty)
        {
            StatusText += "   Selected keyframe";

            if (_timeline.SelectionSize == 1)
                StatusText += ": " + _timeline.Selection.X;
            else
                StatusText += "s: " + _timeline.Selection.X + " to " + _timeline.Selection.Y;
        }
    }

    [RelayCommand]
    private void Play()
    {
        if (_playTimer is null || _panel is null || _timeline is null) return;

        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            _playNextTick = _playClock.Elapsed + PlayStep;
            _playTimer.Start();
            if (_editor.CurrentAnim?.WadAnimation is not null)
            {
                _chainedInitialAnim = _editor.CurrentAnim.Index;
                _chainedInitialCursorPos = _timeline.Value;
                _chainedInitialSelection = _timeline.Selection;
                if (_editor.CurrentAnim.WadAnimation.KeyFrames.Count > 1)
                    _frameCount = _timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
            }
        }
        else
        {
            _playTimer.Stop();
            if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback &&
                _editor.Tool.Configuration.AnimationEditor_RewindAfterChainPlayback)
            {
                var origNode = _editor.Animations.FirstOrDefault(item => item.Index == _chainedInitialAnim);
                if (origNode is not null && origNode != _editor.CurrentAnim)
                {
                    if (!SelectAnimByIndex(_chainedInitialAnim))
                        SelectAnimation(origNode);
                    else
                    {
                        _timeline.Value = _chainedInitialCursorPos;
                        _timeline.SelectionStart = _chainedInitialSelection.X;
                        _timeline.SelectionEnd = _chainedInitialSelection.Y;
                    }
                }
                else if (origNode == _editor.CurrentAnim &&
                         _editor.CurrentAnim.WadAnimation.NextFrame >= (_editor.GetRealNumberOfFrames() - 1) &&
                         _editor.GetRealFrameNumber() >= _editor.CurrentAnim.WadAnimation.NextFrame)
                    _timeline.Value = _chainedInitialCursorPos;
            }

            _blendState.Clear();
            _panel.DisablePicking = false;
        }

        _panel.GridPosition = System.Numerics.Vector3.Zero;
        _panel.Invalidate();
        _editor.Tool.TogglePlayback(IsPlaying, _editor.Tool.Configuration.AnimationEditor_ChainPlayback);
    }

    private bool SelectAnimByIndex(int index)
    {
        var item = Animations.FirstOrDefault(a => a.Node.Index == index);

        if (item is null)
        {
            // Animation is hidden by the list filter, so jump to it directly.
            var node = _editor.Animations.FirstOrDefault(a => a.Index == index);

            if (node is null)
                return false;

            SelectedAnim = null;
            SelectAnimation(node);
            return true;
        }

        if (!ReferenceEquals(item, SelectedAnim))
            SelectedAnim = item;

        return true;
    }

    private static readonly TimeSpan PlayStep = TimeSpan.FromMilliseconds(30);
    private const int MaxCatchUpSteps = 3;

    /// <summary>
    /// Advances playback by whole 30 ms steps of the wall clock. Ticks delayed by camera dragging or heavy rendering are made up
    /// for, so the playback speed does not depend on how busy the dispatcher is (the legacy timer was served ahead of painting).
    /// </summary>
    private void OnPlayTimerTick()
    {
        var now = _playClock.Elapsed;
        var steps = 0;

        while (now >= _playNextTick && steps < MaxCatchUpSteps && IsPlaying)
        {
            _playNextTick += PlayStep;
            steps++;
            OnPlayTick();
        }

        // After a long stall, drop the backlog instead of fast-forwarding through it.
        if (now >= _playNextTick)
            _playNextTick = now + PlayStep;
    }

    private void OnPlayTick()
    {
        if (_panel is null || _timeline is null ||
            _editor.CurrentAnim?.WadAnimation is null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 1)
            return;

        int realFrameNumber = _editor.GetRealNumberOfFrames();
        int realRangeNumber = _editor.CurrentAnim.WadAnimation.EndFrame > realFrameNumber - 1 ? realFrameNumber : _editor.CurrentAnim.WadAnimation.EndFrame + 1;

        _frameCount++;

        var nextIndex = _editor.CurrentAnim.WadAnimation.NextAnimation;
        var nextFrame = _editor.CurrentAnim.WadAnimation.NextFrame;
        var nextRange = new TombLib.VectorInt2(realRangeNumber);

        bool chain = _editor.Tool.Configuration.AnimationEditor_ChainPlayback;

        if (chain && _chainedIncomingAnimation >= 0)
        {
            if (_chainedIncomingAnimation < _editor.Animations.Count &&
                _chainedIncomingFrame >= 0 &&
                _chainedIncomingFrame < _editor.GetRealNumberOfFrames(_chainedIncomingAnimation) &&
                _chainedIncomingFrameRange.X >= 0 &&
                _chainedIncomingFrameRange.Y >= _chainedIncomingFrameRange.X &&
                _chainedIncomingFrameRange.Y <= realFrameNumber)
            {
                nextIndex = (ushort)_chainedIncomingAnimation;
                nextFrame = (ushort)_chainedIncomingFrame;
                nextRange = _chainedIncomingFrameRange;
            }
            else
            {
                ShowPopup(L("MsgPendingStateChangeBad", _chainedIncomingAnimation), PopupType.Error);
                _chainedIncomingAnimation = -1;
                _blendState.ClearPendingBlend();
            }
        }

        if (_frameCount >= nextRange.X && _frameCount <= nextRange.Y)
        {
            if (chain)
            {
                bool blendStarted = _blendState.TryBegin(_editor.CurrentAnim, _frameCount, _editor.Tool.Configuration.AnimationEditor_SmoothAnimation);
                _panel.DisablePicking = blendStarted;
                _chainedIncomingAnimation = -1;

                var nextNode = _editor.Animations.FirstOrDefault(item => item.Index == nextIndex);
                if (nextNode is not null)
                {
                    if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid && _editor.CurrentAnim.WadAnimation.AnimCommands.Count > 0 &&
                        _editor.CurrentAnim.WadAnimation.AnimCommands.Any(cmd => cmd.Type == WadAnimCommandType.SetPosition))
                        _chainedSetPosRecoveryCount = 0;

                    _editor.CurrentAnim.WadAnimation.AnimCommands
                        .Where(cmd => cmd.Type == WadAnimCommandType.SetPosition)
                        .ToList()
                        .ForEach(cmd => _panel.GridPosition += new System.Numerics.Vector3(cmd.Parameter1, cmd.Parameter2, cmd.Parameter3));

                    if (nextNode != _editor.CurrentAnim)
                        SelectAnimByIndex(nextIndex);

                    var maxFrameNumber = _editor.GetRealNumberOfFrames(nextIndex);
                    if (nextFrame >= maxFrameNumber)
                    {
                        _frameCount = 0;
                        ShowPopup(L("MsgNoFrameInAnimation", nextFrame, nextIndex), PopupType.Warning);
                    }
                    else
                        _frameCount = nextFrame;
                }
                else
                    ShowPopup(L("MsgChainBroken", nextIndex), PopupType.Warning);
            }
            else
                _frameCount = 0;
        }

        if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid)
        {
            if (!chain && _frameCount >= (realRangeNumber - 1) &&
                _editor.CurrentAnim.WadAnimation.NextAnimation != _editor.CurrentAnim.Index)
                _panel.GridPosition = System.Numerics.Vector3.Zero;

            var startVel = new Vector3(_editor.CurrentAnim.WadAnimation.StartLateralVelocity, 0.0f, _editor.CurrentAnim.WadAnimation.StartVelocity);
            var endVel = new Vector3(_editor.CurrentAnim.WadAnimation.EndLateralVelocity, 0.0f, _editor.CurrentAnim.WadAnimation.EndVelocity);

            if (_editor.Moveable.Id.TypeId == 0)
            {
                startVel = GetPreviewVelocity(startVel);
                endVel = GetPreviewVelocity(endVel);
            }

            var shiftX = Vector3.Lerp(startVel, endVel, (float)_frameCount / realRangeNumber);
            _panel.GridPosition += shiftX;

            if (_editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange)
            {
                if (_panel.GridPosition.Y != 0 &&
                    !_editor.CurrentAnim.WadAnimation.AnimCommands.Any(cmd => cmd.Type == WadAnimCommandType.SetPosition) &&
                    _editor.CurrentAnim.WadAnimation.NextAnimation == _editor.CurrentAnim.Index)
                    _chainedSetPosRecoveryCount++;
                else
                    _chainedSetPosRecoveryCount = 0;

                if (_chainedSetPosRecoveryCount >= _gridRecoveryWaitInterval)
                {
                    _gridRecoveryCount += _gridRecoveryStep;
                    var shiftY = new System.Numerics.Vector3(_panel.GridPosition.X, 0.0f, _panel.GridPosition.Z);
                    if (_gridRecoveryCount < 1.0f)
                        shiftY.Y = (float)TombLib.MathC.SmoothStep(_panel.GridPosition.Y, 0.0f, _gridRecoveryCount);
                    else { _gridRecoveryCount = 0.0f; _chainedSetPosRecoveryCount = 0; }
                    _panel.GridPosition = shiftY;
                }
            }
        }

        byte frameRate = _editor.CurrentAnim.WadAnimation.FrameRate == 0 ? (byte)1 : _editor.CurrentAnim.WadAnimation.FrameRate;
        bool isKeyFrame = _frameCount % frameRate == 0;

        if (isKeyFrame)
        {
            int newFrameNumber = (int)Math.Round((double)(_frameCount / frameRate));
            _timeline.Value = newFrameNumber > _timeline.Maximum ? 0 : newFrameNumber;
        }
        else if (_editor.Tool.Configuration.AnimationEditor_SmoothAnimation)
        {
            float k = (float)_frameCount / frameRate;
            k = _frameCount == realFrameNumber - 1 ? 1.0f : k - (float)Math.Floor(k);
            SelectFrame(k);
        }

        if (_blendState.IsActive)
        {
            _blendState.BuildPose(_panel.Model, _editor.CurrentAnim, _frameCount, _editor.Tool.Configuration.AnimationEditor_SmoothAnimation);
            _panel.Invalidate();
            if (!_blendState.Advance())
                _panel.DisablePicking = false;
        }

        UpdateStatusLabel();
    }

    private Vector3 GetPreviewVelocity(Vector3 velocity) => _editor.CurrentAnim.WadAnimation.StateId switch
    {
        5 or 16 or 23 or 25 or 32 or 47 => new Vector3(velocity.X, velocity.Y, -velocity.Z),
        86 or 163 when _editor.Wad.GameVersion == TRVersion.Game.TombEngine => new Vector3(velocity.X, velocity.Y, -velocity.Z),
        21 or 26 or 31 or 49 or 60 or 78 => new Vector3(velocity.Z, 0.0f, velocity.X),
        22 or 27 or 30 or 48 or 58 or 77 => new Vector3(-velocity.Z, 0.0f, -velocity.X),
        _ => velocity
    };

    private void ShowPopup(string message, PopupType type)
    {
        if (_panel is not null)
            PopUpInfo.Show(_popup, null, _panel, message, type);
    }

    private void PreviewSounds()
    {
        if (!_editor.Tool.Configuration.AnimationEditor_SoundPreview || _editor.Tool.ReferenceLevel is null)
            return;

        _overallPlaybackCount++;
        if (_overallPlaybackCount > _materialIndexSwitchInterval)
        {
            _overallPlaybackCount = 0;
            var materialSounds = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap
                .Where(s => s.Name.IndexOf("FOOTSTEPS_", StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
            if (materialSounds.Count > 1)
                while (true)
                {
                    var newMaterialIndex = (new Random()).Next(0, materialSounds.Count - 1);
                    if (materialSounds.Count == 1 || newMaterialIndex != _currentMaterialIndex)
                    {
                        _currentMaterialIndex = materialSounds[newMaterialIndex].Id;
                        break;
                    }
                }
        }

        var previewSoundType = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
        {
            int idToPlay = -1;
            if (ac.Type == WadAnimCommandType.PlaySound)
                idToPlay = ac.Parameter2;
            else if (ac.Type == WadAnimCommandType.FlipEffect && previewSoundType == SoundPreviewType.LandWithMaterial &&
                     _editor.Wad.GameVersion.Native() >= TRVersion.Game.TR3)
            {
                var flipID = ac.Parameter2;
                if (flipID == 32 || flipID == 33) idToPlay = _currentMaterialIndex;
            }

            if (idToPlay == -1 || ac.Parameter1 != _frameCount)
                continue;

            var soundType = ac.Type == WadAnimCommandType.FlipEffect ? WadSoundEnvironmentType.Land : (WadSoundEnvironmentType)ac.Parameter3;

            if (ac.Type == WadAnimCommandType.FlipEffect &&
                (previewSoundType == SoundPreviewType.Water || previewSoundType == SoundPreviewType.Quicksand || previewSoundType == SoundPreviewType.Underwater))
                continue;
            if (soundType == WadSoundEnvironmentType.Land && !(previewSoundType == SoundPreviewType.Land || previewSoundType == SoundPreviewType.LandWithMaterial))
                continue;
            if (soundType == WadSoundEnvironmentType.Water && previewSoundType != SoundPreviewType.Water) continue;
            if (soundType == WadSoundEnvironmentType.Quicksand && previewSoundType != SoundPreviewType.Quicksand) continue;
            if (soundType == WadSoundEnvironmentType.Underwater && previewSoundType != SoundPreviewType.Underwater) continue;

            var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(s => s.Id == idToPlay);
            if (soundInfo is null)
            {
                ShowPopup(L("MsgSoundMissing", idToPlay), PopupType.Warning);
                continue;
            }

            if (!_editor.Tool.ReferenceLevel.Settings.SelectedSounds.Contains(idToPlay))
                ShowPopup(L("MsgSoundDisabled", idToPlay), PopupType.Warning);
            else
                try { WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo); }
                catch (Exception exc)
                {
                    _logger.Warn(exc, "Unable to play sound info {Id}.", idToPlay);
                    ShowPopup(L("MsgSoundPlayFailed", idToPlay, exc.Message), PopupType.Warning);
                }
        }
    }

    private void UpdateReferenceLevel()
    {
        if (_panel is null) return;
        _panel.Level = _editor.Tool.ReferenceLevel;
        _panel.Invalidate();
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview && _editor.Tool.ReferenceLevel is not null;

        _allowUpdate = false;
        Rooms.Clear();
        if (_editor.Tool.ReferenceLevel is not null)
        {
            RoomsEnabled = true;
            Rooms.Add("(select room)");
            foreach (var room in _editor.Tool.ReferenceLevel.Rooms)
                if (room is not null) Rooms.Add(room);
            SelectedRoomItem = Rooms.Count > 0 ? Rooms[0] : null;
        }
        else
            RoomsEnabled = false;
        _allowUpdate = true;
    }

    partial void OnSelectedRoomItemChanged(object? value)
    {
        if (!_allowUpdate || _panel is null) return;
        if (value is Room room)
        {
            _panel.Room = room;
            _panel.RoomPosition = room.GetLocalCenter();
        }
        else
        {
            _panel.Room = null;
            _panel.RoomPosition = System.Numerics.Vector3.Zero;
        }
        _panel.Invalidate();
    }

    [RelayCommand]
    private void GotoStart() { if (_timeline is not null) _timeline.Value = _timeline.Minimum; }
    /// <summary>Steps one frame and wraps around at the ends (keyboard navigation of the previous form).</summary>
    public void StepFrameLooping(bool forward)
    {
        if (_timeline is null)
            return;

        if (forward)
            _timeline.ValueLoopInc();
        else
            _timeline.ValueLoopDec();
    }

    /// <summary>Marks the start (in) or end (out) of the frame selection at the current frame.</summary>
    public void MarkSelection(bool start)
    {
        if (_timeline is null)
            return;

        if (start)
            _timeline.SelectionStart = _timeline.Value;
        else
            _timeline.SelectionEnd = _timeline.Value;
    }

    public void SelectAllFrames() => _timeline?.SelectAll();

    /// <summary>Clears the mesh, bone list and timeline selections (Escape).</summary>
    public void ClearSelections()
    {
        if (_panel is not null)
        {
            _panel.SelectedMesh = null;
            UpdateTransformUI();
            _panel.Invalidate();
        }

        SelectedBoneItem = null;
        _timeline?.ResetSelection();
    }

    [RelayCommand]
    private void GotoPrev() { if (_timeline is not null && _timeline.Value > _timeline.Minimum) _timeline.Value--; }
    [RelayCommand]
    private void GotoNext() { if (_timeline is not null && _timeline.Value < _timeline.Maximum) _timeline.Value++; }
    [RelayCommand]
    private void GotoEnd() { if (_timeline is not null) _timeline.Value = _timeline.Maximum; }

    [RelayCommand]
    private void ResetCamera() => _panel?.ResetCamera();
}
