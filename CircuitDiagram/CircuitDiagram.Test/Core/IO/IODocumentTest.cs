using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CircuitDiagram.IO;

namespace CircuitDiagram.Test.Core.IO
{
    [TestClass]
    public class IODocumentTest
    {
        [TestMethod]
        public void TestGetComponentTypes()
        {
            // Create the component types
            var com1 = new IOComponentType("component1");
            var com2 = new IOComponentType("collection1", "component2");
            var com3 = new IOComponentType("collection1", "component3");

            // Create the test document
            IODocument doc = new IODocument();

            // Add instances of each component
            doc.Components.Add(new IOComponent("1", null, null, null, null, com1));
            doc.Components.Add(new IOComponent("1", null, null, null, null, com2));
            doc.Components.Add(new IOComponent("1", null, null, null, null, com3));

            // Add a duplicate of component 3
            doc.Components.Add(new IOComponent("1", null, null, null, null, com3));

            // Call the method being tested
            var comTypes = doc.GetComponentTypes();

            // There should be a "special:unknown" collection,
            // and a "collection1" collection
            Assert.AreEqual(2, comTypes.Count);
            Assert.IsTrue(comTypes.ContainsKey(IODocument.UnknownCollection));
            Assert.IsTrue(comTypes.ContainsKey("collection1"));

            // "special:unknown" should contain "component1"
            Assert.AreEqual(1, comTypes[IODocument.UnknownCollection].Count);
            Assert.IsTrue(comTypes[IODocument.UnknownCollection].Contains(com1));

            // "collection1" should contain "component2" and "component3"
            Assert.AreEqual(2, comTypes["collection1"].Count);
            Assert.IsTrue(comTypes["collection1"].Contains(com2));
            Assert.IsTrue(comTypes["collection1"].Contains(com3));
        }
    }
}
