using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Compiler
{
    public class FileResource : IResource
    {
        private readonly string fileName;

        public FileResource(string fileName)
        {
            this.fileName = fileName;
            MimeType = FileExtensionToMimeType(Path.GetExtension(fileName));
        }

        public string MimeType { get; }

        public Stream Open()
        {
            return File.Open(fileName, FileMode.Open);
        }

        public static string FileExtensionToMimeType(string extension)
        {
            switch (extension)
            {
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
