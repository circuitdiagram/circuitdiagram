using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    public class ComponentIdentifier
    {
        public ComponentDescription Description { get; set; }
        public ComponentConfiguration Configuration { get; set; }

        public ComponentIdentifier()
        {
        }

        public ComponentIdentifier(ComponentDescription description)
        {
            Description = description;
        }

        public ComponentIdentifier(ComponentDescription description, ComponentConfiguration configuration)
        {
            Description = description;
            Configuration = configuration;
        }
    }
}
