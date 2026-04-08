using UnityEngine;
using GolfGame.Golf.Ball;

namespace GolfGame.Golf.Course
{
    public enum HazardEffect
    {
        Bunker,     // Ball gets stuck, reduced distance on next shot
        Water,      // 1 stroke penalty, return to last position
        OutOfBounds // 1 stroke penalty + distance
    }

    [RequireComponent(typeof(Collider))]
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] private HazardEffect effect = HazardEffect.Bunker;
        [SerializeField] private int strokePenalty;

        public HazardEffect Effect => effect;
        public int StrokePenalty => strokePenalty;

        private void Awake()
        {
            // Ensure collider is set as trigger for water/OB
            var col = GetComponent<Collider>();
            if (effect == HazardEffect.Water || effect == HazardEffect.OutOfBounds)
            {
                col.isTrigger = true;
            }
        }

        private void Reset()
        {
            // Set defaults based on tag
            if (gameObject.CompareTag("Water"))
            {
                effect = HazardEffect.Water;
                strokePenalty = 1;
            }
            else if (gameObject.CompareTag("OutOfBounds"))
            {
                effect = HazardEffect.OutOfBounds;
                strokePenalty = 1;
            }
            else
            {
                effect = HazardEffect.Bunker;
                strokePenalty = 0;
            }
        }
    }
}
