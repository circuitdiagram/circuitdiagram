using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides options for saving an IODocument to a particular format, and allows the user to modify the options.
    /// </summary>
    public interface IUserSaveOptions : ISaveOptions
    {
        /// <summary>
        /// Returns an editor that can be used by the user to modify the save options.
        /// </summary>
        /// <returns>The editor object.</returns>
        SaveOptionsEditor GetVisualEditor();
    }
}
