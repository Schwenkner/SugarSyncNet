// <copyright file="AccessTokenRequest.cs" company="BerlinSoft">
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
    /// Implements the <see cref=" AccessTokenRequest"/> class of SugarSync
    /// </summary>
    [XmlRoot("tokenAuthRequest")]
    public class AccessTokenRequest
    {
        /// <summary>
        /// Gets or sets the access key id
        /// </summary>
        [XmlElement("accessKeyId")]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the secret access key
        /// </summary>
        [XmlElement("privateAccessKey")]
        public string PrivateAccessKey { get; set; }

        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        [XmlElement("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
