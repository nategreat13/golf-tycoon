using UnityEngine;

namespace GolfGame.Golf
{
    [CreateAssetMenu(fileName = "NewClub", menuName = "Golf/Club Data")]
    public class ClubData : ScriptableObject
    {
        public string clubId;
        public string clubName;
        public ClubType clubType;

        [Header("Base Stats")]
        [Tooltip("Maximum distance in yards at full power")]
        public float maxDistance = 150f;

        [Tooltip("Launch angle in degrees")]
        public float loftAngle = 30f;

        [Tooltip("How forgiving the accuracy bar is (1 = normal, higher = more forgiving)")]
        public float accuracyModifier = 1f;

        [Tooltip("How much spin control the player has (0-1)")]
        public float spinControl = 0f;

        [Header("Upgrade Bonuses Per Level")]
        public float distancePerUpgrade = 10f;
        public float accuracyPerUpgrade = 0.1f;
        public float spinPerUpgrade = 0.05f;

        [Header("Visual")]
        public Sprite icon;

        [Header("Economy")]
        public long purchaseCost;
        public long upgradeCostBase = 200;

        public float GetDistance(int upgradeLevel)
        {
            return maxDistance + distancePerUpgrade * upgradeLevel;
        }

        public float GetAccuracy(int upgradeLevel)
        {
            return accuracyModifier + accuracyPerUpgrade * upgradeLevel;
        }

        public float GetSpinControl(int upgradeLevel)
        {
            return Mathf.Clamp01(spinControl + spinPerUpgrade * upgradeLevel);
        }

        public long GetUpgradeCost(int currentLevel)
        {
            return (long)(upgradeCostBase * Mathf.Pow(1.5f, currentLevel));
        }
    }

    public enum ClubType
    {
        Driver,
        Wood,
        Iron,
        Wedge,
        Putter
    }
}
