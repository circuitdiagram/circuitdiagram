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
        private static EditComponentWindow EditWindow { get; set; }

        public virtual double MinimumWidth { get { return 10.0f; } }

        public Point m_startLocation;
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

        public virtual Rect BoundingBox
        {
            get
            {
                if (Horizontal)
                    return new Rect(new Point(StartLocation.X, StartLocation.Y - 4), new Size(EndLocation.X - StartLocation.X, 8));
                else
                    return new Rect(new Point(StartLocation.X - 4, StartLocation.Y), new Size(8, EndLocation.Y - StartLocation.Y));
            }
        }

        protected ComponentEditor Editor { get; set; }

        public bool Horizontal { get { return StartLocation.Y == EndLocation.Y; } }

        public EComponent()
        {
            StaticInitialize();
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

        public abstract void Render(IRenderer dc, Color color);

        public virtual bool Intersects(Point point)
        {
            Rect thisRect = new Rect(StartLocation, EndLocation - StartLocation);
            return thisRect.IntersectsWith(new Rect(point, new Size(1, 1)));
        }

        public virtual void SaveData(XmlWriter writer)
        {
        }

        public virtual void LoadData(XmlReader reader)
        {
            
        }
    }
}
