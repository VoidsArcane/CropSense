using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Linq;

// Fetching Data from API
using HttpClient client = new HttpClient();

string url = "https://api.open-meteo.com/v1/forecast?latitude=37.945&longitude=23.7142&daily=weather_code,temperature_2m_max,temperature_2m_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,precipitation_hours,precipitation_probability_max,sunrise,sunset,daylight_duration,sunshine_duration,uv_index_max,wind_gusts_10m_max,wind_speed_10m_mean,shortwave_radiation_sum,et0_fao_evapotranspiration,temperature_2m_mean,cloud_cover_mean,relative_humidity_2m_mean&timezone=auto";

HttpResponseMessage response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();

// Parsing out Relevant Data
using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement.GetProperty("daily");

/*
// Printing out the JSON (DEBUG)
string prettyJson = JsonSerializer.Serialize(root, new JsonSerializerOptions
{
	WriteIndented = true
});

Console.WriteLine(prettyJson);
*/
// Generating Report
StringBuilder stringBuilder = new StringBuilder();

stringBuilder.Append("<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\">");

stringBuilder.Append("<table class=\"table table-striped table-hover table-bordered\"><thead class=\"table-dard\"><th>Date</th>");
stringBuilder.Append("<th>Min Temp</th><th>Max Temp</th><th>Snow</th><th>Rain</th>");
stringBuilder.Append("<th>ET₀</th>");
stringBuilder.Append("<th>Shortwave Radiation</th><th>Wind Gusts</th>");
stringBuilder.Append("</thead><tbody>");

for (int i = 0; i < root.GetProperty("time").EnumerateArray().Count(); i++)
{
	stringBuilder.Append("<tr>");
	string? date = root.GetProperty("time").EnumerateArray().ElementAt(i).GetString();
	stringBuilder.Append("<td>" + date + "</td>");

	float tempMax = root.GetProperty("temperature_2m_max").EnumerateArray().ElementAt(i).GetSingle();
	float tempMin = root.GetProperty("temperature_2m_min").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<td>" + tempMin + "</td>");
	stringBuilder.Append("<td>" + tempMax + "</td>");

	double snowfallSum = root.GetProperty("snowfall_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + snowfallSum + "</td>");

	double precipitationSum = root.GetProperty("precipitation_sum").EnumerateArray().ElementAt(i).GetDouble();
	double rainSum = precipitationSum - snowfallSum;
	stringBuilder.Append("<td>" + rainSum + "</td>");

	double evapotranspiration = root.GetProperty("et0_fao_evapotranspiration").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + evapotranspiration + "</td>");

	double shortwaveRadiationSum = root.GetProperty("shortwave_radiation_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + shortwaveRadiationSum + "</td>");

	float windGustsMax = root.GetProperty("wind_gusts_10m_max").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<td>" + windGustsMax + "</td>");

	stringBuilder.Append("</tr>");
}

stringBuilder.Append("</tbody></table>");
string reportFilePath = "report.html";
System.IO.File.WriteAllText(reportFilePath, stringBuilder.ToString());

System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
	FileName = reportFilePath,
	UseShellExecute = true
});

