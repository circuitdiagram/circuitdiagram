using CircuitDiagram.Drawing.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers.RenderCommands;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    public class RectCommandWithDefinitionsReader : IRenderCommandReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IComponentPointParser componentPointParser;
        private readonly DefinitionsSection definitionsSection;

        public RectCommandWithDefinitionsReader(
            IXmlLoadLogger logger,
            IComponentPointParser componentPointParser,
            ISectionRegistry sectionRegistry)
        {
            this.logger = logger;
            this.componentPointParser = componentPointParser;
            definitionsSection = sectionRegistry.GetSection<DefinitionsSection>();
        }

        public bool ReadRenderCommand(XElement element, ComponentDescription description, out IXmlRenderCommand command)
        {
            var rectCommand = new XmlRectCommandWithDefinitions();
            command = rectCommand;

            if (element.Attribute("thickness") != null)
                rectCommand.StrokeThickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                rectCommand.Fill = true;

            if (element.Attribute("location") != null)
            {
                if (!componentPointParser.TryParse(element.Attribute("location"), out var location))
                    return false;
                rectCommand.Location = location;
            }
            else
            {
                var x = element.Attribute("x");
                var y = element.Attribute("y");
                if (!componentPointParser.TryParse(x, y, out var location))
                    return false;
                rectCommand.Location = location;
            }

            if (!TryReadDouble(element.Attribute("width"), out var width))
            {
                return false;
            }

            rectCommand.Width = width;
            rectCommand.Height = double.Parse(element.Attribute("height").Value);

            return true;
        }

        private bool TryReadDouble(XAttribute attr, out ConditionalCollection<double> result)
        {
            if (!attr.Value.StartsWith("$"))
            {
                if (double.TryParse(attr.Value, out var plainValue))
                {
                    result = new ConditionalCollection<double> { new Conditional<double>(plainValue, ConditionTree.Empty) };
                    return true;
                }

                result = null;
                return false;
            }

            // Check variable exists
            var variableName = attr.Value.Substring(1);
            if (!definitionsSection.Definitions.TryGetValue(variableName, out var variableValues))
            {
                logger.LogError(attr, $"Variable '{attr.Value}' does not exist");
                result = null;
                return false;
            }

            // Check all possible values are valid
            result = new ConditionalCollection<double>();
            foreach (var variableValue in variableValues)
            {
                if (!double.TryParse(variableValue.Value, out var parsedValue))
                {
                    logger.LogError(attr, $"Value '{attr.Value}' for ${variableName} is not a valid number");
                    return false;
                }

                result.Add(new Conditional<double>(parsedValue, variableValue.Conditions));
            }

            return true;
        }
    }
}
