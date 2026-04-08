using UnityEngine;

namespace GolfGame.Golf.Ball
{
    /// <summary>
    /// Dedicated ball-follow camera for flight sequences.
    /// Tracks the ball from behind and to the side during flight,
    /// and settles into a standard behind-ball view when grounded.
    /// Can be used as a secondary camera or as a Cinemachine-compatible follow target.
    /// </summary>
    public class BallCamera : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform ballTransform;

        [Header("Grounded View (Tee / Fairway)")]
        [SerializeField] private Vector3 groundedOffset = new Vector3(0, 2f, -3f);
        [SerializeField] private float groundedSmoothTime = 0.4f;
        [SerializeField] private float groundedRotationSpeed = 5f;

        [Header("Flight View")]
        [SerializeField] private float flightSideOffset = 4f;
        [SerializeField] private float flightHeight = 5f;
        [SerializeField] private float flightBehind = 2f;
        [SerializeField] private float flightSmoothTime = 0.12f;
        [SerializeField] private float flightRotationSpeed = 10f;
        [SerializeField] private float flightPullBackRate = 1.5f;
        [SerializeField] private float flightMaxPullBack = 10f;

        [Header("Green View")]
        [SerializeField] private Vector3 greenOffset = new Vector3(0, 0.5f, -1.5f);
        [SerializeField] private float greenSmoothTime = 0.5f;
        [SerializeField] private float greenRotationSpeed = 4f;

        private Vector3 currentVelocity;
        private GolfBall golfBall;
        private float flightTime;
        private Vector3 launchDirection = Vector3.forward;

        private void Start()
        {
            if (ballTransform != null)
                golfBall = ballTransform.GetComponent<GolfBall>();
        }

        private void LateUpdate()
        {
            if (ballTransform == null) return;

            if (golfBall == null)
            {
                // Simple fallback follow
                FollowGrounded(groundedOffset, groundedSmoothTime, groundedRotationSpeed);
                return;
            }

            switch (golfBall.State)
            {
                case BallState.InFlight:
                    FollowFlight();
                    break;
                case BallState.OnGreen:
                    FollowGrounded(greenOffset, greenSmoothTime, greenRotationSpeed);
                    break;
                default:
                    flightTime = 0f;
                    FollowGrounded(groundedOffset, groundedSmoothTime, groundedRotationSpeed);
                    break;
            }
        }

        private void FollowFlight()
        {
            flightTime += Time.deltaTime;

            // Infer flight direction from the ball's velocity
            Rigidbody rb = ballTransform.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.5f)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                if (vel.sqrMagnitude > 0.1f)
                    launchDirection = vel.normalized;
            }

            float pullBack = Mathf.Min(flightBehind + flightTime * flightPullBackRate, flightMaxPullBack);
            Vector3 side = Vector3.Cross(Vector3.up, launchDirection).normalized;

            Vector3 targetPos = ballTransform.position
                - launchDirection * pullBack
                + side * flightSideOffset
                + Vector3.up * flightHeight;

            transform.position = Vector3.SmoothDamp(
                transform.position, targetPos, ref currentVelocity, flightSmoothTime);

            // Look slightly ahead of ball
            Vector3 lookTarget = ballTransform.position + launchDirection * 2f;
            SmoothLookAt(lookTarget, flightRotationSpeed);
        }

        private void FollowGrounded(Vector3 offset, float smoothTime, float rotSpeed)
        {
            Vector3 targetPos = ballTransform.position + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPos, ref currentVelocity, smoothTime);

            SmoothLookAt(ballTransform.position, rotSpeed);
        }

        private void SmoothLookAt(Vector3 target, float speed)
        {
            Vector3 dir = target - transform.position;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.deltaTime * speed);
        }

        public void SetBallTransform(Transform ball)
        {
            ballTransform = ball;
            golfBall = ball != null ? ball.GetComponent<GolfBall>() : null;
            flightTime = 0f;
            launchDirection = Vector3.forward;
        }

        public void SnapToTarget()
        {
            if (ballTransform == null) return;

            Vector3 offset = groundedOffset;
            if (golfBall != null && golfBall.State == BallState.OnGreen)
                offset = greenOffset;

            transform.position = ballTransform.position + offset;
            transform.LookAt(ballTransform.position);
            currentVelocity = Vector3.zero;
            flightTime = 0f;
        }
    }
}
