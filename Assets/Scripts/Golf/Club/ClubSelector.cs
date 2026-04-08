using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Golf
{
    public class ClubSelector : MonoBehaviour
    {
        [SerializeField] private ClubInventory inventory;

        public ClubData SelectedClub { get; private set; }
        public int SelectedClubUpgradeLevel { get; private set; }

        private List<ClubData> availableClubs;
        private int currentIndex;

        public event System.Action<ClubData, int> OnClubChanged;

        public void RefreshAvailableClubs()
        {
            if (inventory == null) return;
            availableClubs = inventory.GetOwnedClubData();
            currentIndex = 0;
            if (availableClubs.Count > 0)
            {
                SelectClub(0);
            }
        }

        public void NextClub()
        {
            if (availableClubs == null || availableClubs.Count == 0) return;
            currentIndex = (currentIndex + 1) % availableClubs.Count;
            SelectClub(currentIndex);
        }

        public void PreviousClub()
        {
            if (availableClubs == null || availableClubs.Count == 0) return;
            currentIndex = (currentIndex - 1 + availableClubs.Count) % availableClubs.Count;
            SelectClub(currentIndex);
        }

        public void AutoSelectClub(float distanceToPin)
        {
            if (availableClubs == null || availableClubs.Count == 0) return;

            // Find club whose max distance is closest to but >= distance to pin
            ClubData best = availableClubs[0];
            float bestDiff = float.MaxValue;

            foreach (var club in availableClubs)
            {
                float clubDist = club.GetDistance(inventory.GetUpgradeLevel(club.clubId));
                float diff = clubDist - distanceToPin;

                if (diff >= 0 && diff < bestDiff)
                {
                    bestDiff = diff;
                    best = club;
                }
            }

            int idx = availableClubs.IndexOf(best);
            if (idx >= 0)
            {
                currentIndex = idx;
                SelectClub(currentIndex);
            }
        }

        private void SelectClub(int index)
        {
            SelectedClub = availableClubs[index];
            SelectedClubUpgradeLevel = inventory.GetUpgradeLevel(SelectedClub.clubId);
            OnClubChanged?.Invoke(SelectedClub, SelectedClubUpgradeLevel);
        }
    }
}
