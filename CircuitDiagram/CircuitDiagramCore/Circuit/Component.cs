using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Circuit
{
    /// <summary>
    /// Represents an element in a circuit that has it's own behaviour and a defined set of properties.
    /// </summary>
    public class Component : IConnectedElement
    {
        public Component(ComponentType type)
            :this(type, null)
        {
        }

        public Component(ComponentType type, ComponentConfiguration configuration)
        {
            Type = type;
            Configuration = configuration;
            Properties = new DependentDictionary<PropertyName, PropertyValue>(type.PropertyNames, p => new PropertyValue(), allowOther: true);
            Connections = new DependentDictionary<ConnectionName, NamedConnection>(type.ConnectionNames,
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
        public IDictionary<PropertyName, PropertyValue> Properties { get; }

        /// <summary>
        /// Connections for the component, in the format Name-ConnectionID.
        /// </summary>
        public IReadOnlyDictionary<ConnectionName, NamedConnection> Connections { get; }

        public CollectionType GetCollectionType()
        {
            return Configuration != null ? new CollectionType(Type.Collection, Configuration.Implements) : Type;
        }

        public override string ToString()
        {
            return string.Format("Component of type {0}", Type.Name.Value);
        }
    }
}
