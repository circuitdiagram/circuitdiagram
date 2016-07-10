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
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Circuit;
using NUnit.Framework;

namespace CircuitDiagram.IO.Test.Data
{
    [TestFixture]
    public class DependentDictionaryTest
    {
        private List<int> keys;
        private DependentDictionary<int, string> dictionary;

        [SetUp]
        public void Setup()
        {
            keys = new List<int> {1, 2, 3, 4, 5};
            dictionary = new DependentDictionary<int, string>(keys, i => i.ToString());
        }

        [Test]
        public void GeneratesAllKeys()
        {
            Assert.That(dictionary.Keys, Is.EquivalentTo(keys));
        }

        [Test]
        public void NoAdd()
        {
            Assert.Throws<InvalidOperationException>(() => dictionary.Add(6, "6"));
        }

        [Test]
        public void SetValidKey()
        {
            dictionary[1] = "test";
            Assert.That(dictionary[1], Is.EqualTo("test"));
        }

        [Test]
        public void SetInvalidKey()
        {
            Assert.Throws<InvalidOperationException>(() => dictionary[6] = "test");
        }

        [Test]
        public void ReflectsKeysChange()
        {
            keys.Add(6);
            Assert.That(dictionary[6], Is.EqualTo("6"));
        }

        [Test]
        public void SetNonDependentKey()
        {
            var dictionary = new DependentDictionary<int, string>(keys, i => i.ToString(), allowOther: true);
            dictionary[6] = "test";
            Assert.That(dictionary[6], Is.EqualTo("test"));
        }

        [Test]
        public void CountIncludesNonDependentValues()
        {
            var dictionary = new DependentDictionary<int, string>(keys, i => i.ToString(), allowOther: true);
            dictionary[6] = "test";
            Assert.That(dictionary.Count, Is.EqualTo(6));
        }

        [Test]
        public void NotSetExplicitly()
        {
            Assert.That(dictionary.IsSetExplicitly(1), Is.False);
        }

        [Test]
        public void SetExplicitly()
        {
            dictionary[1] = "test";
            Assert.That(dictionary.IsSetExplicitly(1), Is.True);
        }

        [Test]
        public void IsSetExplicitlyInvalidKey()
        {
            Assert.Throws<KeyNotFoundException>(() => dictionary.IsSetExplicitly(6));
        }
    }
}
