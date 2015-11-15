using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Compiler;

namespace ComponentCompiler
{
    class MappedDirectoryResourceProvider : IResourceProvider
    {
        public MappedDirectoryResourceProvider(string directory)
        {
            Directory = directory;
            Mappings = new Dictionary<string, string>();
        }

        public string Directory { get; }

        public Dictionary<string, string> Mappings { get; } 

        public bool HasResource(string name)
        {
            return Mappings.ContainsKey(name) || File.Exists(Path.Combine(Directory, name));
        }

        public IResource GetResource(string name)
        {
            if (Mappings.ContainsKey(Path.GetFileNameWithoutExtension(name)))
                return new FileResource(Mappings[name]);

            if (File.Exists(Path.Combine(Directory, name)))
                return new FileResource(Path.Combine(Directory, name));

            throw new InvalidOperationException($"Resource {name} not found.");
        }
    }
}
