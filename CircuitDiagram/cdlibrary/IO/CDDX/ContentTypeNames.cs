
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Holds the content types for parts of a CDDX document.
    /// </summary>
    public static class ContentTypeNames
    {
        /// <summary>
        /// The entire CDDX file.
        /// </summary>
        public const string CDDX = "application/vnd.circuitdiagram.document";

        /// <summary>
        /// The main document part.
        /// </summary>
        public const string MainDocument = "application/vnd.circuitdiagram.document.main+xml";

        /// <summary>
        /// The core properties part.
        /// </summary>
        public const string CoreProperties = "application/vnd.circuitdiagram.document.core-properties+xml";

        /// <summary>
        /// The extended properties part.
        /// </summary>
        public const string ExtendedProperties = "application/vnd.circuitdiagram.document.extended-properties+xml";
    }
}
