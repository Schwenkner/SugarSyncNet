using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Net;
using SugarSyncNet.Models;

namespace SugarSyncNet.Test
{

    [TestClass]
    public class AsyncTests
    {
        private static Client client;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            client = new Client(TestSettings.AccessKeyId, TestSettings.PrivateAccessKey, TestSettings.ApplicationId);
        }

        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }

        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }

        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        [TestCleanup()]
        public void MyTestCleanup()
        {
            HttpWebResponse response;
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            CollectionContents sugarSyncCollectionContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            foreach (Collection collection in sugarSyncCollectionContents.Collections.Where(c => c.DisplayName == TestSettings.SugarSyncTestFolderNameAsyncTests))
            {
                CollectionContents innerContents = client.GetAsync<CollectionContents>(collection.Contents).Result;
                foreach (Collection innerColection in innerContents.Collections)
                {
                    response = client.DeleteAsync(innerColection.Reference).Result;
                }
                foreach (File innerFile in innerContents.Files)
                {
                    response = client.DeleteAsync(innerFile.Reference).Result;
                }
                response = client.DeleteAsync(collection.Reference).Result;
            }
        }

        [TestMethod]
        public void AuthenticateWithNameAndPasswordAsync()
        {
            UserLogin userLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            Assert.IsNotNull(userLogin, "No user login returned");
            Assert.IsFalse(string.IsNullOrEmpty(userLogin.RefreshToken), "No refresh token got");
        }

        [TestMethod]
        public void GetAccessTokenAsync()
        {
            UserLogin userLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            string accessToken = client.GetAccessTokenAsync(userLogin.RefreshToken).Result;
            Assert.IsFalse(string.IsNullOrEmpty(accessToken), "No access token got");
        }

        [TestMethod]
        public void UserAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            Assert.IsNotNull(user, "No user returned");
            Assert.AreEqual(TestSettings.UserName, user.UserName, "Wrong user name returned");
        }


        [TestMethod]
        public void GetFoldersPresentationAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 4, "At least the default folders shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("syncFolder", collection.Type, "Wrong collection type");
            }
        }


        [TestMethod]
        public void GetWorkspacesPresentationAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.Workspaces).Result;
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one workspace shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("workspace", collection.Type, "Wrong collection type");
            }
        }

        [TestMethod]
        public void GetAlbumsPresentationAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.Albums).Result;
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one album shall exist");
            foreach (Collection collection in contents.Collections)
            {
                Assert.AreEqual("album", collection.Type, "Wrong collection type");
            }
        }

        [TestMethod]
        public void WorkspaceInfoAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.Workspaces).Result;
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one workspace shall exist");
            Workspace workspace = client.GetAsync<Workspace>(contents.Collections[0].Reference).Result;
            Assert.IsNotNull(workspace, "Workspace info not found");
        }

        [TestMethod]
        public void FolderAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Assert.IsNotNull(contents, "No results");
            Assert.IsTrue(contents.Collections.Count >= 1, "At least one folder shall exist");
            Folder folder = client.GetAsync<Folder>(contents.Collections[0].Reference).Result;
            Assert.IsNotNull(folder, "Folder info not found");
            Assert.IsNull(folder.Parent, "Should not have a parent");
        }

        [TestMethod]
        public void CreateFolderAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // create folder
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            Assert.IsFalse(string.IsNullOrEmpty(response.Headers["Location"]), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == response.Headers["Location"]);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");
        }

        [TestMethod]
        public void DeleteFolderAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            Assert.IsFalse(string.IsNullOrEmpty(response.Headers["Location"]), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == response.Headers["Location"]);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // delete folder
            response = client.DeleteAsync(testFolder.Reference).Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Assert.IsFalse(sugarSyncContents.Collections.Any(collection => collection.DisplayName == TestSettings.SugarSyncTestFolderNameAsyncTests), "Folder not deleted");
        }

        [TestMethod]
        public void RenameFolderAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // rename folder
            response = client.PutAsync(testFolder.Reference, new FolderRequest { DisplayName = "NewFolder" }).Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Assert.IsTrue(sugarSyncContents.Collections.Any(collection => collection.Reference == testFolderReference), "Folder reference not found");
            Assert.IsTrue(sugarSyncContents.Collections.Any(collection => collection.DisplayName == "NewFolder"), "Renamed folder not found");
            response = client.PutAsync(testFolder.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
        }

        [TestMethod]
        public void CreateFileAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == response.Headers["Location"]);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");
        }

        [TestMethod]
        public void DeleteFileAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // delete file
            response = client.DeleteAsync(fileReference).Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            Assert.IsFalse(testContents.Files.Any(), "File not deleted");
        }

        [TestMethod]
        public void RenameFileAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.jpg";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // rename file
            response = client.PutAsync<File>(fileReference, new File { DisplayName = "NewFile.jpg" }).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.AreEqual("NewFile.jpg", file.DisplayName, "File not renamed");
        }

        [TestMethod]
        public void UploadAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.txt";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.UploadAsync(file.Reference + "/data", stream, "text/plain").Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");
        }

        [TestMethod]
        public void CopyFileAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.txt";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.UploadAsync(file.Reference + "/data", stream, "text/plain").Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // copy file
            string newTestFileName = "NewTestFile.txt";
            response = client.PostAsync(testFolder.Reference, new FileCopy { SourceReference = fileReference, DisplayName = newTestFileName }).Result;
            string fileCopyReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            Assert.AreEqual(2, testContents.Files.Count, "Copied file not found");
        }

        [TestMethod]
        public void DownloadFileAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.txt";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.UploadAsync(file.Reference + "/data", stream, "text/plain").Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // download
            System.IO.Stream resultStream = client.DownloadAsync(file.FileData).Result;
            System.IO.StreamReader reader = new System.IO.StreamReader(resultStream);
            string downloadedString = reader.ReadToEnd();
            Assert.IsFalse(string.IsNullOrEmpty(downloadedString), "No string returned");
            Assert.AreEqual(stringToUpload, downloadedString, "Different string returned");
        }

        [TestMethod]
        public void FileVersionAsync()
        {
            client.UserLogin = client.GetRefreshTokenAsync(TestSettings.UserName, TestSettings.Password).Result;
            User user = client.GetAsync<User>(Client.SugarSyncUrl + "user").Result;
            CollectionContents contents = client.GetAsync<CollectionContents>(user.SyncFolders).Result;
            Collection sugarSyncCollection = contents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            Assert.IsNotNull(sugarSyncCollection, "SugarSync folder not found");

            // prepare
            string testFolderName = TestSettings.SugarSyncTestFolderNameAsyncTests;
            HttpWebResponse response = client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = testFolderName }).Result;
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            string testFolderReference = response.Headers["Location"];
            Assert.IsFalse(string.IsNullOrEmpty(testFolderReference), "No reference returned");
            CollectionContents sugarSyncContents = client.GetAsync<CollectionContents>(sugarSyncCollection.Contents).Result;
            Collection testFolder = sugarSyncContents.Collections.FirstOrDefault(collection => collection.Reference == testFolderReference);
            Assert.IsNotNull(testFolder, "test folder not found");
            Assert.AreEqual(testFolderName, testFolder.DisplayName, "Name does not match");

            // create file
            string testFileName = "TestFile.txt";
            response = client.PostAsync(testFolder.Reference, new File { DisplayName = testFileName, MediaType = "image/jpeg" }).Result;
            string fileReference = response.Headers["Location"];
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Wrong status");
            CollectionContents testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            File file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsNotNull(file, "File not foind");
            Assert.IsFalse(file.PresentOnServer, "File not temporary");

            // upload data
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            response = client.UploadAsync(file.Reference + "/data", stream, "text/plain").Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            testContents = client.GetAsync<CollectionContents>(testFolder.Contents).Result;
            file = testContents.Files.FirstOrDefault(f => f.Reference == fileReference);
            Assert.IsTrue(file.PresentOnServer, "File is still temporary");

            // get version info
            FileVersions versions = client.GetAsync<FileVersions>(file.Reference + "/version").Result;
            Assert.IsNotNull(versions, "No versions info found");
            Assert.AreEqual(1, versions.Versions.Count, "Wrong number of versions found");

            // upload new version
            response = client.UploadAsync(file.FileData, stream, "text/plain").Result;
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Wrong status");
            versions = client.GetAsync<FileVersions>(file.Reference + "/version").Result;
            Assert.IsNotNull(versions, "No versions info found");
            Assert.AreEqual(2, versions.Versions.Count, "Wrong number of versions found");
        }
    }
}
