using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public class Wire : IPositionalElement
    {
        public Wire(ElementLocation location)
        {
            Layout = new LayoutInformation(location);
        }

        public LayoutInformation Layout { get; }
    }
}
