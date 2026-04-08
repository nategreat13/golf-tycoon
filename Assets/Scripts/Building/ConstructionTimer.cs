using System;
using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Building
{
    /// <summary>
    /// Standalone construction timer component.
    /// Can be attached to any building/slot that needs a timed construction phase.
    /// </summary>
    public class ConstructionTimer : MonoBehaviour
    {
        public bool IsRunning { get; private set; }
        public float Duration { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float Progress => Duration > 0 ? 1f - (RemainingSeconds / Duration) : 0f;

        private long startTimeBinary;

        public event Action OnCompleted;
        public event Action<float> OnProgressUpdated; // 0 to 1

        public void StartTimer(float durationSeconds)
        {
            Duration = durationSeconds;
            startTimeBinary = DateTime.UtcNow.ToBinary();
            IsRunning = true;
            RemainingSeconds = durationSeconds;
        }

        public void StartTimer(float durationSeconds, long existingStartTime)
        {
            Duration = durationSeconds;
            startTimeBinary = existingStartTime;
            IsRunning = true;
            UpdateRemaining();
        }

        public void Cancel()
        {
            IsRunning = false;
            RemainingSeconds = 0;
        }

        /// <summary>
        /// Instantly complete the timer (e.g., for premium currency speed-up).
        /// </summary>
        public void InstantComplete()
        {
            if (!IsRunning) return;
            RemainingSeconds = 0;
            IsRunning = false;
            OnProgressUpdated?.Invoke(1f);
            OnCompleted?.Invoke();
        }

        private void Update()
        {
            if (!IsRunning) return;

            UpdateRemaining();

            OnProgressUpdated?.Invoke(Progress);

            if (RemainingSeconds <= 0)
            {
                IsRunning = false;
                RemainingSeconds = 0;
                OnCompleted?.Invoke();
            }
        }

        private void UpdateRemaining()
        {
            var time = ServiceLocator.Get<TimeManager>();
            if (time != null)
            {
                RemainingSeconds = time.GetRemainingSeconds(startTimeBinary, Duration);
            }
            else
            {
                // Fallback without TimeManager
                float elapsed = (float)(DateTime.UtcNow - DateTime.FromBinary(startTimeBinary)).TotalSeconds;
                RemainingSeconds = Mathf.Max(0, Duration - elapsed);
            }
        }

        public string GetFormattedTime()
        {
            if (RemainingSeconds <= 0) return "Done";
            int hours = (int)(RemainingSeconds / 3600);
            int minutes = (int)((RemainingSeconds % 3600) / 60);
            int seconds = (int)(RemainingSeconds % 60);

            if (hours > 0) return $"{hours}h {minutes}m";
            if (minutes > 0) return $"{minutes}m {seconds}s";
            return $"{seconds}s";
        }
    }
}
