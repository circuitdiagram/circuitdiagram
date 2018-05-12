using System;
using System.Collections.Generic;
using System.Text;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescription.Conditions;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.Conditions
{
    class VersionedConditionParser : IConditionParser
    {
        private readonly IConditionParser parserLatest;
        private readonly IConditionParser parserV11;

        public VersionedConditionParser()
        {
            parserLatest = new ConditionParser();
            parserV11 = new ConditionParser(new ConditionFormat {StatesUnderscored = true});
        }

        public IConditionTreeItem Parse(ComponentDescription description, string input)
        {
            var version = description.Metadata.FormatVersion;

            if (version < new Version(1, 1))
                return new LegacyConditionParser(description).Parse(description, input);

            if (version == new Version(1, 1))
                return parserV11.Parse(description, input);

            return parserLatest.Parse(description, input);
        }
    }
}
