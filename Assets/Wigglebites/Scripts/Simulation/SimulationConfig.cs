using System;

namespace Wigglebites.Simulation
{
    public sealed class SimulationConfig
    {
        public SimulationConfig(
            float arenaSize,
            float baseSpeed,
            float boostSpeed,
            float boostMassCostPerSecond,
            float minimumBoostMass,
            float turnRateDegrees,
            float startingMass,
            float headRadius,
            float bodyRadius,
            float foodPickupRadius,
            float bodySampleSpacing,
            int baseBodySegments,
            float combatMassRatio,
            float eliminationDropFraction,
            float eliminationFoodMass,
            float collisionCooldownSeconds,
            float safeSpawnRadius = 6f,
            float spawnGraceSeconds = 2f,
            float botRespawnDelaySeconds = 3f,
            int maximumFoodCount = 500)
        {
            if (arenaSize <= 0f) throw new ArgumentOutOfRangeException(nameof(arenaSize));
            if (baseSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(baseSpeed));
            if (boostSpeed < baseSpeed) throw new ArgumentOutOfRangeException(nameof(boostSpeed));
            if (boostMassCostPerSecond < 0f) throw new ArgumentOutOfRangeException(nameof(boostMassCostPerSecond));
            if (minimumBoostMass < 0f) throw new ArgumentOutOfRangeException(nameof(minimumBoostMass));
            if (turnRateDegrees <= 0f) throw new ArgumentOutOfRangeException(nameof(turnRateDegrees));
            if (startingMass <= 0f) throw new ArgumentOutOfRangeException(nameof(startingMass));
            if (headRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(headRadius));
            if (bodyRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(bodyRadius));
            if (arenaSize <= headRadius * 2f) throw new ArgumentOutOfRangeException(nameof(arenaSize));
            if (foodPickupRadius < 0f) throw new ArgumentOutOfRangeException(nameof(foodPickupRadius));
            if (bodySampleSpacing <= 0f) throw new ArgumentOutOfRangeException(nameof(bodySampleSpacing));
            if (baseBodySegments < 2) throw new ArgumentOutOfRangeException(nameof(baseBodySegments));
            if (combatMassRatio < 1f) throw new ArgumentOutOfRangeException(nameof(combatMassRatio));
            if (eliminationDropFraction < 0f || eliminationDropFraction > 1f) throw new ArgumentOutOfRangeException(nameof(eliminationDropFraction));
            if (eliminationFoodMass <= 0f) throw new ArgumentOutOfRangeException(nameof(eliminationFoodMass));
            if (collisionCooldownSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(collisionCooldownSeconds));
            if (safeSpawnRadius < headRadius * 2f) throw new ArgumentOutOfRangeException(nameof(safeSpawnRadius));
            if (safeSpawnRadius >= arenaSize * 0.5f) throw new ArgumentOutOfRangeException(nameof(safeSpawnRadius));
            if (spawnGraceSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(spawnGraceSeconds));
            if (botRespawnDelaySeconds < 0f) throw new ArgumentOutOfRangeException(nameof(botRespawnDelaySeconds));
            if (maximumFoodCount <= 0) throw new ArgumentOutOfRangeException(nameof(maximumFoodCount));

            ArenaSize = arenaSize;
            BaseSpeed = baseSpeed;
            BoostSpeed = boostSpeed;
            BoostMassCostPerSecond = boostMassCostPerSecond;
            MinimumBoostMass = minimumBoostMass;
            TurnRateRadians = turnRateDegrees * ((float)Math.PI / 180f);
            StartingMass = startingMass;
            HeadRadius = headRadius;
            BodyRadius = bodyRadius;
            FoodPickupRadius = foodPickupRadius;
            BodySampleSpacing = bodySampleSpacing;
            BaseBodySegments = baseBodySegments;
            CombatMassRatio = combatMassRatio;
            EliminationDropFraction = eliminationDropFraction;
            EliminationFoodMass = eliminationFoodMass;
            CollisionCooldownSeconds = collisionCooldownSeconds;
            SafeSpawnRadius = safeSpawnRadius;
            SpawnGraceSeconds = spawnGraceSeconds;
            BotRespawnDelaySeconds = botRespawnDelaySeconds;
            MaximumFoodCount = maximumFoodCount;
        }

        public float ArenaSize { get; }
        public float BaseSpeed { get; }
        public float BoostSpeed { get; }
        public float BoostMassCostPerSecond { get; }
        public float MinimumBoostMass { get; }
        public float TurnRateRadians { get; }
        public float StartingMass { get; }
        public float HeadRadius { get; }
        public float BodyRadius { get; }
        public float FoodPickupRadius { get; }
        public float BodySampleSpacing { get; }
        public int BaseBodySegments { get; }
        public float CombatMassRatio { get; }
        public float EliminationDropFraction { get; }
        public float EliminationFoodMass { get; }
        public float CollisionCooldownSeconds { get; }
        public float SafeSpawnRadius { get; }
        public float SpawnGraceSeconds { get; }
        public float BotRespawnDelaySeconds { get; }
        public int MaximumFoodCount { get; }
    }
}
