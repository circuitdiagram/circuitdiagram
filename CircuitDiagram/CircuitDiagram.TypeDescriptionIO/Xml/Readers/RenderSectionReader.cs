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

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    public class RenderSectionReader : IXmlSectionReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IConditionParser conditionParser;
        private readonly IComponentPointParser componentPointParser;

        public RenderSectionReader(IXmlLoadLogger logger, IConditionParser conditionParser, IComponentPointParser componentPointParser)
        {
            this.logger = logger;
            this.conditionParser = conditionParser;
            this.componentPointParser = componentPointParser;
        }

        public virtual void ReadSection(XElement element, ComponentDescription description)
        {
            var results = new List<RenderDescription>();

            var groupElements = element.Elements(element.GetDefaultNamespace() + "group");
            foreach (var groupElement in groupElements)
            {
                var renderDescription = ReadRenderGroup(description, groupElement);
                if (renderDescription != null)
                    results.Add(renderDescription);
            }

            description.RenderDescriptions = results.ToArray();
        }

        protected virtual RenderDescription ReadRenderGroup(ComponentDescription description, XElement renderNode)
        {
            IConditionTreeItem conditionCollection = ConditionTree.Empty;
            var conditionsAttribute = renderNode.Attribute("conditions");
            if (conditionsAttribute != null)
            {
                if (!conditionParser.Parse(conditionsAttribute, description, logger, out conditionCollection))
                    return null;
            }

            var commands = new List<IRenderCommand>();

            foreach (var renderCommandNode in renderNode.Descendants())
            {
                string commandType = renderCommandNode.Name.LocalName;
                if (commandType == "line")
                {
                    if (ReadLineCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "rect")
                {
                    if (ReadRectCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "ellipse")
                {
                    if (ReadEllipseCommand(description.Metadata.FormatVersion, renderCommandNode, out var command))
                        commands.Add(command);
                }
                else if (commandType == "text")
                {
                    if (ReadTextCommand(renderCommandNode, description, out var command))
                        commands.Add(command);
                }
                else if (commandType == "path")
                {
                    if (ReadPathCommand(renderCommandNode, out var command))
                        commands.Add(command);
                }
            }

            return new RenderDescription(conditionCollection, commands.ToArray());
        }

        protected virtual bool ReadLineCommand(XElement element, out Line command)
        {
            command = new Line();

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

        protected virtual bool ReadRectCommand(XElement element, out Rectangle command)
        {
            command = new Rectangle();

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

        protected virtual bool ReadEllipseCommand(Version formatVersion, XElement element, out Ellipse command)
        {
            command = new Ellipse();

            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.Attribute("centre") != null)
            {
                if (!componentPointParser.TryParse(element.Attribute("centre"), out var centre))
                    return false;
                command.Centre = centre;
            }
            else
            {
                var x = element.Attribute("x");
                var y = element.Attribute("y");
                if (!componentPointParser.TryParse(x, y, out var centre))
                    return false;
                command.Centre = centre;
            }

            string radius = "r";
            if (formatVersion <= new Version(1, 1))
                radius = "radius";

            if (element.GetAttributeValue(radius + "x", logger, out var rx))
                command.RadiusX = double.Parse(rx);

            if (element.GetAttributeValue(radius + "y", logger, out var ry))
                command.RadiusY = double.Parse(ry);

            return true;
        }

        protected virtual bool ReadTextCommand(XElement element, ComponentDescription description, out RenderText command)
        {
            command = new RenderText();

            ReadTextLocation(element, command);

            string tAlignment = "TopLeft";
            if (element.Attribute("align") != null)
                tAlignment = element.Attribute("align").Value;

            if (!Enum.TryParse(tAlignment, out TextAlignment alignment))
                logger.LogError(element.Attribute("align"), $"Invalid value for text alignment: '{tAlignment}'");
            command.Alignment = alignment;

            double size = 11d;
            if (element.Attribute("size") != null)
            {
                if (element.Attribute("size").Value.ToLowerInvariant() == "large")
                    size = 12d;
            }

            var textValueNode = element.Element(XmlLoader.ComponentNamespace + "value");
            if (textValueNode != null)
            {
                foreach (var spanNode in textValueNode.Elements())
                {
                    string nodeValue = spanNode.Value;
                    var formatting = TextRunFormatting.Normal;

                    if (spanNode.Name.LocalName == "sub")
                        formatting = TextRunFormatting.Subscript;
                    else if (spanNode.Name.LocalName == "sup")
                        formatting = TextRunFormatting.Superscript;
                    else if (spanNode.Name.LocalName != "span")
                        logger.LogWarning(spanNode, $"Unknown node '{spanNode.Name}' will be treated as <span>");

                    var textRun = new TextRun(nodeValue, formatting);

                    if (!ValidateText(element, description, textRun.Text))
                        return false;

                    command.TextRuns.Add(textRun);
                }
            }
            else if (element.GetAttribute("value", logger, out var value))
            {
                var textRun = new TextRun(value.Value, new TextRunFormatting(TextRunFormattingType.Normal, size));

                if (!ValidateText(value, description, textRun.Text))
                    return false;

                command.TextRuns.Add(textRun);
            }
            else
            {
                return false;
            }
            
            return true;
        }

        private bool ValidateText(XAttribute attribute, ComponentDescription description, string text)
        {
            if (ValidateText(description, text, out var errorMessage))
                return true;

            logger.LogError(attribute, errorMessage);
            return false;
        }

        private bool ValidateText(XElement element, ComponentDescription description, string text)
        {
            if (ValidateText(description, text, out var errorMessage))
                return true;

            logger.LogError(element, errorMessage);
            return false;
        }

        protected virtual bool ValidateText(ComponentDescription description, string text, out string errorMessage)
        {
            if (!text.StartsWith("$"))
            {
                errorMessage = null;
                return true;
            }

            var propertyName = text.Substring(1);
            if (description.Properties.Any(x => x.Name == propertyName))
            {
                errorMessage = null;
                return true;
            }

            errorMessage = $"Property {propertyName} used for text value does not exist";
            return false;
        }

        protected virtual bool ReadTextLocation(XElement element, RenderText command)
        {
            if (!element.GetAttribute("x", logger, out var x) ||
                !element.GetAttribute("y", logger, out var y))
                return false;

            if (!componentPointParser.TryParse(x, y, out var location))
                return false;
            command.Location = location;

            return true;
        }

        protected virtual bool ReadPathCommand(XElement element, out RenderPath command)
        {
            command = new RenderPath();

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
