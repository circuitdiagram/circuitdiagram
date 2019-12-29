// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    class DefinitionsSectionReader : IXmlSectionReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IConditionParser conditionParser;
        private readonly ISectionRegistry sectionRegistry;

        public DefinitionsSectionReader(IXmlLoadLogger logger, IConditionParser conditionParser, ISectionRegistry sectionRegistry)
        {
            this.logger = logger;
            this.conditionParser = conditionParser;
            this.sectionRegistry = sectionRegistry;
        }

        public void ReadSection(XElement element, ComponentDescription description)
        {
            var definitions = new Dictionary<string, ConditionalCollection<string>>();

            foreach (var definitionNode in element.Elements(XmlLoader.ComponentNamespace + "def"))
            {
                if (!definitionNode.GetAttributeValue("name", logger, out var name))
                {
                    continue;
                }

                var values = new ConditionalCollection<string>();
                foreach (var valueWhen in definitionNode.Elements(XmlLoader.ComponentNamespace + "when"))
                {
                    valueWhen.GetAttribute("conditions", logger, out var conditionsAttribute);
                    valueWhen.GetAttributeValue("value", logger, out var whenValue);
                    conditionParser.Parse(conditionsAttribute, description, logger, out var conditions);

                    values.Add(new Conditional<string>(whenValue, conditions));
                }

                definitions.Add(name, values);
            }

            foreach (var constNode in element.Elements(XmlLoader.ComponentNamespace + "const"))
            {
                if (!constNode.GetAttributeValue("name", logger, out var name) ||
                    !constNode.GetAttributeValue("value", logger, out var value))
                {
                    continue;
                }

                var values = new ConditionalCollection<string>();
                values.Add(new Conditional<string>(value, ConditionTree.Empty));

                definitions.Add(name, values);
            }

            sectionRegistry.RegisterSection(new DefinitionsSection(definitions));
        }
    }
}
