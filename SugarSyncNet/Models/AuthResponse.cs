// <copyright file="AuthResponse.cs" company="BerlinSoft">
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
    /// Implements the <see cref=" AuthResponse"/> class of SugarSync
    /// </summary>
    [XmlRoot("authorization")]
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets the expiration date and time of the access token
        /// </summary>
        [XmlElement("expiration")]
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Gets or sets the user
        /// </summary>
        [XmlElement("user")]
        public string User { get; set; }
    }
}
