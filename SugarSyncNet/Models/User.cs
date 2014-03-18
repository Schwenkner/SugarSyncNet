// <copyright file="User.cs" company="BerlinSoft">
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
    /// Represents the <see cref="User"/> class of the SugarSync user
    /// </summary>
    [XmlRoot("user")]
    public class User
    {
        /// <summary>
        /// Gets or sets the user name of the <see cref="User"/>
        /// </summary>
        [XmlElement("username")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the nick name of the <see cref="User"/>
        /// </summary>
        [XmlElement("nickname")]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the salt (reserved) of the <see cref="User"/>
        /// </summary>
        [XmlElement("salt")]
        public string Salt { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Quota"/> of the <see cref="User"/>
        /// </summary>
        [XmlElement("quota")]
        public Quota Quota { get; set; }

        /// <summary>
        /// Gets or sets the workspaces url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("workspaces")]
        public string Workspaces { get; set; }

        /// <summary>
        /// Gets or sets the sync folders url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("syncfolders")]
        public string SyncFolders { get; set; }

        /// <summary>
        /// Gets or sets the deleted items url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("deleted")]
        public string Deleted { get; set; }

        /// <summary>
        /// Gets or sets the magic briefcase url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("magicBriefcase")]
        public string MagicBriefcase { get; set; }

        /// <summary>
        /// Gets or sets the web archive url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("webArchive")]
        public string WebArchive { get; set; }

        /// <summary>
        /// Gets or sets the mobile photos url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("mobilePhotos")]
        public string MobilePhotos { get; set; }

        /// <summary>
        /// Gets or sets the albums url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("albums")]
        public string Albums { get; set; }

        /// <summary>
        /// Gets or sets the recent activities url (<see cref="Workspace"/>) of the <see cref="User"/>
        /// </summary>
        [XmlElement("recentActivities")]
        public string RecentActivities { get; set; }

        /// <summary>
        /// Gets or sets the received shares
        /// </summary>
        [XmlElement("receivedShares")]
        public string ReceivedShares { get; set; }

        /// <summary>
        /// Gets or sets the public links
        /// </summary>
        [XmlElement("publicLinks")]
        public string PublicLinks { get; set; }

        /// <summary>
        /// Gets or sets the maximum public link size
        /// </summary>
        [XmlElement("maximumPublicLinkSize")]
        public long MaximumPublicLinkSize { get; set; }
    }
}
