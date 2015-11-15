using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using CircuitDiagram.IO;

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
            writer.Descriptions.Add(context.Description);
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
