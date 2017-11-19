using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using CircuitDiagram.Logging;
using ComponentCompiler.OutputGenerators;
using Microsoft.Extensions.Logging;

namespace ComponentCompiler
{
    class OutputGeneratorRepository
    {
        private static readonly ILogger Log = LogManager.GetLogger<OutputGeneratorRepository>();

        private static readonly IReadOnlyList<IOutputGenerator> BuiltInGenerators = new IOutputGenerator[]
        {
            new BinaryComponentGenerator(),
            new SvgPreviewRenderer(),
            new PngPreviewRenderer()
        };

        private readonly IReadOnlyList<IOutputGenerator> externalGenerators = new IOutputGenerator[0];

        public IEnumerable<IOutputGenerator> AllGenerators => BuiltInGenerators.Concat(externalGenerators);

#if NET461
        public OutputGeneratorRepository()
        {
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), "plugins");

            if (!Directory.Exists(path))
            {
                externalGenerators = new IOutputGenerator[0];
                return;
            }

            var assemblies = Directory
                .GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(x =>
                {
                    var fileName = Path.GetFileName(x);
                    return !fileName.StartsWith("System.") && !fileName.StartsWith("Microsoft.");
                })
                .Select(assembly =>
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(assembly);
                        Log.LogDebug($"Loaded {assembly}");
                        return asm;
                    }
                    catch (FileLoadException ex)
                    {
                        Log.LogDebug($"Failed to load {assembly}: {ex}");
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();

            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                externalGenerators = container.GetExports<IOutputGenerator>().ToList();
            }
        }
#endif

        public bool TryGetGeneratorByFileExtension(string extension, out IOutputGenerator generator)
        {
            generator = AllGenerators.FirstOrDefault(x => x.FileExtension == extension);
            return generator != null;
        }

        public bool TryGetGeneratorByFormat(string extension, out IOutputGenerator generator)
        {
            generator = AllGenerators.FirstOrDefault(x => x.Format == extension);
            return generator != null;
        }
    }
}
