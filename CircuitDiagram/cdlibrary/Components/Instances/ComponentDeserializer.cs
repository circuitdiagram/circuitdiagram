using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    /// <summary>
    /// Converts a serialized component string to a collection of components.
    /// </summary>
    public class ComponentDeserializer
    {
        public List<Component> Components { get; private set; }

        public ComponentDeserializer(string serialized)
        {
            this.Components = new List<Component>();

            string[] components = serialized.Split(new string[] {"\r\n" } , StringSplitOptions.RemoveEmptyEntries);
            foreach (string component in components)
            {
                Component deserialized = Component.Create(component);
                deserialized.Location = System.Windows.Vector.Add(deserialized.Location, new System.Windows.Vector(10, 10));
                Components.Add(deserialized);
            }
        }
    }
}
