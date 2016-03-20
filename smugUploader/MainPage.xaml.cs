using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using smugUploader.OAuth;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace smugUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private smugAuthorization smug;

        public MainPage()
        {
            this.InitializeComponent();
            smug = new smugAuthorization("xxGn8kaV0HfrQ1m8fvrtKYQFcqnAQUvl", "28eb860ff20d1884ec54e5cb9c3dad9a");
        }

        private static async Task<string> GetResponseContent(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await (await client.GetAsync(url)).Content.ReadAsStringAsync();
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

            bool connected = await smug.connectToSmugMug();
            if (connected)
            {
                textBox.Text = "Connected to SmugMug";
            }
            else
            {
                textBox.Text = "Not connected to SmugMug";
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            textBox.Text = e.Parameter as string;
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            //Read in values in vault.

            var vault = new Windows.Security.Credentials.PasswordVault();
            var credential = vault.Retrieve("OAuthToken", "SmugMug");

            string[] auths = credential.Password.Split('|');

            OAuthToken token = new OAuthToken(auths[0], auths[1], auths[2], auths[3]);

            Debug.WriteLine(auths[0]);
            Debug.WriteLine(auths[1]);
            Debug.WriteLine(auths[2]);
            Debug.WriteLine(auths[3]);

            OAuthMessageHandler handler = new OAuthMessageHandler(
                token.ApiKey,
                token.Secret,
                token.Token,
                token.TokenSecret);
            HttpClient client = new HttpClient(handler);

            // Make sure we request JSON data
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            string uri = @"https://api.smugmug.com/api/v2!authuser";
            //handler.GetAuthenticationHeaderForRequest(new Uri(uri), HttpMethod.Get);
            HttpResponseMessage response = await client.GetAsync(uri);
            textBlock.Text = await response.Content.ReadAsStringAsync();
            //var credentialList = vault.FindAllByResource("OAuthToken");
            //if (credentialList.Count > 0)
            //{
            //    if (credentialList.Count == 1)
            //    {
            //        credential = credentialList[0];
            //    }
            //    else
            //    {
            //        // When there are multiple usernames,
            //        // retrieve the default username. If one doesn’t
            //        // exist, then display UI to have the user select
            //        // a default username.

            //        defaultUserName = GetDefaultUserNameUI();

            //        credential = vault.Retrieve(resourceName, defaultUserName);
            //    }
            //}
        }
    }
}
