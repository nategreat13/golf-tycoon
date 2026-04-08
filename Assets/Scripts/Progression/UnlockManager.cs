using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Progression
{
    public struct UnlockRequirement
    {
        public string id;
        public string displayName;
        public string description;
        public int requiredReputation;
        public int requiredLevel;
    }

    public class UnlockManager : MonoBehaviour
    {
        private static readonly List<UnlockRequirement> unlocks = new()
        {
            // Club unlocks
            new UnlockRequirement { id = "club_driver", displayName = "Driver", description = "The big stick", requiredReputation = 100, requiredLevel = 2 },
            new UnlockRequirement { id = "club_3wood", displayName = "3-Wood", description = "Versatile fairway wood", requiredReputation = 300, requiredLevel = 4 },
            new UnlockRequirement { id = "club_5iron", displayName = "5-Iron", description = "Mid-range precision", requiredReputation = 200, requiredLevel = 3 },
            new UnlockRequirement { id = "club_pw", displayName = "Pitching Wedge", description = "Approach specialist", requiredReputation = 150, requiredLevel = 2 },
            new UnlockRequirement { id = "club_sw", displayName = "Sand Wedge", description = "Bunker escape artist", requiredReputation = 250, requiredLevel = 3 },

            // Feature unlocks
            new UnlockRequirement { id = "hazard_water", displayName = "Water Hazards", description = "Add water to hole designs", requiredReputation = 200, requiredLevel = 3 },
            new UnlockRequirement { id = "hazard_bunker_water", displayName = "Complex Hazards", description = "Combine bunkers and water", requiredReputation = 500, requiredLevel = 5 },
            new UnlockRequirement { id = "theme_desert", displayName = "Desert Theme", description = "Unlock desert visual theme", requiredReputation = 400, requiredLevel = 4 },
            new UnlockRequirement { id = "theme_tropical", displayName = "Tropical Theme", description = "Unlock tropical visual theme", requiredReputation = 600, requiredLevel = 6 },
            new UnlockRequirement { id = "theme_links", displayName = "Links Theme", description = "Unlock classic links theme", requiredReputation = 1000, requiredLevel = 8 },

            // Expansion unlocks
            new UnlockRequirement { id = "expand_3", displayName = "3-Hole Course", description = "Expand to 3 holes", requiredReputation = GameConstants.Rep3HolesUnlock, requiredLevel = 5 },
            new UnlockRequirement { id = "expand_9", displayName = "9-Hole Course", description = "Expand to 9 holes", requiredReputation = GameConstants.Rep9HolesUnlock, requiredLevel = 10 },
            new UnlockRequirement { id = "expand_18", displayName = "18-Hole Course", description = "Expand to 18 holes", requiredReputation = GameConstants.Rep18HolesUnlock, requiredLevel = 15 },
        };

        public bool IsUnlocked(string unlockId)
        {
            var rep = ServiceLocator.Get<ReputationSystem>();
            if (rep == null) return false;

            var req = unlocks.Find(u => u.id == unlockId);
            if (string.IsNullOrEmpty(req.id)) return false;

            return rep.Reputation >= req.requiredReputation;
        }

        public UnlockRequirement? GetRequirement(string unlockId)
        {
            var found = unlocks.Find(u => u.id == unlockId);
            if (string.IsNullOrEmpty(found.id)) return null;
            return found;
        }

        public List<UnlockRequirement> GetAllUnlocks() => new List<UnlockRequirement>(unlocks);

        public List<UnlockRequirement> GetAvailableUnlocks()
        {
            var rep = ServiceLocator.Get<ReputationSystem>();
            int currentRep = rep?.Reputation ?? 0;

            return unlocks.FindAll(u => currentRep >= u.requiredReputation);
        }

        public List<UnlockRequirement> GetNextUnlocks(int count = 3)
        {
            var rep = ServiceLocator.Get<ReputationSystem>();
            int currentRep = rep?.Reputation ?? 0;

            var locked = unlocks.FindAll(u => currentRep < u.requiredReputation);
            locked.Sort((a, b) => a.requiredReputation.CompareTo(b.requiredReputation));

            return locked.GetRange(0, Mathf.Min(count, locked.Count));
        }
    }
}
