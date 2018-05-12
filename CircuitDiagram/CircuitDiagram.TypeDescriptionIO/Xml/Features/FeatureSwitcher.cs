// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Features
{
    public class FeatureSwitcher : IFeatureSwitcher
    {
        private readonly Dictionary<string, XElement> enabledFeatures = new Dictionary<string, XElement>();

        public void EnableFeatureCandidate(string key, XElement element)
        {
            enabledFeatures.Add(key, element);
        }

        public bool IsFeatureEnabled(string key)
        {
            return enabledFeatures.ContainsKey(key);
        }

        public bool IsFeatureEnabled(string key, out XElement source)
        {
            return enabledFeatures.TryGetValue(key, out source);
        }
    }
}
