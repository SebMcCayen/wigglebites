using System;

namespace Wigglebites.Simulation
{
    public sealed class FoodState
    {
        public FoodState(int id, SimVector2 position, float massValue)
        {
            if (massValue <= 0f) throw new ArgumentOutOfRangeException(nameof(massValue));

            Id = id;
            Position = position;
            MassValue = massValue;
            IsActive = true;
        }

        public int Id { get; }
        public SimVector2 Position { get; private set; }
        public float MassValue { get; private set; }
        public bool IsActive { get; private set; }

        internal void Consume() => IsActive = false;

        internal void Reactivate(SimVector2 position, float massValue)
        {
            if (massValue <= 0f) throw new ArgumentOutOfRangeException(nameof(massValue));
            Position = position;
            MassValue = massValue;
            IsActive = true;
        }
    }
}
