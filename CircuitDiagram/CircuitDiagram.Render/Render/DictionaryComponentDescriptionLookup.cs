using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.Render
{
    public class DictionaryComponentDescriptionLookup : IComponentDescriptionLookup
    {
        protected readonly Dictionary<ComponentType, ComponentDescription> LookupDictionary;
        private readonly Dictionary<Guid, ComponentDescription> guidLookup;
        private readonly Dictionary<ComponentName, ComponentDescription> nameLookup; 

        public DictionaryComponentDescriptionLookup()
        {
            LookupDictionary = new Dictionary<ComponentType, ComponentDescription>();
            guidLookup = new Dictionary<Guid, ComponentDescription>();
            nameLookup = new Dictionary<ComponentName, ComponentDescription>();
        }

        public ComponentDescription GetDescription(ComponentType componentType)
        {
            // 1. Exact match
            ComponentDescription description;
            if (LookupDictionary.TryGetValue(componentType, out description))
                return description;

            // 2. ID match
            if (componentType.Id.HasValue && guidLookup.TryGetValue(componentType.Id.Value, out description))
                return description;

            // 3. Name match
            if (nameLookup.TryGetValue(componentType.Name, out description))
                return description;

            // Not found
            return null;
        }

        public void AddDescription(ComponentType componentType, ComponentDescription description)
        {
            LookupDictionary.Add(componentType, description);

            if (componentType.Id.HasValue)
                guidLookup.Add(componentType.Id.Value, description);

            if (!nameLookup.ContainsKey(componentType.Name))
                nameLookup.Add(componentType.Name, description);
        }
    }
}
