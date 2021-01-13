using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Prowl
{
    public class ProwlMessage
    {
        private readonly HttpClient http = new HttpClient();

        public async Task<HttpResponseMessage> SendAsync(string application, string description)
        {
            string url = "https://prowl.weks.net/publicapi/add";
            string apiKey = Environment.GetEnvironmentVariable("PROWL_API_KEY") ?? throw new NullReferenceException("PROWL_API_KEY");

            var values = new Dictionary<string, string>
                {
                    { "apikey", apiKey },
                    { "application", application },
                    { "description", description }
                };

            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await http.PostAsync(url, content);
            return response;
        }
    }
}
