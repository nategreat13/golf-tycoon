using UnityEngine;
using TMPro;
using GolfGame.Core;
using GolfGame.Golf.Scoring;

namespace GolfGame.UI.Components
{
    /// <summary>
    /// Floating score popup that appears when a hole is completed.
    /// Shows labels like "BIRDIE!", "PAR", "BOGEY" that animate upward and fade out.
    /// Subscribes to HoleCompletedEvent via EventBus.
    /// </summary>
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 80f;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private float scalePunchAmount = 1.3f;
        [SerializeField] private float scalePunchDuration = 0.2f;

        private static readonly Color GoldColor = new Color(1f, 0.84f, 0f);
        private static readonly Color WhiteColor = Color.white;
        private static readonly Color RedColor = new Color(1f, 0.3f, 0.3f);

        private Canvas parentCanvas;

        private void OnEnable()
        {
            EventBus.Subscribe<HoleCompletedEvent>(OnHoleCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HoleCompletedEvent>(OnHoleCompleted);
        }

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void OnHoleCompleted(HoleCompletedEvent evt)
        {
            var result = ScoreCalculator.GetHoleResult(evt.strokes, evt.par);
            string label = ScoreCalculator.GetLabelString(result.label);
            Color color = GetColorForScore(result.label);
            SpawnPopup(label, color);
        }

        private Color GetColorForScore(ScoreLabel label)
        {
            switch (label)
            {
                case ScoreLabel.HoleInOne:
                case ScoreLabel.Albatross:
                case ScoreLabel.Eagle:
                case ScoreLabel.Birdie:
                    return GoldColor;
                case ScoreLabel.Par:
                    return WhiteColor;
                default:
                    return RedColor;
            }
        }

        private void SpawnPopup(string text, Color color)
        {
            // Create popup GameObject as child of this component's transform (on the Canvas)
            var popupObj = new GameObject("ScorePopup");
            popupObj.transform.SetParent(parentCanvas != null ? parentCanvas.transform : transform, false);

            var rectTransform = popupObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 50); // Center-ish, slightly above middle
            rectTransform.sizeDelta = new Vector2(400, 100);

            var tmp = popupObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 64;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;

            // Add outline for readability
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color32(0, 0, 0, 128);

            // Add the animator to drive the float-up and fade
            var animator = popupObj.AddComponent<ScorePopupAnimator>();
            animator.Init(floatSpeed, fadeDuration, scalePunchAmount, scalePunchDuration);
        }
    }

    /// <summary>
    /// Handles the animation of a single score popup instance.
    /// Floats upward, punches scale, and fades out before self-destructing.
    /// </summary>
    public class ScorePopupAnimator : MonoBehaviour
    {
        private float floatSpeed;
        private float fadeDuration;
        private float scalePunchAmount;
        private float scalePunchDuration;

        private RectTransform rectTransform;
        private TextMeshProUGUI text;
        private float elapsed;
        private Color startColor;
        private Vector3 baseScale;

        public void Init(float floatSpeed, float fadeDuration, float scalePunch, float scalePunchDur)
        {
            this.floatSpeed = floatSpeed;
            this.fadeDuration = fadeDuration;
            this.scalePunchAmount = scalePunch;
            this.scalePunchDuration = scalePunchDur;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (text != null)
                startColor = text.color;
            baseScale = Vector3.one;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            // Float upward
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;
            }

            // Scale punch: start big, shrink to normal
            if (elapsed < scalePunchDuration)
            {
                float t = elapsed / scalePunchDuration;
                float scale = Mathf.Lerp(scalePunchAmount, 1f, t);
                transform.localScale = baseScale * scale;
            }
            else
            {
                transform.localScale = baseScale;
            }

            // Fade out
            if (text != null)
            {
                float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            // Destroy when fully faded
            if (elapsed >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }
    }
}
