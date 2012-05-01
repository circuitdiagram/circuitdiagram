using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Represents the type of load result.
    /// </summary>
    public enum DocumentLoadResultType
    {
        /// <summary>
        /// The type has not been set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The load succeeded with no problems.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The load failed for an unknown reason.
        /// </summary>
        FailUnknown = 2,

        /// <summary>
        /// The load failed becuase the document was in a newer format.
        /// </summary>
        FailNewerVersion = 3,

        /// <summary>
        /// The load failed because the document was not in the correct format.
        /// </summary>
        FailIncorrectFormat = 4,

        /// <summary>
        /// The load succeeded, but the document was in a newer format, so some format features may be unsupported.
        /// </summary>
        SuccessNewerVersion = 5
    }
}
