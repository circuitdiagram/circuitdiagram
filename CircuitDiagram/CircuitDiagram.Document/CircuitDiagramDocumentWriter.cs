using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CircuitDiagram.Document.InternalWriter;
using CircuitDiagram.IO.Data;
using CircuitDiagram.IO.Document.InternalWriter;
using CircuitDiagram.IO.Write;

namespace CircuitDiagram.IO.Document
{
    /// <summary>
    /// Writes documents in the Circuit Diagram Document format.
    /// <see cref="http://www.circuit-diagram.org/help/cddx-file-format"/>
    /// </summary>
    public sealed class CircuitDiagramDocumentWriter : ICircuitWriter
    {
        private readonly PackageManager packageManager;
        private readonly MainDocumentWriter mainDocumentWriter;

        public CircuitDiagramDocumentWriter()
        {
            packageManager = new PackageManager();
            mainDocumentWriter = new MainDocumentWriter();
        }

        public string FileTypeName => "Circuit Diagram Document";

        public string FileTypeExtension => ".cddx";

        public Formatting Formatting
        {
            get { return mainDocumentWriter.Formatting; }
            set { mainDocumentWriter.Formatting = value; }
        }

        public void WriteCircuit(CircuitDocument document, Stream stream)
        {
            // Cannot write directly to a file stream, so write to this and copy later
            using (var ms = new MemoryStream())
            {
                // Create the package that contains the document files
                using (var package = Package.Open(ms, FileMode.Create))
                {
                    // Write the main document. This contains the connection and layout data.
                    using (var mainDocStream = packageManager.CreateMainDocumentPart(package))
                        mainDocumentWriter.Write(document, mainDocStream);

                    // Write the metadata. This contains document size, author, created time etc.

                    // Write any additional resources

                }

                ms.WriteTo(stream);
            }
        }
    }
}
