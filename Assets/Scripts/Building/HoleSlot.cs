using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Building
{
    /// <summary>
    /// Visual representation of a hole slot in the property builder view.
    /// </summary>
    public class HoleSlot : MonoBehaviour
    {
        [SerializeField] private GameObject emptyVisual;
        [SerializeField] private GameObject constructionVisual; // "Ground Under Repair" sign
        [SerializeField] private GameObject builtVisual;
        [SerializeField] private GameObject upgradingVisual;
        [SerializeField] private TMPro.TextMeshPro timerText;
        [SerializeField] private TMPro.TextMeshPro holeInfoText;

        public int SlotIndex { get; private set; }
        public HoleSaveData Data { get; private set; }

        public void Initialize(int index, HoleSaveData data)
        {
            SlotIndex = index;
            Data = data;
            UpdateVisuals();
        }

        public void UpdateData(HoleSaveData data)
        {
            Data = data;
            UpdateVisuals();
        }

        private void Update()
        {
            if (Data == null) return;

            // Update timer display for construction/upgrading
            if (Data.state == HoleSlotState.UnderConstruction || Data.state == HoleSlotState.Upgrading)
            {
                UpdateTimerDisplay();
            }
        }

        private void UpdateVisuals()
        {
            if (Data == null)
            {
                SetActiveVisual(emptyVisual);
                return;
            }

            switch (Data.state)
            {
                case HoleSlotState.Empty:
                    SetActiveVisual(emptyVisual);
                    break;
                case HoleSlotState.UnderConstruction:
                case HoleSlotState.Designing:
                    SetActiveVisual(constructionVisual);
                    break;
                case HoleSlotState.Built:
                    SetActiveVisual(builtVisual);
                    UpdateHoleInfo();
                    break;
                case HoleSlotState.Upgrading:
                    SetActiveVisual(upgradingVisual);
                    break;
            }
        }

        private void SetActiveVisual(GameObject active)
        {
            if (emptyVisual != null) emptyVisual.SetActive(active == emptyVisual);
            if (constructionVisual != null) constructionVisual.SetActive(active == constructionVisual);
            if (builtVisual != null) builtVisual.SetActive(active == builtVisual);
            if (upgradingVisual != null) upgradingVisual.SetActive(active == upgradingVisual);
        }

        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            var time = ServiceLocator.Get<TimeManager>();
            if (time == null) return;

            float duration = Data.state == HoleSlotState.UnderConstruction
                ? GameConstants.HoleConstructionTime
                : GameConstants.HoleUpgradeTime;

            float remaining = time.GetRemainingSeconds(Data.constructionStartTimeUtc, duration);

            if (remaining <= 0)
            {
                timerText.text = "Complete!";
            }
            else
            {
                int minutes = (int)(remaining / 60);
                int seconds = (int)(remaining % 60);
                timerText.text = $"{minutes}:{seconds:D2}";
            }
        }

        private void UpdateHoleInfo()
        {
            if (holeInfoText == null || Data == null) return;
            holeInfoText.text = $"Par {Data.par}\nLv.{Data.qualityLevel}\nPlayed: {Data.timesPlayed}x";
        }
    }
}
