using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using CircuitDiagram.CLI.Component.OutputGenerators;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI.Component
{
    class OutputGeneratorRepository
    {
        private readonly IReadOnlyList<IOutputGenerator> _generators;

        public OutputGeneratorRepository(ILoggerFactory loggerFactory, PngRenderer pngRenderer)
        {
            _generators = new IOutputGenerator[]
            {
                new BinaryComponentGenerator(loggerFactory),
                new SvgPreviewRenderer(),
                new PngPreviewRenderer()
                {
                    Renderer = pngRenderer,
                },
            };
        }

        public bool TryGetGeneratorByFileExtension(string extension, out IOutputGenerator generator)
        {
            generator = _generators.FirstOrDefault(x => x.FileExtension == extension);
            return generator != null;
        }

        public bool TryGetGeneratorByFormat(string format, out IOutputGenerator generator)
        {
            generator = _generators.FirstOrDefault(x => x.Format == format);
            return generator != null;
        }
    }
}
