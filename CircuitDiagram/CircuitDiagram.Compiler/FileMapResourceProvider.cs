using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Compiler
{
    public class FileMapResourceProvider : IResourceProvider
    {
        public FileMapResourceProvider()
        {
            Mappings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Mappings { get; }

        public virtual bool HasResource(string name)
        {
            return Mappings.ContainsKey(name);
        }

        public virtual IResource GetResource(string name)
        {
            return new FileResource(Mappings[name]);
        }
    }
}
