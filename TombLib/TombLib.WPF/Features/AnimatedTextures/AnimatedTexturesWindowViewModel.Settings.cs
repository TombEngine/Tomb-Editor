#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>A predefined TRNG setting value (FPS or UvRotate) shown in a combo box.</summary>
    public sealed class NgSettingPair
    {
        public NgSettingPair(float key, string display)
        {
            Key = key;
            Display = display;
        }

        public float Key { get; }
        public string Display { get; }
        public override string ToString() => Display;
    }

    /// <summary>NG (TRNG) and TombEngine per-set settings (ports FormAnimatedTextures.OnEffectChanged et al.).</summary>
    public partial class AnimatedTexturesWindowViewModel
    {
        public List<NgSettingPair> NgFpsOptions { get; } = new();
        public List<NgSettingPair> NgUvRotateOptions { get; } = new();

        public bool IsTrng => _version == TRVersion.Game.TRNG;
        public bool IsTombEngine => _version == TRVersion.Game.TombEngine;

        private bool IsRotateEffect =>
            _selectedAnimationType is AnimatedTextureAnimationType.UVRotate
                or AnimatedTextureAnimationType.HalfRotate
                or AnimatedTextureAnimationType.RiverRotate;

        // Visibility / enablement of the settings widgets.
        public bool ShowNgFpsCombo => IsTrng && IsRotateEffect;
        public bool ShowNgUvRotateCombo => IsTrng;
        public bool NgUvRotateEnabled => IsTrng && IsRotateEffect;
        public bool ShowTeUvRotate => IsTombEngine && _selectedAnimationType == AnimatedTextureAnimationType.UVRotate;
        public bool ShowFpsNumeric => !ShowNgFpsCombo && !ShowTeUvRotate;

        public bool FpsNumericEnabled =>
            _selectedAnimationType is not (AnimatedTextureAnimationType.PFrames or AnimatedTextureAnimationType.Video);

        private void InitNgOptions()
        {
            if (!IsTrng)
                return;

            for (int i = -64; i < 0; i++)
                NgUvRotateOptions.Add(new NgSettingPair(i, _localizationService.Format("NgUvRotateValue", i)));
            NgUvRotateOptions.Add(new NgSettingPair(0, _localizationService["DefaultFromScript"]));
            for (int i = 1; i <= 64; i++)
                NgUvRotateOptions.Add(new NgSettingPair(i, _localizationService.Format("NgUvRotateValue", i)));

            for (int i = 1; i <= 32; i++)
                NgFpsOptions.Add(new NgSettingPair(i, _localizationService.Format("NgFpsValue", i)));
        }

        // TRNG combos.

        [ObservableProperty] private NgSettingPair? _selectedNgFps;
        [ObservableProperty] private NgSettingPair? _selectedNgUvRotate;

        partial void OnSelectedNgFpsChanged(NgSettingPair? value)
        {
            if (_lockUi || SelectedSet == null || value == null)
                return;
            SelectedSet.Fps = value.Key;
            _context.OnAnimatedTexturesChanged?.Invoke();
        }

        partial void OnSelectedNgUvRotateChanged(NgSettingPair? value)
        {
            if (_lockUi || SelectedSet == null || value == null)
                return;
            SelectedSet.UvRotate = (sbyte)value.Key;
            _context.OnAnimatedTexturesChanged?.Invoke();
        }

        // TombEngine UVRotate.

        private double _tenUvRotateSpeed = 1.0;
        public double TenUvRotateSpeed
        {
            get => _tenUvRotateSpeed;
            set
            {
                if (!SetProperty(ref _tenUvRotateSpeed, value) || _lockUi || SelectedSet == null)
                    return;
                SelectedSet.TenUvRotateSpeed = (float)value;
            }
        }

        private double _tenUvRotateDirection;
        public double TenUvRotateDirection
        {
            get => _tenUvRotateDirection;
            set
            {
                if (!SetProperty(ref _tenUvRotateDirection, value) || _lockUi || SelectedSet == null)
                    return;
                SelectedSet.TenUvRotateDirection = (float)value;
            }
        }

        /// <summary>Loads the per-set settings values from the model (call with _lockUi already set).</summary>
        private void LoadSettings(AnimatedTextureSet set)
        {
            _tenUvRotateSpeed = set.TenUvRotateSpeed;
            _tenUvRotateDirection = set.TenUvRotateDirection;

            if (IsTrng)
            {
                SelectedNgFps = ClosestNgValue(NgFpsOptions, set.Fps);
                SelectedNgUvRotate = ClosestNgValue(NgUvRotateOptions, set.UvRotate);
            }
        }

        private static NgSettingPair? ClosestNgValue(List<NgSettingPair> options, float value)
        {
            if (options.Count == 0)
                return null;
            var best = options[0];
            foreach (var option in options)
                if (Math.Abs(option.Key - value) < Math.Abs(best.Key - value))
                    best = option;
            return best;
        }

        /// <summary>Re-raises the computed visibility/value settings properties when the effect changes.</summary>
        private void RefreshSettingsState()
        {
            OnPropertyChanged(nameof(ShowNgFpsCombo));
            OnPropertyChanged(nameof(ShowNgUvRotateCombo));
            OnPropertyChanged(nameof(NgUvRotateEnabled));
            OnPropertyChanged(nameof(ShowTeUvRotate));
            OnPropertyChanged(nameof(ShowFpsNumeric));
            OnPropertyChanged(nameof(FpsNumericEnabled));
            OnPropertyChanged(nameof(TenUvRotateSpeed));
            OnPropertyChanged(nameof(TenUvRotateDirection));
        }
    }
}
