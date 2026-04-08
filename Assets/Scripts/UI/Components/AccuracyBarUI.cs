using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Golf.ShotMechanic;

namespace GolfGame.UI.Components
{
    public class AccuracyBarUI : MonoBehaviour
    {
        [SerializeField] private AccuracyBar accuracyBar;

        [Header("Layout")]
        [SerializeField] private float barWidth = 500f;
        [SerializeField] private float barHeight = 40f;
        [SerializeField] private float bottomOffset = 80f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color needleColor = Color.white;

        // Root container
        private GameObject container;

        // Colored zone images (built as horizontal segments)
        private Image[] zoneSegments;
        private const int SegmentCount = 80;

        // Needle / indicator
        private RectTransform needleRect;
        private Image needleImage;

        // Sweet spot highlight overlay
        private Image sweetSpotGlow;

        private void Awake()
        {
            BuildVisuals();
        }

        private void BuildVisuals()
        {
            // --- Root container ---
            container = new GameObject("AccuracyBarContainer");
            container.transform.SetParent(transform, false);
            var containerRect = container.AddComponent<RectTransform>();
            // Bottom center of screen
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, bottomOffset);
            containerRect.sizeDelta = new Vector2(barWidth + 40f, barHeight + 50f);

            // --- Outer frame ---
            var frame = CreateChild(container.transform, "Frame");
            var frameRect = SetAnchors(frame, 0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(barWidth + 6f, barHeight + 6f);
            var frameImg = frame.AddComponent<Image>();
            frameImg.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);

            // --- Background ---
            var bg = CreateChild(container.transform, "Background");
            var bgRect = SetAnchors(bg, 0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(barWidth, barHeight);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = backgroundColor;

            // --- Fill area (masked) ---
            var fillArea = CreateChild(bg.transform, "FillArea");
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);
            fillArea.AddComponent<RectMask2D>();

            float innerWidth = barWidth - 4f;
            float innerHeight = barHeight - 4f;

            // --- Colored zone segments (left to right: red -> yellow -> green -> yellow -> red) ---
            zoneSegments = new Image[SegmentCount];
            float segWidth = innerWidth / SegmentCount;

            for (int i = 0; i < SegmentCount; i++)
            {
                float t = (float)i / (SegmentCount - 1); // 0 = left edge, 1 = right edge
                var seg = CreateChild(fillArea.transform, $"Seg_{i}");
                var segRect = seg.AddComponent<RectTransform>();
                segRect.anchorMin = new Vector2(0f, 0f);
                segRect.anchorMax = new Vector2(0f, 1f);
                segRect.pivot = new Vector2(0f, 0.5f);
                segRect.anchoredPosition = new Vector2(i * segWidth, 0f);
                segRect.sizeDelta = new Vector2(segWidth + 0.5f, 0f); // slight overlap

                var segImg = seg.AddComponent<Image>();
                segImg.color = GetZoneColor(t);
                zoneSegments[i] = segImg;
            }

            // --- Sweet spot glow overlay (center 20%) ---
            var sweetSpot = CreateChild(fillArea.transform, "SweetSpotGlow");
            var ssRect = sweetSpot.AddComponent<RectTransform>();
            ssRect.anchorMin = new Vector2(0.4f, 0f);
            ssRect.anchorMax = new Vector2(0.6f, 1f);
            ssRect.offsetMin = Vector2.zero;
            ssRect.offsetMax = Vector2.zero;
            sweetSpotGlow = sweetSpot.AddComponent<Image>();
            sweetSpotGlow.color = new Color(0.4f, 1f, 0.4f, 0.15f);

            // --- Center line marker ---
            var centerLine = CreateChild(fillArea.transform, "CenterLine");
            var clRect = centerLine.AddComponent<RectTransform>();
            clRect.anchorMin = new Vector2(0.5f, 0f);
            clRect.anchorMax = new Vector2(0.5f, 1f);
            clRect.pivot = new Vector2(0.5f, 0.5f);
            clRect.anchoredPosition = Vector2.zero;
            clRect.sizeDelta = new Vector2(2f, 0f);
            var clImg = centerLine.AddComponent<Image>();
            clImg.color = new Color(1f, 1f, 1f, 0.5f);

            // --- Needle / indicator ---
            var needle = CreateChild(container.transform, "Needle");
            needleRect = needle.AddComponent<RectTransform>();
            needleRect.anchorMin = new Vector2(0.5f, 0.5f);
            needleRect.anchorMax = new Vector2(0.5f, 0.5f);
            needleRect.pivot = new Vector2(0.5f, 0.5f);
            needleRect.sizeDelta = new Vector2(4f, barHeight + 16f);
            needleImage = needle.AddComponent<Image>();
            needleImage.color = needleColor;

            // Needle top triangle cap
            var cap = CreateChild(needle.transform, "Cap");
            var capRect = cap.AddComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0.5f, 1f);
            capRect.anchorMax = new Vector2(0.5f, 1f);
            capRect.pivot = new Vector2(0.5f, 0f);
            capRect.anchoredPosition = Vector2.zero;
            capRect.sizeDelta = new Vector2(10f, 8f);
            var capImg = cap.AddComponent<Image>();
            capImg.color = needleColor;

