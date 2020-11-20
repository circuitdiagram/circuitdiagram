using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CircuitDiagram.Compiler;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI.Component
{
    class ResourceProviderFactory
    {
        public static IResourceProvider Create(ILogger logger, IReadOnlyList<string> resources)
        {
            IResourceProvider resourceProvider = null;
            if (resources != null && resources.Count == 1)
            {
                string directory = resources.Single();
                if (!Directory.Exists(directory))
                {
                    logger.LogError($"Directory '{directory}' used for --resources does not exist.");
                    Environment.Exit(1);
                }

                logger.LogDebug($"Using directory '{directory}' as resource provider.");
                resourceProvider = new DirectoryResourceProvider(resources.Single());
            }
            else if (resources != null && resources.Count % 2 == 0)
            {
                logger.LogDebug("Mapping resources as key-file pairs.");
                resourceProvider = new FileMapResourceProvider();
                for (int i = 0; i + 1 < resources.Count; i += 2)
                    ((FileMapResourceProvider)resourceProvider).Mappings.Add(resources[i], resources[i + 1]);
            }
            else if (resources != null)
            {
                logger.LogError("--resources must either be a directory or a space-separated list of [key] [filename] pairs.");
            }
            else
            {
                logger.LogDebug("Not supplying resources.");
                resourceProvider = new FileMapResourceProvider();
            }

            return resourceProvider;
        }
    }
}
