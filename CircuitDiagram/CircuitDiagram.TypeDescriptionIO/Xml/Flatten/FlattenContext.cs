using CircuitDiagram.Circuit;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription.Conditions;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Flatten
{
    public class FlattenContext
    {
        public FlattenContext(IXmlLoadLogger logger, IConditionTreeItem ancestorConditions, AutoRotateContext autoRotate)
        {
            Logger = logger;
            AncestorConditions = ancestorConditions;
            AutoRotate = autoRotate;
        }

        public IXmlLoadLogger Logger { get; }

        public IConditionTreeItem AncestorConditions { get; }

        public AutoRotateContext AutoRotate { get; }
    }
}
