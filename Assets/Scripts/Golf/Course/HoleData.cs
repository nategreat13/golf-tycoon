using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf
{
    [CreateAssetMenu(fileName = "NewHole", menuName = "Golf/Hole Data")]
    public class HoleData : ScriptableObject
    {
        public string holeName;
        public int par = 3;
        public HoleLength length = HoleLength.Short;
        public HoleHazardSet hazardSet = HoleHazardSet.None;

        [Header("Layout")]
        [Tooltip("Distance from tee to pin in yards")]
        public float yardage = 150f;

        [Tooltip("Fairway width multiplier (1 = normal)")]
        public float fairwayWidth = 1f;

        [Tooltip("Green size multiplier (1 = normal)")]
        public float greenSize = 1f;

        [Tooltip("Green slope strength (0 = flat, 1 = very sloped)")]
        [Range(0f, 1f)]
        public float greenSlope = 0.2f;

        [Tooltip("Direction of green slope")]
        public Vector2 greenSlopeDirection = Vector2.right;

        [Header("Hazard Positions (normalized 0-1 along hole length)")]
        public HazardPlacement[] hazards;

        [Header("Visual")]
        public int themeIndex;

        [Header("Elevation")]
        [Tooltip("Height difference from tee to green in yards")]
        public float elevationChange = 0f;

        [Tooltip("Dogleg angle (0 = straight, positive = right, negative = left)")]
        public float doglegAngle = 0f;

        [Tooltip("Where along the hole the dogleg occurs (0-1)")]
        [Range(0f, 1f)]
        public float doglegPosition = 0.5f;
    }

    [System.Serializable]
    public class HazardPlacement
    {
        public HazardType type;
        [Tooltip("Position along hole length (0 = tee, 1 = green)")]
        [Range(0f, 1f)]
        public float longitudinalPosition;
        [Tooltip("Lateral offset (-1 = far left, 0 = center, 1 = far right)")]
        [Range(-1f, 1f)]
        public float lateralOffset;
        [Tooltip("Size multiplier")]
        public float size = 1f;
    }

    public enum HazardType
    {
        Bunker,
        Water,
        Trees,
        OutOfBounds
    }
}
