using NUnit.Framework;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class SimulationTests
    {
        private static SimulationConfig CreateConfig(float arenaSize = 100f) => new SimulationConfig(
            arenaSize,
            baseSpeed: 5f,
            turnRateDegrees: 90f,
            startingMass: 10f,
            headRadius: 0.5f,
            foodPickupRadius: 0.5f,
            bodySampleSpacing: 0.7f,
            baseBodySegments: 8);

        [Test]
        public void Tick_AdvancesSnakeAtConfiguredSpeed()
        {
            var simulation = new GameSimulation(CreateConfig());
            var snake = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right);

            simulation.Tick(0.2f);

            Assert.That(snake.Position.X, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(snake.Position.Y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void Tick_LimitsHeadingChangeByTurnRate()
        {
            var simulation = new GameSimulation(CreateConfig());
            var snake = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right);
            simulation.SetDesiredHeading(1, new SimVector2(-1f, 0f));

            simulation.Tick(1f);

            Assert.That(snake.Heading.X, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(snake.Heading.Y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Tick_ConsumesNearbyFoodAndAddsMassOnce()
        {
            var simulation = new GameSimulation(CreateConfig());
            var snake = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right);
            var food = simulation.AddFood(1, new SimVector2(1f, 0f), 3f);

            simulation.Tick(0.2f);
            simulation.Tick(0.2f);

            Assert.That(food.IsActive, Is.False);
            Assert.That(snake.Mass, Is.EqualTo(13f).Within(0.0001f));
        }

        [Test]
        public void Tick_ReflectsSnakeAtArenaBoundary()
        {
            var simulation = new GameSimulation(CreateConfig(arenaSize: 4f));
            var snake = simulation.AddSnake(1, true, new SimVector2(1.4f, 0f), SimVector2.Right);

            simulation.Tick(0.2f);

            Assert.That(snake.Position.X, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(snake.Heading.X, Is.LessThan(0f));
        }
    }
}
