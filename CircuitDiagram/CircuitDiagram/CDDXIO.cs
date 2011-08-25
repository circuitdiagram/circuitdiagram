using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace CircuitDiagram
{
    static class CDDXIO
    {
        public static void Write(string path, CircuitDocument document)
        {
            FileStream fileStream = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            // Write header
            writer.Write(MagicNumber); // Magic number, 4 bytes
            writer.Write(FormatVersion); // Format version, 4 bytes
            writer.Write(winAbout.AppVersion.ToString()); // Application version
            writer.Write((int)(CDDXContentEncoding.XML | CDDXContentEncoding.Deflate)); // Content encoding/compression
            writer.Write((uint)(fileStream.Position + 4)); // Offset to content
            
            // Write content
            DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
            document.Save(deflateStream);

            fileStream.Close();
        }

        public static CircuitDocument Read(string path)
        {
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                BinaryReader reader = new BinaryReader(fileStream);
                int magicNumber = reader.ReadInt32();
                int formatVersion = reader.ReadInt32();
                string appVersion = reader.ReadString();
                CDDXContentEncoding contentFlags = (CDDXContentEncoding)reader.ReadInt32();
                uint contentOffset = 0;
                if (formatVersion >= 2)
                    contentOffset = reader.ReadUInt32(); // offset to content

                if (formatVersion >= 2)
                    fileStream.Seek(contentOffset, SeekOrigin.Begin);
                CircuitDocument newDocument = new CircuitDocument();
                DocumentLoadResult result = DocumentLoadResult.None;
                if ((contentFlags & CDDXContentEncoding.Deflate) == CDDXContentEncoding.Deflate)
                {
                    DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
                    result = newDocument.Load(deflateStream);
                }
                else
                {
                    result = newDocument.Load(fileStream);
                }
                
                if (result == DocumentLoadResult.FailIncorrectFormat)
                    System.Windows.MessageBox.Show("The document was not in the correct format.", "Could Not Load Document", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                else if (formatVersion > FormatVersion)
                    System.Windows.MessageBox.Show("This document was created in a newer version of Circuit Diagram, and may not display correctly.", "Newer File Format", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return newDocument;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("The document was not in the correct format.", "Could Not Load Document", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new CircuitDocument();
            }
        }

        [Flags]
        enum CDDXContentEncoding
        {
            None = 0,
            /// <summary>
            /// The data is stored in XML format.
            /// </summary>
            XML = 1,
            /// <summary>
            /// The content is compressed using the DEFLATE algorithm.
            /// </summary>
            Deflate = 2
        }

        static readonly int MagicNumber = 6766888;
        static readonly int FormatVersion = 2;
    }

    public enum DocumentLoadResult
    {
        None = 0,
        Success = 1,
        FailUnknown = 2,
        FailNewerVersion = 3,
        FailIncorrectFormat = 4
    }
}
