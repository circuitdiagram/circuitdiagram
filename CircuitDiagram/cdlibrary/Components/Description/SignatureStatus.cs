using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CircuitDiagram.Components.Description
{
    public class SignatureStatus
    {
        /// <summary>
        /// Gets or a value indicating whether the component is signed.
        /// </summary>
        public bool IsSigned { get { return Certificate != null; } }

        /// <summary>
        /// Gets or sets a value indicating whether the signature hash is valid.
        /// </summary>
        public bool IsHashValid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the signature used is trusted.
        /// </summary>
        public bool IsCertificateTrusted { get; set; }

        /// <summary>
        /// Gets or sets the signature used to sign this component.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Gets a value indicating whether thie component is validly signed.
        /// </summary>
        public bool IsSignatureValid { get { return IsSigned && IsHashValid && IsCertificateTrusted; } }
    }
}
