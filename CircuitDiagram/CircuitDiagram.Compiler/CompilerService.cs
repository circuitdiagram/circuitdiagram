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
        
        public ComponentCompileResult Compile(Stream input, Stream output, IResourceProvider resourceProvider, CompileOptions options)
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
