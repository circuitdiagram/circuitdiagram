// FlipFlop.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2011  Sam Fisher
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
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace CircuitDiagram.EComponents
{
    class FlipFlop : EComponent
    {
        public Size Size
        {
            get { return new Size(EndLocation.X - StartLocation.X, EndLocation.Y - StartLocation.Y); }
        }

        public override Rect BoundingBox
        {
            get
            {
                return new Rect(new Point(StartLocation.X, StartLocation.Y - 40d), new Point(EndLocation.X, EndLocation.Y + 40d));
            }
        }

        public override void UpdateLayout()
        {
            if (StartLocation.X == EndLocation.X)
                EndLocation = new Point(StartLocation.X + 60d, StartLocation.Y);
            this.ImplementMinimumSize(60d);
        }

        public override void Render(IRenderer dc, System.Windows.Media.Color colour)
        {
            Point rectStart = new Point(StartLocation.X + Size.Width / 2 - 20d, StartLocation.Y - 30d);
            if ((rectStart.X) % 10 != 0)
                        rectStart.X += 5d;
            dc.DrawRectangle(Colors.Transparent, colour, 2d, new Rect(rectStart.X, rectStart.Y, 40d, 60d));
            dc.DrawLine(colour, 2d, StartLocation, new Point(rectStart.X, StartLocation.Y));
            dc.DrawLine(colour, 2d, new Point(StartLocation.X, StartLocation.Y - 20d), new Point(rectStart.X, StartLocation.Y - 20d));
            dc.DrawLine(colour, 2d, new Point(rectStart.X + 40d, StartLocation.Y - 20d), new Point(EndLocation.X, StartLocation.Y - 20d));
            dc.DrawLine(colour, 2d, new Point(rectStart.X + 40d, StartLocation.Y + 20d), new Point(EndLocation.X, StartLocation.Y + 20d));
            dc.DrawLine(colour, 2d, new Point(rectStart.X + 20d, StartLocation.Y - 40d), new Point(rectStart.X + 20d, rectStart.Y));
            dc.DrawLine(colour, 2d, new Point(rectStart.X + 20d, StartLocation.Y + 30d), new Point(rectStart.X + 20d, StartLocation.Y + 40d));

            // Labels
            dc.DrawPath(Colors.Transparent, colour, 1f, String.Format("M {0} l 6,6 l -6,6", new Point(rectStart.X, StartLocation.Y - 6d).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            FormattedText text = new FormattedText("D", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawText("D", "Arial", 12d, colour, new Point(rectStart.X + 3d, StartLocation.Y - 20d - text.Height / 2));
            text = new FormattedText("Q", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawText("Q", "Arial", 12d, colour, new Point(rectStart.X + 37d - text.Width, StartLocation.Y - 20d - text.Height / 2));
            dc.DrawText("Q", "Arial", 12d, colour, new Point(rectStart.X + 37d - text.Width, StartLocation.Y + 20d - text.Height / 2));
            dc.DrawLine(colour, 1d, new Point(rectStart.X + 37d - text.Width, StartLocation.Y + 20d - text.Height / 2), new Point(rectStart.X + 37d, StartLocation.Y + 20d - text.Height / 2));
            text = new FormattedText("S", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawText("S", "Arial", 12d, colour, new Point(rectStart.X + 20d - text.Width / 2, StartLocation.Y - 20d - text.Height / 2));
            text = new FormattedText("R", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12d, new SolidColorBrush(Colors.Black));
            dc.DrawText("R", "Arial", 12d, colour, new Point(rectStart.X + 20d - text.Width / 2, StartLocation.Y + 20d - text.Height / 2));
        }
    }
}
