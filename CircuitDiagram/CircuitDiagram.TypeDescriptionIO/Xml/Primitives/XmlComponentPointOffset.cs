using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Primitives
{
    public class XmlComponentPointOffset : IXmlComponentPointOffset
    {
        public XmlComponentPointOffset(double offset, OffsetAxis axis)
        {
            Offset = offset;
            Axis = axis;
        }

        public double Offset { get; }

        public OffsetAxis Axis { get; }

        public IEnumerable<Conditional<ComponentPointOffset>> Flatten(FlattenContext context)
        {
            var offset = new ComponentPointOffset
            {
                Axis = Axis,
                Offset = Offset,
            };

            yield return new Conditional<ComponentPointOffset>(offset, ConditionTree.Empty);
        }
    }
}
