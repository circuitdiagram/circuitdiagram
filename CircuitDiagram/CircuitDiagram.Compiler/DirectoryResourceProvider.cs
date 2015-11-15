using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Compiler
{
    public class DirectoryResourceProvider : IResourceProvider
    {
        private readonly string directory;

        public DirectoryResourceProvider(string directory)
        {
            this.directory = directory;
        }

        public bool HasResource(string name)
        {
            return File.Exists(Path.Combine(directory, name));
        }

        public IResource GetResource(string name)
        {
            if (!HasResource(name))
                throw new InvalidOperationException($"Resource {name} not found.");

            return new FileResource(Path.Combine(directory, name));
        }
    }
}
