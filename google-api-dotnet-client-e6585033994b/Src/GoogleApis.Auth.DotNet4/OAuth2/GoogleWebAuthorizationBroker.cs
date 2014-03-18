﻿/*
Copyright 2013 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

namespace Google.Apis.Auth.OAuth2
{
    /// <summary>A helper utility to manage the authorization code flow.</summary>
    public class GoogleWebAuthorizationBroker
    {
        /// <summary>The folder which is used by the <seealso cref="Google.Apis.Util.Store.FileDataStore"/>.</summary>
        public static string Folder = "Google.Apis.Auth";

        /// <summary>Asynchronously authorizes the specified user.</summary>
        /// <remarks>
        /// In case no data store is specified, <seealso cref="Google.Apis.Util.Store.FileDataStore"/> will be used by 
        /// default.
        /// </remarks>
        /// <param name="clientSecrets">The client secrets.</param>
        /// <param name="scopes">
        /// The scopes which indicate the Google API access your application is requesting.
        /// </param>
        /// <param name="user">The user to authorize.</param>
        /// <param name="taskCancellationToken">Cancellation token to cancel an operation.</param>
        /// <param name="dataStore">The data store, if not specified a file data store will be used.</param>
        /// <returns>User credential.</returns>
        public static async Task<UserCredential> AuthorizeAsync(ClientSecrets clientSecrets,
            IEnumerable<string> scopes, string user, CancellationToken taskCancellationToken,
            IDataStore dataStore = null)
        {
            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
            };
            return await AuthorizeAsyncCore(initializer, scopes, user, taskCancellationToken, dataStore);
        }

        /// <summary>Asynchronously authorizes the specified user.</summary>
        /// <remarks>
        /// In case no data store is specified, <seealso cref="Google.Apis.Util.Store.FileDataStore"/> will be used by 
        /// default.
        /// </remarks>
        /// <param name="clientSecretsStream">
        /// The client secrets stream. The authorization code flow constructor is responsible for disposing the stream.
        /// </param>
        /// <param name="scopes">
        /// The scopes which indicate the Google API access your application is requesting.
        /// </param>
        /// <param name="user">The user to authorize.</param>
        /// <param name="taskCancellationToken">Cancellation token to cancel an operation.</param>
        /// <param name="dataStore">The data store, if not specified a file data store will be used.</param>
        /// <returns>User credential.</returns>
        public static async Task<UserCredential> AuthorizeAsync(Stream clientSecretsStream,
            IEnumerable<string> scopes, string user, CancellationToken taskCancellationToken,
            IDataStore dataStore = null)
        {
            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecretsStream = clientSecretsStream,
            };
            return await AuthorizeAsyncCore(initializer, scopes, user, taskCancellationToken, dataStore);
        }

        /// <summary>The core logic for asynchronously authorizing the specified user.</summary>
        /// <param name="initializer">The authorization code initializer.</param>
        /// <param name="scopes">
        /// The scopes which indicate the Google API access your application is requesting.
        /// </param>
        /// <param name="user">The user to authorize.</param>
        /// <param name="taskCancellationToken">Cancellation token to cancel an operation.</param>
        /// <param name="dataStore">The data store, if not specified a file data store will be used.</param>
        /// <returns>User credential.</returns>
        private static async Task<UserCredential> AuthorizeAsyncCore(AuthorizationCodeFlow.Initializer initializer,
            IEnumerable<string> scopes, string user, CancellationToken taskCancellationToken,
            IDataStore dataStore = null)
        {
            initializer.Scopes = scopes;
            initializer.DataStore = dataStore ?? new FileDataStore(Folder);
            var flow = new GoogleAuthorizationCodeFlow(initializer);

            // Create authorization code installed app instance and authorize the user.
            return await new AuthorizationCodeInstalledApp(flow, new LocalServerCodeReceiver()).AuthorizeAsync
                (user, taskCancellationToken);
        }
    }
}