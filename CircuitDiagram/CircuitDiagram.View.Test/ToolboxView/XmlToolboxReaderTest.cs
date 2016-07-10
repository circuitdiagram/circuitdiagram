// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CircuitDiagram.Circuit;
using CircuitDiagram.Render;
using CircuitDiagram.View.Services;
using CircuitDiagram.View.ToolboxView;
using Moq;
using NUnit.Framework;

namespace CircuitDiagram.View.Test.ToolboxView
{
    [TestFixture]
    public class XmlToolboxReaderTest
    {
        private XmlToolboxReader reader;

        private readonly ComponentType wire;
        private readonly ComponentType resistor;

        public XmlToolboxReaderTest()
        {
            wire = new ComponentType(Guid.Parse("6353882b-5208-4f88-a83b-2271cc82b94f"), "Wire");

            resistor = new ComponentType(Guid.Parse("dab6d52b-51a0-49b0-bc40-3cda966148aa"), "Resistor");
            resistor.Configurations.Add(new ComponentConfiguration
            {
                Name = "Resistor"
            });
            resistor.Configurations.Add(new ComponentConfiguration
            {
                Name = "Variable Resistor"
            });
        }

        [SetUp]
        public void Setup()
        {
            reader = new XmlToolboxReader(Mock.Of<IComponentIconProvider>());
        }

        private ToolboxEntry[][] ReadEntries()
        {
            ToolboxEntry[][] toolbox;

            var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().First(x => x.EndsWith("Toolbox.xml"));
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                toolbox = reader.GetToolbox(resourceStream, new[]
                {
                    wire,
                    resistor
                });
            }
            return toolbox;
        }

        [Test]
        public void Read()
        {
            var toolbox = ReadEntries();

            Assert.That(toolbox, Has.Length.EqualTo(2));

            // Wire
            Assert.That(toolbox[0], Has.Length.EqualTo(1));
            Assert.That(toolbox[0][0].Name, Is.EqualTo("Wire"));

            // Resistor
            Assert.That(toolbox[1], Has.Length.EqualTo(2));
            Assert.That(toolbox[1][0].Name, Is.EqualTo("Resistor"));
            Assert.That(toolbox[1][1].Name, Is.EqualTo("Variable Resistor"));
        }

        [Test]
        public void Keys()
        {
            var toolbox = ReadEntries();

            Assert.That(toolbox[0][0].Key.Value, Is.EqualTo(Key.W));
        }
    }
}
