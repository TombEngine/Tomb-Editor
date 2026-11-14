#nullable enable

using System.Collections.Generic;
using System.Linq;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using WadTool.Controls;

namespace WadTool
{
    public abstract class AnimationEditorUndoRedoInstance : UndoRedoInstance
    {
        public AnimationEditor Parent { get; internal set; }
        protected int AnimCount;

        protected AnimationEditorUndoRedoInstance(AnimationEditor parent) { Parent = parent; AnimCount = Parent.Animations.Count; }
    }

    public class AnimationUndoInstance : AnimationEditorUndoRedoInstance
    {
        private AnimationNode Animation;

        public AnimationUndoInstance(AnimationEditor parent, AnimationNode anim) : base(parent)
        {
            Animation = anim.Clone();

            Valid = () => Parent.Animations.Count == AnimCount &&
                          Animation.DirectXAnimation is not null &&
                          Animation.WadAnimation is not null &&
                          Animation.Index >= 0 &&
                          Animation.Index < Parent.Animations.Count;

            UndoAction = () =>
            {
                Parent.Animations[Animation.Index] = Animation;
                Parent.Tool.AnimationEditorAnimationChanged(Animation, true);
            };

            RedoInstance = () => new AnimationUndoInstance(Parent, Parent.Animations[Animation.Index]);
        }
    }

    public class MeshUndoInstance : UndoRedoInstance
    {
        private WadMesh Mesh;
        private PanelRenderingMesh Parent;

        public MeshUndoInstance(PanelRenderingMesh parent, WadMesh mesh)
        {
            Mesh = mesh.Clone();
            Parent = parent;

            Valid = () => Parent.Mesh is not null;
            UndoAction = () =>
            {
                Parent.ResetCameraOnMeshChange = false;
                Parent.Mesh = Mesh;
                Parent.ResetCameraOnMeshChange = true;
            };
            RedoInstance = () => new MeshUndoInstance(Parent, Parent.Mesh);
        }
    }

    public class WadToolUndoManager : UndoManager
    {
        public WadToolClass Tool;

        public WadToolUndoManager(WadToolClass tool, int undoDepth) : base(undoDepth)
        {
            Tool = tool;

            UndoStackChanged += (s, e) => Tool.UndoStackChanged();
            MessageSent += (s, e) => Tool.SendMessage(e.Message, PopupType.Warning);
        }

        public void PushAnimationChanged(AnimationEditor editor, AnimationNode anim) => Push(new AnimationUndoInstance(editor, anim));
        public void PushAnimationChanged(AnimationEditor editor, List<AnimationNode> anims) => Push(anims.Select(anim => (new AnimationUndoInstance(editor, anim)) as UndoRedoInstance).ToList());
        public void PushMeshChanged(PanelRenderingMesh editor) => Push(new MeshUndoInstance(editor, editor.Mesh));
    }
}
