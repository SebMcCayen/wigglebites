using System;

namespace Wigglebites.Simulation
{
    public sealed class SimulationConfig
    {
        public SimulationConfig(
            float arenaSize,
            float baseSpeed,
            float turnRateDegrees,
            float startingMass,
            float headRadius,
            float foodPickupRadius,
            float bodySampleSpacing,
            int baseBodySegments)
        {
            if (arenaSize <= 0f) throw new ArgumentOutOfRangeException(nameof(arenaSize));
            if (baseSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(baseSpeed));
            if (turnRateDegrees <= 0f) throw new ArgumentOutOfRangeException(nameof(turnRateDegrees));
            if (startingMass <= 0f) throw new ArgumentOutOfRangeException(nameof(startingMass));
            if (headRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(headRadius));
            if (foodPickupRadius < 0f) throw new ArgumentOutOfRangeException(nameof(foodPickupRadius));
            if (bodySampleSpacing <= 0f) throw new ArgumentOutOfRangeException(nameof(bodySampleSpacing));
            if (baseBodySegments < 2) throw new ArgumentOutOfRangeException(nameof(baseBodySegments));

            ArenaSize = arenaSize;
            BaseSpeed = baseSpeed;
            TurnRateRadians = turnRateDegrees * ((float)Math.PI / 180f);
            StartingMass = startingMass;
            HeadRadius = headRadius;
            FoodPickupRadius = foodPickupRadius;
            BodySampleSpacing = bodySampleSpacing;
            BaseBodySegments = baseBodySegments;
        }

        public float ArenaSize { get; }
        public float BaseSpeed { get; }
        public float TurnRateRadians { get; }
        public float StartingMass { get; }
        public float HeadRadius { get; }
        public float FoodPickupRadius { get; }
        public float BodySampleSpacing { get; }
        public int BaseBodySegments { get; }
    }
}
