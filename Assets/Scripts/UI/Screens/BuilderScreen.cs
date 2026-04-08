using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Core;
using GolfGame.Building;
using GolfGame.Data;

namespace GolfGame.UI.Screens
{
    public class BuilderScreen : UIScreen
    {
        [Header("Property Info")]
        [SerializeField] private TextMeshProUGUI courseNameText;
        [SerializeField] private TextMeshProUGUI tierText;
        [SerializeField] private TextMeshProUGUI holeCountText;
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Actions")]
        [SerializeField] private Button buildHoleButton;
        [SerializeField] private Button expandButton;
        [SerializeField] private Button upgradeDrivingRangeButton;
        [SerializeField] private Button backButton;

        [Header("Build Hole Panel")]
        [SerializeField] private GameObject buildHolePanel;
        [SerializeField] private Button[] parButtons; // 3, 4, 5
        [SerializeField] private Button[] hazardButtons;
        [SerializeField] private Button confirmBuildButton;
        [SerializeField] private Button cancelBuildButton;
        [SerializeField] private TextMeshProUGUI buildCostText;
        [SerializeField] private TextMeshProUGUI buildTimeText;

        [Header("Expansion Info")]
        [SerializeField] private TextMeshProUGUI expansionCostText;
        [SerializeField] private TextMeshProUGUI expansionStatusText;

        [Header("Driving Range")]
        [SerializeField] private TextMeshProUGUI drivingRangeLevelText;
        [SerializeField] private TextMeshProUGUI drivingRangeIncomeText;

        private HoleDesigner designer;

        private void Awake()
        {
            designer = gameObject.AddComponent<HoleDesigner>();

            buildHoleButton?.onClick.AddListener(ShowBuildPanel);
            expandButton?.onClick.AddListener(OnExpandClicked);
            upgradeDrivingRangeButton?.onClick.AddListener(OnUpgradeDrivingRange);
            backButton?.onClick.AddListener(OnBackClicked);
            confirmBuildButton?.onClick.AddListener(OnConfirmBuild);
            cancelBuildButton?.onClick.AddListener(HideBuildPanel);

            // Par buttons
            if (parButtons != null)
            {
                for (int i = 0; i < parButtons.Length; i++)
                {
                    int par = i + 3;
                    parButtons[i]?.onClick.AddListener(() => designer.SetPar(par));
                }
            }

            if (buildHolePanel != null)
                buildHolePanel.SetActive(false);
        }

        protected override void OnShow()
        {
            UpdateDisplay();

            var property = ServiceLocator.Get<PropertyManager>();
            if (property != null)
                property.OnPropertyChanged += UpdateDisplay;
        }

        protected override void OnHide()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property != null)
                property.OnPropertyChanged -= UpdateDisplay;
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            UpdateCurrency();
        }

        private void UpdateDisplay()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property == null) return;

            if (courseNameText != null) courseNameText.text = property.CourseName;
            if (tierText != null) tierText.text = $"Tier: {property.CurrentTier}-Hole Course";
            if (holeCountText != null) holeCountText.text = $"Built: {property.GetBuiltHoleCount()}/{property.MaxHoles}";

            // Build button
            if (buildHoleButton != null)
                buildHoleButton.interactable = property.CanBuildNewHole();

            // Expansion
            UpdateExpansionDisplay(property);

            // Driving range
            UpdateDrivingRangeDisplay();

            UpdateCurrency();
        }

        private void UpdateCurrency()
        {
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency != null && currencyText != null)
                currencyText.text = $"${currency.Amount:N0}";
        }

        private void UpdateExpansionDisplay(PropertyManager property)
        {
            int nextTier = property.GetNextTier();

            if (property.IsExpanding)
            {
                if (expansionStatusText != null)
                    expansionStatusText.text = "Expansion in progress...";
                if (expandButton != null)
                    expandButton.interactable = false;
            }
            else if (nextTier < 0)
            {
                if (expansionStatusText != null)
                    expansionStatusText.text = "Maximum expansion reached!";
                if (expandButton != null)
                    expandButton.interactable = false;
            }
            else
            {
                if (expansionCostText != null)
                    expansionCostText.text = $"Expand to {nextTier} holes: ${property.GetExpansionCost():N0}";
                if (expandButton != null)
                    expandButton.interactable = property.CanExpand();
            }
        }

        private void UpdateDrivingRangeDisplay()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property?.DrivingRange == null) return;

            int level = property.DrivingRange.level;
            if (drivingRangeLevelText != null)
                drivingRangeLevelText.text = $"Driving Range Lv.{level}";

            float incomePerMin = GameConstants.DrivingRangeBaseIncome *
                Mathf.Pow(GameConstants.DrivingRangeUpgradeMultiplier, level - 1);
            if (drivingRangeIncomeText != null)
                drivingRangeIncomeText.text = $"${incomePerMin:F0}/min";
        }

        private void ShowBuildPanel()
        {
            designer.ResetDesign();
            if (buildHolePanel != null) buildHolePanel.SetActive(true);

            if (buildCostText != null) buildCostText.text = $"Cost: ${designer.EstimatedCost:N0}";
            if (buildTimeText != null)
            {
                int mins = (int)(GameConstants.HoleConstructionTime / 60);
                buildTimeText.text = $"Build Time: {mins}m";
            }
        }

        private void HideBuildPanel()
        {
            if (buildHolePanel != null) buildHolePanel.SetActive(false);
        }

        private void OnConfirmBuild()
        {
            if (designer.TryBuildHole())
            {
                HideBuildPanel();
                UpdateDisplay();
            }
        }

        private void OnExpandClicked()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property != null && property.TryStartExpansion())
            {
                UpdateDisplay();
            }
        }

        private void OnUpgradeDrivingRange()
        {
            // Simplified: directly upgrade from property manager data
            var property = ServiceLocator.Get<PropertyManager>();
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (property?.DrivingRange == null || currency == null) return;

            long cost = (long)(GameConstants.DrivingRangeUpgradeCost *
                Mathf.Pow(GameConstants.DrivingRangeUpgradeMultiplier, property.DrivingRange.level - 1));

            if (currency.TrySpend(cost))
            {
                property.DrivingRange.level++;
                UpdateDisplay();
            }
        }

        private void OnBackClicked()
        {
            GameManager.Instance?.ChangeState(GameState.MainMenu);
        }
    }
}
