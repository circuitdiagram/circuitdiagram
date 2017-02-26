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

        [Test]
        public void DoesntRegenerateValues()
        {
            var dictionary = new DependentDictionary<int, object>(Enumerable.Range(1, 1), x => new object());
            var value1 = dictionary[1];
            var value2 = dictionary[1];
            Assert.That(value1, Is.EqualTo(value2));
        }
    }
}
