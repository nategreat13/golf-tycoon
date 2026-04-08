using UnityEngine;

namespace GolfGame.Building
{
    /// <summary>
    /// Top-down/isometric camera for the property builder view.
    /// Supports pinch-to-zoom and drag-to-pan for mobile.
    /// </summary>
    public class BuildingCamera : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panSmoothing = 5f;
        [SerializeField] private Vector2 panLimitX = new Vector2(-100, 100);
        [SerializeField] private Vector2 panLimitZ = new Vector2(-100, 100);

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 10f;
        [SerializeField] private float maxZoom = 60f;
        [SerializeField] private float zoomSmoothing = 5f;

        [Header("View Angle")]
        [SerializeField] private float viewAngle = 55f; // degrees from horizontal

        private UnityEngine.Camera cam;
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector2 lastTouchMidpoint;
        private float lastPinchDistance;
        private bool isDragging;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null) cam = UnityEngine.Camera.main;

            targetZoom = (minZoom + maxZoom) / 2f;
            targetPosition = transform.position;
        }

        private void Start()
        {
            // Set initial rotation
            transform.rotation = Quaternion.Euler(viewAngle, 0, 0);
        }

        private void Update()
        {
            HandleInput();
            ApplyTransform();
        }

        private void HandleInput()
        {
            // Mobile touch input
            if (Input.touchCount == 1)
            {
                HandleSingleTouch(Input.GetTouch(0));
            }
            else if (Input.touchCount == 2)
            {
                HandlePinchZoom(Input.GetTouch(0), Input.GetTouch(1));
            }
            else
            {
                isDragging = false;
            }

            // Mouse/keyboard fallback
            if (Input.touchCount == 0)
            {
                HandleMouseInput();
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.deltaPosition;
                        targetPosition -= new Vector3(delta.x, 0, delta.y) * panSpeed * Time.deltaTime / cam.orthographicSize;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }

        private void HandlePinchZoom(Touch touch0, Touch touch1)
        {
            isDragging = false;

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                lastPinchDistance = currentDistance;
                return;
            }

            float delta = currentDistance - lastPinchDistance;
            targetZoom -= delta * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            lastPinchDistance = currentDistance;
        }

        private void HandleMouseInput()
        {
            // Pan with right click or middle click
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                float h = Input.GetAxis("Mouse X") * panSpeed * 0.5f;
                float v = Input.GetAxis("Mouse Y") * panSpeed * 0.5f;
                targetPosition -= new Vector3(h, 0, v);
            }

            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * zoomSpeed * 10f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }

        private void ApplyTransform()
        {
            // Clamp pan limits
            targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);

            // Smooth position
            Vector3 smoothed = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSmoothing);
            transform.position = smoothed;

            // Smooth zoom
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothing);
            }
            else
            {
                // For perspective, adjust Y position as zoom
                float currentY = transform.position.y;
                float targetY = targetZoom;
                transform.position = new Vector3(transform.position.x,
                    Mathf.Lerp(currentY, targetY, Time.deltaTime * zoomSmoothing),
                    transform.position.z);
            }
        }

        public void FocusOn(Vector3 worldPosition)
        {
            targetPosition = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);
        }
    }
}
