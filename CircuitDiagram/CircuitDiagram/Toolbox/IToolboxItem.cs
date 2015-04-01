using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    interface IToolboxItem
    {
        string DisplayName { get; }

        MultiResolutionImage Icon { get; }
    }
}
