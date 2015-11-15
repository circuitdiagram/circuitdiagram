using System;
using System.IO;
using System.Text.RegularExpressions;
using CircuitDiagram.Components.Description;
using CircuitDiagram.IO;
using log4net;

namespace CircuitDiagram.Compiler.CompileStages
{
    class SetIconsCompileStage : ICompileStage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SetIconsCompileStage));

        public void Run(CompileContext context)
        {
            // Set the default icon
            string defaultIconName = GetIconResourceName(context.Description.ComponentName);
            context.Description.Metadata.Icon = LoadIcon(defaultIconName, context);

            // Set icons for each configuration
            foreach (var configuration in context.Description.Metadata.Configurations)
            {
                string iconName = GetIconResourceName(context.Description.ComponentName, configuration.Name);
                configuration.Icon = LoadIcon(iconName, context);
            }
        }

        private MultiResolutionImage LoadIcon(string resourceName, CompileContext context)
        {
            int[] resolutions = { 32, 64 };

            var icon = new MultiResolutionImage();
            foreach (int resolution in resolutions)
            {
                string resolutionName = $"{resourceName}_{resolution}.png";

                if (!context.Resources.HasResource(resolutionName))
                {
                    string warnMessage = $"Icon {resolutionName} not found.";
                    Log.Warn(warnMessage);
                    context.Errors.Add(new CompileError(LoadErrorCategory.Warning, warnMessage));
                    continue;
                }

                var resource = context.Resources.GetResource(resolutionName);
                using (var stream = resource.Open())
                {
                    byte[] data = ReadToArray(stream);

                    icon.Add(new SingleResolutionImage
                    {
                        Data = data,
                        MimeType = resource.MimeType
                    });
                }
            }

            return icon.Count > 0 ? icon : null;
        }

        public static byte[] ReadToArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static string GetIconResourceName(string componentName, string configuration = null)
        {
            if (configuration == null)
                return SanitizeName(componentName);
            else
                return SanitizeName(componentName) + "_" + SanitizeName(configuration);
        }

        private static string SanitizeName(string name)
        {
            name = name.ToLowerInvariant();
            name = name.Replace(" ", "_");
            return Regex.Replace(name, @"[\(\)-]", "");
        }
    }
}
