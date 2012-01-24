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
using System.Windows;
using System.IO;

namespace CircuitDiagram.Render
{
    public class SVGRenderer : IRenderContext
    {
        public MemoryStream SVGDocument { get; private set; }
        private XmlTextWriter Writer { get { return m_writer; } }

        private XmlTextWriter m_writer;

        public SVGRenderer()
        {
            SVGDocument = new MemoryStream();
            m_writer = new XmlTextWriter(SVGDocument, Encoding.UTF8);
            m_writer.Formatting = Formatting.Indented;
        }

        public void Begin()
        {
            Writer.WriteStartDocument();
            Writer.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
            Writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            Writer.WriteAttributeString("version", "1.1");
            Writer.WriteAttributeString("width", "100%");
            Writer.WriteAttributeString("height", "100%");
        }

        public void End()
        {
            Writer.WriteEndDocument();
            Writer.Flush();
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            m_writer.WriteStartElement("line");
            m_writer.WriteAttributeString("x1", start.X.ToString());
            m_writer.WriteAttributeString("y1", start.Y.ToString());
            m_writer.WriteAttributeString("x2", end.X.ToString());
            m_writer.WriteAttributeString("y2", end.Y.ToString());
            m_writer.WriteAttributeString("style", "stroke:rgb(0,0,0);stroke-linecap:square;stroke-width:" + thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("rect");
            m_writer.WriteAttributeString("x", start.X.ToString());
            m_writer.WriteAttributeString("y", start.Y.ToString());
            m_writer.WriteAttributeString("width", size.Width.ToString());
            m_writer.WriteAttributeString("height", size.Height.ToString());
            m_writer.WriteAttributeString("style", "fill-opacity:0;stroke:rgb(0,0,0);stroke-width:" + thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

            m_writer.WriteStartElement("ellipse");
            m_writer.WriteAttributeString("cx", centre.X.ToString());
            m_writer.WriteAttributeString("cy", centre.Y.ToString());
            m_writer.WriteAttributeString("rx", radiusX.ToString());
            m_writer.WriteAttributeString("ry", radiusY.ToString());
            m_writer.WriteAttributeString("style", String.Format("fill-opacity:" + fillOpacity + ";fill:rgb({0},{1},{2});stroke:rgb(0,0,0);stroke-width:" + thickness.ToString(), 0, 0, 0));
            m_writer.WriteEndElement();
        }

        public void DrawPath(Point start, IList<Components.Render.Path.IPathCommand> commands, double thickness, bool fill = false)
        {
            string data = "M " + start.X.ToString() + "," + start.Y.ToString();
            Point last = new Point(0, 0);
            foreach (CircuitDiagram.Components.Render.Path.IPathCommand pathCommand in commands)
            {
                data += " " + pathCommand.Shorthand(start, last);
                last = new Point(last.X + pathCommand.End.X, last.Y + pathCommand.End.Y);
            }

            string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

            m_writer.WriteStartElement("path");
            m_writer.WriteAttributeString("d", data);
            m_writer.WriteAttributeString("style", "fill-opacity:" + fillOpacity + ";fill:rgb(0,0,0);stroke:rgb(0,0,0);stroke-width:" + thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawText(Point anchor, Components.Render.TextAlignment alignment, string text, double size)
        {
            m_writer.WriteStartElement("text");
            m_writer.WriteAttributeString("x", anchor.X.ToString());
            m_writer.WriteAttributeString("y", anchor.Y.ToString());

            string textAnchor = "start";
            if (alignment == Components.Render.TextAlignment.BottomCentre  || alignment == Components.Render.TextAlignment.CentreCentre || alignment == Components.Render.TextAlignment.TopCentre)
                textAnchor = "middle";
            else if (alignment == Components.Render.TextAlignment.BottomRight || alignment == Components.Render.TextAlignment.CentreRight || alignment == Components.Render.TextAlignment.TopRight)
                textAnchor = "end";

            string alignmentBaseline = "auto";
            if (alignment == Components.Render.TextAlignment.CentreCentre || alignment == Components.Render.TextAlignment.CentreLeft || alignment == Components.Render.TextAlignment.CentreRight)
                alignmentBaseline = "central";
            else if (alignment == Components.Render.TextAlignment.TopCentre || alignment == Components.Render.TextAlignment.TopLeft || alignment == Components.Render.TextAlignment.TopRight)
                alignmentBaseline = "before-edge";

            m_writer.WriteAttributeString("style", "font-family:Arial;font-size:" + size.ToString() + ";text-anchor:" + textAnchor + ";alignment-baseline:" + alignmentBaseline);
            m_writer.WriteString(text);
            m_writer.WriteEndElement();
        }
    }
}
