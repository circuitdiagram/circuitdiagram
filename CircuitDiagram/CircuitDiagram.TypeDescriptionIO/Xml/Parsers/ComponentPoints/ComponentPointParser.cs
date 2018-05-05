using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    class ComponentPointParser : IComponentPointParser
    {
        public bool TryParse(string x, string y, IXmlLineInfo xLine, IXmlLineInfo yLine, out ComponentPoint componentPoint)
        {
            var relativeToX = ComponentPosition.Absolute;
            var relativeToY = ComponentPosition.Absolute;
            var offset = new Vector();

            if (x.StartsWith("_Start", StringComparison.OrdinalIgnoreCase))
                relativeToX = ComponentPosition.Start;
            if (y.StartsWith("_Start", StringComparison.OrdinalIgnoreCase))
                relativeToY = ComponentPosition.Start;
            if (x.StartsWith("_Middle", StringComparison.OrdinalIgnoreCase))
                relativeToX = ComponentPosition.Middle;
            if (y.StartsWith("_Middle", StringComparison.OrdinalIgnoreCase))
                relativeToY = ComponentPosition.Middle;
            if (x.StartsWith("_End", StringComparison.OrdinalIgnoreCase))
                relativeToX = ComponentPosition.End;
            if (y.StartsWith("_End", StringComparison.OrdinalIgnoreCase))
                relativeToY = ComponentPosition.End;

            var regex = new Regex("[\\-\\+][0-9\\.]+");
            var xmatch = regex.Match(x);
            var ymatch = regex.Match(y);

            if (xmatch.Captures.Count >= 1)
                offset = new Vector(double.Parse(xmatch.Captures[0].Value), offset.Y);
            if (ymatch.Captures.Count >= 1)
                offset = new Vector(offset.X, double.Parse(ymatch.Captures[0].Value));

            componentPoint = new ComponentPoint(relativeToX, relativeToY, offset);
            return true;
        }

        public bool TryParse(string point, IXmlLineInfo line, out ComponentPoint componentPoint)
        {
            var relativeToX = ComponentPosition.Absolute;
            var relativeToY = ComponentPosition.Absolute;
            var offset = new Vector();

            var regex0 = new Regex("(_Start|_Middle|_End)[0-9-\\+xy\\.]+(_Start|_Middle|_End)");
            var regex1 = new Regex("(_Start|_Middle|_End)[0-9-\\+xy\\.]+");
            var regex2 = new Regex("(_Start|_Middle|_End)");
            if (regex0.IsMatch(point))
            {
                // Not supported
            }
            else if (regex1.IsMatch(point))
            {
                if (point.StartsWith("_Start", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.Start;
                    relativeToY = ComponentPosition.Start;
                }
                if (point.StartsWith("_Middle", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.Middle;
                    relativeToY = ComponentPosition.Middle;
                }
                else if (point.StartsWith("_End", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.End;
                    relativeToY = ComponentPosition.End;
                }

                var xoffset = new Regex("[\\+\\-0-9\\.]+x");
                var yoffset = new Regex("[\\+\\-0-9\\.]+y");

                var xmatch = xoffset.Match(point);
                var ymatch = yoffset.Match(point);

                if (xmatch.Captures.Count >= 1)
                    offset = new Vector(double.Parse(xmatch.Captures[0].Value.Replace("x", "")), offset.Y);
                if (ymatch.Captures.Count >= 1)
                    offset = new Vector(offset.X, double.Parse(ymatch.Captures[0].Value.Replace("y", "")));
            }
            else if (regex2.IsMatch(point))
            {
                if (point.StartsWith("_Start", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.Start;
                    relativeToY = ComponentPosition.Start;
                }
                else if (point.StartsWith("_Middle", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.Middle;
                    relativeToY = ComponentPosition.Middle;
                }
                else if (point.StartsWith("_End", StringComparison.OrdinalIgnoreCase))
                {
                    relativeToX = ComponentPosition.End;
                    relativeToY = ComponentPosition.End;
                }
            }

            componentPoint = new ComponentPoint(relativeToX, relativeToY, offset);
            return true;
        }
    }
}
