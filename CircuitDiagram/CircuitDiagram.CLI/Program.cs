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
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CircuitDiagram.CLI.Circuit;
using CircuitDiagram.CLI.Component;
using CircuitDiagram.CLI.ComponentOutline;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await BuildCommandLine()
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            return new CommandLineBuilder()
                .AddGlobalOption(SilentOption)
                .AddGlobalOption(VerboseOption)
                .AddCommand(CircuitApp.BuildCommand())
                .AddCommand(ComponentApp.BuildCommand())
                .AddCommand(ComponentOutlineApp.BuildCommand());
        }

        private static Option<bool> SilentOption = new Option<bool>("-s", "Does not output anything to the console.")
        {
            Name = "silent",
        };

        private static Option<bool> VerboseOption = new Option<bool>("-v", "Outputs extra information to the console.")
        {
            Name = "verbose",
        };
    }
}
