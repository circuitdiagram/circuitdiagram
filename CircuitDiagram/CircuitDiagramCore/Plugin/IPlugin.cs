using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Plugin
{
    /// <summary>
    /// Represents an individual plugin which can be enabled or disabled.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The elements belonging to this plugin.
        /// </summary>
        IList<IPluginPart> PluginParts { get; }
    }
}
