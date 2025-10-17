using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


using HttpClient client = new HttpClient();

string url = "https://api.open-meteo.com/v1/forecast?latitude=37.945&longitude=23.7142&daily=temperature_2m_max,temperature_2m_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,precipitation_hours,precipitation_probability_max,sunrise,sunset,daylight_duration,uv_index_max,wind_speed_10m_max,wind_gusts_10m_max,weather_code&timezone=auto";

HttpResponseMessage response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();

using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement;



// Printing out the JSON
string prettyJson = JsonSerializer.Serialize(root, new JsonSerializerOptions
{
	WriteIndented = true
});

Console.WriteLine(prettyJson);
