using System;
using System.Collections.Generic;

namespace Wigglebites.Simulation
{
    public sealed class SnakeState
    {
        private readonly List<SimVector2> _path = new List<SimVector2>();
        private readonly float _sampleSpacing;
        private readonly int _baseBodySegments;
        private SimVector2 _desiredHeading;

        public SnakeState(
            int id,
            bool isPlayer,
            SimVector2 position,
            SimVector2 heading,
            float speed,
            float turnRateRadians,
            float startingMass,
            float sampleSpacing,
            int baseBodySegments)
        {
            if (heading.SqrMagnitude <= 0f) throw new ArgumentException("Heading must be non-zero.", nameof(heading));
            if (speed <= 0f) throw new ArgumentOutOfRangeException(nameof(speed));
            if (turnRateRadians <= 0f) throw new ArgumentOutOfRangeException(nameof(turnRateRadians));
            if (startingMass <= 0f) throw new ArgumentOutOfRangeException(nameof(startingMass));
            if (sampleSpacing <= 0f) throw new ArgumentOutOfRangeException(nameof(sampleSpacing));
            if (baseBodySegments < 2) throw new ArgumentOutOfRangeException(nameof(baseBodySegments));

            Id = id;
            IsPlayer = isPlayer;
            Position = position;
            Heading = heading.Normalized;
            _desiredHeading = Heading;
            Speed = speed;
            TurnRateRadians = turnRateRadians;
            Mass = startingMass;
            _sampleSpacing = sampleSpacing;
            _baseBodySegments = baseBodySegments;

            for (var index = 0; index < BodySegmentCount; index++)
            {
                _path.Add(position - (Heading * (_sampleSpacing * index)));
            }
        }

        public int Id { get; }
        public bool IsPlayer { get; }
        public SimVector2 Position { get; private set; }
        public SimVector2 Heading { get; private set; }
        public float Speed { get; }
        public float TurnRateRadians { get; }
        public float Mass { get; private set; }
        public int BodySegmentCount => _baseBodySegments + Math.Max(0, (int)(Mass / 10f));

        public void SetDesiredHeading(SimVector2 heading)
        {
            if (heading.SqrMagnitude > 0.00001f)
            {
                _desiredHeading = heading.Normalized;
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));

            var angle = (float)Math.Atan2(
                SimVector2.Cross(Heading, _desiredHeading),
                SimVector2.Dot(Heading, _desiredHeading));
            var maximumTurn = TurnRateRadians * deltaTime;
            var appliedTurn = Math.Clamp(angle, -maximumTurn, maximumTurn);

            Heading = SimVector2.Rotate(Heading, appliedTurn).Normalized;
            Position += Heading * (Speed * deltaTime);
            RecordPathSample();
        }

        public SimVector2 GetBodyPosition(int segmentIndex)
        {
            if (segmentIndex < 0) throw new ArgumentOutOfRangeException(nameof(segmentIndex));
            if (_path.Count == 0) return Position;

            var targetDistance = segmentIndex * _sampleSpacing;
            var traversed = 0f;

            for (var index = 1; index < _path.Count; index++)
            {
                var section = SimVector2.Distance(_path[index - 1], _path[index]);
                if (traversed + section >= targetDistance && section > 0.00001f)
                {
                    var amount = (targetDistance - traversed) / section;
                    return _path[index - 1] + ((_path[index] - _path[index - 1]) * amount);
                }

                traversed += section;
            }

            return _path[_path.Count - 1];
        }

        internal void AddMass(float amount)
        {
            if (amount <= 0f) throw new ArgumentOutOfRangeException(nameof(amount));
            Mass += amount;
        }

        internal void ConstrainToArena(float halfExtent)
        {
            var clampedX = Math.Clamp(Position.X, -halfExtent, halfExtent);
            var clampedY = Math.Clamp(Position.Y, -halfExtent, halfExtent);
            if (clampedX.Equals(Position.X) && clampedY.Equals(Position.Y)) return;

            var hitVerticalWall = !clampedX.Equals(Position.X);
            var hitHorizontalWall = !clampedY.Equals(Position.Y);
            Position = new SimVector2(clampedX, clampedY);
            var reflected = Heading;
            if (hitVerticalWall) reflected = new SimVector2(-reflected.X, reflected.Y);
            if (hitHorizontalWall) reflected = new SimVector2(reflected.X, -reflected.Y);
            Heading = reflected.Normalized;
            _desiredHeading = Heading;
        }

        private void RecordPathSample()
        {
            if (_path.Count == 0)
            {
                _path.Add(Position);
            }
            else if (SimVector2.Distance(Position, _path[0]) > 0.00001f)
            {
                _path.Insert(0, Position);
            }

            var maximumLength = (BodySegmentCount + 2) * _sampleSpacing;
            var traversed = 0f;
            for (var index = 1; index < _path.Count; index++)
            {
                traversed += SimVector2.Distance(_path[index - 1], _path[index]);
                if (traversed <= maximumLength) continue;

                _path.RemoveRange(index, _path.Count - index);
                break;
            }
        }
    }
}
