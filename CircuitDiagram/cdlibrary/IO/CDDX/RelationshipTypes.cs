using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Holds the relationship types for a CDDX document.
    /// </summary>
    public static class RelationshipTypes
    {
        /// <summary>
        /// Relationship for the main document part.
        /// </summary>
        public const string Document = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/circuitDiagramDocument";

        /// <summary>
        /// Relationship for an embedded component, referenced from within the main document part.
        /// </summary>
        public const string IncludedComponent = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/component";

        /// <summary>
        /// Relationship for the embedded thumbnail.
        /// </summary>
        public const string Thumbnail = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/thumbnail";

        /// <summary>
        /// Relationship for the core properties.
        /// </summary>
        public const string CoreProperties = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/core-properties";

        /// <summary>
        /// Relationship for the extended properties.
        /// </summary>
        public const string ExtendedProperties = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/relationships/metadata/extended-properties";
    }
}
