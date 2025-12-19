using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormAnimationFixer : DarkForm
    {
        private readonly AnimationEditor _editor;
        private List<AnimationNode> _animations;

        public string ChangedAnimations { get; private set; }

        public FormAnimationFixer(AnimationEditor editor, List<AnimationNode> animations)
        {
            InitializeComponent();
            _editor = editor;
            _animations = animations;

            ChangedAnimations = string.Empty;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Tool.Configuration);
        }

        private bool FixAnimation(AnimationNode animation, bool fixEndFrame, bool fixNextAnim, bool fixNextFrame, bool fixStateChangeRanges, bool fixStateChangeNextAnim, bool fixStateChangeNextFrameLow, bool fixName)
        {
            bool anyChange = false;

            var wadAnim = animation.WadAnimation;
            var maxFrames = wadAnim.GetRealNumberOfFrames();

            if (fixEndFrame &&
                wadAnim.EndFrame >= wadAnim.GetRealNumberOfFrames())
                {
                    wadAnim.EndFrame = (ushort)(wadAnim.GetRealNumberOfFrames() - 1);
                    anyChange = true;
                }

            if (fixNextAnim &&
                wadAnim.NextAnimation >= _editor.Animations.Count)
            {
                wadAnim.NextAnimation %= (ushort)_editor.Animations.Count;
                anyChange = true;
            }

            if (fixNextFrame &&
                wadAnim.NextFrame > _editor.Animations[wadAnim.NextAnimation].WadAnimation.EndFrame)
            {
                wadAnim.NextFrame = _editor.Animations[wadAnim.NextAnimation].WadAnimation.EndFrame;
                anyChange = true;
            }

            if (fixName)
            {
                wadAnim.Name = TrCatalog.GetAnimationName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, (uint)animation.Index);
                anyChange = true;
            }

            foreach (var sch in wadAnim.StateChanges)
                foreach (var disp in sch.Dispatches)
                {
                    if (fixStateChangeRanges)
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

                    if (fixStateChangeNextAnim &&
                        disp.NextAnimation >= _editor.Animations.Count)
                    {
                        disp.NextAnimation = (ushort)(_editor.Animations.Count - 1);
                        anyChange = true;
                    }

                    if (fixStateChangeNextFrameLow &&
                        disp.NextAnimation < _editor.Animations.Count && disp.NextFrameLow > _editor.Animations[disp.NextAnimation].WadAnimation.EndFrame)
                    {
                        disp.NextFrameLow = _editor.Animations[disp.NextAnimation].WadAnimation.EndFrame;
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

        private void btOk_Click(object sender, EventArgs e)
        {
            if (_animations != null && _animations.Count > 0 &&
                DarkMessageBox.Show(this, "Make sure you know what you are doing.\nThis action may break compatibility with legacy engines!", 
                                    "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                var undoList = new List<UndoRedoInstance>();

                foreach (var anim in _animations)
                {
                    undoList.Add(new AnimationUndoInstance(_editor, anim));

                    if (FixAnimation(anim, cbEndFrame.Checked, cbNextAnim.Checked, cbNextFrame.Checked, cbSchRanges.Checked, cbSchNextAnim.Checked, cbSchNextFrame.Checked, cbFixName.Checked))
                        _editor.Tool.AnimationEditorAnimationChanged(anim, false);
                    else
                        undoList.RemoveAt(undoList.Count - 1);
                }

                if (undoList.Count > 0)
                {
                    _editor.Tool.UndoManager.Push(undoList);
                    DialogResult = DialogResult.OK;
                }
                else
                    DialogResult = DialogResult.Ignore;
            }
            else
                DialogResult = DialogResult.Cancel;

            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
