using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CircuitDiagram.Compiler;
using Moq;
using NUnit.Framework;

namespace CircuitDiagram.ComponentCompiler.Test
{
   [TestFixture]
    public class CompilerServiceTest
    {
        [Test]
        public void TestCompile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string testComponentResource = assembly.GetManifestResourceNames().First(r => r.EndsWith("TestComponent.xml"));

            var compiler = new CompilerService();

            var output = new MemoryStream();

            ComponentCompileResult result;
            using (var input = assembly.GetManifestResourceStream(testComponentResource))
            {
                result = compiler.Compile(input, output, Mock.Of<IResourceProvider>(), new CompileOptions());
            }

            Assert.That(result.Success);
            Assert.That(result.ComponentName, Is.EqualTo("Wire"));
        }
    }
}
