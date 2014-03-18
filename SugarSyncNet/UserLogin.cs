// <copyright file="UserLogin.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Net;
using SugarSyncNet.Models;

namespace SugarSyncNet
{
    /// <summary>
    /// Represents the <see cref="UserLogin"/> class
    /// </summary>
    public class UserLogin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLogin"/> class
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        public UserLogin(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
