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
        public Wire(LayoutInformation layout)
        {
            Layout = layout;
        }

        public LayoutInformation Layout { get; }
    }
}
