using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Sitecore.Commerce.Sample.Console.Authentication
{
    public static class SitecoreIdServerAuth
    {
        public static string GetToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Program.SitecoreIdServerUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", "postman-api"),
                    new KeyValuePair<string, string>("scope", "openid EngineAPI postman_api"),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", Program.UserName),
                    new KeyValuePair<string, string>("password", Program.Password)
                });

                var response = client.PostAsync("connect/token", content).Result;
                var result = JsonConvert.DeserializeObject<TokenResponse>(response.Content.ReadAsStringAsync().Result);
                return result.access_token;
            }
        }

        private struct TokenResponse
        {
            public string access_token { get; set; }
            public long expires_in { get; set; }
            public string token_type { get; set; }
        }
    }
}
