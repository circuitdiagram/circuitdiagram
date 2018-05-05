using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Sections
{
    class XmlSectionFactory<T> : IXmlSection<T> where T : class
    {
        public XmlSectionFactory(SectionRegistry sectionRegistry)
        {
            var section = (IXmlSection<T>)sectionRegistry.GetSection(typeof(T));
            IsAvailable = section.IsAvailable;
            Value = section.Value;
        }

        public bool IsAvailable { get; }

        public T Value { get; }
    }
}
