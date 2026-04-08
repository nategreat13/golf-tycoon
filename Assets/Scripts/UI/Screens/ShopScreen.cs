using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Core;
using GolfGame.Golf;

namespace GolfGame.UI.Screens
{
    public class ShopScreen : UIScreen
    {
        [Header("Club List")]
        [SerializeField] private Transform clubListContainer;
        [SerializeField] private GameObject clubEntryPrefab;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        [Header("References")]
        [SerializeField] private ClubInventory clubInventory;

        private void Awake()
        {
            backButton?.onClick.AddListener(OnBack);
        }

        protected override void OnShow()
        {
            UpdateCurrency();
            PopulateClubList();
        }

        private void UpdateCurrency()
        {
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency != null && currencyText != null)
                currencyText.text = $"${currency.Amount:N0}";
        }

        private void PopulateClubList()
        {
            if (clubListContainer == null || clubEntryPrefab == null || clubInventory == null)
                return;

            foreach (Transform child in clubListContainer)
                Destroy(child.gameObject);

            foreach (var club in clubInventory.AllClubs)
            {
                var entry = Instantiate(clubEntryPrefab, clubListContainer);
                var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
                var buttons = entry.GetComponentsInChildren<Button>();

                bool owned = clubInventory.OwnsClub(club.clubId);
                int level = clubInventory.GetUpgradeLevel(club.clubId);

                if (texts.Length >= 2)
                {
                    texts[0].text = club.clubName;
                    if (owned)
                    {
                        var info = ClubUpgrade.GetUpgradeInfo(club, level,
                            ServiceLocator.Get<Economy.CurrencyManager>()?.Amount ?? 0);
                        texts[1].text = info.isMaxLevel
                            ? $"Lv.{level} (MAX) - {info.currentDistance:F0} yds"
                            : $"Lv.{level} - {info.currentDistance:F0} yds | Upgrade: ${info.upgradeCost:N0}";
                    }
                    else
                    {
                        texts[1].text = $"Locked - ${club.purchaseCost:N0}";
                    }
                }

                if (buttons.Length > 0)
                {
                    string clubId = club.clubId;
                    if (owned)
                    {
                        buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
                        buttons[0].interactable = level < Data.GameConstants.MaxClubUpgradeLevel;
                        buttons[0].onClick.AddListener(() =>
                        {
                            clubInventory.TryUpgradeClub(clubId);
                            PopulateClubList();
                            UpdateCurrency();
                        });
                    }
                    else
                    {
                        buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Buy";
                        buttons[0].onClick.AddListener(() =>
                        {
                            clubInventory.TryUnlockClub(clubId);
                            PopulateClubList();
                            UpdateCurrency();
                        });
                    }
                }
            }
        }

        private void OnBack()
        {
            UIManager.Instance?.GoBack();
        }
    }
}
