using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GolfGame.Social;

namespace GolfGame.UI.Screens
{
    public class LeaderboardScreen : UIScreen
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI courseNameText;
        [SerializeField] private TextMeshProUGUI courseRecordText;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private GameObject entryPrefab;

        [Header("Tabs")]
        [SerializeField] private Button allTimeTab;
        [SerializeField] private Button weeklyTab;
        [SerializeField] private Button backButton;

        [Header("References")]
        [SerializeField] private LeaderboardManager leaderboardManager;

        private string currentCourseId;
        private bool showWeekly;

        private void Awake()
        {
            allTimeTab?.onClick.AddListener(() => { showWeekly = false; Refresh(); });
            weeklyTab?.onClick.AddListener(() => { showWeekly = true; Refresh(); });
            backButton?.onClick.AddListener(() => UIManager.Instance?.GoBack());
        }

        public void ShowForCourse(string courseId, string courseName)
        {
            currentCourseId = courseId;
            if (courseNameText != null) courseNameText.text = courseName;
            showWeekly = false;
            Show();
            Refresh();
        }

        private void Refresh()
        {
            if (leaderboardManager == null || entryContainer == null || entryPrefab == null)
                return;

            // Clear
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);

            var board = leaderboardManager.GetLeaderboard(currentCourseId);
            var entries = showWeekly ? board.weeklyEntries : board.allTimeEntries;

            // Course record
            if (courseRecordText != null)
            {
                courseRecordText.text = board.courseRecord < int.MaxValue
                    ? $"Course Record: {board.courseRecord} by {board.courseRecordHolder}"
                    : "No records yet";
            }

            // Entries
            for (int i = 0; i < entries.Count && i < 20; i++)
            {
                var entry = entries[i];
                var obj = Instantiate(entryPrefab, entryContainer);
                var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{i + 1}";
                    texts[1].text = entry.playerName;
                    texts[2].text = entry.score.ToString();
                }
            }
        }
    }
}
