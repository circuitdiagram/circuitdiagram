using System.Collections.Generic;
using System.IO;
using System.Linq;
using CircuitDiagram.IO;

namespace CircuitDiagram.Compiler.CompileStages
{
    class CompileStageRunner
    {
        private readonly IResourceProvider resourceResolver;
        private readonly IList<ICompileStage> stages;
        
        public CompileStageRunner(IEnumerable<ICompileStage> stages, IResourceProvider resourceResolver)
        {
            this.resourceResolver = resourceResolver;
            this.stages = stages.ToList();
        }

        public ComponentCompileResult Run(Stream input, Stream output, CompileOptions compileOptions)
        {
            var context = new CompileContext
            {
                Input = input,
                Output = output,
                Options = compileOptions,
                Resources = resourceResolver
            };

            var compiledEntry = new ComponentCompileResult();

            foreach (var stage in stages)
            {
                stage.Run(context);

                if (context.Description != null && compiledEntry.ComponentName == null)
                {
                    compiledEntry.ComponentName = context.Description.ComponentName;
                    compiledEntry.Author = context.Description.Metadata.Author;
                    compiledEntry.Guid = context.Description.Metadata.GUID;
                }
            }

            compiledEntry.Success = context.Errors.Count(e => e.Level.Value == LoadErrorCategory.Error) == 0;

            return compiledEntry;
        }
    }
}
