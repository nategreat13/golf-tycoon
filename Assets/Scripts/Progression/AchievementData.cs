using UnityEngine;

namespace GolfGame.Progression
{
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "Golf/Achievement")]
    public class AchievementData : ScriptableObject
    {
        public string achievementId;
        public string displayName;
        public string description;
        public Sprite icon;
        public int reputationReward;

        public enum AchievementType
        {
            HolesBuilt,
            RoundsPlayed,
            CourseExpanded,
            ScoreUnderPar,
            HoleInOne,
            VisitorCount,
            ReputationReached,
            ClubsOwned,
            DrivingRangeLevel
        }

        public AchievementType type;
        public int targetValue;
    }
}
