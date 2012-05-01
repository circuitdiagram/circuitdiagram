using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides a user interface to enable the user to modify save options.
    /// </summary>
    public class SaveOptionsEditor : UserControl
    {
        protected ISaveOptions Options { get; set; }

        /// <summary>
        /// Sets the default save options to use before the editor is shown.
        /// </summary>
        /// <param name="options">The default options to use.</param>
        public virtual void SetOptions(ISaveOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Gets the modified save options once the user has finished making changes.
        /// </summary>
        /// <returns></returns>
        public virtual ISaveOptions GetOptions()
        {
            return Options;
        }
    }
}
