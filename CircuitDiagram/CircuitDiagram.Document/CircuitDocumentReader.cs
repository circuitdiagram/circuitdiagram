using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document.InternalReader;
using CircuitDiagram.IO;

namespace CircuitDiagram.Document
{
    public class CircuitDocumentReader : ICircuitReader
    {
        private readonly MainDocumentReader mainDocumentReader = new MainDocumentReader();

        public string FileTypeName => "Circuit Diagram Document";

        public string FileTypeExtension => ".cddx";

        public CircuitDiagramDocument ReadCircuit(Stream stream)
        {
            var document = new CircuitDiagramDocument();

            mainDocumentReader.Read(document, stream);

            return document;
        }

        CircuitDocument ICircuitReader.ReadCircuit(Stream stream)
        {
            return ReadCircuit(stream);
        }
    }
}
