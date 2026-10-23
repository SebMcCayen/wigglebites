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
        private readonly Dictionary<int, int> _eliminators = new Dictionary<int, int>();
        private readonly Dictionary<int, TailSeverAction> _pendingTailSevers = new Dictionary<int, TailSeverAction>();
        private readonly SpatialHash2D<FoodState> _foodIndex;
        private readonly SpatialHash2D<BodyPoint> _bodyIndex;
        private readonly List<FoodState> _nearbyFood = new List<FoodState>();
        private readonly List<BodyPoint> _nearbyBodyPoints = new List<BodyPoint>();
        private readonly Dictionary<ContactKey, BodyPoint> _contactPoints = new Dictionary<ContactKey, BodyPoint>();
        private int _foodDistanceChecks;
        private int _combatDistanceChecks;
        private int _nextFoodId = 1;

        public GameSimulation(SimulationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _foodIndex = new SpatialHash2D<FoodState>(_config.SpatialCellSize);
            _bodyIndex = new SpatialHash2D<BodyPoint>(_config.SpatialCellSize);
        }

        public IReadOnlyList<SnakeState> Snakes => _snakes;
        public IReadOnlyList<FoodState> Food => _food;
        public float StartingMass => _config.StartingMass;
        public float CombatMassRatio => _config.CombatMassRatio;
        public SimulationDiagnostics LastDiagnostics { get; private set; }

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
            if (_food.Count >= _config.MaximumFoodCount) throw new InvalidOperationException("The food pool is full.");

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
            if (!IsSpawnSafe(position, heading, _config.StartingMass)) throw new InvalidOperationException("Respawn position is not safe.");

            snake.Respawn(position, heading, _config.StartingMass, _config.SpawnGraceSeconds);
        }

        public bool TryFindSafeSpawn(
            Random random,
            float candidateMass,
            out SimVector2 position,
            out SimVector2 heading,
            int maximumAttempts = 128)
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
                var angle = (float)(random.NextDouble() * Math.PI * 2d);
                var candidateHeading = new SimVector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                if (!IsSpawnSafe(candidate, candidateHeading, candidateMass)) continue;

                position = candidate;
                heading = candidateHeading;
                return true;
            }

            position = SimVector2.Zero;
            heading = SimVector2.Right;
            return false;
        }

        public bool IsSpawnSafe(SimVector2 position, SimVector2 heading, float candidateMass)
        {
            if (heading.SqrMagnitude <= 0f) throw new ArgumentException("Heading must be non-zero.", nameof(heading));
            if (candidateMass <= 0f) throw new ArgumentOutOfRangeException(nameof(candidateMass));

            var normalizedHeading = heading.Normalized;
            var extent = (_config.ArenaSize * 0.5f) - _config.HeadRadius;
            var candidateSegmentCount = _config.BaseBodySegments + Math.Max(0, (int)(candidateMass / 10f));
            for (var candidateIndex = 0; candidateIndex < candidateSegmentCount; candidateIndex++)
            {
                var candidateSegment = position - (normalizedHeading * (_config.BodySampleSpacing * candidateIndex));
                if (Math.Abs(candidateSegment.X) > extent || Math.Abs(candidateSegment.Y) > extent) return false;

                for (var snakeIndex = 0; snakeIndex < _snakes.Count; snakeIndex++)
                {
                    var snake = _snakes[snakeIndex];
                    if (!snake.IsAlive || snake.Mass < candidateMass) continue;

                    for (var segmentIndex = 0; segmentIndex < snake.BodySegmentCount; segmentIndex++)
                    {
                        if (SimVector2.Distance(candidateSegment, snake.GetBodyPosition(segmentIndex)) < _config.SafeSpawnRadius)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            _foodDistanceChecks = 0;
            _combatDistanceChecks = 0;
            RebuildFoodIndex();
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
            LastDiagnostics = new SimulationDiagnostics(
                _foodIndex.Count,
                _bodyIndex.Count,
                _foodDistanceChecks,
                _combatDistanceChecks);
        }

        private void ConsumeNearbyFood(SnakeState snake)
        {
            var pickupDistance = _config.HeadRadius + _config.FoodPickupRadius;
            var pickupDistanceSquared = pickupDistance * pickupDistance;
            _foodIndex.Query(snake.Position, pickupDistance, _nearbyFood);
            for (var foodIndex = 0; foodIndex < _nearbyFood.Count; foodIndex++)
            {
                var item = _nearbyFood[foodIndex];
                if (!item.IsActive) continue;
                _foodDistanceChecks++;
                if ((snake.Position - item.Position).SqrMagnitude > pickupDistanceSquared) continue;

                item.Consume();
                snake.AddMass(item.MassValue);
            }
        }

        private void ResolveCombat()
        {
            _combatContacts.Clear();
            _pendingEliminations.Clear();
            _eliminators.Clear();
            _pendingTailSevers.Clear();
            BuildContactLookup();

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
                            MeetsCombatThreshold(left.Mass, right.Mass, _config.CombatMassRatio)));
                    }

                    if (rightTouchesLeft)
                    {
                        _combatContacts.Add(new CombatContact(
                            right,
                            left,
                            leftContact,
                            MeetsCombatThreshold(right.Mass, left.Mass, _config.CombatMassRatio)));
                    }
                }
            }

            for (var index = 0; index < _combatContacts.Count; index++)
            {
                var contact = _combatContacts[index];
                if (!contact.AttackerQualifies) continue;

                if (contact.ContactPoint.SegmentIndex == 0)
                {
                    _pendingEliminations.Add(contact.Defender.Id);
                    if (!_eliminators.TryGetValue(contact.Defender.Id, out var currentAttackerId) ||
                        contact.Attacker.Id < currentAttackerId)
                    {
                        _eliminators[contact.Defender.Id] = contact.Attacker.Id;
                    }
                    continue;
                }

                var candidate = new TailSeverAction(
                    contact.Attacker.Id,
                    contact.ContactPoint.SegmentIndex,
                    contact.ContactPoint.Position);
                if (!_pendingTailSevers.TryGetValue(contact.Defender.Id, out var currentSever) ||
                    candidate.SegmentIndex < currentSever.SegmentIndex ||
                    (candidate.SegmentIndex == currentSever.SegmentIndex && candidate.AttackerId < currentSever.AttackerId))
                {
                    _pendingTailSevers[contact.Defender.Id] = candidate;
                }
            }

            for (var index = 0; index < _combatContacts.Count; index++)
            {
                var contact = _combatContacts[index];
                if (contact.AttackerQualifies) continue;
                contact.Attacker.ApplyDeflection(
                    contact.Attacker.Position - contact.ContactPoint.Position,
                    _config.CollisionCooldownSeconds);
            }

            for (var index = 0; index < _snakes.Count; index++)
            {
                var snake = _snakes[index];
                if (!_pendingEliminations.Contains(snake.Id)) continue;

                if (_eliminators.TryGetValue(snake.Id, out var attackerId))
                {
                    FindSnake(attackerId)?.RecordElimination();
                }
                snake.Eliminate();
                SpawnEliminationFood(snake);
            }

            for (var index = 0; index < _snakes.Count; index++)
            {
                var snake = _snakes[index];
                if (!snake.IsAlive || _pendingEliminations.Contains(snake.Id)) continue;
                if (!_pendingTailSevers.TryGetValue(snake.Id, out var sever)) continue;

                SpawnSeveredTailFood(snake, sever.SegmentIndex);
                snake.ApplyCollisionCooldown(_config.CollisionCooldownSeconds);
                var attacker = FindSnake(sever.AttackerId);
                attacker?.ApplyDeflection(
                    attacker.Position - sever.ContactPosition,
                    _config.CollisionCooldownSeconds);
            }
        }

        private bool TryGetContactPoint(SnakeState attacker, SnakeState defender, out BodyPoint contactPoint)
        {
            contactPoint = new BodyPoint(defender, 0, defender.Position);
            if (!_contactPoints.TryGetValue(new ContactKey(attacker.Id, defender.Id), out var bodyPoint)) return false;
            contactPoint = bodyPoint;
            return true;
        }

        internal static bool MeetsCombatThreshold(float attackerMass, float defenderMass, float combatMassRatio)
        {
            var thresholdBits = BitConverter.SingleToInt32Bits(defenderMass * combatMassRatio);
            var threshold = BitConverter.Int32BitsToSingle(thresholdBits);
            return attackerMass >= threshold;
        }

        private void RebuildFoodIndex()
        {
            _foodIndex.Clear();
            for (var index = 0; index < _food.Count; index++)
            {
                var item = _food[index];
                if (item.IsActive) _foodIndex.Insert(item.Position, item);
            }
        }

        private void BuildContactLookup()
        {
            _bodyIndex.Clear();
            _contactPoints.Clear();
            for (var snakeIndex = 0; snakeIndex < _snakes.Count; snakeIndex++)
            {
                var snake = _snakes[snakeIndex];
                if (!snake.IsAlive) continue;
                for (var segmentIndex = 0; segmentIndex < snake.BodySegmentCount; segmentIndex++)
                {
                    var position = snake.GetBodyPosition(segmentIndex);
                    _bodyIndex.Insert(position, new BodyPoint(snake, segmentIndex, position));
                }
            }

            var maximumContactDistance = Math.Max(
                _config.HeadRadius * 2f,
                _config.HeadRadius + _config.BodyRadius);
            for (var attackerIndex = 0; attackerIndex < _snakes.Count; attackerIndex++)
            {
                var attacker = _snakes[attackerIndex];
                if (!attacker.IsAlive || attacker.CollisionCooldownRemaining > 0f || attacker.IsCombatProtected) continue;
                _bodyIndex.Query(attacker.Position, maximumContactDistance, _nearbyBodyPoints);
                for (var pointIndex = 0; pointIndex < _nearbyBodyPoints.Count; pointIndex++)
                {
                    var point = _nearbyBodyPoints[pointIndex];
                    if (point.Snake.Id == attacker.Id || point.Snake.IsCombatProtected) continue;

                    var contactDistance = point.SegmentIndex == 0
                        ? _config.HeadRadius * 2f
                        : _config.HeadRadius + _config.BodyRadius;
                    _combatDistanceChecks++;
                    if ((attacker.Position - point.Position).SqrMagnitude > contactDistance * contactDistance) continue;

                    var key = new ContactKey(attacker.Id, point.Snake.Id);
                    var pointDistanceSquared = (attacker.Position - point.Position).SqrMagnitude;
                    if (!_contactPoints.TryGetValue(key, out var current))
                    {
                        _contactPoints[key] = point;
                        continue;
                    }

                    var currentDistanceSquared = (attacker.Position - current.Position).SqrMagnitude;
                    if (pointDistanceSquared < currentDistanceSquared - 0.00001f ||
                        (Math.Abs(pointDistanceSquared - currentDistanceSquared) <= 0.00001f &&
                         point.SegmentIndex < current.SegmentIndex))
                    {
                        _contactPoints[key] = point;
                    }
                }
            }
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
                if (!TrySpawnFood(new SimVector2(x, y), foodMass)) break;
                remainingMass -= foodMass;
                index++;
            }
        }

        private void SpawnSeveredTailFood(SnakeState snake, int firstRemovedSegment)
        {
            var previousSegmentCount = snake.BodySegmentCount;
            if (firstRemovedSegment <= 0 || firstRemovedSegment >= previousSegmentCount) return;

            var droppedPositions = new SimVector2[previousSegmentCount - firstRemovedSegment];
            for (var index = 0; index < droppedPositions.Length; index++)
            {
                droppedPositions[index] = snake.GetBodyPosition(firstRemovedSegment + index);
            }

            var droppedMass = snake.SeverTailAtSegment(firstRemovedSegment);
            if (droppedMass <= 0f || droppedPositions.Length == 0) return;

            var candyMass = droppedMass / droppedPositions.Length;
            for (var index = 0; index < droppedPositions.Length; index++)
            {
                var angle = (snake.Id * 0.73f) + (index * 2.3999632f);
                var offset = new SimVector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 0.12f;
                if (!TrySpawnFood(droppedPositions[index] + offset, candyMass)) break;
            }
        }

        private bool TrySpawnFood(SimVector2 position, float massValue)
        {
            for (var index = 0; index < _food.Count; index++)
            {
                if (_food[index].IsActive) continue;
                _food[index].Reactivate(position, massValue);
                return true;
            }

            if (_food.Count >= _config.MaximumFoodCount) return false;
            AddFood(_nextFoodId, position, massValue);
            return true;
        }

        private readonly struct CombatContact
        {
            public CombatContact(
                SnakeState attacker,
                SnakeState defender,
                BodyPoint contactPoint,
                bool attackerQualifies)
            {
                Attacker = attacker;
                Defender = defender;
                ContactPoint = contactPoint;
                AttackerQualifies = attackerQualifies;
            }

            public SnakeState Attacker { get; }
            public SnakeState Defender { get; }
            public BodyPoint ContactPoint { get; }
            public bool AttackerQualifies { get; }
        }

        private readonly struct TailSeverAction
        {
            public TailSeverAction(int attackerId, int segmentIndex, SimVector2 contactPosition)
            {
                AttackerId = attackerId;
                SegmentIndex = segmentIndex;
                ContactPosition = contactPosition;
            }

            public int AttackerId { get; }
            public int SegmentIndex { get; }
            public SimVector2 ContactPosition { get; }
        }

        private readonly struct BodyPoint
        {
            public BodyPoint(SnakeState snake, int segmentIndex, SimVector2 position)
            {
                Snake = snake;
                SegmentIndex = segmentIndex;
                Position = position;
            }

            public SnakeState Snake { get; }
            public int SegmentIndex { get; }
            public SimVector2 Position { get; }
        }

        private readonly struct ContactKey : IEquatable<ContactKey>
        {
            public ContactKey(int attackerId, int defenderId)
            {
                AttackerId = attackerId;
                DefenderId = defenderId;
            }

            public int AttackerId { get; }
            public int DefenderId { get; }

            public bool Equals(ContactKey other) => AttackerId == other.AttackerId && DefenderId == other.DefenderId;
            public override bool Equals(object obj) => obj is ContactKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(AttackerId, DefenderId);
        }
    }
}
