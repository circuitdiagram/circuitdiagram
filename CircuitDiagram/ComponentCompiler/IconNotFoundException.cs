using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cdcompile
{
    class IconNotFoundException : Exception
    {
        public string ComponentName { get; private set; }
        public string Configuration { get; private set; }
        public int Resolution { get; private set; }
        public string Path { get; private set; }

        public IconNotFoundException(string componentName, string configuration, int resolution, string path)
        {
            ComponentName = componentName;
            Configuration = configuration;
            Resolution = resolution;
            Path = path;
        }

        public override string ToString()
        {
            return String.Format("Icon for {0}\\{1}@{2} not found at {3}.", ComponentName, Configuration, Resolution, Path);
        }
    }
}
