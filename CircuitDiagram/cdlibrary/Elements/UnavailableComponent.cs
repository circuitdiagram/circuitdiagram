using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Elements
{
    /// <summary>
    /// Represents a component which cannot be modified.
    /// </summary>
    public class UnavailableComponent : IComponentElement
    {
        public string ImplementationCollection
        {
            get { throw new NotImplementedException(); }
        }

        public string ImplementationItem
        {
            get { throw new NotImplementedException(); }
        }

        public double Size
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFlipped
        {
            get { throw new NotImplementedException(); }
        }

        public Orientation Orientation
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, object> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public System.Windows.Vector Location
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler Updated;

        public void Render(Render.IRenderContext dc, bool absolute)
        {
            throw new NotImplementedException();
        }
    }
}
