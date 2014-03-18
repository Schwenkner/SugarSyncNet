// <copyright file="SugarSyncException.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SugarSyncNet
{
    /// <summary>
    /// Represents the <see cref="SugarSyncException"/> class
    /// </summary>
    public class SugarSyncException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncException"/> class
        /// </summary>
        /// <param name="status">The <see cref="HttpStatusCode"/> of the request caused the exception</param>
        public SugarSyncException(HttpStatusCode status)
        {
            StatusCode = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncException"/> class
        /// </summary>
        public SugarSyncException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncException"/> class
        /// </summary>
        /// <param name="response">The <see cref="HttpWebResponse"/> of the request caused the exception</param>
        public SugarSyncException(HttpWebResponse response)
            : this(response.StatusCode)
        {
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncException"/> class
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to be wrapped with this exception</param>
        public SugarSyncException(Exception exception)
            : base("SugarSync error occurred", exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SugarSyncException"/> class
        /// </summary>
        /// <param name="message">The message of the exception</param>
        public SugarSyncException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the <see cref="HttpWebResponse"/> of the request caused the exception
        /// </summary>
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> of the request caused the exception
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
    }
}
