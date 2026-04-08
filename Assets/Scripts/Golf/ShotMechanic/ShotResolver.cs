using UnityEngine;
using GolfGame.Utility;

namespace GolfGame.Golf.ShotMechanic
{
    public struct ShotInput
    {
        public float power;           // 0-1
        public float deviationAngle;  // degrees, negative = left
        public ClubData club;
        public int clubUpgradeLevel;
        public Vector3 ballPosition;
        public Vector3 aimDirection;   // forward direction on XZ plane
        public float terrainModifier;  // 1 = normal, 0.5 = bunker, etc.
    }

    public struct ShotResult
    {
        public Vector3 launchVelocity;
        public float estimatedDistance;
        public float deviationAngle;
    }

    public static class ShotResolver
    {
        public static ShotResult Calculate(ShotInput input)
        {
            float clubDistance = input.club.GetDistance(input.clubUpgradeLevel);
            float actualDistance = clubDistance * input.power * input.terrainModifier;
            float loftAngle = input.club.loftAngle;

            // Convert aim direction by applying deviation
            Vector3 aimDir = input.aimDirection.normalized;
            Quaternion deviation = Quaternion.AngleAxis(input.deviationAngle, Vector3.up);
            aimDir = deviation * aimDir;

            // Calculate launch velocity to achieve the desired distance
            // Using projectile motion: distance = v^2 * sin(2*theta) / g
            // Solve for v: v = sqrt(distance * g / sin(2*theta))
            float g = Physics.gravity.magnitude;
            float theta = loftAngle * Mathf.Deg2Rad;
            float sin2Theta = Mathf.Sin(2f * theta);

            // Yards to Unity units (1 yard ≈ 0.9144 meters, we use 1 unit = 1 yard for simplicity)
            float distanceUnits = actualDistance;

            float launchSpeed;
            if (sin2Theta > 0.01f)
            {
                launchSpeed = Mathf.Sqrt(distanceUnits * g / sin2Theta);
            }
            else
            {
                // Very low or very high loft — fallback
                launchSpeed = Mathf.Sqrt(distanceUnits * g);
            }

            // Compose velocity vector
            Vector3 horizontalDir = new Vector3(aimDir.x, 0, aimDir.z).normalized;
            float horizontalSpeed = launchSpeed * Mathf.Cos(theta);
            float verticalSpeed = launchSpeed * Mathf.Sin(theta);

            Vector3 velocity = horizontalDir * horizontalSpeed + Vector3.up * verticalSpeed;

            return new ShotResult
            {
                launchVelocity = velocity,
                estimatedDistance = actualDistance,
                deviationAngle = input.deviationAngle
            };
        }

        /// <summary>
        /// Preview trajectory for ShotArc line renderer.
        /// Returns an array of world-space positions along the predicted path.
        /// </summary>
        public static Vector3[] PredictTrajectory(Vector3 startPos, Vector3 velocity, int steps = 50, float timeStep = 0.05f)
        {
            Vector3[] points = new Vector3[steps];
            Vector3 pos = startPos;
            Vector3 vel = velocity;
            float g = Physics.gravity.magnitude;

            for (int i = 0; i < steps; i++)
            {
                points[i] = pos;
                vel.y -= g * timeStep;
                pos += vel * timeStep;

                // Stop if below ground
                if (pos.y < 0f)
                {
                    pos.y = 0f;
                    points[i] = pos;
                    // Fill remaining with last position
                    for (int j = i + 1; j < steps; j++)
                        points[j] = pos;
                    break;
                }
            }

            return points;
        }
    }
}
