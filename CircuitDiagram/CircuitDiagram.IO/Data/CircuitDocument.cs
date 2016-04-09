using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    public class CircuitDocument
    {
        public CircuitDocument()
        {
            Elements = new List<IElement>();
        }

        public Size Size { get; set; }

        public ICollection<IElement> Elements { get; }

        public IEnumerable<IPositionalElement> PositionalElements => Elements.Where(el => el is IPositionalElement).Cast<IPositionalElement>();

        public IEnumerable<Component> Components => Elements.Where(el => el is Component).Cast<Component>();
    }
}
