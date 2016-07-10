using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Circuit
{
    public class Wire : IPositionalElement
    {
        public Wire(Point location)
        {
            Layout = new LayoutInformation(location);
        }

        public LayoutInformation Layout { get; }
    }
}
