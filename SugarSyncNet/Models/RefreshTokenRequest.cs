// <copyright file="RefreshTokenRequest.cs" company="BerlinSoft">
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
    /// Class implements the <see cref=" RefreshTokenRequest"/> of SugarSync
    /// </summary>
    [XmlRoot("appAuthorization")]
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        [XmlElement("username")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [XmlElement("password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the access key id
        /// </summary>
        [XmlElement("accessKeyId")]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the private access key
        /// </summary>
        [XmlElement("privateAccessKey")]
        public string PrivateAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the application id
        /// </summary>
        [XmlElement("application")]
        public string Application { get; set; }
    }
}
