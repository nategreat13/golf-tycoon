using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf.Ball
{
    /// <summary>
    /// Additional physics layer on top of Unity Rigidbody for golf-specific behavior.
    /// Handles green slope influence, wind (future), and spin effects.
    /// </summary>
    [RequireComponent(typeof(GolfBall))]
    [RequireComponent(typeof(Rigidbody))]
    public class BallPhysics : MonoBehaviour
    {
        [Header("Green Slope")]
        [SerializeField] private float slopeForce = 2f;

        [Header("Wind (Future)")]
        [SerializeField] private Vector3 windDirection = Vector3.zero;
        [SerializeField] private float windStrength = 0f;

        [Header("Bounce")]
        [SerializeField] private float bounceRestitution = GameConstants.BallBounceRestitution;

        private GolfBall ball;
        private Rigidbody rb;
        private Vector2 currentGreenSlope;
        private float currentGreenSlopeStrength;

        private void Awake()
        {
            ball = GetComponent<GolfBall>();
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Set by GreenPhysics when ball enters a green zone.
        /// </summary>
        public void SetGreenSlope(Vector2 slopeDirection, float strength)
        {
            currentGreenSlope = slopeDirection;
            currentGreenSlopeStrength = strength;
        }

        public void ClearGreenSlope()
        {
            currentGreenSlope = Vector2.zero;
            currentGreenSlopeStrength = 0f;
        }

        private void FixedUpdate()
        {
            // Apply green slope when on green
            if (ball.State == BallState.OnGreen && currentGreenSlopeStrength > 0f)
            {
                Vector3 slopeForceVec = new Vector3(
                    currentGreenSlope.x,
                    0f,
                    currentGreenSlope.y
                ) * currentGreenSlopeStrength * slopeForce;

                rb.AddForce(slopeForceVec, ForceMode.Acceleration);
            }

            // Apply wind when in flight
            if (ball.State == BallState.InFlight && windStrength > 0f)
            {
                rb.AddForce(windDirection.normalized * windStrength, ForceMode.Acceleration);
            }
        }

        public void SetWind(Vector3 direction, float strength)
        {
            windDirection = direction;
            windStrength = strength;
        }
    }
}
