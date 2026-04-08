using UnityEngine;

namespace GolfGame.Golf.ShotMechanic
{
    [RequireComponent(typeof(LineRenderer))]
    public class ShotArc : MonoBehaviour
    {
        [SerializeField] private int previewSteps = 50;
        [SerializeField] private float previewTimeStep = 0.05f;
        [SerializeField] private Color arcColor = new Color(1f, 0.9f, 0.2f, 0.9f);
        [SerializeField] private float arcWidth = 0.3f;

        private LineRenderer lineRenderer;

        private GameObject landingZone;
        private Renderer landingZoneRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = arcWidth;
            lineRenderer.endWidth = arcWidth * 0.5f;
            lineRenderer.startColor = arcColor;
            lineRenderer.endColor = new Color(arcColor.r, arcColor.g, arcColor.b, 0.3f);
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;

            // Create landing zone indicator
            landingZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            landingZone.name = "LandingZone";
            landingZone.transform.SetParent(transform);
            landingZone.transform.localScale = new Vector3(2f, 0.01f, 2f);
            // Remove collider so it doesn't interfere with gameplay
            var col = landingZone.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            // Semi-transparent yellow material
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0.9f, 0.2f, 0.35f);
            landingZoneRenderer = landingZone.GetComponent<Renderer>();
            landingZoneRenderer.material = mat;
            landingZone.SetActive(false);
        }

        public void ShowPreview(Vector3 startPos, Vector3 velocity)
        {
            Vector3[] points = ShotResolver.PredictTrajectory(startPos, velocity, previewSteps, previewTimeStep);

            // Flatten arc to ground plane for top-down view
            for (int i = 0; i < points.Length; i++)
                points[i].y = 0.5f;

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            // Position landing zone at the last trajectory point
            if (points.Length > 0 && landingZone != null)
            {
                landingZone.SetActive(true);
                Vector3 landingPoint = points[points.Length - 1];
                landingZone.transform.position = new Vector3(landingPoint.x, 0.5f, landingPoint.z);
            }
        }

        public void ShowPreviewForClub(Vector3 startPos, Vector3 aimDirection, ClubData club, int upgradeLevel)
        {
            // Show max distance arc at full power, perfect accuracy
            var input = new ShotInput
            {
                power = 1f,
                deviationAngle = 0f,
                club = club,
                clubUpgradeLevel = upgradeLevel,
                ballPosition = startPos,
                aimDirection = aimDirection,
                terrainModifier = 1f
            };

            var result = ShotResolver.Calculate(input);
            ShowPreview(startPos, result.launchVelocity);
        }

        public void Hide()
        {
            lineRenderer.positionCount = 0;
            if (landingZone != null)
                landingZone.SetActive(false);
        }
    }
}
