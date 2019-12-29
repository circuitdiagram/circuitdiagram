// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2019  Samuel Fisher
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
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render.Path;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescriptionIO.Xml.Readers.RenderCommands;
using Autofac.Features.Indexed;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    public class RenderSectionReader : IXmlSectionReader
    {
        private static readonly XName GroupElementName = XmlLoader.ComponentNamespace + "group";
        private static readonly XName GElementName = XmlLoader.ComponentNamespace + "g";
        private static readonly XName LineElementName = XmlLoader.ComponentNamespace + "line";
        private static readonly XName RectElementName = XmlLoader.ComponentNamespace + "rect";
        private static readonly XName EllipseElementName = XmlLoader.ComponentNamespace + "ellipse";
        private static readonly XName PathElementName = XmlLoader.ComponentNamespace + "path";
        private static readonly XName TextElementName = XmlLoader.ComponentNamespace + "text";

        private readonly IXmlLoadLogger logger;
        private readonly IConditionParser conditionParser;
        private readonly IComponentPointParser componentPointParser;
        private readonly IAutoRotateOptionsReader autoRotateOptionsReader;
        private readonly IIndex<string, IRenderCommandReader> renderCommandReaders;

        public RenderSectionReader(
            IXmlLoadLogger logger,
            IConditionParser conditionParser,
            IComponentPointParser componentPointParser,
            IAutoRotateOptionsReader autoRotateOptionsReader,
            IIndex<string, IRenderCommandReader> renderCommandReaders)
        {
            this.logger = logger;
            this.conditionParser = conditionParser;
            this.componentPointParser = componentPointParser;
            this.autoRotateOptionsReader = autoRotateOptionsReader;
            this.renderCommandReaders = renderCommandReaders;
        }

        public virtual void ReadSection(XElement element, ComponentDescription description)
        {
            var groups = new List<XmlRenderGroup>();
            var defaultGroup = new XmlRenderGroup(ConditionTree.Empty);
            autoRotateOptionsReader.TrySetAutoRotateOptions(element, defaultGroup);

            groups.Add(defaultGroup);
            foreach (var child in element.Elements())
            {
                groups.AddRange(ReadElement(child, description, defaultGroup));
            }

            var flatGroups = groups.SelectMany(x => x.FlattenRoot(logger)).ToArray();
            description.RenderDescriptions = flatGroups.GroupBy(x => ConditionsReducer.SimplifyConditions(x.Conditions)).Select(g => new RenderDescription(g.Key, g.SelectMany(x => x.Value).ToArray())).ToArray();
        }

        public virtual IEnumerable<XmlRenderGroup> ReadElement(XElement element, ComponentDescription description, XmlRenderGroup groupContext)
        {
            if (element.Name == GroupElementName || element.Name == GElementName)
            {
                return ReadRenderGroup(description, element, groupContext);
            }
            else if (renderCommandReaders.TryGetValue($"{XmlLoader.ComponentNamespace.NamespaceName}:{element.Name.LocalName}", out var renderCommandReader))
            {
                if (renderCommandReader.ReadRenderCommand(element, description, out var command))
                {
                    groupContext.Value.Add(command);
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
            else if (element.Name == LineElementName)
            {
                if (ReadLineCommand(element, out var line))
                {
                    groupContext.Value.Add(line);
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
            else if (element.Name == RectElementName)
            {
                if (ReadRectCommand(element, out var rect))
                {
                    groupContext.Value.Add(rect);
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
            else if (element.Name == EllipseElementName)
            {
                if (ReadEllipseCommand(element, out var ellipse))
                {
                    groupContext.Value.Add(ellipse);
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
            else if (element.Name == PathElementName)
            {
                if (ReadPathCommand(element, out var path))
                {
                    groupContext.Value.Add(path);
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
            else if (element.HasElements)
            {
                if (element.Name.Namespace == XmlLoader.ComponentNamespace)
                {
                    logger.LogWarning(element, $"Descending unknown render element '{element.Name.LocalName}'");
                }
                return element.Elements().SelectMany(x => ReadElement(x, description, groupContext));
            }
            else
            {
                if (element.Name.Namespace == XmlLoader.ComponentNamespace)
                {
                    logger.LogWarning(element, $"Unknown render element '{element.Name.LocalName}'");
                }
                return Enumerable.Empty<XmlRenderGroup>();
            }
        }

        protected virtual IEnumerable<XmlRenderGroup> ReadRenderGroup(ComponentDescription description, XElement renderElement, XmlRenderGroup parentGroup)
        {
            IConditionTreeItem conditionCollection = ConditionTree.Empty;
            var conditionsAttribute = renderElement.Attribute("conditions");
            if (conditionsAttribute != null)
            {
                if (!conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection))
                    yield break;
            }

            var renderGroup = new XmlRenderGroup(new ConditionTree(ConditionTree.ConditionOperator.AND, parentGroup.Conditions, conditionCollection))
            {
                AutoRotate = parentGroup.AutoRotate,
                AutoRotateFlip = parentGroup.AutoRotateFlip,
            };

            autoRotateOptionsReader.TrySetAutoRotateOptions(renderElement, renderGroup);

            var childGroups = renderElement.Elements().SelectMany(x => ReadElement(x, description, renderGroup));

            yield return renderGroup;
            foreach (var child in childGroups)
                yield return child;
        }

        protected virtual bool ReadLineCommand(XElement element, out XmlLineCommand command)
        {
            command = new XmlLineCommand();

            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            if (!componentPointParser.TryParse(element.Attribute("start"), out var start))
                return false;
            command.Start = start;

            if (!componentPointParser.TryParse(element.Attribute("end"), out var end))
                return false;
            command.End = end;

            return true;
        }

        protected virtual bool ReadRectCommand(XElement element, out XmlRectCommand command)
        {
            command = new XmlRectCommand();

            if (element.Attribute("thickness") != null)
                command.StrokeThickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.Attribute("location") != null)
            {
                if (!componentPointParser.TryParse(element.Attribute("location"), out var location))
                    return false;
                command.Location = location;
            }
            else
            {
                var x = element.Attribute("x");
                var y = element.Attribute("y");
                if (!componentPointParser.TryParse(x, y, out var location))
                    return false;
                command.Location = location;
            }

            command.Width = double.Parse(element.Attribute("width").Value);
            command.Height = double.Parse(element.Attribute("height").Value);

            return true;
        }

        protected virtual bool ReadEllipseCommand(XElement element, out XmlEllipseCommand command)
        {
            command = new XmlEllipseCommand();

            if (element.Attribute("thickness") != null)
                command.StrokeThickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.Attribute("centre") != null)
            {
                if (!componentPointParser.TryParse(element.Attribute("centre"), out var centre))
                {
                    return false;
                }
                command.Centre = centre;
            }
            else
            {
                var x = element.Attribute("x");
                var y = element.Attribute("y");
                if (!componentPointParser.TryParse(x, y, out var centre))
                {
                    return false;
                }
                command.Centre = centre;
            }

            if (element.GetAttributeValue("rx", logger, out var rx))
                command.RadiusX = double.Parse(rx);

            if (element.GetAttributeValue("ry", logger, out var ry))
                command.RadiusY = double.Parse(ry);

            return true;
        }

        protected virtual bool ReadPathCommand(XElement element, out XmlRenderPath command)
        {
            command = new XmlRenderPath();

            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            if (!componentPointParser.TryParse(element.Attribute("start"), out var start))
                return false;
            command.Start = start;

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.GetAttributeValue("data", logger, out var data))
                command.Commands = PathHelper.ParseCommands(data);

            return true;
        }
    }
}
