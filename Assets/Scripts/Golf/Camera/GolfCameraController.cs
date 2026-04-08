using UnityEngine;
using GolfGame.Golf.Ball;
using GolfGame.Golf.ShotMechanic;

namespace GolfGame.Golf.Camera
{
    public enum CameraState
    {
        Tee,
        Flight,
        Approach,
        Putt,
        Celebration
    }

    public class GolfCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private GolfBall golfBall;
        [SerializeField] private ShotController shotController;
        [SerializeField] private Transform pinTarget;

        [Header("Top-Down Settings")]
        [SerializeField] private float cameraHeight = 50f;
        [SerializeField] private float positionSmoothTime = 0.2f;
        [SerializeField] private float orthoSizeLerpSpeed = 4f;

        [Header("Orthographic Sizes Per State")]
        [SerializeField] private float teeOrthoSize = 25f;
        [SerializeField] private float flightOrthoSize = 30f;
        [SerializeField] private float approachOrthoSize = 25f;
        [SerializeField] private float puttOrthoSize = 12f;
        [SerializeField] private float celebrationOrthoSize = 10f;

        public CameraState CurrentState { get; private set; } = CameraState.Tee;

        private Vector3 positionVelocity;
        private Transform cameraTransform;
        private float targetOrthoSize;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;

            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = teeOrthoSize;
                cameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }

            targetOrthoSize = teeOrthoSize;
        }

        private void LateUpdate()
        {
            if (golfBall == null || cameraTransform == null) return;

            UpdateCameraState();

            // Determine follow target position
            Vector3 followTarget;
            if (CurrentState == CameraState.Celebration && pinTarget != null)
            {
                followTarget = new Vector3(pinTarget.position.x, cameraHeight, pinTarget.position.z);
            }
            else
            {
                Vector3 ballPos = golfBall.transform.position;
                followTarget = new Vector3(ballPos.x, cameraHeight, ballPos.z);
            }

            // SmoothDamp toward target position
            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position, followTarget, ref positionVelocity, positionSmoothTime);

            // Keep rotation locked straight down
            cameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Lerp orthographic size toward target
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, targetOrthoSize, Time.deltaTime * orthoSizeLerpSpeed);
        }

        private void UpdateCameraState()
        {
            if (shotController == null) return;

            var prevState = CurrentState;

            CurrentState = shotController.CurrentState switch
            {
                ShotState.Idle or ShotState.Aiming or ShotState.PowerSetting or ShotState.AccuracySetting
                    => golfBall.State == BallState.OnGreen ? CameraState.Putt : CameraState.Tee,
                ShotState.Launching or ShotState.Flying
                    => CameraState.Flight,
                ShotState.Landed
                    => golfBall.State == BallState.OnGreen ? CameraState.Putt : CameraState.Approach,
                ShotState.HoleComplete
                    => CameraState.Celebration,
                _ => CameraState.Tee
            };

            // Update target ortho size based on current state
            targetOrthoSize = CurrentState switch
            {
                CameraState.Tee => teeOrthoSize,
                CameraState.Flight => flightOrthoSize,
                CameraState.Approach => approachOrthoSize,
                CameraState.Putt => puttOrthoSize,
                CameraState.Celebration => celebrationOrthoSize,
                _ => teeOrthoSize
            };

            // Reset velocity on state change for snappier transitions
            if (prevState != CurrentState)
            {
                positionVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Instantly snap the camera to a given state with no smoothing.
        /// Useful for scene load or hard cuts.
        /// </summary>
        public void SnapToState(CameraState state)
        {
            CurrentState = state;
            positionVelocity = Vector3.zero;

            targetOrthoSize = state switch
            {
                CameraState.Tee => teeOrthoSize,
                CameraState.Flight => flightOrthoSize,
                CameraState.Approach => approachOrthoSize,
                CameraState.Putt => puttOrthoSize,
                CameraState.Celebration => celebrationOrthoSize,
                _ => teeOrthoSize
            };

            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = targetOrthoSize;
            }

            if (golfBall == null || cameraTransform == null) return;

            // Snap position instantly
            Vector3 snapTarget;
            if (state == CameraState.Celebration && pinTarget != null)
            {
                snapTarget = new Vector3(pinTarget.position.x, cameraHeight, pinTarget.position.z);
            }
            else
            {
                Vector3 ballPos = golfBall.transform.position;
                snapTarget = new Vector3(ballPos.x, cameraHeight, ballPos.z);
            }

            cameraTransform.position = snapTarget;
            cameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        /// <summary>
        /// Assign the pin/hole transform at runtime (e.g. when loading a hole).
        /// </summary>
        public void SetPinTarget(Transform pin)
        {
            pinTarget = pin;
        }
    }
}
