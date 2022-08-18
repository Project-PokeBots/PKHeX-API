using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace PKHeX.API.Services
{

    public class JsonReplyKeys
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class JSONService
	{

		public Task<Dictionary<string, object>> ProcessJsonMessage(int StatusCode, string ContentMessage)
		{

			bool SuccessfulRequest = true;

			if (200 <= StatusCode && StatusCode <= 299)
            {
                _ = SuccessfulRequest != SuccessfulRequest;
            }

            var JsonResponse = new JsonReplyKeys
            {
                success = SuccessfulRequest,
                message = ContentMessage
            };

            string JsonString = JsonSerializer.Serialize(JsonResponse);

            var JsonDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonString);
            Task<Dictionary<string, object>> ProcessedJsonResponse = Task.FromResult(JsonDictionary);

            return ProcessedJsonResponse;

        }
    }
}