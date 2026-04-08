using UnityEngine;
using GolfGame.Core;
using GolfGame.Golf.Ball;

namespace GolfGame.Golf.ShotMechanic
{
    public enum ShotState
    {
        Idle,
        Aiming,
        PowerSetting,
        AccuracySetting,
        Launching,
        Flying,
        Landed,
        HoleComplete
    }

    public class ShotController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerBar powerBar;
        [SerializeField] private AccuracyBar accuracyBar;
        [SerializeField] private ShotArc shotArc;
        [SerializeField] private GolfBall golfBall;

        [Header("Aiming")]
        [SerializeField] private float aimRotateSpeed = 60f;

        public ShotState CurrentState { get; private set; } = ShotState.Idle;
        public ClubData ActiveClub { get; set; }
        public int ActiveClubUpgradeLevel { get; set; }

        private Vector3 aimDirection = Vector3.forward;
        private Vector3 shotStartPosition;

        /// <summary>
        /// Current aim direction, readable by the camera system.
        /// </summary>
        public Vector3 AimDirection => aimDirection;
        private float lockedPower;
        private float lockedDeviation;
        private float terrainModifier = 1f;

        public void BeginShot()
        {
            if (CurrentState != ShotState.Idle && CurrentState != ShotState.Landed)
                return;

            CurrentState = ShotState.Aiming;
            aimDirection = Vector3.forward;

            if (shotArc != null && ActiveClub != null)
            {
                shotArc.ShowPreviewForClub(
                    golfBall.transform.position,
                    aimDirection,
                    ActiveClub,
                    ActiveClubUpgradeLevel
                );
            }
        }

        public void SetTerrainModifier(float modifier)
        {
            terrainModifier = modifier;
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case ShotState.Aiming:
                    HandleAiming();
                    if (InputTapped())
                        TransitionToPower();
                    break;

                case ShotState.PowerSetting:
                    if (InputTapped())
                        TransitionToAccuracy();
                    break;

                case ShotState.AccuracySetting:
                    if (InputTapped())
                        TransitionToLaunch();
                    break;

                case ShotState.Flying:
                    if (golfBall.IsStationary)
                        OnBallStopped();
                    break;
            }
        }

        private void HandleAiming()
        {
            float horizontal = 0f;

            // Touch: swipe left/right
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    horizontal = touch.deltaPosition.x * 0.1f;
                }
            }
            // Mouse/keyboard fallback
            else
            {
                horizontal = Input.GetAxis("Horizontal");
            }

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                aimDirection = Quaternion.AngleAxis(horizontal * aimRotateSpeed * Time.deltaTime, Vector3.up) * aimDirection;

                if (shotArc != null && ActiveClub != null)
                {
                    shotArc.ShowPreviewForClub(
                        golfBall.transform.position,
                        aimDirection,
                        ActiveClub,
                        ActiveClubUpgradeLevel
                    );
                }
            }
        }

        private void TransitionToPower()
        {
            CurrentState = ShotState.PowerSetting;
            // Keep arc visible during power setting — only hide after accuracy tap
            powerBar.Activate();
            EventBus.Publish(new ShotStartedEvent { strokeNumber = 0 });
        }

        private void TransitionToAccuracy()
        {
            lockedPower = powerBar.Lock();
            CurrentState = ShotState.AccuracySetting;
            shotArc?.Hide();

            float clubAccuracy = ActiveClub != null ? ActiveClub.GetAccuracy(ActiveClubUpgradeLevel) : 1f;
            accuracyBar.Activate(clubAccuracy);
        }

        private void TransitionToLaunch()
        {
            lockedDeviation = accuracyBar.Lock();
            CurrentState = ShotState.Launching;
            ExecuteShot();
        }

        private void ExecuteShot()
        {
            shotStartPosition = golfBall.transform.position;

            var input = new ShotInput
            {
                power = lockedPower,
                deviationAngle = lockedDeviation,
                club = ActiveClub,
                clubUpgradeLevel = ActiveClubUpgradeLevel,
                ballPosition = golfBall.transform.position,
                aimDirection = aimDirection,
                terrainModifier = terrainModifier
            };

            var result = ShotResolver.Calculate(input);
            golfBall.Launch(result.launchVelocity);

            CurrentState = ShotState.Flying;
            terrainModifier = 1f; // Reset for next shot
        }

        private void OnBallStopped()
        {
            if (golfBall.IsHoled)
            {
                CurrentState = ShotState.HoleComplete;
                EventBus.Publish(new BallHoledEvent());
                return;
            }

            CurrentState = ShotState.Landed;
            EventBus.Publish(new ShotCompletedEvent
            {
                distance = Vector3.Distance(shotStartPosition, golfBall.transform.position),
                strokeNumber = 0
            });

            // Auto-start next shot after a brief pause
            Invoke(nameof(BeginShot), 1f);
        }

        private bool InputTapped()
        {
            // Spacebar always works (not affected by UI)
            if (Input.GetKeyDown(KeyCode.Space))
                return true;

            // Ignore taps/clicks on UI elements (buttons, etc.)
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return false;

            // Touch
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;

            // Mouse/keyboard
            if (Input.GetMouseButtonDown(0))
                return true;

            return false;
        }

        public void Reset()
        {
            CurrentState = ShotState.Idle;
            powerBar?.Deactivate();
            accuracyBar?.Deactivate();
            shotArc?.Hide();
        }
    }
}
