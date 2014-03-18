// <copyright file="Folder.cs" company="BerlinSoft">
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
    /// Implements the <see cref="Folder"/> class of SugarSync
    /// </summary>
    [XmlRoot("folder")]
    public class Folder
    {
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the folder
        /// </summary>
        [XmlElement("timeCreated")]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the reference url of the parent folder
        /// </summary>
        [XmlElement("parent")]
        public string Parent { get; set; }

        /// <summary>
        /// Gets or sets the url of the collections of this folder
        /// </summary>
        [XmlElement("collections")]
        public string Collections { get; set; }

        /// <summary>
        /// Gets or sets the reference url of the files of this folder
        /// </summary>
        [XmlElement("files")]
        public string Files { get; set; }

        /// <summary>
        /// Gets or sets the reference url of the contents of this folder
        /// </summary>
        [XmlElement("contents")]
        public string Contents { get; set; }
    }
}
