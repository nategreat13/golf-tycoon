using UnityEngine;
using TMPro;
using GolfGame.Core;

namespace GolfGame.Golf
{
    public class ShotFeedback : MonoBehaviour
    {
        private Canvas parentCanvas;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            EventBus.Subscribe<ShotCompletedEvent>(OnShotCompleted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ShotCompletedEvent>(OnShotCompleted);
        }

        private void OnShotCompleted(ShotCompletedEvent evt)
        {
            if (evt.distance < 1f) return;
            SpawnDistanceText(evt.distance);
        }

        private void SpawnDistanceText(float distance)
        {
            var obj = new GameObject("DistanceFeedback");
            obj.transform.SetParent(parentCanvas != null ? parentCanvas.transform : transform, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 100);
            rect.sizeDelta = new Vector2(300, 60);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{distance:F0} yards";
            tmp.fontSize = 36;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color32(0, 0, 0, 180);
            obj.AddComponent<ShotFeedbackAnimator>();
        }
    }

    public class ShotFeedbackAnimator : MonoBehaviour
    {
        private float elapsed;
        private RectTransform rect;
        private TextMeshProUGUI text;
        private Color startColor;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (text != null) startColor = text.color;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (rect != null)
                rect.anchoredPosition += Vector2.up * 40f * Time.deltaTime;
            if (text != null)
            {
                float alpha = Mathf.Clamp01(1f - (elapsed / 2.5f));
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            if (elapsed >= 2.5f) Destroy(gameObject);
        }
    }
}
