using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

using HttpClient client = new HttpClient();

string url = "https://api.open-meteo.com/v1/forecast?latitude=37.945&longitude=23.7142&daily=weather_code,temperature_2m_max,temperature_2m_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,precipitation_hours,precipitation_probability_max,sunrise,sunset,daylight_duration,sunshine_duration,uv_index_max,wind_gusts_10m_max,wind_speed_10m_max,shortwave_radiation_sum,et0_fao_evapotranspiration,et0_fao_evapotranspiration_sum,temperature_2m_mean,cloud_cover_mean,cloud_cover_max,cloud_cover_min,relative_humidity_2m_max,relative_humidity_2m_min,relative_humidity_2m_mean&timezone=auto";

HttpResponseMessage response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();

using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement.GetProperty("daily");


// Printing out the JSON
string prettyJson = JsonSerializer.Serialize(root, new JsonSerializerOptions
{
	WriteIndented = true
});

Console.WriteLine(prettyJson);


StringBuilder stringBuilder = new StringBuilder();

foreach (JsonProperty dataField in root.EnumerateObject())
{
	stringBuilder.Append("<p>" + dataField.Value + "</p>");
}

string reportFilePath = "report.html";
System.IO.File.WriteAllText(reportFilePath, stringBuilder.ToString());

System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
	FileName = reportFilePath,
	UseShellExecute = true
});

