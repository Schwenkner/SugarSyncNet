// <copyright file="TestSettings.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SugarSyncNet.Test
{
    /// <summary>
    /// Implements the <see cref="TestSettings"/> class
    /// </summary>
    public static class TestSettings
    {
        /// <summary>
        /// access key if
        /// </summary>
        private static string accessKeyId = "SugarSync acces key id";

        /// <summary>
        /// private access key
        /// </summary>
        private static string privateAccessKey = "SugarSync private access key";

        /// <summary>
        /// Application id
        /// </summary>
        private static string applicationId = "SugarSync application id";

        /// <summary>
        /// Login user name
        /// </summary>
        private static string userName = "SugareSync login name";

        /// <summary>
        /// Login password
        /// </summary>
        private static string password = "SugarSync password";

        /// <summary>
        /// Name of the test folder for synchronous tests
        /// </summary>
        private static string sugarSyncTestFolderNameSyncTests = "TestFolderSugarSyncSyncTests";

        /// <summary>
        /// Name of the folder for asynchronous tests
        /// </summary>
        private static string sugarSyncTestFolderNameAsyncTests = "TestFolderSugarSyncAsyncTests";

        /// <summary>
        /// Gets the access key id
        /// </summary>
        public static string AccessKeyId
        {
            get { return accessKeyId; }
        }

        /// <summary>
        /// Gets the private access key
        /// </summary>
        public static string PrivateAccessKey
        {
            get { return privateAccessKey; }
        }

        /// <summary>
        /// Gets the application id
        /// </summary>
        public static string ApplicationId
        {
            get { return applicationId; }
        }

        /// <summary>
        /// Gets the user name
        /// </summary>
        public static string UserName
        {
            get { return userName; }
        }

        /// <summary>
        /// Gets the user password
        /// </summary>
        public static string Password
        {
            get { return password; }
        }

        /// <summary>
        /// Gets the SugarSync test folder name for synchronous tests
        /// </summary>
        public static string SugarSyncTestFolderNameSyncTests
        {
            get { return sugarSyncTestFolderNameSyncTests; }
        }

        /// <summary>
        /// Gets the SugarSync test folder name for asynchronous tests
        /// </summary>
        public static string SugarSyncTestFolderNameAsyncTests
        {
            get { return sugarSyncTestFolderNameAsyncTests; }
        }
    }
}
