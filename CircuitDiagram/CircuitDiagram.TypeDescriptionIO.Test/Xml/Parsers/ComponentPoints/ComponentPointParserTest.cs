using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Test.Xml.Parsers.ComponentPoints
{
    [TestFixture]
    public class ComponentPointParserTest
    {
        protected IComponentPointParser _parser;

        [SetUp]
        public virtual void Setup()
        {
            _parser = new ComponentPointParser(new NullXmlLoadLogger());
        }

        [TestCase("_Start", "_Start", ComponentPosition.Start, 0, ComponentPosition.Start, 0)]
        [TestCase("_Start+5", "_Start+10", ComponentPosition.Start, 5, ComponentPosition.Start, 10)]
        [TestCase("_Middle+15", "_End+25", ComponentPosition.Middle, 15, ComponentPosition.End, 25)]
        [TestCase("_Start-5", "_Start-10", ComponentPosition.Start, -5, ComponentPosition.Start, -10)]
        public void TestParseSeparateAxes(string x, string y, ComponentPosition expectedRelativeToX, double expectedXOffset, ComponentPosition expectedRelativeToY, double expectedYOffset)
        {
            bool success = _parser.TryParse(x, y, new FileRange(), new FileRange(), out var result);
            Assert.That(success, Is.True);

            Assert.That(result.RelativeToX, Is.EqualTo(expectedRelativeToX));
            Assert.That(result.RelativeToY, Is.EqualTo(expectedRelativeToY));

            double xOffset = result.Offsets.Cast<XmlComponentPointOffset>().Where(x => x.Axis == OffsetAxis.X).Sum(x => x.Offset);
            double yOffset = result.Offsets.Cast<XmlComponentPointOffset>().Where(x => x.Axis == OffsetAxis.Y).Sum(x => x.Offset);

            Assert.That(xOffset, Is.EqualTo(expectedXOffset));
            Assert.That(yOffset, Is.EqualTo(expectedYOffset));
        }

        [TestCase("_Start", ComponentPosition.Start, 0, 0)]
        [TestCase("_Start+5x+10y", ComponentPosition.Start, 5, 10)]
        [TestCase("_Middle-25x+15y", ComponentPosition.Middle, -25, 15)]
        public void TestParseCombinedAxes(string input, ComponentPosition expectedRelativeTo, double expectedXOffset, double expectedYOffset)
        {
            bool success = _parser.TryParse(input, new FileRange(), out var result);
            Assert.That(success, Is.True);

            Assert.That(result.RelativeToX, Is.EqualTo(expectedRelativeTo));
            Assert.That(result.RelativeToY, Is.EqualTo(expectedRelativeTo));

            double xOffset = result.Offsets.Cast<XmlComponentPointOffset>().Where(x => x.Axis == OffsetAxis.X).Sum(x => x.Offset);
            double yOffset = result.Offsets.Cast<XmlComponentPointOffset>().Where(x => x.Axis == OffsetAxis.Y).Sum(x => x.Offset);

            Assert.That(xOffset, Is.EqualTo(expectedXOffset));
            Assert.That(yOffset, Is.EqualTo(expectedYOffset));
        }
    }
}
