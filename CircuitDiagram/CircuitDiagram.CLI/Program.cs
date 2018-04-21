// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
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
using System.IO;
using System.Linq;
using System.Reflection;
using CircuitDiagram.CLI.Circuit;
using CircuitDiagram.CLI.Component;
using CircuitDiagram.CLI.Render;

namespace CircuitDiagram.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var command = Command.Help;

            IEnumerable<string> firstLevelArgs = args;
            if (args.Length == 0)
                firstLevelArgs = new[] {"-h"};
            if (args.Length > 1 && (args.Last() == "-h" || args.Last() == "--help"))
                firstLevelArgs = firstLevelArgs.SkipLast(1);

            if (firstLevelArgs.First() == "-h" || firstLevelArgs.First() == "--help")
            {
                PrintHeader();
            }

            ArgumentSyntax.Parse(firstLevelArgs, options =>
            {
                options.ApplicationName = "cdcli";
                options.ErrorOnUnexpectedArguments = false;
                
                options.DefineCommand("circuit", ref command, Command.Circuit, "Render a cddx circuit as an image.");
                options.DefineCommand("component", ref command, Command.Component, "Compile and render components.");
                options.DefineCommand("render", ref command, Command.Render, "Render a component as an image.");
            });
            
            switch (command)
            {
                case Command.Circuit:
                {
                    CircuitApp.Run(args.Skip(1).ToArray());
                    break;
                }
                case Command.Component:
                {
                    ComponentApp.Run(args.Skip(1).ToArray());
                    break;
                }
                case Command.Render:
                {
                    RenderApp.Run(args.Skip(1).ToArray());
                    break;
                }
            }
        }

        private static void PrintHeader()
        {
            Console.WriteLine("Circuit Diagram CLI Tool");

            var assemblyName = typeof(Program).Assembly.GetName();
            Console.WriteLine($"Version {assemblyName.Version}");
            Console.WriteLine("See http://www.circuit-diagram.org/ for more information.");
            Console.WriteLine();
        }
        
        enum Command
        {
            Help,
            Circuit,
            Component,
            Render,
        }
    }
}
