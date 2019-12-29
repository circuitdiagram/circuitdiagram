using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    class ComponentPointWithDefinitionParser : ComponentPointParser
    {
        private readonly IXmlLoadLogger logger;
        private readonly DefinitionsSection definitionsSection;
        
        public ComponentPointWithDefinitionParser(IXmlLoadLogger logger, ISectionRegistry sectionRegistry)
            : base(logger)
        {
            this.logger = logger;
            definitionsSection = sectionRegistry.GetSection<DefinitionsSection>();
        }

        protected override bool TryParseOffset(string offset, OffsetAxis axis, FileRange range, out IXmlComponentPointOffset result)
        {
            if (!offset.Contains("$"))
            {
                // Does not contain a variable
                return base.TryParseOffset(offset, axis, range, out result);
            }

            offset = offset.Replace("(", "").Replace(")", "");

            var variableIndex = offset.IndexOf("$");
            var variableName = offset.Substring(variableIndex + 1);

            // Check variable exists
            if (!definitionsSection.Definitions.TryGetValue(variableName, out var variableValues))
            {
                logger.Log(LogLevel.Error, range, $"Variable '{variableName}' does not exist", null);
                result = null;
                return false;
            }

            // Check all possible values are valid
            var parsedValues = new ConditionalCollection<double>();
            foreach(var variableValue in variableValues)
            {
                if (!double.TryParse(variableValue.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    logger.Log(LogLevel.Error, range, $"Value '{variableValue.Value}' for ${variableName} is not a valid decimal", null);
                    result = null;
                    return false;
                }

                parsedValues.Add(new TypeDescription.Conditions.Conditional<double>(parsedValue, variableValue.Conditions));
            }

            result = new ComponentPointOffsetWithDefinition(offset.First() == '-', parsedValues, axis);
            return true;
        }
    }
}
