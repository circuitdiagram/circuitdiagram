using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides options for saving an IODocument to a particular format.
    /// </summary>
    public interface ISaveOptions
    {
        /// <summary>
        /// Serializes the save options to the specified serializer.
        /// </summary>
        /// <param name="serializer">The serialization object.</param>
        void Serialize(ISerializer serializer);

        /// <summary>
        /// Deserializes the save options from the specified serializer.
        /// </summary>
        /// <param name="serializer">The serialization object.</param>
        void Deserialize(ISerializer serializer);
    }
}
