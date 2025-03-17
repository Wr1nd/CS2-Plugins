using hoursLimtation;
using McMaster.NETCore.Plugins;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API;

namespace hoursLimtation
{

    public partial class hoursLimtation
    {
        public HttpClient _httpClient;

        private async Task<int> FetchCS2PlaytimeAsync(string steamId)
        {

            var url = $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={_config.ApiKey}&steamid={steamId}&format=json";
            var json = await GetApiResponseAsync(url);
            return json != null ? ParseCS2Playtime(json) : 0;
        }

        private async Task<string?> GetApiResponseAsync(string url)
	    {

            _httpClient = new HttpClient();

            try
		    {
			    var response = await _httpClient.GetAsync(url);
			    if (response.IsSuccessStatusCode)
			    {
                    return await response.Content.ReadAsStringAsync();

			    }
		    }
		    catch (Exception e)
		    {
               Console.WriteLine(e);
		    }
		    return null;
	    }

        private int ParseCS2Playtime(string json)
        {
            JObject data = JObject.Parse(json);
            Console.WriteLine("Parsed JSON: " + data.ToString()); 
            JToken? game = data["response"]?["games"]?.FirstOrDefault(x => x["appid"]?.Value<int>() == 730);
            return game?["playtime_forever"]?.Value<int>() ?? 0;
        }

    }
}
