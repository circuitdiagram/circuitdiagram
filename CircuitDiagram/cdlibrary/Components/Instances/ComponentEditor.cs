using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components
{
    public delegate IComponentEditor CreateComponentEditorDelegate(Component component);

    public interface IComponentEditor
    {
        event ComponentUpdatedDelegate ComponentUpdated;
        void Update();
    }

    public delegate void ComponentUpdatedDelegate(object sender, ComponentUpdatedEventArgs e);

    public class ComponentUpdatedEventArgs : EventArgs
    {
        public Component Component { get; private set; }
        public string PreviousData { get; private set; }

        public ComponentUpdatedEventArgs(Component component, string previousData)
        {
            Component = component;
            PreviousData = previousData;
        }
    }
}
