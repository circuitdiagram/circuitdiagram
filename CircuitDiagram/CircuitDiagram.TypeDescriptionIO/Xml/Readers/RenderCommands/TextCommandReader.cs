using CircuitDiagram.Drawing.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers.RenderCommands
{
    public class TextCommandReader : IRenderCommandReader
    {
        private static readonly Version TextRotationMinFormatVersion = new Version(1, 4);

        private readonly IXmlLoadLogger logger;
        private readonly IComponentPointParser componentPointParser;

        public TextCommandReader(IXmlLoadLogger logger, IComponentPointParser componentPointParser)
        {
            this.logger = logger;
            this.componentPointParser = componentPointParser;
        }

        public bool ReadRenderCommand(XElement element, ComponentDescription description, out IXmlRenderCommand command)
        {
            var textCommand = new XmlRenderText();
            command = textCommand;

            if (!ReadTextLocation(element, textCommand))
            {
                return false;
            }

            string tAlignment = "TopLeft";
            if (element.Attribute("align") != null)
                tAlignment = element.Attribute("align").Value;

            if (!Enum.TryParse(tAlignment, out TextAlignment alignment))
            {
                logger.LogError(element.Attribute("align"), $"Invalid value for text alignment: '{tAlignment}'");
                return false;
            }
            textCommand.Alignment = alignment;

            var tRotation = "0";
            if (description.Metadata.FormatVersion >= TextRotationMinFormatVersion && element.Attribute("rotate") != null)
                tRotation = element.Attribute("rotate").Value;

            var rotation = TextRotation.None;
            switch (tRotation)
            {
                case "0":
                    rotation = TextRotation.None;
                    break;
                case "90":
                    rotation = TextRotation.Rotate90;
                    break;
                case "180":
                    rotation = TextRotation.Rotate180;
                    break;
                case "270":
                    rotation = TextRotation.Rotate270;
                    break;
                default:
                    logger.LogError(element.Attribute("rotate"), $"Invalid value for text rotation: '{tRotation}'");
                    break;
            }
            textCommand.Rotation = rotation;

            double size = 11d;
            if (element.Attribute("size") != null)
            {
                switch (element.Attribute("size").Value.ToLowerInvariant())
                {
                    case "large":
                        size = 12.0;
                        break;
                    case "sm":
                    case "small":
                        size = 10.0;
                        break;
                    case "xs":
                        size = 9.0;
                        break;
                    case "xxs":
                    case "2xs":
                        size = 8.0;
                        break;
                    case "3xs":
                        size = 7.0;
                        break;
                    default:
                        logger.LogWarning(element.Attribute("size"), $"Invalid value for size attribute: '{element.Attribute("size").Value}'");
                        break;
                }
            }

            var textValueNode = element.Element(XmlLoader.ComponentNamespace + "value");
            if (textValueNode != null)
            {
                foreach (var spanNode in textValueNode.Elements())
                {
                    string nodeValue = spanNode.Value;
                    var formatting = new TextRunFormatting(TextRunFormattingType.Normal, size);

                    if (spanNode.Name.LocalName == "sub")
                        formatting.FormattingType = TextRunFormattingType.Subscript;
                    else if (spanNode.Name.LocalName == "sup")
                        formatting.FormattingType = TextRunFormattingType.Superscript;
                    else if (spanNode.Name.LocalName != "span")
                        logger.LogWarning(spanNode, $"Unknown node '{spanNode.Name}' will be treated as <span>");

                    var textRun = new TextRun(nodeValue, formatting);

                    if (!ValidateText(element, description, textRun.Text))
                        return false;

                    textCommand.TextRuns.Add(textRun);
                }
            }
            else if (element.GetAttribute("value", logger, out var value))
            {
                var textRun = new TextRun(value.Value, new TextRunFormatting(TextRunFormattingType.Normal, size));

                if (!ValidateText(value, description, textRun.Text))
                    return false;

                textCommand.TextRuns.Add(textRun);
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

        protected virtual bool ReadTextLocation(XElement element, XmlRenderText command)
        {
            if (!element.GetAttribute("x", logger, out var x) ||
                !element.GetAttribute("y", logger, out var y))
                return false;

            if (!componentPointParser.TryParse(x, y, out var location))
                return false;
            command.Location = location;

            return true;
        }
    }
}
