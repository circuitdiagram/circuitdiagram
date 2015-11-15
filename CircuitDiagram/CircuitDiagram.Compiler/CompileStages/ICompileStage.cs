namespace CircuitDiagram.Compiler.CompileStages
{
    interface ICompileStage
    {
        void Run(CompileContext context);
    }
}
