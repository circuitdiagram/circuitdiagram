using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    /// <summary>
    /// Converts multiple components to a string in a serialized format.
    /// </summary>
    public class ComponentSerializer
    {
        private StringBuilder m_builder;

        /// <summary>
        /// Creates a new ComponentSerializer;
        /// </summary>
        public ComponentSerializer()
        {
            m_builder = new StringBuilder();
        }

        /// <summary>
        /// Serializes the specified component.
        /// </summary>
        /// <param name="component"></param>
        public void AddComponent(Component component)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            component.Serialize(properties);
            string serialized = ComponentDataString.ConvertToString(properties, ",");
            m_builder.AppendLine(serialized);
        }

        /// <summary>
        /// Gets the component string.
        /// </summary>
        /// <returns>String containing serialized component data.</returns>
        public override string ToString()
        {
            return m_builder.ToString();
        }
    }
}
