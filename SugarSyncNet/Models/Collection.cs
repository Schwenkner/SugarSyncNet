// <copyright file="Collection.cs" company="BerlinSoft">
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
    /// Implements the <see cref="Collection"/> class of SugarSync
    /// </summary>
    [XmlRoot("collection")]
    public class Collection
    {
        /// <summary>
        /// Gets or sets the type of the collection
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the display name of the collection
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the reference url of the collection
        /// </summary>
        [XmlElement("ref")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the contents url of the collection
        /// </summary>
        [XmlElement("contents")]
        public string Contents { get; set; }
    }
}
