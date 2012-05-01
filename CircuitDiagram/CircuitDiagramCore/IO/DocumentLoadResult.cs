using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Holds information about the load operation of a document.
    /// </summary>
    public class DocumentLoadResult
    {
        /// <summary>
        /// The type of result.
        /// </summary>
        public DocumentLoadResultType Type { get; set; }

        /// <summary>
        /// The file format of the document.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// A list of errors that occurred while loading.
        /// </summary>
        public List<string> Errors { get; private set; }

        /// <summary>
        /// Creates a new DocumentLoadResult.
        /// </summary>
        public DocumentLoadResult()
        {
            Type = DocumentLoadResultType.None;
            Errors = new List<string>();
        }
    }
}
