using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Golf
{
    public class ClubInventory : MonoBehaviour
    {
        [SerializeField] private List<ClubData> allClubs;

        private Dictionary<string, int> ownedClubs = new(); // clubId -> upgradeLevel

        public IReadOnlyList<ClubData> AllClubs => allClubs;

        public void Initialize(ClubSaveData saveData)
        {
            ownedClubs.Clear();
            if (saveData?.ownedClubs != null)
            {
                foreach (var club in saveData.ownedClubs)
                {
                    ownedClubs[club.clubId] = club.upgradeLevel;
                }
            }
        }

        public bool OwnsClub(string clubId) => ownedClubs.ContainsKey(clubId);

        public int GetUpgradeLevel(string clubId)
        {
            return ownedClubs.TryGetValue(clubId, out int level) ? level : 0;
        }

        public List<ClubData> GetOwnedClubData()
        {
            return allClubs.Where(c => ownedClubs.ContainsKey(c.clubId)).ToList();
        }

        public bool TryUnlockClub(string clubId)
        {
            if (ownedClubs.ContainsKey(clubId)) return false;

            ClubData data = allClubs.Find(c => c.clubId == clubId);
            if (data == null) return false;

            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency == null || !currency.TrySpend(data.purchaseCost)) return false;

            ownedClubs[clubId] = 0;
            EventBus.Publish(new ClubUnlockedEvent { clubName = data.clubName });
            return true;
        }

        public bool TryUpgradeClub(string clubId)
        {
            if (!ownedClubs.ContainsKey(clubId)) return false;

            int currentLevel = ownedClubs[clubId];
            if (currentLevel >= GameConstants.MaxClubUpgradeLevel) return false;

            ClubData data = allClubs.Find(c => c.clubId == clubId);
            if (data == null) return false;

            long cost = data.GetUpgradeCost(currentLevel);
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency == null || !currency.TrySpend(cost)) return false;

            ownedClubs[clubId] = currentLevel + 1;
            return true;
        }

        public ClubSaveData GetSaveData()
        {
            var data = new ClubSaveData
            {
                ownedClubs = new List<OwnedClub>()
            };
            foreach (var kvp in ownedClubs)
            {
                data.ownedClubs.Add(new OwnedClub
                {
                    clubId = kvp.Key,
                    upgradeLevel = kvp.Value
                });
            }
            return data;
        }
    }
}
