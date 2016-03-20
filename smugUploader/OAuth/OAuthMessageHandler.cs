using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using smugUploader.OAuth.Helpers;
using System.Net;
using System.Diagnostics;

namespace smugUploader.OAuth
{
    public class OAuthMessageHandler : DelegatingHandler
    {
        private readonly string _apiKey;
        private readonly string _secret;
        private readonly string _authToken;
        private readonly string _authTokenSecret;

        public OAuthMessageHandler(string apiKey, string secret, string authToken, string authTokenSecret)
        {
            _apiKey = apiKey;
            _secret = secret;
            _authToken = authToken;
            _authTokenSecret = authTokenSecret;

            this.InnerHandler = new HttpClientHandler();
        }

        public string GetAuthenticationHeaderForRequest(Uri requestUri, HttpMethod method)
        {

            string nonce = OAuthHelpers.GenerateNonce();
            string timestamp = OAuthHelpers.GenerateTimestamp();
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string,string>(Constants.oauth_consumer_key, _apiKey),
                new KeyValuePair<string,string>(Constants.oauth_nonce, nonce ),
                new KeyValuePair<string,string>(Constants.oauth_timestamp, timestamp ),
                new KeyValuePair<string,string>(Constants.oauth_signature_method, "HMAC-SHA1"),
                new KeyValuePair<string,string>(Constants.oauth_version, Constants.oauth_version_1a),
                new KeyValuePair<string,string>(Constants.oauth_token, _authToken),
            };

            Debug.WriteLine(Constants.oauth_consumer_key +"&"+ _apiKey);
            Debug.WriteLine(Constants.oauth_nonce + "&" + nonce);
            Debug.WriteLine(Constants.oauth_timestamp + "&" + timestamp);
            Debug.WriteLine(Constants.oauth_signature_method + "&" + "HMAC-SHA1");
            Debug.WriteLine(Constants.oauth_version + "&" + Constants.oauth_version_1a);
            Debug.WriteLine(Constants.oauth_token + "&" + _authToken);
            Debug.WriteLine(Constants.oauth_token_secret + "&" + _authTokenSecret);
            string baseUri = requestUri.OriginalString;

            // We need to handle the case where the request comes with query parameters
            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                baseUri = requestUri.OriginalString.Replace(requestUri.Query, "");

                foreach (var param in requestUri.Query.Split(new char[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var values = param.Split('=');
                    string name = values[0];
                    string value = string.Empty;
                    if (values.Length > 1)
                    {
                        value = values[1];
                    }
                    parameters.Add(new KeyValuePair<string, string>(name, value));
                }
            }

            string baseString = OAuthHelpers.GenerateBaseString(baseUri, method.ToString(), parameters);
            string sig = OAuthHelpers.EncodeValue(OAuthHelpers.GenerateHMACDigest(baseString, _secret, _authTokenSecret));

            parameters.Add(new KeyValuePair<string, string>(Constants.oauth_signature, sig));

            StringBuilder sb = new StringBuilder();
            foreach (var param in parameters)
            {
                if (param.Key.StartsWith("oauth"))
                {
                    sb.AppendFormat("{0}={1},", param.Key, WebUtility.HtmlEncode(param.Value));
                }
            }
            sb.Remove(sb.Length - 1, 1);

            Debug.WriteLine(sb.ToString());

            return sb.ToString();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            string header = GetAuthenticationHeaderForRequest(request.RequestUri, request.Method);

            request.Headers.Authorization = new AuthenticationHeaderValue(Constants.OAuthAuthenticationHeader, header);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
