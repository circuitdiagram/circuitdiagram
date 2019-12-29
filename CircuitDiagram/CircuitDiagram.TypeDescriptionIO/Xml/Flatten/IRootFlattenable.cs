using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public interface IRootFlattenable<T> : IAutoRotateRoot
    {
        IEnumerable<T> Flatten(FlattenContext context);
    }
}
