using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Core;
using GolfGame.Golf.Scoring;

namespace GolfGame.UI.Screens
{
    public class ScorecardScreen : UIScreen
    {
        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI relativeScoreText;
        [SerializeField] private TextMeshProUGUI summaryText;

        [Header("Per-Hole Display")]
        [SerializeField] private Transform holeScoreContainer;
        [SerializeField] private GameObject holeScoreEntryPrefab;

        [Header("Rewards")]
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private TextMeshProUGUI reputationGainText;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        private RoundResult currentResult;

        private void Awake()
        {
            playAgainButton?.onClick.AddListener(OnPlayAgain);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
        }

        public void ShowResult(RoundResult result)
        {
            currentResult = result;
            Show();

            if (totalScoreText != null)
                totalScoreText.text = result.totalStrokes.ToString();

            if (relativeScoreText != null)
                relativeScoreText.text = ScoreCalculator.GetRelativeString(result.RelativeToPar);

            if (summaryText != null)
                summaryText.text = result.GetSummaryString();

            PopulateHoleScores(result);
            ShowRewards(result);
        }

        private void PopulateHoleScores(RoundResult result)
        {
            if (holeScoreContainer == null || holeScoreEntryPrefab == null) return;

            // Clear existing
            foreach (Transform child in holeScoreContainer)
                Destroy(child.gameObject);

            var holeResults = result.GetHoleResults();
            for (int i = 0; i < holeResults.Count; i++)
            {
                var entry = Instantiate(holeScoreEntryPrefab, holeScoreContainer);
                var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{i + 1}";
                    texts[1].text = $"Par {holeResults[i].par}";
                    texts[2].text = $"{holeResults[i].strokes} ({ScoreCalculator.GetLabelString(holeResults[i].label)})";
                }
            }
        }

        private void ShowRewards(RoundResult result)
        {
            // Calculate rewards
            int repGain = GolfGame.Data.GameConstants.ReputationPerRoundPlayed;

            if (reputationGainText != null)
                reputationGainText.text = $"+{repGain} Reputation";

            // Grant rewards
            var rep = ServiceLocator.Get<Progression.ReputationSystem>();
            rep?.AddReputation(repGain);
        }

        private void OnPlayAgain()
        {
            Hide();
            GameManager.Instance?.ChangeState(GameState.Playing);
        }

        private void OnMainMenu()
        {
            Hide();
            GameManager.Instance?.ChangeState(GameState.MainMenu);
        }
    }
}
