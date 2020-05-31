using CircuitDiagram.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.Render
{
    public class MissingComponentDescriptionException : Exception
    {
        public MissingComponentDescriptionException(ComponentType missingType)
            : base($"No component description available for {missingType}")
        {
            MissingType = missingType;
        }

        public ComponentType MissingType { get; }
    }
}
