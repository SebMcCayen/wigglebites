namespace Wigglebites.Simulation
{
    public readonly struct SimulationDiagnostics
    {
        public SimulationDiagnostics(
            int indexedFoodCount,
            int indexedBodyPointCount,
            int foodDistanceChecks,
            int combatDistanceChecks)
        {
            IndexedFoodCount = indexedFoodCount;
            IndexedBodyPointCount = indexedBodyPointCount;
            FoodDistanceChecks = foodDistanceChecks;
            CombatDistanceChecks = combatDistanceChecks;
        }

        public int IndexedFoodCount { get; }
        public int IndexedBodyPointCount { get; }
        public int FoodDistanceChecks { get; }
        public int CombatDistanceChecks { get; }
    }
}
