// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2019  Samuel Fisher
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
using System.IO;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;

namespace CircuitDiagram.CLI.Component
{
    static class PreviewGenerationOptionsReader
    {
        public static void Read(string path, PreviewGenerationOptions options)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var reader = new StreamReader(fs);

                while (!reader.EndOfStream)
                {
                    var tokens = reader.ReadLine().Split('=');

                    if (tokens.Length < 2)
                        continue;

                    switch (tokens[0].Trim())
                    {
                        case "horizontal":
                            options.Horizontal = bool.Parse(tokens[1]);
                            break;
                        case "configuration":
                            options.Configuration = tokens[1];
                            break;
                        default:
                            if (tokens[0].StartsWith("$"))
                                options.Properties[tokens[0].Substring(1)] = tokens[1];
                            break;
                    }
                }
            }
        }
    }
}
