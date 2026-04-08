using UnityEngine;
using GolfGame.Core;
using GolfGame.Building;
using GolfGame.Data;

namespace GolfGame.Economy
{
    public class IncomeCalculator : MonoBehaviour
    {
        /// <summary>
        /// Returns the total passive income per second from all sources.
        /// </summary>
        public float GetIncomePerSecond()
        {
            float total = 0;

            // Driving range income
            total += GetDrivingRangeIncome();

            // AI golfer income from holes
            total += GetAIGolferIncome();

            // Reputation multiplier
            total *= GetReputationMultiplier();

            return total;
        }

        public float GetDrivingRangeIncome()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property?.DrivingRange == null) return 0;

            int level = property.DrivingRange.level;
            return GameConstants.DrivingRangeBaseIncome *
                Mathf.Pow(GameConstants.DrivingRangeUpgradeMultiplier, level - 1) / 60f;
        }

        public float GetAIGolferIncome()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property == null) return 0;

            int builtHoles = property.GetBuiltHoleCount();
            if (builtHoles == 0) return 0;

            // Base income per hole per AI golfer visit
            float incomePerVisit = GameConstants.AIGolferBaseIncome * GetCourseQualityMultiplier();
            // Visits per second
            float visitsPerSecond = 1f / GameConstants.AIGolferIntervalSeconds;
            // Each hole generates income independently
            return incomePerVisit * visitsPerSecond * builtHoles;
        }

        public float GetRealPlayerVisitIncome()
        {
            return GameConstants.AIGolferBaseIncome *
                GameConstants.RealPlayerIncomeMultiplier *
                GetCourseQualityMultiplier();
        }

        public float GetCourseQualityMultiplier()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property == null) return 1f;

            float totalQuality = 0;
            int count = 0;
            foreach (var hole in property.Holes)
            {
                if (hole.state == HoleSlotState.Built)
                {
                    totalQuality += hole.qualityLevel;
                    count++;
                }
            }

            if (count == 0) return 1f;
            float avgQuality = totalQuality / count;
            return 1f + (avgQuality - 1f) * 0.25f; // Each quality level adds 25%
        }

        public float GetReputationMultiplier()
        {
            var rep = ServiceLocator.Get<Progression.ReputationSystem>();
            if (rep == null) return 1f;
            return 1f + rep.Reputation / 1000f;
        }
    }
}
