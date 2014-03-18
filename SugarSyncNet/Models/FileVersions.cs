// <copyright file="FileVersions.cs" company="BerlinSoft">
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
    /// Class implements the <see cref="FileVersions"/> of SugarSync 
    /// </summary>
    [XmlRoot("fileVersions")]
    public class FileVersions
    {
        /// <summary>
        /// Gets or sets the version
        /// </summary>
        [XmlElement("fileVersion")]
        public List<File> Versions { get; set; }
    }
}
