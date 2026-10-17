using System;
using System.Collections.Generic;
using Wigglebites.Simulation;

namespace Wigglebites.AI
{
    public sealed class BotRosterController
    {
        private readonly GameSimulation _simulation;
        private readonly Random _random;
        private readonly BotBrain _brain = new BotBrain();
        private readonly List<BotAgent> _agents = new List<BotAgent>();
        private readonly float _perceptionRadius;
        private readonly float _respawnDelay;

        public BotRosterController(GameSimulation simulation, int randomSeed, float perceptionRadius, float respawnDelay)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            if (perceptionRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(perceptionRadius));
            if (respawnDelay < 0f) throw new ArgumentOutOfRangeException(nameof(respawnDelay));
            _random = new Random(randomSeed);
            _perceptionRadius = perceptionRadius;
            _respawnDelay = respawnDelay;
        }

        public int Count => _agents.Count;

        public BotPersonality GetPersonality(int snakeId)
        {
            for (var index = 0; index < _agents.Count; index++)
            {
                if (_agents[index].SnakeId == snakeId) return _agents[index].Personality;
            }
            throw new ArgumentException($"Snake {snakeId} is not in the bot roster.", nameof(snakeId));
        }

        public void SpawnInitialRoster(int botCount, int firstSnakeId = 2)
        {
            if (botCount <= 0) throw new ArgumentOutOfRangeException(nameof(botCount));
            if (_agents.Count > 0) throw new InvalidOperationException("The bot roster has already been spawned.");

            for (var index = 0; index < botCount; index++)
            {
                if (!_simulation.TryFindSafeSpawn(_random, _simulation.StartingMass, out var position))
                {
                    throw new InvalidOperationException($"Could not find a safe spawn for bot {index + 1}.");
                }

                var snakeId = firstSnakeId + index;
                _simulation.AddSnake(snakeId, false, position, CreateHeading());
                _agents.Add(new BotAgent(
                    snakeId,
                    PersonalityForRosterIndex(index, botCount),
                    DecisionInterval(snakeId),
                    InitialDecisionDelay(snakeId)));
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));

            for (var index = 0; index < _agents.Count; index++)
            {
                var agent = _agents[index];
                var snake = _simulation.FindSnake(agent.SnakeId);
                if (snake == null) throw new InvalidOperationException($"Bot snake {agent.SnakeId} is missing.");

                if (!snake.IsAlive)
                {
                    if (!agent.RespawnScheduled)
                    {
                        agent.RespawnScheduled = true;
                        agent.RespawnRemaining = _respawnDelay;
                        continue;
                    }

                    agent.RespawnRemaining -= deltaTime;
                    if (agent.RespawnRemaining <= 0f &&
                        _simulation.TryFindSafeSpawn(_random, _simulation.StartingMass, out var respawnPosition))
                    {
                        _simulation.RespawnSnake(agent.SnakeId, respawnPosition, CreateHeading());
                        agent.RespawnScheduled = false;
                        agent.DecisionRemaining = InitialDecisionDelay(agent.SnakeId);
                    }
                    continue;
                }

                agent.RespawnScheduled = false;
                agent.DecisionRemaining -= deltaTime;
                if (agent.DecisionRemaining > 0f) continue;

                var decision = _brain.ChooseDecision(
                    agent.Personality,
                    snake,
                    _simulation.Snakes,
                    _simulation.Food,
                    _perceptionRadius,
                    _simulation.CombatMassRatio);
                _simulation.SetDesiredHeading(agent.SnakeId, decision.Heading);
                _simulation.SetBoostRequested(agent.SnakeId, decision.Boost);
                agent.DecisionRemaining += agent.DecisionInterval;
                if (agent.DecisionRemaining <= 0f) agent.DecisionRemaining = agent.DecisionInterval;
            }
        }

        public static BotPersonality PersonalityForRosterIndex(int index, int botCount)
        {
            if (index < 0 || index >= botCount) throw new ArgumentOutOfRangeException(nameof(index));
            var collectorCount = (int)Math.Round(botCount * 0.4f, MidpointRounding.AwayFromZero);
            var hunterCount = (int)Math.Round(botCount * 0.3f, MidpointRounding.AwayFromZero);
            var survivorCount = (int)Math.Round(botCount * 0.2f, MidpointRounding.AwayFromZero);
            if (index < collectorCount) return BotPersonality.Collector;
            if (index < collectorCount + hunterCount) return BotPersonality.Hunter;
            if (index < collectorCount + hunterCount + survivorCount) return BotPersonality.Survivor;
            return BotPersonality.Aggressive;
        }

        private SimVector2 CreateHeading()
        {
            var angle = (float)(_random.NextDouble() * Math.PI * 2d);
            return new SimVector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private static float DecisionInterval(int snakeId) => 0.1f + (((snakeId * 37) % 101) / 1000f);

        private static float InitialDecisionDelay(int snakeId)
        {
            var interval = DecisionInterval(snakeId);
            return ((snakeId * 53) % 100) / 100f * interval;
        }

        private sealed class BotAgent
        {
            public BotAgent(int snakeId, BotPersonality personality, float decisionInterval, float decisionRemaining)
            {
                SnakeId = snakeId;
                Personality = personality;
                DecisionInterval = decisionInterval;
                DecisionRemaining = decisionRemaining;
            }

            public int SnakeId { get; }
            public BotPersonality Personality { get; }
            public float DecisionInterval { get; }
            public float DecisionRemaining { get; set; }
            public bool RespawnScheduled { get; set; }
            public float RespawnRemaining { get; set; }
        }
    }
}
