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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Compiler.CompileStages;
using log4net;

namespace CircuitDiagram.Compiler
{
    public class CompilerService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CompilerService));

        public ComponentCompileResult Compile(Stream input, Stream output, IResourceProvider resourceProvider,
                                              CompileOptions options)
        {
            Log.Info($"Compiling {input.StreamToString()}");

            var runner = new CompileStageRunner(new ICompileStage[]
            {
                new LoadFromXmlCompileStage(),
                new SetIconsCompileStage(),
                new OutputCdcomCompileStage()
            }, resourceProvider);

            ComponentCompileResult result;

            try
            {
                result = runner.Run(input, output, options);

                Log.Info($"Compiled to {output.StreamToString()}");
            }
            catch (Exception ex)
            {
                result = new ComponentCompileResult
                {
                    Success = false
                };

                Log.Error(ex);
            }

            return result;
        }
    }
}
