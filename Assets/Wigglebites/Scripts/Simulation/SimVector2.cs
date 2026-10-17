using System;

namespace Wigglebites.Simulation
{
    public readonly struct SimVector2 : IEquatable<SimVector2>
    {
        public static readonly SimVector2 Zero = new SimVector2(0f, 0f);
        public static readonly SimVector2 Right = new SimVector2(1f, 0f);

        public SimVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
        public float SqrMagnitude => (X * X) + (Y * Y);
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public SimVector2 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                return magnitude > 0.00001f ? this / magnitude : Zero;
            }
        }

        public static float Dot(SimVector2 left, SimVector2 right) =>
            (left.X * right.X) + (left.Y * right.Y);

        public static float Cross(SimVector2 left, SimVector2 right) =>
            (left.X * right.Y) - (left.Y * right.X);

        public static float Distance(SimVector2 left, SimVector2 right) =>
            (left - right).Magnitude;

        public static SimVector2 Rotate(SimVector2 value, float radians)
        {
            var cosine = (float)Math.Cos(radians);
            var sine = (float)Math.Sin(radians);
            return new SimVector2(
                (value.X * cosine) - (value.Y * sine),
                (value.X * sine) + (value.Y * cosine));
        }

        public static SimVector2 operator +(SimVector2 left, SimVector2 right) =>
            new SimVector2(left.X + right.X, left.Y + right.Y);

        public static SimVector2 operator -(SimVector2 left, SimVector2 right) =>
            new SimVector2(left.X - right.X, left.Y - right.Y);

        public static SimVector2 operator *(SimVector2 value, float scalar) =>
            new SimVector2(value.X * scalar, value.Y * scalar);

        public static SimVector2 operator /(SimVector2 value, float scalar) =>
            new SimVector2(value.X / scalar, value.Y / scalar);

        public bool Equals(SimVector2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is SimVector2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X:0.###}, {Y:0.###})";
    }
}
