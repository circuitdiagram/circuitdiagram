// SVGRenderer.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Xml;
using System.IO;
using System.Reflection;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    public class SvgDrawingContext : IDrawingContext
    {
        private readonly double width;
        private readonly double height;
        private readonly XmlWriter writer;
        
        /// <summary>
        /// Creates a new SVGRenderer, which will produce an output SVG with the specified width and height.
        /// </summary>
        /// <param name="width">Width of the output SVG.</param>
        /// <param name="height">height of the output SVG.</param>
        public SvgDrawingContext(double width, double height)
        {
            SvgDocument = new MemoryStream();
            writer = XmlWriter.Create(SvgDocument, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t"
            });
            this.width = width;
            this.height = height;
        }

        public MemoryStream SvgDocument { get; }

        public void Begin()
        {
            string cdlibraryVersion = typeof(SvgDrawingContext).GetTypeInfo().Assembly.GetName().Version.ToString();

            writer.WriteStartDocument();
            writer.WriteComment(" Generator: Circuit Diagram, cdlibrary.dll " + cdlibraryVersion + " ");
            writer.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
            writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("width", width.ToString());
            writer.WriteAttributeString("height", height.ToString());
        }

        public void End()
        {
            writer.WriteEndDocument();
            writer.Flush();
        }

        public void StartSection(object tag)
        {
            // Do nothing.
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            writer.WriteStartElement("line");
            writer.WriteAttributeString("x1", start.X.ToString());
            writer.WriteAttributeString("y1", start.Y.ToString());
            writer.WriteAttributeString("x2", end.X.ToString());
            writer.WriteAttributeString("y2", end.Y.ToString());
            writer.WriteAttributeString("style", "stroke:rgb(0,0,0);stroke-linecap:square;stroke-width:" + thickness.ToString());
            writer.WriteEndElement();
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            writer.WriteStartElement("rect");
            writer.WriteAttributeString("x", start.X.ToString());
            writer.WriteAttributeString("y", start.Y.ToString());
            writer.WriteAttributeString("width", size.Width.ToString());
            writer.WriteAttributeString("height", size.Height.ToString());
            writer.WriteAttributeString("style", "fill-opacity:0;stroke:rgb(0,0,0);stroke-width:" + thickness.ToString());
            writer.WriteEndElement();
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

            writer.WriteStartElement("ellipse");
            writer.WriteAttributeString("cx", centre.X.ToString());
            writer.WriteAttributeString("cy", centre.Y.ToString());
            writer.WriteAttributeString("rx", radiusX.ToString());
            writer.WriteAttributeString("ry", radiusY.ToString());
            writer.WriteAttributeString("style", string.Format("fill-opacity:" + fillOpacity + ";fill:rgb({0},{1},{2});stroke:rgb(0,0,0);stroke-width:" + thickness.ToString(), 0, 0, 0));
            writer.WriteEndElement();
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            string data = "M " + start.X.ToString() + "," + start.Y.ToString();
            Point last = new Point(0, 0);
            foreach (IPathCommand pathCommand in commands)
            {
                data += " " + pathCommand.Shorthand(start, last);
                last = new Point(pathCommand.End.X, pathCommand.End.Y);
            }

            string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

            writer.WriteStartElement("path");
            writer.WriteAttributeString("d", data);
            writer.WriteAttributeString("style", "fill-opacity:" + fillOpacity + ";fill:rgb(0,0,0);stroke:rgb(0,0,0);stroke-width:" + thickness.ToString());
            writer.WriteEndElement();
        }

        public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            writer.WriteStartElement("text");
            writer.WriteAttributeString("x", anchor.X.ToString());
            writer.WriteAttributeString("y", anchor.Y.ToString());

            string textAnchor = "start";
            if (alignment == TextAlignment.BottomCentre  || alignment == TextAlignment.CentreCentre || alignment == TextAlignment.TopCentre)
                textAnchor = "middle";
            else if (alignment == TextAlignment.BottomRight || alignment == TextAlignment.CentreRight || alignment == TextAlignment.TopRight)
                textAnchor = "end";

            string dy = "-0.3em";
            if (alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreRight)
                dy = ".3em";
            else if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.TopLeft || alignment == TextAlignment.TopRight)
                dy = "1em";

            writer.WriteAttributeString("style", "font-family:Arial;font-size:" + textRuns.FirstOrDefault().Formatting.Size.ToString() + ";text-anchor:" + textAnchor);
            writer.WriteAttributeString("dy", dy);

            foreach (TextRun run in textRuns)
            {
                if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
                    writer.WriteStartElement("tspan");
                if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    writer.WriteAttributeString("baseline-shift", "sub");
                    writer.WriteAttributeString("style", "font-size:0.8em");
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    writer.WriteAttributeString("baseline-shift", "super");
                    writer.WriteAttributeString("style", "font-size:0.8em");
                }
                writer.WriteString(run.Text);
                if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
                    writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
