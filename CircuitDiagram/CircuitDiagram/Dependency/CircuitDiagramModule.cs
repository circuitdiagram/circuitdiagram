using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Modularity;

namespace CircuitDiagram.Dependency
{
    [Module(ModuleName = "CircuitDiagram.Base")]
    public class CircuitDiagramModule : IModule
    {
        private readonly IUnityContainer container;

        public CircuitDiagramModule(IUnityContainer container)
        {
            this.container = container;
        }

        public void Initialize()
        {
            
        }
    }
}
