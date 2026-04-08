using UnityEngine;
using TMPro;
using GolfGame.Building;

namespace GolfGame.UI.Components
{
    public class TimerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private UnityEngine.UI.Image progressBar;
        [SerializeField] private ConstructionTimer timer;

        private void Update()
        {
            if (timer == null || !timer.IsRunning)
            {
                if (timerText != null) timerText.text = "";
                if (progressBar != null) progressBar.fillAmount = 0;
                return;
            }

            if (timerText != null)
                timerText.text = timer.GetFormattedTime();

            if (progressBar != null)
                progressBar.fillAmount = timer.Progress;
        }

        public void SetTimer(ConstructionTimer t)
        {
            timer = t;
        }
    }
}
