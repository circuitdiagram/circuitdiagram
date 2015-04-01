using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    class CustomToolboxItem : IToolboxItem
    {
        public string DisplayName { get; set; }

        public MultiResolutionImage Icon { get; set; }
    }
}
