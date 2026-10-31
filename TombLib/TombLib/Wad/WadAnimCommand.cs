using NLog;
using System;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadAnimCommand : ICloneable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WadAnimCommandType Type { get; set; }
        public short Parameter1 { get; set; }
        public short Parameter2 { get; set; }
        public short Parameter3 { get; set; }

        public bool FrameBased => Type >= WadAnimCommandType.PlaySound;
        public bool PositionBased => Type == WadAnimCommandType.SetPosition;
        public bool VelocityBased => Type == WadAnimCommandType.SetJumpDistance;

        public string Description => ToString();
        public override string ToString()
        {
            switch (Type)
            {
                case WadAnimCommandType.EmptyHands:
                    return "Remove guns from hands";
                case WadAnimCommandType.SetJumpDistance:
                    return "Set jump reference <H, V> = <" + Parameter1 + ", " + Parameter2 + ">";
                case WadAnimCommandType.KillEntity:
                    return "Kill entity";
                case WadAnimCommandType.SetPosition:
                    return "Set position reference <X, Y, Z> = <" + Parameter1 + ", " + Parameter2 + ", " + Parameter3 + ">";
                case WadAnimCommandType.PlaySound:
                    return "Play Sound ID = " + Parameter2 + " (" + ((WadSoundEnvironmentType)Parameter3).ToString() + ") on Frame = " + Parameter1;
                case WadAnimCommandType.FlipEffect:
                    return "Play FlipEffect ID = " + Parameter2 + 
                        (Parameter3 == 0 ? string.Empty : (" (" + ((WadFootstepFlipeffectCondition)Parameter3).ToString().SplitCamelcase() + ")")) +
                        " on Frame = " + Parameter1;
                case WadAnimCommandType.DisableInterpolation:
                    return "Disable interpolation on Frame = " + Parameter1;
                case WadAnimCommandType.DetachFromParent:
                    return "Detach from parent on Frame = " + Parameter1;
                default:
                    return Type.ToString().SplitCamelcase();
            }
        }

        public WadAnimCommand Clone() => (WadAnimCommand)MemberwiseClone();
        object ICloneable.Clone() => Clone();

        public static bool DistinctiveEquals(WadAnimCommand first, WadAnimCommand second, bool considerFrames)
        {
            if (ReferenceEquals(first, null) != ReferenceEquals(second, null))
                return false;
            else if ((ReferenceEquals(first, null) == true) && (ReferenceEquals(second, null) == true))
                return true;

            if (first.Type != second.Type) return false;

            if (first.FrameBased)
            {
                return ((!considerFrames || first.Parameter1 == second.Parameter1) &&
                                            first.Parameter2 == second.Parameter2 &&
                                            first.Parameter3 == second.Parameter3);
            }
            else if (first.VelocityBased)
            {
                return (first.Parameter1 == second.Parameter1 &&
                        first.Parameter2 == second.Parameter2);
            }
            else if (first.PositionBased)
            {
                return (first.Parameter1 == second.Parameter1 &&
                        first.Parameter2 == second.Parameter2 &&
                        first.Parameter3 == second.Parameter3);
            }
            else
                return true; // Equal or unknown command
        }

        public void ConvertLegacyConditions()
        {
            if (Parameter3 != 0)
            {
                logger.Warn("Attempt to convert legacy conditions for animcommand which were already converted.");
                return;
            }

            if (Type == WadAnimCommandType.FlipEffect)
            {
                switch (Parameter2 & 0xC000)
                {
                    case (1 << 14):
                        Parameter3 = (short)WadFootstepFlipeffectCondition.LeftFoot;
                        break;

                    case (1 << 15):
                        Parameter3 = (short)WadFootstepFlipeffectCondition.RightFoot;
                        break;

                    default:
                        Parameter3 = (short)WadFootstepFlipeffectCondition.Always;
                        break;
                }

                Parameter2 = (short)(Parameter2 & 0x3FFF);
            }
            else if (Type == WadAnimCommandType.PlaySound)
            {
                switch (Parameter2 & 0xF000)
                {
                    case 0:
                    default:
                        Parameter3 = (short)WadSoundEnvironmentType.Always;
                        break;

                    case (1 << 14):
                        Parameter3 = (short)WadSoundEnvironmentType.Land;
                        break;

                    case (1 << 15):
                        Parameter3 = (short)WadSoundEnvironmentType.Water;
                        break;

                    case (1 << 12):
                        Parameter3 = (short)WadSoundEnvironmentType.Quicksand;
                        break;

                    case (1 << 13):
                        Parameter3 = (short)WadSoundEnvironmentType.Underwater;
                        break;
                }

                Parameter2 = (short)(Parameter2 & 0x0FFF);
            }
            else
            {
                throw new Exception("Attempt to convert sound environment type on a non-PlaySound animcommand");
            }
        }

        public short GetLegacyBitmask()
        {
            short result = 0;

            if (Type == WadAnimCommandType.FlipEffect)
            {
                switch ((WadFootstepFlipeffectCondition)Parameter3)
                {
                    case WadFootstepFlipeffectCondition.LeftFoot:
                        result = unchecked((short)(1 << 14));
                        break;

                    case WadFootstepFlipeffectCondition.RightFoot:
                        result = unchecked((short)(1 << 15));
                        break;

                    default:
                        result = 0;
                        break;
                }
            }
            else if (Type == WadAnimCommandType.PlaySound)
            {
                switch ((WadSoundEnvironmentType)Parameter3)
                {
                    case WadSoundEnvironmentType.Land:
                        result = unchecked((short)(1 << 14));
                        break;

                    case WadSoundEnvironmentType.Water:
                        result = unchecked((short)(1 << 15));
                        break;

                    // New sound conditions are ignored for classic engines.
                    default:
                        result = 0;
                        break;
                }
            }

            return result;
        }

        public static bool operator ==(WadAnimCommand first, WadAnimCommand second) => DistinctiveEquals(first, second, true);
        public static bool operator !=(WadAnimCommand first, WadAnimCommand second) => !(first == second);

        public bool Equals(WadAnimCommand other) => this == other;
        public override bool Equals(object other) => other is WadAnimCommand && this == (WadAnimCommand)other;
        public override int GetHashCode() => ("t" + Type + "p1" + Parameter1 + "p2" + Parameter2 + "p3" + Parameter3).GetHashCode();
    }
}
