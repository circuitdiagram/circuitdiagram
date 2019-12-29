using CircuitDiagram.Circuit;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Render.TypeDescription.Conditions;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Connections
{
    public class XmlConnectionDescription : IFlattenable<ConnectionDescription>
    {
        public XmlConnectionDescription(
            ConnectionName name,
            XmlComponentPoint start,
            XmlComponentPoint end,
            ConditionalCollection<ConnectionEdge> edge)
        {
            Name = name;
            Start = start;
            End = end;
            Edge = edge;
        }

        public ConnectionName Name { get; }

        public XmlComponentPoint Start { get; }

        public XmlComponentPoint End { get; }

        public ConditionalCollection<ConnectionEdge> Edge { get; }

        public IEnumerable<Conditional<ConnectionDescription>> Flatten(FlattenContext context)
        {
            foreach (var start in Start.Flatten(context))
            {
                foreach (var end in End.Flatten(context))
                {
                    foreach(var edge in Edge)
                    {
                        var conditions = ConditionTreeBuilder.And(
                            start.Conditions,
                            end.Conditions,
                            edge.Conditions);

                        var connection = new ConnectionDescription(
                            start.Value,
                            end.Value,
                            edge.Value,
                            Name);

                        yield return new Conditional<ConnectionDescription>(connection, conditions);
                    }
                }
            }
        }
    }
}
