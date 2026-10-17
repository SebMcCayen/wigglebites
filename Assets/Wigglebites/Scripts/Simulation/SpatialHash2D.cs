using System;
using System.Collections.Generic;

namespace Wigglebites.Simulation
{
    public sealed class SpatialHash2D<T>
    {
        private readonly float _cellSize;
        private readonly Dictionary<Cell, List<T>> _buckets = new Dictionary<Cell, List<T>>();

        public SpatialHash2D(float cellSize)
        {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _cellSize = cellSize;
        }

        public int Count { get; private set; }

        public void Clear()
        {
            foreach (var bucket in _buckets.Values) bucket.Clear();
            Count = 0;
        }

        public void Insert(SimVector2 position, T item)
        {
            var cell = CellFor(position);
            if (!_buckets.TryGetValue(cell, out var bucket))
            {
                bucket = new List<T>();
                _buckets.Add(cell, bucket);
            }

            bucket.Add(item);
            Count++;
        }

        public void Query(SimVector2 center, float radius, List<T> results)
        {
            if (radius < 0f) throw new ArgumentOutOfRangeException(nameof(radius));
            if (results == null) throw new ArgumentNullException(nameof(results));
            results.Clear();

            var minimum = CellFor(new SimVector2(center.X - radius, center.Y - radius));
            var maximum = CellFor(new SimVector2(center.X + radius, center.Y + radius));
            for (var y = minimum.Y; y <= maximum.Y; y++)
            {
                for (var x = minimum.X; x <= maximum.X; x++)
                {
                    if (!_buckets.TryGetValue(new Cell(x, y), out var bucket)) continue;
                    results.AddRange(bucket);
                }
            }
        }

        private Cell CellFor(SimVector2 position) => new Cell(
            (int)Math.Floor(position.X / _cellSize),
            (int)Math.Floor(position.Y / _cellSize));

        private readonly struct Cell : IEquatable<Cell>
        {
            public Cell(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public bool Equals(Cell other) => X == other.X && Y == other.Y;
            public override bool Equals(object obj) => obj is Cell other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(X, Y);
        }
    }
}
