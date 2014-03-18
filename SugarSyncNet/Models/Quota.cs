// <copyright file="Quota.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SugarSyncNet.Models
{
    /// <summary>
    /// Implements the <see cref="Quota"/> class of SugarSync
    /// </summary>
    public class Quota
    {
        /// <summary>
        /// Gets or sets the limit
        /// </summary>
        [XmlElement("limit")]
        public long Limit { get; set; }

        /// <summary>
        /// Gets or sets the current usage
        /// </summary>
        [XmlElement("usage")]
        public long Usage { get; set; }
    }
}
