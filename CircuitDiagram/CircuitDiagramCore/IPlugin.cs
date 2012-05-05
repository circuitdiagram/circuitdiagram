using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    /// <summary>
    /// Represents an individual plugin which can be enabled or disabled.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The version of the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The author of the plugin.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// The GUID of the plugin.
        /// </summary>
        Guid GUID { get; }

        /// <summary>
        /// The elements belonging to this plugin.
        /// </summary>
        IList<IPluginPart> PluginParts { get; }
    }
}
