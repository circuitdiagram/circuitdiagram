﻿// This file is part of Circuit Diagram.
// Copyright (c) 2017 Samuel Fisher
//  
// Circuit Diagram is free software; you can redistribute it and/or
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
// along with Circuit Diagram. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CircuitDiagram.CLI.ComponentPreview;
using CircuitDiagram.Compiler;
using CircuitDiagram.TypeDescription;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.CLI.Component.OutputGenerators
{
    class BinaryComponentGenerator : IOutputGenerator
    {
        public string Format => "binary";

        public string FileExtension => ".cdcom";

        private readonly CompilerService compiler;

        public BinaryComponentGenerator(ILoggerFactory loggerFactory)
        {
            compiler = new CompilerService(loggerFactory);
        }

        public bool AcceptsSourceFileType(SourceFileType sourceType) => sourceType == SourceFileType.ComponentDescription;

        public void Generate(ComponentDescription description, ComponentConfiguration configuration, IResourceProvider resourceProvider, PreviewGenerationOptions options, Stream input, Stream output, SourceFileType sourceType)
        {
            ComponentCompileResult result = compiler.Compile(input, output, resourceProvider, new CompileOptions() { WriteExtendedMetadata = true });
            if (!result.Success)
                throw new Exception();
        }
    }
}
