using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf
{
    /// <summary>
    /// UI helper for displaying club upgrade information.
    /// </summary>
    public class ClubUpgrade : MonoBehaviour
    {
        public struct UpgradeInfo
        {
            public string clubName;
            public int currentLevel;
            public int maxLevel;
            public float currentDistance;
            public float nextDistance;
            public float currentAccuracy;
            public float nextAccuracy;
            public long upgradeCost;
            public bool canUpgrade;
            public bool isMaxLevel;
        }

        public static UpgradeInfo GetUpgradeInfo(ClubData club, int currentLevel, long playerCurrency)
        {
            bool isMax = currentLevel >= GameConstants.MaxClubUpgradeLevel;
            long cost = isMax ? 0 : club.GetUpgradeCost(currentLevel);

            return new UpgradeInfo
            {
                clubName = club.clubName,
                currentLevel = currentLevel,
                maxLevel = GameConstants.MaxClubUpgradeLevel,
                currentDistance = club.GetDistance(currentLevel),
                nextDistance = isMax ? club.GetDistance(currentLevel) : club.GetDistance(currentLevel + 1),
                currentAccuracy = club.GetAccuracy(currentLevel),
                nextAccuracy = isMax ? club.GetAccuracy(currentLevel) : club.GetAccuracy(currentLevel + 1),
                upgradeCost = cost,
                canUpgrade = !isMax && playerCurrency >= cost,
                isMaxLevel = isMax
            };
        }
    }
}
