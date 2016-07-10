using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Conditions.Parsers
{
    /// <summary>
    /// Specifies the format options used to parse conditions.
    /// </summary>
    public class ConditionFormat
    {
        /// <summary>
        /// Gets or sets a value indicating whether states should be preceeded by an underscore.
        /// </summary>
        public bool StatesUnderscored { get; set; }
    }
}
