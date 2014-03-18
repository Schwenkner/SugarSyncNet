/*
Copyright 2010 Google Inc

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

using NUnit.Framework;

using Google.Apis.Discovery;
using Google.Apis.Requests.Parameters;

namespace Google.Apis.Tests.Apis.Requests
{
    /// <summary>Tests <seealso cref="Google.Apis.Requests.Parameters.ParameterValidator"/>.</summary>
    [TestFixture]
    public class ParameterValidatorTest
    {
        /// <summary>Tests that validate regex returns <c>false</c> on empty parameter sets.</summary>
        [Test]
        public void ValidateRegexEmptyNeedsDataTest()
        {
            var parameter = new Parameter { Pattern = ".+", Name = "test" };
            Assert.IsFalse(ParameterValidator.ValidateRegex(parameter, ""));
        }

        /// <summary>Tests validate regex.</summary>
        [Test]
        public void ValidateRegexTest()
        {
            var parameter = new Parameter { Pattern = ".+", Name = "test" };
            Assert.IsTrue(ParameterValidator.ValidateRegex(parameter, "Test"));
        }
    }
}