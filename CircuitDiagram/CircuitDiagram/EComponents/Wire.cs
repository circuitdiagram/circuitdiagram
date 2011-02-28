using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CircuitDiagram.EComponents
{
    public class Wire : EComponent
    {
        public Wire()
        {
        }

        public override void Render(IRenderer dc, Color color)
        {
            dc.DrawLine(color, 2.0f, StartLocation, EndLocation);
        }
    }
}
