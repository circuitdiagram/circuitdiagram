using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Specifies the type of save options to be used with an IODocument writer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SaveOptionsTypeAttribute : Attribute
    {
        /// <summary>
        /// The type of save options this writer can be configured with.
        /// </summary>
        public Type SaveOptionsType { get; set; }

        public SaveOptionsTypeAttribute(Type type)
        {
            SaveOptionsType = type;
        }
    }
}
