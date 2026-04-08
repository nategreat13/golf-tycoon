using UnityEngine;

namespace GolfGame.Golf.Ball
{
    /// <summary>
    /// Adds a TrailRenderer to the golf ball that activates during flight.
    /// Creates a white fading trail behind the ball for visual feedback.
    /// </summary>
    [RequireComponent(typeof(GolfBall))]
    public class BallTrail : MonoBehaviour
    {
        [SerializeField] private float trailTime = 0.5f;
        [SerializeField] private float startWidth = 0.08f;
        [SerializeField] private float endWidth = 0f;
        [SerializeField] private Color trailColor = Color.white;

        private TrailRenderer trail;
        private GolfBall golfBall;
        private BallState lastState;

        private void Awake()
        {
            golfBall = GetComponent<GolfBall>();

            trail = GetComponent<TrailRenderer>();
            if (trail == null)
                trail = gameObject.AddComponent<TrailRenderer>();

            ConfigureTrail();
            trail.emitting = false;
        }

        private void ConfigureTrail()
        {
            trail.time = trailTime;
            trail.startWidth = startWidth;
            trail.endWidth = endWidth;
            trail.numCornerVertices = 4;
            trail.numCapVertices = 4;
            trail.minVertexDistance = 0.1f;

            // Use unlit material so trail is always visible
            var mat = new Material(Shader.Find("Sprites/Default"));
            if (mat == null)
                mat = new Material(Shader.Find("UI/Default"));
            trail.material = mat;

            // White trail that fades from semi-transparent to fully transparent
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(trailColor, 0f),
                    new GradientColorKey(trailColor, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trail.colorGradient = gradient;

            // Render on top of terrain
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
        }

        private void Update()
        {
            BallState currentState = golfBall.State;

            if (currentState == BallState.InFlight && lastState != BallState.InFlight)
            {
                // Ball just launched - clear old trail and start emitting
                trail.Clear();
                trail.emitting = true;
            }
            else if (currentState != BallState.InFlight && lastState == BallState.InFlight)
            {
                // Ball just stopped flying - stop emitting (trail fades out naturally)
                trail.emitting = false;
            }

            lastState = currentState;
        }
    }
}
