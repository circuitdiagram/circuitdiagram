// XmlRenderer.cs
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
using System.Globalization;
using System.Xml.Linq;
using CircuitDiagram.Drawing;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Render.Path;
using CircuitDiagram.IO;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Renders as an XML preview document.
    /// </summary>
    public class XmlRenderer : IDrawingContext
    {
        public const string PreviewContentType = "application/vnd.circuitdiagram.document.preview+xml";
        public static readonly XNamespace PreviewNamespace = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/xmlvectorpreview";

        public bool Absolute
        {
            get { return true; }
        }

        private readonly MemoryStream m_stream;
        private readonly XmlWriter m_writer;
        private readonly Size size;

        public XmlRenderer(Size size)
        {
            this.size = size;
            m_stream = new MemoryStream();
            m_writer = XmlWriter.Create(m_stream, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t"
            });
        }

        public void Begin()
        {
            m_writer.WriteStartDocument();
            m_writer.WriteStartElement("preview", PreviewNamespace.NamespaceName);
            m_writer.WriteAttributeString("version", "1.0");
            m_writer.WriteAttributeString("width", size.Width.ToString());
            m_writer.WriteAttributeString("height", size.Height.ToString());
        }

        public void End()
        {
            m_writer.WriteEndElement();
            m_writer.WriteEndDocument();
            m_writer.Flush();
        }

        public void StartSection(object tag)
        {
            // TODO
        }

        public void WriteXmlTo(Stream stream)
        {
            m_stream.WriteTo(stream);
        }

        public void DrawLine(Point start, Point end, double thickness)
        {
            m_writer.WriteStartElement("line");
            m_writer.WriteAttributeString("start", start.ToString());
            m_writer.WriteAttributeString("end", end.ToString());
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawRectangle(Point start, Size size, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("rect");
            m_writer.WriteAttributeString("start", start.ToString());
            m_writer.WriteAttributeString("size", size.ToString());
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("ellipse");
            m_writer.WriteAttributeString("centre", centre.ToString());
            m_writer.WriteAttributeString("rx", radiusX.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("ry", radiusY.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawPath(Point start, IList<IPathCommand> commands, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("path");
            m_writer.WriteAttributeString("start", start.ToString());
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());

            using (MemoryStream dataStream = new MemoryStream())
            {
                System.IO.BinaryWriter dataWriter = new System.IO.BinaryWriter(dataStream);
                dataWriter.Write(commands.Count);
                foreach (IPathCommand pathCommand in commands)
                {
                    dataWriter.Write((int)pathCommand.Type);
                    pathCommand.Write(dataWriter);
                }
                dataWriter.Flush();

                m_writer.WriteValue(Convert.ToBase64String(dataStream.ToArray()));
            }

            m_writer.WriteEndElement();
        }

        public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            m_writer.WriteStartElement("text");
            m_writer.WriteAttributeString("anchor", anchor.ToString());
            m_writer.WriteAttributeString("alignment", alignment.ToString());
            foreach (var textRun in textRuns)
            {
                m_writer.WriteStartElement("run");
                m_writer.WriteAttributeString("size", textRun.Formatting.Size.ToString());
                m_writer.WriteAttributeString("formatting", textRun.Formatting.FormattingType.ToString());
                m_writer.WriteValue(textRun.Text);
                m_writer.WriteEndElement();
            }
            m_writer.WriteEndElement();
        }

        public static bool RenderFromXml(Stream xmlStream, IDrawingContext drawingContext, out Size imageSize)
        {
            var doc = XDocument.Load(xmlStream);

            var previewNode = doc.Elements().First(x => x.Name == PreviewNamespace + "preview");
            imageSize = new Size(double.Parse(previewNode.Attribute("width").Value),
                double.Parse(previewNode.Attribute("height").Value));

            var renderElements = previewNode.Elements();
            foreach (var renderElement in renderElements)
            {
                if (renderElement.Name == "line")
                {
                    Point start = Point.Parse(renderElement.Attribute("start").Value);
                    Point end = Point.Parse(renderElement.Attribute("end").Value);
                    double thickness = double.Parse(renderElement.Attribute("thickness").Value);
                    drawingContext.DrawLine(start, end, thickness);
                }
                else if (renderElement.Name == "rect")
                {
                    Point start = Point.Parse(renderElement.Attribute("start").Value);
                    Size size = Size.Parse(renderElement.Attribute("size").Value);
                    double thickness = double.Parse(renderElement.Attribute("thickness").Value);
                    bool fill = bool.Parse(renderElement.Attribute("fill").Value);
                    drawingContext.DrawRectangle(start, size, thickness, fill);
                }
                else if (renderElement.Name == "ellipse")
                {
                    Point centre = Point.Parse(renderElement.Attribute("centre").Value);
                    double radiusx = double.Parse(renderElement.Attribute("rx").Value);
                    double radiusy = double.Parse(renderElement.Attribute("ry").Value);
                    double thickness = double.Parse(renderElement.Attribute("thickness").Value);
                    bool fill = bool.Parse(renderElement.Attribute("fill").Value);
                    drawingContext.DrawEllipse(centre, radiusx, radiusy, thickness, fill);
                }
                else if (renderElement.Name == "path")
                {
                    Point start = Point.Parse(renderElement.Attribute("start").Value);
                    double thickness = double.Parse(renderElement.Attribute("thickness").Value);
                    bool fill = bool.Parse(renderElement.Attribute("fill").Value);
                    string data = renderElement.Value;
                    List<IPathCommand> pathCommands = new List<IPathCommand>();
                    using (MemoryStream dataStream = new MemoryStream(Convert.FromBase64String(data)))
                    {
                        BinaryReader reader = new BinaryReader(dataStream);

                        int numCommands = reader.ReadInt32();

                        for (int l = 0; l < numCommands; l++)
                        {
                            CommandType pType = (CommandType)reader.ReadInt32();
                            IPathCommand theCommand = null;
                            switch (pType)
                            {
                                case CommandType.MoveTo:
                                    theCommand = new MoveTo();
                                    break;
                                case CommandType.LineTo:
                                    theCommand = new LineTo();
                                    break;
                                case CommandType.CurveTo:
                                    theCommand = new CurveTo();
                                    break;
                                case CommandType.EllipticalArcTo:
                                    theCommand = new EllipticalArcTo();
                                    break;
                                case CommandType.QuadraticBeizerCurveTo:
                                    theCommand = new QuadraticBeizerCurveTo();
                                    break;
                                case CommandType.SmoothCurveTo:
                                    theCommand = new SmoothCurveTo();
                                    break;
                                case CommandType.SmoothQuadraticBeizerCurveTo:
                                    theCommand = new SmoothQuadraticBeizerCurveTo();
                                    break;
                                default:
                                    theCommand = new ClosePath();
                                    break;
                            }
                            theCommand.Read(reader);
                            pathCommands.Add(theCommand);
                        }
                    }
                    drawingContext.DrawPath(start, pathCommands, thickness, fill);
                }
                else if (renderElement.Name == "text")
                {
                    Point anchor = Point.Parse(renderElement.Attribute("anchor").Value);
                    TextAlignment alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), renderElement.Attribute("alignment").Value);
                    List<TextRun> runs = new List<TextRun>();
                    foreach (var runNode in renderElement.Elements())
                    {
                        if (runNode.Name != "run")
                            continue;

                        double size = double.Parse(runNode.Attribute("size").Value);
                        TextRunFormattingType formattingType = (TextRunFormattingType)Enum.Parse(typeof(TextRunFormattingType), runNode.Attribute("formatting").Value);
                        string text = runNode.Value;
                        runs.Add(new TextRun(text, new TextRunFormatting(formattingType, size)));
                    }
                    drawingContext.DrawText(anchor, alignment, runs);
                }
            }

            return true;
        }
    }
}
