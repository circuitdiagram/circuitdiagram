// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
                    compiledEntry.Description = context.Description;
                }
            }

            compiledEntry.Success = context.Errors.Count(e => e.Level.Value == LoadErrorCategory.Error) == 0;

            return compiledEntry;
        }
    }
}
