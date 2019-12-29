using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Components.Description;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Connections;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    class ConnectionsSectionReader : IXmlSectionReader
    {
        private static readonly XName GroupElementName = XmlLoader.ComponentNamespace + "group";
        private static readonly XName GElementName = XmlLoader.ComponentNamespace + "g";
        private static readonly XName ConnectionElementName = XmlLoader.ComponentNamespace + "connection";

        private readonly IXmlLoadLogger logger;
        private readonly IConditionParser conditionParser;
        private readonly IComponentPointParser componentPointParser;
        private readonly IAutoRotateOptionsReader autoRotateOptionsReader;

        private int unnamedConnectionId = 0;

        public ConnectionsSectionReader(IXmlLoadLogger logger,
                                        IConditionParser conditionParser,
                                        IComponentPointParser componentPointParser,
                                        IAutoRotateOptionsReader autoRotateOptionsReader)
        {
            this.logger = logger;
            this.conditionParser = conditionParser;
            this.componentPointParser = componentPointParser;
            this.autoRotateOptionsReader = autoRotateOptionsReader;
        }

        public void ReadSection(XElement element, ComponentDescription description)
        {
            var groups = new List<XmlConnectionGroup>();
            var defaultGroup = new XmlConnectionGroup(ConditionTree.Empty);
            autoRotateOptionsReader.TrySetAutoRotateOptions(element, defaultGroup);

            groups.Add(defaultGroup);
            foreach (var child in element.Elements())
            {
                groups.AddRange(ReadElement(child, description, defaultGroup));
            }

            var flatGroups = groups.SelectMany(x => x.FlattenRoot(logger)).ToArray();
            description.Connections = flatGroups.GroupBy(x => ConditionsReducer.SimplifyConditions(x.Conditions)).Select(g => new ConnectionGroup(g.Key, g.SelectMany(x => x.Value).ToArray())).ToArray();
        }

        public virtual IEnumerable<XmlConnectionGroup> ReadElement(XElement element, ComponentDescription description, XmlConnectionGroup groupContext)
        {
            if (element.Name == GroupElementName || element.Name == GElementName)
            {
                return ReadConnectionGroup(description, element, groupContext);
            }
            else if (element.Name == ConnectionElementName)
            {
                if (ReadConnection(element, out var connection))
                {
                    groupContext.Value.Add(connection);
                }
                return Enumerable.Empty<XmlConnectionGroup>();
            }
            else
            {
                if (element.Name.Namespace == XmlLoader.ComponentNamespace)
                {
                    logger.LogWarning(element, $"Unknown connection element '{element.Name.LocalName}'");
                }
                return Enumerable.Empty<XmlConnectionGroup>();
            }
        }

        protected virtual IEnumerable<XmlConnectionGroup> ReadConnectionGroup(ComponentDescription description, XElement connectionGroupElement, XmlConnectionGroup parentGroup)
        {
            IConditionTreeItem conditionCollection = ConditionTree.Empty;
            var conditionsAttribute = connectionGroupElement.Attribute("conditions");
            if (conditionsAttribute != null)
            {
                if (!conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection))
                    yield break;
            }

            var connectionGroup = new XmlConnectionGroup(new ConditionTree(ConditionTree.ConditionOperator.AND, parentGroup.Conditions, conditionCollection));
            autoRotateOptionsReader.TrySetAutoRotateOptions(connectionGroupElement, parentGroup, connectionGroup);

            var childGroups = connectionGroupElement.Elements().SelectMany(x => ReadElement(x, description, connectionGroup));

            yield return connectionGroup;
            foreach (var child in childGroups)
                yield return child;
        }

        private bool ReadConnection(XElement connectionElement, out XmlConnectionDescription connection)
        {
            ConnectionEdge edge = ConnectionEdge.None;
            if (connectionElement.Attribute("edge") != null)
            {
                string edgeText = connectionElement.Attribute("edge").Value.ToLowerInvariant();
                if (edgeText == "start")
                    edge = ConnectionEdge.Start;
                else if (edgeText == "end")
                    edge = ConnectionEdge.End;
                else if (edgeText == "both")
                    edge = ConnectionEdge.Both;
            }
            string connectionName;
            if (connectionElement.Attribute("name") != null)
                connectionName = connectionElement.Attribute("name").Value;
            else
                connectionName = $"#{unnamedConnectionId++}";

            if (!componentPointParser.TryParse(connectionElement.Attribute("start"), out var start) ||
                !componentPointParser.TryParse(connectionElement.Attribute("end"), out var end))
            {
                connection = null;
                return false;
            }

            var edgeConditional = new ConditionalCollection<ConnectionEdge>();
            edgeConditional.Add(new Conditional<ConnectionEdge>(edge, ConditionTree.Empty));

            connection = new XmlConnectionDescription(
                connectionName,
                start,
                end,
                edgeConditional);

            return true;
        }
    }
}
