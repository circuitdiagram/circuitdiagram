using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.CLI.Component.Manifest
{
    interface IManifestEntry
    {
        Guid ComponentGuid { get; }
        string InputFile { get; }
        IReadOnlyDictionary<string, string> OutputFiles { get; }
    }
}
