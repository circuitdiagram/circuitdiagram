using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CircuitDiagram.TypeDescriptionIO
{
    /// <summary>
    /// Represents an image with copies at multiple resolutions available.
    /// </summary>
    public class MultiResolutionImage : List<SingleResolutionImage>
    {
        /// <summary>
        /// Gets the loaded icon data for display in the program UI.
        /// </summary>
        public List<object> LoadedIcons { get; set; }

        public MultiResolutionImage()
        {
            LoadedIcons = new List<object>();
        }
    }

    public class SingleResolutionImage
    {
        /// <summary>
        /// Gets or sets the raw data for this image.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets the mime type for this image.
        /// </summary>
        public string MimeType { get; set; }
    }
}
