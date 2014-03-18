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

using NUnit.Framework;

using Google.Apis.Logging;

namespace Google.Apis.Tests
{
    /// <summary> Tests for <see cref="Google.ApplicationContext"/>. </summary>
    [TestFixture]
    public class ApplicationContextTests
    {
        private class MockLogger : ILogger
        {
            public bool IsDebugEnabled
            {
                get { throw new NotImplementedException(); }
            }

            public ILogger ForType(Type type)
            {
                throw new NotImplementedException();
            }

            public ILogger ForType<T>()
            {
                throw new NotImplementedException();
            }

            public void Info(string message, params object[] formatArgs)
            {
                throw new NotImplementedException();
            }

            public void Warning(string message, params object[] formatArgs)
            {
                throw new NotImplementedException();
            }

            public void Debug(string message, params object[] formatArgs)
            {
                throw new NotImplementedException();
            }

            public void Error(Exception exception, string message, params object[] formatArgs)
            {
                throw new NotImplementedException();
            }

            public void Error(string message, params object[] formatArgs)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Checks the registered default logger.
        /// </summary>
        [Test]
        public void GetLoggerDefaultTest()
        {
            Assert.IsInstanceOf<NullLogger>(ApplicationContext.Logger);
        }

        /// <summary>
        /// Confirms that the RegisterLogger method will not fail when no logger was previously registered.
        /// </summary>
        [Test]
        public void RegisterLoggerTest()
        {
            // Initialize with a NullLogger:
            Assert.IsInstanceOf<NullLogger>(ApplicationContext.Logger);

            // Register a new logger:
            Assert.DoesNotThrow(() => ApplicationContext.RegisterLogger(new MockLogger()));

            // Fail test:
            Assert.Throws<InvalidOperationException>(() => ApplicationContext.RegisterLogger(new MockLogger()));

            // change back to default logger
            ApplicationContext.logger = null;
        }
    }
}
