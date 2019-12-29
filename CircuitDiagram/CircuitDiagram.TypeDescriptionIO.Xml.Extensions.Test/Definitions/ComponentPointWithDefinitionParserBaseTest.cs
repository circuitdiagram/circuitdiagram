using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Test.Xml.Parsers.ComponentPoints;
using CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Definitions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Sections;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Extensions.Test.Definitions
{
    [TestFixture]
    public class ComponentPointWithDefinitionParserBaseTest : ComponentPointParserTest
    {
        public override void Setup()
        {
            _parser = new ComponentPointWithDefinitionParser(new NullXmlLoadLogger(), Mock.Of<ISectionRegistry>());
        }
    }
}
