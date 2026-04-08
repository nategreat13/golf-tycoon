namespace GolfGame.Golf.Scoring
{
    public enum ScoreLabel
    {
        HoleInOne,
        Albatross,   // -3
        Eagle,       // -2
        Birdie,      // -1
        Par,         //  0
        Bogey,       // +1
        DoubleBogey, // +2
        TriplePlus   // +3 or worse
    }

    public struct ScoreResult
    {
        public int strokes;
        public int par;
        public int relativeToPar;
        public ScoreLabel label;
    }

    public static class ScoreCalculator
    {
        public static ScoreResult GetHoleResult(int strokes, int par)
        {
            int relative = strokes - par;
            ScoreLabel label;

            if (strokes == 1)
                label = ScoreLabel.HoleInOne;
            else if (relative <= -3)
                label = ScoreLabel.Albatross;
            else if (relative == -2)
                label = ScoreLabel.Eagle;
            else if (relative == -1)
                label = ScoreLabel.Birdie;
            else if (relative == 0)
                label = ScoreLabel.Par;
            else if (relative == 1)
                label = ScoreLabel.Bogey;
            else if (relative == 2)
                label = ScoreLabel.DoubleBogey;
            else
                label = ScoreLabel.TriplePlus;

            return new ScoreResult
            {
                strokes = strokes,
                par = par,
                relativeToPar = relative,
                label = label
            };
        }

        public static string GetLabelString(ScoreLabel label)
        {
            return label switch
            {
                ScoreLabel.HoleInOne => "Hole in One!",
                ScoreLabel.Albatross => "Albatross!",
                ScoreLabel.Eagle => "Eagle!",
                ScoreLabel.Birdie => "Birdie!",
                ScoreLabel.Par => "Par",
                ScoreLabel.Bogey => "Bogey",
                ScoreLabel.DoubleBogey => "Double Bogey",
                ScoreLabel.TriplePlus => "Triple Bogey+",
                _ => ""
            };
        }

        public static string GetRelativeString(int relativeToPar)
        {
            if (relativeToPar == 0) return "E";
            if (relativeToPar > 0) return $"+{relativeToPar}";
            return relativeToPar.ToString();
        }
    }
}
