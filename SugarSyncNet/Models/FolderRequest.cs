// <copyright file="FolderRequest.cs" company="BerlinSoft">
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
    /// Implements the <see cref="FolderRequest"/> class of SugarSync
    /// </summary>
    [XmlRoot("folder")]
    public class FolderRequest
    {
        /// <summary>
        /// Gets or sets the display name of the folder
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }
    }
}
