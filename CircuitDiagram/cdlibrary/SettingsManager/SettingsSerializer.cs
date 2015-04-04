using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.SettingsManager
{
    /// <summary>
    /// An ISerializer which stores its values in the settings file.
    /// </summary>
    public class SettingsSerializer : ISerializer
    {
        public string Category { get; set; }

        public void Add(string key, string value)
        {
            if (!String.IsNullOrEmpty(Category))
                Settings.Write(Category + "." + key, value);
            else
                Settings.Write(key, value);
        }

        public void Add(string key, int value)
        {
            if (!String.IsNullOrEmpty(Category))
                Settings.Write(Category + "." + key, value);
            else
                Settings.Write(key, value);
        }

        public void Add(string key, double value)
        {
            if (!String.IsNullOrEmpty(Category))
                Settings.Write(Category + "." + key, value);
            else
                Settings.Write(key, value);
        }

        public void Add(string key, bool value)
        {
            if (!String.IsNullOrEmpty(Category))
                Settings.Write(Category + "." + key, value);
            else
                Settings.Write(key, value);
        }

        public void Add(string key, string[] value)
        {
            if (!String.IsNullOrEmpty(Category))
                Settings.Write(Category + "." + key, value);
            else
                Settings.Write(key, value);
        }

        public string GetString(string key)
        {
            if (!String.IsNullOrEmpty(Category))
                return Settings.Read(Category + "." + key) as string;
            else
                return Settings.Read(key) as string;
        }

        public int GetInt32(string key)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(string key)
        {
            throw new NotImplementedException();
        }

        public bool GetBool(string key)
        {
            if (!String.IsNullOrEmpty(Category))
                return Settings.ReadBool(Category + "." + key);
            else
                return Settings.ReadBool(key);
        }

        public string[] GetStringArray(string key)
        {
            if (!String.IsNullOrEmpty(Category))
                return Settings.Read(Category + "." + key) as string[];
            else
                return Settings.Read(key) as string[];
        }

        public bool HasProperty(string key)
        {
            return Settings.HasSetting(key);
        }
    }
}
