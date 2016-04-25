using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PicoBird
{
    public class API
    {
        private static readonly string APIROOT = "https://api.twitter.com/1.1";

        private HttpClient client;
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }

        // Constructor
        public API(string consumerKey, string consumerSecret)
        {
            client = new HttpClient();
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        // Send OAuth signed requests.
        private async Task<HttpResponseMessage> SendRequest(
            HttpMethod method,
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
        {
            string baseUrl = APIROOT + resource;    // api url without query string
            string requestUrl = baseUrl;    // api url with query string (if any)
            string queryString = query != null ? PercentEncode(query) : "";
            if (!queryString.Equals("")) requestUrl += "?" + queryString;

            NameValueCollection parameters = new NameValueCollection
            {
                { "oauth_consumer_key", ConsumerKey },
                { "oauth_nonce", GenerateNonce() },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", GenerateTimestamp() },
                { "oauth_token", Token },
                { "oauth_version", "1.0" }
            };
            NameValueCollection headerParams = new NameValueCollection(parameters);
            if (query != null) parameters.Add(query);
            if (method == HttpMethod.Post) parameters.Add(data);
            string paramString = PercentEncode(parameters);

            string signBase = method.ToString();
            signBase += "&" + Uri.EscapeDataString(baseUrl);
            signBase += "&" + Uri.EscapeDataString(paramString);

            string signKey = Uri.EscapeDataString(ConsumerSecret) + "&";
            signKey += Uri.EscapeDataString(TokenSecret);

            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signKey));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signBase)));
            headerParams.Add("oauth_signature", signature);
            string headerString = "OAuth "
                + string.Join(", ", (from k in headerParams.AllKeys orderby k ascending
                                     from v in headerParams.GetValues(k)
                                     select $"{Uri.EscapeDataString(k)}=\"{Uri.EscapeDataString(v)}\""));

            HttpRequestMessage request = new HttpRequestMessage(method, requestUrl);
            request.Headers.Add("Authorization", headerString);
            if (method == HttpMethod.Post)
                request.Content = new FormUrlEncodedContent(
                    from k in data.AllKeys from v in data.GetValues(k) select new KeyValuePair<string, string>(k, v));

            HttpResponseMessage res = await client.SendAsync(request);

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
                throw new APIException(res);

            return res;
        }

        public async Task<HttpResponseMessage> Get(
            string resource,
            NameValueCollection query = null)
            => await SendRequest(HttpMethod.Get, resource, query);

        public async Task<T> Get<T>(
            string resource,
            NameValueCollection query = null)
            => JsonConvert.DeserializeObject<T>(
                await (await SendRequest(HttpMethod.Get, resource, query)).Content.ReadAsStringAsync());

        public async Task<HttpResponseMessage> Post(
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
            => await SendRequest(HttpMethod.Post, resource, query, data);

        public async Task<T> Post<T>(
            string resource,
            NameValueCollection query = null,
            NameValueCollection data = null)
            => JsonConvert.DeserializeObject<T>(
                await (await SendRequest(HttpMethod.Post, resource, query, data)).Content.ReadAsStringAsync());

        private static string PercentEncode(NameValueCollection nvc)
        {
            var values = (from k in nvc.AllKeys orderby k ascending
                         from v in nvc.GetValues(k)
                         select $"{Uri.EscapeDataString(k)}={Uri.EscapeDataString(v)}");
            return string.Join("&", values);
        }

        private static string GenerateNonce()
        {
            Random rand = new Random();
            byte[] x = new byte[32];
            rand.NextBytes(x);
            return Regex.Replace(Convert.ToBase64String(x), "[+/=]", "");
        }

        private static string GenerateTimestamp()
            => ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)).ToString();
    }
}
