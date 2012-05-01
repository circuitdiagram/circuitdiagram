// DocumentLoadResultType.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

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
