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
using System.Text.RegularExpressions;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescriptionIO;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.Compiler.CompileStages
{
    class SetIconsCompileStage : ICompileStage
    {
        private readonly ILogger _logger;

        public SetIconsCompileStage(ILogger<SetIconsCompileStage> logger)
        {
            _logger = logger;
        }

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
                    _logger.LogWarning(warnMessage);
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
