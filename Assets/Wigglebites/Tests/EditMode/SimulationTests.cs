using NUnit.Framework;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class SimulationTests
    {
        private static SimulationConfig CreateConfig(float arenaSize = 100f) => new SimulationConfig(
            arenaSize,
            baseSpeed: 5f,
            boostSpeed: 8f,
            boostMassCostPerSecond: 2f,
            minimumBoostMass: 10f,
            turnRateDegrees: 90f,
            startingMass: 10f,
            headRadius: 0.5f,
            bodyRadius: 0.45f,
            foodPickupRadius: 0.5f,
            bodySampleSpacing: 0.7f,
            baseBodySegments: 8,
            combatMassRatio: 1.1f,
            eliminationDropFraction: 0.5f,
            eliminationFoodMass: 2f,
            collisionCooldownSeconds: 0.25f);

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
            Assert.That(snake.GetBodyPosition(0).X, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(snake.Heading.X, Is.LessThan(0f));
        }

        [Test]
        public void Tick_BoostsAndDrainsMassWithoutCrossingFloor()
        {
            var simulation = new GameSimulation(CreateConfig());
            var snake = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right, startingMass: 12f);
            simulation.SetBoostRequested(1, true);

            simulation.Tick(1f);
            simulation.Tick(1f);

            Assert.That(snake.Position.X, Is.EqualTo(13f).Within(0.0001f));
            Assert.That(snake.Mass, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(snake.IsBoosting, Is.False);
        }

        [Test]
        public void Tick_EliminatesDefenderAtExactCombatThresholdAndDropsMass()
        {
            var simulation = new GameSimulation(CreateConfig());
            var attacker = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right, startingMass: 11f);
            var defender = simulation.AddSnake(2, false, SimVector2.Zero, new SimVector2(-1f, 0f), startingMass: 10f);

            simulation.Tick(0.01f);

            Assert.That(attacker.IsAlive, Is.True);
            Assert.That(defender.IsAlive, Is.False);
            Assert.That(simulation.Food.Count, Is.EqualTo(3));

            var droppedMass = 0f;
            for (var index = 0; index < simulation.Food.Count; index++)
            {
                droppedMass += simulation.Food[index].MassValue;
            }
            Assert.That(droppedMass, Is.EqualTo(5f).Within(0.0001f));
        }

        [Test]
        public void Tick_DeflectsBothSnakesWhenNeitherMeetsCombatThreshold()
        {
            var simulation = new GameSimulation(CreateConfig());
            var left = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right, startingMass: 10.9f);
            var right = simulation.AddSnake(2, false, SimVector2.Zero, new SimVector2(-1f, 0f), startingMass: 10f);

            simulation.Tick(0.01f);

            Assert.That(left.IsAlive, Is.True);
            Assert.That(right.IsAlive, Is.True);
            Assert.That(left.CollisionCooldownRemaining, Is.GreaterThan(0f));
            Assert.That(right.CollisionCooldownRemaining, Is.GreaterThan(0f));
        }

        [Test]
        public void Tick_UsesOneContactSnapshotAcrossThreeSnakes()
        {
            var simulation = new GameSimulation(CreateConfig());
            var first = simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right, startingMass: 11f);
            var second = simulation.AddSnake(2, false, SimVector2.Zero, SimVector2.Right, startingMass: 10.5f);
            var third = simulation.AddSnake(3, false, SimVector2.Zero, SimVector2.Right, startingMass: 10f);

            simulation.Tick(0.01f);

            Assert.That(first.IsAlive, Is.True);
            Assert.That(second.IsAlive, Is.True);
            Assert.That(third.IsAlive, Is.False, "The first snake must still resolve its later qualifying contact.");
        }
    }
}
