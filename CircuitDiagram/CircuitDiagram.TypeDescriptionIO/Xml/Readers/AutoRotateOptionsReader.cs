using CircuitDiagram.Circuit;
using CircuitDiagram.TypeDescriptionIO.Xml.Flatten;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Readers
{
    public class AutoRotateOptionsReader : IAutoRotateOptionsReader
    {
        private readonly IXmlLoadLogger logger;

        public AutoRotateOptionsReader(IXmlLoadLogger logger)
        {
            this.logger = logger;
        }

        public bool TrySetAutoRotateOptions(XElement element, IAutoRotateRoot target)
        {
            return TrySetAutoRotateOptions(element, null, target);
        }

        public bool TrySetAutoRotateOptions(XElement element, IAutoRotateRoot ancestor, IAutoRotateRoot target)
        {
            if (ancestor != null)
            {
                target.AutoRotate = ancestor.AutoRotate;
                target.AutoRotateFlip = ancestor.AutoRotateFlip;
            }

            var autorotate = element.Attribute("autorotate");
            if (autorotate == null)
            {
                return true;
            }

            var options = autorotate.Value.Split(',');

            if (options.Length == 0)
            {
                logger.LogError(autorotate, "Autorotate options cannot be empty");
                return false;
            }

            if (!Enum.TryParse<AutoRotateType>(options[0], true, out var autoRotateType))
            {
                logger.LogError(autorotate, $"Unknown autorotation type '{options[0]}'");
                return false;
            }

            var flipState = FlipState.None;
            foreach (var option in options.Skip(1))
            {
                if (string.Equals(option, "FlipPrimary", StringComparison.OrdinalIgnoreCase))
                {
                    flipState |= FlipState.Primary;
                }
                else if (string.Equals(option, "FlipSecondary", StringComparison.OrdinalIgnoreCase))
                {
                    flipState |= FlipState.Secondary;
                }
                else
                {
                    logger.LogError(autorotate, $"Unknown option '{option}'");
                    return false;
                }
            }

            target.AutoRotate = autoRotateType;
            target.AutoRotateFlip = flipState;
            return true;
        }
    }
}
