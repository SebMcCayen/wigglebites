using System;
using System.Collections.Generic;
using Wigglebites.Simulation;

namespace Wigglebites.AI
{
    public readonly struct BotDecision
    {
        public BotDecision(SimVector2 heading, bool boost)
        {
            Heading = heading;
            Boost = boost;
        }

        public SimVector2 Heading { get; }
        public bool Boost { get; }
    }

    public sealed class BotBrain
    {
        public BotDecision ChooseDecision(
            BotPersonality personality,
            SnakeState snake,
            IReadOnlyList<SnakeState> snakes,
            IReadOnlyList<FoodState> food,
            float perceptionRadius,
            float combatMassRatio = 1.1f)
        {
            if (snake == null) throw new ArgumentNullException(nameof(snake));
            if (snakes == null) throw new ArgumentNullException(nameof(snakes));
            if (food == null) throw new ArgumentNullException(nameof(food));
            if (perceptionRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(perceptionRadius));
            if (combatMassRatio < 1f) throw new ArgumentOutOfRangeException(nameof(combatMassRatio));

            var foodHeading = FindClosestFoodHeading(snake, food, perceptionRadius, out _);
            switch (personality)
            {
                case BotPersonality.Hunter:
                    if (TryFindSnakeHeading(snake, snakes, perceptionRadius, combatMassRatio, BotPersonality.Hunter, out var preyHeading, out var preyDistance))
                    {
                        return new BotDecision(preyHeading, ShouldBoost(snake, preyHeading, preyDistance, perceptionRadius));
                    }
                    return new BotDecision(foodHeading, false);

                case BotPersonality.Survivor:
                    if (TryFindSnakeHeading(snake, snakes, perceptionRadius * 0.65f, combatMassRatio, BotPersonality.Survivor, out var threatHeading, out _))
                    {
                        return new BotDecision(new SimVector2(-threatHeading.X, -threatHeading.Y), false);
                    }
                    return new BotDecision(foodHeading, false);

                case BotPersonality.Aggressive:
                    if (TryFindSnakeHeading(snake, snakes, perceptionRadius, combatMassRatio, BotPersonality.Aggressive, out var opponentHeading, out var opponentDistance))
                    {
                        return new BotDecision(opponentHeading, ShouldBoost(snake, opponentHeading, opponentDistance, perceptionRadius));
                    }
                    return new BotDecision(foodHeading, false);

                default:
                    return new BotDecision(foodHeading, false);
            }
        }

        private static SimVector2 FindClosestFoodHeading(
            SnakeState snake,
            IReadOnlyList<FoodState> food,
            float perceptionRadius,
            out float distance)
        {
            FoodState closest = null;
            var closestDistanceSquared = perceptionRadius * perceptionRadius;
            for (var index = 0; index < food.Count; index++)
            {
                var candidate = food[index];
                if (!candidate.IsActive) continue;
                var distanceSquared = (candidate.Position - snake.Position).SqrMagnitude;
                if (distanceSquared >= closestDistanceSquared) continue;
                closest = candidate;
                closestDistanceSquared = distanceSquared;
            }

            if (closest == null)
            {
                distance = perceptionRadius;
                var angle = snake.Id * 2.3999632f;
                return new SimVector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }

            distance = (float)Math.Sqrt(closestDistanceSquared);
            return (closest.Position - snake.Position).Normalized;
        }

        private static bool TryFindSnakeHeading(
            SnakeState snake,
            IReadOnlyList<SnakeState> snakes,
            float perceptionRadius,
            float combatMassRatio,
            BotPersonality personality,
            out SimVector2 heading,
            out float distance)
        {
            SnakeState closest = null;
            var closestDistanceSquared = perceptionRadius * perceptionRadius;
            for (var index = 0; index < snakes.Count; index++)
            {
                var candidate = snakes[index];
                if (!candidate.IsAlive || candidate.Id == snake.Id || !MatchesTarget(personality, snake, candidate, combatMassRatio)) continue;
                var distanceSquared = (candidate.Position - snake.Position).SqrMagnitude;
                if (distanceSquared >= closestDistanceSquared) continue;
                closest = candidate;
                closestDistanceSquared = distanceSquared;
            }

            if (closest == null)
            {
                heading = snake.Heading;
                distance = perceptionRadius;
                return false;
            }

            heading = (closest.Position - snake.Position).Normalized;
            distance = (float)Math.Sqrt(closestDistanceSquared);
            return true;
        }

        private static bool MatchesTarget(
            BotPersonality personality,
            SnakeState self,
            SnakeState candidate,
            float combatMassRatio)
        {
            if (personality == BotPersonality.Hunter)
                return GameSimulation.MeetsCombatThreshold(self.Mass, candidate.Mass, combatMassRatio);
            if (personality == BotPersonality.Survivor)
                return GameSimulation.MeetsCombatThreshold(candidate.Mass, self.Mass, combatMassRatio);
            return true;
        }

        private static bool ShouldBoost(SnakeState snake, SimVector2 targetHeading, float distance, float perceptionRadius) =>
            snake.Mass > snake.MinimumBoostMass + 1f &&
            distance < perceptionRadius * 0.6f &&
            SimVector2.Dot(snake.Heading, targetHeading) > 0.75f;
    }
}
