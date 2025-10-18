using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Linq;

// Fetching Data from API
using HttpClient client = new HttpClient();

string url = "https://api.open-meteo.com/v1/forecast?latitude=37.945&longitude=23.7142&daily=temperature_2m_max,temperature_2m_min,rain_sum,snowfall_sum,precipitation_sum,wind_gusts_10m_max,shortwave_radiation_sum,et0_fao_evapotranspiration&timezone=auto";

HttpResponseMessage response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();

// Parsing out Relevant Data
using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement.GetProperty("daily");

// Generating Report
StringBuilder stringBuilder = new StringBuilder();

stringBuilder.Append("<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\">");
stringBuilder.Append("<table class=\"table table-striped table-hover table-bordered\"><thead class=\"table-dark\"><th>Date</th>");
stringBuilder.Append("<th>Min Temp (°C)</th><th>Max Temp (°C)</th><th>Snow (mm)</th><th>Rain (mm)</th>");
stringBuilder.Append("<th>ET₀ (mm)</th>");
stringBuilder.Append("<th>Shortwave Radiation (MJ/m²)</th><th>Wind Gusts (km/h)</th>");
stringBuilder.Append("</thead><tbody>");

for (int i = 0; i < root.GetProperty("time").EnumerateArray().Count(); i++)
{
	string? date = root.GetProperty("time").EnumerateArray().ElementAt(i).GetString();
	float maxTemp = root.GetProperty("temperature_2m_max").EnumerateArray().ElementAt(i).GetSingle();
	float minTemp = root.GetProperty("temperature_2m_min").EnumerateArray().ElementAt(i).GetSingle();
	float windGusts = root.GetProperty("wind_gusts_10m_max").EnumerateArray().ElementAt(i).GetSingle();
	double snow = root.GetProperty("snowfall_sum").EnumerateArray().ElementAt(i).GetDouble();
	double totalPrecipitation = root.GetProperty("precipitation_sum").EnumerateArray().ElementAt(i).GetDouble();
	double rain = totalPrecipitation - snow;
	double evapotranspiration = root.GetProperty("et0_fao_evapotranspiration").EnumerateArray().ElementAt(i).GetDouble();
	double shortwaveRadiation = root.GetProperty("shortwave_radiation_sum").EnumerateArray().ElementAt(i).GetDouble();

	string minTempColor = minTemp <= 5 ? "color: red;" : minTemp <= 10 ? "color: orange;" : "";
	string windGustColor = windGusts >= 50 ? "color: red;" : "";

	stringBuilder.Append("<tr>");
	stringBuilder.Append($"<td>{date}</td>");
	stringBuilder.Append($"<td style='{minTempColor}'> {minTemp} </td>");
	stringBuilder.Append($"<td>{maxTemp}</td>");
	stringBuilder.Append($"<td>{snow}</td>");
	stringBuilder.Append($"<td>{rain}</td>");
	stringBuilder.Append($"<td>{evapotranspiration}</td>");
	stringBuilder.Append($"<td>{shortwaveRadiation}</td>");
	stringBuilder.Append($"<td style='{windGustColor}'>{windGusts}</td>");
	stringBuilder.Append("</tr>");
}
stringBuilder.Append("</tbody></table>");

// Creating and Opening Report
string reportFilePath = "report.html";
System.IO.File.WriteAllText(reportFilePath, stringBuilder.ToString());

System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
	FileName = reportFilePath,
	UseShellExecute = true
});

