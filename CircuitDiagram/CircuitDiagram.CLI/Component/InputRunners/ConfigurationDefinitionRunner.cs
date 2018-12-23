using CircuitDiagram.CLI.Component.Manifest;
using CircuitDiagram.CLI.Component.OutputGenerators;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.Render;
using CircuitDiagram.TypeDescription.Template;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Immutable;

namespace CircuitDiagram.CLI.Component.InputRunners
{
    class ConfigurationDefinitionRunner
    {
        private readonly ILogger logger;
        private readonly IComponentDescriptionLookup componentDescriptionLookup;
        private readonly OutputRunner outputRunner;

        public ConfigurationDefinitionRunner(ILogger logger, IComponentDescriptionLookup componentDescriptionLookup, OutputRunner outputRunner)
        {
            this.logger = logger;
            this.componentDescriptionLookup = componentDescriptionLookup;
            this.outputRunner = outputRunner;
        }

        public IManifestEntry CompileOne(string inputFile, PreviewGenerationOptions previewOptions, IDictionary<IOutputGenerator, string> formats)
        {
            logger.LogInformation(inputFile);

            var validFormats = formats.Where(x => new[] { "png", "svg" }.Contains(x.Key.Format)).ToDictionary(x => x.Key, x => x.Value);

            var reader = new ConfigurationDefinitionReader(componentDescriptionLookup);
            using (var fs = File.OpenRead(inputFile))
            {
                var configurationDefinition = reader.ReadDefinition(fs);

                var renderOptions = new PreviewGenerationOptions
                {
                    Center = previewOptions.Center,
                    Crop = previewOptions.Crop,
                    DebugLayout = previewOptions.DebugLayout,
                    Width = previewOptions.Width,
                    Height = previewOptions.Height,
                    Horizontal = previewOptions.Horizontal,
                    Size = previewOptions.Size,
                    RawProperties = configurationDefinition.Configuration.Setters,
                };

                var outputs = outputRunner.Generate(fs, configurationDefinition.ComponentDescription, Path.GetFileNameWithoutExtension(inputFile), validFormats, renderOptions);

                return new ComponentConfigurationManifestEntry
                {
                    InputFile = ComponentDescriptionRunner.CleanPath(inputFile),
                    ComponentGuid = configurationDefinition.ComponentDescription.Metadata.GUID,
                    ConfigurationName = configurationDefinition.Configuration.Name,
                    OutputFiles = outputs.ToImmutableDictionary(),
                };
            }
        }
    }
}
