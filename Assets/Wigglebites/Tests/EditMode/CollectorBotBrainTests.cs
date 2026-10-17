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
            var snake = new SnakeState(
                1,
                false,
                SimVector2.Zero,
                SimVector2.Right,
                baseSpeed: 5f,
                boostSpeed: 8f,
                boostMassCostPerSecond: 2f,
                minimumBoostMass: 10f,
                turnRateRadians: 2f,
                startingMass: 10f,
                sampleSpacing: 0.7f,
                baseBodySegments: 8);
            var food = new[]
            {
                new FoodState(1, new SimVector2(8f, 0f), 1f),
                new FoodState(2, new SimVector2(0f, 2f), 1f)
            };

            var heading = new CollectorBotBrain().ChooseHeading(snake, food);

            Assert.That(heading.X, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(heading.Y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Survivor_FleesLargerNearbySnake()
        {
            var survivor = CreateSnake(1, SimVector2.Zero, 10f);
            var threat = CreateSnake(2, new SimVector2(2f, 0f), 20f);

            var decision = new BotBrain().ChooseDecision(
                BotPersonality.Survivor,
                survivor,
                new[] { survivor, threat },
                new FoodState[0],
                20f);

            Assert.That(decision.Heading.X, Is.EqualTo(-1f).Within(0.0001f));
            Assert.That(decision.Heading.Y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(decision.Boost, Is.False);
        }

        [Test]
        public void Hunter_TargetsSmallerNearbySnake()
        {
            var hunter = CreateSnake(1, SimVector2.Zero, 20f);
            var prey = CreateSnake(2, new SimVector2(0f, 2f), 10f);

            var decision = new BotBrain().ChooseDecision(
                BotPersonality.Hunter,
                hunter,
                new[] { hunter, prey },
                new FoodState[0],
                20f);

            Assert.That(decision.Heading.X, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(decision.Heading.Y, Is.EqualTo(1f).Within(0.0001f));
        }

        private static SnakeState CreateSnake(int id, SimVector2 position, float mass) => new SnakeState(
            id,
            false,
            position,
            SimVector2.Right,
            baseSpeed: 5f,
            boostSpeed: 8f,
            boostMassCostPerSecond: 2f,
            minimumBoostMass: 10f,
            turnRateRadians: 2f,
            startingMass: mass,
            sampleSpacing: 0.7f,
            baseBodySegments: 8);
    }
}
