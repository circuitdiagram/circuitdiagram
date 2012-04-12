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
using System.Windows;
using CircuitDiagram.Components.Render;
using CircuitDiagram.Components.Render.Path;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Renders as an XML preview document.
    /// </summary>
    public class XmlRenderer : IRenderContext
    {
        public const string PreviewContentType = "application/vnd.circuitdiagram.document.preview+xml";
        public const string PreviewNamespace = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/metadata/xmlvectorpreview";

        public bool Absolute
        {
            get { return true; }
        }

        private MemoryStream m_stream;
        private XmlTextWriter m_writer;

        private System.Windows.Size Size;

        public XmlRenderer(System.Windows.Size size)
        {
            Size = size;
            m_stream = new MemoryStream();
            m_writer = new XmlTextWriter(m_stream, Encoding.UTF8);
            m_writer.Formatting = Formatting.Indented;
        }

        public void Begin()
        {
            m_writer.WriteStartDocument();
            m_writer.WriteStartElement("preview", PreviewNamespace);
            m_writer.WriteAttributeString("version", "1.0");
            if (Size != null)
            {
                m_writer.WriteAttributeString("width", Size.Width.ToString());
                m_writer.WriteAttributeString("height", Size.Height.ToString());
            }
        }

        public void End()
        {
            m_writer.WriteEndElement();
            m_writer.WriteEndDocument();
            m_writer.Flush();
        }

        public void WriteXmlTo(Stream stream)
        {
            m_stream.WriteTo(stream);
        }

        public void DrawLine(System.Windows.Point start, System.Windows.Point end, double thickness)
        {
            m_writer.WriteStartElement("line");
            m_writer.WriteAttributeString("start", start.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("end", end.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawRectangle(System.Windows.Point start, System.Windows.Size size, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("rect");
            m_writer.WriteAttributeString("start", start.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("size", size.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawEllipse(System.Windows.Point centre, double radiusX, double radiusY, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("ellipse");
            m_writer.WriteAttributeString("centre", centre.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("rx", radiusX.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("ry", radiusY.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());
            m_writer.WriteEndElement();
        }

        public void DrawPath(System.Windows.Point start, IList<Components.Render.Path.IPathCommand> commands, double thickness, bool fill = false)
        {
            m_writer.WriteStartElement("path");
            m_writer.WriteAttributeString("start", start.ToString(CultureInfo.InvariantCulture));
            m_writer.WriteAttributeString("thickness", thickness.ToString());
            m_writer.WriteAttributeString("fill", fill.ToString());

            using (MemoryStream dataStream = new MemoryStream())
            {
                BinaryWriter dataWriter = new BinaryWriter(dataStream);
                dataWriter.Write(commands.Count);
                foreach (CircuitDiagram.Components.Render.Path.IPathCommand pathCommand in commands)
                {
                    dataWriter.Write((int)pathCommand.Type);
                    pathCommand.Write(dataWriter);
                }
                dataWriter.Flush();

                m_writer.WriteValue(Convert.ToBase64String(dataStream.ToArray()));
            }

            m_writer.WriteEndElement();
        }

        public void DrawText(System.Windows.Point anchor, Components.Render.TextAlignment alignment, IEnumerable<Components.Render.TextRun> textRuns)
        {
            m_writer.WriteStartElement("text");
            m_writer.WriteAttributeString("anchor", anchor.ToString(CultureInfo.InvariantCulture));
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

        public static bool RenderFromXml(Stream xmlStream, IRenderContext renderContext, out Size imageSize)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlStream);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("p", PreviewNamespace);

            XmlNode previewNode = doc.SelectSingleNode("/p:preview", namespaceManager);
            imageSize = new Size(double.Parse(previewNode.Attributes["width"].InnerText), double.Parse(previewNode.Attributes["height"].InnerText));

            XmlNodeList renderNodes = previewNode.ChildNodes;
            foreach (XmlNode renderNode in renderNodes)
            {
                XmlElement renderElement = renderNode as XmlElement;

                if (renderElement == null)
                    continue;

                if (renderElement.Name == "line")
                {
                    Point start = Point.Parse(renderElement.Attributes["start"].InnerText);
                    Point end = Point.Parse(renderElement.Attributes["end"].InnerText);
                    double thickness = double.Parse(renderElement.Attributes["thickness"].InnerText);
                    renderContext.DrawLine(start, end, thickness);
                }
                else if (renderElement.Name == "rect")
                {
                    Point start = Point.Parse(renderElement.Attributes["start"].InnerText);
                    Size size = Size.Parse(renderElement.Attributes["size"].InnerText);
                    double thickness = double.Parse(renderElement.Attributes["thickness"].InnerText);
                    bool fill = bool.Parse(renderElement.Attributes["fill"].InnerText);
                    renderContext.DrawRectangle(start, size, thickness, fill);
                }
                else if (renderElement.Name == "ellipse")
                {
                    Point centre = Point.Parse(renderElement.Attributes["centre"].InnerText);
                    double radiusx = double.Parse(renderElement.Attributes["rx"].InnerText);
                    double radiusy = double.Parse(renderElement.Attributes["ry"].InnerText);
                    double thickness = double.Parse(renderElement.Attributes["thickness"].InnerText);
                    bool fill = bool.Parse(renderElement.Attributes["fill"].InnerText);
                    renderContext.DrawEllipse(centre, radiusx, radiusy, thickness, fill);
                }
                else if (renderElement.Name == "path")
                {
                    Point start = Point.Parse(renderElement.Attributes["start"].InnerText);
                    double thickness = double.Parse(renderElement.Attributes["thickness"].InnerText);
                    bool fill = bool.Parse(renderElement.Attributes["fill"].InnerText);


                    string data = renderElement.InnerText;
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

                    renderContext.DrawPath(start, pathCommands, thickness, fill);
                }
                else if (renderElement.Name == "text")
                {
                    Point anchor = Point.Parse(renderElement.Attributes["anchor"].InnerText);
                    TextAlignment alignment = (TextAlignment)Enum.Parse(typeof(TextAlignment), renderElement.Attributes["alignment"].InnerText);
                    List<TextRun> runs = new List<TextRun>();
                    foreach (XmlNode runNode in renderElement.ChildNodes)
                    {
                        if (runNode.Name != "run")
                            continue;

                        double size = double.Parse(runNode.Attributes["size"].InnerText);
                        TextRunFormattingType formattingType = (TextRunFormattingType)Enum.Parse(typeof(TextRunFormattingType), runNode.Attributes["formatting"].InnerText);
                        string text = runNode.InnerText;
                        runs.Add(new TextRun(text, new TextRunFormatting(formattingType, size)));
                    }
                    renderContext.DrawText(anchor, alignment, runs);
                }
            }

            return true;
        }
    }
}
