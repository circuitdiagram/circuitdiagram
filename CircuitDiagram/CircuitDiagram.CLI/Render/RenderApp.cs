using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.IO;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.Logging;
using CircuitDiagram.Render.Skia;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace CircuitDiagram.CLI.Render
{
    class RenderApp
    {
        public static void Run(string[] args)
        {
            string input = null;
            string output = "render.png";
            bool watch = false;
            bool silent = false;

            ArgumentSyntax.Parse(args, options =>
            {
                options.ApplicationName = "cdcli render";

                options.DefineOption("o|output", ref output, "Path to output file (default: render.png).");
                options.DefineOption("s|silent", ref silent, "Does not output anything to the console on successful operation.");
                options.DefineOption("w|watch", ref watch, "Re-render output whenever the input file changes.");
                options.DefineParameter("input", ref input, "Path to input XML component file.");
            });

            var loggerFactory = new LoggerFactory();
            if (!silent)
                loggerFactory.AddProvider(new BasicConsoleLogger(LogLevel.Information));

            var logger = loggerFactory.CreateLogger(typeof(RenderApp));

            var renderOptions = new PreviewGenerationOptions
            {
                Size = 80.0,
                Center = true,
                Crop = false,
                Width = 640,
                Height = 480,
                Horizontal = false,
            };

            if (File.Exists("render.properties"))
                renderOptions = PreviewGenerationOptionsReader.Read("render.properties");

            Render(logger, input, output, renderOptions);

            if (watch)
            {
                var fullPath = Path.GetFullPath(input);
                var watcher = new FileSystemWatcher(Path.GetDirectoryName(fullPath));

                while (true)
                {
                    var changes = watcher.WaitForChanged(WatcherChangeTypes.Changed);

                    if (changes.Name == input || changes.Name == "render.properties")
                    {
                        if (File.Exists("render.properties"))
                            renderOptions = PreviewGenerationOptionsReader.Read("render.properties");

                        Render(logger, input, output, renderOptions);
                    }
                }
            }
        }

        private static void Render(ILogger logger, string inputPath, string outputPath, PreviewGenerationOptions renderOptions)
        {
            logger.LogInformation($"{inputPath} -> {outputPath}");

            var loader = new XmlLoader();
            using (var fs = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                    {
                        logger.LogError("Unable to render due to errors.");
                        return;
                    }
                }

                var description = loader.GetDescriptions()[0];

                var drawingContext = PreviewRenderer.RenderPreview((size) => new SkiaDrawingContext((int)size.Width, (int)size.Height, SKColors.White), description, renderOptions);

                using (var outputFs = File.OpenWrite(outputPath))
                    drawingContext.WriteAsPng(outputFs);
            }
        }
    }
}
