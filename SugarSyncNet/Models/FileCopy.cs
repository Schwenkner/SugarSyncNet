// <copyright file="FileCopy.cs" company="BerlinSoft">
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
    /// Class implements the <see cref=" FileCopy"/> of SugarSync
    /// </summary>
    [XmlRoot("fileCopy")]
    public class FileCopy
    {
        /// <summary>
        /// Gets or sets the source reference
        /// </summary>
        [XmlAttribute("source")]
        public string SourceReference { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }
    }
}
