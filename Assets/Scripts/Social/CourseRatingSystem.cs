using System;
using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Social
{
    [Serializable]
    public class CourseRating
    {
        public string playerId;
        public int stars; // 1-5
        public long timestampUtc;
    }

    public class CourseRatingSystem : MonoBehaviour
    {
        private Dictionary<string, List<CourseRating>> ratings = new();

        public float GetAverageRating(string courseId)
        {
            if (!ratings.TryGetValue(courseId, out var courseRatings) || courseRatings.Count == 0)
                return 0f;

            float total = 0;
            foreach (var r in courseRatings)
                total += r.stars;

            return total / courseRatings.Count;
        }

        public int GetRatingCount(string courseId)
        {
            if (!ratings.TryGetValue(courseId, out var courseRatings))
                return 0;
            return courseRatings.Count;
        }

        public void SubmitRating(string courseId, string playerId, int stars)
        {
            stars = Mathf.Clamp(stars, 1, 5);

            if (!ratings.ContainsKey(courseId))
                ratings[courseId] = new List<CourseRating>();

            // Remove existing rating from this player
            ratings[courseId].RemoveAll(r => r.playerId == playerId);

            ratings[courseId].Add(new CourseRating
            {
                playerId = playerId,
                stars = stars,
                timestampUtc = DateTime.UtcNow.ToBinary()
            });
        }

        public int? GetPlayerRating(string courseId, string playerId)
        {
            if (!ratings.TryGetValue(courseId, out var courseRatings))
                return null;

            var rating = courseRatings.Find(r => r.playerId == playerId);
            return rating?.stars;
        }
    }
}
