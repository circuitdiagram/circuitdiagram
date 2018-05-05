using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;

namespace CircuitDiagram.CLI.Render
{
    static class PreviewGenerationOptionsReader
    {
        public static PreviewGenerationOptions Read(string path)
        {
            var options = new PreviewGenerationOptions
            {
                Horizontal = true,
                Size = 80,
                Center = true,
                Width = 640.0,
                Height = 480.0,
                Properties = new Dictionary<string, string>(),
            };

            if (!File.Exists(path))
                return options;

            Retry<IOException>.Times(2, TimeSpan.FromMilliseconds(100), () =>
            {
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    var reader = new StreamReader(fs);

                    while (!reader.EndOfStream)
                    {
                        var tokens = reader.ReadLine().Split('=');

                        if (tokens.Length < 2)
                            continue;

                        switch (tokens[0].Trim())
                        {
                            case "horizontal":
                                options.Horizontal = bool.Parse(tokens[1]);
                                break;
                            case "configuration":
                                options.Configuration = tokens[1];
                                break;
                            default:
                                if (tokens[0].StartsWith("$"))
                                    options.Properties[tokens[0].Substring(1)] = tokens[1];
                                break;
                        }
                    }
                }
            });

            return options;
        }
    }
}
