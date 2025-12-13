using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public struct WadMoveableId : IWadObjectId, IEquatable<WadMoveableId>, IComparable<WadMoveableId>
    {
        public uint TypeId;

        public WadMoveableId(uint objTypeId)
        {
            TypeId = objTypeId;
        }

        public int CompareTo(WadMoveableId other) => TypeId.CompareTo(other.TypeId);
        public int CompareTo(object other) => CompareTo((WadMoveableId)other);
        public static bool operator <(WadMoveableId first, WadMoveableId second) => first.TypeId < second.TypeId;
        public static bool operator <=(WadMoveableId first, WadMoveableId second) => first.TypeId <= second.TypeId;
        public static bool operator >(WadMoveableId first, WadMoveableId second) => first.TypeId > second.TypeId;
        public static bool operator >=(WadMoveableId first, WadMoveableId second) => first.TypeId >= second.TypeId;
        public static bool operator ==(WadMoveableId first, WadMoveableId second) => first.TypeId == second.TypeId;
        public static bool operator !=(WadMoveableId first, WadMoveableId second) => !(first == second);
        public bool Equals(WadMoveableId other) => this == other;
        public override bool Equals(object other) => other is WadMoveableId && this == (WadMoveableId)other;
        public override int GetHashCode() => unchecked((int)TypeId);

        public string ToString(TRVersion.Game gameVersion)
        {
            return "(" + TypeId + ") " + TrCatalog.GetMoveableName(gameVersion, TypeId);
        }
        public override string ToString() => "Uncertain game version - " + ToString(TRVersion.Game.TR4);
        public string ShortName(TRVersion.Game gameVersion) => TrCatalog.GetMoveableName(gameVersion, TypeId);

        public bool IsWaterfall(TRVersion.Game gameVersion)
        {
            return (gameVersion.Native() == TRVersion.Game.TR4 && TypeId >= 423 && TypeId <= 425) ||
                   (gameVersion.Native() == TRVersion.Game.TR5 && TypeId >= 410 && TypeId <= 415);
        }
        public bool IsOptics(TRVersion.Game gameVersion)
        {
            return (gameVersion.Native() == TRVersion.Game.TR4 && TypeId >= 461 && TypeId <= 462) ||
                   (gameVersion.Native() == TRVersion.Game.TR5 && TypeId >= 456 && TypeId <= 457);
        }

        public static WadMoveableId Lara = new WadMoveableId(0);

        public static WadMoveableId? GetHorizon(TRVersion.Game gameVersion) {
            switch (gameVersion.Native()) {
                case TRVersion.Game.TR2:
                    return new WadMoveableId(254);
                case TRVersion.Game.TR3:
                    return new WadMoveableId(355);
                case TRVersion.Game.TR4:
                case TRVersion.Game.TR5:
                case TRVersion.Game.TRNG:
                    return new WadMoveableId(459);
                case TRVersion.Game.TombEngine:
                    return new WadMoveableId(1350);
                default:
                    return null;
            }
        }
    }

    public class WadMoveable : IWadObject
    {
        public WadMoveableId Id { get; private set; }
        public DataVersion Version { get; set; } = DataVersion.GetNext();
        public WadMesh Skin { get; set; }
        public List<WadMesh> Meshes => Bones.Select(bone => bone.Mesh).ToList();
        public List<WadAnimation> Animations { get; } = new List<WadAnimation>();
        //public WadBone Skeleton { get; set; } = new WadBone();
        public List<WadBone> Bones { get; } = new List<WadBone>();

        public WadMoveable(WadMoveableId id)
        {
            Id = id;
        }

        public string ToString(TRVersion.Game gameVersion) => Id.ToString(gameVersion.Native());
        public override string ToString() => Id.ToString();
        IWadObjectId IWadObject.Id => Id;

        public WadMoveable Clone()
        {
            var mov = new WadMoveable(Id);
            mov.Skin = Skin?.Clone() ?? null;
            foreach (var mesh in Meshes)
                mov.Meshes.Add(mesh.Clone());
            foreach (var bone in Bones)
                mov.Bones.Add(bone.Clone());
            foreach (var animation in Animations)
                mov.Animations.Add(animation.Clone());
            return mov;
        }

        // Keep a reference to model's skin and skinned model to prevent memory leaks
        // which happen for unclear reason if new moveable is always returned.

        private WadMoveable _skin = null;
        private WadMoveable _skinnedModel = null;

        public WadMoveable ReplaceDummyMeshes(WadMoveable skin)
        {
            if (skin.Meshes.Count != Meshes.Count) return this;
            if (Meshes.Count != Bones.Count) return this;

            // Identify mesh ID which occurs several times in a model which possibly indicates
            // that used mesh is a dummy mesh. If there is no such mesh, return original model.

            var dummyHash = Meshes.GroupBy(m => m.Hash).Where(g => g.Count() > 1).FirstOrDefault()?.Key ?? Hash.Zero;
            if (dummyHash == Hash.Zero)
                return this;

            // If skin is the same as in previous call, immediately return same skinned model.
            // Otherwise construct skinned model again.

            if (_skin != null && _skinnedModel != null && _skinnedModel.Version == skin.Version)
                return _skinnedModel;

            _skin = skin;
            _skinnedModel = new WadMoveable(Id);

            if (skin.Skin != null)
                _skinnedModel.Skin = skin.Skin;

            for (int i = 0; i < Meshes.Count; i++)
            {
                WadMesh msh = Meshes[i].Clone();
                WadMesh msh2 = skin.Meshes[i].Clone();
                WadBone bone = Bones[i].Clone();

                if (msh.Hash != dummyHash)
                {
                    _skinnedModel.Bones.Add(bone);
                    continue;
                }
                bone.Mesh = msh2;
                _skinnedModel.Bones.Add(bone);
            }

            // Copy only first animation of the model to skin because it's the only one
            // which is used by preview windows utilizing skinned model.

            if (Animations.Count > 0)
                _skinnedModel.Animations.Add(Animations[0].Clone());
            else
                _skinnedModel.Animations.Add(new WadAnimation());

            _skinnedModel.Version = skin.Version;
            return _skinnedModel;
        }
    }
}
