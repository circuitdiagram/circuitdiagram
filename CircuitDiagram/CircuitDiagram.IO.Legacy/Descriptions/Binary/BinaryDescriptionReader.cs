using CircuitDiagram.Components;
using CircuitDiagram.Components.Description;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CircuitDiagram.TypeDescription;

namespace CircuitDiagram.IO.Descriptions
{
    public class BinaryDescriptionReader
    {
        bool isSigned;
        bool validSignature;
        bool certificateTrusted;
        X509Certificate2 certificate;

        public List<ComponentDescription> ComponentDescriptions = new List<ComponentDescription>();
        public List<BinaryResource> Resources = new List<BinaryResource>();
        public List<BinaryDescriptionContentItem> Items = new List<BinaryDescriptionContentItem>();
        public Dictionary<uint, MultiResolutionImage> Images = new Dictionary<uint, MultiResolutionImage>(); 

        public X509Chain CertificateChain { get; set; }

        public bool Read(Stream stream)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);

                // read header
                ulong magicNumber = reader.ReadUInt64();
                byte formatVersion = reader.ReadByte();
                byte[] md5hash = reader.ReadBytes(16);
                uint reserved = reader.ReadUInt32();
                uint fileLength = reader.ReadUInt32();
                uint offsetToContent = reader.ReadUInt32();
                uint numContentItems = reader.ReadUInt32();

                // Signing
                CheckSignature(stream, reader, offsetToContent);

                // Construct BinaryReadInfo
                BinaryReadInfo readInfo = new BinaryReadInfo()
                {
                     FormatVersion = formatVersion,
                     IsSigned = isSigned,
                     IsSignatureValid = validSignature,
                     IsCertificateTrusted = certificateTrusted
                };

                var componentDescriptions = new List<ComponentResource>();

                // Load content
                for (uint contentCounter = 0; contentCounter < numContentItems; contentCounter++)
                {
                    var itemType = (BinaryConstants.ContentItemType)reader.ReadUInt16();

                    if (itemType == BinaryConstants.ContentItemType.Resource)
                    {
                        var item = new BinaryResource();
                        item.Read(reader, readInfo);
                        Items.Add(item);
                        Resources.Add(item);
                    }
                    else if (itemType == BinaryConstants.ContentItemType.Component)
                    {
                        var item = new ComponentResource();
                        item.Read(reader, readInfo);
                        Items.Add(item);
                        componentDescriptions.Add(item);
                    }
                    else
                    {
                        // Unknown item type
                        continue;
                    }
                }

                // Process images
                foreach(var resource in Resources)
                {
                    if (resource.ResourceType == BinaryResourceType.BitmapImage
                        || resource.ResourceType == BinaryResourceType.JPEGImage
                        || resource.ResourceType == BinaryResourceType.PNGImage
                        || true)
                    {
                        if (!Images.ContainsKey(resource.ID))
                            Images.Add(resource.ID, new MultiResolutionImage());

                        Images[resource.ID].Add(new SingleResolutionImage()
                        {
                            MimeType = BinaryConstants.ResourceMimeTypeToString((uint)resource.ResourceType),
                            Data = resource.Buffer
                        });
                    }
                }

                // Add component descriptions
                foreach(var componentDescription in componentDescriptions)
                {
                    componentDescription.SetIcons(Images);
                    ComponentDescriptions.Add(componentDescription.ComponentDescription);
                }

                return true;
            }
            catch (Exception)
            {
                // Invalid binary file
                return false;
            }
        }

        private void CheckSignature(Stream stream, BinaryReader reader, uint offsetToContent)
        {
            isSigned = reader.ReadBoolean();
            byte[] sha1Sig = null;
            byte[] certData = null;
            if (isSigned)
            {
                int sigLength = reader.ReadInt32();
                sha1Sig = reader.ReadBytes(sigLength);
                int certLength = reader.ReadInt32();
                certData = reader.ReadBytes(certLength);
            }

            if (reader.BaseStream.Position != offsetToContent)
                reader.BaseStream.Seek(offsetToContent, SeekOrigin.Begin);

            if (isSigned)
            {
                certificate = new X509Certificate2(certData);
                certificateTrusted = CertificateChain.Build(certificate);

                byte[] buffer = new byte[stream.Length - stream.Position];
                stream.Read(buffer, 0, buffer.Length);

                var dsa = certificate.PublicKey.Key as RSACryptoServiceProvider;
                validSignature = dsa.VerifyData(buffer, new SHA1CryptoServiceProvider(), sha1Sig);

                reader.BaseStream.Seek(offsetToContent, SeekOrigin.Begin);
            }
        }
    }
}
