using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Core;
using GolfGame.Economy;
using GolfGame.Progression;

namespace GolfGame.UI.Screens
{
    public class MainMenuScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button visitButton;
        [SerializeField] private Button drivingRangeButton;
        [SerializeField] private Button shopButton;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private TextMeshProUGUI reputationText;
        [SerializeField] private TextMeshProUGUI courseNameText;
        [SerializeField] private TextMeshProUGUI courseTierText;
        [SerializeField] private TextMeshProUGUI incomeRateText;
        [SerializeField] private TextMeshProUGUI titleText;

        // Currency lerp animation
        private float displayedCurrency;
        private float targetCurrency;
        private const float CurrencyLerpSpeed = 8f;

        // Play button pulse
        private TextMeshProUGUI playButtonLabel;
        private float pulseTimer;
        private const float PulseSpeed = 2.5f;
        private const float PulseMinAlpha = 0.75f;

        private void Awake()
        {
            playButton?.onClick.AddListener(OnPlayClicked);
            buildButton?.onClick.AddListener(OnBuildClicked);
            visitButton?.onClick.AddListener(OnVisitClicked);
            drivingRangeButton?.onClick.AddListener(OnDrivingRangeClicked);
            shopButton?.onClick.AddListener(OnShopClicked);

            // Cache play button label for pulsing
            if (playButton != null)
                playButtonLabel = playButton.GetComponentInChildren<TextMeshProUGUI>();

            // Initialize displayed currency so it doesn't lerp from zero on first show
            var currency = ServiceLocator.Get<CurrencyManager>();
            if (currency != null)
            {
                displayedCurrency = currency.Amount;
                targetCurrency = currency.Amount;
            }
        }

        protected override void OnShow()
        {
            UpdateDisplay();
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            UpdateCurrencyDisplay();
            UpdatePlayButtonPulse();
        }

        private void UpdateDisplay()
        {
            // Snap currency on full refresh
            var currencyMgr = ServiceLocator.Get<CurrencyManager>();
            if (currencyMgr != null)
            {
                targetCurrency = currencyMgr.Amount;
                displayedCurrency = currencyMgr.Amount;
                if (currencyText != null)
                    currencyText.text = $"${displayedCurrency:N0}";
            }

            var property = ServiceLocator.Get<Building.PropertyManager>();
            if (property != null)
            {
                if (courseNameText != null)
                    courseNameText.text = property.CourseName;
                if (courseTierText != null)
                    courseTierText.text = $"{property.GetBuiltHoleCount()}/{property.MaxHoles} Holes";
            }

            var rep = ServiceLocator.Get<ReputationSystem>();
            if (rep != null)
            {
                if (reputationText != null)
                    reputationText.text = $"Rep: {rep.Reputation} (Lv.{rep.Level})";
                if (titleText != null)
                    titleText.text = rep.GetTitle();
            }

            var income = ServiceLocator.Get<IncomeCalculator>();
            if (income != null && incomeRateText != null)
            {
                float perMin = income.GetIncomePerSecond() * 60f;
                incomeRateText.text = $"Income: {perMin:F0}/min";
            }
        }

        private void UpdateCurrencyDisplay()
        {
            var currency = ServiceLocator.Get<CurrencyManager>();
            if (currency != null && currencyText != null)
            {
                targetCurrency = currency.Amount;

                // Lerp toward target for a smooth counting effect
                if (Mathf.Abs(displayedCurrency - targetCurrency) > 0.5f)
                {
                    displayedCurrency = Mathf.Lerp(displayedCurrency, targetCurrency, Time.deltaTime * CurrencyLerpSpeed);
                }
                else
                {
                    displayedCurrency = targetCurrency;
                }

                currencyText.text = $"${displayedCurrency:N0}";
            }
        }

        private void UpdatePlayButtonPulse()
        {
            if (playButtonLabel == null) return;

            pulseTimer += Time.deltaTime * PulseSpeed;
            // Oscillate alpha between PulseMinAlpha and 1.0
            float alpha = Mathf.Lerp(PulseMinAlpha, 1f, (Mathf.Sin(pulseTimer) + 1f) * 0.5f);
            var c = playButtonLabel.color;
            playButtonLabel.color = new Color(c.r, c.g, c.b, alpha);
        }

        private void OnPlayClicked()
        {
            GameManager.Instance?.ChangeState(GameState.Playing);
        }

        private void OnBuildClicked()
        {
            GameManager.Instance?.ChangeState(GameState.Building);
        }

        private void OnVisitClicked()
        {
            GameManager.Instance?.ChangeState(GameState.Visiting);
        }

        private void OnDrivingRangeClicked()
        {
            GameManager.Instance?.ChangeState(GameState.DrivingRange);
        }

        private void OnShopClicked()
        {
            UIManager.Instance?.ShowScreen<ShopScreen>();
        }
    }
}
