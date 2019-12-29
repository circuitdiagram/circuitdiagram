using CircuitDiagram.Drawing;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Render
{
    public class XmlLineCommand : IXmlRenderCommand
    {
        public XmlComponentPoint Start { get; set; }

        public XmlComponentPoint End { get; set; }

        public double Thickness { get; set; } = 2.0;

        public IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            foreach (var start in Start.Flatten(context))
            {
                foreach(var end in End.Flatten(context))
                {
                    var command = new Line(start.Value, end.Value, Thickness);
                    var conditions = new ConditionTree(
                        ConditionTree.ConditionOperator.AND,
                        start.Conditions,
                        end.Conditions);

                    yield return new Conditional<IRenderCommand>(command, conditions);
                }
            }
        }
    }
}
