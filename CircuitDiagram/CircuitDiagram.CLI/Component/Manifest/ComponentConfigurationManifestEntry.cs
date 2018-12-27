using CircuitDiagram.TypeDescription.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.CLI.Component.Manifest
{
    class ComponentConfigurationManifestEntry : IManifestEntry
    {
        public DefinitionMetadata Metadata { get; set; }
        public string InputFile { get; set; }
        public Guid ComponentGuid { get; set; }
        public string ConfigurationName { get; set; }
        public IReadOnlyDictionary<string, string> OutputFiles { get; set; }
    }
}
