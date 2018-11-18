using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.IO;
using CircuitDiagram.Logging;
using CircuitDiagram.Render.Skia;
using CircuitDiagram.TypeDescriptionIO.Xml;
using CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Definitions;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace CircuitDiagram.CLI.Render
{
    class RenderApp
    {
        public static void Run(string[] args)
        {
            string input = null;
            string propertiesPath = "render.properties";
            string output = "render.png";
            bool watch = false;
            bool silent = false;
            bool debugLayout = false;

            ArgumentSyntax.Parse(args, options =>
            {
                options.ApplicationName = "cdcli render";

                options.DefineOption("o|output", ref output, "Path to output file (default: render.png).");
                options.DefineOption("s|silent", ref silent, "Does not output anything to the console on successful operation.");
                options.DefineOption("w|watch", ref watch, "Re-render output whenever the input file changes.");
                options.DefineOption("p|properties", ref propertiesPath, "Path to render properties file.");
                options.DefineOption("d|debug-layout", ref debugLayout, "Draw layout lines.");
                options.DefineParameter("input", ref input, "Path to input XML component file.");
            });
            
            var loggerFactory = new LoggerFactory();
            if (!silent)
                loggerFactory.AddProvider(new BasicConsoleLogger(LogLevel.Information));

            var logger = loggerFactory.CreateLogger(typeof(RenderApp));

            var renderOptions = PreviewGenerationOptionsReader.Read(propertiesPath);
            renderOptions.DebugLayout |= debugLayout;
            renderOptions.Center = !renderOptions.DebugLayout;

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
                        renderOptions = PreviewGenerationOptionsReader.Read(propertiesPath);

                        Render(logger, input, output, renderOptions);
                    }
                }
            }
        }

        private static void Render(ILogger logger, string inputPath, string outputPath, PreviewGenerationOptions renderOptions)
        {
            var loader = new XmlLoader();
            loader.UseDefinitions();

            using (var fs = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (!loader.Load(fs, logger, out var description))
                {
                    logger.LogError("Unable to render due to errors.");
                    return;
                }
                
                var drawingContext = PreviewRenderer.RenderPreview((size) => new SkiaDrawingContext((int)size.Width, (int)size.Height, SKColors.White), description, renderOptions);

                using (var outputFs = File.OpenWrite(outputPath))
                {
                    drawingContext.WriteAsPng(outputFs);
                }
            }

            logger.LogInformation($"{inputPath} -> {outputPath}");
        }
    }
}
