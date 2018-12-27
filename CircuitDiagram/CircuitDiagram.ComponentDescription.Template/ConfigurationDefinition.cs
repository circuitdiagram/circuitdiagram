using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescription.Definition
{
    public class ConfigurationDefinition
    {
        public ConfigurationDefinition(DefinitionMetadata metadata,
                                       ComponentDescription componentDescription,
                                       ComponentConfiguration configuration)
        {
            Metadata = metadata;
            ComponentDescription = componentDescription;
            Configuration = configuration;
        }

        public DefinitionMetadata Metadata { get; }

        public ComponentDescription ComponentDescription { get; }

        public ComponentConfiguration Configuration { get; }
    }
}
