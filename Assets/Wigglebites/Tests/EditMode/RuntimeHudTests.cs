using NUnit.Framework;
using Wigglebites.Simulation;
using Wigglebites.UI;

namespace Wigglebites.Tests
{
    public sealed class RuntimeHudTests
    {
        [Test]
        public void TouchControls_AreContainedAndSeparatedOnPortraitScreen()
        {
            var layout = RuntimeHud.CalculateTouchControlLayout(1080f, 1920f, 82f);

            Assert.That(layout.Steering.xMin, Is.GreaterThanOrEqualTo(0f));
            Assert.That(layout.Boost.xMax, Is.LessThanOrEqualTo(1080f));
            Assert.That(layout.Steering.yMax, Is.LessThanOrEqualTo(1920f));
            Assert.That(layout.Boost.xMin, Is.GreaterThan(layout.Steering.xMax));
            Assert.That(layout.Steering.width, Is.GreaterThanOrEqualTo(82f));
            Assert.That(layout.Boost.height, Is.EqualTo(layout.Steering.height));
        }

        [Test]
        public void TouchControls_RemainUsableOnNarrowSafeArea()
        {
            var layout = RuntimeHud.CalculateTouchControlLayout(320f, 568f, 58f);

            Assert.That(layout.Steering.xMin, Is.GreaterThanOrEqualTo(0f));
            Assert.That(layout.Boost.xMax, Is.LessThanOrEqualTo(320f));
            Assert.That(layout.Boost.xMin, Is.GreaterThanOrEqualTo(layout.Steering.xMax));
            Assert.That(layout.Steering.width, Is.GreaterThanOrEqualTo(58f));
        }

        [TestCase(1f, 0f, "→")]
        [TestCase(0f, 1f, "↑")]
        [TestCase(-1f, 0f, "←")]
        [TestCase(0f, -1f, "↓")]
        [TestCase(1f, 1f, "↗")]
        public void HeadingArrow_CommunicatesDirection(float x, float y, string expected)
        {
            Assert.That(RuntimeHud.HeadingArrow(new SimVector2(x, y)), Is.EqualTo(expected));
        }
    }
}
