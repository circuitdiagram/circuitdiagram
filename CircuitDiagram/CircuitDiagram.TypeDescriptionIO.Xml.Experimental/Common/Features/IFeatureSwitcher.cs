using System;
using System.Collections.Generic;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Experimental.Common.Features
{
    public interface IFeatureSwitcher
    {
        bool IsFeatureEnabled(string featureKey);
    }
}
