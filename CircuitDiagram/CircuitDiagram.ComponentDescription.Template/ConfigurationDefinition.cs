using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescription.Template
{
    public class ConfigurationDefinition
    {
        public ConfigurationDefinition(Guid guid, ComponentDescription componentDescription, ComponentConfiguration configuration)
        {
            Guid = guid;
            ComponentDescription = componentDescription;
            Configuration = configuration;
        }

        public Guid Guid { get; }

        public ComponentDescription ComponentDescription { get; }

        public ComponentConfiguration Configuration { get; }
    }
}
