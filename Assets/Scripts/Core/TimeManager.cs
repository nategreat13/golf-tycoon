using System;
using UnityEngine;

namespace GolfGame.Core
{
    public class TimeManager : MonoBehaviour
    {
        private const string LAST_SAVE_KEY = "LastSaveTime";

        public DateTime GetLastSaveTime()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            if (saveSystem?.Data != null && saveSystem.Data.lastSaveTimeUtc != 0)
            {
                return DateTime.FromBinary(saveSystem.Data.lastSaveTimeUtc);
            }
            return DateTime.UtcNow;
        }

        public void UpdateLastSaveTime()
        {
            var saveSystem = ServiceLocator.Get<SaveSystem>();
            if (saveSystem?.Data != null)
            {
                saveSystem.Data.lastSaveTimeUtc = DateTime.UtcNow.ToBinary();
            }
        }

        public float GetSecondsSince(DateTime utcTime)
        {
            return (float)(DateTime.UtcNow - utcTime).TotalSeconds;
        }

        public float GetSecondsSince(long binaryTime)
        {
            if (binaryTime == 0) return 0;
            return GetSecondsSince(DateTime.FromBinary(binaryTime));
        }

        public bool HasTimePassed(long startTimeBinary, float durationSeconds)
        {
            if (startTimeBinary == 0) return false;
            return GetSecondsSince(startTimeBinary) >= durationSeconds;
        }

        public float GetRemainingSeconds(long startTimeBinary, float durationSeconds)
        {
            if (startTimeBinary == 0) return durationSeconds;
            float elapsed = GetSecondsSince(startTimeBinary);
            return Mathf.Max(0, durationSeconds - elapsed);
        }
    }
}
