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
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Compiler;
using CircuitDiagram.IO;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.CLI;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.CLI.Compiler.OutputGenerators;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI.Compiler
{
    public class CompilerApp
    {
        private readonly ILogger logger;
        private readonly IResourceProvider resourceProvider;

        public CompilerApp(ILoggerFactory loggerFactory, IResourceProvider resourceProvider)
        {
            logger = loggerFactory.CreateLogger<CompilerApp>();
            this.resourceProvider = resourceProvider;
        }

        public void Run(IReadOnlyList<string> input, bool recursive, string manifest, string output, bool autoSize, double imgWidth, double imgHeight, IReadOnlyList<string> formats)
        {
            if (!input.Any())
            {
                logger.LogError("At least one input file must be specified.");
                Environment.Exit(1);
            }

            var generators = new Dictionary<IOutputGenerator, string>();
            var outputGenerators = new OutputGeneratorRepository();
            bool outputIsDirectory = Directory.Exists(output);
            if (output != null && !outputIsDirectory)
            {
                if (outputGenerators.TryGetGeneratorByFileExtension(Path.GetExtension(output), out var generator))
                {
                    // Use the generator implied by the file extension
                    generators.Add(generator, output);
                }
                else if (formats?.Any() != true)
                {
                    logger.LogError("Unable to infer format from output file extension." + Environment.NewLine +
                                    "Specify a known file extension or specify explicitly using --format");
                    Environment.Exit(1);
                }
                else
                {
                    logger.LogInformation("Unable to infer format from output file extension. Using formats only.");
                }
            }

            if (formats?.Any() == true)
            {
                foreach (var format in formats)
                {
                    if (outputGenerators.TryGetGeneratorByFormat(format, out var generator))
                    {
                        generators.Add(generator, outputIsDirectory ? output : null);
                    }
                    else
                    {
                        logger.LogError($"Unknown format: {format}");
                        Environment.Exit(1);
                    }
                }
            }

            var previewOptions = new PreviewGenerationOptions
            {
                Center = true,
                Crop = autoSize,
                Width = imgWidth,
                Height = imgHeight
            };

            var results = new List<CompileResult>();

            foreach (var i in input)
            {
                if (File.Exists(i))
                {
                    var result = CompileOne(i, previewOptions, generators);
                    results.Add(result);
                }
                else if (Directory.Exists(i))
                {
                    foreach (var generator in generators)
                    {
                        if (generator.Value != null && !Directory.Exists(generator.Value))
                        {
                            logger.LogError("Outputs must be directories when the input is a directory.");
                            Environment.Exit(1);
                        }
                    }

                    foreach (var file in Directory.GetFiles(i, "*.xml",
                                                            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    {
                        var result = CompileOne(file, previewOptions, generators);
                        results.Add(result);
                    }
                }
                else
                {
                    logger.LogError($"Input is not a valid file or directory: {i}");
                    Environment.Exit(1);
                }
            }

            if (manifest != null)
            {
                using (var manifestFs = File.Open(manifest, FileMode.Create))
                {
                    logger.LogInformation($"Writing manifest to {manifest}");
                    ManifestGenerator.WriteManifest(results, manifestFs);
                }
            }
        }

        CompileResult CompileOne(string inputFile, PreviewGenerationOptions previewOptions, IDictionary<IOutputGenerator, string> formats)
        {
            logger.LogInformation(inputFile);

            var loader = new XmlLoader();
            using (var fs = File.OpenRead(inputFile))
            {
                loader.Load(fs);

                if (loader.LoadErrors.Any())
                {
                    foreach (var error in loader.LoadErrors)
                    {
                        switch (error.Category)
                        {
                            case LoadErrorCategory.Error:
                                logger.LogError(error.Message);
                                break;
                        }
                    }

                    if (loader.LoadErrors.Any(x => x.Category == LoadErrorCategory.Error))
                        Environment.Exit(1);
                }

                var description = loader.GetDescriptions()[0];

                var outputs = Generate(fs, description, Path.GetFileNameWithoutExtension(inputFile), resourceProvider, formats, previewOptions);

                return new CompileResult(description.Metadata.Author,
                                         description.ComponentName,
                                         description.Metadata.GUID,
                                         true,
                                         description.Metadata.AdditionalInformation,
                                         inputFile,
                                         description.Metadata.Entries.ToImmutableDictionary(),
                                         outputs.ToImmutableDictionary());
            }
        }

        internal IEnumerable<KeyValuePair<string, string>> Generate(FileStream input,
                                                                    ComponentDescription description,
                                                                    string inputBaseName,
                                                                    IResourceProvider resourceProvider,
                                                                    IDictionary<IOutputGenerator, string> formats,
                                                                    PreviewGenerationOptions previewOptions)
        {
            foreach (var f in formats)
            {
                string format = f.Key.FileExtension.Substring(1);
                string autoGeneratedName = $"{inputBaseName}{f.Key.FileExtension}";
                string outputPath = f.Value != null && Directory.Exists(f.Value) ? Path.Combine(f.Value, autoGeneratedName) : f.Value ?? autoGeneratedName;
                using (var output = File.Open(outputPath, FileMode.Create))
                {
                    logger.LogDebug($"Starting {format} generation.");
                    input.Seek(0, SeekOrigin.Begin);
                    f.Key.Generate(description,
                                   resourceProvider,
                                   previewOptions,
                                   input,
                                   output);
                    logger.LogInformation($"  {format,-4} -> {outputPath}");
                }

                yield return new KeyValuePair<string, string>(format, outputPath);
            }
        }
    }
}
