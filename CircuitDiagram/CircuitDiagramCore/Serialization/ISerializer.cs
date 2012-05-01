using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram
{
    /// <summary>
    /// Provides methods for storing and retreiving values.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Adds a new string property with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        void Add(string key, string value);

        /// <summary>
        /// Adds a new int property with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        void Add(string key, int value);

        /// <summary>
        /// Adds a new double property with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        void Add(string key, double value);

        /// <summary>
        /// Adds a new bool property with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        void Add(string key, bool value);

        /// <summary>
        /// Adds a new string array property with the specified key and value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        void Add(string key, string[] value);

        /// <summary>
        /// Gets the property with the specified key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property value if it exists, a default value otherwise.</returns>
        string GetString(string key);

        /// <summary>
        /// Gets the property with the specified key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property value if it exists, a default value otherwise.</returns>
        int GetInt32(string key);

        /// <summary>
        /// Gets the property with the specified key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property value if it exists, a default value otherwise.</returns>
        double GetDouble(string key);

        /// <summary>
        /// Gets the property with the specified key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property value if it exists, a default value otherwise.</returns>
        bool GetBool(string key);

        /// <summary>
        /// Gets the property with the specified key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>The property value if it exists, a default value otherwise.</returns>
        string[] GetStringArray(string key);

        /// <summary>
        /// Determines whether the specified property exists.
        /// </summary>
        /// <param name="key">The key of the property to check.</param>
        /// <returns>True if the property exists, false otherwise.</returns>
        bool HasProperty(string key);
    }
}
