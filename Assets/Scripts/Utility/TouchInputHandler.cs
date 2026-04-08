using UnityEngine;

namespace GolfGame.Utility
{
    /// <summary>
    /// Abstracts touch and mouse input into a unified interface for mobile and editor.
    /// </summary>
    public class TouchInputHandler : MonoBehaviour
    {
        public static TouchInputHandler Instance { get; private set; }

        public bool TapThisFrame { get; private set; }
        public bool Holding { get; private set; }
        public bool Released { get; private set; }
        public Vector2 TapPosition { get; private set; }
        public Vector2 DragDelta { get; private set; }
        public float PinchDelta { get; private set; }

        private Vector2 lastTouchPosition;
        private float lastPinchDistance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            TapThisFrame = false;
            Released = false;
            DragDelta = Vector2.zero;
            PinchDelta = 0f;

            if (Input.touchCount > 0)
            {
                HandleTouch();
            }
            else
            {
                HandleMouse();
            }
        }

        private void HandleTouch()
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TapThisFrame = true;
                    TapPosition = touch.position;
                    lastTouchPosition = touch.position;
                    Holding = true;
                    break;

                case TouchPhase.Moved:
                    DragDelta = touch.position - lastTouchPosition;
                    lastTouchPosition = touch.position;
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    Released = true;
                    Holding = false;
                    break;
            }

            // Pinch zoom
            if (Input.touchCount >= 2)
            {
                Touch touch1 = Input.GetTouch(1);
                float currentDist = Vector2.Distance(touch.position, touch1.position);

                if (touch1.phase == TouchPhase.Began)
                {
                    lastPinchDistance = currentDist;
                }
                else
                {
                    PinchDelta = currentDist - lastPinchDistance;
                    lastPinchDistance = currentDist;
                }
            }
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TapThisFrame = true;
                TapPosition = Input.mousePosition;
                Holding = true;
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 currentPos = Input.mousePosition;
                DragDelta = currentPos - lastTouchPosition;
                lastTouchPosition = currentPos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Released = true;
                Holding = false;
            }

            // Scroll wheel as pinch
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                PinchDelta = scroll * 100f;
            }
        }
    }
}
