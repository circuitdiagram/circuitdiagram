using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CircuitDiagram.IO;
using CircuitDiagram.IO.Descriptions.Xml;
using CircuitDiagram.Components.Description;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Components;
using CircuitDiagram.Components.Description.Render;
using NUnit.Framework;

namespace CircuitDiagram.Test.CDLibrary.IO.Descriptions
{
    [TestFixture]
    public class XmlLoaderTest
    {
        private static readonly ConditionTreeLeaf IsHorizontalCondition = new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Equal, new PropertyUnion(true));
        private static readonly ConditionTreeLeaf IsNotHorizontalCondition = new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.NotEqual, new PropertyUnion(true));

        [Test]
        public void TestLoadXmlDescription_V1_2()
        {
            TestComponent("TestComponent_v1.2.xml");
        }

        [Test]
        public void TestLoadXmlDescription_V1_0()
        {
            TestComponent("TestComponent_v1.0.xml");
        }

        private void TestComponent(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.First(r => r.EndsWith(resource));

            using (var xml = assembly.GetManifestResourceStream(resourceName))
            {
                var loader = new XmlLoader();
                loader.Load(xml);

                // Make sure there were no errors
                Assert.AreEqual(0, loader.LoadErrors.Count());

                var testDescription = loader.GetDescriptions()[0];

                // Check component data
                AssertMetadata(testDescription);
                AssertProperties(testDescription);
                AssertFlags(testDescription);
                AssertConnections(testDescription);
                AssertRender(testDescription);
            }
        }

        private void AssertMetadata(ComponentDescription desc)
        {
            Assert.AreEqual("Test Component", desc.ComponentName);
            Assert.AreEqual(60d, desc.MinSize);
            Assert.AreEqual("Circuit Diagram", desc.Metadata.Author);
        }

        private void AssertProperties(ComponentDescription desc)
        {
            Assert.AreEqual(1, desc.Properties.Length);

            var prop = desc.Properties[0];

            Assert.AreEqual("TestProperty", prop.Name);
            Assert.AreEqual("Property", prop.DisplayName);
            Assert.AreEqual("test", prop.SerializedName);
            Assert.AreEqual(PropertyType.Decimal, prop.Type);
            Assert.AreEqual(new PropertyUnion(4700d), prop.Default);

            // Formatting
            Assert.AreEqual(3, prop.FormatRules.Count());

            // Conditional 1
            Assert.AreEqual(new ConditionTreeLeaf(ConditionType.Property, "TestProperty", ConditionComparison.Less, new PropertyUnion(1000.0)), prop.FormatRules[0].Conditions);
            Assert.AreEqual(@"$TestProperty  \u2126", prop.FormatRules[0].Value);

            // Conditional 2
            Assert.AreEqual(new ConditionTreeLeaf(ConditionType.Property, "TestProperty", ConditionComparison.Less, new PropertyUnion(1000000d)), prop.FormatRules[1].Conditions);
            Assert.AreEqual(@"$TestProperty(div_1000_)(round_1)  k\u2126", prop.FormatRules[1].Value);

            // Default
            Assert.AreEqual(@"$TestProperty(div_1000000)(round_1)  M\u2126", prop.FormatRules[2].Value);
        }

        private void AssertFlags(ComponentDescription desc)
        {
            Assert.AreEqual(1, desc.Flags.Length);

            var flag = desc.Flags[0];
            Assert.AreEqual(new ConditionTreeLeaf(ConditionType.Property, "TestProperty", ConditionComparison.Greater, new PropertyUnion(1000d)), flag.Conditions);
            Assert.AreEqual(FlagOptions.MiddleMustAlign, flag.Value);
        }

        private void AssertConnections(ComponentDescription desc)
        {
            Assert.AreEqual(2, desc.Connections.Length);

            var group1 = desc.Connections[0];
            Assert.AreEqual(IsHorizontalCondition, group1.Conditions);
            Assert.AreEqual(2, group1.Value.Length);
            Assert.AreEqual(new ConnectionDescription(new ComponentPoint("_Start"), new ComponentPoint("_Middle-21x"), ConnectionEdge.Start, "a"), group1.Value[0]);
            Assert.AreEqual(new ConnectionDescription(new ComponentPoint("_Middle+21x"), new ComponentPoint("_End"), ConnectionEdge.End, "b"), group1.Value[1]);

            var group2 = desc.Connections[1];
            Assert.AreEqual(IsNotHorizontalCondition, group2.Conditions);
            Assert.AreEqual(new ConnectionDescription(new ComponentPoint("_Start"), new ComponentPoint("_Middle-21y"), ConnectionEdge.Start, "a"), group2.Value[0]);
            Assert.AreEqual(new ConnectionDescription(new ComponentPoint("_Middle+21y"), new ComponentPoint("_End"), ConnectionEdge.End, "b"), group2.Value[1]);
        }

        private void AssertRender(ComponentDescription desc)
        {
            Assert.AreEqual(2, desc.RenderDescriptions.Length);

            var group1 = desc.RenderDescriptions[0];
            Assert.AreEqual(IsHorizontalCondition, group1.Conditions);
            Assert.AreEqual(new Line(new ComponentPoint("_Start"), new ComponentPoint("_Middle-20x"), 2d), group1.Value[0]);
            Assert.AreEqual(new Rectangle(new ComponentPoint("_Middle-20x", "_Start-8y"), 40d, 16d, 2d, false), group1.Value[1]);
            Assert.AreEqual(new Line(new ComponentPoint("_Middle+20x"), new ComponentPoint("_End"), 2d), group1.Value[2]);
            Assert.AreEqual(new Text(new ComponentPoint("_Middle", "_Start-17"), Render.TextAlignment.CentreCentre, 10d, "$TestProperty"), group1.Value[3]);

            var group2 = desc.RenderDescriptions[1];
            Assert.AreEqual(IsNotHorizontalCondition, group2.Conditions);
            Assert.AreEqual(new Line(new ComponentPoint("_Start"), new ComponentPoint("_Middle-20y"), 2d), group2.Value[0]);
            Assert.AreEqual(new Rectangle(new ComponentPoint("_Start-8x", "_Middle-20y"), 16d, 40d, 2d, false), group2.Value[1]);
            Assert.AreEqual(new Line(new ComponentPoint("_Middle+20y"), new ComponentPoint("_End"), 2d), group2.Value[2]);
            Assert.AreEqual(new Text(new ComponentPoint("_Start-14", "_Middle"), Render.TextAlignment.CentreRight, 10d, "$TestProperty"), group2.Value[3]);   
        }
    }
}
