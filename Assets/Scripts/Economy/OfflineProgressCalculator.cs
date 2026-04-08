using System;
using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Economy
{
    public class OfflineProgressCalculator
    {
        /// <summary>
        /// Calculates how much currency the player earned while offline.
        /// </summary>
        public long Calculate(DateTime lastSaveTime, float incomePerSecond)
        {
            float secondsOffline = (float)(DateTime.UtcNow - lastSaveTime).TotalSeconds;

            if (secondsOffline <= 0) return 0;

            // Cap at max offline hours
            float maxSeconds = GameConstants.MaxOfflineHours * 3600f;
            secondsOffline = Mathf.Min(secondsOffline, maxSeconds);

            long earnings = (long)(incomePerSecond * secondsOffline);
            return Mathf.Max(0, (int)earnings);
        }
    }
}
