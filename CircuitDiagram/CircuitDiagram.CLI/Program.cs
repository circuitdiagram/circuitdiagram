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
using System.Collections.Immutable;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;
using CircuitDiagram.CLI.Compiler;
using CircuitDiagram.CLI.Render;
using CircuitDiagram.Compiler;
using CircuitDiagram.Logging;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI
{
    class Program
    {
        private static readonly ILogger Log = LogManager.GetLogger<Program>();

        static void Main(string[] args)
        {
            IReadOnlyList<string> input = Array.Empty<string>();
            string singleInput = null;
            IReadOnlyList<string> resources = null;
            IReadOnlyList<string> additionalFormats = null;
            string output = null;
            string manifest = null;
            IReadOnlyList<string> componentDirectories = Array.Empty<string>();

            // Preview options
            bool autosize = false;
            double imgWidth = 640;
            double imgHeight = 480;

            bool recursive = false;
            bool silent = false;
            bool verbose = false;
            var command = Command.None;

            var cliOptions = ArgumentSyntax.Parse(args, options =>
            {
                options.ApplicationName = "cdcli";

                options.DefineCommand("version", ref command, Command.Version, "Prints the version of this application.");

                options.DefineCommand("compile", ref command, Command.Compile, "Compile components.");

                options.DefineOption("o|output", ref output,
                                     "Output file (the format will be inferred from the extension). Cannot be used for directory inputs or in combination with specific output format options.");
                var manifestOption = options.DefineOption("manifest", ref manifest, false, "Writes a manifest file listing the compiled components.");
                if (manifestOption.IsSpecified && manifest == null)
                    manifest = "manifest.xml";
                options.DefineOption("autosize", ref autosize, "Automatically sizes the output image to fit the rendered preview.");
                options.DefineOption("w|width", ref imgWidth, double.Parse, "Width of output images to generate (default=640).");
                options.DefineOption("h|height", ref imgHeight, double.Parse, "Height of output images to generate (default=480).");
                options.DefineOption("r|recursive", ref recursive, "Recursively searches sub-directories of the input directory.");
                options.DefineOption("s|silent", ref silent, "Does not output anything to the console on successful operation.");
                options.DefineOption("v|verbose", ref verbose, "Outputs extra information to the console.");
                options.DefineOptionList("resources", ref resources, "Resources to use in generating the output. Either a directory, or a space-separated list of [key] [filename] pairs.");
                options.DefineOptionList("format", ref additionalFormats, "Output formats to write.");
                options.DefineParameterList("input", ref input, "Components to compile.");

                options.DefineCommand("list-generators", ref command, Command.ListGenerators, "List the available output formats for compiling components.");

                options.DefineCommand("render", ref command, Command.Render, "Render a cddx circuit as an image.");
                options.DefineOption("o|output", ref output, "Path to output image file.");
                options.DefineOptionList("components", ref componentDirectories, "Paths to directories containing component definitions.");
                options.DefineParameter("input", ref singleInput, "Path to cddx circuit to render.");
            });

            if (!silent)
                LogManager.LoggerFactory.AddProvider(new BasicConsoleLogger(verbose ? LogLevel.Debug : LogLevel.Information));

            IResourceProvider resourceProvider = null;
            if (resources != null && resources.Count == 1)
            {
                string directory = resources.Single();
                if (!Directory.Exists(directory))
                    cliOptions.ReportError($"Directory '{directory}' used for --resources does not exist.");

                Log.LogDebug($"Using directory '{directory}' as resource provider.");
                resourceProvider = new DirectoryResourceProvider(resources.Single());
            }
            else if (resources != null && resources.Count % 2 == 0)
            {
                Log.LogDebug("Mapping resources as key-file pairs.");
                resourceProvider = new FileMapResourceProvider();
                for (int i = 0; i + 1 < resources.Count; i += 2)
                    ((FileMapResourceProvider)resourceProvider).Mappings.Add(resources[i], resources[i + 1]);
            }
            else if (resources != null)
            {
                cliOptions.ReportError("--resources must either be a directory or a space-separated list of [key] [filename] pairs.");
            }
            else
            {
                Log.LogDebug("Not supplying resources.");
                resourceProvider = new FileMapResourceProvider();
            }

            switch (command)
            {
                case Command.None:
                    cliOptions.ReportError("You must specify a command.");
                    Environment.Exit(1);
                    return;
                case Command.Version:
                    var assemblyName = typeof(Program).GetTypeInfo().Assembly.GetName();
                    Console.WriteLine($"Circuit Diagram CLI Tool {assemblyName.Version} ({assemblyName.ProcessorArchitecture})");
                    return;
                case Command.Compile:
                {
                    var app = new CompilerApp(LogManager.LoggerFactory, resourceProvider);
                    app.Run(input, recursive, manifest, output, autosize, imgWidth, imgHeight, additionalFormats);
                    return;
                }
                case Command.ListGenerators:
                {
                    foreach (var generator in new OutputGeneratorRepository().AllGenerators)
                    {
                        Console.WriteLine($"{generator.Format}: {generator.FileExtension}");
                    }

                    return;
                }
                case Command.Render:
                {
                    var app = new RenderApp();
                    app.Run(singleInput, output, componentDirectories.ToArray());
                    return;
                }
            }
        }

        enum Command
        {
            None,
            Version,
            Compile,
            ListGenerators,
            Render
        }
    }
}
