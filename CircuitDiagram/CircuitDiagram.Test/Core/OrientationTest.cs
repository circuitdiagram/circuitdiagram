using System;
using NUnit.Framework;

namespace CircuitDiagram.Test.Core
{
    [TestFixture]
    public class OrientationTest
    {
        /// <summary>
        /// Tests that reversing a Horizontal orientation becomes Vertical,
        /// and that Vertical becomes Horizontal.
        /// </summary>
        [Test]
        public void TestReverse()
        {
            // Horizontal to Vertical
            var horizontal = Orientation.Horizontal;
            var reversed = horizontal.Reverse();
            Assert.AreEqual(Orientation.Vertical, reversed);

            // Vertical to Horizontal
            var vertical = Orientation.Vertical;
            reversed = vertical.Reverse();
            Assert.AreEqual(Orientation.Horizontal, reversed);
        }
    }
}
