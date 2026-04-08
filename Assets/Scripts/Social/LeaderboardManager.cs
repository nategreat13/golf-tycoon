using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GolfGame.Social
{
    [Serializable]
    public class LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public int score;
        public long timestampUtc;
    }

    [Serializable]
    public class CourseLeaderboard
    {
        public string courseId;
        public List<LeaderboardEntry> allTimeEntries = new();
        public List<LeaderboardEntry> weeklyEntries = new();
        public int courseRecord = int.MaxValue;
        public string courseRecordHolder;

        public void AddEntry(LeaderboardEntry entry)
        {
            allTimeEntries.Add(entry);
            allTimeEntries = allTimeEntries.OrderBy(e => e.score).Take(100).ToList();

            weeklyEntries.Add(entry);
            weeklyEntries = weeklyEntries.OrderBy(e => e.score).Take(50).ToList();

            if (entry.score < courseRecord)
            {
                courseRecord = entry.score;
                courseRecordHolder = entry.playerName;
            }
        }

        public void PruneWeeklyEntries()
        {
            long oneWeekAgo = DateTime.UtcNow.AddDays(-7).ToBinary();
            weeklyEntries.RemoveAll(e => e.timestampUtc < oneWeekAgo);
        }
    }

    public class LeaderboardManager : MonoBehaviour
    {
        private Dictionary<string, CourseLeaderboard> leaderboards = new();

        public CourseLeaderboard GetLeaderboard(string courseId)
        {
            if (!leaderboards.TryGetValue(courseId, out var board))
            {
                board = new CourseLeaderboard { courseId = courseId };
                leaderboards[courseId] = board;
            }
            return board;
        }

        public void SubmitScore(string courseId, string playerId, string playerName, int totalStrokes)
        {
            var board = GetLeaderboard(courseId);
            board.AddEntry(new LeaderboardEntry
            {
                playerId = playerId,
                playerName = playerName,
                score = totalStrokes,
                timestampUtc = DateTime.UtcNow.ToBinary()
            });
        }

        public List<LeaderboardEntry> GetTopScores(string courseId, int count = 10)
        {
            var board = GetLeaderboard(courseId);
            return board.allTimeEntries.Take(count).ToList();
        }

        public int? GetCourseRecord(string courseId)
        {
            var board = GetLeaderboard(courseId);
            return board.courseRecord < int.MaxValue ? board.courseRecord : null;
        }

        public int? GetPlayerBest(string courseId, string playerId)
        {
            var board = GetLeaderboard(courseId);
            var playerEntries = board.allTimeEntries.Where(e => e.playerId == playerId);
            return playerEntries.Any() ? playerEntries.Min(e => e.score) : null;
        }
    }
}
