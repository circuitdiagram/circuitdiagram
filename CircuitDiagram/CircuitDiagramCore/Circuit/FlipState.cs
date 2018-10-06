using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Circuit
{
    [Flags]
    public enum FlipState
    {
        None = 0,
        Primary = 1,
        Secondary = 2,
        Both = 3,
    }
}
