using System;
using CircuitDiagram.Render.Path;
using System.Windows;
using NUnit.Framework;

namespace CircuitDiagram.Test.Core.Render
{
    [TestFixture]
    public class PathTest
    {
        [Test]
        public void TestClosePath()
        {
            var c = new ClosePath();

            // Test Shorthand()
            Assert.AreEqual("Z", c.Shorthand(new Point(), new Point()));
        }

        [Test]
        public void TestCurveTo()
        {
            var c = new CurveTo(1, 2, 3, 4, 5, 6);
            var offset = new Point(7, 8);
            var previous = new Point(9, 10);

            // Test Shorthand()
            Assert.AreEqual("c -8,-8 -6,-6 -4,-4", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("c -8,-12 -6,-14 -4,-16", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("c -10,-8 -12,-6 -14,-4", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestEllipticalArcTo()
        {
            var c = new EllipticalArcTo(2, 2, 5, false, false, 1, 2);
            var offset = new Point(7, 8);
            var previous = new Point(9, 10);

            // Test Shorthand()
            Assert.AreEqual("A 2,2 5 0 0 8,10", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("A 2,2 5 0 1 8,6", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("A 2,2 5 0 1 6,10", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestLineTo()
        {
            var c = new LineTo(10, 11);
            var offset = new Point(1, 2);
            var previous = new Point(0, 0);

            // Test Shorthand()
            Assert.AreEqual("L 11,13", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("L 11,-9", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("L -9,13", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestMoveTo()
        {
            var c = new MoveTo(10, 11);
            var offset = new Point(1, 2);
            var previous = new Point(0, 0);

            // Test Shorthand()
            Assert.AreEqual("M 11,13", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("M 11,-9", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("M -9,13", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestQuadraticBezierCurveTo()
        {
            var c = new QuadraticBeizerCurveTo(1, 2, 3, 4);
            var offset = new Point(7, 8);
            var previous = new Point(9, 10);

            // Test Shorthand()
            Assert.AreEqual("Q 8,10 10,12", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("Q 8,6 10,4", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("Q 6,10 4,12", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestSmoothCurveTo()
        {
            var c = new SmoothCurveTo(1, 2, 3, 4);
            var offset = new Point(1, 2);
            var previous = new Point(1, 1);

            // Test Shorthand()
            Assert.AreEqual("S 1,2 3,4", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("S 1,-2 3,-4", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("S -1,2 -3,4", c.Flip(true).Shorthand(offset, previous));
        }

        [Test]
        public void TestSmoothQuadraticBezierCurveTo()
        {
            var c = new SmoothQuadraticBeizerCurveTo(3, 4);
            var offset = new Point(1, 2);
            var previous = new Point(1, 1);

            // Test Shorthand()
            Assert.AreEqual("T 3,4", c.Shorthand(offset, previous));

            // Test Flip()
            Assert.AreEqual("T 3,-4", c.Flip(false).Shorthand(offset, previous));
            Assert.AreEqual("T -3,4", c.Flip(true).Shorthand(offset, previous));
        }
    }
}
