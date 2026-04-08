using System;
using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Economy;

namespace GolfGame.Social
{
    [Serializable]
    public class VisitableCourse
    {
        public string courseId;
        public string ownerId;
        public string ownerName;
        public string courseName;
        public int holeCount;
        public float rating;
        public int timesPlayed;
        public int courseRecord;
    }

    public class CourseVisitManager : MonoBehaviour
    {
        [SerializeField] private int maxBrowseResults = 20;

        // Simulated course list (would come from server in production)
        private List<VisitableCourse> availableCourses = new();

        public event Action<List<VisitableCourse>> OnCoursesLoaded;

        /// <summary>
        /// Fetch available courses to visit. In production, this would be a server call.
        /// For now, generates some sample courses.
        /// </summary>
        public void FetchAvailableCourses()
        {
            // Generate sample courses for offline play
            if (availableCourses.Count == 0)
            {
                GenerateSampleCourses();
            }

            OnCoursesLoaded?.Invoke(availableCourses);
        }

        /// <summary>
        /// Called when a real player completes a round on someone else's course.
        /// Rewards both the visitor and the course owner.
        /// </summary>
        public void CompleteVisit(string courseOwnerId, int score)
        {
            // Reward the visiting player
            var currency = ServiceLocator.Get<CurrencyManager>();
            var income = ServiceLocator.Get<IncomeCalculator>();

            if (currency != null && income != null)
            {
                // Visitor gets a flat reward + bonus for good score
                long visitReward = 50;
                currency.Add(visitReward);
            }

            // Reward the course owner (would be server-side in production)
            if (income != null)
            {
                float ownerIncome = income.GetRealPlayerVisitIncome();
                currency?.Add((long)ownerIncome);
            }

            // Reputation for visiting
            var rep = ServiceLocator.Get<Progression.ReputationSystem>();
            rep?.AddReputation(GolfGame.Data.GameConstants.ReputationPerRoundPlayed);

            EventBus.Publish(new CourseVisitedEvent
            {
                courseOwnerId = courseOwnerId,
                score = score
            });
        }

        private void GenerateSampleCourses()
        {
            string[] names = {
                "Pine Valley", "Augusta Links", "Pebble Beach Jr",
                "Desert Oasis", "Tropical Paradise", "Sunset Ridge",
                "Eagle Creek", "Wolf Run", "Hidden Valley"
            };

            for (int i = 0; i < names.Length; i++)
            {
                availableCourses.Add(new VisitableCourse
                {
                    courseId = $"sample_{i}",
                    ownerId = $"npc_{i}",
                    ownerName = $"Player{i + 1}",
                    courseName = names[i],
                    holeCount = i < 3 ? 1 : (i < 6 ? 3 : 9),
                    rating = 3f + UnityEngine.Random.Range(0f, 2f),
                    timesPlayed = UnityEngine.Random.Range(10, 500),
                    courseRecord = UnityEngine.Random.Range(-3, 5)
                });
            }
        }
    }
}
