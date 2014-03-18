#define singleThreaded
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SugarSyncNet.Models;
using System.Threading;
using System.Net;

namespace SugarSyncNet.Test
{
#if singleThreaded
	class SingleThreadedAutoResetEvent
	{
		private bool wait = true;

		public void Set()
		{
			wait = false;
		}

		public bool WaitOne(TimeSpan timeToWait)
		{
			DateTime start = DateTime.Now;
			while (wait && DateTime.Now < start + timeToWait)
			{
				Thread.Sleep(10);
			}

			bool res = !wait;
			wait = true;
			return res;
		}
	}
#endif

	[TestClass]
	public class AsyncTests
	{
		private static Client client;
		private static User user;
		private static Collection sugarSyncTestCollection;

		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			client = new Client(TestSettings.AccessKeyId, TestSettings.PrivateAccessKey);
		}

		// Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
		[TestInitialize()]
		public void MyTestInitialize()
		{
			EnsureTestCollectionExists();
		}

		// Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
		[TestCleanup()]
		public void MyTestCleanup()
		{
			CleanupTestCollection();
		}

		private void EnsureTestCollectionExists()
		{
			if (user == null)
			{
				UserAsync();
			}
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			SugarSyncException exception;
			CollectionContents contentsCollection = null;
			client.Get<CollectionContents>(
				user.SyncFolders,
				success => { contentsCollection = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contentsCollection, "No results");

			Collection userCollection = contentsCollection.Collections[4];
			client.Get<CollectionContents>(
				userCollection.Contents,
				success => { contentsCollection = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			if (contentsCollection.Collections.Count(collection => collection.DisplayName == TestSettings.SugarSyncTestFolderNameAsyncTests) == 0)
			{
				client.Post(
					userCollection.Reference,
					new FolderRequest { DisplayName = TestSettings.SugarSyncTestFolderNameAsyncTests },
					success => { completion.Set(); },
					failure => { exception = failure; completion.Set(); });
				Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			}
			client.Get<CollectionContents>(
				userCollection.Contents,
				success => { contentsCollection = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			sugarSyncTestCollection = contentsCollection.Collections.Where(collection => collection.DisplayName == TestSettings.SugarSyncTestFolderNameAsyncTests).FirstOrDefault();
			Assert.IsNotNull(sugarSyncTestCollection, "Test folder not existing");
		}

		private void CleanupTestCollection()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			SugarSyncException exception;
			CollectionContents contentsCollection = null;
			client.Get<CollectionContents>(
				user.SyncFolders,
				success => { contentsCollection = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contentsCollection, "No results");

			Collection userCollection = contentsCollection.Collections[4];
			client.Get<CollectionContents>(
				userCollection.Contents,
				success => { contentsCollection = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			Collection testCollectionToCleanup = contentsCollection.Collections.Where(collection => collection.DisplayName == TestSettings.SugarSyncTestFolderNameAsyncTests).FirstOrDefault();
			if (testCollectionToCleanup != null)
			{
				client.Delete(
					testCollectionToCleanup.Reference,
					success => { completion.Set(); },
					failure => { exception = failure; completion.Set(); });
			}
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void GetAuthQuestAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			string authQuest = string.Empty;
			SugarSyncException exception;
			client.GetAuthQuestAsync(
				TestSettings.UserName,
				TestSettings.Password,
				success => { authQuest = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsFalse(string.IsNullOrEmpty(authQuest), "No auth quest returned");
		}

		[TestMethod]
		public void UserAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			GetAuthQuestAsync();
			user = null;
			SugarSyncException exception;
			client.Get<User>(
				Client.SugarSyncUrl + "user",
				success => { user = success;  completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(user, "No user returned");
			Assert.AreEqual(TestSettings.UserName, user.UserName, "Wrong user name returned");
		}

		[TestMethod]
		public void GetFoldersPresentationAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			client.Get<CollectionContents>(
				user.SyncFolders,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
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
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			client.Get<CollectionContents>(
				user.Workspaces,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
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
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			client.Get<CollectionContents>(
				user.Albums,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
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
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			client.Get<CollectionContents>(
				user.Workspaces,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents, "No results");
			Assert.IsTrue(contents.Collections.Count >= 1, "At least one album shall exist");

			Workspace workspace = null;
			client.Get<Workspace>(
				contents.Collections[0].Reference,
				success => { workspace = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(workspace, "Workspace info not found");
		}

		[TestMethod]
		public void FolderAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			client.Get<CollectionContents>(
				user.SyncFolders,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents, "No results");
			Assert.IsTrue(contents.Collections.Count >= 1, "At least one folder shall exist");

			Folder folder = null;
			client.Get<Folder>(
				contents.Collections[0].Reference,
				success => { folder = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(folder, "Folder info not found");
			Assert.IsNull(folder.Parent, "Should not have a parent");
		}

		[TestMethod]
		public void CreateFolderAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFolderName = "TestFolder";

			client.Post(
				sugarSyncTestCollection.Reference,
				new FolderRequest { DisplayName = testFolderName },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault(), "Test folder not created");

			client.Delete(
				contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault().Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void DeleteFolderAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFolderName = "TestFolder";

			client.Post(
				sugarSyncTestCollection.Reference,
				new FolderRequest { DisplayName = testFolderName },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault(), "Test folder not created");

			client.Delete(
				contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault().Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNull(contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault(), "Test folder not deleted");
		}

		[TestMethod]
		public void RenameFolderAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFolderName = "TestFolder";
			string newFolderName = "NewFolder";

			client.Post(
				sugarSyncTestCollection.Reference,
				new FolderRequest { DisplayName = testFolderName },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault(), "Test folder not created");

			client.Put(
				contents.Collections.Where(collection => collection.DisplayName == testFolderName).FirstOrDefault().Reference,
				new FolderRequest { DisplayName = "NewFolder" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Collections.Where(collection => collection.DisplayName == newFolderName).FirstOrDefault(), "Test folder not renamed");

			client.Delete(
				contents.Collections.Where(collection => collection.DisplayName == newFolderName).FirstOrDefault().Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void CreateFileAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.jpg";

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "image/jpeg" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Delete(
				contents.Files[0].Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void DeleteFileAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.jpg";

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "image/jpeg" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Delete(
				contents.Files[0].Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.AreEqual(0, contents.Files.Count, "File not deleted");
		}

		[TestMethod]
		public void RenameFileAsync()
		{
#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.jpg";
			string newTestFileName = "NewFile.tst";

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "image/jpeg" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Put(
				contents.Files[0].Reference,
				new File { DisplayName = newTestFileName },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(newTestFileName, contents.Files[0].DisplayName, "Test file not renamed");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Delete(
				contents.Files[0].Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void UploadAsync()
		{
			string stringToUpload = "Hello SugarSync";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);

#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.txt";
			HttpStatusCode statusCode;

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "text/xml" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Upload(
				contents.Files[0].Reference + "/data",
				stream,
				"text/xml",
				success => { statusCode = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Delete(
				contents.Files[0].Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void CopyFileAsync()
		{
			string stringToUpload = "Hello SugarSync";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);

#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.txt";
			string newTestFileName = "NewFile.txt";
			HttpStatusCode statusCode;

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "text/xml" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Upload(
				contents.Files[0].Reference + "/data",
				stream,
				"text/xml",
				success => { statusCode = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Post(
				sugarSyncTestCollection.Reference,
				new CopyFile { SourceReference = contents.Files[0].Reference, DisplayName = newTestFileName },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.AreEqual(1, contents.Files.Count(file => file.DisplayName == newTestFileName), "Copied file not found");
			foreach (File file in contents.Files)
			{
				client.Delete(
					file.Reference,
					success => { completion.Set(); },
					failure => { exception = failure; completion.Set(); });
				Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			}
		}

		[TestMethod]
		public void DownloadFileAsync()
		{
			string stringToUpload = "Hello SugarSync";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);

#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.txt";
			HttpStatusCode statusCode;

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "text/xml" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Upload(
				contents.Files[0].Reference + "/data",
				stream,
				"text/xml",
				success => { statusCode = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.IsNotNull(contents.Files[0].FileData, "No file data");

			System.IO.Stream resultStream = null;
			client.Download(
				contents.Files[0].FileData,
				success => { resultStream = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(resultStream, "No stream returned");

			System.IO.StreamReader reader = new System.IO.StreamReader(resultStream);
			string downloadedString = reader.ReadToEnd();
			Assert.IsFalse(string.IsNullOrEmpty(downloadedString), "No string returned");
			Assert.AreEqual(stringToUpload, downloadedString, "Different string returned");

			client.Delete(
				contents.Files[0].Reference,
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}

		[TestMethod]
		public void FileVersionAsync()
		{
			string stringToUpload = "Hello SugarSync";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);

#if singleThreaded
			SingleThreadedAutoResetEvent completion = new SingleThreadedAutoResetEvent();
#else
			AutoResetEvent completion = new AutoResetEvent(false);
#endif
			CollectionContents contents = null;
			SugarSyncException exception;
			string testFileName = "TestFile.txt";
			HttpStatusCode statusCode;

			client.Post(
				sugarSyncTestCollection.Reference,
				new File { DisplayName = testFileName, MediaType = "text/xml" },
				success => { completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			client.Get<CollectionContents>(
				sugarSyncTestCollection.Contents,
				success => { contents = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(contents.Files, "No files collection found");
			Assert.AreEqual(1, contents.Files.Count, "No file found");
			Assert.AreEqual(testFileName, contents.Files[0].DisplayName, "Test file not found");
			Assert.AreEqual(false, contents.Files[0].PresentOnServer, "File not temporary");

			client.Upload(
				contents.Files[0].Reference + "/data",
				stream,
				"text/xml",
				success => { statusCode = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");

			FileVersions versions = null;
			client.Get<FileVersions>(
				contents.Files[0].Reference + "/version",
				success => { versions = success; completion.Set(); },
				failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
			Assert.IsNotNull(versions, "No versions info found");
			Assert.AreEqual(1, versions.Versions.Count, "Wrong number of versions found");

			client.Delete(
			contents.Files[0].Reference,
			success => { completion.Set(); },
			failure => { exception = failure; completion.Set(); });
			Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(30)), "Timed out");
		}
	}
}
