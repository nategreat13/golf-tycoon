using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Golf
{
    [CreateAssetMenu(fileName = "NewCourse", menuName = "Golf/Course Data")]
    public class CourseData : ScriptableObject
    {
        public string courseName;
        public string courseDescription;
        public List<HoleData> holes;
        public int themeIndex;

        [Header("Metadata")]
        public string ownerId;
        public int totalPar;
        public float difficultyRating; // 1-5

        public void RecalculatePar()
        {
            totalPar = 0;
            foreach (var hole in holes)
            {
                if (hole != null)
                    totalPar += hole.par;
            }
        }
    }
}
