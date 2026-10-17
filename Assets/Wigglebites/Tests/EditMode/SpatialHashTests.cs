using System.Collections.Generic;
using NUnit.Framework;
using Wigglebites.Simulation;

namespace Wigglebites.Tests
{
    public sealed class SpatialHashTests
    {
        [Test]
        public void Query_ReturnsItemsAcrossNegativeCellBoundaryWithoutDuplicates()
        {
            var index = new SpatialHash2D<int>(4f);
            var results = new List<int>();
            index.Insert(new SimVector2(-0.1f, 0f), 1);
            index.Insert(new SimVector2(0.1f, 0f), 2);
            index.Insert(new SimVector2(20f, 20f), 3);

            index.Query(SimVector2.Zero, 0.2f, results);

            CollectionAssert.AreEquivalent(new[] { 1, 2 }, results);
        }

        [Test]
        public void Clear_ReusesIndexWithoutReturningStaleItems()
        {
            var index = new SpatialHash2D<int>(2f);
            var results = new List<int>();
            index.Insert(SimVector2.Zero, 1);
            index.Clear();
            index.Insert(new SimVector2(5f, 5f), 2);

            index.Query(SimVector2.Zero, 1f, results);

            Assert.That(index.Count, Is.EqualTo(1));
            Assert.That(results, Is.Empty);
        }
    }
}
