using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CircuitDiagram.Primitives;
using CircuitDiagram.TypeDescription;
using CircuitDiagram.TypeDescriptionIO.Xml.Logging;
using CircuitDiagram.TypeDescriptionIO.Xml.Primitives;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Parsers.ComponentPoints
{
    public class ComponentPointParser : IComponentPointParser
    {
        private readonly IXmlLoadLogger _logger;

        public ComponentPointParser(IXmlLoadLogger logger)
        {
            _logger = logger;
        }

        public bool TryParse(string x, string y, FileRange xRange, FileRange yRange, out XmlComponentPoint componentPoint)
        {
            if (!TryParseComponentPosition(x, xRange, out var relativeToX, out var remainingX))
            {
                componentPoint = null;
                return false;
            }

            if (!TryParseOffsets(remainingX, OffsetAxis.X, xRange, out var xOffsets))
            {
                componentPoint = null;
                return false;
            }

            if (!TryParseComponentPosition(y, yRange, out var relativeToY, out var remainingY))
            {
                componentPoint = null;
                return false;
            }

            if (!TryParseOffsets(remainingY, OffsetAxis.Y, yRange, out var yOffsets))
            {
                componentPoint = null;
                return false;
            }

            var offsets = xOffsets.Concat(yOffsets).ToList();

            componentPoint = new XmlComponentPoint(relativeToX, relativeToY, offsets);
            return true;
        }

        public bool TryParse(string point, FileRange range, out XmlComponentPoint componentPoint)
        {
            if (!TryParseComponentPosition(point, range, out var relativeTo, out var remaining))
            {
                componentPoint = null;
                return false;
            }

            if (!TryParseOffsets(remaining, null, range, out var offsets))
            {
                componentPoint = null;
                return false;
            }

            componentPoint = new XmlComponentPoint(relativeTo, relativeTo, offsets);
            return true;
        }

        private bool TryParseComponentPosition(string s, FileRange range, out ComponentPosition componentPosition, out string remaining)
        {
            if (s.StartsWith("_Start", StringComparison.OrdinalIgnoreCase))
            {
                componentPosition = ComponentPosition.Start;
                remaining = s.Substring("_Start".Length);
                return true;
            }
            else if (s.StartsWith("_Middle", StringComparison.OrdinalIgnoreCase))
            {
                componentPosition = ComponentPosition.Middle;
                remaining = s.Substring("_Middle".Length);
                return true;
            }
            else if (s.StartsWith("_End", StringComparison.OrdinalIgnoreCase))
            {
                componentPosition = ComponentPosition.End;
                remaining = s.Substring("_End".Length);
                return true;
            }
            else
            {
                _logger.Log(LogLevel.Error, range, $"Invalid point '{s}' (expected a string beginning with '_Start', '_Middle' or '_End'", null);
                componentPosition = ComponentPosition.Absolute;
                remaining = null;
                return false;
            }
        }

        protected virtual bool TryParseOffsets(string s, OffsetAxis? contextAxis, FileRange range, out IList<IXmlComponentPointOffset> offsets)
        {
            var reader = new StringReader(s);
            offsets = new List<IXmlComponentPointOffset>();

            var offsetBuilder = new StringBuilder();
            int ci;
            while ((ci = reader.Read()) != -1)
            {
                char c = (char)ci;

                if ((c == '+' || c == '-') && offsetBuilder.Length != 0)
                {
                    _logger.Log(LogLevel.Error, range, "Invalid offset", null);
                    return false;
                }

                if (c == '+')
                {
                    continue;
                }

                if (c == '-' || c == '.')
                {
                    offsetBuilder.Append(c);
                    continue;
                }

                if (reader.Peek() != -1 && (char)reader.Peek() != '+' && (char)reader.Peek() != '-')
                {
                    offsetBuilder.Append(c);
                    continue;
                }

                offsetBuilder.Append(c);
                var offsetStr = offsetBuilder.ToString();

                // Parse axis
                OffsetAxis axis;
                string remaining;
                if (contextAxis != null)
                {
                    axis = contextAxis.Value;
                    remaining = offsetStr;
                }
                else
                {
                    remaining = offsetStr.Substring(0, offsetStr.Length - 1);
                    if (!TryParseOffsetAxis(offsetStr.Last(), range, out axis))
                    {
                        return false;
                    }
                }

                if (!TryParseOffset(remaining, axis, range, out var offset))
                {
                    return false;
                }

                offsets.Add(offset);
                offsetBuilder.Clear();
            }

            return true;
        }

        protected virtual bool TryParseOffset(string offset, OffsetAxis axis, FileRange range, out IXmlComponentPointOffset result)
        {
            if (offset.Length == 0)
            {
                _logger.Log(LogLevel.Error, range, $"Expected a number before '{axis.ToString().ToLowerInvariant()}'", null);
                result = null;
                return false;
            }

            if (!double.TryParse(offset, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var offsetValue))
            {
                _logger.Log(LogLevel.Error, range, $"Unable to parse '{offset}' as double", null);
                result = null;
                return false;
            }

            result = new XmlComponentPointOffset(offsetValue, axis);
            return true;
        }

        private bool TryParseOffsetAxis(char c, FileRange range, out OffsetAxis axis)
        {
            switch (c)
            {
                case 'x':
                    {
                        axis = OffsetAxis.X;
                        return true;
                    }
                case 'y':
                    {
                        axis = OffsetAxis.Y;
                        return true;
                    }
                default:
                    {
                        _logger.Log(LogLevel.Error, range, $"Unexpected '{c}'; expected 'x' or 'y'", null);
                        axis = OffsetAxis.X;
                        return false;
                    }
            }
        }
    }
}
