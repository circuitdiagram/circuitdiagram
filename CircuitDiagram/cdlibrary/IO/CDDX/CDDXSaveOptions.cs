using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitDiagram.IO.CDDX
{
    /// <summary>
    /// Allows customizing how a CDDX document will be saved.
    /// </summary>
    public class CDDXSaveOptions : ISaveOptions
    {
        /// <summary>
        /// Whether connection information should be saved in the document.
        /// </summary>
        public bool Connections { get; set; }

        /// <summary>
        /// Whether layout information should be saved in the document.
        /// </summary>
        public bool Layout { get; set; }

        /// <summary>
        /// Whether a thumbnail should be embedded in the document.
        /// </summary>
        public bool Thumbnail { get; set; }

        /// <summary>
        /// Creates a new save options with the default settings.
        /// </summary>
        public CDDXSaveOptions()
        {
            // Set default settings.
            Connections = true;
            Layout = true;
            Thumbnail = true;
        }

        /// <summary>
        /// Creates a new save options with the specified settings.
        /// </summary>
        /// <param name="connections">Whether connection information should be saved in the document.</param>
        /// <param name="layout">Whether layout information should be saved in the document.</param>
        /// <param name="thumbnail">Whether a thumbnail should be embedded in the document.</param>
        public CDDXSaveOptions(bool connections, bool layout, bool thumbnail)
        {
            Connections = connections;
            Layout = layout;
            Thumbnail = thumbnail;
        }

        /// <summary>
        /// Serializes the save options to the specified serializer.
        /// </summary>
        /// <param name="serializer">The serialization object.</param>
        public void Serialize(ISerializer serializer)
        {
            serializer.Add("Connections", Connections);
            serializer.Add("Layout", Layout);
            serializer.Add("Thumbnail", Thumbnail);
        }

        /// <summary>
        /// Deserializes the save options from the specified serializer.
        /// </summary>
        /// <param name="serializer">The serialization object.</param>
        public void Deserialize(ISerializer serializer)
        {
            if (serializer.HasProperty("Connections"))
                Connections = serializer.GetBool("Connections");
            if (serializer.HasProperty("Layout"))
                Layout = serializer.GetBool("Layout");
            if (serializer.HasProperty("Thumbnail"))
                Thumbnail = serializer.GetBool("Thumbnail");
        }

        public static bool operator ==(CDDXSaveOptions a, CDDXSaveOptions b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return (a.Connections == b.Connections && a.Layout == b.Layout && a.Thumbnail == b.Thumbnail);
        }

        public static bool operator !=(CDDXSaveOptions a, CDDXSaveOptions b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CDDXSaveOptions))
                return false;

            return (this == obj as CDDXSaveOptions);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
