using System;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.IO.Descriptions.Xml
{
    static class IRenderCommandXmlExtensions
    {
        public static void LoadFromXml(this Line command, XElement element, LoadContext lc)
        {
            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            string start;
            if (element.GetAttribute("start", lc, out start))
                command.Start = new ComponentPoint(start);

            string end;
            if (element.GetAttribute("end", lc, out end))
                command.End = new ComponentPoint(end);
        }

        public static void LoadFromXml(this Rectangle command, XElement element)
        {
            if (element.Attribute("thickness") != null)
                command.StrokeThickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.Attribute("location") != null)
                command.Location = new ComponentPoint(element.Attribute("location").Value);
            else
            {
                string x = element.Attribute("x").Value;
                string y = element.Attribute("y").Value;
                command.Location = new ComponentPoint(x, y);
            }

            command.Width = double.Parse(element.Attribute("width").Value);
            command.Height = double.Parse(element.Attribute("height").Value);
        }

        public static void LoadFromXml(this Ellipse command, XElement element, LoadContext lc)
        {
            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            if (element.Attribute("centre") != null)
                command.Centre = new ComponentPoint(element.Attribute("centre").Value);
            else
            {
                string x = element.Attribute("x").Value;
                string y = element.Attribute("y").Value;
                command.Centre = new ComponentPoint(x, y);
            }

            string radius = "r";
            if (lc.FormatVersion <= new Version(1, 1))
                radius = "radius";

            string rx;
            if (element.GetAttribute(radius + "x", lc, out rx))
                command.RadiusX = double.Parse(rx);

            string ry;
            if (element.GetAttribute(radius + "y", lc, out ry))
                command.RadiusY = double.Parse(ry);
        }

        public static void LoadFromXml(this RenderText command, XElement element, LoadContext lc)
        {
            if (element.Attribute("location") != null)
                command.Location = new ComponentPoint(element.Attribute("location").Value);
            else
            {
                string x = element.Attribute("x").Value;
                string y = element.Attribute("y").Value;
                command.Location = new ComponentPoint(x, y);
            }

            string tAlignment = "TopLeft";
            if (element.Attribute("align") != null)
                tAlignment = element.Attribute("align").Value;
            command.Alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), tAlignment, true);

            double size = 11d;
            if (element.Attribute("size") != null)
            {
                if (element.Attribute("size").Value.ToLowerInvariant() == "large")
                    size = 12d;
            }

            var textValueNode = element.Element(XmlLoaderLegacy.ComponentNamespace + "value");
            if (textValueNode != null)
            {
                foreach (var spanNode in textValueNode.Elements())
                {
                    IXmlLineInfo line = spanNode as IXmlLineInfo;

                    string nodeValue = spanNode.Value;

                    if (spanNode.Name.LocalName == "span")
                        command.TextRuns.Add(new TextRun(nodeValue, TextRunFormatting.Normal));
                    else if (spanNode.Name.LocalName == "sub")
                        command.TextRuns.Add(new TextRun(nodeValue, TextRunFormatting.Subscript));
                    else if (spanNode.Name.LocalName == "sup")
                        command.TextRuns.Add(new TextRun(nodeValue, TextRunFormatting.Superscript));
                    else
                        lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Warning,
                            String.Format("Unexpected node {0}.", spanNode.Name.LocalName)));
                }
            }
            else
            {
                command.TextRuns.Add(new TextRun(element.Attribute("value").Value, new TextRunFormatting(TextRunFormattingType.Normal, size)));
            }
        }

        public static void LoadFromXml(this RenderPath command, XElement element, LoadContext lc)
        {
            if (element.Attribute("thickness") != null)
                command.Thickness = double.Parse(element.Attribute("thickness").Value);

            string start;
            if (element.GetAttribute("start", lc, out start))
                command.Start = new ComponentPoint(start);

            var fill = element.Attribute("fill");
            if (fill != null && fill.Value.ToLowerInvariant() != "false")
                command.Fill = true;

            string data;
            if (element.GetAttribute("data", lc, out data))
                command.Commands = PathHelper.ParseCommands(data);
        }
    }
}
