using UnityEngine;
using TMPro;
using GolfGame.Core;
using GolfGame.Economy;

namespace GolfGame.UI.Components
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private string format = "${0:N0}";
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationSpeed = 5f;

        private long displayedAmount;
        private long targetAmount;

        private void Start()
        {
            var currency = ServiceLocator.Get<CurrencyManager>();
            if (currency != null)
            {
                displayedAmount = currency.Amount;
                targetAmount = currency.Amount;
                currency.OnCurrencyChanged += OnCurrencyChanged;
                UpdateText();
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<CurrencyManager>(out var currency))
                currency.OnCurrencyChanged -= OnCurrencyChanged;
        }

        private void OnCurrencyChanged(long oldAmount, long newAmount)
        {
            targetAmount = newAmount;
            if (!animateChanges)
            {
                displayedAmount = newAmount;
                UpdateText();
            }
        }

        private void Update()
        {
            if (!animateChanges || displayedAmount == targetAmount) return;

            long diff = targetAmount - displayedAmount;
            long step = (long)(Mathf.Abs(diff) * animationSpeed * Time.deltaTime);
            step = Mathf.Max(1, (int)step);

            if (diff > 0)
                displayedAmount = System.Math.Min(displayedAmount + step, targetAmount);
            else
                displayedAmount = System.Math.Max(displayedAmount - step, targetAmount);

            UpdateText();
        }

        private void UpdateText()
        {
            if (amountText != null)
                amountText.text = string.Format(format, displayedAmount);
        }
    }
}
