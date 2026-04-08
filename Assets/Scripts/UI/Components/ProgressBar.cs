using UnityEngine;
using UnityEngine.UI;

namespace GolfGame.UI.Components
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private float smoothSpeed = 5f;

        private float targetFill;
        private float currentFill;

        public void SetProgress(float value)
        {
            targetFill = Mathf.Clamp01(value);
        }

        public void SetProgressImmediate(float value)
        {
            targetFill = Mathf.Clamp01(value);
            currentFill = targetFill;
            ApplyFill();
        }

        private void Update()
        {
            if (Mathf.Abs(currentFill - targetFill) > 0.001f)
            {
                currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
                ApplyFill();
            }
        }

        private void ApplyFill()
        {
            if (fillImage != null)
                fillImage.fillAmount = currentFill;
        }
    }
}
