using System.IO;
using System.Linq;
using CircuitDiagram.Components.Description;
using CircuitDiagram.IO;
using CircuitDiagram.IO.Descriptions.Xml;

namespace CircuitDiagram.Compiler.CompileStages
{
    class LoadFromXmlCompileStage : ICompileStage
    {
        public void Run(CompileContext context)
        {
            XmlLoader loader = new XmlLoader();
            loader.Load(context.Input);

            context.Errors.AddRange(loader.LoadErrors.Select(e => new CompileError(e)));
            if (loader.LoadErrors.Count(e => e.Category == LoadErrorCategory.Error) > 0)
                return;

            var description = loader.GetDescriptions()[0];
            
            // The component XML format doesn't provide an ID, so make one now
            description.ID = "C0";

            context.Description = description;
        }
    }
}
