using UnityEngine;

namespace GolfGame.Utility
{
    public static class MathHelpers
    {
        /// <summary>
        /// Calculate the distance a projectile will travel given launch speed and angle.
        /// </summary>
        public static float ProjectileRange(float launchSpeed, float angleRadians, float gravity)
        {
            return (launchSpeed * launchSpeed * Mathf.Sin(2f * angleRadians)) / gravity;
        }

        /// <summary>
        /// Calculate time of flight for a projectile.
        /// </summary>
        public static float ProjectileFlightTime(float launchSpeed, float angleRadians, float gravity)
        {
            return (2f * launchSpeed * Mathf.Sin(angleRadians)) / gravity;
        }

        /// <summary>
        /// Calculate max height of a projectile.
        /// </summary>
        public static float ProjectileMaxHeight(float launchSpeed, float angleRadians, float gravity)
        {
            float vy = launchSpeed * Mathf.Sin(angleRadians);
            return (vy * vy) / (2f * gravity);
        }

        /// <summary>
        /// Distance between two points ignoring Y axis (horizontal distance).
        /// </summary>
        public static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = b.x - a.x;
            float dz = b.z - a.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Remap a value from one range to another.
        /// </summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Smooth step with configurable edge values.
        /// </summary>
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Yards to Unity units (1:1 mapping for simplicity).
        /// </summary>
        public static float YardsToUnits(float yards) => yards;
        public static float UnitsToYards(float units) => units;
    }
}
