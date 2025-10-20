using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Linq;

// Parsing Config
string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

var lines = File.ReadAllLines(iniPath);
string section = "";
var config = new Dictionary<string, Dictionary<string, string>>();

foreach (var line in lines)
{
    string trimmed = line.Trim();
    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
    {
        section = trimmed[1..^1];
        config[section] = new Dictionary<string, string>();
    }
    else if (trimmed.Contains('=') && !trimmed.StartsWith(";"))
    {
        var parts = trimmed.Split('=', 2);
        config[section][parts[0].Trim()] = parts[1].Trim();
    }
}

string location = config["general"]["location"];
string forecast_days = config["general"]["forecast_days"];
bool auto_open_report = config["general"]["auto_open_report"] == "true" ? true : false;
string reportFilePath = config["general"]["report_file_path"];
string reportType = config["general"]["report_type"];


StringBuilder variablesIncluded = new StringBuilder();

Dictionary<string, string> variables = config["include_variables"];

foreach (KeyValuePair<string, string> entry in variables)
{
	if (entry.Value == "true") variablesIncluded.Append(entry.Key);
	if (entry.Key != variables.Last().Key) variablesIncluded.Append(",");
}

// Fetching Data from API
using HttpClient client = new HttpClient();

// Fetching Latitude and Longitude for confic location
double latitude = 0.00;
double longitude = 0.00;

HttpResponseMessage response = await client.GetAsync($"https://geocoding-api.open-meteo.com/v1/search?name={location}&count=1&language=en");
response.EnsureSuccessStatusCode();

string locationData = await response.Content.ReadAsStringAsync();

using JsonDocument locationJSON = JsonDocument.Parse(locationData);
JsonElement root = locationJSON.RootElement.GetProperty("results").EnumerateArray().ElementAt(0);
latitude = root.GetProperty("latitude").GetDouble();
longitude = root.GetProperty("longitude").GetDouble();



string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily={variablesIncluded}&timezone=auto&forecast_days={forecast_days}";

response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();
using JsonDocument doc = JsonDocument.Parse(json);
root = doc.RootElement.GetProperty("daily");

/*
var options = new JsonSerializerOptions { WriteIndented = true };
string prettyJson = JsonSerializer.Serialize(doc, options);
*/

// Generating Report
StringBuilder stringBuilder = new StringBuilder();

if (reportType == "html")
{
	stringBuilder.Append("<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\">");
	stringBuilder.Append("<table class=\"table table-striped table-hover table-bordered\"><thead class=\"table-dark\"><th>Date</th>");
	stringBuilder.Append("<th>Min Temp (°C)</th><th>Max Temp (°C)</th><th>Snow (mm)</th><th>Rain (mm)</th>");
	stringBuilder.Append("<th>ET₀ (mm)</th>");
	stringBuilder.Append("<th>Shortwave Radiation (MJ/m²)</th><th>Wind Gusts (km/h)</th>");
	stringBuilder.Append("</thead><tbody>");
}

if (reportType == "csv")
{
	JsonProperty[] props = root.EnumerateObject().ToArray();
	for (int i = 0; i < props.Length; i++)
	{
		if (props[i].Name != "rain_sum")
		{
			stringBuilder.Append(props[i].Name);
			if (i != props.Length - 1) stringBuilder.Append(",");
		}
	}
	stringBuilder.AppendLine();
}

for (int i = 0; i < root.GetProperty("time").EnumerateArray().Count(); i++)
{
	string? date = root.GetProperty("time").EnumerateArray().ElementAt(i).GetString();
	float maxTemp = 0;
	float minTemp = 0;
	float windGusts = 0;
	double snow = 0;
	double totalPrecipitation = 0;
	double evapotranspiration = 0;
	double shortwaveRadiation = 0;

	var tempElement = root.GetProperty("temperature_2m_max").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetSingle(out maxTemp);

	tempElement = root.GetProperty("temperature_2m_min").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetSingle(out minTemp);

	tempElement = root.GetProperty("wind_gusts_10m_max").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetSingle(out windGusts);

	tempElement = root.GetProperty("snowfall_sum").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetDouble(out snow);

	tempElement = root.GetProperty("precipitation_sum").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetDouble(out totalPrecipitation);

	tempElement = root.GetProperty("et0_fao_evapotranspiration").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetDouble(out evapotranspiration);

	tempElement = root.GetProperty("shortwave_radiation_sum").EnumerateArray().ElementAt(i);
	if (tempElement.ValueKind == JsonValueKind.Number) tempElement.TryGetDouble(out shortwaveRadiation);


	double rain = totalPrecipitation - snow;


	if (reportType == "html")
	{
		string? minTempColor = minTemp <= 5 ? "color: red;" : minTemp <= 10 ? "color: orange;" : "";
		string? windGustColor = windGusts >= 50 ? "color: red;" : "";
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
	
	if (reportType == "csv")
	{
		stringBuilder.AppendLine($"{date},{minTemp},{maxTemp},{snow},{rain},{evapotranspiration},{shortwaveRadiation},{windGusts}");
	}
}
if (reportType == "html") stringBuilder.Append("</tbody></table>");

// Creating and Opening Report
string reportFileBasePath = reportFilePath;
string reportFileName = $"report_{root.GetProperty("time").EnumerateArray().ElementAt(0).GetString()}.{reportType}";
System.IO.File.WriteAllText(reportFileBasePath + reportFileName, stringBuilder.ToString());

if (auto_open_report)
{
	System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
	{
		FileName = reportFileName,
		UseShellExecute = true
	});
}


