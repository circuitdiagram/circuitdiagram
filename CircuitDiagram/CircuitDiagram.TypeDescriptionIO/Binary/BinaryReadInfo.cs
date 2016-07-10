using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO.Binary
{
    internal class BinaryReadInfo
    {
        public byte FormatVersion { get; set; }

        public bool IsSigned { get; set; }

        public bool IsSignatureValid { get; set; }

        public bool IsCertificateTrusted { get; set; }

        public X509Certificate2 Certificate { get; set; }
    }
}
