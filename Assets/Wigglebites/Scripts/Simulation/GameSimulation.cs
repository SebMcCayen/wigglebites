using System;
using System.Collections.Generic;

namespace Wigglebites.Simulation
{
    public sealed class GameSimulation
    {
        private readonly SimulationConfig _config;
        private readonly List<SnakeState> _snakes = new List<SnakeState>();
        private readonly List<FoodState> _food = new List<FoodState>();
        private readonly List<CombatContact> _combatContacts = new List<CombatContact>();
        private readonly HashSet<int> _pendingEliminations = new HashSet<int>();
        private int _nextFoodId = 1;

        public GameSimulation(SimulationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IReadOnlyList<SnakeState> Snakes => _snakes;
        public IReadOnlyList<FoodState> Food => _food;
        public float StartingMass => _config.StartingMass;
        public float CombatMassRatio => _config.CombatMassRatio;

        public SnakeState AddSnake(
            int id,
            bool isPlayer,
            SimVector2 position,
            SimVector2 heading,
            float? startingMass = null)
        {
            if (FindSnake(id) != null) throw new ArgumentException($"Snake id {id} already exists.", nameof(id));

            var snake = new SnakeState(
                id,
                isPlayer,
                position,
                heading,
                _config.BaseSpeed,
                _config.BoostSpeed,
                _config.BoostMassCostPerSecond,
                _config.MinimumBoostMass,
                _config.TurnRateRadians,
                startingMass ?? _config.StartingMass,
                _config.BodySampleSpacing,
                _config.BaseBodySegments);
            _snakes.Add(snake);
            return snake;
        }

        public FoodState AddFood(int id, SimVector2 position, float massValue)
        {
            for (var index = 0; index < _food.Count; index++)
            {
                if (_food[index].Id == id) throw new ArgumentException($"Food id {id} already exists.", nameof(id));
            }

            var food = new FoodState(id, position, massValue);
            _food.Add(food);
            _nextFoodId = Math.Max(_nextFoodId, id + 1);
            return food;
        }

        public SnakeState FindSnake(int id)
        {
            for (var index = 0; index < _snakes.Count; index++)
            {
                if (_snakes[index].Id == id) return _snakes[index];
            }

            return null;
        }

        public void SetDesiredHeading(int snakeId, SimVector2 heading)
        {
            var snake = FindSnake(snakeId);
            if (snake == null) throw new ArgumentException($"Unknown snake id {snakeId}.", nameof(snakeId));
            snake.SetDesiredHeading(heading);
        }

        public void SetBoostRequested(int snakeId, bool requested)
        {
            var snake = FindSnake(snakeId);
            if (snake == null) throw new ArgumentException($"Unknown snake id {snakeId}.", nameof(snakeId));
            snake.SetBoostRequested(requested);
        }

        public void RespawnSnake(int snakeId, SimVector2 position, SimVector2 heading)
        {
            var snake = FindSnake(snakeId);
            if (snake == null) throw new ArgumentException($"Unknown snake id {snakeId}.", nameof(snakeId));
            if (snake.IsAlive) throw new InvalidOperationException($"Snake {snakeId} is already alive.");
            if (!IsSpawnSafe(position, _config.StartingMass)) throw new InvalidOperationException("Respawn position is not safe.");

            snake.Respawn(position, heading, _config.StartingMass, _config.SpawnGraceSeconds);
        }

        public bool TryFindSafeSpawn(Random random, float candidateMass, out SimVector2 position, int maximumAttempts = 128)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (candidateMass <= 0f) throw new ArgumentOutOfRangeException(nameof(candidateMass));
            if (maximumAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maximumAttempts));

            var extent = (_config.ArenaSize * 0.5f) - _config.SafeSpawnRadius;
            for (var attempt = 0; attempt < maximumAttempts; attempt++)
            {
                var candidate = new SimVector2(
                    Lerp(-extent, extent, (float)random.NextDouble()),
                    Lerp(-extent, extent, (float)random.NextDouble()));
                if (!IsSpawnSafe(candidate, candidateMass)) continue;

                position = candidate;
                return true;
            }

            position = SimVector2.Zero;
            return false;
        }

