using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Economy
{
    /// <summary>
    /// Manages green fee pricing for the player's course.
    /// Higher fees = more money per visit but fewer AI golfers come.
    /// </summary>
    public class PricingManager : MonoBehaviour
    {
        [SerializeField] private float minFee = 5f;
        [SerializeField] private float maxFee = 100f;

        public float CurrentGreenFee { get; private set; } = 10f;

        /// <summary>
        /// AI golfer visit frequency multiplier based on pricing.
        /// Lower fee = more visits, higher fee = fewer visits.
        /// At min fee: 1.5x visits. At max fee: 0.3x visits.
        /// </summary>
        public float GetVisitFrequencyMultiplier()
        {
            float t = Mathf.InverseLerp(minFee, maxFee, CurrentGreenFee);
            return Mathf.Lerp(1.5f, 0.3f, t);
        }

        /// <summary>
        /// Income per AI visit = green fee * quality
        /// </summary>
        public float GetIncomePerVisit()
        {
            var income = ServiceLocator.Get<IncomeCalculator>();
            float quality = income?.GetCourseQualityMultiplier() ?? 1f;
            return CurrentGreenFee * quality;
        }

        public void SetGreenFee(float fee)
        {
            CurrentGreenFee = Mathf.Clamp(fee, minFee, maxFee);
        }

        /// <summary>
        /// Suggested optimal fee based on course quality and reputation.
        /// </summary>
        public float GetSuggestedFee()
        {
            var income = ServiceLocator.Get<IncomeCalculator>();
            if (income == null) return minFee;

            float quality = income.GetCourseQualityMultiplier();
            float repMult = income.GetReputationMultiplier();
            return Mathf.Clamp(minFee * quality * repMult, minFee, maxFee);
        }
    }
}
