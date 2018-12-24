using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.CLI.Component.Manifest
{
    class ComponentDescriptionManifestEntry : IManifestEntry
    {
        public string InputFile { get; set; }
        public string Author { get; set; }
        public string ComponentName { get; set; }
        public Guid ComponentGuid { get; set; }
        public bool Success { get; set; }
        public string Description { get; set; }
        public IReadOnlyDictionary<string, string> Metadata { get; set; }
        public IReadOnlyDictionary<string, string> OutputFiles { get; set; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ConfigurationOutputFiles { get; set; }
    }
}
