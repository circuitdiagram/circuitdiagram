using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    static class Flattener
    {
        public static IEnumerable<T> FlattenRoot<T>(this IRootFlattenable<T> rootFlattenable, IXmlLoadLogger logger)
        {
            if (rootFlattenable.AutoRotate == AutoRotateType.Off)
            {
                var autoRotateContext = new AutoRotateContext(false, FlipType.None, FlipState.None);
                var context = new FlattenContext(logger, ConditionTree.Empty, autoRotateContext);
                return rootFlattenable.Flatten(context);
            }
            else
            {
                var horizontalAutoRotateContext = new AutoRotateContext(false, FlipType.None, FlipState.None);
                var horizontalConditions = new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Equal, new PropertyValue(true));
                var horizontalContext = new FlattenContext(logger, horizontalConditions, horizontalAutoRotateContext);

                var flipType = FlipType.None;
                if ((rootFlattenable.AutoRotateFlip & FlipState.Primary) == FlipState.Primary)
                    flipType |= FlipType.Vertical;
                if ((rootFlattenable.AutoRotateFlip & FlipState.Secondary) == FlipState.Secondary)
                    flipType |= FlipType.Horizontal;

                var verticalAutoRotateContext = new AutoRotateContext(true, flipType, rootFlattenable.AutoRotateFlip);
                var verticalConditions = new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Equal, new PropertyValue(false));
                var verticalContext = new FlattenContext(logger, verticalConditions, verticalAutoRotateContext);

                return rootFlattenable.Flatten(horizontalContext).Concat(rootFlattenable.Flatten(verticalContext));
            }
        }
    }
}
