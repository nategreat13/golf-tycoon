using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf.ShotMechanic
{
    public class PowerBar : MonoBehaviour
    {
        [SerializeField] private float speed = GameConstants.PowerBarSpeed;
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);

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

        public void Deactivate()
        {
            IsActive = false;
            IsLocked = false;
            Value = 0f;
        }

        public float Lock()
        {
            IsLocked = true;
            IsActive = false;
            return Value;
        }

        private void Update()
        {
            if (!IsActive || IsLocked) return;

            time += Time.deltaTime * speed * speedCurve.Evaluate(Value);
            Value = Mathf.PingPong(time, 1f);
        }
    }
}