        public bool IsSpawnSafe(SimVector2 position, float candidateMass)
        {
            var extent = (_config.ArenaSize * 0.5f) - _config.SafeSpawnRadius;
            if (Math.Abs(position.X) > extent || Math.Abs(position.Y) > extent) return false;

            for (var snakeIndex = 0; snakeIndex < _snakes.Count; snakeIndex++)
            {
                var snake = _snakes[snakeIndex];
                if (!snake.IsAlive || snake.Mass < candidateMass) continue;

                for (var segmentIndex = 0; segmentIndex < snake.BodySegmentCount; segmentIndex++)
                {
                    if (SimVector2.Distance(position, snake.GetBodyPosition(segmentIndex)) < _config.SafeSpawnRadius)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Tick(float deltaTime)
        {
            var halfExtent = (_config.ArenaSize * 0.5f) - _config.HeadRadius;
            for (var snakeIndex = 0; snakeIndex < _snakes.Count; snakeIndex++)
            {
                var snake = _snakes[snakeIndex];
                if (!snake.IsAlive) continue;
                snake.Tick(deltaTime);
                snake.ConstrainToArena(halfExtent);
                ConsumeNearbyFood(snake);
            }

            ResolveCombat();
        }

        private void ConsumeNearbyFood(SnakeState snake)
        {
            var pickupDistance = _config.HeadRadius + _config.FoodPickupRadius;
            for (var foodIndex = 0; foodIndex < _food.Count; foodIndex++)
            {
                var item = _food[foodIndex];
                if (!item.IsActive) continue;
                if (SimVector2.Distance(snake.Position, item.Position) > pickupDistance) continue;

                item.Consume();
                snake.AddMass(item.MassValue);
            }
        }

        private void ResolveCombat()
        {
            _combatContacts.Clear();
            _pendingEliminations.Clear();

            for (var leftIndex = 0; leftIndex < _snakes.Count; leftIndex++)
            {
                var left = _snakes[leftIndex];
                if (!left.IsAlive) continue;

                for (var rightIndex = leftIndex + 1; rightIndex < _snakes.Count; rightIndex++)
                {
                    var right = _snakes[rightIndex];
                    if (!right.IsAlive) continue;

                    var leftTouchesRight = TryGetContactPoint(left, right, out var rightContact);
                    var rightTouchesLeft = TryGetContactPoint(right, left, out var leftContact);

                    if (leftTouchesRight)
                    {
                        _combatContacts.Add(new CombatContact(
                            left,
                            right,
                            rightContact,
                            left.Mass >= right.Mass * _config.CombatMassRatio));
                    }

                    if (rightTouchesLeft)
                    {
                        _combatContacts.Add(new CombatContact(
                            right,
                            left,
                            leftContact,
                            right.Mass >= left.Mass * _config.CombatMassRatio));
                    }
                }
            }

            for (var index = 0; index < _combatContacts.Count; index++)
            {
                var contact = _combatContacts[index];
                if (contact.AttackerQualifies)
                {
                    _pendingEliminations.Add(contact.Defender.Id);
                }
            }

            for (var index = 0; index < _combatContacts.Count; index++)
            {
                var contact = _combatContacts[index];
                if (contact.AttackerQualifies) continue;
                contact.Attacker.ApplyDeflection(
                    contact.Attacker.Position - contact.ContactPoint,
                    _config.CollisionCooldownSeconds);
            }

            for (var index = 0; index < _snakes.Count; index++)
            {
                var snake = _snakes[index];
                if (!_pendingEliminations.Contains(snake.Id)) continue;

                snake.Eliminate();
                SpawnEliminationFood(snake);
            }
        }

        private bool TryGetContactPoint(SnakeState attacker, SnakeState defender, out SimVector2 contactPoint)
        {
            contactPoint = defender.Position;
            if (attacker.CollisionCooldownRemaining > 0f || attacker.IsCombatProtected || defender.IsCombatProtected) return false;

            for (var segmentIndex = 0; segmentIndex < defender.BodySegmentCount; segmentIndex++)
            {
                var segmentPosition = defender.GetBodyPosition(segmentIndex);
                var contactDistance = segmentIndex == 0
                    ? _config.HeadRadius * 2f
                    : _config.HeadRadius + _config.BodyRadius;
                if (SimVector2.Distance(attacker.Position, segmentPosition) > contactDistance) continue;

                contactPoint = segmentPosition;
                return true;
            }

            return false;
        }

        private static float Lerp(float minimum, float maximum, float amount) =>
            minimum + ((maximum - minimum) * amount);

        private void SpawnEliminationFood(SnakeState snake)
        {
            var remainingMass = snake.Mass * _config.EliminationDropFraction;
            var index = 0;
            var halfExtent = (_config.ArenaSize * 0.5f) - _config.FoodPickupRadius;

            while (remainingMass > 0.0001f)
            {
                var foodMass = Math.Min(_config.EliminationFoodMass, remainingMass);
                var angle = (snake.Id * 0.73f) + (index * 2.3999632f);
                var radius = 0.8f + (0.12f * (float)Math.Sqrt(index));
                var x = Math.Clamp(snake.Position.X + ((float)Math.Cos(angle) * radius), -halfExtent, halfExtent);
                var y = Math.Clamp(snake.Position.Y + ((float)Math.Sin(angle) * radius), -halfExtent, halfExtent);
                AddFood(_nextFoodId, new SimVector2(x, y), foodMass);
                remainingMass -= foodMass;
                index++;
            }
        }

        private readonly struct CombatContact
        {
            public CombatContact(
                SnakeState attacker,
                SnakeState defender,
                SimVector2 contactPoint,
                bool attackerQualifies)
            {
                Attacker = attacker;
                Defender = defender;
                ContactPoint = contactPoint;
                AttackerQualifies = attackerQualifies;
            }

            public SnakeState Attacker { get; }
            public SnakeState Defender { get; }
            public SimVector2 ContactPoint { get; }
            public bool AttackerQualifies { get; }
        }
    }
}
