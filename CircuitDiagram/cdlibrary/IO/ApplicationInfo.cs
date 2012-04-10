using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Stores information about the application which is using cdlibrary.dll.
    /// </summary>
    public static class ApplicationInfo
    {
        /// <summary>
        /// The full application name, including version.
        /// </summary>
        public static string FullName { get; set; }
        /// <summary>
        /// The application name, excluding version.
        /// </summary>
        public static string Name { get; set; }
        /// <summary>
        /// The application version.
        /// </summary>
        public static string Version { get; set; }
    }
}
