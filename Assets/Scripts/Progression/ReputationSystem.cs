using System;
using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Progression
{
    public class ReputationSystem : MonoBehaviour
    {
        public int Reputation { get; private set; }
        public int Level => GetLevel(Reputation);

        public event Action<int, int> OnReputationChanged; // old, new

        public void SetReputation(int value)
        {
            Reputation = Mathf.Max(0, value);
        }

        public void AddReputation(int amount)
        {
            if (amount <= 0) return;
            int old = Reputation;
            Reputation += amount;

            EventBus.Publish(new ReputationChangedEvent
            {
                oldReputation = old,
                newReputation = Reputation
            });
            OnReputationChanged?.Invoke(old, Reputation);
        }

        public static int GetLevel(int reputation)
        {
            // Simple level calculation: sqrt-based curve
            return 1 + (int)Mathf.Sqrt(reputation / 50f);
        }

        public float GetProgressToNextLevel()
        {
            int currentLevel = Level;
            int repForCurrent = GetReputationForLevel(currentLevel);
            int repForNext = GetReputationForLevel(currentLevel + 1);
            float range = repForNext - repForCurrent;
            if (range <= 0) return 1f;
            return (Reputation - repForCurrent) / range;
        }

        public static int GetReputationForLevel(int level)
        {
            int l = level - 1;
            return l * l * 50;
        }

        public string GetTitle()
        {
            int level = Level;
            return level switch
            {
                <= 3 => "Groundskeeper",
                <= 6 => "Club Manager",
                <= 10 => "Course Director",
                <= 15 => "Head Pro",
                <= 20 => "Golf Tycoon",
                <= 30 => "Legend",
                _ => "Hall of Famer"
            };
        }
    }
}
