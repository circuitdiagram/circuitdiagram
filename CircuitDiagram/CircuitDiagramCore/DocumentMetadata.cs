using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    /// <summary>
    /// Represents the metadata for a circuit document.
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// Individual or entity that created the document.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// Name given to the document.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the content of the document.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date the document was created.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The date the document was last changed.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// The application that created the document.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// The version of the application that created the document.
        /// </summary>
        public string AppVersion { get; set; }
    }
}
