using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;

namespace GolfGame.Golf.Scoring
{
    public class ScorecardManager : MonoBehaviour
    {
        private List<int> strokesPerHole = new();
        private List<int> parsPerHole = new();
        private int currentHoleStrokes;

        public int CurrentHoleStrokes => currentHoleStrokes;
        public int TotalStrokes
        {
            get
            {
                int total = currentHoleStrokes;
                foreach (int s in strokesPerHole) total += s;
                return total;
            }
        }
        public int TotalPar
        {
            get
            {
                int total = 0;
                foreach (int p in parsPerHole) total += p;
                return total;
            }
        }
        public int HolesCompleted => strokesPerHole.Count;
        public IReadOnlyList<int> StrokesPerHole => strokesPerHole;
        public IReadOnlyList<int> ParsPerHole => parsPerHole;

        public void StartRound()
        {
            strokesPerHole.Clear();
            parsPerHole.Clear();
            currentHoleStrokes = 0;
        }

        public void StartHole(int par)
        {
            currentHoleStrokes = 0;
            parsPerHole.Add(par);
        }

        public void AddStroke()
        {
            currentHoleStrokes++;
        }

        public void AddPenaltyStroke(int count = 1)
        {
            currentHoleStrokes += count;
        }

        public ScoreResult CompleteHole()
        {
            int par = parsPerHole.Count > 0 ? parsPerHole[parsPerHole.Count - 1] : 3;
            int strokes = currentHoleStrokes;
            strokesPerHole.Add(strokes);

            var result = ScoreCalculator.GetHoleResult(strokes, par);

            EventBus.Publish(new HoleCompletedEvent
            {
                holeIndex = strokesPerHole.Count - 1,
                strokes = strokes,
                par = par
            });

            currentHoleStrokes = 0;
            return result;
        }

        public RoundResult CompleteRound()
        {
            var result = new RoundResult
            {
                strokesPerHole = new List<int>(strokesPerHole),
                parsPerHole = new List<int>(parsPerHole),
                totalStrokes = TotalStrokes,
                totalPar = TotalPar
            };

            EventBus.Publish(new RoundCompletedEvent
            {
                totalStrokes = result.totalStrokes,
                totalPar = result.totalPar,
                holeCount = strokesPerHole.Count
            });

            return result;
        }
    }
}
