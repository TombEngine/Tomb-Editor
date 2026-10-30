#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.WPF.Features.AnimatedTextures
{
    public enum ProceduralAnimationType
    {
        HorizontalStretch,
        VerticalStretch,
        Scale,
        HorizontalSkew1,
        HorizontalSkew2,
        VerticalSkew1,
        VerticalSkew2,
        Spin,
        HorizontalPan,
        VerticalPan,
        Shake
    }

    public enum AnimGenerationType
    {
        New,
        Clone,
        Merge,
        Replace,
        AddFrames
    }

    public sealed class ProceduralPreset
    {
        public ProceduralPreset(string display, ProceduralAnimationType type)
        {
            Display = display;
            Type = type;
        }

        public string Display { get; }
        public ProceduralAnimationType Type { get; }
        public override string ToString() => Display;
    }

    /// <summary>Procedural animation generation (ports <c>FormAnimatedTextures.GenerateProceduralAnimation</c>).</summary>
    public partial class AnimatedTexturesWindowViewModel
    {
        private const int MaxLegacyFrames = 16;

        // " (with " — also used as the marker for stripping a previously appended effect postfix.
        private string AnimNameCombineString => _localizationService["EffectNamePrefix"];

        public IReadOnlyList<ProceduralPreset> ProceduralPresets { get; }

        private IReadOnlyList<ProceduralPreset> BuildProceduralPresets() =>
            new List<ProceduralPreset>
            {
                new(_localizationService["PresetStretchHorizontal"], ProceduralAnimationType.HorizontalStretch),
                new(_localizationService["PresetStretchVertical"], ProceduralAnimationType.VerticalStretch),
                new(_localizationService["PresetScale"], ProceduralAnimationType.Scale),
                new(_localizationService["PresetSkewHorizontal1"], ProceduralAnimationType.HorizontalSkew1),
                new(_localizationService["PresetSkewHorizontal2"], ProceduralAnimationType.HorizontalSkew2),
                new(_localizationService["PresetSkewVertical1"], ProceduralAnimationType.VerticalSkew1),
                new(_localizationService["PresetSkewVertical2"], ProceduralAnimationType.VerticalSkew2),
                new(_localizationService["PresetSpin"], ProceduralAnimationType.Spin),
                new(_localizationService["PresetPanHorizontal"], ProceduralAnimationType.HorizontalPan),
                new(_localizationService["PresetPanVertical"], ProceduralAnimationType.VerticalPan),
                new(_localizationService["PresetShake"], ProceduralAnimationType.Shake)
            };

        [ObservableProperty] private ProceduralPreset? _selectedPreset;
        [ObservableProperty] private double _genFrameCount = MaxLegacyFrames;
        [ObservableProperty] private double _genStrength = 100.0;
        [ObservableProperty] private bool _genSmooth;
        [ObservableProperty] private bool _genLoop;

        [RelayCommand] private void GenerateNew() => Generate(AnimGenerationType.New);
        [RelayCommand] private void GenerateClone() => Generate(AnimGenerationType.Clone);
        [RelayCommand] private void GenerateMerge() => Generate(AnimGenerationType.Merge);
        [RelayCommand] private void GenerateReplace() => Generate(AnimGenerationType.Replace);
        [RelayCommand] private void GenerateAddFrames() => Generate(AnimGenerationType.AddFrames);

        private void Generate(AnimGenerationType genType)
        {
            var preset = SelectedPreset ?? ProceduralPresets.First();
            GenerateProceduralAnimation(preset, (int)GenFrameCount, (float)(GenStrength / 100.0), GenSmooth, GenLoop, genType);
        }

        private void ReloadFrames()
        {
            bool wasLocked = _lockUi;
            _lockUi = true;
            Frames.Clear();
            if (SelectedSet != null)
                foreach (var frame in SelectedSet.Frames)
                    Frames.Add(new AnimatedTextureFrameViewModel(frame));
            _lockUi = wasLocked;
        }

        private bool GenerateProceduralAnimation(ProceduralPreset preset, int resultingFrameCount, float effectStrength, bool smooth, bool loop, AnimGenerationType genType)
        {
            ProceduralAnimationType type = preset.Type;

            TextureArea textureArea = _textureMap.SelectedTexture;
            if (textureArea.Texture is not LevelTexture && textureArea.Texture is not WadTexture)
            {
                _messageService.ShowError(_localizationService["NoValidTextureRegion"], _localizationService["InvalidSelection"]);
                return false;
            }

            // Reverse strength for scale/stretch types, where the visible effect opposes the function.
            effectStrength = (float)MathC.Clamp((type <= ProceduralAnimationType.Scale) ? -effectStrength : effectStrength, -1.0, 1.0);

            int startIndex = 0;
            AnimatedTextureSet targetSet;

            if (genType != AnimGenerationType.New)
            {
                if (SelectedSet == null)
                {
                    _messageService.ShowError(_localizationService["NoValidAnimation"], _localizationService["InvalidSelection"]);
                    return false;
                }

                targetSet = SelectedSet;

                if (genType == AnimGenerationType.Clone)
                    targetSet = targetSet.Clone();
                else if (genType == AnimGenerationType.AddFrames && targetSet.Frames.Count > 0)
                    startIndex = (SelectedFrame != null ? targetSet.Frames.IndexOf(SelectedFrame.Model) : -1) + 1;
                else if (genType == AnimGenerationType.Replace)
                    targetSet.Frames.Clear();

                if (string.IsNullOrEmpty(targetSet.Name))
                    targetSet.Name = _localizationService.Format("NewSetName", _sets.Count + 1);
            }
            else
            {
                targetSet = new AnimatedTextureSet { Name = _localizationService.Format("ProceduralSetName", _sets.Count + 1) };
            }

            if (genType is AnimGenerationType.New or AnimGenerationType.Replace or AnimGenerationType.AddFrames)
            {
                for (int i = 0; i < resultingFrameCount; i++)
                {
                    var dummyFrame = new AnimatedTextureFrame
                    {
                        Texture = textureArea.Texture,
                        TexCoord0 = textureArea.TexCoord0,
                        TexCoord1 = textureArea.TexCoord1,
                        TexCoord2 = textureArea.TexCoord2,
                        TexCoord3 = textureArea.TexCoord3
                    };

                    if (genType != AnimGenerationType.AddFrames)
                        targetSet.Frames.Add(dummyFrame);
                    else
                        targetSet.Frames.Insert(startIndex, dummyFrame);
                }
            }

            if (genType != AnimGenerationType.AddFrames)
                resultingFrameCount = targetSet.Frames.Count;

            if (resultingFrameCount <= 0)
            {
                _messageService.ShowError(_localizationService["NoFrames"], _localizationService["NoFramesTitle"]);
                return false;
            }

            bool realAnim = resultingFrameCount > 1;

            if (genType != AnimGenerationType.New)
            {
                int foundPostfixPos = targetSet.Name.IndexOf(AnimNameCombineString, StringComparison.Ordinal);
                if (foundPostfixPos != -1)
                    targetSet.Name = targetSet.Name.Substring(0, foundPostfixPos);
                targetSet.Name += AnimNameCombineString + preset.Display + _localizationService["EffectNameSuffix"];
            }

            var rnd = new Random();

            if (type == ProceduralAnimationType.Shake && smooth)
                loop = true;

            for (int cnt = 0, i = startIndex; cnt < resultingFrameCount; cnt++, i++)
            {
                var referenceFrame = targetSet.Frames[i].Clone();
                float midFrame = loop && realAnim ? resultingFrameCount / 2.0f : resultingFrameCount;
                float bias = Math.Abs(cnt - midFrame) / midFrame;
                float weight = (smooth && realAnim ? (float)MathC.SmoothStep(0.0, 1.0, bias) : bias) * effectStrength;

                switch (type)
                {
                    case ProceduralAnimationType.HorizontalSkew1:
                    case ProceduralAnimationType.HorizontalSkew2:
                    case ProceduralAnimationType.VerticalSkew1:
                    case ProceduralAnimationType.VerticalSkew2:
                        {
                            bool otherDirection = (int)type % 2 == 0;

                            if (effectStrength > 0.0f)
                                weight = effectStrength - weight;
                            else
                                weight = Math.Abs(weight);
                            if (!otherDirection)
                                weight = 1.0f - weight;

                            int[] index = new int[2];
                            Vector2[] coord1 = new Vector2[2];
                            Vector2[] coord2 = new Vector2[2];

                            if (otherDirection)
                            {
                                index[0] = 0;
                                index[1] = 2;
                            }
                            else
                            {
                                index[0] = 1;
                                index[1] = 3;
                            }

                            if (type == ProceduralAnimationType.HorizontalSkew2)
                            {
                                coord2[0] = referenceFrame.TexCoord3;
                                coord2[1] = referenceFrame.TexCoord1;
                            }
                            else
                            {
                                coord2[0] = referenceFrame.TexCoord1;
                                coord2[1] = referenceFrame.TexCoord3;
                            }

                            if (type == ProceduralAnimationType.HorizontalSkew1)
                            {
                                coord1[0] = referenceFrame.TexCoord2;
                                coord1[1] = referenceFrame.TexCoord0;
                            }
                            else
                            {
                                coord1[0] = referenceFrame.TexCoord0;
                                coord1[1] = referenceFrame.TexCoord2;
                            }

                            for (int c = 0; c < 2; c++)
                            {
                                var result = Vector2.Lerp(coord1[c], coord2[c], weight);
                                switch (index[c])
                                {
                                    case 0: targetSet.Frames[i].TexCoord0 = result; break;
                                    case 1: targetSet.Frames[i].TexCoord1 = result; break;
                                    case 2: targetSet.Frames[i].TexCoord2 = result; break;
                                    case 3: targetSet.Frames[i].TexCoord3 = result; break;
                                }
                            }
                        }
                        break;

                    case ProceduralAnimationType.HorizontalStretch:
                    case ProceduralAnimationType.VerticalStretch:
                    case ProceduralAnimationType.Scale:
                        {
                            if (effectStrength < 0)
                                weight += Math.Abs(effectStrength);

                            bool[] passes =
                            {
                                type == ProceduralAnimationType.HorizontalStretch || type == ProceduralAnimationType.Scale,
                                type == ProceduralAnimationType.VerticalStretch || type == ProceduralAnimationType.Scale
                            };

                            for (int p = 0; p < 2; p++)
                                if (passes[p])
                                {
                                    var center1 = Vector2.Lerp(referenceFrame.TexCoord0, (p == 0 ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1), 0.5f);
                                    var center2 = Vector2.Lerp(referenceFrame.TexCoord2, (p == 0 ? referenceFrame.TexCoord1 : referenceFrame.TexCoord3), 0.5f);

                                    targetSet.Frames[i].TexCoord0 = Vector2.Lerp(referenceFrame.TexCoord0, center1, weight);
                                    targetSet.Frames[i].TexCoord1 = Vector2.Lerp(referenceFrame.TexCoord1, (p == 0 ? center2 : center1), weight);
                                    targetSet.Frames[i].TexCoord2 = Vector2.Lerp(referenceFrame.TexCoord2, center2, weight);
                                    targetSet.Frames[i].TexCoord3 = Vector2.Lerp(referenceFrame.TexCoord3, (p == 0 ? center1 : center2), weight);

                                    if (p == 0)
                                        referenceFrame = targetSet.Frames[i].Clone();
                                }
                        }
                        break;

                    case ProceduralAnimationType.Spin:
                        {
                            weight = effectStrength - weight;
                            double currAngle = ((2 * Math.PI) * weight) - Math.PI * 0.25;

                            var cross1 = Vector3.Cross(new Vector3(referenceFrame.TexCoord0.X, referenceFrame.TexCoord0.Y, 1),
                                                       new Vector3(referenceFrame.TexCoord2.X, referenceFrame.TexCoord2.Y, 1));
                            var cross2 = Vector3.Cross(new Vector3(referenceFrame.TexCoord1.X, referenceFrame.TexCoord1.Y, 1),
                                                       new Vector3(referenceFrame.TexCoord3.X, referenceFrame.TexCoord3.Y, 1));

                            var intersection = Vector3.Cross(cross1, cross2);
                            var center = new Vector2(intersection.X / intersection.Z, intersection.Y / intersection.Z);

                            float r0 = Vector2.Distance(center, referenceFrame.TexCoord0);
                            float r1 = Vector2.Distance(center, referenceFrame.TexCoord1);
                            float r2 = Vector2.Distance(center, referenceFrame.TexCoord2);
                            float r3 = Vector2.Distance(center, referenceFrame.TexCoord3);

                            if (float.IsNaN(r1) || float.IsNaN(r2) || float.IsNaN(r3) || float.IsNaN(r0))
                                continue;

                            targetSet.Frames[i].TexCoord0 = new Vector2(center.X + r0 * (float)Math.Cos(currAngle + Math.PI), center.Y + r0 * (float)Math.Sin(currAngle + Math.PI));
                            targetSet.Frames[i].TexCoord1 = new Vector2(center.X + r1 * (float)Math.Cos(currAngle + Math.PI * 1.5f), center.Y + r1 * (float)Math.Sin(currAngle + Math.PI * 1.5f));
                            targetSet.Frames[i].TexCoord2 = new Vector2(center.X + r2 * (float)Math.Cos(currAngle), center.Y + r2 * (float)Math.Sin(currAngle));
                            targetSet.Frames[i].TexCoord3 = new Vector2(center.X + r3 * (float)Math.Cos(currAngle + Math.PI / 2), center.Y + r3 * (float)Math.Sin(currAngle + Math.PI / 2));
                        }
                        break;

                    case ProceduralAnimationType.HorizontalPan:
                    case ProceduralAnimationType.VerticalPan:
                        {
                            bool horizontal = type == ProceduralAnimationType.HorizontalPan;
                            var multiplier = -Math.Sign(effectStrength) * (Math.Abs(weight) - Math.Abs(effectStrength));

                            var dist1 = Vector2.Distance(referenceFrame.TexCoord0, horizontal ? referenceFrame.TexCoord3 : referenceFrame.TexCoord1) * multiplier;
                            var dist2 = Vector2.Distance(referenceFrame.TexCoord1, horizontal ? referenceFrame.TexCoord2 : referenceFrame.TexCoord0) * multiplier;

                            targetSet.Frames[i].TexCoord0 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist1);
                            targetSet.Frames[i].TexCoord1 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist1);
                            targetSet.Frames[i].TexCoord2 += horizontal ? new Vector2(dist1, 0) : new Vector2(0, dist2);
                            targetSet.Frames[i].TexCoord3 += horizontal ? new Vector2(dist2, 0) : new Vector2(0, dist2);
                        }
                        break;

                    case ProceduralAnimationType.Shake:
                        {
                            int rndStrength = (int)((loop ? Math.Abs(effectStrength - weight) : effectStrength) * 16.0f);
                            if (rndStrength <= 0)
                                break;

                            float xRnd = rnd.Next(-rndStrength, rndStrength);
                            float yRnd = rnd.Next(-rndStrength, rndStrength);
                            Vector2 rndAdd = new(xRnd, yRnd);

                            targetSet.Frames[i].TexCoord0 += rndAdd;
                            targetSet.Frames[i].TexCoord1 += rndAdd;
                            targetSet.Frames[i].TexCoord2 += rndAdd;
                            targetSet.Frames[i].TexCoord3 += rndAdd;
                        }
                        break;
                }
            }

            // New/Clone create a new set; the rest mutate the existing one in place.
            if (genType < AnimGenerationType.Merge)
            {
                var existing = _sets.FirstOrDefault(s => s.Equals(targetSet));
                if (existing != null)
                {
                    _messageService.ShowInformation(_localizationService["DuplicateAnimation"]);
                    RebuildSets();
                    SelectedSet = existing;
                    return false;
                }

                _sets.Add(targetSet);
            }

            RebuildSets();
            SelectedSet = targetSet;
            ReloadFrames();
            _context.OnAnimatedTexturesChanged?.Invoke();

            int rowIndex = startIndex + resultingFrameCount - 1;
            if (rowIndex >= 0 && rowIndex < Frames.Count)
                SelectedFrame = Frames[rowIndex];

            return true;
        }
    }
}
