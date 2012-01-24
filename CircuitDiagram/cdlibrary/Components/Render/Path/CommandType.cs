using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.Components.Render.Path
{
    public enum CommandType
    {
        MoveTo = 0,
        LineTo = 1,
        CurveTo = 2,
        SmoothCurveTo = 3,
        QuadraticBeizerCurveTo = 4,
        SmoothQuadraticBeizerCurveTo = 5,
        EllipticalArcTo = 6,
        ClosePath = 7
    }
}
