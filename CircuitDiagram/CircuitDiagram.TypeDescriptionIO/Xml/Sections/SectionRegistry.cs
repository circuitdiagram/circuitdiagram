using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Sections
{
    class SectionRegistry : ISectionRegistry
    {
        private readonly Dictionary<Type, object> sections = new Dictionary<Type, object>();

        public void RegisterSection<T>(T section)
        {
            sections[typeof(T)] = section;
        }

        public T GetSection<T>()
        {
            if (sections.TryGetValue(typeof(T), out var sectionValue))
                return (T)sectionValue;

            return default(T);
        }
    }
}
