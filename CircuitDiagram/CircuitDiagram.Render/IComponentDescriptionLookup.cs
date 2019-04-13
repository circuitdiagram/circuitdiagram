using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Render
{
    /// <summary>
    /// Provides methods to locate a <see cref="ComponentDescription"/> for given a <see cref="ComponentType"/>.
    /// </summary>
    public interface IComponentDescriptionLookup
    {
        /// <summary>
        /// Finds a <see cref="ComponentDescription"/> for the given <see cref="ComponentType"/>.
        /// </summary>
        /// <param name="componentType">The type to find a description for.</param>
        /// <returns>A description for the given component.</returns>
        ComponentDescription GetDescription(ComponentType componentType);
    }
}
