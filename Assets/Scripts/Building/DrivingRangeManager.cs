using System;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Building
{
    public class DrivingRangeManager : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public const int MaxLevel = 10;

        public float IncomePerMinute =>
            GameConstants.DrivingRangeBaseIncome * Mathf.Pow(GameConstants.DrivingRangeUpgradeMultiplier, Level - 1);

        public float IncomePerSecond => IncomePerMinute / 60f;

        public event Action<int> OnLevelChanged;

        public void Initialize(DrivingRangeSaveData data)
        {
            if (data != null)
                Level = Mathf.Clamp(data.level, 1, MaxLevel);
        }

        public long GetUpgradeCost()
        {
            return (long)(GameConstants.DrivingRangeUpgradeCost *
                Mathf.Pow(GameConstants.DrivingRangeUpgradeMultiplier, Level - 1));
        }

        public bool CanUpgrade()
        {
            if (Level >= MaxLevel) return false;
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            return currency != null && currency.Amount >= GetUpgradeCost();
        }

        public bool TryUpgrade()
        {
            if (!CanUpgrade()) return false;

            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (!currency.TrySpend(GetUpgradeCost())) return false;

            Level++;
            OnLevelChanged?.Invoke(Level);
            return true;
        }

        public DrivingRangeSaveData GetSaveData()
        {
            return new DrivingRangeSaveData
            {
                level = Level,
                lastCollectionTimeUtc = DateTime.UtcNow.ToBinary()
            };
        }
    }
}
