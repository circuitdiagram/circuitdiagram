using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Binary
{
    public enum BinaryResourceType : uint
    {
        None = 0,
        PNGImage = 1,
        BitmapImage = 2,
        JPEGImage = 3,
    }
}
