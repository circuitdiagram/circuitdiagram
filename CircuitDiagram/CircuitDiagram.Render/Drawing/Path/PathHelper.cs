// PathHelper.cs
//
// Circuit Diagram http://www.circuit-diagram.org/
//
// Copyright (C) 2012  Sam Fisher
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CircuitDiagram.Render.Path
{
    /// <summary>
    /// Provides methods related to manipulating paths.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Converts an svg-style path strin to a list of path commands.
        /// </summary>
        /// <param name="data">The path to convert.</param>
        /// <returns>A list of path commands.</returns>
        public static List<IPathCommand> ParseCommands(string data)
        {
            string pathLetters = "mlhvcsqtaz";
            Regex commandsRegex = new Regex("[" + pathLetters + pathLetters.ToUpperInvariant() + "] ?\\-?[0-9e,\\-. ]*");
            Regex letterRegex = new Regex("[" + pathLetters + pathLetters.ToUpperInvariant() + "]");
            Regex numberRegex = new Regex("[0-9e,\\-. ]+");
            MatchCollection commandsMatch = commandsRegex.Matches(data);
            List<IPathCommand> pathCommands = new List<IPathCommand>();

            double lastX = 0;
            double lastY = 0;
            foreach (Match pathCommand in commandsMatch)
            {
                string letter = letterRegex.Match(pathCommand.Value).Value;
                bool isAbsolute = (letter.ToLowerInvariant() != letter);

                CommandType pCommand;
                switch (letter.ToLowerInvariant())
                {
                    case "m":
                        pCommand = CommandType.MoveTo;
                        break;
                    case "l":
                        pCommand = CommandType.LineTo;
                        break;
                    case "h":
                        pCommand = CommandType.LineTo;
                        break;
                    case "v":
                        pCommand = CommandType.LineTo;
                        break;
                    case "c":
                        pCommand = CommandType.CurveTo;
                        break;
                    case "s":
                        pCommand = CommandType.SmoothCurveTo;
                        break;
                    case "q":
                        pCommand = CommandType.QuadraticBeizerCurveTo;
                        break;
                    case "t":
                        pCommand = CommandType.SmoothQuadraticBeizerCurveTo;
                        break;
                    case "a":
                        pCommand = CommandType.EllipticalArcTo;
                        break;
                    case "z":
                        pCommand = CommandType.ClosePath;
                        break;
                    default:
                        continue;
                }

                if (pCommand == CommandType.LineTo || pCommand == CommandType.MoveTo || pCommand == CommandType.SmoothQuadraticBeizerCurveTo)
                {
                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int o = 0; o + 2 <= numbers.Length; o += 2) // shorthand syntax
                    {
                        double xLocation = double.Parse(numbers[o + 0]);
                        double yLocation = 0;
                        if (letter.ToLowerInvariant() == "h" || letter.ToLowerInvariant() == "v")
                        {
                            if (letter.ToLowerInvariant() == "v")
                            {
                                yLocation = xLocation;
                                xLocation = 0;
                            }

                            if (!isAbsolute)
                            {
                                xLocation += lastX;
                                yLocation += lastY;
                            }

                            pathCommands.Add(new LineTo(xLocation, yLocation));
                        }
                        else
                        {
                            yLocation = double.Parse(numbers[o + 1]);
                            if (!isAbsolute)
                            {
                                xLocation += lastX;
                                yLocation += lastY;
                            }

                            switch (pCommand)
                            {
                                case CommandType.MoveTo:
                                    pathCommands.Add(new MoveTo(xLocation, yLocation));
                                    break;
                                case CommandType.LineTo:
                                    pathCommands.Add(new LineTo(xLocation, yLocation));
                                    break;
                                case CommandType.SmoothQuadraticBeizerCurveTo:
                                    pathCommands.Add(null);
                                    break;
                            }
                        }

                        lastX = xLocation;
                        lastY = yLocation;
                    }
                }
                else if (pCommand == CommandType.SmoothCurveTo || pCommand == CommandType.QuadraticBeizerCurveTo)
                {
                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    double xA = double.Parse(numbers[0]);
                    double yA = double.Parse(numbers[1]);
                    double xB = double.Parse(numbers[2]);
                    double yB = double.Parse(numbers[3]);
                    if (!isAbsolute)
                    {
                        xA += lastX;
                        yA += lastY;
                        xB += lastX;
                        yB += lastY;
                    }

                    switch (pCommand)
                    {
                        case CommandType.SmoothCurveTo:
                            pathCommands.Add(new SmoothCurveTo());
                            break;
                        case CommandType.QuadraticBeizerCurveTo:
                            pathCommands.Add(new QuadraticBeizerCurveTo(xA, yA, xB, yB));
                            break;
                    }

                    lastX = xB;
                    lastY = yB;
                }
                else if (pCommand == CommandType.CurveTo)
                {
                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int o = 0; o + 6 <= numbers.Length; o += 6) // shorthand syntax
                    {
                        double xA = double.Parse(numbers[o + 0]);
                        double yA = double.Parse(numbers[o + 1]);
                        double xB = double.Parse(numbers[o + 2]);
                        double yB = double.Parse(numbers[o + 3]);
                        double xC = double.Parse(numbers[o + 4]);
                        double yC = double.Parse(numbers[o + 5]);
                        if (!isAbsolute)
                        {
                            xA += lastX;
                            yA += lastY;
                            xB += lastX;
                            yB += lastY;
                            xC += lastX;
                            yC += lastY;
                        }

                        switch (pCommand)
                        {
                            case CommandType.CurveTo:
                                pathCommands.Add(new CurveTo(xA, yA, xB, yB, xC, yC));
                                break;
                        }

                        lastX = xC;
                        lastY = yC;
                    }
                }
                else if (pCommand == CommandType.EllipticalArcTo)
                {
                    Match numbersMatch = numberRegex.Match(pathCommand.Value);
                    string[] numbers = numbersMatch.Value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int o = 0; o + 7 <= numbers.Length; o += 7) // shorthand syntax
                    {
                        double rx = double.Parse(numbers[o + 0]);
                        double ry = double.Parse(numbers[o + 1]);
                        double xrotation = double.Parse(numbers[o + 2]);
                        double islargearc = double.Parse(numbers[o + 3]);
                        double sweep = double.Parse(numbers[o + 4]);
                        double x = double.Parse(numbers[o + 5]);
                        double y = double.Parse(numbers[o + 6]);
                        if (!isAbsolute)
                        {
                            x += lastX;
                            y += lastY;
                        }

                        switch (pCommand)
                        {
                            case CommandType.EllipticalArcTo:
                                pathCommands.Add(new EllipticalArcTo(rx, ry, xrotation, islargearc == 0, sweep == 1, x, y));
                                break;
                        }

                        lastX = x;
                        lastY = y;
                    }
                }
                else if (pCommand == CommandType.ClosePath)
                {
                    pathCommands.Add(new ClosePath());
                }
            }

            return pathCommands;
        }
    }
}
