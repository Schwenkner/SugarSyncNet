// <copyright file="Client.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SugarSyncNet;
using SugarSyncNet.Models;

namespace SugarSyncNet
{
    /// <summary>
    /// Represents a SugarSync client
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Home of SugarSync url
        /// </summary>
        private static string sugarSyncUrl = "https://api.sugarsync.com/";

        /// <summary>
        /// the api access key
        /// </summary>
        private string apiAccessKey;

        /// <summary>
        /// the secret access key 
        /// </summary>
        private string secretAccessKey;

        /// <summary>
        /// the application id
        /// </summary>
        private string applicationId;

        /// <summary>
        /// the access token
        /// </summary>
        private string accessToken;

        /// <summary>
        /// the user login
        /// </summary>
        private UserLogin userLogin;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class of SugarSync.
        /// </summary>
        /// <param name="accessKeyId">The SugarSync public access key Id</param>
        /// <param name="privateAccessKey">The SugarSync private access key Id</param>
        /// <param name="applicationId">The SugarSync application Id</param>
        public Client(string accessKeyId, string privateAccessKey, string applicationId)
        {
            this.apiAccessKey = accessKeyId;
            this.secretAccessKey = privateAccessKey;
            this.applicationId = applicationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class of SugarSync.
        /// </summary>
        /// <param name="accessKeyId">The SugarSync public access key Id</param>
        /// <param name="privateAccessKey">The SugarSync private access key Id</param>
        /// <param name="applicationId">The SugarSync application Id</param>
        /// <param name="refreshToken">The SugarSync refresh token, used to get the access token</param>
        public Client(string accessKeyId, string privateAccessKey, string applicationId, string refreshToken)
            : this(accessKeyId, privateAccessKey, applicationId)
        {
            UserLogin = new UserLogin(refreshToken);
        }

        /// <summary>
        /// Gets the SugarSync url
        /// </summary>
        public static string SugarSyncUrl
        {
            get { return sugarSyncUrl; }
        }

        /// <summary>
        /// Gets or sets the <see cref="UserLogin"/> with the fetched refresh token
        /// </summary>
        public UserLogin UserLogin
        {
            get { return userLogin; }
            set { userLogin = value; }
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Gets the refresh token used to login later. SugarSync provides a kind of oauth without requesting the access token via a web.
        /// If using this method to get the access token, the user credential are never stored ore published
        /// </summary>
        /// <param name="userName">User name of the SugarSync account</param>
        /// <param name="password">User secret password of the SugarSync account</param>
        /// <returns>The <see cref="UserLogin"/> with the got refresh token</returns>
        public UserLogin GetRefreshToken(string userName, string password)
        {
            SugarSyncRequest<RefreshTokenRequest> request = new SugarSyncRequest<RefreshTokenRequest>("POST", new Uri(SugarSyncUrl + "app-authorization", UriKind.Absolute), null, new RefreshTokenRequest { UserName = userName, Password = password, AccessKeyId = apiAccessKey, PrivateAccessKey = secretAccessKey, Application = applicationId });
            return new UserLogin(Execute<object, RefreshTokenRequest>(request).Response.Headers["Location"]);
        }
#endif

        /// <summary>
        /// Gets the refresh token in asynchronous way used to login later. SugarSync provides a kind of oauth without requesting the access token via a web.
        /// If using this method to get the access token, the user credential are never stored ore published
        /// </summary>
        /// <param name="userName">User name of the SugarSync account</param>
        /// <param name="password">User secret password of the SugarSync account</param>
        /// <returns>The <see cref="UserLogin"/> with the got refresh token</returns>
        public async Task<UserLogin> GetRefreshTokenAsync(string userName, string password)
        {
            SugarSyncRequest<RefreshTokenRequest> request = new SugarSyncRequest<RefreshTokenRequest>("POST", new Uri(SugarSyncUrl + "app-authorization", UriKind.Absolute), null, new RefreshTokenRequest { UserName = userName, Password = password, AccessKeyId = apiAccessKey, PrivateAccessKey = secretAccessKey, Application = applicationId });
            return new UserLogin((await ExecuteAsync<object, RefreshTokenRequest>(request)).Response.Headers["Location"]);
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Retrieves an access token by given refresh token.
        /// This method is only for testing purposes, there is no need to fetch the access token.
        /// </summary>
        /// <param name="refreshToken">The saved refresh token</param>
        /// <returns>The access token</returns>
        public string GetAccessToken(string refreshToken)
        {
            SugarSyncRequest<AccessTokenRequest> request = new SugarSyncRequest<AccessTokenRequest>("POST", new Uri(SugarSyncUrl + "authorization", UriKind.Absolute), null, new AccessTokenRequest { AccessKeyId = apiAccessKey, PrivateAccessKey = secretAccessKey, RefreshToken = refreshToken });
            return Execute<AuthResponse, AccessTokenRequest>(request).Response.Headers["Location"];
        }
#endif
        /// <summary>
        /// Retrieves an access token by given refresh token in asynchronous way.
        /// This method is only for testing purposes, there is no need to fetch the access token.
        /// </summary>
        /// <param name="refreshToken">The saved refresh token</param>
        /// <returns>The access token</returns>
        public async Task<string> GetAccessTokenAsync(string refreshToken)
        {
            SugarSyncRequest<AccessTokenRequest> request = new SugarSyncRequest<AccessTokenRequest>("POST", new Uri(SugarSyncUrl + "authorization", UriKind.Absolute), null, new AccessTokenRequest { AccessKeyId = apiAccessKey, PrivateAccessKey = secretAccessKey, RefreshToken = refreshToken });
            return (await ExecuteAsync<AuthResponse, AccessTokenRequest>(request)).Response.Headers["Location"];
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// This method performs a GET request to SugarSync service and awaits the response
        /// </summary>
        /// <typeparam name="T">Type of a valid SugarSync response deserialized into the answer</typeparam>
        /// <param name="url">The url of the request</param>
        /// <returns>The from SugarSync service returned answer</returns>
        public T Get<T>(string url)
        {
            accessToken = accessToken ?? GetAccessToken(userLogin.RefreshToken);
            SugarSyncRequest<object> request = new SugarSyncRequest<object>("GET", new Uri(url, UriKind.Absolute), accessToken);
            return Execute<T, object>(request).Data;
        }
#endif

        /// <summary>
        /// This method performs a GET request to SugarSync service and awaits the response in asynchronous way
        /// </summary>
        /// <typeparam name="T">Type of a valid SugarSync response deserialized into the answer</typeparam>
        /// <param name="url">The url of the request</param>
        /// <returns>The returned answer from SugarSync service</returns>
        public async Task<T> GetAsync<T>(string url)
        {
            SugarSyncRequest<object> request = new SugarSyncRequest<object>("GET", new Uri(url, UriKind.Absolute), await AccessTokenAsync());
            return (await ExecuteAsync<T, object>(request)).Data;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// This method posts a request to SugarSync service and awaits the answer
        /// </summary>
        /// <typeparam name="T">Type of a valid SugarSync response deserialized into the answer</typeparam>
        /// <param name="url">The url of the request</param>
        /// <param name="data">The data to post</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse Post<T>(string url, T data)
        {
            accessToken = accessToken ?? GetAccessToken(userLogin.RefreshToken);
            SugarSyncRequest<T> request = new SugarSyncRequest<T>("POST", new Uri(url, UriKind.Absolute), accessToken, data);
            return Execute<object, T>(request).Response;
        }
#endif

        /// <summary>
        /// This method posts a request to SugarSync service and awaits the answer in asynchronous way
        /// </summary>
        /// <typeparam name="T">Type of a valid SugarSync response deserialized into the answer</typeparam>
        /// <param name="url">The url of the request</param>
        /// <param name="data">The data to post</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> PostAsync<T>(string url, T data)
        {
            SugarSyncRequest<T> request = new SugarSyncRequest<T>("POST", new Uri(url, UriKind.Absolute), await AccessTokenAsync(), data);
            return (await ExecuteAsync<object, T>(request)).Response;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method  executes Http PUT request to SugarSync
        /// </summary>
        /// <typeparam name="T">Type of the request</typeparam>
        /// <param name="url">Url address where to request the operation</param>
        /// <param name="data">Data to send to SugarSync</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse Put<T>(string url, T data)
        {
            accessToken = accessToken ?? GetAccessToken(userLogin.RefreshToken);
            SugarSyncRequest<T> request = new SugarSyncRequest<T>("PUT", new Uri(url, UriKind.Absolute), accessToken, data);
            return Execute<object, T>(request).Response;
        }
#endif

        /// <summary>
        /// Method  executes Http PUT request to SugarSync in asynchronous was
        /// </summary>
        /// <typeparam name="T">Type of the request</typeparam>
        /// <param name="url">Url address where to request the operation</param>
        /// <param name="data">Data to send to SugarSync</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> PutAsync<T>(string url, T data)
        {
            SugarSyncRequest<T> request = new SugarSyncRequest<T>("PUT", new Uri(url, UriKind.Absolute), await AccessTokenAsync(), data);
            return (await ExecuteAsync<object, T>(request)).Response;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method to delete a referenced item
        /// </summary>
        /// <param name="url">The url of the item</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse Delete(string url)
        {
            accessToken = accessToken ?? GetAccessToken(userLogin.RefreshToken);
            SugarSyncRequest<object> request = new SugarSyncRequest<object>("DELETE", new Uri(url, UriKind.Absolute), accessToken);
            return Execute<object, object>(request).Response;
        }
#endif

        /// <summary>
        /// Method to delete a referenced item in asynchronous way
        /// </summary>
        /// <param name="url">The url of the item</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> DeleteAsync(string url)
        {
            SugarSyncRequest<object> request = new SugarSyncRequest<object>("DELETE", new Uri(url, UriKind.Absolute), await AccessTokenAsync());
            return (await ExecuteAsync<object, object>(request)).Response;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method to delete a referenced item including it's child items in asynchronous way
        /// </summary>
        /// <param name="url">The url of the item</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse DeleteRecursive(string url)
        {
            CollectionContents contents = Get<CollectionContents>(url + "/contents");
            foreach (Collection colection in contents.Collections)
            {
                DeleteRecursive(colection.Reference);
            }

            foreach (SugarSyncNet.Models.File file in contents.Files)
            {
                DeleteRecursive(file.Reference);
            }

            return Delete(url);
        }
#endif

        /// <summary>
        /// Method to delete a referenced item including it's child items in asynchronous way
        /// </summary>
        /// <param name="url">The url of the item</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> DeleteRecursiveAsync(string url)
        {
            CollectionContents contents = await GetAsync<CollectionContents>(url + "/contents");
            foreach (Collection colection in contents.Collections)
            {
                await DeleteRecursiveAsync(colection.Reference);
            }

            foreach (SugarSyncNet.Models.File file in contents.Files)
            {
                await DeleteRecursiveAsync(file.Reference);
            }

            return await DeleteAsync(url);
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method to upload streamed data to SugarSync service.
        /// </summary>
        /// <param name="url">The url of the <see cref="CollectionContents"/> where to upload the data to.</param>
        /// <param name="stream">The stream to upload</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse Upload(string url, Stream stream, string contentType)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = contentType;
            request.Headers[HttpRequestHeader.Authorization] = accessToken;
#if !WINDOWS_PHONE
            request.IfModifiedSince = DateTime.UtcNow;
#else
			request.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif
            request.ContentLength = stream.Length;
            stream.CopyTo(request.GetRequestStream());
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }
#endif

        /// <summary>
        /// Method to upload streamed data to SugarSync service in asynchronous way.
        /// </summary>
        /// <param name="url">The url of the <see cref=" CollectionContents"/> where to upload the data to.</param>
        /// <param name="stream">The stream to upload</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> UploadAsync(string url, Stream stream, string contentType)
        {
            var taskComplete = new TaskCompletionSource<HttpWebResponse>();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = contentType;
            request.Headers[HttpRequestHeader.Authorization] = await AccessTokenAsync();
#if !WINDOWS_PHONE
            request.IfModifiedSince = DateTime.UtcNow;
#else
            request.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif

            using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, request))
            {
                stream.CopyTo(requestStream);
            }

            WebResponse response = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            return (HttpWebResponse)response;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method to upload a file to SugarSync service.
        /// This method checks if the file already exists and overrides it if so.
        /// Otherwise a new file is created
        /// </summary>
        /// <param name="url">The reference url of the <see cref="CollectionContents"/> where the file upload to</param>
        /// <param name="fileName">The file display name</param>
        /// <param name="stream">The stream to upload</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public HttpWebResponse UploadFile(string url, string fileName, Stream stream, string contentType)
        {
            CollectionContents collectionContents = Get<CollectionContents>(url + "/contents");
            SugarSyncNet.Models.File file = collectionContents.Files.FirstOrDefault(f => f.DisplayName == fileName);
            string fileReference;
            if (file == null)
            {
                HttpWebResponse response = Post(url, new SugarSyncNet.Models.File { DisplayName = fileName, MediaType = contentType });
                fileReference = response.Headers["Location"];
            }
            else
            {
                fileReference = file.Reference;
            }

            return Upload(fileReference + "/data", stream, "text/xml");
        }
#endif

        /// <summary>
        /// Method to upload a file to SugarSync service in asynchronous way.
        /// This method checks if the file already exists and overrides it if so.
        /// Otherwise a new file is created
        /// </summary>
        /// <param name="url">The reference url of the <see cref="CollectionContents"/> where the file upload to</param>
        /// <param name="fileName">The file display name</param>
        /// <param name="stream">The stream to upload</param>
        /// <param name="contentType">The mime type of the content</param>
        /// <returns>The <see cref="HttpWebResponse"/> of the request</returns>
        public async Task<HttpWebResponse> UploadFileAsync(string url, string fileName, Stream stream, string contentType)
        {
            CollectionContents collectionContents = await GetAsync<CollectionContents>(url + "/contents");
            SugarSyncNet.Models.File file = collectionContents.Files.FirstOrDefault(f => f.DisplayName == fileName);
            string fileReference;
            if (file == null)
            {
                HttpWebResponse response = await PostAsync(url, new SugarSyncNet.Models.File { DisplayName = fileName, MediaType = contentType });
                fileReference = response.Headers["Location"];
            }
            else
            {
                fileReference = file.Reference;
            }

            return await UploadAsync(fileReference + "/data", stream, "text/xml");
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Downloads a stream from the given url
        /// </summary>
        /// <param name="url">The reference url of the file to be downloaded</param>
        /// <returns>The data stream</returns>
        public Stream Download(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = accessToken;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            MemoryStream stream = new MemoryStream();
            response.GetResponseStream().CopyTo(stream);
            stream.Position = 0;
            return stream;
        }
#endif

        /// <summary>
        /// Downloads a stream from the given url in asynchronous way
        /// </summary>
        /// <param name="url">The reference url of the file to be downloaded</param>
        /// <returns>The data stream</returns>
        public async Task<Stream> DownloadAsync(string url)
        {
            var taskComplete = new TaskCompletionSource<Stream>();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Headers[HttpRequestHeader.Authorization] = AccessTokenAsync().Result;
#if !WINDOWS_PHONE
            request.IfModifiedSince = DateTime.UtcNow;
#else
            request.Headers[HttpRequestHeader.IfModifiedSince] = DateTime.UtcNow.ToString();
#endif

            WebResponse response = await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, request);
            MemoryStream stream = new MemoryStream();
            response.GetResponseStream().CopyTo(stream);
            stream.Position = 0;
            return stream;
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Downlaods a stream from SugarSync service from the given folder with the specified file name
        /// </summary>
        /// <param name="url">The reference url of the folder containing the file</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>The data stream</returns>
        public Stream DownloadFile(string url, string fileName)
        {
            CollectionContents collectionContents = Get<CollectionContents>(url + "/contents");
            SugarSyncNet.Models.File file = collectionContents.Files.FirstOrDefault(f => f.DisplayName == fileName);
            if (file != null)
            {
                return Download(file.FileData);
            }

            throw new SugarSyncException("File not found");
        }
#endif

        /// <summary>
        /// Downlaods a stream from SugarSync service from the given folder with the specified file name in asynchronous way
        /// </summary>
        /// <summary xml:lang="de">
        /// Das ist ein Test
        /// </summary>
        /// <param name="url">The reference url of the folder containing the file</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>The data stream</returns>
        public async Task<Stream> DownloadFileAsync(string url, string fileName)
        {
            var taskComplete = new TaskCompletionSource<Stream>();
            CollectionContents collectionContents = await GetAsync<CollectionContents>(url + "/contents");
            SugarSyncNet.Models.File file = collectionContents.Files.FirstOrDefault(f => f.DisplayName == fileName);
            if (file != null)
            {
                return await DownloadAsync(file.FileData);
            }

            throw new SugarSyncException("File not found");
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Returns an access token by given refresh token.
        /// The access token is used to authenticate further requests.
        /// This method takes care, if an access token is already requested
        /// </summary>
        /// <returns>The access token</returns>
        private string AccessToken()
        {
            if (userLogin == null)
            {
                throw new SugarSyncException("Not logged in");
            }

            return accessToken = accessToken ?? GetAccessToken(userLogin.RefreshToken);
        }
#endif
        /// <summary>
        /// Returns an access token in asynchronous way by given refresh token.
        /// The access token is used to authenticate further requests
        /// This method takes care, if an access token is already requested
        /// </summary>
        /// <returns>The access token</returns>
        private async Task<string> AccessTokenAsync()
        {
            if (userLogin == null)
            {
                throw new SugarSyncException("Not logged in");
            }

            return accessToken = accessToken ?? await GetAccessTokenAsync(userLogin.RefreshToken);
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Method to execute a <see cref="HttpWebRequest"/> and await the answer
        /// </summary>
        /// <typeparam name="TResponse">Type of the response</typeparam>
        /// <typeparam name="TRequest">Type of the request</typeparam>
        /// <param name="request">The request to be executed</param>
        /// <returns>The response returned from SugarSync</returns>
        private SugarSyncResponse<TResponse> Execute<TResponse, TRequest>(SugarSyncRequest<TRequest> request)
        {
            if (request.Data != null)
            {
                XmlSerializer requestSerializer = new XmlSerializer(typeof(TRequest));
                Stream stream = request.Request.GetRequestStream();
                requestSerializer.Serialize(stream, request.Data);
                stream.Close();
            }

            SugarSyncResponse<TResponse> response = new SugarSyncResponse<TResponse>();
            try
            {
                response.Response = (HttpWebResponse)request.Request.GetResponse();
            }
            catch (WebException webEx)
            {
                throw new SugarSyncException(webEx);
            }

            if (typeof(TResponse) != typeof(object))
            {
                XmlSerializer responseSerializer = new XmlSerializer(typeof(TResponse));
#if DEBUG
                StreamReader reader = new StreamReader(response.Response.GetResponseStream());
                string text = reader.ReadToEnd();
                Debug.WriteLine(text);
                StringReader stringReader = new StringReader(text);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                response.Data = (TResponse)responseSerializer.Deserialize(xmlReader);
#else
                response.Data = (TResponse)responseSerializer.Deserialize(response.Response.GetResponseStream());
#endif
            }

            return response;
        }
#endif

        /// <summary>
        /// Method to execute a <see cref="HttpWebRequest"/> in asynchronous way and await the answer
        /// </summary>
        /// <typeparam name="TResponse">Type of the response</typeparam>
        /// <typeparam name="TRequest">Type of the request</typeparam>
        /// <param name="request">The request to be executed</param>
        /// <returns>The response returned from SugarSync</returns>
        private async Task<SugarSyncResponse<TResponse>> ExecuteAsync<TResponse, TRequest>(SugarSyncRequest<TRequest> request)
        {
            SugarSyncResponse<TResponse> response = new SugarSyncResponse<TResponse>();
            if (request.Data != null)
            {
                using (var requestStream = await Task<Stream>.Factory.FromAsync(request.Request.BeginGetRequestStream, request.Request.EndGetRequestStream, null))
                {
                    XmlSerializer requestSerializer = new XmlSerializer(typeof(TRequest));
#if DEBUG
                    StringWriter writer = new StringWriter();
                    requestSerializer.Serialize(writer, request.Data);
#endif
                    requestSerializer.Serialize(requestStream, request.Data);
                }

                response.Response = (HttpWebResponse)await Task<WebResponse>.Factory.FromAsync(request.Request.BeginGetResponse, request.Request.EndGetResponse, request);
                if (typeof(TResponse) != typeof(object))
                {
                    XmlSerializer responseSerializer = new XmlSerializer(typeof(TResponse));
#if DEBUG
                    StreamReader reader = new StreamReader(response.Response.GetResponseStream());
                    string text = reader.ReadToEnd();
                    Debug.WriteLine(text);
                    XmlReader xmlReader = XmlReader.Create(new StringReader(text));
                    response.Data = (TResponse)responseSerializer.Deserialize(xmlReader);
#else
                    response.Data = (TResponse)responseSerializer.Deserialize(response.Response.GetResponseStream());
#endif
                }
            }
            else
            {
                request.Request.ContentType = string.Empty;
                response.Response = (HttpWebResponse)await Task<WebResponse>.Factory.FromAsync(request.Request.BeginGetResponse, request.Request.EndGetResponse, request);
                if (typeof(TResponse) != typeof(object))
                {
                    using (var responseStream = response.Response.GetResponseStream())
                    {
                        XmlSerializer responseSerializer = new XmlSerializer(typeof(TResponse));
#if DEBUG
                        StreamReader reader = new StreamReader(response.Response.GetResponseStream());
                        string text = reader.ReadToEnd();
                        Debug.WriteLine(text);
                        XmlReader xmlReader = XmlReader.Create(new StringReader(text));
                        response.Data = (TResponse)responseSerializer.Deserialize(xmlReader);
#else
                        response.Data = (TResponse)responseSerializer.Deserialize(response.Response.GetResponseStream());
#endif
                    }
                }
            }

            return response;
        }
    }
}
