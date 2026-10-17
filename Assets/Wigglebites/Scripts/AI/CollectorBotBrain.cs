using System.Collections.Generic;
using Wigglebites.Simulation;

namespace Wigglebites.AI
{
    public sealed class CollectorBotBrain
    {
        public SimVector2 ChooseHeading(SnakeState snake, IReadOnlyList<FoodState> food)
        {
            FoodState closest = null;
            var closestDistanceSquared = float.MaxValue;

            for (var index = 0; index < food.Count; index++)
            {
                var candidate = food[index];
                if (!candidate.IsActive) continue;

                var offset = candidate.Position - snake.Position;
                if (offset.SqrMagnitude >= closestDistanceSquared) continue;

                closest = candidate;
                closestDistanceSquared = offset.SqrMagnitude;
            }

            return closest == null ? snake.Heading : (closest.Position - snake.Position).Normalized;
        }
    }
}
