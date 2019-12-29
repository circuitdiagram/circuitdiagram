using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Test.Definitions
{
    [TestFixture]
    public class ComponentPointWithDefinitionParserTest
    {
        [Test]
        public void TestParseValidDefinitionSeparateAxes()
        {
            var parser = CreateParser("SomeVariable", "10", "20");

            bool success = parser.TryParse("_Start+$SomeVariable", "_End-$SomeVariable", new FileRange(), new FileRange(), out var result);
            Assert.That(success, Is.True);

            Assert.That(result.RelativeToX, Is.EqualTo(ComponentPosition.Start));
            Assert.That(result.RelativeToY, Is.EqualTo(ComponentPosition.End));

            var offsets = result.Offsets.Cast<ComponentPointOffsetWithDefinition>();

            var xOffset = offsets.Single(x => x.Axis == Primitives.OffsetAxis.X);
            Assert.That(xOffset.Negative, Is.False);
            Assert.That(xOffset.Values.Count, Is.EqualTo(2));

            var yOffset = offsets.Single(x => x.Axis == Primitives.OffsetAxis.Y);
            Assert.That(yOffset.Negative, Is.True);
            Assert.That(yOffset.Values.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestParseValidDefinitionCombinedAxes()
        {
            var parser = CreateParser("SomeVariable", "10", "20");

            bool success = parser.TryParse("_Start+($SomeVariable)x-$(SomeVariable)y", new FileRange(), out var result);
            Assert.That(success, Is.True);

            Assert.That(result.RelativeToX, Is.EqualTo(ComponentPosition.Start));
            Assert.That(result.RelativeToY, Is.EqualTo(ComponentPosition.Start));

            var offsets = result.Offsets.Cast<ComponentPointOffsetWithDefinition>();

            var xOffset = offsets.Single(x => x.Axis == Primitives.OffsetAxis.X);
            Assert.That(xOffset.Negative, Is.False);
            Assert.That(xOffset.Values.Count, Is.EqualTo(2));

            var yOffset = offsets.Single(x => x.Axis == Primitives.OffsetAxis.Y);
            Assert.That(yOffset.Negative, Is.True);
            Assert.That(yOffset.Values.Count, Is.EqualTo(2));
        }

        private ComponentPointWithDefinitionParser CreateParser(
            string variableName,
            string horizontalValue,
            string verticalValue)
        {
            var definitionsSection = new DefinitionsSection(
                new Dictionary<string, ConditionalCollection<string>>
                {
                    [variableName] = new ConditionalCollection<string>
                    {
                        new Conditional<string>(horizontalValue, new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Truthy, new Circuit.PropertyValue(true))),
                        new Conditional<string>(verticalValue, new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Falsy, new Circuit.PropertyValue(true))),
                    }
                });

            var sectionRegistry = new Mock<ISectionRegistry>();
            sectionRegistry.Setup(x => x.GetSection<DefinitionsSection>())
                .Returns(definitionsSection);

            return new ComponentPointWithDefinitionParser(new NullXmlLoadLogger(), sectionRegistry.Object);
        }
    }
}
