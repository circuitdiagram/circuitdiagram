using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    class ComponentPointOffsetWithDefinition : IXmlComponentPointOffset
    {
        public ComponentPointOffsetWithDefinition(bool negative, ConditionalCollection<double> values, OffsetAxis axis)
        {
            Negative = negative;
            Values = values;
            Axis = axis;
        }

        public bool Negative { get; }

        public ConditionalCollection<double> Values { get; }

        public OffsetAxis Axis { get; }

        public IEnumerable<Conditional<ComponentPointOffset>> Flatten(FlattenContext context)
        {
            foreach(var value in Values)
            {
                var offset = new ComponentPointOffset
                {
                    Axis = Axis,
                    Offset = value.Value * (Negative ? -1 : 1),
                };
                yield return new Conditional<ComponentPointOffset>(offset, value.Conditions);
            }
        }
    }
}
