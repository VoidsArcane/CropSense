using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;

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

// Extracting Variables
Console.Write(root);
Dictionary<string, JsonElement> data = new Dictionary<string, JsonElement>();
foreach (JsonProperty prop in root.EnumerateObject())
{
	data.Add(prop.Name, prop.Value);
}

// Generating Report
StringBuilder stringBuilder = new StringBuilder();



if (reportType == "csv")
{
	foreach (string key in data.Keys)
	{
		stringBuilder.Append(key);
		if (key != data.Keys.Last()) stringBuilder.Append(",");
	}
	stringBuilder.AppendLine();

	for (int i = 0; i < data.First().Value.EnumerateArray().Count(); i++)
	{
		foreach (JsonElement array in data.Values)
		{
			stringBuilder.Append(array.EnumerateArray().ElementAt(i));
			if (!array.Equals(data.Values.Last())) stringBuilder.Append(",");
		}
		stringBuilder.AppendLine();
	}

}

if (reportType == "html")
{
	stringBuilder.Append("<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\">");
	stringBuilder.Append("<table class=\"table table-striped table-hover table-bordered\"><thead class=\"table-dark\">");
	
	foreach (string key in data.Keys)
	{
		stringBuilder.Append($"<th>{key}</th>");
	}
	
	stringBuilder.Append("</thead><tbody>");

}

if (reportType == "html")
{
	for (int i = 0; i < data.First().Value.EnumerateArray().Count(); i++)
	{
		stringBuilder.Append("<tr>");
		foreach (JsonElement array in data.Values)
		{
			stringBuilder.Append($"<td>{array.EnumerateArray().ElementAt(i)}</td>");
		}
		stringBuilder.Append("</tr>");
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


