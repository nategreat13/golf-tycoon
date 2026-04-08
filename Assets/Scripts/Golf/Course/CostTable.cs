using UnityEngine;

namespace GolfGame.Golf
{
    [CreateAssetMenu(fileName = "CostTable", menuName = "Golf/Cost Table")]
    public class CostTable : ScriptableObject
    {
        [Header("Hole Construction")]
        public long holeBaseCost = 1000;
        public float holeCostPerTier = 1.5f; // multiplier per quality level

        [Header("Hole Upgrades")]
        public long holeUpgradeBaseCost = 500;
        public float holeUpgradeCostMultiplier = 1.5f;

        [Header("Driving Range")]
        public long drivingRangeUpgradeCost = 300;
        public float drivingRangeUpgradeMultiplier = 1.4f;

        [Header("Course Expansion")]
        public long expansion1To3Cost = 5000;
        public long expansion3To9Cost = 25000;
        public long expansion9To18Cost = 100000;

        [Header("Construction Times (seconds)")]
        public float holeConstructionTime = 300f;
        public float holeUpgradeTime = 600f;
        public float drivingRangeUpgradeTime = 180f;
        public float expansion1To3Time = 900f;
        public float expansion3To9Time = 1800f;
        public float expansion9To18Time = 3600f;

        public long GetHoleCost(int qualityLevel)
        {
            return (long)(holeBaseCost * Mathf.Pow(holeCostPerTier, qualityLevel - 1));
        }

        public long GetHoleUpgradeCost(int currentLevel)
        {
            return (long)(holeUpgradeBaseCost * Mathf.Pow(holeUpgradeCostMultiplier, currentLevel));
        }

        public long GetDrivingRangeUpgradeCost(int currentLevel)
        {
            return (long)(drivingRangeUpgradeCost * Mathf.Pow(drivingRangeUpgradeMultiplier, currentLevel - 1));
        }

        public long GetExpansionCost(int currentTier)
        {
            return currentTier switch
            {
                1 => expansion1To3Cost,
                3 => expansion3To9Cost,
                9 => expansion9To18Cost,
                _ => long.MaxValue
            };
        }

        public float GetExpansionTime(int currentTier)
        {
            return currentTier switch
            {
                1 => expansion1To3Time,
                3 => expansion3To9Time,
                9 => expansion9To18Time,
                _ => float.MaxValue
            };
        }
    }
}
