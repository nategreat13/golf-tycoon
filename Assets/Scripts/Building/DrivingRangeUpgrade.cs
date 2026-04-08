using UnityEngine;

namespace GolfGame.Building
{
    /// <summary>
    /// UI helper for displaying driving range upgrade information.
    /// </summary>
    public class DrivingRangeUpgrade : MonoBehaviour
    {
        public struct UpgradeInfo
        {
            public int currentLevel;
            public int maxLevel;
            public float currentIncomePerMin;
            public float nextIncomePerMin;
            public long upgradeCost;
            public bool canUpgrade;
            public bool isMaxLevel;
        }

        public static UpgradeInfo GetInfo(DrivingRangeManager range, long playerCurrency)
        {
            bool isMax = range.Level >= DrivingRangeManager.MaxLevel;
            float currentIncome = range.IncomePerMinute;
            float nextIncome = isMax ? currentIncome :
                GolfGame.Data.GameConstants.DrivingRangeBaseIncome *
                Mathf.Pow(GolfGame.Data.GameConstants.DrivingRangeUpgradeMultiplier, range.Level);

            long cost = isMax ? 0 : range.GetUpgradeCost();

            return new UpgradeInfo
            {
                currentLevel = range.Level,
                maxLevel = DrivingRangeManager.MaxLevel,
                currentIncomePerMin = currentIncome,
                nextIncomePerMin = nextIncome,
                upgradeCost = cost,
                canUpgrade = !isMax && playerCurrency >= cost,
                isMaxLevel = isMax
            };
        }
    }
}
