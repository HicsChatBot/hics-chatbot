using System.Text.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HicsChatBot.Services
{
    public class BaseService
    {
        protected readonly Uri baseAddress = new Uri("DATABASE_ENDPOINT");
        private readonly HttpClient client;

        protected BaseService()
        {
            client = new HttpClient()
            {
                BaseAddress = baseAddress
            };
        }

        /**
        executes the get request and returns a JSON string of response.
        */
        protected async Task<string> Get(string uri)
        {
            HttpResponseMessage resp = await client.GetAsync(uri);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                // throw some sort of error
                string str = await resp.Content.ReadAsStringAsync();
                string err = JsonObject.Parse(str)["errors"][0].GetValue<string>();
                throw new Exception("FAILED TO GET: " + err);
            }

            var jsonResponse = await resp.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        public async Task<string> Post(string uri, object data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);
            HttpContent stringContent = ToStringContent(jsonContent);

            HttpResponseMessage resp = await client.PostAsync(uri, stringContent);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                // throw some sort of error
                string str = await resp.Content.ReadAsStringAsync();
                string err = JsonObject.Parse(str)["errors"][0].GetValue<string>();
                throw new Exception("FAILED TO POST: " + err);
            }

            // resp.EnsureSuccessStatusCode();

            var jsonResponse = await resp.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        /**
        jsonString: json string of the content to send.
        */
        private static StringContent ToStringContent(string jsonString)
        {
            return new StringContent(
                jsonString,
                Encoding.UTF8,
                "application/json");
        }
    }
}
