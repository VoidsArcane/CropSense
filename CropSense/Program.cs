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

stringBuilder.Append("<table><thead><th>Date</th><th>Weather</th><th>Mean Temp</th>");
stringBuilder.Append("<th>Min Temp</th><th>Max Temp</th><th>Snow</th><th>Rain</th>");
stringBuilder.Append("<th>Precipitation %</th><th>EV0</th><th>Sunshine Duration</th>");
stringBuilder.Append("<th>Shortwave Radiation</th><th>Max Wind Gusts</th><th>Wind</th>");
stringBuilder.Append("<th>Cloud Cover</th><th>Humidity</th><th>Notes</th></thead><tbody>");

for (int i = 0; i < root.GetProperty("time").EnumerateArray().Count(); i++)
{
	stringBuilder.Append("<tr>");
	string? date = root.GetProperty("time").EnumerateArray().ElementAt(i).GetString();
	stringBuilder.Append("<td>" + date + "</td>");

	int weatherCode = root.GetProperty("weather_code").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<td>" + weatherCode + "</td>");

	float temperatureMean = root.GetProperty("temperature_2m_mean").EnumerateArray().ElementAt(i).GetSingle();
	float tempMax = root.GetProperty("temperature_2m_max").EnumerateArray().ElementAt(i).GetSingle();
	float tempMin = root.GetProperty("temperature_2m_min").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<td>" + temperatureMean + "</td>");
	stringBuilder.Append("<td>" + tempMin + "</td>");
	stringBuilder.Append("<td>" + tempMax + "</td>");

	double snowfallSum = root.GetProperty("snowfall_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + snowfallSum + "</td>");

	double precipitationSum = root.GetProperty("precipitation_sum").EnumerateArray().ElementAt(i).GetDouble();
	double rainSum = precipitationSum - snowfallSum;
	stringBuilder.Append("<td>" + rainSum + "</td>");

	int PrecipitationProbability = root.GetProperty("precipitation_probability_max").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<td>" + PrecipitationProbability + "</td>");

	double evapotranspiration = root.GetProperty("et0_fao_evapotranspiration").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + evapotranspiration + "</td>");

	double sunshineDuration = Math.Round(root.GetProperty("sunshine_duration").EnumerateArray().ElementAt(i).GetDouble() / 3600, 2);
	stringBuilder.Append("<td>" + sunshineDuration + "</td>");

	double shortwaveRadiationSum = root.GetProperty("shortwave_radiation_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<td>" + shortwaveRadiationSum + "</td>");

	float windGustsMax = root.GetProperty("wind_gusts_10m_max").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<td>" + windGustsMax + "</td>");

	float windSpeed = root.GetProperty("wind_speed_10m_mean").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<td>" + windSpeed + "</td>");

	int cloudCover = root.GetProperty("cloud_cover_mean").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<td>" + cloudCover + "</td>");

	int relativeHumidity = root.GetProperty("relative_humidity_2m_mean").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<td>" + relativeHumidity + "</td>");
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

