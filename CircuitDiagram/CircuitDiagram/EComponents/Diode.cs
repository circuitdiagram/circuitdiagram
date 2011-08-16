// Diode.cs
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

namespace CircuitDiagram.EComponents
{
    public class Diode : EComponent
    {
        public override Rect BoundingBox
        {
            get
            {
                if (Type == DiodeType.Bridge)
                {
                    return new Rect(new Point(RenderStartLocation.X, RenderStartLocation.Y - 32), new Size(RenderEndLocation.X - RenderStartLocation.X, 94));
                }
                else if ((Type == DiodeType.LED && Horizontal && IsFlipped) || (Type == DiodeType.Photo && Horizontal && !IsFlipped))
                {
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 8), new Size(EndLocation.X - StartLocation.X, 30));
                }
                else if (Type == DiodeType.LED)
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 22), new Size(EndLocation.X - StartLocation.X, 30));
                    else
                        return new Rect(new Point(StartLocation.X - 8, StartLocation.Y), new Size(30, EndLocation.Y - StartLocation.Y));
                }
                else if (Type == DiodeType.Photo)
                {
                    if (Horizontal)
                        return new Rect(new Point(StartLocation.X, StartLocation.Y - 8), new Size(EndLocation.X - StartLocation.X, 30));
                    else
                        return new Rect(new Point(StartLocation.X - 22, StartLocation.Y), new Size(30, EndLocation.Y - StartLocation.Y));
                }
                else if (Type == DiodeType.Zener)
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

        public DiodeType Type { get; set; }

        public Diode()
        {
            CanFlip = true;
            Type = DiodeType.Standard;
        }

        public override void Initialize()
        {
            base.Editor = new DiodeEditor(this);
        }

        protected override void CustomUpdateLayout()
        {
            ImplementMinimumSize(30f);
            if (Type == DiodeType.Bridge)
            {
                CanFlip = false;
                CanResize = false;
                EndLocation = new Point(StartLocation.X + 110d, StartLocation.Y);
            }
            else
            {
                CanFlip = true;
                CanResize = true;
            }
        }

        public override void Render(IRenderer dc, Color color)
        {
            Point middle = new Point((StartLocation.X + EndLocation.X) / 2, (StartLocation.Y + EndLocation.Y) / 2);

            if (Type == DiodeType.Bridge)
            {
                dc.DrawPath(null, color, 2d, "m 62.790306,-29.834653 c 0,0.837455 -0.678892,1.516347 -1.516347,1.516347 -0.837456,0 -1.516347,-0.678892 -1.516347,-1.516347 0,-0.837456 0.678891,-1.516347 1.516347,-1.516347 0.837455,0 1.516347,0.678891 1.516347,1.516347 z M 92.25606,0.1685698 c 0,0.8374553 -0.678892,1.5163472 -1.516347,1.5163472 -0.837455,0 -1.516347,-0.6788919 -1.516347,-1.5163472 0,-0.8374554 0.678892,-1.5163472 1.516347,-1.5163472 0.837455,0 1.516347,0.6788918 1.516347,1.5163472 z M 62.790306,29.934484 c 0,0.837458 -0.678892,1.516346 -1.516347,1.516346 -0.837456,0 -1.516347,-0.678888 -1.516347,-1.516346 0,-0.83745 0.678891,-1.516345 1.516347,-1.516345 0.837455,0 1.516347,0.678895 1.516347,1.516345 z M 33.17145,-0.023613 c 0,0.8374553 -0.678891,1.5163472 -1.516347,1.5163472 -0.837455,0 -1.516347,-0.6788919 -1.516347,-1.5163472 0,-0.8374554 0.678892,-1.5163472 1.516347,-1.5163472 0.837456,0 1.516347,0.6788918 1.516347,1.5163472 z m 19.607141,-21.755533 -16.322168,4.967618 11.354551,11.3545501 4.645449,-15.9994441 m 5.999444,5.354551 -11.354552,-11.354551 M 40.14402,8.920957 45.111636,25.243145 56.46619,13.888623 40.466745,9.2436954 M 45.821296,3.2436951 34.466745,14.598218 M 69.436454,-21.507358 74.404071,-5.1851886 85.758624,-16.539739 69.75918,-21.184634 m 5.35455,-6 -11.35455,11.354552 M 83.20692,8.1055904 66.884751,13.073156 78.239303,24.427777 82.884751,8.4283288 M 88.884196,13.782852 77.529643,2.4283292 M 61,-30.034365 91.034365,-3.0000001e-7 61,30.034366 30.965636,-3.0000001e-7 z m 1.072515,60.034366 50.000005,0 M 2.509696e-7,-3.0000001e-7 30,-3.0000001e-7 M -1.8764288e-6,59.999999 107.39316,59.945977 l 0.0396,-59.8098075 -15.979894,0 M 61.30378,-30 l 50.00001,0", StartLocation.X, StartLocation.Y);
            }
            else
            {
                if (Horizontal)
                {
                    dc.DrawLine(color, 2.0f, RenderStartLocation, RenderEndLocation);
                    if (!IsFlipped)
                    {
                        dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m 0,-8 l 0,16 m 0,-8 l -15,8 l 0,-16 l 15,8", middle.X + 7f, middle.Y));
                        if (Type == DiodeType.Zener)
                            dc.DrawLine(color, 2f, new Point(middle.X + 8f, middle.Y + 9f), new Point(middle.X + 2f, middle.Y + 9f));
                        else if (Type == DiodeType.LED)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -2,-11 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 4,-4 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                        }
                        else if (Type == DiodeType.Photo)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -26,17 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -22,22 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                        }
                    }
                    else
                    {
                        dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m 0,-8 l 0,16 m 0,-8 l 15,8 l 0,-16 l -15,8", middle.X - 7f, middle.Y));
                        if (Type == DiodeType.Zener)
                            dc.DrawLine(color, 2f, new Point(middle.X - 8f, middle.Y - 9f), new Point(middle.X - 2f, middle.Y - 9f));
                        else if (Type == DiodeType.LED)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 2,11 l -8,8 m -1,1 l 4,-2 l -2,-2 l -2,4 l 4,-2", middle.X - 5.0, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -4,4 l -8,8 m -1,1 l 4,-2 l -2,-2 l -2,4 l 4,-2", middle.X - 5.0, middle.Y));
                        }
                        else if (Type == DiodeType.Photo)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 22,-22 l -8,8 m -1,1 l 4,-2 l -2,-2 l -2,4 l 4,-2", middle.X - 5.0, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 26,-17 l -8,8 m -1,1 l 4,-2 l -2,-2 l -2,4 l 4,-2", middle.X - 5.0, middle.Y));
                        }
                    }
                }
                else
                {
                    dc.DrawLine(color, 2.0f, RenderStartLocation, RenderEndLocation);
                    if (!IsFlipped)
                    {
                        dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l 16,0 m-8,0 l 8,-15 l-16,0 l 8,15", middle.X, middle.Y + 7f));
                        if (Type == DiodeType.Zener)
                            dc.DrawLine(color, 2f, new Point(middle.X + 9f, middle.Y + 8f), new Point(middle.X + 9f, middle.Y + 2f));
                        else if (Type == DiodeType.LED)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 11,-2 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y + 5d));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 4,4 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y + 5d));
                        }
                        else if (Type == DiodeType.Photo)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -22,-22 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y + 5d));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -26,-17 l 8,8 m 1,1 l -2,-4 l -2,2 l 4,2 l -2,-4", middle.X, middle.Y + 5d));
                        }
                    }
                    else
                    {
                        dc.DrawPath(null, color, 2.0f, String.Format("M {0},{1} m -8,0 l 16,0 m-8,0 l 8,15 l-16,0 l 8,-15", middle.X, middle.Y - 7f));
                        if (Type == DiodeType.Zener)
                            dc.DrawLine(color, 2f, new Point(middle.X - 9f, middle.Y - 8f), new Point(middle.X - 9f, middle.Y - 2f));
                        else if (Type == DiodeType.LED)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -2,-11 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m 4,-4 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                        }
                        else if (Type == DiodeType.Photo)
                        {
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -26,14 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                            dc.DrawPath(color, color, 2.0f, String.Format("M {0},{1} m -22,21 l 8,-8 m 1,-1 l -4,2 l 2,2 l 2,-4 l -4,2", middle.X + 5d, middle.Y));
                        }
                    }
                }
            }
        }

        public override void LoadData(System.Xml.XmlReader reader)
        {
            try
            {
                Type = DiodeType.Standard;
                Type = (DiodeType)int.Parse(reader.GetAttribute("t"));
            }
            catch (Exception)
            {
            }
        }

        public override void SaveData(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("t", ((int)Type).ToString());
        }

        public override void SaveData(System.IO.TextWriter writer)
        {
            base.SaveData(writer);
            EComponent.WriteProperty(writer, "t", ((int)Type).ToString());
        }

        public override void LoadData(System.IO.TextReader reader)
        {
            Dictionary<string, string> properties;
            base.LoadData(reader, out properties);
            if (properties.ContainsKey("t"))
                Type = (DiodeType)int.Parse(properties["t"]);
        }
    }

    public enum DiodeType
    {
        Standard,
        Zener,
        LED,
        Photo,
        Bridge
    }
}
