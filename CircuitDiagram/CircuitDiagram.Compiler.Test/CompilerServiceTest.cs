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
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace CircuitDiagram.Compiler.Test
{
    [TestFixture]
    public class CompilerServiceTest
    {
        [Test]
        public void TestCompile()
        {
            var assembly = typeof(CompilerServiceTest).GetTypeInfo().Assembly;
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
