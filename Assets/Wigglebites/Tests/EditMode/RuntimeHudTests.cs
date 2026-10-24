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
            Assert.That(layout.Steering.xMax, Is.LessThanOrEqualTo(1080f * 0.6f));
            Assert.That(layout.Boost.xMin, Is.GreaterThanOrEqualTo(1080f * 0.6f));
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
            Assert.That(layout.Steering.xMax, Is.LessThanOrEqualTo(320f * 0.6f));
            Assert.That(layout.Boost.xMin, Is.GreaterThanOrEqualTo(320f * 0.6f));
            Assert.That(layout.Steering.width, Is.GreaterThanOrEqualTo(58f));
        }

        [Test]
        public void TouchControls_AreClampedInsideShortSafeArea()
        {
            var layout = RuntimeHud.CalculateTouchControlLayout(320f, 100f, 58f);

            Assert.That(layout.Steering.yMin, Is.GreaterThanOrEqualTo(0f));
            Assert.That(layout.Steering.yMax, Is.LessThanOrEqualTo(100f));
            Assert.That(layout.Boost.yMax, Is.LessThanOrEqualTo(100f));
        }

        [Test]
        public void MenuLayout_KeepsPrimaryActionFirstAndAllButtonsInsidePanel()
        {
            var layout = RuntimeHud.CalculateMenuLayout(1080f, 1920f, 82f);

            Assert.That(layout.Panel.Contains(layout.Play.min), Is.True);
            Assert.That(layout.Panel.Contains(layout.Play.max), Is.True);
            Assert.That(layout.Panel.Contains(layout.Effects.min), Is.True);
            Assert.That(layout.Panel.Contains(layout.Effects.max), Is.True);
            Assert.That(layout.Panel.Contains(layout.Sound.min), Is.True);
            Assert.That(layout.Panel.Contains(layout.Sound.max), Is.True);
            Assert.That(layout.Play.yMin, Is.LessThan(layout.Effects.yMin));
            Assert.That(layout.Play.height, Is.GreaterThanOrEqualTo(82f));
            Assert.That(layout.Effects.width, Is.EqualTo(layout.Sound.width));
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

        [TestCase(false, false, false, "TURBO\nHÅLL")]
        [TestCase(true, false, true, "TURBO\nSTARTAR")]
        [TestCase(true, true, true, "TURBO\nAKTIV")]
        [TestCase(true, false, false, "TURBO\nMER MASS")]
        public void BoostLabel_ReportsSimulationState(
            bool held,
            bool boosting,
            bool canBoost,
            string expected)
        {
            Assert.That(RuntimeHud.BoostLabel(held, boosting, canBoost), Is.EqualTo(expected));
        }
    }
}
