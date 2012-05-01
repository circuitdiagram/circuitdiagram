using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Specific options for text formatting.
    /// </summary>
    public enum TextRunFormattingType
    {
        /// <summary>
        /// The text should be rendered normally.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// The text should be rendered as subscript.
        /// </summary>
        Subscript = 2,

        /// <summary>
        /// The text should be rendered as superscript.
        /// </summary>
        Superscript = 4
    }
}
