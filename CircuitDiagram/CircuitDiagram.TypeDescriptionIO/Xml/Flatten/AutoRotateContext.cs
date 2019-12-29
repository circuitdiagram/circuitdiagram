using CircuitDiagram.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public class AutoRotateContext
    {
        public AutoRotateContext(
            bool mirror,
            FlipType flipType,
            FlipState flipState)
        {
            Mirror = mirror;
            FlipType = flipType;
            FlipState = flipState;
        }

        /// <summary>
        /// Reflect in the line y=x.
        /// </summary>
        public bool Mirror { get; }

        public FlipType FlipType { get; }

        public FlipState FlipState { get; }
    }
}
