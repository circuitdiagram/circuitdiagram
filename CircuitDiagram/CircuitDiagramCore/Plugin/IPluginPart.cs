using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Plugin
{
    /// <summary>
    /// Represents an element within a plugin.
    /// </summary>
    public interface IPluginPart
    {
        /// <summary>
        /// Gets the name of the plugin part.
        /// </summary>
        string PluginPartName { get; }
    }
}