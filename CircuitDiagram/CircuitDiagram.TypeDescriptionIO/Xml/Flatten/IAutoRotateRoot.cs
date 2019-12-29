using CircuitDiagram.Circuit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public interface IAutoRotateRoot
    {
        /// <summary>
        /// Determines whether additional render commands are generated automatically to support component rotation.
        /// </summary>
        AutoRotateType AutoRotate { get; set; }

        /// <summary>
        /// Determines whether to apply a component flip when generating autorotated commands.
        /// </summary>
        FlipState AutoRotateFlip { get; set; }
    }
}
