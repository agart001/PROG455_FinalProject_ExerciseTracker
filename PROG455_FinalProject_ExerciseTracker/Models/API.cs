using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Xml;

namespace PROG455_FinalProject_ExerciseTracker.Models
{
    /// <summary>
    /// Represents an API for interacting with a web service.
    /// </summary>
    public class API
    {

        /// <summary>
        /// API base url
        /// </summary>
        private string? url;

        /// <summary>
        /// Initializes a new instance of the <see cref="API"/> class.
        /// </summary>
        public API()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="API"/> class with a specified URL.
        /// </summary>
        public API(string url)
        {
            this.url = url;
        }

        #region POST

        /// <summary>
        /// Asynchronously makes a POST request to the API.
        /// </summary>
        public async Task AsyncPOST(string endpoint, IDictionary<string, string> values)
        {
            var request = new FormUrlEncodedContent(values);
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url!);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(endpoint, request);

                var asString = await response.Content.ReadAsStringAsync();

                if (asString != null)
                {
                    POSTResult = asString;
                }
            };
        }

        /// <summary>
        /// Gets the result of the last POST request
        /// </summary>
        public string? POSTResult { get; internal set; }

        #endregion

        #region GET

        /// <summary>
        /// Asynchronously makes a GET request to the API.
        /// </summary>
        public async Task AsyncGET(string method)
        {
            var request = url + method;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(request);

                var asString = await response.Content.ReadAsStringAsync();

                if (asString != null)
                {
                    GETResult = asString;
                }
            };
        }

        /// <summary>
        /// Gets the result of the last GET request.
        /// </summary>
        public string? GETResult { get; internal set; }

        #endregion

        #region JSON

        public string NSJsonSerialize(object obj) => JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

        public T? NSJsonDeserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

        #endregion
    }


    /// <summary>
    /// Represents a query object for an API.
    /// </summary>
    public struct APIQuery
    {
        /// <summary>
        /// Gets or sets the name of the table to query.
        /// </summary>
        public string Table { get; internal set; }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        public string Query { get; internal set; }

        /// <summary>
        /// Returns a JSON string representation of the APIQuery object.
        /// </summary>
        /// <returns>A JSON string representing the APIQuery object.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
