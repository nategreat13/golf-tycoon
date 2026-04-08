using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Progression
{
    public class AchievementTracker : MonoBehaviour
    {
        [SerializeField] private List<AchievementData> allAchievements;

        private HashSet<string> unlockedIds = new();

        public event System.Action<AchievementData> OnAchievementUnlocked;

        public void Initialize(List<string> savedAchievements)
        {
            unlockedIds = new HashSet<string>(savedAchievements ?? new List<string>());
        }

        public bool IsUnlocked(string achievementId) => unlockedIds.Contains(achievementId);

        public List<AchievementData> GetAll() => allAchievements;

        public List<AchievementData> GetUnlocked()
        {
            return allAchievements.FindAll(a => unlockedIds.Contains(a.achievementId));
        }

        public List<AchievementData> GetLocked()
        {
            return allAchievements.FindAll(a => !unlockedIds.Contains(a.achievementId));
        }

        public void CheckAndUnlock(AchievementData.AchievementType type, int currentValue)
        {
            foreach (var achievement in allAchievements)
            {
                if (achievement.type != type) continue;
                if (unlockedIds.Contains(achievement.achievementId)) continue;
                if (currentValue < achievement.targetValue) continue;

                Unlock(achievement);
            }
        }

        private void Unlock(AchievementData achievement)
        {
            unlockedIds.Add(achievement.achievementId);

            // Grant reputation reward
            if (achievement.reputationReward > 0)
            {
                var rep = ServiceLocator.Get<ReputationSystem>();
                rep?.AddReputation(achievement.reputationReward);
            }

            OnAchievementUnlocked?.Invoke(achievement);
            Debug.Log($"Achievement unlocked: {achievement.displayName}");
        }

        public List<string> GetSaveData()
        {
            return new List<string>(unlockedIds);
        }
    }
}
