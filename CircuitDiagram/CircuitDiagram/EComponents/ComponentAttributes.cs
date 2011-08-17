using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.EComponents
{
    /// <summary>
    /// Specifies that the property should be included when the component is being saved and loaded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComponentSerializableAttribute : Attribute
    {
        public ComponentSerializeOptions Options { get; private set; }
        public string SerializedName { get; private set; }

        public ComponentSerializableAttribute(ComponentSerializeOptions options = ComponentSerializeOptions.None)
        {
            Options = options;
        }

        public ComponentSerializableAttribute(string serializedName)
        {
            SerializedName = serializedName;
        }
    }

    [Flags]
    public enum ComponentSerializeOptions
    {
        None = 0,
        Lowercase = 1
    }
}
