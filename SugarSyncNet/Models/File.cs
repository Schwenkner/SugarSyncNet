// <copyright file="File.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SugarSyncNet.Models
{
    /// <summary>
    /// Class implements the <see cref="File"/> of SugarSync
    /// </summary>
    [XmlRoot("file")]
    public class File
    {
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the reference
        /// </summary>
        [XmlElement("ref")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the size
        /// </summary>
        [XmlElement("size")]
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time
        /// </summary>
        [XmlElement("lastModified")]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the created date and time
        /// </summary>
        [XmlElement("timeCreated")]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the media type
        /// </summary>
        [XmlElement("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is present on server
        /// </summary>
        [XmlElement("presentOnServer")]
        public bool PresentOnServer { get; set; }

        /// <summary>
        /// Gets or sets the parent reference
        /// </summary>
        [XmlElement("parent")]
        public string Parent { get; set; }

        /// <summary>
        /// Gets or sets the file data reference
        /// </summary>
        [XmlElement("fileData")]
        public string FileData { get; set; }
    }
}
