using CircuitDiagram.Drawing;
using CircuitDiagram.Render.TypeDescription.Conditions;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions
{
    class XmlRectCommandWithDefinitions : XmlRectCommand
    {
        public new ConditionalCollection<double> Width { get; set; }

        public override IEnumerable<Conditional<IRenderCommand>> Flatten(FlattenContext context)
        {
            foreach (var location in Location.Flatten(context))
            {
                foreach (var width in Width)
                {
                    var conditions = ConditionTreeBuilder.And(new[]
                    {
                        location.Conditions,
                        width.Conditions,
                    });

                    var command = new Rectangle(
                        location.Value,
                        width.Value,
                        Height,
                        StrokeThickness,
                        Fill);

                    yield return new Conditional<IRenderCommand>(command, conditions);
                }
            }
        }
    }
}
