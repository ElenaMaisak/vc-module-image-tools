﻿using System.Drawing;

namespace VirtoCommerce.ImageToolsModule.Web.Models
{
    /// <summary>
    /// Specification to generate a thumbnail.
    /// </summary>
    public class ThumbnailParameters
    {
        /// <summary>
        /// Method of thumbnails generation
        /// 
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Thumbnail width.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Thumbnail height.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Background layer color of image.
        /// If the original image has an aspect ratio different from thumbnail, 
        /// not covered part the thumbnail will be filled with that color.
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// Thumbnail alias (using to generate a thumbnail url as a suffix)  
        /// </summary>
        public string Alias { get; set; }

    }
}