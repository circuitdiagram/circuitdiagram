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
    public class RectCommandReader : IRenderCommandReader
    {
        private readonly IXmlLoadLogger logger;
        private readonly IComponentPointParser componentPointParser;

        public RectCommandReader(IXmlLoadLogger logger, IComponentPointParser componentPointParser)
        {
            this.logger = logger;
            this.componentPointParser = componentPointParser;
        }

        public bool ReadRenderCommand(XElement element, ComponentDescription description, out IXmlRenderCommand command)
        {
            var rectCommand = new XmlRectCommand();
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

            rectCommand.Width = double.Parse(element.Attribute("width").Value);
            rectCommand.Height = double.Parse(element.Attribute("height").Value);

            return true;
        }
    }
}
