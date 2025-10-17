using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Linq;

// Fetching Data from API
using HttpClient client = new HttpClient();

string url = "https://api.open-meteo.com/v1/forecast?latitude=37.945&longitude=23.7142&daily=weather_code,temperature_2m_max,temperature_2m_min,rain_sum,showers_sum,snowfall_sum,precipitation_sum,precipitation_hours,precipitation_probability_max,sunrise,sunset,daylight_duration,sunshine_duration,uv_index_max,wind_gusts_10m_max,wind_speed_10m_max,shortwave_radiation_sum,et0_fao_evapotranspiration,temperature_2m_mean,cloud_cover_mean,relative_humidity_2m_mean&timezone=auto";

HttpResponseMessage response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

string json = await response.Content.ReadAsStringAsync();

// Parsing out Relevant Data
using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement.GetProperty("daily");

// Printing out the JSON (DEBUG)
string prettyJson = JsonSerializer.Serialize(root, new JsonSerializerOptions
{
	WriteIndented = true
});

Console.WriteLine(prettyJson);

// Generating Report
StringBuilder stringBuilder = new StringBuilder();

for (int i = 0; i < root.GetProperty("time").EnumerateArray().Count(); i++)
{
	string? date = root.GetProperty("time").EnumerateArray().ElementAt(i).GetString();
	stringBuilder.Append("<p>Date : " + date + "</p>");

	int weatherCode = root.GetProperty("weather_code").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<p> Weather Code: " + weatherCode + "</p>");

	float tempMax = root.GetProperty("temperature_2m_max").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<p> Max Temp : " + tempMax + "</p>");

	float tempMin = root.GetProperty("temperature_2m_min").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<p> Min Temp : " + tempMin + "</p>");

	double rainSum = root.GetProperty("rain_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Rain Sum : " + rainSum + "</p>");

	double showerSum = root.GetProperty("showers_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Shower Sum : " + showerSum + "</p>");

	double snowfallSum = root.GetProperty("snowfall_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Snowfall Sum : " + snowfallSum + "</p>");

	double precipitationSum = root.GetProperty("precipitation_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Precipitation Sum : " + precipitationSum + "</p>");

	double precipitationHours = root.GetProperty("precipitation_hours").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Precipitation Hours : " + precipitationHours + "</p>");

	int precipitationProbabilityMax = root.GetProperty("precipitation_probability_max").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<p> Precipitation Probability Max : " + precipitationProbabilityMax + "</p>");

	string? sunrise = root.GetProperty("sunrise").EnumerateArray().ElementAt(i).GetString();
	stringBuilder.Append("<p>Sunrise : " + sunrise + "</p>");

	string? sunset = root.GetProperty("sunset").EnumerateArray().ElementAt(i).GetString();
	stringBuilder.Append("<p>Sunset : " + sunset + "</p>");

	double daylightDuration = root.GetProperty("daylight_duration").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Daylight duration : " + daylightDuration + "</p>");

	double sunshineDuration = root.GetProperty("sunshine_duration").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Sunshine duration : " + sunshineDuration + "</p>");

	double uvIndexMax = root.GetProperty("uv_index_max").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Uv Index Max : " + uvIndexMax + "</p>");

	float windGustsMax = root.GetProperty("wind_gusts_10m_max").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<p> Wind Gusts Max : " + windGustsMax + "</p>");

	float windSpeedMax = root.GetProperty("wind_speed_10m_max").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<p> Wind Speed Max : " + windSpeedMax + "</p>");

	double shortwaveRadiationSum = root.GetProperty("shortwave_radiation_sum").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Shortwave Radiation Sum : " + shortwaveRadiationSum + "</p>");

	double evapotranspiration = root.GetProperty("et0_fao_evapotranspiration").EnumerateArray().ElementAt(i).GetDouble();
	stringBuilder.Append("<p> Evapotranspiration : " + evapotranspiration + "</p>");

	float temperatureMean = root.GetProperty("temperature_2m_mean").EnumerateArray().ElementAt(i).GetSingle();
	stringBuilder.Append("<p> Temperature Mean : " + temperatureMean + "</p>");

	int cloudCoverMean = root.GetProperty("cloud_cover_mean").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<p> Cloud Cover Mean : " + cloudCoverMean + "</p>");

	int relativeHumidity = root.GetProperty("relative_humidity_2m_mean").EnumerateArray().ElementAt(i).GetInt32();
	stringBuilder.Append("<p> Relative Humidity Mean : " + relativeHumidity + "</p>");

	stringBuilder.Append("<hr>");

}

string reportFilePath = "report.html";
System.IO.File.WriteAllText(reportFilePath, stringBuilder.ToString());

System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
{
	FileName = reportFilePath,
	UseShellExecute = true
});

