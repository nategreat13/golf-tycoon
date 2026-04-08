using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Golf.Course
{
    /// <summary>
    /// Loads a course from CourseData and manages hole transitions during a round.
    /// </summary>
    public class CourseLoader : MonoBehaviour
    {
        [SerializeField] private GameObject holeTemplatePrefab;
        [SerializeField] private Transform holeContainer;

        private CourseData activeCourse;
        private List<HoleInstance> loadedHoles = new();
        private int currentHoleIndex;

        public HoleInstance CurrentHole => currentHoleIndex < loadedHoles.Count ? loadedHoles[currentHoleIndex] : null;
        public int CurrentHoleNumber => currentHoleIndex + 1;
        public int TotalHoles => activeCourse?.holes?.Count ?? 0;
        public bool IsLastHole => currentHoleIndex >= TotalHoles - 1;

        public event System.Action<HoleInstance, int> OnHoleLoaded;

        public void LoadCourse(CourseData course)
        {
            UnloadCourse();
            activeCourse = course;
            currentHoleIndex = 0;

            if (course.holes == null || course.holes.Count == 0)
            {
                Debug.LogError("Course has no holes!");
                return;
            }

            // Pre-instantiate all holes (offset them so only current is visible)
            for (int i = 0; i < course.holes.Count; i++)
            {
                var holeData = course.holes[i];
                if (holeData == null) continue;

                GameObject holeObj;
                if (holeTemplatePrefab != null)
                {
                    holeObj = Instantiate(holeTemplatePrefab, holeContainer);
                }
                else
                {
                    holeObj = new GameObject($"Hole_{i + 1}");
                    holeObj.transform.SetParent(holeContainer);
                    holeObj.AddComponent<HoleInstance>();
                }

                // Offset each hole along X axis so they don't overlap
                holeObj.transform.localPosition = new Vector3(i * 600f, 0, 0);
                holeObj.name = $"Hole_{i + 1}_{holeData.holeName}";

                var instance = holeObj.GetComponent<HoleInstance>();
                if (instance != null)
                {
                    instance.Initialize(holeData);
                    loadedHoles.Add(instance);
                }

                // Deactivate all except first
                holeObj.SetActive(i == 0);
            }

            if (loadedHoles.Count > 0)
            {
                OnHoleLoaded?.Invoke(loadedHoles[0], 0);
            }
        }

        public HoleInstance AdvanceToNextHole()
        {
            if (IsLastHole) return null;

            // Deactivate current
            if (CurrentHole != null)
                CurrentHole.gameObject.SetActive(false);

            currentHoleIndex++;

            // Activate next
            if (CurrentHole != null)
            {
                CurrentHole.gameObject.SetActive(true);
                OnHoleLoaded?.Invoke(CurrentHole, currentHoleIndex);
            }

            return CurrentHole;
        }

        public void UnloadCourse()
        {
            foreach (var hole in loadedHoles)
            {
                if (hole != null)
                    Destroy(hole.gameObject);
            }
            loadedHoles.Clear();
            activeCourse = null;
            currentHoleIndex = 0;
        }

        private void OnDestroy()
        {
            UnloadCourse();
        }
    }
}
