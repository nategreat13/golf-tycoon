using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Golf.ShotMechanic;

namespace GolfGame.UI.Components
{
    public class PowerBarUI : MonoBehaviour
    {
        [SerializeField] private PowerBar powerBar;

        [Header("Layout")]
        [SerializeField] private float barWidth = 60f;
        [SerializeField] private float barHeight = 400f;
        [SerializeField] private float screenXPercent = 0.90f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color indicatorColor = Color.white;

        // Root container that we toggle on/off
        private GameObject container;

        // The colored fill segments (bottom to top: green -> yellow -> red)
        private Image[] fillSegments;
        private const int SegmentCount = 60;

        // Indicator arrow
        private RectTransform indicatorRect;
        private Image indicatorImage;

        // Background
        private RectTransform bgRect;

        // Cached fill area height for indicator positioning
        private float fillAreaHeight;
        private float fillAreaBottomY;

        private void Awake()
        {
            BuildVisuals();
        }

        private void BuildVisuals()
        {
            // --- Root container ---
            container = new GameObject("PowerBarContainer");
            container.transform.SetParent(transform, false);
            var containerRect = container.AddComponent<RectTransform>();
            // Position: 20% from left edge, vertically centered
            containerRect.anchorMin = new Vector2(screenXPercent, 0.5f);
            containerRect.anchorMax = new Vector2(screenXPercent, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(barWidth + 50f, barHeight + 60f);

            // --- Outer frame (slight border) ---
            var frame = CreateChild(container.transform, "Frame");
            var frameRect = SetAnchors(frame, 0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(barWidth + 6f, barHeight + 6f);
            var frameImg = frame.AddComponent<Image>();
            frameImg.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);

            // --- Background ---
            var bg = CreateChild(container.transform, "Background");
            bgRect = SetAnchors(bg, 0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(barWidth, barHeight);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = backgroundColor;

            // --- Fill area (masked region for colored segments) ---
            var fillArea = CreateChild(bg.transform, "FillArea");
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);
            var fillMask = fillArea.AddComponent<RectMask2D>();

            fillAreaHeight = barHeight - 4f;
            fillAreaBottomY = 0f;

            // --- Colored segments (each is a thin horizontal slice) ---
            fillSegments = new Image[SegmentCount];
            float segHeight = fillAreaHeight / SegmentCount;

            for (int i = 0; i < SegmentCount; i++)
            {
                float t = (float)i / (SegmentCount - 1); // 0 at bottom, 1 at top
                var seg = CreateChild(fillArea.transform, $"Seg_{i}");
                var segRect = seg.AddComponent<RectTransform>();
                segRect.anchorMin = new Vector2(0f, 0f);
                segRect.anchorMax = new Vector2(1f, 0f);
                segRect.pivot = new Vector2(0.5f, 0f);
                segRect.anchoredPosition = new Vector2(0f, i * segHeight);
                segRect.sizeDelta = new Vector2(0f, segHeight + 0.5f); // slight overlap to avoid gaps

                var segImg = seg.AddComponent<Image>();
                segImg.color = GetGradientColor(t);
                fillSegments[i] = segImg;
            }

            // --- Tick marks and labels ---
            CreateTickLabel(container.transform, 0f, "0%");
            CreateTickLabel(container.transform, 0.5f, "50%");
            CreateTickLabel(container.transform, 1f, "100%");

            // --- Indicator / marker (white triangle arrow on the right side) ---
            var indicator = CreateChild(container.transform, "Indicator");
            indicatorRect = indicator.AddComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
            indicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
            indicatorRect.pivot = new Vector2(0f, 0.5f);
            indicatorRect.sizeDelta = new Vector2(18f, 12f);

            indicatorImage = indicator.AddComponent<Image>();
            indicatorImage.color = indicatorColor;

            // Inner bright line across the bar at the indicator position
            var indicatorLine = CreateChild(container.transform, "IndicatorLine");
            var lineRect = indicatorLine.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.5f, 0.5f);
            lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            lineRect.sizeDelta = new Vector2(barWidth + 2f, 3f);
            var lineImg = indicatorLine.AddComponent<Image>();
            lineImg.color = new Color(1f, 1f, 1f, 0.9f);
            // Store the line rect so we can move it alongside the indicator
            _indicatorLineRect = lineRect;

            // --- Glow overlay at current fill level ---
            var glow = CreateChild(fillArea.transform, "Glow");
            _glowRect = glow.AddComponent<RectTransform>();
            _glowRect.anchorMin = new Vector2(0f, 0f);
            _glowRect.anchorMax = new Vector2(1f, 0f);
            _glowRect.pivot = new Vector2(0.5f, 0.5f);
            _glowRect.sizeDelta = new Vector2(0f, 20f);
            _glowImage = glow.AddComponent<Image>();
            _glowImage.color = new Color(1f, 1f, 1f, 0.3f);

            // --- "POWER" label above the bar ---
            var powerLabel = CreateChild(container.transform, "PowerLabel");
            var powerLabelRect = powerLabel.AddComponent<RectTransform>();
            powerLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            powerLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            powerLabelRect.pivot = new Vector2(0.5f, 0.5f);
            powerLabelRect.anchoredPosition = new Vector2(0f, barHeight * 0.5f + 20f);
            powerLabelRect.sizeDelta = new Vector2(80f, 30f);
            var powerTmp = powerLabel.AddComponent<TextMeshProUGUI>();
            powerTmp.text = "POWER";
            powerTmp.fontSize = 16f;
            powerTmp.fontStyle = FontStyles.Bold;
            powerTmp.color = Color.white;
            powerTmp.alignment = TextAlignmentOptions.Center;
            powerTmp.enableAutoSizing = false;
            powerTmp.overflowMode = TextOverflowModes.Overflow;

            container.SetActive(false);
        }

