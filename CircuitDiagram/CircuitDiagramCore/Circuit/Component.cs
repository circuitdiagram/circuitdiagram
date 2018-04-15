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
        {
            Type = type;
            Properties = new Dictionary<PropertyName, PropertyValue>();
            Connections = new Dictionary<ConnectionName, NamedConnection>();
        }

        /// <summary>
        /// Gets or sets the type of component.
        /// </summary>
        public ComponentType Type { get; set; }

        /// <summary>
        /// Additional properties for the component.
        /// </summary>
        public IDictionary<PropertyName, PropertyValue> Properties { get; }

        /// <summary>
        /// Connections for the component, in the format Name-ConnectionID.
        /// </summary>
        public IDictionary<ConnectionName, NamedConnection> Connections { get; }
        
        public override string ToString()
        {
            return string.Format("Component of type {0}", Type.CollectionItem);
        }
    }
}
