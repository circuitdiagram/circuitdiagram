using CircuitDiagram.Primitives;
using CircuitDiagram.Render.TypeDescription.Conditions;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Primitives
{
    public class XmlComponentPoint : IFlattenable<ComponentPoint>
    {
        public XmlComponentPoint(
            ComponentPosition relativeToX,
            ComponentPosition relativeToY,
            IEnumerable<IXmlComponentPointOffset> offsets)
        {
            RelativeToX = relativeToX;
            RelativeToY = relativeToY;
            Offsets = offsets.ToList();
        }

        public ComponentPosition RelativeToX { get; }

        public ComponentPosition RelativeToY { get; }

        public IReadOnlyList<IXmlComponentPointOffset> Offsets { get; }

        public IEnumerable<Conditional<ComponentPoint>> Flatten(FlattenContext context)
        {
            var allOffsets = Offsets.Select(x => x.Flatten(context));

            foreach (var offsets in allOffsets.CartesianProduct())
            {
                double xOffset = offsets.Where(x => x.Value.Axis == OffsetAxis.X != context.AutoRotate.Mirror).Sum(x => x.Value.Offset);
                double yOffset = offsets.Where(x => x.Value.Axis == OffsetAxis.Y != context.AutoRotate.Mirror).Sum(x => x.Value.Offset);
                var point = new ComponentPoint(RelativeToX, RelativeToX, new Vector(xOffset, yOffset)).Flip(context.AutoRotate.FlipType, context.AutoRotate.FlipState);

                var conditions = ConditionTreeBuilder.And(offsets.Select(x => x.Conditions));

                yield return new Conditional<ComponentPoint>(point, conditions);
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == null)
            {
                return null;
            }

            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) => accumulator.SelectMany(
                    accseq => sequence,
                    (accseq, item) => accseq.Concat(new[] { item })));
        }
    }
}
