using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Stores namespaces that are used in CDDX files.
    /// </summary>
    public static class Namespaces
    {
        /// <summary>
        /// The XML namespace used for the main document.
        /// </summary>
        public const string Document = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/document";

        /// <summary>
        /// The XML namespace used for references to component descriptions within the main document.
        /// </summary>
        public const string DocumentComponentDescriptions = "http://schemas.circuit-diagram.org/circuitDiagramDocument/2012/document/component-descriptions";

        /// <summary>
        /// The XML namespace used for OPC relationships.
        /// </summary>
        public const string Relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        /// <summary>
        /// The namespace for the Dublin Core elements.
        /// </summary>
        public const string DublinCore = "http://purl.org/dc/terms/";

        /// <summary>
        /// The namespace for the Dublin Core Terms elements.
        /// </summary>
        public const string DublinCoreTerms = "http://purl.org/dc/terms/";
    }
}
