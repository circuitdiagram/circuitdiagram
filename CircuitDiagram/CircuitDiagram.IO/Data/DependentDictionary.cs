using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.IO.Data
{
    /// <summary>
    /// A dictionary which only allows keys that are defined by a given source,
    /// and auto-generates values which are not otherwise specified.
    /// </summary>
    public class DependentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IEnumerable<TKey> keys; 
        private readonly Func<TKey, TValue> valueGenerator;
        private readonly Dictionary<TKey, TValue> values;

        public DependentDictionary(IEnumerable<TKey> keys, Func<TKey, TValue> valueGenerator)
        {
            this.keys = keys;
            this.valueGenerator = valueGenerator;
            values = new Dictionary<TKey, TValue>();
        } 

        private IDictionary<TKey, TValue> Generate()
        {
            return keys.ToDictionary(key => key, key =>
            {
                TValue val;
                if (values.TryGetValue(key, out val))
                    return val;
                return valueGenerator(key);
            });
        } 

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Generate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Generate().Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Generate().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)values).Remove(item);
        }

        public int Count => keys.Count();

        public bool IsReadOnly => false;

        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }

        public void Add(TKey key, TValue value)
        {
            throw new InvalidOperationException("Additional keys cannot be added.");
        }

        public bool Remove(TKey key)
        {
            return values.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (values.TryGetValue(key, out value))
                return true;

            if (keys.Contains(key))
            {
                value = valueGenerator(key);
                return true;
            }

            return false;
        }

        public TValue this[TKey key]
        {
            get { return Generate()[key]; }
            set
            {
                if (!keys.Contains(key))
                    throw new InvalidOperationException("The supplied key is not allowed.");

                if (!values.ContainsKey(key))
                    values.Add(key, value);
                else
                    values[key] = value;
            }
        }

        public ICollection<TKey> Keys => keys.ToArray();

        public ICollection<TValue> Values => Generate().Values;
    }
}
