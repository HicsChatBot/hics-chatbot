using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace HicsChatBot.Services
{
    public class BaseService
    {
        protected readonly Uri baseAddress = new Uri(System.Environment.GetEnvironmentVariable("DATABASE_ENDPOINT"));
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

        protected async Task<string> Get(string uri, Dictionary<string, string> uri_params)
        {
            List<string> uri_params_xs = new();

            foreach (KeyValuePair<string, string> kvp in uri_params)
            {
                uri_params_xs.Add($"{kvp.Key}={kvp.Value}");
            }

            uri += "?" + String.Join("&", uri_params_xs.ToArray());

            return await Get(uri);
        }

        public async Task<string> Delete(string uri, object data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);
            HttpContent stringContent = ToStringContent(jsonContent);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(baseAddress.AbsoluteUri + uri),
                Content = stringContent,
            };

            HttpResponseMessage resp = await client.SendAsync(request);

            if (resp.StatusCode != HttpStatusCode.OK)
            {
                // throw some sort of error
                string str = await resp.Content.ReadAsStringAsync();
                string err = JsonObject.Parse(str)["errors"][0].GetValue<string>();
                throw new Exception("FAILED TO DELETE: " + err);
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
