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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace smugUploader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
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
            const string OAuthBaseUrl = "https://secure.smugmug.com";
            const string OAuthGetRequestTokenUrl = OAuthBaseUrl + "/services/oauth/1.0a/getRequestToken";
            const string OAuthAuthorizeUrl = OAuthBaseUrl + "/services/oauth/1.0a/authorize";
            const string OAuthGetAccessTokenUrl = OAuthBaseUrl + "/services/oauth/1.0a/getAccessToken";
            const string NonWebOAuthApplicationCallback = "oob";

            smugUploader.OAuthAuthenticator authenticator = new smugUploader.OAuthAuthenticator("xxGn8kaV0HfrQ1m8fvrtKYQFcqnAQUvl", "28eb860ff20d1884ec54e5cb9c3dad9a");
            string reqTokUrl = authenticator.CreateGetRequestTokenAddress(OAuthGetRequestTokenUrl, HttpMethod.Get.ToString(), NonWebOAuthApplicationCallback);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(reqTokUrl);
            if (response.IsSuccessStatusCode)
            {
                textBox.Text = await response.Content.ReadAsStringAsync();
            }

            //string tokens = GetResponseContent(reqTokUrl).Result;
            // Figure out which authorization options are requested (if any)
            //string authorizationOptions = options == null ? string.Empty : "?" + options.AsQueryString();
            //string authorizeUrl = authenticator.CreateAuthorizeAddress(OAuthAuthorizeUrl, tokens);
            

            //var uri = new Uri(@authorizeUrl);

            // Launch the URI
            //var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            //if (success)
            //{
            //    textBox.Text = reqTokUrl;
            //}
            //else
            //{
            //    textBox.Text = "ohhhh";
            //}
            //string tokens = GetResponseContent(reqTokUrl).Result;
            //OAuth.OAuth smugOauth = new OAuth.OAuth("xxGn8kaV0HfrQ1m8fvrtKYQFcqnAQUvl", "28eb860ff20d1884ec54e5cb9c3dad9a");
            //string normalized = smugOauth.GetAuthTokens();

        }
    }
}
