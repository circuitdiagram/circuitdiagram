using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescription.Template
{
    public class ConfigurationDefinition
    {
        public ConfigurationDefinition(ComponentDescription componentDescription, ComponentConfiguration configuration)
        {
            ComponentDescription = componentDescription;
            Configuration = configuration;
        }

        public ComponentDescription ComponentDescription { get; }

        public ComponentConfiguration Configuration { get; }
    }
}
