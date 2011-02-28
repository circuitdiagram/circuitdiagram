using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CircuitDiagram
{
    public class ComponentEditor : UserControl
    {
        public string Title { get; set; }

        public ComponentEditor()
        {
            
        }

        public virtual void LoadComponent(EComponent component)
        {
        }

        public virtual void UpdateChanges(EComponent component)
        {
        }
    }
}
