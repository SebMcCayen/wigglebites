using NUnit.Framework;
using Wigglebites.Application;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class MatchControllerTests
    {
        [Test]
        public void Advance_EndsAtDurationAndRanksByMassEliminationsThenId()
        {
            var simulation = new GameSimulation(CreateConfig());
            simulation.AddSnake(1, true, new SimVector2(-30f, 0f), SimVector2.Right, startingMass: 10f);
            var leadingBot = simulation.AddSnake(2, false, new SimVector2(20f, 0f), SimVector2.Right, startingMass: 11f);
            simulation.AddSnake(3, false, new SimVector2(20f, 0f), SimVector2.Right, startingMass: 10f);
            simulation.AddSnake(4, false, new SimVector2(-20f, 0f), SimVector2.Right, startingMass: 11f);
            simulation.Tick(0.01f);
            Assert.That(leadingBot.Eliminations, Is.EqualTo(1));
            var match = new MatchController(simulation, 1, 180f);
            match.Start();

            match.Advance(180f);

            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Results));
            Assert.That(match.RemainingSeconds, Is.EqualTo(0f));
            Assert.That(match.Standings[0].SnakeId, Is.EqualTo(2));
            Assert.That(match.Standings[1].SnakeId, Is.EqualTo(4));
            Assert.That(match.PlayerRank, Is.EqualTo(3));
        }

        [Test]
        public void Advance_EndsImmediatelyWhenPlayerIsEliminated()
        {
            var simulation = new GameSimulation(CreateConfig());
            simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right, startingMass: 10f);
            simulation.AddSnake(2, false, SimVector2.Zero, SimVector2.Right, startingMass: 20f);
            var match = new MatchController(simulation, 1, 180f);
            match.Start();
            simulation.Tick(0.01f);

            match.Advance(0.01f);

            Assert.That(match.Phase, Is.EqualTo(MatchPhase.Results));
            Assert.That(match.FinalPlayerMass, Is.EqualTo(10f));
        }

        [Test]
        public void PauseAndResume_FreezeAndRestoreCountdown()
        {
            var simulation = new GameSimulation(CreateConfig());
            simulation.AddSnake(1, true, SimVector2.Zero, SimVector2.Right);
            var match = new MatchController(simulation, 1, 180f);
            match.Start();
            match.Pause();

            match.Advance(10f);
            Assert.That(match.RemainingSeconds, Is.EqualTo(180f));

            match.Resume();
            match.Advance(1f);
            Assert.That(match.RemainingSeconds, Is.EqualTo(179f));
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
            collisionCooldownSeconds: 0.25f);
    }
}
