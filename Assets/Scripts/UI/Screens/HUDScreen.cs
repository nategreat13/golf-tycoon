using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Golf;
using GolfGame.Golf.ShotMechanic;
using GolfGame.Golf.Scoring;

namespace GolfGame.UI.Screens
{
    /// <summary>
    /// In-game HUD showing club info, stroke count, power/accuracy bars, and hole info.
    /// </summary>
    public class HUDScreen : UIScreen
    {
        [Header("Hole Info")]
        [SerializeField] private TextMeshProUGUI holeNumberText;
        [SerializeField] private TextMeshProUGUI parText;
        [SerializeField] private TextMeshProUGUI yardageText;
        [SerializeField] private TextMeshProUGUI strokeCountText;

        [Header("Club Info")]
        [SerializeField] private TextMeshProUGUI clubNameText;
        [SerializeField] private TextMeshProUGUI clubDistanceText;
        [SerializeField] private Button prevClubButton;
        [SerializeField] private Button nextClubButton;

        [Header("Shot Bars")]
        [SerializeField] private Components.PowerBarUI powerBarUI;
        [SerializeField] private Components.AccuracyBarUI accuracyBarUI;

        [Header("Distance")]
        [SerializeField] private TextMeshProUGUI distanceText;
        [SerializeField] private Transform pinTarget;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI scoreRelativeText;

        [Header("References")]
        [SerializeField] private ShotController shotController;
        [SerializeField] private ClubSelector clubSelector;
        [SerializeField] private ScorecardManager scorecard;
        [SerializeField] private Transform ballTransform;

        private void Awake()
        {
            prevClubButton?.onClick.AddListener(() => clubSelector?.PreviousClub());
            nextClubButton?.onClick.AddListener(() => clubSelector?.NextClub());

            if (clubSelector != null)
                clubSelector.OnClubChanged += OnClubChanged;
        }

        protected override void OnShow()
        {
            UpdateAll();
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            UpdateShotState();
            UpdateStrokeCount();
            UpdateDistance();
        }

        public void SetHoleInfo(int holeNumber, int par, float yardage)
        {
            if (holeNumberText != null) holeNumberText.text = $"Hole {holeNumber}";
            if (parText != null) parText.text = $"Par {par}";
            if (yardageText != null) yardageText.text = $"{yardage:F0} yds";
        }

        private void OnClubChanged(ClubData club, int upgradeLevel)
        {
            if (clubNameText != null) clubNameText.text = club.clubName;
            if (clubDistanceText != null) clubDistanceText.text = $"{club.GetDistance(upgradeLevel):F0} yds";
        }

        private void UpdateShotState()
        {
            if (shotController == null) return;

            bool showPower = shotController.CurrentState == ShotState.PowerSetting;
            bool showAccuracy = shotController.CurrentState == ShotState.AccuracySetting;

            if (statusText != null)
            {
                statusText.text = shotController.CurrentState switch
                {
                    ShotState.Aiming => "Tap to set power",
                    ShotState.PowerSetting => "TAP!",
                    ShotState.AccuracySetting => "TAP!",
                    ShotState.Flying => "...",
                    ShotState.Landed => "",
                    ShotState.HoleComplete => "IN THE HOLE!",
                    _ => ""
                };
            }
        }

        private void UpdateStrokeCount()
        {
            if (scorecard == null || strokeCountText == null) return;
            strokeCountText.text = $"Strokes: {scorecard.CurrentHoleStrokes}";

            if (scoreRelativeText != null)
            {
                int relative = scorecard.TotalStrokes - scorecard.TotalPar;
                scoreRelativeText.text = ScoreCalculator.GetRelativeString(relative);
            }
        }

        private void UpdateDistance()
        {
            if (distanceText == null || pinTarget == null || ballTransform == null) return;

            float distanceYards = Vector3.Distance(ballTransform.position, pinTarget.position);
            distanceText.text = $"{distanceYards:F0} yds to pin";
        }

        private void UpdateAll()
        {
            UpdateShotState();
            UpdateStrokeCount();
            UpdateDistance();
        }
    }
}
