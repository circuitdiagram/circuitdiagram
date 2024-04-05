using System.Collections.Generic;
using CircuitDiagram.Drawing;
using CircuitDiagram.Render.TypeDescription.Conditions;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Render;

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

                    double flatWidth = context.AutoRotate.Mirror ? Height : width.Value;
                    double flatHeight = context.AutoRotate.Mirror ? width.Value: Height;

                    var command = new Rectangle(
                        location.Value,
                        flatWidth,
                        flatHeight,
                        StrokeThickness,
                        Fill);

                    yield return new Conditional<IRenderCommand>(command, conditions);
                }
            }
        }
    }
}
