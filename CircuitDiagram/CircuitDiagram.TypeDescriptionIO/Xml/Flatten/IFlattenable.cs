using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public interface IFlattenable<T>
    {
        IEnumerable<Conditional<T>> Flatten(FlattenContext context);
    }
}
