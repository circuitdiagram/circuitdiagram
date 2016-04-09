using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Document;

namespace CircuitDiagram.IO.Document.InternalWriter
{
    class PackageManager
    {
        public Stream CreateMainDocumentPart(Package package)
        {
            var documentUri = PackUriHelper.CreatePartUri(new Uri(@"circuitdiagram\Document.xml", UriKind.Relative));
            var documentPart = package.CreatePart(documentUri, ContentTypeNames.MainDocument, CompressionOption.Normal);
            package.CreateRelationship(documentPart.Uri, TargetMode.Internal, RelationshipTypes.Document);

            return documentPart.GetStream(FileMode.Create);
        }
    }
}
