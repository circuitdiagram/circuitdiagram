using System;
using System.Collections.Generic;
using System.Text;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Circuit
{
    public static class LayoutInformationExtensions
    {
        public static FlipType GetFlipType(this LayoutInformation layout)
        {
            switch (layout.Flip)
            {
                case FlipState.None:
                    return FlipType.None;
                case FlipState.Primary:
                    return layout.Orientation == Orientation.Horizontal ? FlipType.Horizontal : FlipType.Vertical;
                case FlipState.Secondary:
                    return layout.Orientation == Orientation.Horizontal ? FlipType.Vertical : FlipType.Horizontal;
                default:
                    return FlipType.Both;
            }
        }
    }
}
