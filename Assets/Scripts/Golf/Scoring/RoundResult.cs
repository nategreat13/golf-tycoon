using System.Collections.Generic;

namespace GolfGame.Golf.Scoring
{
    public class RoundResult
    {
        public List<int> strokesPerHole;
        public List<int> parsPerHole;
        public int totalStrokes;
        public int totalPar;

        public int RelativeToPar => totalStrokes - totalPar;

        public string GetSummaryString()
        {
            string relative = ScoreCalculator.GetRelativeString(RelativeToPar);
            return $"{totalStrokes} ({relative}) - {strokesPerHole.Count} holes";
        }

        public List<ScoreResult> GetHoleResults()
        {
            var results = new List<ScoreResult>();
            for (int i = 0; i < strokesPerHole.Count; i++)
            {
                results.Add(ScoreCalculator.GetHoleResult(strokesPerHole[i], parsPerHole[i]));
            }
            return results;
        }
    }
}
