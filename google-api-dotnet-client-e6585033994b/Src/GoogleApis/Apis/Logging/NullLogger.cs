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

namespace Google.Apis.Logging
{
    /// <summary>
    /// Represents a NullLogger which does not do any logging.
    /// </summary>
    public class NullLogger : ILogger
    {
        #region ILogger Members

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public ILogger ForType(Type type)
        {
            return new NullLogger();
        }

        public ILogger ForType<T>()
        {
            return new NullLogger();
        }

        public void Info(string message, params object[] formatArgs) {}

        public void Warning(string message, params object[] formatArgs) {}

        public void Debug(string message, params object[] formatArgs) {}

        public void Error(Exception exception, string message, params object[] formatArgs) {}

        public void Error(string message, params object[] formatArgs) {}

        #endregion
    }
}