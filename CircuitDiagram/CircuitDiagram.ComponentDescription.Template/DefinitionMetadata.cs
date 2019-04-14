using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescription.Definition
{
    public class DefinitionMetadata
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }        
        public string Description { get; set; }
    }
}
