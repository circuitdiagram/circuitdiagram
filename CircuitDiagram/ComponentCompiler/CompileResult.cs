// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;

namespace ComponentCompiler
{
    class CompileResult
    {
        public CompileResult(string author,
                             string componentName,
                             Guid guid,
                             bool success,
                             string description,
                             string input,
                             IReadOnlyDictionary<string, string> metadata,
                             IReadOnlyDictionary<string, string> outputs)
        {
            Author = author;
            ComponentName = componentName;
            Guid = guid;
            Success = success;
            Description = description;
            Input = input;
            Metadata = metadata;
            Outputs = outputs;
        }

        public string Author { get; }
        public string ComponentName { get; }
        public Guid Guid { get; }
        public bool Success { get; }
        public string Description { get; }
        public string Input { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }
        public IReadOnlyDictionary<string, string> Outputs { get; }
    }
}
