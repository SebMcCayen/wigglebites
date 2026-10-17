using UnityEngine;
using Wigglebites.Simulation;

namespace Wigglebites.Config
{
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Wigglebites/Game Balance")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Simulation")]
        [SerializeField, Min(1f)] private float arenaSize = 100f;
        [SerializeField, Min(1f)] private float simulationRate = 30f;
        [SerializeField, Min(0.1f)] private float baseSpeed = 5f;
        [SerializeField, Range(10f, 720f)] private float turnRateDegrees = 180f;

        [Header("Snake")]
        [SerializeField, Min(1f)] private float startingMass = 10f;
        [SerializeField, Min(0.1f)] private float headRadius = 0.65f;
        [SerializeField, Min(0.1f)] private float bodySampleSpacing = 0.7f;
        [SerializeField, Min(2)] private int baseBodySegments = 8;

        [Header("Food")]
        [SerializeField, Range(1, 500)] private int initialFoodCount = 80;
        [SerializeField, Min(0.05f)] private float foodPickupRadius = 0.35f;
        [SerializeField, Min(0.1f)] private float minimumFoodMass = 1f;
        [SerializeField, Min(0.1f)] private float maximumFoodMass = 3f;

        public float ArenaSize => arenaSize;
        public float SimulationStep => 1f / simulationRate;
        public int InitialFoodCount => initialFoodCount;
        public float MinimumFoodMass => minimumFoodMass;
        public float MaximumFoodMass => maximumFoodMass;

        public SimulationConfig CreateSimulationConfig() => new SimulationConfig(
            arenaSize,
            baseSpeed,
            turnRateDegrees,
            startingMass,
            headRadius,
            foodPickupRadius,
            bodySampleSpacing,
            baseBodySegments);

        private void OnValidate()
        {
            maximumFoodMass = Mathf.Max(minimumFoodMass, maximumFoodMass);
        }
    }
}
