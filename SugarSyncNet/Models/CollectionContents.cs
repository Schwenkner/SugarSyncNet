// <copyright file="CollectionContents.cs" company="BerlinSoft">
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
    /// Class implements the <see cref=" CollectionContents"/> of SugarSync
    /// </summary>
    [XmlRoot("collectionContents")]
    public class CollectionContents
    {
        /// <summary>
        /// Gets or sets the start value for collections to get
        /// </summary>
        [XmlAttribute("start")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the end value for collections to get
        /// </summary>
        [XmlAttribute("end")]
        public int End { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we have more collections
        /// </summary>
        [XmlAttribute("hasMore")]
        public bool HasMore { get; set; }

        /// <summary>
        /// Gets or sets the list of got collections
        /// </summary>
        [XmlElement("collection")]
        public List<Collection> Collections { get; set; }

        /// <summary>
        /// Gets or sets the list of got files
        /// </summary>
        [XmlElement("file")]
        public List<File> Files { get; set; }
    }
}
