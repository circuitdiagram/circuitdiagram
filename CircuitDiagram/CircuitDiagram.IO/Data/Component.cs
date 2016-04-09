using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// Represents a component within a stored document format.
    /// </summary>
    public class Component : IElement
    {
        public Component(ComponentType type, ComponentConfiguration configuration)
        {
            Type = type;
            Configuration = configuration;
            Properties = new List<ComponentProperty>();
            Connections = type.ConnectionNames.Select(cn => new NamedConnection(cn, this)).ToArray();
        }

        /// <summary>
        /// Gets or sets the type of component.
        /// </summary>
        public ComponentType Type { get; }

        public ComponentConfiguration Configuration { get; set; }

        /// <summary>
        /// Additional properties for the component.
        /// </summary>
        public IList<ComponentProperty> Properties { get; }

        /// <summary>
        /// Connections for the component, in the format Name-ConnectionID.
        /// </summary>
        public IReadOnlyList<NamedConnection> Connections { get; }
    }
}
