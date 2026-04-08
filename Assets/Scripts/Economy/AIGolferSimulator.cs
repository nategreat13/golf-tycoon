using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Economy
{
    /// <summary>
    /// Simulates AI golfers visiting the player's course and generating income.
    /// Ticks on a regular interval to add passive income.
    /// </summary>
    public class AIGolferSimulator : MonoBehaviour
    {
        private float tickTimer;
        private float tickInterval = 5f; // Check every 5 seconds for smoother income

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;
                SimulateTick();
            }
        }

        private void SimulateTick()
        {
            var income = ServiceLocator.Get<IncomeCalculator>();
            var currency = ServiceLocator.Get<CurrencyManager>();

            if (income == null || currency == null) return;

            float incomePerSecond = income.GetIncomePerSecond();
            long earned = (long)(incomePerSecond * tickInterval);

            if (earned > 0)
            {
                currency.Add(earned);
            }
        }
    }
}
