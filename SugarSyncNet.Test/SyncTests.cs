// <copyright file="SyncTests.cs" company="BerlinSoft">
// BerlinSoft All rights reserved.
// </copyright>
// <author>Wernfried Schwenkner</author>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SugarSyncNet.Models;

namespace SugarSyncNet.Test
{
    /// <summary>
    /// Implements the synchronous tests for SyncTests
    /// </summary>
    [TestClass]
    public class SyncTests
    {
        /// <summary>
        /// SugarSync client used for test
        /// </summary>
        private static Client client;

        /// <summary>
        /// The test context
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncTests"/> class.
        /// </summary>
        public SyncTests()
        {
            // TODO: Konstruktorlogik hier hinzufügen
        }

        /// <summary>
        /// Gets or sets the test context with informations about
        /// test execution and functionality
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Zusätzliche Testattribute
        /// <summary>
        /// Initializes the test before all tests are executed
        /// </summary>
        /// <param name="testContext">The test context</param>
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            client = new Client(TestSettings.AccessKeyId, TestSettings.PrivateAccessKey, TestSettings.ApplicationId);
        }

        /// <summary>
        /// Cleans up test after all tests have been executed
        /// </summary>
        [ClassCleanup]
        public static void MyClassCleanup()
        {
        }

        /// <summary>
        /// Initializes before each test
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
        }

        /// <summary>
        /// Cleans up after every test
        /// </summary>
        [TestCleanup]
        public void MyTestCleanup()
        {
            HttpWebResponse response;
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            CollectionContents sugarSyncCollectionContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            foreach (Collection collection in sugarSyncCollectionContents.Collections.Where(c => c.DisplayName == TestSettings.SugarSyncTestFolderNameSyncTests))
            {
                CollectionContents innerContents = client.Get<CollectionContents>(collection.Contents);
                foreach (Collection innerColection in innerContents.Collections)
                {
                    response = client.Delete(innerColection.Reference);
                }

                foreach (File innerFile in innerContents.Files)
                {
                    response = client.Delete(innerFile.Reference);
                }

                response = client.Delete(collection.Reference);
            }
        }

        #endregion

        /// <summary>
        /// Test for authentication with name and password
        /// </summary>
        [TestMethod]
        public void AuthenticateWithNameAndPassword()
        {
            UserLogin userLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            Assert.IsNotNull(userLogin, "No user login returned");
            Assert.IsFalse(string.IsNullOrEmpty(userLogin.RefreshToken), "No refresh token got");
        }

        /// <summary>
        /// Test for getting an access token
        /// </summary>
        [TestMethod]
        public void GetAccessToken()
        {
            UserLogin userLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            string accessToken = client.GetAccessToken(userLogin.RefreshToken);
            Assert.IsFalse(string.IsNullOrEmpty(accessToken), "No access token got");
        }

        /// <summary>
        /// Method to test getting the user from SugarSync
        /// </summary>
        [TestMethod]
        public void User()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            Assert.IsNotNull(user, "No user returned");
            Assert.AreEqual(TestSettings.UserName, user.UserName, "Wrong user name returned");
        }

        /// <summary>
        /// Test to get a folder presentation
        /// </summary>
        [TestMethod]
        public void GetFoldersPresentation()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 4, "At least the default folders shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("syncFolder", collection.Type, "Wrong collection type");
            }
        }

        /// <summary>
        /// Test to get the workspace presentation
        /// </summary>
        [TestMethod]
        public void GetWorkspacesPresentation()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.Workspaces);
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one workspace shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("workspace", collection.Type, "Wrong collection type");
            }
        }

        /// <summary>
        /// Test to get the albums presentation
        /// </summary>
        [TestMethod]
        public void GetAlbumsPresentation()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.Albums);
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one album shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("album", collection.Type, "Wrong collection type");
            }
        }

        /// <summary>
        /// Test to get the workspace info
        /// </summary>
        [TestMethod]
        public void WorkspaceInfo()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.Workspaces);
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one workspace shall exist");
            Workspace workspace = client.Get<Workspace>(contents.Collections[0].Reference);
            Assert.IsNotNull(workspace, "Workspace info not found");
        }

        /// <summary>
        /// Test to get one folder
        /// </summary>
        [TestMethod]
        public void Folder()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one folder shall exist");
            Folder folder = client.Get<Folder>(contents.Collections[0].Reference);
            Assert.IsNotNull(folder, "Folder info not found");
            Assert.IsNull(folder.Parent, "Should not have a parent");
        }

        /// <summary>
        /// Test to create one folder
        /// </summary>
        [TestMethod]
        public void CreateFolder()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // create folder
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            Assert.IsFalse(string.IsNullOrEmpty(response.Headers["Location"]), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == response.Headers["Location"]);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");
        }

        /// <summary>
        /// Test to delete a folder
        /// </summary>
        [TestMethod]
        public void DeleteFolder()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            Assert.IsFalse(string.IsNullOrEmpty(response.Headers["Location"]), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == response.Headers["Location"]);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // delete folder
            response = client.Delete(testFolder.Reference);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Assert.IsFalse(sugarSyncContents.Collections.Any(collection => collection.DisplayName == TestSettings.SugarSyncTestFolderNameSyncTests), "Folder not deleted");
        }

        /// <summary>
        /// Test to rename a folder
        /// </summary>
        [TestMethod]
        public void RenameFolder()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // rename folder
            response = client.Put(testFolder.Reference, new FolderRequest { DisplayName = "NewFolder" });
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Assert.IsTrue(sugarSyncContents.Collections.Any(collection => collection.Reference == testFolderReference), "Folder reference not found");
            Assert.IsTrue(sugarSyncContents.Collections.Any(collection => collection.DisplayName == "NewFolder"), "Renamed folder not found");
            response = client.Put(testFolder.Reference, new FolderRequest { DisplayName = testFolderName });
        }

        /// <summary>
        /// Test to create a file not to upload data
        /// </summary>
        [TestMethod]
        public void CreateFile()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == response.Headers["Location"]);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");
        }

        /// <summary>
        /// Test to delete a file
        /// </summary>
        [TestMethod]
        public void DeleteFile()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // delete file
            response = client.Delete(fileReference);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            Assert.IsFalse(testContents.Files.Any(), "File not deleted");
        }

        /// <summary>
        /// Test to rename a file
        /// </summary>
        [TestMethod]
        public void RenameFile()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // rename file
            response = client.Put<File>(fileReference, new File { DisplayName = "NewFile.jpg" });
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.AreEqual("NewFile.jpg", file.DisplayName, "File not renamed");
        }

        /// <summary>
        /// Test to upload data onto a newly created file
        /// </summary>
        [TestMethod]
        public void Upload()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.Upload(file.Reference + "/data", stream, "text/plain");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");
        }

        /// <summary>
        /// Test to copy a file
        /// </summary>
        [TestMethod]
        public void CopyFile()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.Upload(file.Reference + "/data", stream, "text/plain");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // copy file
            string newTestFileName = "NewTestFile.txt";
            response = client.Post(testFolder.Reference, new FileCopy { SourceReference = fileReference, DisplayName = newTestFileName });
            string fileCopyReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            Assert.AreEqual(2, testContents.Files.Count, "Copied file not found");
        }

        /// <summary>
        /// Test to download a file
        /// </summary>
        [TestMethod]
        public void DownloadFile()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.Upload(file.Reference + "/data", stream, "text/plain");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // download
            System.IO.Stream resultStream = client.Download(file.FileData);
            System.IO.StreamReader reader = new System.IO.StreamReader(resultStream);
            string downloadedString = reader.ReadToEnd();
            Assert.IsFalse(string.IsNullOrEmpty(downloadedString), "No string returned");
            Assert.AreEqual(stringToUpload, downloadedString, "Different string returned");
        }

        /// <summary>
        /// Test to get file versions
        /// </summary>
        [TestMethod]
        public void FileVersion()
        {
            client.UserLogin = client.GetRefreshToken(TestSettings.UserName, TestSettings.Password);
            User user = client.Get<User>(Client.SugarSyncUrl + "user");
            CollectionContents contents = client.Get<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameSyncTests;
            HttpWebResponse response = client.Post(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.Get<CollectionContents>(sugarSyncCollection.Contents);
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.Post(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" });
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.Get<CollectionContents>(testFolder.Contents);
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.Upload(file.Reference + "/data", stream, "text/plain");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.Get<CollectionContents>(testFolder.Contents);
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // get version info
            FileVersions versions = client.Get<FileVersions>(file.Reference + "/version");
            Assert.IsNotNull(versions, "No versions info found");
            Assert.AreEqual(1, versions.Versions.Count, "Wrong number of versions found");

            // upload new version
            response = client.Upload(file.FileData, stream, "text/plain");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            versions = client.Get<FileVersions>(file.Reference + "/version");
            Assert.IsNotNull(versions, "No versions info found");
            Assert.AreEqual(2, versions.Versions.Count, "Wrong number of versions found");
        }
    }
}
