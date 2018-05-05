using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Components.Description;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    class ConnectionsSectionReader : IXmlSectionReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IConditionParser conditionParser;
        private readonly IComponentPointParser componentPointParser;

        public ConnectionsSectionReader(IXmlLoadLogger logger,
                                        IConditionParser conditionParser,
                                        IComponentPointParser componentPointParser)
        {
            this.logger = logger;
            this.conditionParser = conditionParser;
            this.componentPointParser = componentPointParser;
        }

        public void ReadSection(XElement connectionsElement, ComponentDescription description)
        {
            List<ConnectionGroup> parsedConnectionGroups = new List<ConnectionGroup>();
            var connectionGroupNodes = connectionsElement.Elements(XmlLoader.ComponentNamespace + "group");
            foreach (var connectionGroupNode in connectionGroupNodes)
            {
                IConditionTreeItem conditionCollection = ConditionTree.Empty;
                List<ConnectionDescription> connections = new List<ConnectionDescription>();

                var conditionsAttribute = connectionGroupNode.Attribute("conditions");
                if (conditionsAttribute != null)
                {
                    if (!conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection))
                        continue;
                }

                foreach (var connectionNode in connectionGroupNode.Elements(XmlLoader.ComponentNamespace + "connection"))
                {
                    ConnectionEdge edge = ConnectionEdge.None;
                    if (connectionNode.Attribute("edge") != null)
                    {
                        string edgeText = connectionNode.Attribute("edge").Value.ToLowerInvariant();
                        if (edgeText == "start")
                            edge = ConnectionEdge.Start;
                        else if (edgeText == "end")
                            edge = ConnectionEdge.End;
                        else if (edgeText == "both")
                            edge = ConnectionEdge.Both;
                    }
                    string connectionName = "#";
                    if (connectionNode.Attribute("name") != null)
                        connectionName = connectionNode.Attribute("name").Value;

                    if (!componentPointParser.TryParse(connectionNode.Attribute("start"), out var start) ||
                        !componentPointParser.TryParse(connectionNode.Attribute("end"), out var end))
                        continue;

                    connections.Add(new ConnectionDescription(start, end, edge, connectionName));
                }

                parsedConnectionGroups.Add(new ConnectionGroup(conditionCollection, connections.ToArray()));
            }

            description.Connections = parsedConnectionGroups.ToArray();
        }
    }
}
