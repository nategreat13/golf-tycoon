using System;
using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Social
{
    /// <summary>
    /// Handles async multiplayer: serializing courses for sharing,
    /// downloading other players' courses, and managing visit rewards.
    /// In production, this would integrate with Unity Gaming Services or a custom backend.
    /// </summary>
    [Serializable]
    public class SharedCourseData
    {
        public string courseId;
        public string ownerId;
        public string ownerName;
        public string courseName;
        public int tier;
        public List<SharedHoleData> holes;
        public float rating;
        public int timesPlayed;
    }

    [Serializable]
    public class SharedHoleData
    {
        public int par;
        public int lengthType; // HoleLength enum as int
        public int hazardSet;  // HoleHazardSet enum as int
        public int themeIndex;
        public int qualityLevel;
    }

    public class AsyncMultiplayerManager : MonoBehaviour
    {
        public event Action<SharedCourseData> OnCourseDownloaded;
        public event Action<string> OnError;

        /// <summary>
        /// Serialize the player's course into a shareable format.
        /// </summary>
        public SharedCourseData ExportPlayerCourse()
        {
            var property = ServiceLocator.Get<Building.PropertyManager>();
            var save = ServiceLocator.Get<SaveSystem>();

            if (property == null || save == null) return null;

            var shared = new SharedCourseData
            {
                courseId = save.Data.playerId,
                ownerId = save.Data.playerId,
                ownerName = save.Data.playerName,
                courseName = property.CourseName,
                tier = property.CurrentTier,
                holes = new List<SharedHoleData>(),
                rating = 0,
                timesPlayed = 0
            };

            foreach (var hole in property.Holes)
            {
                if (hole.state == HoleSlotState.Built)
                {
                    shared.holes.Add(new SharedHoleData
                    {
                        par = hole.par,
                        lengthType = (int)hole.length,
                        hazardSet = (int)hole.hazardSet,
                        themeIndex = hole.themeIndex,
                        qualityLevel = hole.qualityLevel
                    });
                }
            }

            return shared;
        }

        /// <summary>
        /// In production: upload course to server.
        /// For now: serializes to JSON for local storage.
        /// </summary>
        public string SerializeCourse(SharedCourseData course)
        {
            return JsonUtility.ToJson(course);
        }

        /// <summary>
        /// In production: download from server.
        /// For now: deserializes from JSON.
        /// </summary>
        public SharedCourseData DeserializeCourse(string json)
        {
            try
            {
                return JsonUtility.FromJson<SharedCourseData>(json);
            }
            catch (Exception e)
            {
                OnError?.Invoke($"Failed to load course: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convert downloaded course data into playable CourseData + HoleData.
        /// </summary>
        public Golf.CourseData ConvertToPlayable(SharedCourseData shared)
        {
            var courseData = ScriptableObject.CreateInstance<Golf.CourseData>();
            courseData.courseName = shared.courseName;
            courseData.ownerId = shared.ownerId;
            courseData.holes = new List<Golf.HoleData>();

            foreach (var sharedHole in shared.holes)
            {
                var holeData = ScriptableObject.CreateInstance<Golf.HoleData>();
                holeData.par = sharedHole.par;
                holeData.length = (HoleLength)sharedHole.lengthType;
                holeData.hazardSet = (HoleHazardSet)sharedHole.hazardSet;
                holeData.themeIndex = sharedHole.themeIndex;

                // Set yardage based on par/length
                holeData.yardage = (sharedHole.par, (HoleLength)sharedHole.lengthType) switch
                {
                    (3, HoleLength.Short) => 150f,
                    (4, HoleLength.Short) => 280f,
                    (4, HoleLength.Medium) => 350f,
                    (5, HoleLength.Medium) => 450f,
                    (5, HoleLength.Long) => 540f,
                    _ => 150f
                };

                courseData.holes.Add(holeData);
            }

            courseData.RecalculatePar();
            return courseData;
        }
    }
}
