// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Render;
using CircuitDiagram.View.Services;
using CircuitDiagram.View.ToolboxView;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace CircuitDiagram.View.Dependency
{
    [Module(ModuleName = "CircuitDiagram.View")]
    [ModuleDependency("CircuitDiagram.Base")]
    public class ViewModule : IModule
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;

        public ViewModule(IUnityContainer container, IRegionManager regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            container.RegisterType<ComponentDescriptionService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IComponentDescriptionService, ComponentDescriptionService>();
            container.RegisterType<IComponentDescriptionLookup, IComponentDescriptionService>();
            container.RegisterType<IToolboxReader, XmlToolboxReader>();
            container.RegisterType<IConfigurationValues, ConfigurationValues>(new ContainerControlledLifetimeManager());
            container.RegisterType<IComponentIconProvider, ComponentDescriptionService>();
            container.RegisterType<IDocumentService, DocumentService>();

            regionManager.Regions["MainRegion"].Add(new MainView());
        }
    }
}
