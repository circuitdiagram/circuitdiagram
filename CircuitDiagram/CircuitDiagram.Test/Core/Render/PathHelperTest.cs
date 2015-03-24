using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CircuitDiagram.Render.Path;
using System.Windows;

namespace CircuitDiagram.Test.Core.Render
{
    [TestClass]
    public class PathHelperTest
    {
        [TestMethod]
        public void TestParseCommands()
        {
            string input = "M 1,1 l 1,0 M 1,2 l 1,0";
            var path = PathHelper.ParseCommands(input);

            // Path should contain 4 elements
            Assert.AreEqual(4, path.Count);

            // Check each element

            Assert.AreEqual(CommandType.MoveTo, path[0].Type);
            Assert.AreEqual(new Point(1,1), path[0].End);

            Assert.AreEqual(CommandType.LineTo, path[1].Type);
            Assert.AreEqual(new Point(2, 1), path[1].End);

            Assert.AreEqual(CommandType.MoveTo, path[2].Type);
            Assert.AreEqual(new Point(1, 2), path[2].End);

            Assert.AreEqual(CommandType.LineTo, path[3].Type);
            Assert.AreEqual(new Point(2, 2), path[3].End);
        }
    }
}
