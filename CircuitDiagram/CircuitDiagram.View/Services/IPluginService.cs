using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Plugin;

namespace CircuitDiagram.View.Services
{
    public interface IPluginService
    {
        IReadOnlyList<IPluginMetadata> GetPlugins();
        IEnumerable<T> GetPluginParts<T>();
    }
}
