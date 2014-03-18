// <copyright file="SugarSyncRequest.cs" company="BerlinSoft">
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
    /// Implements the generic <see cref="SugarSyncRequest&lt;T&gt;"/> class
    /// </summary>
    /// <typeparam name="T">Type of the request</typeparam>
    public class SugarSyncRequest<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncRequest&lt;T&gt;"/> class
        /// </summary>
        /// <param name="method">Method of the request</param>
        /// <param name="uri">The url of the request</param>
        /// <param name="authQuest">A valid access token</param>
        /// <param name="data">The data to send</param>
        public SugarSyncRequest(string method, Uri uri, string authQuest = null, T data = default(T))
        {
            Request = HttpWebRequest.Create(uri) as HttpWebRequest;
            Request.Method = method;
            Request.ContentType = "application/xml; charset=UTF-8";
#if !WINDOWS_PHONE
            Request.IfModifiedSince = DateTime.UtcNow;
#else
            Request.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif
            if (!string.IsNullOrEmpty(authQuest))
            {
                Request.Headers[HttpRequestHeader.Authorization] = authQuest;
            }

            Data = data;
        }

        /// <summary>
        /// Gets the request
        /// </summary>
        public HttpWebRequest Request { get; private set; }

        /// <summary>
        /// Gets or sets the data
        /// </summary>
        public T Data { get; set; }
    }
}
