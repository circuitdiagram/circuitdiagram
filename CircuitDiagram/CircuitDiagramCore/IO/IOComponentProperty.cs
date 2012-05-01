using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents an additional component property.
    /// </summary>
    public class IOComponentProperty
    {
        /// <summary>
        /// The property key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The property value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Whether the property is part of the standard component implementation.
        /// </summary>
        public bool IsStandard { get; set; }

        /// <summary>
        /// Creates a new IOComponentProperty.
        /// </summary>
        public IOComponentProperty()
        {
        }

        /// <summary>
        /// Creates a new IOComponentProperty with the specified parameters.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <param name="isStandard">Whether the property is part of the standard component implementation.</param>
        public IOComponentProperty(string key, object value, bool isStandard)
        {
            Key = key;
            Value = value;
            IsStandard = isStandard;
        }
    }
}
