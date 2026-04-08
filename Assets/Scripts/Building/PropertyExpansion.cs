using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Building
{
    /// <summary>
    /// Handles the course expansion UI logic (1→3→9→18 holes).
    /// </summary>
    public class PropertyExpansion : MonoBehaviour
    {
        public struct ExpansionInfo
        {
            public int currentTier;
            public int nextTier;
            public long cost;
            public float timeDuration;
            public int requiredReputation;
            public int currentReputation;
            public bool canAfford;
            public bool hasReputation;
            public bool canExpand;
            public bool isExpanding;
            public float remainingTime;
            public bool isMaxTier;
        }

        public ExpansionInfo GetExpansionInfo()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            var reputation = ServiceLocator.Get<Progression.ReputationSystem>();

            if (property == null)
                return new ExpansionInfo { isMaxTier = true };

            int nextTier = property.GetNextTier();
            bool isMax = nextTier < 0;

            int requiredRep = nextTier switch
            {
                3 => GameConstants.Rep3HolesUnlock,
                9 => GameConstants.Rep9HolesUnlock,
                18 => GameConstants.Rep18HolesUnlock,
                _ => int.MaxValue
            };

            long cost = isMax ? 0 : property.GetExpansionCost();
            float time = isMax ? 0 : property.GetExpansionTime();
            int currentRep = reputation?.Reputation ?? 0;
            long currentCurrency = currency?.Amount ?? 0;

            float remaining = 0;
            if (property.IsExpanding)
            {
                var timeManager = ServiceLocator.Get<TimeManager>();
                // remaining calculated from property state
            }

            return new ExpansionInfo
            {
                currentTier = property.CurrentTier,
                nextTier = nextTier,
                cost = cost,
                timeDuration = time,
                requiredReputation = requiredRep,
                currentReputation = currentRep,
                canAfford = currentCurrency >= cost,
                hasReputation = currentRep >= requiredRep,
                canExpand = property.CanExpand(),
                isExpanding = property.IsExpanding,
                remainingTime = remaining,
                isMaxTier = isMax
            };
        }

        public string GetTierDescription(int tier)
        {
            return tier switch
            {
                1 => "Starter Course - 1 Hole",
                3 => "Small Course - 3 Holes",
                9 => "Local Course - 9 Holes",
                18 => "Championship Course - 18 Holes",
                _ => "Unknown"
            };
        }

        public string GetExpansionFlavorText(int nextTier)
        {
            return nextTier switch
            {
                3 => "Time to build a real course! Expand to 3 holes and start attracting more golfers.",
                9 => "Your course is gaining reputation. Expand to a full 9-hole course and become a local favorite!",
                18 => "The ultimate expansion! Build a championship 18-hole course and cement your legacy.",
                _ => ""
            };
        }

        public bool TryStartExpansion()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            return property != null && property.TryStartExpansion();
        }
    }
}
