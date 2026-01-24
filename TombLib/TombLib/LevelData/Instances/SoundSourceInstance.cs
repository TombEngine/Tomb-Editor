using TombLib.Wad;

namespace TombLib.LevelData
{
    public enum SoundSourcePlayMode
    {
        Always = 0,
        OnlyInBaseRoom = 1,
        OnlyInAlternateRoom = 2,
        Automatic = 3
    }

    public class SoundSourceInstance : PositionAndScriptBasedObjectInstance, IReplaceable
    {
        public int SoundId { get; set; } = -1;
        public SoundSourcePlayMode PlayMode { get; set; } = SoundSourcePlayMode.Automatic;

        public bool IsEmpty => SoundId == -1;

        public WadSoundInfo GetSoundInfo(Level level)
        {
            if (SoundId != -1)
                return level.Settings.WadTryGetSoundInfo(SoundId);
            else
                return null;
        }

        public string SoundNameToDisplay
        {
            get
            {
                if (IsEmpty || Room == null || Room.Level == null)
                    return "Empty";
                var info = GetSoundInfo(Room.Level);
                if (info == null)
                    return "Empty";
                else
                    return info.Name;
            }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Sound source " + SoundNameToDisplay +
                " (" + (Room?.ToString() ?? "NULL") + ")" +
                ", X = " + SectorPosition.X +
                ", Z = " + SectorPosition.Y +
                GetScriptIDOrName(false);
        }

        public override string ToShortString() => ShortName;

        public string PrimaryAttribDesc => "Sound ID";
        public string SecondaryAttribDesc => string.Empty;

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            return (other is SoundSourceInstance && (other as SoundSourceInstance)?.SoundId == SoundId);
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var thatSound = (SoundSourceInstance)other;
            if (!ReplaceableEquals(other) && thatSound.SoundId != SoundId)
            {
                SoundId = ((SoundSourceInstance)other).SoundId;
                return true;
            }
            else
                return false;
        }

        public string ShortName => "Sound source (ID = " + SoundId + ")" + GetScriptIDOrName() + " (" + (Room?.ToString() ?? "NULL") + ")";
    }
}
