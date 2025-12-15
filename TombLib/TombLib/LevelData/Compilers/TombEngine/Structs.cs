using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.IO;
using TombLib.Types;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public enum TombEnginePolygonShape : int
    {
        Quad,
        Triangle
    }

    public enum TombEngineAnimationBlendType
    {
        Linear,
        Smooth,
        EaseIn,
        EaseOut
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineSpriteTexture
    {
        public int Tile;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float X3;
        public float Y3;
        public float X4;
        public float Y4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineAtlas
    {
		public ImageC ColorMap;
		public ImageC? NormalMap;
		public ImageC? ORSHMap;
		public ImageC? EmissiveMap;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineCollisionInfo
    {
        public float SplitAngle;
        public int[] Portals;
        public Vector3[] Planes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineSectorFlags
    {
        public bool Death;
        public bool Monkeyswing;
        public bool ClimbNorth;
        public bool ClimbSouth;
        public bool ClimbWest;
        public bool ClimbEast;
       
        public bool MarkTriggerer;
        public bool MarkBeetle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineRoomSector
    {
        public int TriggerIndex;
        public int BoxIndex;
        public int StepSound;
        public int Stopper;
        public TombEngineCollisionInfo FloorCollision;
        public TombEngineCollisionInfo CeilingCollision;
        public int WallPortal;
        public TombEngineSectorFlags Flags;
    }

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEnginePolygon
    {
        public TombEnginePolygonShape Shape;
        public List<int> Indices = new List<int>();
        public List<int> VerticesIds = new List<int>();
        public List<Vector2> TextureCoordinates = new List<Vector2>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<Vector3> Tangents = new List<Vector3>();
        public List<Vector3> Binormals = new List<Vector3>();
        public int TextureId;
        public byte BlendMode;
        public bool Animated;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Binormal;
        public int AnimatedSequence;
        public int AnimatedFrame;
        public float ShineStrength;
		public int MaterialIndex;
	}

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineRoomStaticMesh
    {
        public int X;
        public int Y;
        public int Z;
        public short Yaw;
        public short Pitch;
        public short Roll;
        public float Scale;
        public ushort Flags;
        public Vector4 Color;
        public ushort ObjectID;
        public short HitPoints;
        public string LuaName;
    }

    public class NormalHelper
    {
        public TombEnginePolygon Polygon;
        public bool Smooth;

        public NormalHelper(TombEnginePolygon poly)
        {
            Polygon = poly;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoords;
        public Vector3 Color;
        public Vector3 Tangent;
        public Vector3 Bitangent;
        public int[] BoneIndex;
        public float[] BoneWeight;
        public int IndexInPoly;
        public int OriginalIndex;
        public bool DoubleSided;

        public float Glow;
        public float Move;
        public bool  Locked;

        public List<NormalHelper> NormalHelpers = new List<NormalHelper>();
        public bool IsOnPortal;

        // Custom implementation of these because default implementation is *insanely* slow.
        // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
        public static bool operator ==(TombEngineVertex first, TombEngineVertex second)
        {
            return first.Position.X == second.Position.X && first.Position.Y == second.Position.Y && first.Position.Z == second.Position.Z;
        }

        public static bool operator !=(TombEngineVertex first, TombEngineVertex second)
        {
            return !(first == second);
        }

        public bool Equals(TombEngineVertex other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TombEngineVertex))
                return false;
            return this == (TombEngineVertex)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Position.X, (int)Position.Y, (int)Position.Z);
        }
    }

    public static class TombEngineVertexExtensions
    { 
        public static TombEngineVertex SetEffects(this TombEngineVertex vertex, Room room, RoomLightEffect effect)
        {
            var value = (float)room.Properties.LightEffectStrength / 4.0f;
            switch (effect)
            {
                case RoomLightEffect.Glow:
                case RoomLightEffect.Mist:
                case RoomLightEffect.Reflection:
                    vertex.Glow = vertex.Glow == 0f ? value : vertex.Glow;
                    break;

                case RoomLightEffect.Movement:
                    vertex.Move = vertex.Glow == 0f ? value : vertex.Move;
                    break;

                case RoomLightEffect.GlowAndMovement:
                    vertex.Glow = vertex.Glow == 0f ? value : vertex.Glow;
                    vertex.Move = vertex.Glow == 0f ? value : vertex.Move;
                    break;

                default:
                    break;
            }

            return vertex;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class TombEngineMaterial
	{
		public class TombEngineMaterialComparer : IEqualityComparer<TombEngineMaterial>
		{
			public bool Equals(TombEngineMaterial x, TombEngineMaterial y)
			{
                return (x.Texture == y.Texture &&
                    x.BlendMode == y.BlendMode &&
                    x.Animated == y.Animated &&
                    x.NormalMapping == y.NormalMapping &&
                    x.AnimatedSequence == y.AnimatedSequence &&
					x.MaterialIndex == y.MaterialIndex &&
					x.WaterPlaneIndex == y.WaterPlaneIndex);
			}

			public int GetHashCode(TombEngineMaterial obj)
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 23 + obj.Texture.GetHashCode();
					hash = hash * 23 + obj.BlendMode.GetHashCode();
					hash = hash * 23 + obj.Animated.GetHashCode();
					hash = hash * 23 + obj.NormalMapping.GetHashCode();
					hash = hash * 23 + obj.AnimatedSequence.GetHashCode();
					hash = hash * 23 + obj.WaterPlaneIndex.GetHashCode();
					hash = hash * 23 + obj.MaterialIndex.GetHashCode();

					return hash;
				}
			}
		}

		public int Texture;
		public byte BlendMode;
		public bool Animated;
		public bool NormalMapping;
		public int AnimatedSequence;
		public int WaterPlaneIndex;
        public int MaterialIndex;

		public TombEngineMaterial()
		{
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineBucket
    {
        public TombEngineMaterial Material;
        public List<TombEnginePolygon> Polygons;

        public TombEngineBucket()
        {
            Polygons = new List<TombEnginePolygon>();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEnginePortal
    {
        public ushort AdjoiningRoom;
        public VectorInt3 Normal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public VectorInt3[] Vertices;

        public PortalDirection Direction;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineRoom
    {
        public tr_room_info Info;
        public List<TombEngineVertex> Vertices = new List<TombEngineVertex>();
        public List<TombEngineBucket> Buckets;
        public List<TombEnginePortal> Portals;
        public int NumZSectors;
        public int NumXSectors;
        public TombEngineRoomSector[] Sectors;
        public Vector3 AmbientLight;
        public List<TombEngineRoomLight> Lights;
        public List<TombEngineRoomStaticMesh> StaticMeshes;
        public int AlternateRoom;
        public int Flags;
        public int WaterScheme;
        public int ReverbInfo;
        public int AlternateGroup;

        // Helper data
        public List<TombEnginePolygon> Polygons;
        public TrSectorAux[,] AuxSectors;
        public AlternateKind AlternateKind;
        public List<Room> ReachableRooms;
        public bool Visited;
        public bool Flipped;
        public Room FlippedRoom;
        public Room BaseRoom;
        public Room OriginalRoom;

        public void WriteDynamicData(BinaryWriterEx writer)
        {
            writer.Write(OriginalRoom.Name);

            writer.Write(OriginalRoom.Properties.Tags.Count);
            OriginalRoom.Properties.Tags.ForEach(s => writer.Write(s));

            // Write room color
            writer.Write(AmbientLight.X);
            writer.Write(AmbientLight.Y);
            writer.Write(AmbientLight.Z);

            // Write properties
            writer.Write(AlternateRoom);
            writer.Write(Flags);
            writer.Write(WaterScheme);
            writer.Write(ReverbInfo);
            writer.Write(AlternateGroup);

            // Write static meshes
            writer.WriteBlock(StaticMeshes.Count);
            foreach (var sm in StaticMeshes)
            {
                writer.Write(sm.X);
                writer.Write(sm.Y);
                writer.Write(sm.Z);
                writer.Write(sm.Yaw);
                writer.Write(sm.Pitch);
                writer.Write(sm.Roll);
                writer.Write(sm.Scale);
                writer.Write(sm.Flags);
                writer.Write(sm.Color);
                writer.Write(sm.ObjectID);
                writer.Write(sm.HitPoints);
                writer.Write(sm.LuaName);
            }

            // Write volumes
            writer.Write(OriginalRoom.Volumes.Count());
            foreach (var volume in OriginalRoom.Volumes)
            {
                var bvPos = volume.Room.WorldPos + volume.Position;
                bvPos.Y = -bvPos.Y; // Invert Y coordinate to comply with TR coord system

                if (volume is BoxVolumeInstance)
                {
                    writer.Write(0);
                    var bv = volume as BoxVolumeInstance;

                    writer.Write(bvPos);
                    writer.Write(Quaternion.CreateFromYawPitchRoll(MathC.DegToRad(bv.RotationY), MathC.DegToRad(bv.RotationX), MathC.DegToRad(-bv.Roll)));
                    writer.Write(bv.Size / 2.0f);
                }
                else if (volume is SphereVolumeInstance)
                {
                    writer.Write(1);
                    var sv = volume as SphereVolumeInstance;

                    writer.Write(bvPos);
                    writer.Write(Quaternion.Identity);
                    writer.Write(new Vector3(sv.Size / 2.0f));
                }

                writer.Write(volume.Enabled);
                writer.Write(volume.DetectInAdjacentRooms);

                writer.Write(volume.LuaName);
                writer.Write(OriginalRoom.Level.Settings.VolumeEventSets.IndexOf(volume.EventSet));
            }
        }

        public void WriteStaticData(BinaryWriterEx writer)
        {
            writer.WriteBlock(Info);

            writer.Write(Vertices.Count);
            foreach (var p in Vertices)
                writer.Write(p.Position);
            foreach (var c in Vertices)
                writer.Write(c.Color);
            foreach (var v in Vertices)
                writer.Write(new Vector3(v.Glow, v.Move, v.Locked ? 0 : 1));

            writer.Write(Buckets.Count);
            foreach (var bucket in Buckets)
            {
                writer.Write(bucket.Material.Texture);
                writer.Write(bucket.Material.BlendMode);
                writer.Write(bucket.Material.MaterialIndex);
                writer.Write(bucket.Material.Animated);

                writer.Write(bucket.Polygons.Count);
                foreach (var poly in bucket.Polygons)
                {
                    writer.Write((int)poly.Shape);

                    writer.Write((int)poly.AnimatedSequence);
                    writer.Write((int)poly.AnimatedFrame);

                    writer.Write(poly.Normal);

                    foreach (int index in poly.Indices)
                        writer.Write(index);
                    foreach (var uv in poly.TextureCoordinates)
                        writer.Write(uv);
                    foreach (var n in poly.Normals)
                        writer.Write(n);
                    foreach (var t in poly.Tangents)
                        writer.Write(t);
                    foreach (var bt in poly.Binormals)
                        writer.Write(bt);
                }
            }

            // Write portals
            writer.WriteBlock(Portals.Count);
            if (Portals.Count != 0)
            {
                foreach (var p in Portals)
                {
                    writer.Write(p.AdjoiningRoom);
                    writer.Write(p.Normal.X);
                    writer.Write(p.Normal.Y);
                    writer.Write(p.Normal.Z);
                    foreach(var v in p.Vertices)
                    {
                        writer.Write(v.X);
                        writer.Write(v.Y);
                        writer.Write(v.Z);
                    }
                }
            }

            // Write sectors
            writer.Write(NumZSectors);
            writer.Write(NumXSectors);
            foreach (var s in Sectors)
            {
                writer.Write(s.TriggerIndex);
                writer.Write(s.BoxIndex);
                writer.Write(s.StepSound);
                writer.Write(s.Stopper);

                writer.Write(s.FloorCollision.SplitAngle);
                writer.Write(s.FloorCollision.Portals[0]);
                writer.Write(s.FloorCollision.Portals[1]);
                writer.Write(s.FloorCollision.Planes[0]);
                writer.Write(s.FloorCollision.Planes[1]);
                writer.Write(s.CeilingCollision.SplitAngle);
                writer.Write(s.CeilingCollision.Portals[0]);
                writer.Write(s.CeilingCollision.Portals[1]);
                writer.Write(s.CeilingCollision.Planes[0]);
                writer.Write(s.CeilingCollision.Planes[1]);
                writer.Write(s.WallPortal);

                writer.Write(s.Flags.Death);
                writer.Write(s.Flags.Monkeyswing);
                writer.Write(s.Flags.ClimbNorth);
                writer.Write(s.Flags.ClimbSouth);
                writer.Write(s.Flags.ClimbEast);
                writer.Write(s.Flags.ClimbWest);
                writer.Write(s.Flags.MarkTriggerer);
                writer.Write(s.Flags.MarkBeetle);
            }

            // Write lights
            writer.WriteBlock(Lights.Count);
            foreach (var light in Lights)
            {
                writer.Write(light.Position.X);
                writer.Write(light.Position.Y);
                writer.Write(light.Position.Z);
                writer.Write(light.Direction.X);
                writer.Write(light.Direction.Y);
                writer.Write(light.Direction.Z);
                writer.Write(light.Color.X);
                writer.Write(light.Color.Y);
                writer.Write(light.Color.Z);
                writer.Write(light.Intensity);
                writer.Write(light.In);
                writer.Write(light.Out);
                writer.Write(light.Length);
                writer.Write(light.CutOff);
                writer.Write(light.LightType);
                writer.Write(light.CastDynamicShadows);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineRoomLight
    {
        public VectorInt3 Position;
        public Vector3 Direction;
        public Vector3 Color;
        public float Intensity;
        public float In;
        public float Out;
        public float Length;
        public float CutOff;
        public byte LightType;
        public bool CastDynamicShadows;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineMesh
    {
        public bool Hidden;
        public WadMeshLightingType LightingType;
        public BoundingSphere Sphere;
        public List<TombEngineVertex> Vertices = new List<TombEngineVertex>();
        public List<TombEnginePolygon> Polygons = new List<TombEnginePolygon>();
        public List<TombEngineBucket> Buckets;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineBox
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public int TrueFloor;
        public int OverlapIndex;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineOverlap
    {
        public int Box;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineZoneGroup
    {
        public int[][] Zones  = new int[2][];

        public TombEngineZoneGroup()
        {
            foreach (int flipped in new[] { 0, 1 })
            {
                Zones[flipped] = new int[Enum.GetValues(typeof(LevelCompilerTombEngine.ZoneType)).Length];
                Array.Fill(Zones[flipped], int.MaxValue);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineCamera
    {
        public int X;
        public int Y;
        public int Z;
        public int Room;
        public int Flags;
        public int Speed;
        public string LuaName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineSink
    {
        public int X;
        public int Y;
        public int Z;
        public int Strength;
        public int BoxIndex;
        public string LuaName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineSoundSource
    {
        public int X;
        public int Y;
        public int Z;
        public int SoundID;
        public int Flags;
        public string LuaName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineStaticMesh
    {
        public int ObjectID;
        public int Mesh;
        public TombEngineBoundingBox VisibilityBox;
        public TombEngineBoundingBox CollisionBox;
        public ushort Flags;
        public short ShatterType;
        public short ShatterSound;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineMoveable
    {
        public int ObjectID;
        public int Skin;
        public int NumMeshes;
        public int StartingMesh;
        public int MeshTree;
        public int NumAnimations;
        public List<TombEngineAnimation> Animations;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(ObjectID);
            writer.Write(Skin);
            writer.Write(NumMeshes);
            writer.Write(StartingMesh);
            writer.Write(MeshTree);

            writer.Write(NumAnimations);
            foreach (var animation in Animations)
            {
                writer.Write(animation.StateID);
                writer.Write(animation.Interpolation);
                writer.Write(animation.FrameEnd);
                writer.Write(animation.NextAnimation);
                writer.Write(animation.NextFrame);
                writer.Write(animation.BlendFrameCount);
                writer.Write(animation.BlendCurve.Start);
                writer.Write(animation.BlendCurve.End);
                writer.Write(animation.BlendCurve.StartHandle);
                writer.Write(animation.BlendCurve.EndHandle);

                var startX = new Vector2(0.0f, animation.VelocityStart.X);
                var endX = new Vector2(1.0f, animation.VelocityEnd.X);
                var fixedMotionCurveX = new BezierCurve2(startX, endX, startX, endX);
                writer.Write(fixedMotionCurveX.Start);
                writer.Write(fixedMotionCurveX.End);
                writer.Write(fixedMotionCurveX.StartHandle);
                writer.Write(fixedMotionCurveX.EndHandle);

                var startY = new Vector2(0.0f, animation.VelocityStart.Y);
                var endY = new Vector2(1.0f, animation.VelocityEnd.Y);
                var fixedMotionCurveY = new BezierCurve2(startY, endY, startY, endY);
                writer.Write(fixedMotionCurveY.Start);
                writer.Write(fixedMotionCurveY.End);
                writer.Write(fixedMotionCurveY.StartHandle);
                writer.Write(fixedMotionCurveY.EndHandle);

                var startZ = new Vector2(0.0f, animation.VelocityStart.Z);
                var endZ = new Vector2(1.0f, animation.VelocityEnd.Z);
                var fixedMotionCurveZ = new BezierCurve2(startZ, endZ, startZ, endZ);
                writer.Write(fixedMotionCurveZ.Start);
                writer.Write(fixedMotionCurveZ.End);
                writer.Write(fixedMotionCurveZ.StartHandle);
                writer.Write(fixedMotionCurveZ.EndHandle);

                if (animation.KeyFrames.Count == 0)
                {
                    var defaultKeyFrame = new TombEngineKeyFrame();
                    defaultKeyFrame.BoneOrientations = new List<Quaternion>(Enumerable.Repeat(Quaternion.Identity, NumMeshes));

                    writer.Write(1);
                    defaultKeyFrame.Write(writer);
                }
                else
                {
                    writer.Write(animation.KeyFrames.Count);
                    foreach (var keyFrame in animation.KeyFrames)
                    {
                        keyFrame.Write(writer);
                    }
                }

                writer.Write(animation.StateChanges.Count);
                foreach (var stateChange in animation.StateChanges)
                {
                    writer.Write(stateChange.StateID);
                    writer.Write(stateChange.FrameLow);
                    writer.Write(stateChange.FrameHigh);
                    writer.Write(stateChange.NextAnimation);
                    writer.Write(stateChange.NextFrameLow);
                    writer.Write(stateChange.NextFrameHigh);
                    writer.Write(stateChange.BlendFrameCount);
                    writer.Write(stateChange.BlendCurve.Start);
                    writer.Write(stateChange.BlendCurve.End);
                    writer.Write(stateChange.BlendCurve.StartHandle);
                    writer.Write(stateChange.BlendCurve.EndHandle);
                }

                writer.Write(animation.NumAnimCommands);
                foreach (var element in animation.CommandData)
                {
                    if (element is int intComponent)
                    {
                        writer.Write(intComponent);
                    }
                    else if (element is Vector3 vector3Component)
                    {
                        writer.Write(vector3Component);
                    }
                }

                writer.Write(animation.Flags);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineStateChange
    {
        public int StateID;
        public int FrameLow;
        public int FrameHigh;
        public int NextAnimation;
        public int NextFrameLow;
        public int NextFrameHigh;
        public int BlendFrameCount;
        public BezierCurve2 BlendCurve;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineAnimation
    {
        public int StateID;
        public int Interpolation;
        public int FrameEnd;
        public int NextAnimation;
        public int NextFrame;
        public int BlendFrameCount;
        public BezierCurve2 BlendCurve;
        public Vector3 VelocityStart;
        public Vector3 VelocityEnd;
        public List<TombEngineKeyFrame> KeyFrames;
        public List<TombEngineStateChange> StateChanges;
        public int NumAnimCommands;
        public List<object> CommandData;
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TombEngineKeyFrame
    {
        public TombEngineBoundingBox BoundingBox;
        public Vector3 RootOffset;
        public List<Quaternion> BoneOrientations;

        public void Write(BinaryWriterEx writer)
        {
            var center = new Vector3(
                BoundingBox.X1 + BoundingBox.X2,
                BoundingBox.Y1 + BoundingBox.Y2,
                BoundingBox.Z1 + BoundingBox.Z2) / 2;
            var extents = new Vector3(
                BoundingBox.X2 - BoundingBox.X1,
                BoundingBox.Y2 - BoundingBox.Y1,
                BoundingBox.Z2 - BoundingBox.Z1) / 2;

            writer.Write(center);
            writer.Write(extents);
            writer.Write(RootOffset);

            writer.Write(BoneOrientations.Count);
            writer.WriteBlockArray(BoneOrientations);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineBoundingBox
    {
        public short X1;
        public short X2;
        public short Y1;
        public short Y2;
        public short Z1;
        public short Z2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineAiItem
    {
        public ushort ObjectID;
        public ushort Room;
        public int X;
        public int Y;
        public int Z;
        public short Yaw;
        public short Pitch;
        public short Roll;
        public short OCB;
        public ushort Flags;
        public int BoxIndex;
        public string LuaName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineItem
    {
        public ushort ObjectID;
        public short Room;
        public int X;
        public int Y;
        public int Z;
        public short Yaw;
        public short Pitch;
        public short Roll;
        public Vector4 Color;
        public short OCB;
        public ushort Flags;
        public string LuaName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TombEngineMirror
    {
        public short Room;
        public Vector4 Plane;
        public bool ReflectLara;
        public bool ReflectMoveables;
        public bool ReflectStatics;
        public bool ReflectSprites;
        public bool ReflectLights;
	}
}
