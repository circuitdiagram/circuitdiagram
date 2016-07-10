// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Threading.Tasks;

namespace CircuitDiagram.Document.ReaderErrors
{
    class ErrorInstance
    {
        private readonly ReaderErrorCode code;
        private readonly int lineNumber;
        private readonly int linePosition;
        private readonly string parameter;

        public ErrorInstance(ReaderErrorCode code,
                             int lineNumber,
                             int linePosition,
                             string parameter)
        {
            this.code = code;
            this.lineNumber = lineNumber;
            this.linePosition = linePosition;
            this.parameter = parameter;
        }

        public ReaderErrorCode Code => code;

        public override string ToString()
        {
            return code.ToErrorMessage(lineNumber, linePosition, parameter);
        }
    }
}
