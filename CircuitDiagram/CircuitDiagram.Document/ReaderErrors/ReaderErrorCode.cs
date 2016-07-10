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
    class ReaderErrorCode
    {
        private readonly ErrorType type;
        private readonly string code;
        private readonly string description;

        private ReaderErrorCode(ErrorType type, string code, string description)
        {
            this.type = type;
            this.code = code;
            this.description = description;
        }

        public static ReaderErrorCode Warning(string code, string description)
        {
            return new ReaderErrorCode(ErrorType.Warning, code, description);
        }

        public enum ErrorType
        {
            Info,
            Warning,
            Error
        }

        public string ToErrorMessage(int line, int pos, string parameter)
        {
            var message = $"[{type}] ({line}, {pos}) {code}: {description}";

            if (parameter != null)
                message += $" '{parameter}'";

            return message;
        }
    }
}
