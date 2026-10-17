using NUnit.Framework;
using Wigglebites.AI;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class CollectorBotBrainTests
    {
        [Test]
        public void ChooseHeading_TargetsClosestActiveFood()
        {
            var snake = new SnakeState(1, false, SimVector2.Zero, SimVector2.Right, 5f, 2f, 10f, 0.7f, 8);
            var food = new[]
            {
                new FoodState(1, new SimVector2(8f, 0f), 1f),
                new FoodState(2, new SimVector2(0f, 2f), 1f)
            };

            var heading = new CollectorBotBrain().ChooseHeading(snake, food);

            Assert.That(heading.X, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(heading.Y, Is.EqualTo(1f).Within(0.0001f));
        }
    }
}
