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
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using CircuitDiagram.IO;
using CircuitDiagram.TypeDescriptionIO.Binary;
using Microsoft.Extensions.Logging.Abstractions;

namespace CircuitDiagram.Compiler.CompileStages
{
    class OutputCdcomCompileStage : ICompileStage
    {
        public void Run(CompileContext context)
        {
            var settings = new BinaryWriter.BinaryWriterSettings();

            if (context.Options.CertificateThumbprint != null)
                settings.Certificate = FindCertificate(context.Options.CertificateThumbprint);

            var writer = new BinaryWriter(context.Output, settings);

            var transformer = new BinaryCompatibilityTransformer(new NullLogger<BinaryCompatibilityTransformer>());
            var compatibleDescription = transformer.Transform(context.Description);

            writer.Descriptions.Add(compatibleDescription);
            writer.Write();
        }

        private static X509Certificate2 FindCertificate(object thumbprint)
        {
            var store = new X509Store("MY", StoreLocation.CurrentUser);
            store.Open(OpenFlags.OpenExistingOnly);
            var fcollection = store.Certificates;
            return fcollection.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];
        }
    }
}
