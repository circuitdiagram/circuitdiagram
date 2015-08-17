using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace CircuitDiagram
{
    class IdentifierWithShortcut : IToolboxItem
    {
        public string DisplayName
        {
            get
            {
                return Identifier.Configuration != null ? Identifier.Configuration.Name : Identifier.Description.ComponentName;
            }
        }

        public MultiResolutionImage Icon
        {
            get
            {
                return Identifier.Configuration != null && Identifier.Configuration.Icon != null ? Identifier.Configuration.Icon : Identifier.Description.Metadata.Icon;
            }
        }

        public ComponentIdentifier Identifier { get; set; }
        public Key ShortcutKey { get; set; }

        public override string ToString()
        {
            return Identifier.ToString();
        }
    }
}
