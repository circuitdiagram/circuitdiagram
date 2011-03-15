// Diode.cs
//
// Circuit Diagram http://circuitdiagram.codeplex.com/
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

namespace CircuitDiagram.EComponents
{
    public class Diode : EComponent
    {
        public override Rect BoundingBox
        {
            get
            {
                if (ZenerDiode)
                {
                    if (Horizontal)
                        return new Rect(new Point(RenderStartLocation.X, RenderStartLocation.Y - 9), new Size(RenderEndLocation.X - RenderStartLocation.X, 18));
                    else
                        return new Rect(new Point(RenderStartLocation.X - 9, RenderStartLocation.Y), new Size(18, RenderEndLocation.Y - RenderStartLocation.Y));
                }
                else
                {
                    if (Horizontal)
                        return new Rect(new Point(RenderStartLocation.X, RenderStartLocation.Y - 9), new Size(RenderEndLocation.X - RenderStartLocation.X, 18));
                    else
                        return new Rect(new Point(RenderStartLocation.X - 9, RenderStartLocation.Y), new Size(18, RenderEndLocation.Y - RenderStartLocation.Y));
                }
            }
        }

        public bool ZenerDiode { get; set; }

        public Diode()
        {
            CanFlip = true;
            ZenerDiode = false;
        }

        public override void Initialize()
        {
            base.Editor = new DiodeEditor();
        }

        protected override void CustomUpdateLayout()
        {
            ImplementMinimumSize(30f);
        }

        public override void Render(IRenderer dc, Color color)
        {
            Point middle = new Point((StartLocation.X + EndLocation.X) / 2, (StartLocation.Y + EndLocation.Y) / 2);

            if (Horizontal)
            {
                dc.DrawLine(color, 2.0f, RenderStartLocation, RenderEndLocation);
                if (!IsFlipped)
                {
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m 0,-8 l 0,16 m 0,-8 l -15,8 l 0,-16 l 15,8", middle.X + 7f, middle.Y));
                    if (ZenerDiode)
                        dc.DrawLine(color, 2f, new Point(middle.X + 8f, middle.Y + 9f), new Point(middle.X + 2f, middle.Y + 9f));
                }
                else
                {
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m 0,-8 l 0,16 m 0,-8 l 15,8 l 0,-16 l -15,8", middle.X - 7f, middle.Y));
                    if (ZenerDiode)
                        dc.DrawLine(color, 2f, new Point(middle.X - 8f, middle.Y - 9f), new Point(middle.X - 2f, middle.Y - 9f));
                }
            }
            else
            {
                dc.DrawLine(color, 2.0f, RenderStartLocation, RenderEndLocation);
                if (!IsFlipped)
                {
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l 16,0 m-8,0 l 8,-15 l-16,0 l 8,15", middle.X, middle.Y + 7f));
                    if (ZenerDiode)
                        dc.DrawLine(color, 2f, new Point(middle.X + 9f, middle.Y + 8f), new Point(middle.X + 9f, middle.Y + 2f));
                }
                else
                {
                    dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l 16,0 m-8,0 l 8,15 l-16,0 l 8,-15", middle.X, middle.Y - 7f));
                    if (ZenerDiode)
                        dc.DrawLine(color, 2f, new Point(middle.X - 9f, middle.Y - 8f), new Point(middle.X - 9f, middle.Y - 2f));
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                ZenerDiode = false;
                string zener = reader.GetAttribute("zener");
                if (zener != null && zener.ToLower() == "true")
                    ZenerDiode = true;
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            if (ZenerDiode)
                writer.WriteAttributeString("zener", "true");
        }
    }
}
