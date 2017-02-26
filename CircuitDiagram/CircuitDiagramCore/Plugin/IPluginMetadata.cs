using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Plugin
{
    public interface IPluginMetadata
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The version of the plugin.
        /// </summary>
        string Version { get; }

        ///// <summary>
        ///// The author of the plugin.
        ///// </summary>
        string Author { get; }

        ///// <summary>
        ///// The GUID of the plugin.
        ///// </summary>
        string Guid { get; }
    }
}
