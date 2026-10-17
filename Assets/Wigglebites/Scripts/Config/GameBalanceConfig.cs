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
        [SerializeField, Min(10f)] private float matchDurationSeconds = 180f;
        [SerializeField, Min(0.1f)] private float baseSpeed = 5f;
        [SerializeField, Min(0.1f)] private float boostSpeed = 8f;
        [SerializeField, Min(0f)] private float boostMassCostPerSecond = 2f;
        [SerializeField, Min(0f)] private float minimumBoostMass = 10f;
        [SerializeField, Range(10f, 720f)] private float turnRateDegrees = 180f;

        [Header("Snake")]
        [SerializeField, Min(1f)] private float startingMass = 10f;
        [SerializeField, Min(0.1f)] private float headRadius = 0.65f;
        [SerializeField, Min(0.1f)] private float bodyRadius = 0.5f;
        [SerializeField, Min(0.1f)] private float bodySampleSpacing = 0.7f;
        [SerializeField, Min(2)] private int baseBodySegments = 8;

        [Header("Food")]
        [SerializeField, Range(1, 500)] private int initialFoodCount = 350;
        [SerializeField, Range(1, 1000)] private int maximumFoodCount = 500;
        [SerializeField, Min(0.05f)] private float foodPickupRadius = 0.35f;
        [SerializeField, Min(0.1f)] private float minimumFoodMass = 1f;
        [SerializeField, Min(0.1f)] private float maximumFoodMass = 3f;
        [SerializeField, Min(0.5f)] private float spatialCellSize = 4f;

        [Header("Combat")]
        [SerializeField, Min(1f)] private float combatMassRatio = 1.1f;
        [SerializeField, Range(0f, 1f)] private float eliminationDropFraction = 0.5f;
        [SerializeField, Min(0.1f)] private float eliminationFoodMass = 2f;
        [SerializeField, Min(0f)] private float collisionCooldownSeconds = 0.25f;

        [Header("Bots and spawning")]
        [SerializeField, Range(1, 19)] private int botCount = 19;
        [SerializeField, Min(1f)] private float botPerceptionRadius = 24f;
        [SerializeField, Min(1f)] private float safeSpawnRadius = 6f;
        [SerializeField, Min(0f)] private float spawnGraceSeconds = 2f;
        [SerializeField, Min(0f)] private float botRespawnDelaySeconds = 3f;

        public float ArenaSize => arenaSize;
        public float SimulationStep => 1f / simulationRate;
        public int InitialFoodCount => initialFoodCount;
        public float MatchDurationSeconds => matchDurationSeconds;
        public float MinimumFoodMass => minimumFoodMass;
        public float MaximumFoodMass => maximumFoodMass;
        public int BotCount => botCount;
        public float BotPerceptionRadius => botPerceptionRadius;
        public float SpawnGraceSeconds => spawnGraceSeconds;
        public float BotRespawnDelaySeconds => botRespawnDelaySeconds;

        public SimulationConfig CreateSimulationConfig() => new SimulationConfig(
            arenaSize,
            baseSpeed,
            boostSpeed,
            boostMassCostPerSecond,
            minimumBoostMass,
            turnRateDegrees,
            startingMass,
            headRadius,
            bodyRadius,
            foodPickupRadius,
            bodySampleSpacing,
            baseBodySegments,
            combatMassRatio,
            eliminationDropFraction,
            eliminationFoodMass,
            collisionCooldownSeconds,
            safeSpawnRadius,
            spawnGraceSeconds,
            botRespawnDelaySeconds,
            maximumFoodCount,
            spatialCellSize);

        private void OnValidate()
        {
            boostSpeed = Mathf.Max(baseSpeed, boostSpeed);
            minimumBoostMass = Mathf.Min(startingMass, minimumBoostMass);
            maximumFoodMass = Mathf.Max(minimumFoodMass, maximumFoodMass);
            maximumFoodCount = Mathf.Max(initialFoodCount, maximumFoodCount);
            spatialCellSize = Mathf.Max(0.5f, spatialCellSize);
            safeSpawnRadius = Mathf.Max(
                headRadius * 2f,
                Mathf.Min(safeSpawnRadius, (arenaSize * 0.5f) - headRadius));
        }
    }
}
