using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public enum AutoRotateType
    {
        /// <summary>
        /// Do not generate autorotated commands.
        /// </summary>
        Off,

        /// <summary>
        /// Commands are horizontal, generate vertical commands.
        /// </summary>
        HorizontalToVertical,
    }
}