            // --- Labels ---
            CreateLabel(container.transform, -barWidth * 0.5f, "L", TextAlignmentOptions.Center);
            CreateLabel(container.transform, 0f, "CENTER", TextAlignmentOptions.Center);
            CreateLabel(container.transform, barWidth * 0.5f, "R", TextAlignmentOptions.Center);

            // --- "ACCURACY" label above the bar ---
            var accuracyLabel = CreateChild(container.transform, "AccuracyLabel");
            var accuracyLabelRect = accuracyLabel.AddComponent<RectTransform>();
            accuracyLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            accuracyLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            accuracyLabelRect.pivot = new Vector2(0.5f, 0.5f);
            accuracyLabelRect.anchoredPosition = new Vector2(0f, barHeight * 0.5f + 20f);
            accuracyLabelRect.sizeDelta = new Vector2(120f, 30f);
            var accuracyTmp = accuracyLabel.AddComponent<TextMeshProUGUI>();
            accuracyTmp.text = "ACCURACY";
            accuracyTmp.fontSize = 16f;
            accuracyTmp.fontStyle = FontStyles.Bold;
            accuracyTmp.color = Color.white;
            accuracyTmp.alignment = TextAlignmentOptions.Center;
            accuracyTmp.enableAutoSizing = false;
            accuracyTmp.overflowMode = TextOverflowModes.Overflow;

            container.SetActive(false);
        }

        private void CreateLabel(Transform parent, float xOffset, string text, TextAlignmentOptions alignment)
        {
            float yBelow = -(barHeight * 0.5f + 12f);

            var label = CreateChild(parent, $"Label_{text}");
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(xOffset, yBelow);
            labelRect.sizeDelta = new Vector2(60f, 18f);

            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 10f;
            tmp.color = new Color(1f, 1f, 1f, 0.7f);
            tmp.alignment = alignment;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        /// <summary>
        /// Returns the zone color for a normalized position along the bar.
        /// t: 0 = far left, 0.5 = center, 1 = far right.
        /// Pattern: red at edges -> yellow -> green at center.
        /// </summary>
        private Color GetZoneColor(float t)
        {
            // Distance from center (0 = center, 0.5 = edge)
            float dist = Mathf.Abs(t - 0.5f) * 2f; // 0 at center, 1 at edges

            if (dist < 0.2f)
            {
                // Sweet spot: bright green
                return new Color(0.15f, 0.85f, 0.25f, 1f);
            }
            else if (dist < 0.5f)
            {
                // Transition zone: green -> yellow
                float lerp = (dist - 0.2f) / 0.3f;
                return Color.Lerp(
                    new Color(0.15f, 0.85f, 0.25f, 1f),
                    new Color(1f, 0.85f, 0.1f, 1f),
                    lerp
                );
            }
            else if (dist < 0.8f)
            {
                // Warning zone: yellow -> orange
                float lerp = (dist - 0.5f) / 0.3f;
                return Color.Lerp(
                    new Color(1f, 0.85f, 0.1f, 1f),
                    new Color(1f, 0.5f, 0.05f, 1f),
                    lerp
                );
            }
            else
            {
                // Danger zone: orange -> red
                float lerp = (dist - 0.8f) / 0.2f;
                return Color.Lerp(
                    new Color(1f, 0.5f, 0.05f, 1f),
                    new Color(1f, 0.1f, 0.1f, 1f),
                    lerp
                );
            }
        }

        private void Update()
        {
            if (accuracyBar == null) return;

            bool shouldBeVisible = accuracyBar.IsActive || accuracyBar.IsLocked;

            if (container.activeSelf != shouldBeVisible)
                container.SetActive(shouldBeVisible);

            if (!shouldBeVisible) return;

            float value = accuracyBar.Value; // -1 to 1

            // --- Move needle ---
            float xPos = value * (barWidth * 0.5f);
            needleRect.anchoredPosition = new Vector2(xPos, 0f);

            // --- Color the needle based on how close to center ---
            float dist = Mathf.Abs(value);
            Color nColor;
            if (dist < 0.2f)
                nColor = new Color(0.3f, 1f, 0.3f, 1f); // green - in sweet spot
            else if (dist < 0.5f)
                nColor = new Color(1f, 1f, 0.3f, 1f);    // yellow
            else
                nColor = new Color(1f, 0.3f, 0.3f, 1f);  // red

            // Pulse effect
            float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 10f);
            nColor.a = pulse;
            needleImage.color = nColor;

            // Also color the cap
            var cap = needleRect.GetChild(0);
            if (cap != null)
            {
                var capImg = cap.GetComponent<Image>();
                if (capImg != null) capImg.color = nColor;
            }

            // Sweet spot glow pulse
            float ssPulse = 0.1f + 0.08f * Mathf.Sin(Time.time * 3f);
            sweetSpotGlow.color = new Color(0.4f, 1f, 0.4f, ssPulse);
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
