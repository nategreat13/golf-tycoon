using UnityEngine;
using GolfGame.Core;
using GolfGame.Golf.Ball;
using GolfGame.Golf.Course;
using GolfGame.Golf.Scoring;
using GolfGame.Golf.ShotMechanic;
using GolfGame.Golf.Camera;
using GolfGame.UI.Screens;

namespace GolfGame.Golf
{
    /// <summary>
    /// Top-level orchestrator for the golf gameplay scene.
    /// Wires together the shot controller, course loader, scorecard, camera, and UI.
    /// </summary>
    public class GolfGameplayManager : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GolfBall golfBall;
        [SerializeField] private ShotController shotController;
        [SerializeField] private CourseLoader courseLoader;
        [SerializeField] private ScorecardManager scorecard;
        [SerializeField] private GolfCameraController cameraController;
        [SerializeField] private ClubSelector clubSelector;
        [SerializeField] private ClubInventory clubInventory;

        [Header("UI")]
        [SerializeField] private HUDScreen hudScreen;
        [SerializeField] private ScorecardScreen scorecardScreen;

        [Header("Default Course (for testing)")]
        [SerializeField] private CourseData defaultCourse;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Initialize club inventory from save
            var save = ServiceLocator.Get<SaveSystem>();
            if (save?.Data?.clubData != null && clubInventory != null)
            {
                clubInventory.Initialize(save.Data.clubData);
            }

            // Setup club selector
            if (clubSelector != null)
            {
                clubSelector.RefreshAvailableClubs();
                clubSelector.OnClubChanged += OnClubChanged;
            }

            // Subscribe to events
            EventBus.Subscribe<HoleCompletedEvent>(OnHoleCompleted);
            EventBus.Subscribe<ShotStartedEvent>(OnShotStarted);
            EventBus.Subscribe<BallHoledEvent>(OnBallHoled);

            // Load course
            LoadCourse();
        }

        private void LoadCourse()
        {
            if (courseLoader == null || defaultCourse == null)
            {
                Debug.LogWarning("No course to load. Using test setup.");
                StartTestHole();
                return;
            }

            courseLoader.OnHoleLoaded += OnHoleLoaded;
            courseLoader.LoadCourse(defaultCourse);
        }

        private void StartTestHole()
        {
            // Minimal test: place ball and start
            if (golfBall != null)
            {
                golfBall.PlaceOnTee(Vector3.zero);
            }

            scorecard?.StartRound();
            scorecard?.StartHole(3);

            if (shotController != null)
            {
                if (clubSelector?.SelectedClub != null)
                {
                    shotController.ActiveClub = clubSelector.SelectedClub;
                    shotController.ActiveClubUpgradeLevel = clubSelector.SelectedClubUpgradeLevel;
                }
                shotController.BeginShot();
            }

            hudScreen?.SetHoleInfo(1, 3, 150);
            hudScreen?.Show();
        }

        private void OnHoleLoaded(HoleInstance hole, int index)
        {
            if (golfBall != null)
            {
                golfBall.PlaceOnTee(hole.TeePosition);
            }

            // Point the camera at this hole's pin
            if (cameraController != null)
            {
                cameraController.SetPinTarget(hole.PinTransform);
                cameraController.SnapToState(CameraState.Tee);
            }

            if (index == 0)
                scorecard?.StartRound();

            scorecard?.StartHole(hole.Data.par);

            // Auto-select club based on distance
            clubSelector?.AutoSelectClub(hole.DistanceTeeToPin);

            if (shotController != null)
            {
                shotController.ActiveClub = clubSelector?.SelectedClub;
                shotController.ActiveClubUpgradeLevel = clubSelector?.SelectedClubUpgradeLevel ?? 0;
                shotController.SetTerrainModifier(golfBall?.GetTerrainModifier() ?? 1f);
                shotController.BeginShot();
            }

            hudScreen?.SetHoleInfo(index + 1, hole.Data.par, hole.Data.yardage);
            hudScreen?.Show();
        }

        private void OnClubChanged(ClubData club, int upgradeLevel)
        {
            if (shotController != null)
            {
                shotController.ActiveClub = club;
                shotController.ActiveClubUpgradeLevel = upgradeLevel;
            }
        }

        private void OnShotStarted(ShotStartedEvent evt)
        {
            scorecard?.AddStroke();
        }

        private void OnBallHoled(BallHoledEvent evt)
        {
            scorecard?.CompleteHole();
        }

        private void OnHoleCompleted(HoleCompletedEvent evt)
        {
            Debug.Log($"Hole {evt.holeIndex + 1}: {evt.strokes} strokes (par {evt.par})");

            // Check if round is over
            if (courseLoader != null && courseLoader.IsLastHole)
            {
                EndRound();
            }
            else if (courseLoader != null)
            {
                // Advance to next hole after a delay
                Invoke(nameof(NextHole), 2f);
            }
            else
            {
                // Single hole test mode
                EndRound();
            }
        }

        private void NextHole()
        {
            courseLoader?.AdvanceToNextHole();
        }

        private void EndRound()
        {
            var roundResult = scorecard.CompleteRound();
            hudScreen?.Hide();
            scorecardScreen?.ShowResult(roundResult);

            Debug.Log($"Round complete: {roundResult.GetSummaryString()}");

            // Save
            var save = ServiceLocator.Get<SaveSystem>();
            save?.GatherSaveData();
            save?.Save();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<HoleCompletedEvent>(OnHoleCompleted);
            EventBus.Unsubscribe<ShotStartedEvent>(OnShotStarted);
            EventBus.Unsubscribe<BallHoledEvent>(OnBallHoled);

            if (clubSelector != null)
                clubSelector.OnClubChanged -= OnClubChanged;
        }
    }
}