        private RectTransform _indicatorLineRect;
        private RectTransform _glowRect;
        private Image _glowImage;

        private void CreateTickLabel(Transform parent, float t, string text)
        {
            // t: 0 = bottom, 1 = top
            float yOffset = (t - 0.5f) * barHeight;

            // Small tick line on the right of the bar
            var tick = CreateChild(parent, $"Tick_{text}");
            var tickRect = tick.AddComponent<RectTransform>();
            tickRect.anchorMin = new Vector2(0.5f, 0.5f);
            tickRect.anchorMax = new Vector2(0.5f, 0.5f);
            tickRect.pivot = new Vector2(0f, 0.5f);
            tickRect.anchoredPosition = new Vector2(barWidth * 0.5f + 2f, yOffset);
            tickRect.sizeDelta = new Vector2(6f, 2f);
            var tickImg = tick.AddComponent<Image>();
            tickImg.color = new Color(1f, 1f, 1f, 0.6f);

            // Label
            var label = CreateChild(parent, $"Label_{text}");
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(barWidth * 0.5f + 10f, yOffset);
            labelRect.sizeDelta = new Vector2(50f, 20f);

            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12f;
            tmp.color = new Color(1f, 1f, 1f, 0.8f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        private Color GetGradientColor(float t)
        {
            // t: 0 = bottom (low power) = green, 0.5 = yellow, 1.0 = top (max power) = red
            if (t < 0.5f)
            {
                float lerp = t * 2f;
                return Color.Lerp(
                    new Color(0.2f, 0.9f, 0.2f, 1f),  // green
                    new Color(1f, 0.9f, 0.1f, 1f),     // yellow
                    lerp
                );
            }
            else
            {
                float lerp = (t - 0.5f) * 2f;
                return Color.Lerp(
                    new Color(1f, 0.9f, 0.1f, 1f),     // yellow
                    new Color(1f, 0.15f, 0.1f, 1f),    // red
                    lerp
                );
            }
        }

        private void Update()
        {
            if (powerBar == null) return;

            bool shouldBeVisible = powerBar.IsActive || powerBar.IsLocked;

            if (container.activeSelf != shouldBeVisible)
                container.SetActive(shouldBeVisible);

            if (!shouldBeVisible) return;

            float value = powerBar.Value; // 0 to 1

            // --- Dim segments above current power, brighten below ---
            for (int i = 0; i < SegmentCount; i++)
            {
                float t = (float)i / (SegmentCount - 1);
                Color baseColor = GetGradientColor(t);

                if (t <= value)
                {
                    // Below or at power level: full color
                    fillSegments[i].color = baseColor;
                }
                else
                {
                    // Above power level: dimmed
                    fillSegments[i].color = baseColor * new Color(0.3f, 0.3f, 0.3f, 0.4f);
                }
            }

            // --- Move indicator ---
            float yPos = (value - 0.5f) * barHeight;
            float xRight = barWidth * 0.5f + 3f;
            indicatorRect.anchoredPosition = new Vector2(xRight, yPos);
            _indicatorLineRect.anchoredPosition = new Vector2(0f, yPos);

            // Glow at fill level
            _glowRect.anchoredPosition = new Vector2(0f, value * fillAreaHeight);
            Color glowColor = GetGradientColor(value);
            glowColor.a = 0.35f;
            _glowImage.color = glowColor;

            // Pulse the indicator slightly
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 8f);
            indicatorImage.color = new Color(1f, 1f, 1f, pulse);
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform SetAnchors(GameObject go, float anchorX, float anchorY)
        {
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }
    }
}
