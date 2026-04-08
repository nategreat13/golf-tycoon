using System;
using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Data
{
    [Serializable]
    public class PlayerSaveData
    {
        public string playerId;
        public string playerName;
        public long currency;
        public int reputation;
        public long lastSaveTimeUtc;
        public CourseSaveData courseData;
        public ClubSaveData clubData;
        public List<string> achievements;

        public static PlayerSaveData CreateDefault()
        {
            return new PlayerSaveData
            {
                playerId = Guid.NewGuid().ToString(),
                playerName = "Golfer",
                currency = GameConstants.StartingCurrency,
                reputation = 0,
                lastSaveTimeUtc = DateTime.UtcNow.ToBinary(),
                courseData = CourseSaveData.CreateDefault(),
                clubData = ClubSaveData.CreateDefault(),
                achievements = new List<string>()
            };
        }
    }

    [Serializable]
    public class CourseSaveData
    {
        public string courseName;
        public int currentTier; // 1, 3, 9, or 18
        public List<HoleSaveData> holes;
        public DrivingRangeSaveData drivingRange;
        public bool isExpanding;
        public long expansionStartTimeUtc;

        public static CourseSaveData CreateDefault()
        {
            return new CourseSaveData
            {
                courseName = "My Course",
                currentTier = 1,
                holes = new List<HoleSaveData>
                {
                    HoleSaveData.CreateDefault(0)
                },
                drivingRange = new DrivingRangeSaveData { level = 1 },
                isExpanding = false,
                expansionStartTimeUtc = 0
            };
        }
    }

    [Serializable]
    public class HoleSaveData
    {
        public int slotIndex;
        public HoleSlotState state;
        public int par;
        public HoleLength length;
        public HoleHazardSet hazardSet;
        public int themeIndex;
        public int qualityLevel;
        public long constructionStartTimeUtc;
        public int courseRecord; // best score ever on this hole (strokes)
        public int timesPlayed;

        public static HoleSaveData CreateDefault(int index)
        {
            return new HoleSaveData
            {
                slotIndex = index,
                state = HoleSlotState.Built,
                par = 3,
                length = HoleLength.Short,
                hazardSet = HoleHazardSet.None,
                themeIndex = 0,
                qualityLevel = 1,
                constructionStartTimeUtc = 0,
                courseRecord = 0,
                timesPlayed = 0
            };
        }
    }

    [Serializable]
    public class DrivingRangeSaveData
    {
        public int level;
        public long lastCollectionTimeUtc;
    }

    [Serializable]
    public class ClubSaveData
    {
        public List<OwnedClub> ownedClubs;

        public static ClubSaveData CreateDefault()
        {
            return new ClubSaveData
            {
                ownedClubs = new List<OwnedClub>
                {
                    new OwnedClub { clubId = "7iron", upgradeLevel = 0 },
                    new OwnedClub { clubId = "putter", upgradeLevel = 0 }
                }
            };
        }
    }

    [Serializable]
    public class OwnedClub
    {
        public string clubId;
        public int upgradeLevel;
    }

    public enum HoleSlotState
    {
        Empty,
        Designing,
        UnderConstruction,
        Built,
        Upgrading
    }

    public enum HoleLength
    {
        Short,   // ~100-150 yards
        Medium,  // ~200-350 yards
        Long     // ~400-550 yards
    }

    public enum HoleHazardSet
    {
        None,
        Bunker,
        Water,
        BunkerAndWater,
        TreeLined
    }
}
