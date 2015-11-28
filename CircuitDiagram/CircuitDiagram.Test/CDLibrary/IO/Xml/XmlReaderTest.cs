#region Copyright & License Information
/*
 * Copyright 2012-2015 Sam Fisher
 *
 * This file is part of Circuit Diagram
 * http://www.circuit-diagram.org/
 * 
 * Circuit Diagram is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or (at
 * your option) any later version.
 */
#endregion
 
using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using XmlReader = CircuitDiagram.IO.Xml.XmlReader;

namespace CircuitDiagram.Test.CDLibrary.IO.Xml
{
    [TestFixture]
    public class XmlReaderTest
    {
        /// <summary>
        /// Tests loading a circuit in XML v1.1 format.
        /// </summary>
        [Test]
        public void TestLoadXml1_1Circuit()
        {
            // Create the test document

            var doc = new XmlDocument();
            doc.CreateXmlDeclaration("1.0", "utf-8", String.Empty);

            // <circuit> root element
            var circuit = doc.CreateElement("circuit");
            circuit.SetAttribute("version", "1.1");
            circuit.SetAttribute("width", "640");
            circuit.SetAttribute("height", "480");
            doc.AppendChild(circuit);

            // Add a resistor component
            var resistor = CreateComponentXml(doc, "resistor", 10, 10, Orientation.Horizontal, 100);
            resistor.SetAttribute("resistance", "100");
            resistor.SetAttribute("t", "standard");
            circuit.AppendChild(resistor);

            // Add a wire component
            var wire = CreateComponentXml(doc, "wire", 110, 10, Orientation.Horizontal, 10);
            circuit.AppendChild(wire);

            // Add second resistor
            var resistor2 = CreateComponentXml(doc, "resistor", 120, 10, Orientation.Horizontal, 100);
            resistor2.SetAttribute("resistance", "200");
            circuit.AppendChild(resistor2);

            using (var xmlStream = new MemoryStream())
            {
                // Write the document to a stream
                var xmlWriter = new XmlTextWriter(xmlStream, Encoding.UTF8);
                circuit.WriteTo(xmlWriter);
                xmlWriter.Flush();

                // Load the document
                xmlStream.Seek(0, SeekOrigin.Begin);
                XmlReader reader = new XmlReader();

                // Check for success
                bool result = reader.Load(xmlStream);
                Assert.IsTrue(result);

                // Check circuit data
                var circuitDoc = reader.Document;
                Assert.AreEqual(640d, circuitDoc.Size.Width);
                Assert.AreEqual(480d, circuitDoc.Size.Height);

                // Check components
                Assert.AreEqual(2, circuitDoc.Components.Count);
                foreach (var c in circuitDoc.Components)
                {
                    Assert.AreEqual("resistor", c.Type.Item);

                    // Connection information is not available when using the XML format,
                    // so these can't be checked
                }
                
                // Check wires
                Assert.AreEqual(1, circuitDoc.Wires.Count);
            }
            
        }

        private static XmlElement CreateComponentXml(XmlDocument doc, string type, int x, int y, Orientation orientation, int size)
        {
            var resistor = doc.CreateElement("component");
            resistor.SetAttribute("type", type);
            resistor.SetAttribute("x", x.ToString());
            resistor.SetAttribute("y", y.ToString());
            resistor.SetAttribute("orientation", orientation.ToString().ToLowerInvariant());
            resistor.SetAttribute("size", size.ToString());
            return resistor;
        }
    }
}
