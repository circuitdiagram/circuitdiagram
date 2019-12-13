using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CircuitDiagram.CLI.Component.Manifest;
using CircuitDiagram.CLI.Component.OutputGenerators;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.Compiler;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI.Component.InputRunners
{
    class ComponentDescriptionRunner
    {
        private readonly ILogger logger;
        private readonly OutputRunner outputRunner;

        public ComponentDescriptionRunner(ILogger logger, OutputRunner outputRunner)
        {
            this.logger = logger;
            this.outputRunner = outputRunner;
        }

        public IManifestEntry CompileOne(string inputFile, PreviewGenerationOptions previewOptions, bool allConfigurations, IDictionary<IOutputGenerator, string> formats)
        {
            logger.LogInformation(inputFile);

            var loader = new XmlLoader();
            using (var fs = File.OpenRead(inputFile))
            {
                if (!loader.Load(fs, logger, out var description))
                {
                    return null;
                }

                var baseName = Path.GetFileNameWithoutExtension(inputFile);
                var outputs = outputRunner.Generate(fs, description, null, baseName, formats, previewOptions, SourceFileType.ComponentDescription).ToList();

                Dictionary<string, IReadOnlyDictionary<string, string>> configurationOutputs = new Dictionary<string, IReadOnlyDictionary<string, string>>();
                if (allConfigurations)
                {
                    foreach(var configuration in description.Metadata.Configurations)
                    {
                        var renderOptions = new PreviewGenerationOptions
                        {
                            Center = previewOptions.Center,
                            Crop = previewOptions.Crop,
                            DebugLayout = previewOptions.DebugLayout,
                            Width = previewOptions.Width,
                            Height = previewOptions.Height,
                            Horizontal = previewOptions.Horizontal,
                            Size = previewOptions.Size,
                            Configuration = configuration.Name,
                        };

                        var configurationOutput = outputRunner.Generate(fs, description, configuration, baseName, formats, renderOptions, SourceFileType.ComponentDescriptionConfiguration);
                        configurationOutputs[configuration.Name] = configurationOutput.ToImmutableDictionary();
                    }
                }

                var metadata = description.Metadata.Entries.ToDictionary(x => x.Key, x => x.Value);
                var svgIcon = GetSvgIconPath(Path.GetDirectoryName(inputFile), description);
                if (svgIcon != null)
                    metadata["org.circuit-diagram.icon-svg"] = svgIcon;

                return new ComponentDescriptionManifestEntry
                {
                    Author = description.Metadata.Author,
                    ComponentName = description.ComponentName,
                    ComponentGuid = description.Metadata.GUID,
                    Success = true,
                    Description = description.Metadata.AdditionalInformation,
                    Version = description.Metadata.Version.ToString(),
                    InputFile = CleanPath(inputFile),
                    Metadata = metadata,
                    OutputFiles = outputs.ToImmutableDictionary(),
                    ConfigurationOutputFiles = configurationOutputs,
                };
            }
        }

        private static string GetSvgIconPath(string inputDirectory, ComponentDescription description)
        {
            inputDirectory = CleanPath(inputDirectory);

            var componentName = SanitizeName(description.ComponentName);

            if (!description.Metadata.Configurations.Any())
            {
                var icon = $"{componentName}.svg";
                if (Directory.EnumerateFiles(inputDirectory, icon).Any())
                {
                    return Path.Combine(inputDirectory, icon).Replace("\\", "/");
                }
            }

            foreach (var configuration in description.Metadata.Configurations)
            {
                var icon = $"{componentName}--{SanitizeName(configuration.Name)}.svg";
                if (Directory.EnumerateFiles(inputDirectory, icon).Any())
                {
                    return Path.Combine(inputDirectory, icon).Replace("\\", "/");
                }
            }

            return null;
        }

        private static string SanitizeName(string input)
        {
            var result = input.ToLowerInvariant();
            result = Regex.Replace(result, "[^a-z0-9]+", "_");
            if (result.EndsWith("_"))
                result = result.Substring(0, result.Length - 1);
            return result;
        }

        public static string CleanPath(string input)
        {
            var result = input.Replace("\\", "/").Replace("//", "/");
            if (result.StartsWith("./"))
                result = result.Substring(2);
            return result;
        }
    }
}
