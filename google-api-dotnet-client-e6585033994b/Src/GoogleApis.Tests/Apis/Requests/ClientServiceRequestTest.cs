﻿/*
Copyright 2011 Google Inc

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Ionic.Zlib;
using NUnit.Framework;

using Google.Apis.Discovery;
using Google.Apis.Http;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util;


namespace Google.Apis.Tests.Apis.Requests
{
    /// <summary>Tests for the <seealso cref="Google.Apis.Requests.ClientServiceRequest"/>.</summary>
    [TestFixture]
    public class ClientServiceRequestTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            // Uncomment to enable logging during tests
            // ApplicationContext.RegisterLogger(new Google.Apis.Logging.Log4NetLogger());
        }

        /// <summary>Helper method to get a string from the stream.</summary>
        private static string ExtractStringFromStream(Stream stream)
        {
            var buffer = new byte[1000];
            var len = stream.Read(buffer, 0, 1000);
            return Encoding.UTF8.GetString(buffer, 0, len);
        }

        /// <summary>A mock response class.</summary>
        class MockResponse : IDirectResponseSchema
        {
            [Newtonsoft.Json.JsonPropertyAttribute("etag")]
            public string ETag { get; set; }

            [Newtonsoft.Json.JsonPropertyAttribute("name")]
            public string Name { get; set; }

            [Newtonsoft.Json.JsonPropertyAttribute("id")]
            public int Id { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as MockResponse;
                return (other != null && other.ETag == ETag && other.Name == Name && other.Id == Id);
            }

            public override int GetHashCode()
            {
                return (ETag ?? string.Empty).GetHashCode() ^ (Name ?? string.Empty).GetHashCode() + Id;
            }
        }

        /// <summary>A mock request class.</summary>
        class MockRequest : IDirectResponseSchema
        {
            [Newtonsoft.Json.JsonPropertyAttribute("etag")]
            public string ETag { get; set; }

            [Newtonsoft.Json.JsonPropertyAttribute("long_name")]
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as MockRequest;
                return (other != null && other.ETag == ETag && other.Name == Name);
            }

            public override int GetHashCode()
            {
                return (ETag ?? string.Empty).GetHashCode() ^ (Name ?? string.Empty).GetHashCode();
            }
        }

        /// <summary>A mock service request which returns <see cref="MockResponse"/>.</summary>
        class TestClientServiceRequest : ClientServiceRequest<MockResponse>
        {
            /// <summary>Gets or sets a request number. It's used on concurrent tests.</summary>
            public int CallNum { get; set; }
            private string httpMethod;
            private object body;

            public TestClientServiceRequest(IClientService service, string httpMethod, object body)
                : base(service)
            {
                this.httpMethod = httpMethod;
                this.body = body;
                InitParameters();
            }

            public override string MethodName
            {
                get { return httpMethod; }
            }

            public override string RestPath
            {
                get { return "restPath" + CallNum; }
            }

            public override string HttpMethod
            {
                get { return httpMethod; }
            }

            protected override object GetBody()
            {
                return body;
            }
        }

        /// <summary>A mock message handler which returns an error.</summary>
        class ErrorMessageHanlder : CountableMessageHandler
        {
            public string ExpectedError =
                @"Message[Login Required] Location[Authorization - header] Reason[required] Domain[global]";

            protected override Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var error = @"{
                    ""error"": {
                        ""errors"": [
                            {
                                ""domain"": ""global"",
                                ""reason"": ""required"",
                                ""message"": ""Login Required"",
                                ""locationType"": ""header"",
                                ""location"": ""Authorization""
                            }
                        ],
                        ""code"": 401,
                        ""message"": ""Login Required""
                    }
                }";

                var response = new HttpResponseMessage
                    {
                        Content = new StringContent(error),
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };

                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }
        }

        /// <summary>Tests message handler which tests the content on the request and the response.</summary>
        class TestBodyMessageHandler : CountableMessageHandler
        {
            /// <summary>Gets or sets indication is GZip is eanbled.</summary>
            public bool GZipEnabled { get; set; }

            /// <summary>Gets or sets the expected request object.</summary>
            public MockRequest ExpectedRequestObject { get; set; }

            /// <summary>Gets or sets the returned response object </summary>
            public MockResponse ResponseObject { get; set; }

            /// <summary>Gets or sets the Serializer which is used to serialize and deserialize messages.</summary>
            public ISerializer Serializer { get; set; }

            public string ResponseETag = "\"some-etag-here\"";

            protected override async Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var mediaType = "application/json";
                string strObject = null;

                // If gzip enabled the request content is a gzip stream, otherwise it's a string content.
                if (GZipEnabled)
                {
                    Assert.That(request.Content, Is.AssignableFrom<StreamContent>());
                    var encoding = request.Content.Headers.ContentEncoding;
                    Assert.That(encoding.Count == 1 && encoding.First().Equals("gzip"),
                        "Content-Encoding should be 'gzip'");
                    var stream = await request.Content.ReadAsStreamAsync();
                    using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        strObject = ExtractStringFromStream(gzipStream);
                    }
                }
                else
                {
                    Assert.That(request.Content, Is.AssignableFrom<StringContent>());
                    strObject = await request.Content.ReadAsStringAsync();
                }

                Assert.That(request.Content.Headers.ContentType, Is.EqualTo(new MediaTypeHeaderValue(mediaType)
                    {
                        CharSet = Encoding.UTF8.WebName
                    }));

                // Deserialize the requested object and check it's equal to the expected object.
                var obj = Serializer.Deserialize<MockRequest>(strObject);
                Assert.That(obj, Is.EqualTo(ExpectedRequestObject));

                // Return the response (with ETag).
                var response = new HttpResponseMessage();
                var serializedObject = Serializer.Serialize(ResponseObject);
                response.Content = new StringContent(serializedObject, Encoding.UTF8, mediaType);
                response.Headers.ETag = new EntityTagHeaderValue(ResponseETag);
                return response;
            }
        }

        /// <summary>
        /// A mock exception which is thrown from a mock message handler in case it is configured to throw exceptions. 
        /// </summary>
        class InvalidOperationMockException : Exception
        {
            public InvalidOperationMockException(string str)
                : base(str)
            {
            }
        }

        /// <summary>A message handler which returns a HTTP response message or throw an exception.</summary>
        class MockMessageHandler : CountableMessageHandler
        {
            private bool ThrowException { get; set; }
            public MockMessageHandler(bool throwException = false)
            {
                ThrowException = throwException;
            }

            protected override Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (ThrowException)
                {
                    throw new InvalidOperationMockException("INVALID");
                }

                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(new HttpResponseMessage());
                return tcs.Task;
            }
        }

        /// <summary>A message handler which is used to cancel a HTTP request in the middle.</summary>
        class CancelRedirectMessageHandler : CountableMessageHandler
        {
            /// <summary>The cancellation token we are going to use to cancel a request.</summary>
            public CancellationTokenSource CancellationTokenSource { get; set; }

            /// <summary>The request index we are going to cancel.</summary>
            public int CancelRequestNum { get; set; }

            protected override Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (Calls == CancelRequestNum)
                {
                    CancellationTokenSource.Cancel();
                }
                var response = new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.Redirect,
                        RequestMessage = request
                    };
                response.Headers.Location = new Uri("http://www.test.com");

                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }
        }

        /// <summary>
        /// A message handler which checks concurrent calls (each odd request will succeeded, and even request will 
        /// fail on the first try and will succeeded in the second try.
        /// </summary>
        class ConcurrentCallsHandler : CountableMessageHandler
        {
            /// <summary>Gets or sets the Serializer which is used to serialize and deserialize messages.</summary>
            public ISerializer Serializer { get; set; }

            protected override Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage();
                var uri = request.RequestUri.AbsoluteUri;
                int lastDigit = 0;
                if (int.TryParse(uri[uri.Length - 1].ToString(), out lastDigit) && lastDigit % 2 == 0)
                {
                    response.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
                    request.RequestUri = new Uri(uri + (lastDigit + 1));
                }
                else
                {
                    var mockObject = new MockResponse { Name = "Name-" + lastDigit };
                    var serializedObject = Serializer.Serialize(mockObject);
                    response.Content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
                }

                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }

            /// <summary>Unsuccessful response handler which "handles" service unavailable responses.</summary>
            internal class ServiceUnavailableUnsuccessfulResponseHandler : IHttpUnsuccessfulResponseHandler
            {
                public Task<bool> HandleResponseAsync(HandleUnsuccessfulResponseArgs args)
                {
                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                    tcs.SetResult(args.Response.StatusCode.Equals(HttpStatusCode.ServiceUnavailable));
                    return tcs.Task;
                }
            }

            /// <summary>
            /// Adds service unavailable unsuccessful response handler to the configurable message handler.
            /// </summary>
            internal class Initializer : IConfigurableHttpClientInitializer
            {
                public void Initialize(ConfigurableHttpClient httpClient)
                {
                    httpClient.MessageHandler.UnsuccessfulResponseHandlers.Add(
                        new ServiceUnavailableUnsuccessfulResponseHandler());
                }
            }
        }

        #region Execute (and ExecuteAsync)

        /// <summary>Tests that canceling a outgoing request to the server works as expected.</summary>
        [Test]
        public void ExecuteAsync_Cancel()
        {
            SubtestExecuteAsync_Cancel(1);
            SubtestExecuteAsync_Cancel(5);
            SubtestExecuteAsync_Cancel(10);
            SubtestExecuteAsync_Cancel(11);
        }

        /// <summary>
        /// Test helper to test canceling token in a middle of a request to the server
        /// </summary>
        /// <param name="cancelRequestNum">
        /// The index of the "server"'s request which a cancel token will be mimic.
        /// </param>
        private void SubtestExecuteAsync_Cancel(int cancelRequestNum)
        {
            var handler = new CancelRedirectMessageHandler();
            handler.CancellationTokenSource = new CancellationTokenSource();
            handler.CancelRequestNum = cancelRequestNum;

            var initializer = new BaseClientService.Initializer()
                {
                    HttpClientFactory = new MockHttpClientFactory(handler)
                };

            TestClientServiceRequest request;
            using (var service = new MockClientService(initializer))
            {
                request = new TestClientServiceRequest(service, "POST", new MockRequest());
                try
                {
                    request.ExecuteAsync(handler.CancellationTokenSource.Token).Wait();
                    Assert.Fail();
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is TaskCanceledException)
                    {
                        // We expect a task canceled exception in case the canceled request is less or equal total
                        // number of retries.
                        Assert.False(cancelRequestNum > service.HttpClient.MessageHandler.NumRedirects + 1);
                    }
                    else
                    {
                        // Canceled exception wasn't thrown, in that case the cancel request number is bigger than
                        // the actual number of tries.
                        Assert.True(cancelRequestNum > service.HttpClient.MessageHandler.NumTries + 1);
                    }
                }

                var expectedCalls = Math.Min(service.HttpClient.MessageHandler.NumRedirects + 1, cancelRequestNum);
                Assert.That(handler.Calls, Is.EqualTo(expectedCalls));
            }
        }

        /// <summary>Tests the execute method in case the service was disposed.</summary>
        [Test]
        public void Execute_DisposeService()
        {
            var handler = new MockMessageHandler();
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            TestClientServiceRequest request;
            using (var service = new MockClientService(initializer))
            {
                request = new TestClientServiceRequest(service, "POST", new MockRequest());
            }

            // the service was disposed before the request was made (and the message handler as well). As a result an 
            // exception should be thrown before we try to send the request
            Assert.Throws<ObjectDisposedException>(() => request.Execute());
        }

        /// <summary>A subtest for testing GZip and sync-async calls.</summary>
        /// <param name="gzip">Defines if GZip is enabled</param>
        /// <param name="async">Defines which method is going to be called (Execute or ExecuteAsync)</param>
        private void SubtestExecute_GZip(bool gzip, bool async)
        {
            var handler = new TestBodyMessageHandler()
            {
                GZipEnabled = gzip,
                ResponseObject = new MockResponse { Id = 100, Name = "sample name" },
                ExpectedRequestObject = new MockRequest { Name = "long long name" }
            };

            var initializer = new BaseClientService.Initializer()
            {
                GZipEnabled = gzip,
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            using (var service = new MockClientService(initializer))
            {
                handler.Serializer = service.Serializer;

                var request = new TestClientServiceRequest(service, "POST", handler.ExpectedRequestObject);
                MockResponse response = null;
                if (async)
                {
                    var task = request.ExecuteAsync();
                    response = task.Result;
                }
                else
                {
                    response = request.Execute();
                }

                // Note: Even if GZipEnabled is true, we don't need to extract the real string from the GZip stream,
                // because in a real request we use HttpClientHandler which its AutomaticDecompression is set to 
                // System.Net.DecompressionMethods.GZip.

                Assert.That(handler.Calls, Is.EqualTo(1));
                // The returned response should contain ETag, check that the service adds the right ETag property on 
                // the response.
                handler.ResponseObject.ETag = handler.ResponseETag;
                Assert.That(response, Is.EqualTo(handler.ResponseObject));
            }
        }

        /// <summary>Tests execute when GZip is enabled.</summary>
        [Test]
        public void Execute_GZipEnabled()
        {
            SubtestExecute_GZip(true, false);
        }

        /// <summary>Tests execute when GZip is disabled.</summary>
        [Test]
        public void Execute_GZipDisabled()
        {
            SubtestExecute_GZip(false, false);
        }

        /// <summary>Tests async execute when GZip is enabled.</summary>
        [Test]
        public void ExecuteAsync_GZipEnabled()
        {
            SubtestExecute_GZip(true, true);
        }

        /// <summary>Tests async execute when GZip is disabled.</summary>
        [Test]
        public void ExecuteAsync_GZipDisabled()
        {
            SubtestExecute_GZip(false, true);
        }

        /// <summary>Tests execute with unicode characters.</summary>
        [Test]
        public void Execute_UnicodeCharacters()
        {
            var handler = new TestBodyMessageHandler()
            {
                GZipEnabled = false,
                ResponseObject = new MockResponse { Id = 100, Name = @"مرحبا العالم" },
                ExpectedRequestObject = new MockRequest { Name = @"مرحبا العالم! 您好，世界！" }
            };

            var initializer = new BaseClientService.Initializer()
            {
                GZipEnabled = false,
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            using (var service = new MockClientService(initializer))
            {
                handler.Serializer = service.Serializer;

                var request = new TestClientServiceRequest(service, "GET", handler.ExpectedRequestObject);
                var response = request.Execute();
                Assert.That(handler.Calls, Is.EqualTo(1));
                // The returned response should contain ETag, check that the service add the right ETag property on 
                // the response.
                handler.ResponseObject.ETag = handler.ResponseETag;
                Assert.That(response, Is.EqualTo(handler.ResponseObject));
            }
        }

        /// <summary>
        /// A subtest for testing Execute when an exception is thrown while sending the request. This is tested with
        /// and without back-off. If back-off handler is attached to the service's message handler, there should be 3
        /// tries (the default value of <seealso cref="ConfigurableMessageHandler.NumTries"/>) before the operation 
        /// fails.
        /// </summary>
        /// <param name="backOff">Indicates if back-off handler is attached to the service.</param>
        private void SubtestExecute_ThrowException(bool backOff)
        {
            var handler = new MockMessageHandler(true);
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            // Set the default exponential back-off policy by the input.
            initializer.DefaultExponentialBackOffPolicy = backOff ?
                ExponentialBackOffPolicy.Exception : ExponentialBackOffPolicy.None;

            using (var service = new MockClientService(initializer))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                Assert.Throws<InvalidOperationMockException>(() => request.Execute());

                int calls = backOff ? service.HttpClient.MessageHandler.NumTries : 1;
                Assert.That(handler.Calls, Is.EqualTo(calls));
            }
        }

        /// <summary>
        /// Tests execute when an exception is thrown during a request and exponential back-off is enabled.
        /// </summary>
        [Test]
        public void Execute_ThrowException_WithBackOff()
        {
            SubtestExecute_ThrowException(true);
        }

        /// <summary>
        /// Tests execute when an exception is thrown during a request and exponential back-off is disabled.
        /// </summary>
        [Test]
        public void Execute_ThrowException_WithoutBackOff()
        {
            SubtestExecute_ThrowException(false);
        }

        /// <summary>
        /// A subtest for testing ExecuteAsync  when an exception is thrown while sending the request. This is tested 
        /// with and without back-off. If back-off handler is attached to the service's message handler, there should 
        /// be 3 tries (the default value of <seealso cref="ConfigurableMessageHandler.NumTries"/>) before the 
        /// operation fails.
        /// </summary>
        /// <param name="backOff">Indicates if back-off handler is attached to the service.</param>
        private void SubtestExecuteAsync_ThrowException(bool backOff)
        {
            var handler = new MockMessageHandler(true);
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            // Configure the back-off behavior by the input.
            initializer.DefaultExponentialBackOffPolicy = backOff ?
                ExponentialBackOffPolicy.Exception : ExponentialBackOffPolicy.None;

            using (var service = new MockClientService(initializer))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                var task = request.ExecuteAsync();
                try
                {
                    var result = task.Result;
                    Assert.Fail("Exception should be thrown");
                }
                catch (AggregateException ex)
                {
                    Assert.That(ex.InnerException, Is.AssignableFrom(typeof(InvalidOperationMockException)));
                }

                int calls = backOff ? service.HttpClient.MessageHandler.NumTries : 1;
                Assert.That(handler.Calls, Is.EqualTo(calls));
            }
        }

        /// <summary>
        /// Tests async execute when an exception is thrown during a request and exponential back-off is enabled.
        /// </summary>
        [Test]
        public void ExecuteAsync_ThrowException_WithBackOff()
        {
            SubtestExecuteAsync_ThrowException(true);
        }

        /// <summary>
        /// Tests async execute when an exception is thrown during a request and exponential back-off is disabled.
        /// </summary>
        [Test]
        public void ExecuteAsync_ThrowException_WithoutBackOff()
        {
            SubtestExecuteAsync_ThrowException(false);
        }

        /// <summary>Tests execute when server returned an error.</summary>
        [Test]
        public void Execute_Error()
        {
            var handler = new ErrorMessageHanlder();
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            using (var service = new MockClientService(initializer))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                try
                {
                    request.Execute();
                    Assert.Fail();
                }
                catch (Exception ex)
                {
                    Assert.True(ex.Message.Contains(handler.ExpectedError), "Error message is invalid");
                }
                Assert.That(handler.Calls, Is.EqualTo(1));
            }
        }

        /// <summary>Tests execute when server returned an error.</summary>
        [Test]
        public void ExecuteAsync_Error()
        {
            var handler = new ErrorMessageHanlder();
            var initializer = new BaseClientService.Initializer
            {
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            using (var service = new MockClientService(initializer))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                AutoResetEvent resetEvent = new AutoResetEvent(false);
                var task = request.ExecuteAsync();
                var error = string.Empty;
                task.ContinueWith(t =>
                    {
                        // should not ENTER this code, the task should fail
                        resetEvent.Set();
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                task.ContinueWith(t =>
                    {
                        // catch the error
                        error = t.Exception.InnerException.ToString();
                        resetEvent.Set();
                    }, TaskContinuationOptions.NotOnRanToCompletion);
                resetEvent.WaitOne();
                Assert.True(error.Contains(handler.ExpectedError), "Error message is invalid");
                Assert.That(handler.Calls, Is.EqualTo(1));
            }
        }

        /// <summary>Tests async execution of multiple request simultaneously.</summary>
        [Test]
        public void ExecuteAsync_Simultaneously()
        {
            var tasks = new List<Task<MockResponse>>();
            var handler = new ConcurrentCallsHandler();
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientFactory = new MockHttpClientFactory(handler),
                HttpClientInitializer = new ConcurrentCallsHandler.Initializer()
            };

            using (var service = new MockClientService(initializer))
            {
                int calls = 100;
                handler.Serializer = service.Serializer;

                CountdownEvent ce = new CountdownEvent(calls);
                foreach (var i in Enumerable.Range(1, calls))
                {
                    var request = new TestClientServiceRequest(service, "GET", null) { CallNum = i };
                    var task = request.ExecuteAsync();
                    task.ContinueWith(c => ce.Signal());
                    tasks.Add(task);
                }
                ce.Wait();

                for (var i = 1; i <= tasks.Count; ++i)
                {
                    var response = tasks[i - 1].Result;

                    // check that we got the right response. Name should be equal to the index number modulo 10 (or 
                    // index number plus one module 10, if it's an even request - because even request should fail in 
                    // the first request, but should succeed in the next try). That's the logic of our 
                    // ConcurrentCallHandler, see it core SendAsyncCore method to understand better the logic
                    var nameIndex = ((i % 2 == 0) ? i + 1 : i) % 10;
                    Assert.That(response.Name, Is.EqualTo("Name-" + nameIndex));
                }

                // half of the request should succeed in the second call, so total calls should be calls + calls/2
                Assert.That(handler.Calls, Is.EqualTo(calls + calls / 2));
            }
        }

        #endregion

        #region ExecuteStream (and ExecuteAsStreamAsync)

        /// <summary>A subtest for testing execute as stream (async and sync).</summary>
        private void SubtestExecuteAsStream(bool async)
        {
            var handler = new TestBodyMessageHandler
            {
                GZipEnabled = false,
                ResponseObject = new MockResponse { Id = 100, Name = "sample name" },
                ExpectedRequestObject = new MockRequest { Name = "long long name" }
            };

            var initializer = new BaseClientService.Initializer
            {
                GZipEnabled = false,
                HttpClientFactory = new MockHttpClientFactory(handler)
            };

            using (var service = new MockClientService(initializer))
            {
                handler.Serializer = service.Serializer;

                var request = new TestClientServiceRequest(service, "GET", handler.ExpectedRequestObject);

                MockResponse response = null;
                Stream stream = null;
                if (async)
                {
                    request.ExecuteAsStreamAsync().ContinueWith(t =>
                    {
                        stream = t.Result;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion).Wait();
                }
                else
                {
                    stream = request.ExecuteAsStream();
                }

                // Read the object.
                var str = ExtractStringFromStream(stream);
                response = service.Serializer.Deserialize<MockResponse>(str);

                Assert.That(handler.Calls, Is.EqualTo(1));
                Assert.That(response, Is.EqualTo(handler.ResponseObject));
            }
        }

        /// <summary>Tests execute stream.</summary>
        [Test]
        public void ExecuteAsStream()
        {
            SubtestExecuteAsStream(false);
        }

        /// <summary>Tests execute stream (async).</summary>
        [Test]
        public void ExecuteAsStreamAsync()
        {
            SubtestExecuteAsStream(true);
        }

        #endregion

        #region Parameters

        #region Query Parameters

        /// <summary>Client request which contains query parameters.</summary>
        class ClientServiceRequestWithQueryParameters : TestClientServiceRequest
        {
            [RequestParameterAttribute("required", Google.Apis.Util.RequestParameterType.Query)]
            public string Required { get; set; }

            [RequestParameterAttribute("optionalWithValue", Google.Apis.Util.RequestParameterType.Query)]
            public string OptionalWithValue { get; set; }

            [RequestParameterAttribute("optionalWithValue2", Google.Apis.Util.RequestParameterType.Query)]
            public string OptionalWithValue2 { get; set; }

            [RequestParameterAttribute("optionalNull", Google.Apis.Util.RequestParameterType.Query)]
            public string OptionalNull { get; set; }

            [RequestParameterAttribute("optionalEmpty", Google.Apis.Util.RequestParameterType.Query)]
            public string OptionalEmpty { get; set; }

            [RequestParameterAttribute("optionalNotPressent", Google.Apis.Util.RequestParameterType.Query)]
            public string OptionalNotPressent { get; set; }

            public ClientServiceRequestWithQueryParameters(IClientService service, string method, object body)
                : base(service, method, body)
            {
                RequestParameters.Add("required", new Parameter
                {
                    Name = "required",
                    IsRequired = true,
                    ParameterType = "query"
                });
                RequestParameters.Add("optionalWithValue", new Parameter
                {
                    Name = "optionalWithValue",
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = "DoesNotDisplay"
                });
                RequestParameters.Add("optionalWithValue2", new Parameter
                {
                    Name = "optionalWithValue",
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = "DoesNotDisplay"
                });
                RequestParameters.Add("optionalWithNull", new Parameter
                {
                    Name = "optionalWithNull",
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = "c"
                });
                RequestParameters.Add("optionalEmpty", new Parameter
                {
                    Name = "optionalEmpty",
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = "d"
                });
                RequestParameters.Add("optionalNotPressent", new Parameter
                {
                    Name = "optionalNotPressent",
                    IsRequired = false,
                    ParameterType = "query",
                    DefaultValue = "DoesNotDisplay"
                });
            }
        }

        /// <summary>Tests build request with query parameters.</summary>
        [Test]
        public void CreateRequest_QueryParameters()
        {
            using (var service = new MockClientService("https://build_request_params"))
            {
                var request = new ClientServiceRequestWithQueryParameters(service, "GET", null);
                request.Required = "a";
                request.OptionalWithValue = "b";
                request.OptionalWithValue2 = "DoesNotDisplay";
                request.OptionalNull = null;
                request.OptionalEmpty = string.Empty;
                var httpRequest = request.CreateRequest();
                Assert.That(httpRequest.RequestUri, Is.EqualTo(new Uri(
                    "https://build_request_params/restPath0?required=a&optionalWithValue=b&optionalEmpty")));
            }
        }

        /// <summary>Tests build request with missing required query parameter.</summary>
        [Test]
        public void CreateRequest_QueryParameterIsMissing()
        {
            using (var service = new MockClientService("https://build_request_params"))
            {
                var request = new ClientServiceRequestWithQueryParameters(service, "GET", null);
                // request.Required is missing!
                try
                {
                    var httpRequest = request.CreateRequest();
                    Assert.Fail();
                }
                catch (GoogleApiException ex)
                {
                    Assert.True(ex.Message.Contains("Parameter \"required\" is missing"),
                        "Exception with missing parameter should be thrown");
                }
            }
        }

        #endregion

        #region Path Parameters

        /// <summary>Client request which contains path parameters.</summary>
        class ClientServiceRequestWithPathParameters : TestClientServiceRequest
        {
            [RequestParameter("path1", RequestParameterType.Path)]
            public int TestParameterA { get; set; }

            [RequestParameter("path2", RequestParameterType.Path)]
            public int TestParameterB { get; set; }

            public int TestParameterC { get; set; }

            public ClientServiceRequestWithPathParameters(IClientService service, string method, object body)
                : base(service, method, body)
            {
                RequestParameters.Add("path1", new Parameter
                {
                    Name = "path1",
                    ParameterType = "path"
                });
                RequestParameters.Add("path2", new Parameter
                {
                    Name = "path2",
                    ParameterType = "path",
                });
            }

            public override string RestPath
            {
                get { return "restPath/{path1}/something/{path2}"; }
            }
        }

        /// <summary>Tests build request with path parameters.</summary>
        [Test]
        public void CreateRequest_PathParameters()
        {
            using (var service = new MockClientService("https://build_request_params"))
            {
                var request = new ClientServiceRequestWithPathParameters(service, "GET", null);
                request.TestParameterA = 42;
                request.TestParameterB = 43;
                request.TestParameterC = 44; // ignore it, because it doesn't contain RequestParameter attribute
                var httpRequest = request.CreateRequest();
                Assert.That(httpRequest.RequestUri, Is.EqualTo(new Uri(
                    "https://build_request_params/restPath/42/something/43")));
            }
        }

        #endregion

        #region Developer Key

        private const string SimpleDeveloperKey = "ABC123";
        private const string ComplexDeveloperKey = "?&^%  ABC123";

        /// <summary>Tests build request with simple developer key.</summary>
        [Test]
        public void CreateRequest_DeveloperKey()
        {
            var initializer = new BaseClientService.Initializer
            {
                ApiKey = SimpleDeveloperKey
            };

            using (var service = new MockClientService(initializer, "https://build_request_params"))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                var httpRequest = request.CreateRequest();
                Assert.That(httpRequest.RequestUri, Is.EqualTo(new Uri(
                    "https://build_request_params/restPath0?key=" + SimpleDeveloperKey)));
            }
        }

        /// <summary>Tests build request with complex developer key.</summary>
        [Test]
        public void CreateRequest_DeveloperKey_RequiresEscape()
        {
            var initializer = new BaseClientService.Initializer
            {
                ApiKey = ComplexDeveloperKey
            };

            using (var service = new MockClientService(initializer, "https://build_request_params"))
            {
                var request = new TestClientServiceRequest(service, "GET", null);
                var httpRequest = request.CreateRequest();
                Assert.That(httpRequest.RequestUri, Is.EqualTo(new Uri(
                    "https://build_request_params/restPath0?key=%3F%26%5E%25%20%20ABC123")));
            }
        }

        #endregion

        #endregion

        #region Supported Methods

        /// <summary>Tests if invalid method throws an exception.</summary>
        [Test]
        public void CreateRequest_UnsupportedMethods()
        {
            using (var service = new MockClientService("https://build_request_params"))
            {
                var request = new TestClientServiceRequest(service, "Unsupported", null);
                Assert.Throws<ArgumentOutOfRangeException>(() => request.CreateRequest());
            }
        }

        /// <summary>Tests that valid method doesn't throw an exception.</summary>
        [Test]
        public void CreateRequest_SupportedMethods()
        {
            SubtestCreateRequest_SupportedMethod(HttpConsts.Get);
            SubtestCreateRequest_SupportedMethod(HttpConsts.Put);
            SubtestCreateRequest_SupportedMethod(HttpConsts.Post);
            SubtestCreateRequest_SupportedMethod(HttpConsts.Patch);
            SubtestCreateRequest_SupportedMethod(HttpConsts.Delete);
        }

        private void SubtestCreateRequest_SupportedMethod(string method)
        {
            using (var service = new MockClientService("https://build_request_params"))
            {
                var request = new TestClientServiceRequest(service, method, null);
                var httpRequest = request.CreateRequest();
            }
        }

        #endregion

        #region ETag

        /// <summary>Tests the create request method with different ETags.</summary>
        [Test]
        public void CreateRequest_ETag()
        {
            var body = new MockRequest { Name = "long long name" };
            using (var service = new MockClientService())
            {
                // No ETag (ETag = null).
                var request = new TestClientServiceRequest(service, HttpConsts.Get, body);
                var httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(0));

                // ETag has a value, but ETag action is ignored.
                body.ETag = "\"ETAG_HERE\"";
                request = new TestClientServiceRequest(service, HttpConsts.Get, body);
                request.ETagAction = ETagAction.Ignore;
                httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(0));

                // ETag has a value, so use default action (Get -> If-None-Match).
                request = new TestClientServiceRequest(service, HttpConsts.Get, body);
                httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(1));
                Assert.That(httpRequest.Headers.IfNoneMatch.First(), Is.EqualTo(new EntityTagHeaderValue(body.ETag)));

                // ETag has a value, so use default action (Post -> If-Match).
                request = new TestClientServiceRequest(service, HttpConsts.Post, body);
                httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(1));
                Assert.That(httpRequest.Headers.IfMatch.First(), Is.EqualTo(new EntityTagHeaderValue(body.ETag)));

                // ETag has a value, default is override, use the specified ETag action.
                request = new TestClientServiceRequest(service, HttpConsts.Post, body);
                request.ETagAction = ETagAction.IfNoneMatch;
                httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(1));
                Assert.That(httpRequest.Headers.IfNoneMatch.First(), Is.EqualTo(new EntityTagHeaderValue(body.ETag)));

                // ETag has a value, default is override, use the specified ETag action.
                request = new TestClientServiceRequest(service, HttpConsts.Get, body);
                request.ETagAction = ETagAction.IfMatch;
                httpRequest = request.CreateRequest();
                Assert.That(httpRequest.Headers.IfNoneMatch.Count, Is.EqualTo(0));
                Assert.That(httpRequest.Headers.IfMatch.Count, Is.EqualTo(1));
                Assert.That(httpRequest.Headers.IfMatch.First(), Is.EqualTo(new EntityTagHeaderValue(body.ETag)));
            }
        }

        /// <summary>Tests that get default ETag action works as expected.</summary>
        [Test]
        public void GetDefaultETagActionTest()
        {
            Assert.AreEqual(ETagAction.IfNoneMatch, ClientServiceRequest<object>.GetDefaultETagAction(HttpConsts.Get));
            Assert.AreEqual(ETagAction.IfMatch, ClientServiceRequest<object>.GetDefaultETagAction(HttpConsts.Post));
            Assert.AreEqual(ETagAction.IfMatch, ClientServiceRequest<object>.GetDefaultETagAction(HttpConsts.Patch));
            Assert.AreEqual(ETagAction.IfMatch, ClientServiceRequest<object>.GetDefaultETagAction(HttpConsts.Put));
            Assert.AreEqual(ETagAction.IfMatch, ClientServiceRequest<object>.GetDefaultETagAction(HttpConsts.Delete));
            Assert.AreEqual(ETagAction.Ignore, ClientServiceRequest<object>.GetDefaultETagAction("INVALID"));
        }

        #endregion
    }
}