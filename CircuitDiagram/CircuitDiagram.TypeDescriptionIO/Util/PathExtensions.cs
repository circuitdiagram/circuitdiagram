// PathExtensions.cs
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
using CircuitDiagram.Primitives;
using CircuitDiagram.Render.Path;
using CircuitDiagram.TypeDescriptionIO.Util;

namespace CircuitDiagram.IO
{
    /// <summary>
    /// Provides additional methods for path commands.
    /// </summary>
    public static class PathExtensions
    {
        public static void Write(this IPathCommand command, System.IO.BinaryWriter writer)
        {
            switch (command.Type)
            {
                case CommandType.MoveTo:
                    {
                        MoveTo pCommand = command as MoveTo;
                        writer.Write(pCommand.X);
                        writer.Write(pCommand.Y);
                    }
                    break;
                case CommandType.LineTo:
                    {
                        LineTo pCommand = command as LineTo;
                        writer.Write(pCommand.X);
                        writer.Write(pCommand.Y);
                    }
                    break;
                case CommandType.CurveTo:
                    {
                        CurveTo pCommand = command as CurveTo;
                        writer.Write(pCommand.ControlStart);
                        writer.Write(pCommand.ControlEnd);
                        writer.Write(pCommand.End);
                    }
                    break;
                case CommandType.SmoothCurveTo:
                    {
                        SmoothCurveTo pCommand = command as SmoothCurveTo;
                        writer.Write(pCommand.ControlEnd);
                        writer.Write(pCommand.End);
                    }
                    break;
                case CommandType.EllipticalArcTo:
                    {
                        EllipticalArcTo pCommand = command as EllipticalArcTo;
                        writer.Write(pCommand.Size.Width);
                        writer.Write(pCommand.Size.Height);
                        writer.Write(pCommand.End);
                        writer.Write(pCommand.RotationAngle);
                        writer.Write(pCommand.IsLargeArc);
                        writer.Write(pCommand.SweepDirection == SweepDirection.Clockwise);
                    }
                    break;
                case CommandType.QuadraticBeizerCurveTo:
                    {
                        QuadraticBeizerCurveTo pCommand = command as QuadraticBeizerCurveTo;
                        writer.Write(pCommand.Control);
                        writer.Write(pCommand.End);
                    }
                    break;
                case CommandType.SmoothQuadraticBeizerCurveTo:
                    {
                        SmoothQuadraticBeizerCurveTo pCommand = command as SmoothQuadraticBeizerCurveTo;
                        writer.Write(pCommand.End);
                    }
                    break;
                case CommandType.ClosePath:
                    {
                        // Do nothing
                    }
                    break;
            }
        }

        public static void Read(this IPathCommand command, System.IO.BinaryReader reader)
        {
            switch (command.Type)
            {
                case CommandType.MoveTo:
                    {
                        MoveTo pCommand = command as MoveTo;
                        pCommand.X = reader.ReadDouble();
                        pCommand.Y = reader.ReadDouble();
                    }
                    break;
                case CommandType.LineTo:
                    {
                        LineTo pCommand = command as LineTo;
                        pCommand.X = reader.ReadDouble();
                        pCommand.Y = reader.ReadDouble();
                    }
                    break;
                case CommandType.CurveTo:
                    {
                        CurveTo pCommand = command as CurveTo;
                        pCommand.ControlStart = reader.ReadPoint();
                        pCommand.ControlEnd = reader.ReadPoint();
                        pCommand.End = reader.ReadPoint();
                    }
                    break;
                case CommandType.SmoothCurveTo:
                    {
                        SmoothCurveTo pCommand = command as SmoothCurveTo;
                        pCommand.ControlEnd = reader.ReadPoint();
                        pCommand.End = reader.ReadPoint();
                    }
                    break;
                case CommandType.EllipticalArcTo:
                    {
                        EllipticalArcTo pCommand = command as EllipticalArcTo;
                        pCommand.Size = new Size(reader.ReadDouble(), reader.ReadDouble());
                        pCommand.End = reader.ReadPoint();
                        pCommand.RotationAngle = reader.ReadDouble();
                        pCommand.IsLargeArc = reader.ReadBoolean();
                        pCommand.SweepDirection = (reader.ReadBoolean() == false ? SweepDirection.Counterclockwise : SweepDirection.Clockwise);
                    }
                    break;
                case CommandType.QuadraticBeizerCurveTo:
                    {
                        QuadraticBeizerCurveTo pCommand = command as QuadraticBeizerCurveTo;
                        pCommand.Control = reader.ReadPoint();
                        pCommand.End = reader.ReadPoint();
                    }
                    break;
                case CommandType.SmoothQuadraticBeizerCurveTo:
                    {
                        SmoothQuadraticBeizerCurveTo pCommand = command as SmoothQuadraticBeizerCurveTo;
                        pCommand.End = reader.ReadPoint();
                    }
                    break;
                case CommandType.ClosePath:
                    {
                        // Do nothing
                    }
                    break;
            }
        }
    }
}
