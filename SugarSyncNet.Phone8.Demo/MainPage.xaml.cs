using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SugarSyncNet.Models;
using SugarSyncNet.Phone8.Demo.Resources;

namespace SugarSyncNet.Phone8.Demo
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region
        /// <summary>
        /// the user name
        /// </summary>
        private string userName = "user name";

        /// <summary>
        /// The password
        /// </summary>
        private string password = "password";
        #endregion
        // Konstruktor

        /// <summary>
        /// The SugarSync client
        /// </summary>
        private Client client = new Client("Access key", "Private key", "Application Id");
        
        /// <summary>
        /// The user login
        /// </summary>
        private UserLogin userlogin;

        /// <summary>
        /// Name of the syncghronous test folder
        /// </summary>
        private string sugarSyncTestFolderName = "TestFolderSugarSyncPhoneTests";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle get refresh token click
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void Button_GetRefreshTokenClick(object sender, RoutedEventArgs e)
        {
            userlogin = await client.GetRefreshTokenAsync(userName, password);
            MessageBox.Show(userlogin.RefreshToken, "Refresh token", MessageBoxButton.OK);
        }

        /// <summary>
        /// Handle get access token click
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void Button_GetAccessTokenClick(object sender, RoutedEventArgs e)
        {
            string accessToken = await client.GetAccessTokenAsync(userlogin.RefreshToken);
            MessageBox.Show(accessToken, "Access token", MessageBoxButton.OK);
        }

        /// <summary>
        /// Handle get upload file click
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private async void Button_UploadFileClick(object sender, RoutedEventArgs e)
        {
            client.UserLogin = await client.GetRefreshTokenAsync(userName, password);
            User user = await client.GetAsync<User>(Client.SugarSyncUrl + "user");
            CollectionContents userContents = await client.GetAsync<CollectionContents>(user.SyncFolders);
            Collection sugarSyncCollection = userContents.Collections.FirstOrDefault(collection => collection.DisplayName == "SugarSync");
            CollectionContents sugarSyncContents = await client.GetAsync<CollectionContents>(sugarSyncCollection.Contents);
            Collection testCollection = sugarSyncContents.Collections.FirstOrDefault(collection => collection.DisplayName == sugarSyncTestFolderName);
            if (testCollection != null)
            {
                CollectionContents testContents = await client.GetAsync<CollectionContents>(testCollection.Contents);
                foreach (Collection collection in testContents.Collections)
                {
                    CollectionContents contents = await client.GetAsync<CollectionContents>(collection.Contents);
                    foreach (Collection content in contents.Collections)
                    {
                        await client.DeleteAsync(content.Reference);
                    }

                    await client.DeleteAsync(collection.Reference);
                }

                foreach (File file in testContents.Files)
                {
                    await client.DeleteAsync(file.Reference);
                }

                await client.DeleteAsync(testCollection.Reference);
            }

            string testCollectionReference = (await client.PostAsync(sugarSyncCollection.Reference, new FolderRequest { DisplayName = sugarSyncTestFolderName })).Headers["Location"];

            // contentsCollection = await client.GetAsync<CollectionContents>(userCollection.Contents);
            // Collection sugarSyncTestCollection = contentsCollection.Collections.Where(collection => collection.DisplayName == sugarSyncTestFolderName).FirstOrDefault();
            string stringToUpload = "Hello SugarSync";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(stringToUpload);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
            await client.UploadFileAsync(testCollectionReference, "testfile.txt", stream, "text/xml");
            MessageBox.Show("Upload success", "File upload", MessageBoxButton.OK);
        }
    }
}