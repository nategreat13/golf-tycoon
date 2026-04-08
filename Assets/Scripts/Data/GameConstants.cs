namespace GolfGame.Data
{
    public static class GameConstants
    {
        // Course Progression Tiers
        public static readonly int[] CourseTiers = { 1, 3, 9, 18 };

        // Starting currency
        public const long StartingCurrency = 5000;

        // Construction Times (seconds)
        public const float HoleConstructionTime = 300f;      // 5 minutes
        public const float HoleUpgradeTime = 600f;           // 10 minutes
        public const float DrivingRangeUpgradeTime = 180f;   // 3 minutes
        public const float ExpansionTime = 900f;             // 15 minutes

        // Costs
        public const long HoleConstructionCost = 1000;
        public const long HoleUpgradeCostBase = 500;
        public const float HoleUpgradeCostMultiplier = 1.5f;
        public const long DrivingRangeUpgradeCost = 300;
        public const long Expansion1To3Cost = 5000;
        public const long Expansion3To9Cost = 25000;
        public const long Expansion9To18Cost = 100000;

        // Income
        public const float AIGolferIntervalSeconds = 60f;
        public const long AIGolferBaseIncome = 10;
        public const float RealPlayerIncomeMultiplier = 5f;
        public const float DrivingRangeBaseIncome = 5f;
        public const float DrivingRangeUpgradeMultiplier = 1.3f;
        public const float MaxOfflineHours = 8f;

        // Reputation
        public const int ReputationPerHoleBuilt = 50;
        public const int ReputationPerRoundPlayed = 10;
        public const int ReputationPerVisitorRound = 25;
        public const int ReputationForExpansion = 200;

        // Reputation thresholds for unlocks
        public const int Rep3HolesUnlock = 500;
        public const int Rep9HolesUnlock = 2000;
        public const int Rep18HolesUnlock = 8000;

        // Golf gameplay
        public const float PowerBarSpeed = 2.5f;
        public const float AccuracyBarSpeed = 4.0f;
        public const float MaxAccuracyDeviation = 15f;  // degrees
        public const float BallGroundFriction = 0.4f;
        public const float BallBounceRestitution = 0.3f;
        public const float GreenFriction = 0.7f;
        public const float BunkerPenaltyMultiplier = 0.5f;
        public const float BallStopSpeed = 0.05f;

        // Club defaults
        public const int MaxClubUpgradeLevel = 3;
    }
}
