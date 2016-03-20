using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using System.Diagnostics;
using Windows.Security.Credentials;

namespace smugUploader.OAuth
{
    class smugAuthorization
    {
        private readonly string _apiKey;
        private readonly string _secret;
        private string _tokens;
        private OAuthAuthenticator authenticator;

        //Define smugmug constants
        const string OAuthBaseUrl = "https://secure.smugmug.com";
        const string OAuthGetRequestTokenUrl = OAuthBaseUrl + "/services/oauth/1.0a/getRequestToken";
        const string OAuthAuthorizeUrl = OAuthBaseUrl + "/services/oauth/1.0a/authorize";
        const string OAuthGetAccessTokenUrl = OAuthBaseUrl + "/services/oauth/1.0a/getAccessToken";
        const string NonWebOAuthApplicationCallback = "oob";
        const string FullAccess = "?Access=Full&Permissions=Modify";
        const string CallbackUri = "http://callback.smugUploadr.com";



        public smugAuthorization(string apiKey, string secret)
        {
            _apiKey = apiKey;
            _secret = secret;
            authenticator = new OAuthAuthenticator(_apiKey, _secret);
        }


        public async Task<bool> connectToSmugMug()
        {
            


            string authorizeUrl = await authorize();

            WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                        WebAuthenticationOptions.None,
                                        new Uri(authorizeUrl),
                                        new Uri(CallbackUri));

            string pid = "";
            if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                //textBox.Text = WebAuthenticationResult.ResponseData.ToString();
                string tokenResponse = WebAuthenticationResult.ResponseData.ToString();
                string[] elem = tokenResponse.Split('&');
                
                bool oauth_verifier_found = false;
                foreach (var el in elem)
                {
                    string[] tmp = el.Split('=');
                    if (tmp[0] == "oauth_verifier")
                    {
                        pid = tmp[1];
                        oauth_verifier_found = true;
                        break;
                    }
                }
                if (!oauth_verifier_found) { return false; }
            }
            else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                return false;
                //textBox.Text = "HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString();
            }
            else
            {
                return false;
                //textBox.Text = "Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseStatus.ToString();
            }

            Tuple<string, string> tokens = await getOauthToken(pid);

            string OAuthToken = _apiKey + "|" + _secret + "|" + tokens.Item1 + "|" + tokens.Item2;

            Debug.WriteLine(_apiKey);
            Debug.WriteLine(_secret);
            Debug.WriteLine(tokens.Item1);
            Debug.WriteLine(tokens.Item2);

            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(
                "OAuthToken", "smugmug", OAuthToken));

            Debug.WriteLine(tokens.Item1 + ", " + tokens.Item2);
            return true;

        } 


        private async Task<string> authorize()
        {
            
            string reqTokUrl = authenticator.CreateGetRequestTokenAddress(OAuthGetRequestTokenUrl, HttpMethod.Get.ToString(), NonWebOAuthApplicationCallback);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(reqTokUrl);

            if (response.IsSuccessStatusCode)
            {
                _tokens = await response.Content.ReadAsStringAsync();
                string authorizeUrl = authenticator.CreateAuthorizeAddress(OAuthAuthorizeUrl + FullAccess, _tokens);
                return authorizeUrl;
            }
            return null;
        }

        private async Task<Tuple<string,string>> getOauthToken(string pin)
        {
            // Get the request token to exchange for an access token
            string reqToken, reqTokenSecret;
            authenticator.ParseRequestTokens(_tokens, out reqToken, out reqTokenSecret);
            string accessTokenUrl = authenticator.CreateGetAccessTokenAddress(OAuthGetAccessTokenUrl, "GET", reqTokenSecret, reqToken, pin);

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(accessTokenUrl);

            string authToken, authTokenSecret;
            if (response.IsSuccessStatusCode)
            {
                string authTokens = await response.Content.ReadAsStringAsync();

                Debug.WriteLine(authTokens);
                authenticator.ParseRequestTokens(authTokens, out authToken, out authTokenSecret);
            }
            else
            {
                authToken = "";
                authTokenSecret = "";
            }
            Tuple<string, string> tokens = new Tuple<string, string>(authToken, authTokenSecret);
            return tokens;
            
        }

    }
}
