// Text.cs
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
using CircuitDiagram.Circuit;
using CircuitDiagram.Drawing.Text;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Drawing
{
    public class RenderText : IRenderCommand, ICloneable
    {
        public ComponentPoint Location { get; set; }
        public TextAlignment Alignment { get; set; }
        public List<TextRun> TextRuns { get; private set; }
        public RenderCommandType Type => RenderCommandType.Text;
        
        public RenderText()
        {
            Location = new ComponentPoint();
            Alignment = TextAlignment.TopLeft;
            TextRuns = new List<TextRun>();
        }

        public RenderText(ComponentPoint location, TextAlignment alignment, double size, string text)
        {
            Location = location;
            Alignment = alignment;
            var textRun = new TextRun(text, new TextRunFormatting(TextRunFormattingType.Normal, size));
            TextRuns = new List<TextRun>();
            TextRuns.Add(textRun);
        }

        public RenderText(ComponentPoint location, TextAlignment alignment, IEnumerable<TextRun> textRuns)
        {
            Location = location;
            Alignment = alignment;
            TextRuns = new List<TextRun>(textRuns);
        }

        public void Render(LayoutInformation layout, ILayoutContext layoutContext, IDrawingContext drawingContext)
        {
            Point renderLocation = Location.Resolve(layout, layoutContext.Options);

            TextAlignment tempAlignment = Alignment;
            if (layout.IsFlipped && layout.Orientation == Orientation.Horizontal)
            {
                switch (Alignment)
                {
                    case TextAlignment.BottomLeft:
                        tempAlignment = TextAlignment.BottomRight;
                        break;
                    case TextAlignment.BottomRight:
                        tempAlignment = TextAlignment.BottomLeft;
                        break;
                    case TextAlignment.CentreLeft:
                        tempAlignment = TextAlignment.CentreRight;
                        break;
                    case TextAlignment.CentreRight:
                        tempAlignment = TextAlignment.CentreLeft;
                        break;
                    case TextAlignment.TopLeft:
                        tempAlignment = TextAlignment.TopRight;
                        break;
                    case TextAlignment.TopRight:
                        tempAlignment = TextAlignment.TopLeft;
                        break;
                }
            }
            else if (layout.IsFlipped && layout.Orientation == Orientation.Vertical)
            {
                switch (Alignment)
                {
                    case TextAlignment.BottomCentre:
                        tempAlignment = TextAlignment.TopCentre;
                        break;
                    case TextAlignment.BottomLeft:
                        tempAlignment = TextAlignment.TopLeft;
                        break;
                    case TextAlignment.BottomRight:
                        tempAlignment = TextAlignment.TopRight;
                        break;
                    case TextAlignment.TopCentre:
                        tempAlignment = TextAlignment.BottomCentre;
                        break;
                    case TextAlignment.TopLeft:
                        tempAlignment = TextAlignment.BottomLeft;
                        break;
                    case TextAlignment.TopRight:
                        tempAlignment = TextAlignment.BottomRight;
                        break;
                }
            }

            List<TextRun> renderTextRuns = new List<TextRun>(TextRuns.Count);
            // Build runs
            foreach (TextRun run in TextRuns)
            {
                // Resolve value
                string renderValue;
                if (run.Text.StartsWith("$"))
                    renderValue = layoutContext.GetFormattedVariable(run.Text);
                else
                    renderValue = run.Text;

                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\\[uU]([0-9A-F]{4})");
                renderValue = regex.Replace(renderValue, match => ((char)int.Parse(match.Value.Substring(2), System.Globalization.NumberStyles.HexNumber)).ToString());

                renderTextRuns.Add(new TextRun(renderValue, run.Formatting));
            }

            if (layoutContext.Options.Absolute)
                drawingContext.DrawText(Point.Add(renderLocation, layout.Location), tempAlignment, renderTextRuns);
            else
                drawingContext.DrawText(renderLocation, tempAlignment, renderTextRuns);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Text return false.
            var o = obj as RenderText;
            if ((System.Object)o == null)
            {
                return false;
            }

            bool textRunsEqual = TextRuns.SequenceEqual(o.TextRuns);

            // Return true if the fields match:
            return (Location.Equals(o.Location)
                && Alignment.Equals(o.Alignment)
                && textRunsEqual);
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode()
                ^ Alignment.GetHashCode()
                ^ TextRuns.GetHashCode();
        }

        public object Clone()
        {
            return new RenderText(Location, Alignment, TextRuns);
        }
    }
}
