// <copyright file="SugarSyncResponse.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SugarSyncNet
{
    /// <summary>
    /// Implements the <see cref="SugarSyncResponse&lt;T&gt;"/> class
    /// </summary>
    /// <typeparam name="T">Type of the response</typeparam>
    public class SugarSyncResponse<T>
    {
        /// <summary>
        /// Gets or sets the response
        /// </summary>
        public HttpWebResponse Response { get; set; }

        /// <summary>
        /// Gets or sets the data
        /// </summary>
        public T Data { get; set; }
    }
}
