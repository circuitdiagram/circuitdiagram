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

        /// <summary>
        /// The name of the property as it should be stored.
        /// </summary>
        public string SerializedName { get; private set; }

        /// <summary>
        /// The name of the property as it should be presented to the user.
        /// </summary>
        public string DisplayName { get; private set; }

        public ComponentSerializableAttribute(ComponentSerializeOptions options = ComponentSerializeOptions.None)
        {
            Options = options;
        }

        public ComponentSerializableAttribute(string serializedName)
        {
            SerializedName = serializedName;
        }

        public ComponentSerializableAttribute(string serializedName, string displayName)
        {
            SerializedName = serializedName;
            DisplayName = displayName;
        }

        public ComponentSerializableAttribute(ComponentSerializeOptions options, string displayName)
        {
            Options = options;
            DisplayName = displayName;
        }
    }

    [Flags]
    public enum ComponentSerializeOptions
    {
        None = 0,
        StoreLowercase = 1,
        DisplaySentenceCase = 2,
        DisplayAlignLeft = 4
    }
}
