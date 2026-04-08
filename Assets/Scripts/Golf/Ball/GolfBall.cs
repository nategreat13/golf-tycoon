using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf.Ball
{
    public enum BallState
    {
        OnTee,
        InFlight,
        OnFairway,
        OnRough,
        OnGreen,
        InBunker,
        InWater,
        OutOfBounds,
        Holed
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class GolfBall : MonoBehaviour
    {
        [SerializeField] private float stopSpeedThreshold = GameConstants.BallStopSpeed;
        [SerializeField] private float maxFlightTime = 15f;

        public BallState State { get; private set; } = BallState.OnTee;
        public bool IsStationary => State != BallState.InFlight && rb.linearVelocity.magnitude < stopSpeedThreshold;
        public bool IsHoled => State == BallState.Holed;

        private Rigidbody rb;
        private Vector3 lastStablePosition;
        private float flightTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 0.045f; // Golf ball ~45 grams
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void PlaceOnTee(Vector3 position)
        {
            transform.position = position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            State = BallState.OnTee;
            lastStablePosition = position;
        }

        public void Launch(Vector3 velocity)
        {
            State = BallState.InFlight;
            rb.isKinematic = false;
            rb.linearVelocity = velocity;
            flightTimer = 0f;
        }

        private void FixedUpdate()
        {
            if (State == BallState.InFlight)
            {
                flightTimer += Time.fixedDeltaTime;

                // Safety timeout
                if (flightTimer > maxFlightTime)
                {
                    ReturnToLastStable();
                    return;
                }

                // Check if ball has come to rest after initial launch
                if (flightTimer > 0.5f && rb.linearVelocity.magnitude < stopSpeedThreshold)
                {
                    OnBallStopped();
                }
            }
            else if (State == BallState.OnGreen || State == BallState.OnFairway || State == BallState.OnRough)
            {
                // Apply friction for rolling on ground
                if (rb.linearVelocity.magnitude < stopSpeedThreshold)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (State != BallState.InFlight) return;

            // Determine terrain type from collision tag
            string tag = collision.gameObject.tag;
            switch (tag)
            {
                case "Green":
                    State = BallState.OnGreen;
                    ApplyFriction(GameConstants.GreenFriction);
                    break;
                case "Fairway":
                    State = BallState.OnFairway;
                    ApplyFriction(GameConstants.BallGroundFriction);
                    break;
                case "Rough":
                    State = BallState.OnRough;
                    ApplyFriction(GameConstants.BallGroundFriction * 1.5f);
                    break;
                case "Bunker":
                    State = BallState.InBunker;
                    ApplyFriction(GameConstants.BallGroundFriction * 3f);
                    break;
                default:
                    State = BallState.OnFairway;
                    ApplyFriction(GameConstants.BallGroundFriction);
                    break;
            }

            lastStablePosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Hole"))
            {
                // Check if ball is moving slow enough to drop in
                if (rb.linearVelocity.magnitude < 2f)
                {
                    State = BallState.Holed;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    // Move ball into the hole
                    transform.position = other.transform.position;
                }
            }
            else if (other.CompareTag("Water"))
            {
                State = BallState.InWater;
                // Penalty handled by game logic
                Invoke(nameof(ReturnToLastStable), 1f);
            }
            else if (other.CompareTag("OutOfBounds"))
            {
                State = BallState.OutOfBounds;
                Invoke(nameof(ReturnToLastStable), 1f);
            }
        }

        private void OnBallStopped()
        {
            if (State == BallState.InFlight)
            {
                State = BallState.OnFairway; // Default
            }
            lastStablePosition = transform.position;
        }

        private void ReturnToLastStable()
        {
            transform.position = lastStablePosition;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            State = BallState.OnFairway;
        }

        private void ApplyFriction(float friction)
        {
            rb.linearDamping = friction;
        }

        /// <summary>
        /// Returns the terrain modifier for the next shot based on current state.
        /// </summary>
        public float GetTerrainModifier()
        {
            return State switch
            {
                BallState.InBunker => GameConstants.BunkerPenaltyMultiplier,
                BallState.OnRough => 0.8f,
                BallState.OnGreen => 1f,
                BallState.OnFairway => 1f,
                BallState.OnTee => 1f,
                _ => 1f
            };
        }
    }
}
