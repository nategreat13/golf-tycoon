using UnityEngine;
using GolfGame.Golf.Ball;

namespace GolfGame.Golf.Course
{
    /// <summary>
    /// Manages the putting green surface. Applies slope to the ball when on the green.
    /// </summary>
    public class GreenPhysics : MonoBehaviour
    {
        [SerializeField] private Vector2 slopeDirection = Vector2.right;
        [SerializeField] [Range(0f, 1f)] private float slopeStrength = 0.2f;
        [SerializeField] private float greenSpeed = 10f; // Stimp meter reading

        public Vector2 SlopeDirection => slopeDirection;
        public float SlopeStrength => slopeStrength;
        public float GreenSpeed => greenSpeed;

        public void SetSlope(Vector2 direction, float strength)
        {
            slopeDirection = direction.normalized;
            slopeStrength = Mathf.Clamp01(strength);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("GolfBall")) return;

            var ballPhysics = other.GetComponent<BallPhysics>();
            if (ballPhysics != null)
            {
                ballPhysics.SetGreenSlope(slopeDirection, slopeStrength);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("GolfBall")) return;

            var ballPhysics = other.GetComponent<BallPhysics>();
            if (ballPhysics != null)
            {
                ballPhysics.ClearGreenSlope();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize slope direction in editor
            Gizmos.color = Color.blue;
            Vector3 slopeDir3D = new Vector3(slopeDirection.x, 0, slopeDirection.y);
            Gizmos.DrawRay(transform.position, slopeDir3D * slopeStrength * 5f);
        }
    }
}
