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
    public class Component : IConnectedElement
    {
        private readonly DependentDictionary<ConnectionName, NamedConnection> connections;

        public Component(ComponentType type)
            :this(type, null)
        {
        }

        public Component(ComponentType type, ComponentConfiguration configuration)
        {
            Type = type;
            Configuration = configuration;
            Properties = new List<ComponentProperty>();
            connections = new DependentDictionary<ConnectionName, NamedConnection>(type.ConnectionNames,
                cn => new NamedConnection(cn, this));
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
        public IReadOnlyDictionary<ConnectionName, NamedConnection> Connections => connections.ToDictionary(x => x.Key, x => x.Value);
    }
}
