using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.Components.Render.Path
{
    class Path : IRenderCommand
    {
        public ComponentPoint Start { get; set; }
        public double Thickness { get; set; }
        public Color FillColour { get; set; }
        private IList<IPathCommand> m_commands;

        public IList<IPathCommand> Commands
        {
            get { return m_commands; }
        }

        public RenderCommandType Type
        {
            get { return RenderCommandType.Path; }
        }

        public Path()
        {
            Start = new ComponentPoint();
            Thickness = 2d;
            FillColour = Colors.Transparent;
            m_commands = new List<IPathCommand>();
        }

        public Path(ComponentPoint start, double thickness, Color fillColour, IList<IPathCommand> commands)
        {
            Start = start;
            Thickness = thickness;
            FillColour = fillColour;
            m_commands = commands;
        }

        private StreamGeometry GenerateGeometry(Component component)
        {
            Point resolvedStart = Start.Resolve(component);
            StreamGeometry sg = new StreamGeometry();
            using (var dc = sg.Open())
            {
                dc.BeginFigure(resolvedStart, false, false);
                foreach (IPathCommand command in m_commands)
                    command.Draw(dc, new Vector(resolvedStart.X, resolvedStart.Y));
            }
            return sg;
        }

        public void Render(Component component, DrawingContext dc, Color colour)
        {
            dc.DrawGeometry(new SolidColorBrush(FillColour), new Pen(new SolidColorBrush(colour), Thickness), GenerateGeometry(component));
        }

        public void Render(Component component, CircuitDiagram.Render.IRenderContext dc)
        {
            dc.DrawPath(Point.Add(Start.Resolve(component), new Vector(component.Offset.X, component.Offset.Y)), Commands, Thickness, (FillColour != Colors.Transparent && FillColour != Colors.White));
        }

        public static Geometry GetGeometry(Point start, IList<IPathCommand> commands)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (var dc = geometry.Open())
            {
                dc.BeginFigure(start, false, false);
                Vector startOffset = new Vector(start.X, start.Y);
                foreach (IPathCommand command in commands)
                {
                    command.Draw(dc, startOffset);
                }
                dc.Close();
            }
            return geometry;
        }
    }
}
