// EComponent.cs
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
using System.Xml;

namespace CircuitDiagram
{
    public abstract class EComponent
    {
        #region Properties
        public bool CanResize { get; protected set; }
        public bool CanFlip { get; protected set; }
        public bool IsFlipped { get; set; }

        private static EditComponentWindow EditWindow { get; set; }

        private Point m_startLocation;
        private Point m_endLocation;

        public Point StartLocation
        {
            get { return m_startLocation; }
            set { m_startLocation = value; }
        }

        public Point EndLocation
        {
            get { return m_endLocation; }
            set { m_endLocation = value; }
        }

        protected Point RenderStartLocation
        {
            get
            {
                if (Horizontal)
                    return new Point(StartLocation.X - 1, StartLocation.Y);
                else
                    return new Point(StartLocation.X, StartLocation.Y - 1);
            }
        }

        protected Point RenderEndLocation
        {
            get
            {
                if (Horizontal)
                    return new Point(EndLocation.X + 1, EndLocation.Y);
                else
                    return new Point(EndLocation.X, EndLocation.Y + 1);
            }
        }

        public virtual Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(RenderStartLocation.X, RenderStartLocation.Y - 4), new Size(RenderEndLocation.X - RenderStartLocation.X, 8));
                else
                    return new Rect(new Point(RenderStartLocation.X - 4, RenderStartLocation.Y), new Size(8, RenderEndLocation.Y - RenderStartLocation.Y));
            }
        }

        protected ComponentEditor Editor { get; set; }

        public bool Horizontal { get { return StartLocation.Y == EndLocation.Y; } }
        #endregion

        public EComponent()
        {
            StaticInitialize();
            IsFlipped = false;
            CanFlip = false;
            CanResize = true;
            Initialize();
        }

        private static void StaticInitialize()
        {
            if (EditWindow == null)
            {
                EditWindow = new EditComponentWindow();
                EditWindow.Closing += new System.ComponentModel.CancelEventHandler(EditWindow_Closing);
            }
        }

        public static Window EditWindowParent { get { return EditWindow.Owner; } set { EditWindow.Owner = value; } }

        static void EditWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            EditWindow.Hide();
        }

        public virtual void Initialize()
        {
            Editor = new BasicComponentEditor();
            Editor.Title = this.GetType().Name.ToString() + " Properties";
        }

        public void ShowEditor()
        {
            Editor.Title = this.GetType().Name;
            Editor.LoadComponent(this);
            EditWindow.SetEditor(Editor);
            EditWindow.ShowDialog();
            if (EditWindow.CustomResult == true)
            {
                Editor.UpdateChanges(this);
            }
        }

        private static double Biggest(double one, double two)
        {
            return (one >= two ? one : two);
        }

        private static double Smallest(double one, double two)
        {
            return (one < two ? one : two);
        }

        private static Point Snap(Point point, double gridSize)
        {
            return new Point(Math.Round(point.X / gridSize) * gridSize, Math.Round(point.Y / gridSize) * gridSize);
        }

        public void UpdateLayout(CircuitDocument document)
        {
            // reverse points if necessary
            Point newStart = this.StartLocation;
            Point newEnd = this.EndLocation;
            bool switched = false;
            if (this.StartLocation.X < this.EndLocation.X)
            {
                newStart = this.EndLocation;
                newEnd = this.StartLocation;
                switched = true;
            }

            if (document.SnapToGrid) // snap to grid
            {
                if (Math.IEEERemainder(newStart.X, 20d) != 0)
                    newStart.X = Snap(newStart, document.GridSize).X;
                if (Math.IEEERemainder(newStart.Y, 20d) != 0)
                    newStart.Y = Snap(newStart, document.GridSize).Y;
                if (Math.IEEERemainder(newEnd.X, 20d) != 0)
                    newEnd.X = Snap(newEnd, document.GridSize).X;
                if (Math.IEEERemainder(newEnd.Y, 20d) != 0)
                    newEnd.Y = Snap(newEnd, document.GridSize).Y;
            }
            if (document.SnapToHV) // snap to horizontal or vertical
            {
                double height = Biggest(newStart.Y, newEnd.Y) - Smallest(newStart.Y, newEnd.Y);
                double length = Math.Sqrt(Math.Pow(newEnd.X - newStart.X, 2d) + Math.Pow(newEnd.Y - newStart.Y, 2d));
                double bearing = Math.Acos(height / length) * (180 / Math.PI);

                if (bearing <= 45 && switched)
                    newStart.X = newEnd.X;
                else if (bearing <= 45 && !switched)
                    newEnd.X = newStart.X;
                else if (bearing > 45 && switched)
                    newStart.Y = newEnd.Y;
                else
                    newEnd.Y = newStart.Y;
            }

            if (newStart.X > newEnd.X || newStart.Y > newEnd.Y)
            {
                this.StartLocation = newEnd;
                this.EndLocation = newStart;
            }
            else
            {
                this.StartLocation = newStart;
                this.EndLocation = newEnd;
            }

            ImplementMinimumSize(document.GridSize);
            CustomUpdateLayout();
        }

        protected void ImplementMinimumSize(double size)
        {
            if (Horizontal && EndLocation.X - StartLocation.X < size)
                EndLocation = new Point(StartLocation.X + size, EndLocation.Y);
            else if (!Horizontal && EndLocation.Y - StartLocation.Y < size)
                EndLocation = new Point(EndLocation.X, StartLocation.Y + size);
        }

        protected virtual void CustomUpdateLayout()
        {
        }

        public abstract void Render(IRenderer dc, Color color);

        public virtual void SaveData(XmlWriter writer)
        {
        }

        public virtual void LoadData(XmlReader reader)
        {
        }

        public virtual void SaveData(System.IO.TextWriter writer)
        {
            WriteProperty(writer, "type", this.GetType().Name);
            WriteProperty(writer, "x", StartLocation.X.ToString());
            WriteProperty(writer, "y", StartLocation.Y.ToString());
            if (CanResize)
            {
                if (Horizontal)
                    WriteProperty(writer, "size", (EndLocation.X - StartLocation.X).ToString());
                else
                    WriteProperty(writer, "size", (EndLocation.Y - StartLocation.Y).ToString());
            }
           WriteProperty(writer, "orientation", (Horizontal ? "horizontal" : "vertical"));
            if (CanFlip)
                WriteProperty(writer, "flipped", IsFlipped.ToString());
        }

        public virtual void LoadData(System.IO.TextReader reader)
        {
            Dictionary<string, string> properties = LoadProperties(reader);
            if (properties.ContainsKey("x"))
                m_startLocation.X = double.Parse(properties["x"]);
            if (properties.ContainsKey("y"))
                m_startLocation.Y = double.Parse(properties["y"]);
            bool horizontal = true;
            if (properties.ContainsKey("orientation") && properties["orientation"].ToLower() == "vertical")
                horizontal = false;
            if (properties.ContainsKey("size"))
            {
                double size = double.Parse(properties["size"]);
                if (horizontal)
                {
                    m_endLocation.X = m_startLocation.X + size;
                    m_endLocation.Y = m_startLocation.Y;
                }
                else
                {
                    m_endLocation.X = m_startLocation.X;
                    m_endLocation.Y = m_startLocation.Y + size;
                }
            }
            IsFlipped = false;
            if (properties.ContainsKey("flipped") && properties["flipped"].ToLower() == "true")
                IsFlipped = true;
        }

        public void LoadData(System.IO.TextReader reader, out Dictionary<string, string> properties)
        {
            properties = LoadProperties(reader);
            if (properties.ContainsKey("x"))
                m_startLocation.X = double.Parse(properties["x"]);
            if (properties.ContainsKey("y"))
                m_startLocation.Y = double.Parse(properties["y"]);
            bool horizontal = true;
            if (properties.ContainsKey("orientation") && properties["orientation"].ToLower() == "vertical")
                horizontal = false;
            if (properties.ContainsKey("size"))
            {
                double size = double.Parse(properties["size"]);
                if (horizontal)
                {
                    m_endLocation.X = m_startLocation.X + size;
                    m_endLocation.Y = m_startLocation.Y;
                }
                else
                {
                    m_endLocation.X = m_startLocation.X;
                    m_endLocation.Y = m_startLocation.Y + size;
                }
            }
            IsFlipped = false;
            if (properties.ContainsKey("flipped") && properties["flipped"].ToLower() == "true")
                IsFlipped = true;
        }

        public static Dictionary<string, string> LoadProperties(System.IO.TextReader reader)
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                string[] parameters = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parameters.Length >= 2)
                    returnDict.Add(parameters[0], parameters[1]);
                line = reader.ReadLine();
            }
            return returnDict;
        }

        public static void WriteProperty(System.IO.TextWriter writer, string key, string value)
        {
            writer.WriteLine("{0}:{1}", key, value);
        }
    }
}
