// <copyright file="Workspace.cs" company="BerlinSoft">
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
    /// Represents the <see cref="Workspace"/> class of a SugarSync workspace
    /// </summary>
    [XmlRoot("workspace")]
    public class Workspace
    {
        /// <summary>
        /// Gets or sets the display name of the <see cref="Workspace"/>
        /// </summary>
        [XmlElement("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the <see cref="Workspace"/>
        /// </summary>
        [XmlElement("timeCreated")]
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the collections url of the <see cref="Workspace"/>
        /// </summary>
        [XmlElement("collections")]
        public string Collections { get; set; }

        /// <summary>
        /// Gets or sets the files url of the <see cref="Workspace"/>
        /// </summary>
        [XmlElement("files")]
        public string Files { get; set; }

        /// <summary>
        /// Gets or sets the contents url of the <see cref="Workspace"/>
        /// </summary>
        [XmlElement("contents")]
        public string Contents { get; set; }
    }
}
