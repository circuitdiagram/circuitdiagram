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
using CircuitDiagram.Render.Path;

namespace CircuitDiagram.Render
{
    public class SVGRenderer : IRenderContext
    {
        public bool Absolute { get { return true; } }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public MemoryStream SVGDocument { get; private set; }
        private XmlTextWriter Writer { get { return m_writer; } }

        private XmlTextWriter m_writer;

        /// <summary>
        /// Creates a new SVGRenderer, which will produce an output SVG with the specified width and height.
        /// </summary>
        /// <param name="width">Width of the output SVG.</param>
        /// <param name="height">Height of the output SVG.</param>
        public SVGRenderer(double width, double height)
        {
            SVGDocument = new MemoryStream();
            m_writer = new XmlTextWriter(SVGDocument, Encoding.UTF8);
            m_writer.Formatting = Formatting.Indented;
            this.Width = width;
            this.Height = height;
        }

        public void Begin()
        {
            string cdlibraryVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Writer.WriteStartDocument();
            Writer.WriteComment(" Generator: " + (IO.ApplicationInfo.FullName != null ? IO.ApplicationInfo.FullName : "") + ", cdlibrary.dll " + cdlibraryVersion + " ");
            Writer.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
            Writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            Writer.WriteAttributeString("version", "1.1");
            Writer.WriteAttributeString("width", this.Width.ToString());
            Writer.WriteAttributeString("height", this.Height.ToString());
        }

        public void End()
        {
            Writer.WriteEndDocument();
            Writer.Flush();
        }

        public void StartSection(object tag)
        {
            // Do nothing.
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

            m_writer.WriteStartElement("path");
            m_writer.WriteAttributeString("d", data);
            m_writer.WriteAttributeString("style", "fill-opacity:" + fillOpacity + ";fill:rgb(0,0,0);stroke:rgb(0,0,0);stroke-width:" + thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            m_writer.WriteStartElement("text");
            m_writer.WriteAttributeString("x", anchor.X.ToString());
            m_writer.WriteAttributeString("y", anchor.Y.ToString());

            string textAnchor = "start";
            if (alignment == TextAlignment.BottomCentre  || alignment == TextAlignment.CentreCentre || alignment == Render.TextAlignment.TopCentre)
                textAnchor = "middle";
            else if (alignment == TextAlignment.BottomRight || alignment == TextAlignment.CentreRight || alignment == Render.TextAlignment.TopRight)
                textAnchor = "end";

            string dy = "-0.3em";
            if (alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreRight)
                dy = ".3em";
            else if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.TopLeft || alignment == TextAlignment.TopRight)
                dy = "1em";

            m_writer.WriteAttributeString("style", "font-family:Arial;font-size:" + textRuns.FirstOrDefault().Formatting.Size.ToString() + ";text-anchor:" + textAnchor);
            m_writer.WriteAttributeString("dy", dy);

            foreach (TextRun run in textRuns)
            {
                if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
                    m_writer.WriteStartElement("tspan");
                if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
                {
                    m_writer.WriteAttributeString("baseline-shift", "sub");
                    m_writer.WriteAttributeString("style", "font-size:0.8em");
                }
                else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
                {
                    m_writer.WriteAttributeString("baseline-shift", "super");
                    m_writer.WriteAttributeString("style", "font-size:0.8em");
                }
                m_writer.WriteString(run.Text);
                if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
                    m_writer.WriteEndElement();
            }

            m_writer.WriteEndElement();
        }
    }
}
