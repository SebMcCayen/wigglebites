using NUnit.Framework;
using Wigglebites.AI;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class BotRosterTests
    {
        [Test]
        public void SpawnInitialRoster_CreatesNineteenSafelySeparatedBotsWithExpectedMix()
        {
            var simulation = new GameSimulation(CreateConfig());
            simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right);
            var roster = new BotRosterController(simulation, 1337, 24f, 3f);

            roster.SpawnInitialRoster(19);

            Assert.That(roster.Count, Is.EqualTo(19));
            Assert.That(simulation.Snakes.Count, Is.EqualTo(20));
            Assert.That(CountPersonality(roster, BotPersonality.Collector), Is.EqualTo(8));
            Assert.That(CountPersonality(roster, BotPersonality.Hunter), Is.EqualTo(6));
            Assert.That(CountPersonality(roster, BotPersonality.Survivor), Is.EqualTo(4));
            Assert.That(CountPersonality(roster, BotPersonality.Aggressive), Is.EqualTo(1));

            for (var left = 0; left < simulation.Snakes.Count; left++)
            {
                for (var right = left + 1; right < simulation.Snakes.Count; right++)
                {
                    Assert.That(
                        SimVector2.Distance(simulation.Snakes[left].Position, simulation.Snakes[right].Position),
                        Is.GreaterThanOrEqualTo(6f));
                }
            }
        }

        [Test]
        public void Tick_RespawnsEliminatedBotWithCombatGraceAfterDelay()
        {
            var simulation = new GameSimulation(CreateConfig());
            var roster = new BotRosterController(simulation, 42, 24f, 0.1f);
            roster.SpawnInitialRoster(1);
            var bot = simulation.FindSnake(2);
            simulation.AddSnake(99, true, bot.Position, SimVector2.Right, startingMass: 100f);
            simulation.Tick(0.01f);
            Assert.That(bot.IsAlive, Is.False);

            roster.Tick(0.1f);
            Assert.That(bot.IsAlive, Is.False);
            roster.Tick(0.1f);

            Assert.That(bot.IsAlive, Is.True);
            Assert.That(bot.Mass, Is.EqualTo(10f));
            Assert.That(bot.IsCombatProtected, Is.True);
            Assert.That(SimVector2.Distance(bot.Position, simulation.FindSnake(99).Position), Is.GreaterThanOrEqualTo(6f));
        }

        [Test]
        public void Tick_CombatGracePreventsRespawnedBotFromAttackingOrBeingAttacked()
        {
            var simulation = new GameSimulation(CreateConfig());
            var roster = new BotRosterController(simulation, 42, 24f, 0f);
            roster.SpawnInitialRoster(1);
            var bot = simulation.FindSnake(2);
            simulation.AddSnake(99, true, bot.Position, SimVector2.Right, startingMass: 100f);
            simulation.Tick(0.01f);
            roster.Tick(0.01f);
            roster.Tick(0.01f);
            simulation.AddSnake(100, false, bot.Position, SimVector2.Right, startingMass: 5f);

            simulation.Tick(0.01f);

            Assert.That(bot.IsAlive, Is.True);
            Assert.That(simulation.FindSnake(100).IsAlive, Is.True);
        }

        private static int CountPersonality(BotRosterController roster, BotPersonality personality)
        {
            var count = 0;
            for (var snakeId = 2; snakeId <= 20; snakeId++)
            {
                if (roster.GetPersonality(snakeId) == personality) count++;
            }
            return count;
        }

        private static SimulationConfig CreateConfig() => new SimulationConfig(
            arenaSize: 100f,
            baseSpeed: 5f,
            boostSpeed: 8f,
            boostMassCostPerSecond: 2f,
            minimumBoostMass: 10f,
            turnRateDegrees: 180f,
            startingMass: 10f,
            headRadius: 0.65f,
            bodyRadius: 0.5f,
            foodPickupRadius: 0.35f,
            bodySampleSpacing: 0.7f,
            baseBodySegments: 8,
            combatMassRatio: 1.1f,
            eliminationDropFraction: 0.5f,
            eliminationFoodMass: 2f,
            collisionCooldownSeconds: 0.25f,
            safeSpawnRadius: 6f,
            spawnGraceSeconds: 2f,
            botRespawnDelaySeconds: 3f);
    }
}
