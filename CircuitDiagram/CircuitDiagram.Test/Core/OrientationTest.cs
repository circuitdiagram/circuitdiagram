using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CircuitDiagram.Test.Core
{
    [TestClass]
    public class OrientationTest
    {
        /// <summary>
        /// Tests that reversing a Horizontal orientation becomes Vertical,
        /// and that Vertical becomes Horizontal.
        /// </summary>
        [TestMethod]
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
