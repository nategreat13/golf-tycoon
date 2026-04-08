using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf.ShotMechanic
{
    public class AccuracyBar : MonoBehaviour
    {
        [SerializeField] private float speed = GameConstants.AccuracyBarSpeed;
        [SerializeField] private float maxDeviation = GameConstants.MaxAccuracyDeviation;

        /// <summary>
        /// Current value from -1 (far left) to 1 (far right). 0 = perfect center.
        /// </summary>
        public float Value { get; private set; }
        public bool IsLocked { get; private set; }
        public bool IsActive { get; private set; }

        private float time;

        public void Activate()
        {
            IsActive = true;
            IsLocked = false;
            Value = 0f;
            time = 0f;
        }

        public void Activate(float clubAccuracyModifier)
        {
            Activate();
            // Higher accuracy modifier = slower bar = easier to hit center
            speed = GameConstants.AccuracyBarSpeed / Mathf.Max(clubAccuracyModifier, 0.1f);
        }

        public void Deactivate()
        {
            IsActive = false;
            IsLocked = false;
            Value = 0f;
        }

        /// <summary>
        /// Locks the bar and returns the deviation angle in degrees.
        /// Negative = left, Positive = right, 0 = perfect.
        /// </summary>
        public float Lock()
        {
            IsLocked = true;
            IsActive = false;
            return Value * maxDeviation;
        }

        private void Update()
        {
            if (!IsActive || IsLocked) return;

            time += Time.deltaTime * speed;
            // Oscillate from -1 to 1
            Value = Mathf.Sin(time * Mathf.PI);
        }
    }
}
