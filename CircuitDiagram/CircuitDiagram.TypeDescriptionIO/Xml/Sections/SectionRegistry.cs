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

        public object GetSection(Type t)
        {
            if (sections.TryGetValue(t, out var sectionValue))
                return Activator.CreateInstance(typeof(RegisteredSection<>).MakeGenericType(t), sectionValue);

            return Activator.CreateInstance(typeof(RegisteredSection<>).MakeGenericType(t));
        }
    }

    public class RegisteredSection<T> : IXmlSection<T> where T : class
    {
        public RegisteredSection()
        {
        }

        public RegisteredSection(T value)
        {
            Value = value;
        }

        public bool IsAvailable => Value != null;

        public T Value { get; }
    }
}
