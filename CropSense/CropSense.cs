using System.Text.Json;
using System.Text;
using System.Diagnostics;
using System.Net;



////////////////////////////
///    PARSING CONFIG   ///
internal class Program
{
	private static async Task Main(string[] args)
	{
		///////////////////////////
		string configPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).FullName, "config.ini");


		if (!File.Exists(configPath))
		{
			ExitOnError($"Config File Not Found. No Config in {configPath}");
		}

		var lines = File.ReadAllLines(configPath);
		string section = "";
		var config = new Dictionary<string, Dictionary<string, string>>();


		foreach (var line in lines)
		{
			string trimmed = line.Trim();
			if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
			{
				section = trimmed[1..^1].Trim();
				config[section] = new Dictionary<string, string>();
			}
			else if (trimmed.Contains('=') && !trimmed.StartsWith(";") && config.Count != 0)
			{
				var parts = trimmed.Split('=', 2);
				config[section][parts[0].Trim()] = parts[1].Trim();
			}
		}


		///////////////////////////////
		///    VALIDATING CONFIG    ///
		///////////////////////////////


		// Validating Sections
		if (!config.ContainsKey("general"))
		{
			ExitOnError("Config Does not contain a general section");
		}

		if (!config.ContainsKey("include_variables"))
		{
			ExitOnError("Config does not contain a include_variables section");
		}

		Dictionary<string, string> generalSettings = config["general"];
		Dictionary<string, string> variableSettings = config["include_variables"];


		// Validating Fields
		StringBuilder fieldsMissing = new StringBuilder();

		if (!generalSettings.ContainsKey("location")) fieldsMissing.Append("location, ");
		if (!generalSettings.ContainsKey("forecast_days")) fieldsMissing.Append("forecast_days, ");
		if (!generalSettings.ContainsKey("auto_open_report")) fieldsMissing.Append("auto_open_report, ");
		if (!generalSettings.ContainsKey("report_file_path")) fieldsMissing.Append("report_file_path, ");
		if (!generalSettings.ContainsKey("report_type")) fieldsMissing.Append("report_type");

		if (fieldsMissing.Length > 0)
		{
			ExitOnError($"Config: following fields not found\n[{fieldsMissing}]");
		}

		// Validating Values
		StringBuilder valuesMissing = new StringBuilder();

		if (string.IsNullOrWhiteSpace(generalSettings["location"])) valuesMissing.Append("location, ");
		if (string.IsNullOrWhiteSpace(generalSettings["forecast_days"])) valuesMissing.Append("forecast_days, ");
		if (string.IsNullOrWhiteSpace(generalSettings["auto_open_report"])) valuesMissing.Append("auto_open_report, ");
		if (string.IsNullOrWhiteSpace(generalSettings["report_type"])) valuesMissing.Append("report_type");

		if (valuesMissing.Length > 0)
		{
			ExitOnError($"Config: following fields have no value \n[{valuesMissing}]");
		}

		// Validating Value Format

		if (!int.TryParse(generalSettings["forecast_days"], out int ForecastDays))
		{
			ExitOnError($"forecast_days could not be parsed (value: {generalSettings["forecast_days"]}, expected: int)");
		}

		if (!bool.TryParse(generalSettings["auto_open_report"], out bool AutoOpenReport))
		{
			ExitOnError($"auto_open_report value could not be parsed (value: {generalSettings["auto_open_report"]} , expected: true/false)");
		}

		// Validating value correctness
		if (ForecastDays < 1 || ForecastDays > 14)
		{
			ExitOnError("forecast_days is outside the range of 1-14");
		}

		if (generalSettings["report_type"] != "HTML" && generalSettings["report_type"] != "CSV")
		{
			ExitOnError($"Report Type has incorrect value (value: {generalSettings["report_type"]} , expected: HTML/CSV)");
		}

		// Validating Variables Included
		if (variableSettings.Count == 0)
		{
			ExitOnError("include variables has no fields");
		}

		List<string> variablesIncluded = new List<string>();

		foreach (KeyValuePair<string, string> entry in variableSettings)
		{
			if (bool.TryParse(entry.Value, out bool val))
			{
				if (val) variablesIncluded.Add(entry.Key);
			}
		}

		if (variablesIncluded.Count == 0)
		{
			ExitOnError("No Variables selected to be included in Config file");
		}


		//////////////////////////////////////
		/// FETCHING LOCATION COORDINATES  ///
		//////////////////////////////////////
		string location = Uri.EscapeDataString(generalSettings["location"]);

		using HttpClient client = new HttpClient();
		HttpResponseMessage response = await client.GetAsync($"https://geocoding-api.open-meteo.com/v1/search?name={location}&count=1&language=en");
		try
		{
			response.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException exception)
		{
			ExitOnError($"Fetching Location Coordinates failed.\nException: {exception} ");
		}

		string locationData = await response.Content.ReadAsStringAsync();

		JsonDocument responseDocument;
		try
		{
			responseDocument = JsonDocument.Parse(locationData);
		}
		catch (JsonException ex)
		{
			ExitOnError($"Failed to parse location JSON: {ex.Message}");
			throw;
		}

		using JsonDocument locationJSON = responseDocument;



		if (!locationJSON.RootElement.TryGetProperty("results", out JsonElement coordinates))
		{
			ExitOnError("Location not found. Please try a simpler name.");
		}

		if (!coordinates.EnumerateArray().Any())
		{
			ExitOnError("Coordinate array has no Items.");
		}

		if (coordinates.EnumerateArray().Count() > 1)
		{
			ExitOnError("Location too vague, add specifier eg. 'US' etc. ");
		}

		coordinates = coordinates.EnumerateArray().ElementAt(0);

		if (!coordinates.TryGetProperty("latitude", out JsonElement latitude_result))
		{
			ExitOnError("'latitude' property doesn't exist.");
		}

		if (!latitude_result.TryGetDouble(out double latitude))
		{
			ExitOnError("'latitude' property doesn't contain a double.");
		}

		if (!coordinates.TryGetProperty("longitude", out JsonElement longitude_result))
		{
			ExitOnError("'longitude' property doesn't exist.");
		}

		if (!longitude_result.TryGetDouble(out double longitude))
		{
			ExitOnError("'longitude' property doesn't contain a double.");
		}


		///////////////////////////////
		/// FETCHING FORECAST DATA  ///
		///////////////////////////////

		string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily={Uri.EscapeDataString(string.Join(",", variablesIncluded))}&timezone=auto&forecast_days={ForecastDays}";

		response = await client.GetAsync(url);
		try
		{
			response.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException exception)
		{
			ExitOnError($"Fetching Forecast Data failed.\nException: {exception} ");
		}

		string json = await response.Content.ReadAsStringAsync();

		try
		{
			responseDocument = JsonDocument.Parse(json);
		}
		catch (JsonException ex)
		{
			ExitOnError($"Failed to parse forecast JSON: {ex.Message}");
		}

		using JsonDocument doc = responseDocument;

		if (!doc.RootElement.TryGetProperty("daily", out JsonElement root))
		{
			ExitOnError("'daily' property doesn't exist");
		}

		// Extracting Variables
		Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
		foreach (JsonProperty prop in root.EnumerateObject())
		{
			List<string> tempList = new List<string>();
			foreach (JsonElement element in prop.Value.EnumerateArray())
			{
				tempList.Add(element.ToString());
			}

			data.Add(prop.Name, tempList);
		}


		/////////////////////////
		/// GENERATING REPORT ///
		/////////////////////////

		string ReportType = generalSettings["report_type"].Trim().ToUpperInvariant();
		StringBuilder stringBuilder = new StringBuilder();

		if (ReportType == "CSV")
		{
			int keyCount = data.Keys.Count;
			for (int i = 0; i < keyCount; i++)
			{
				string key = data.Keys.ElementAt(i).Replace("\"", "\"\"");
				if (key.Contains(',') || key.Contains('"') || key.Contains('\n') || key.Contains('\r'))
					key = $"\"{key}\"";

				stringBuilder.Append(key);
				if (!(i == keyCount - 1)) stringBuilder.Append(',');
			}
			stringBuilder.AppendLine();

			int rowCount = data.First().Value.Count;
			for (int row = 0; row < rowCount; row++)
			{
				int keyIndex = 0;
				foreach (string key in data.Keys)
				{
					string value = data[key].ElementAt(row).Replace("\"", "\"\"");
					if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
						value = $"\"{value}\"";
					stringBuilder.Append(value);
					if (!(keyIndex == keyCount - 1)) stringBuilder.Append(',');
					keyIndex++;
				}
				stringBuilder.AppendLine();
			}

		}

		if (ReportType == "HTML")
		{
			stringBuilder.AppendLine("<!DOCTYPE html>\n<head>\n<meta charset=\"UTF-8\"/>");
			stringBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"/>");
			stringBuilder.AppendLine("<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\">");
			string iconPath = Path.Combine(AppContext.BaseDirectory, "assets", "app_icon.ico");

			stringBuilder.AppendLine($"<link rel=\"icon\" type=\"image/x-icon\" href={WebUtility.HtmlEncode(iconPath)}>");
			stringBuilder.AppendLine("<title>CropSense</title>");
			stringBuilder.AppendLine("</head>\n<body>");
			stringBuilder.AppendLine("<table class=\"table table-striped table-hover table-bordered\">");
			stringBuilder.AppendLine("<thead class=\"table-dark\">");

			foreach (string key in data.Keys)
			{
				stringBuilder.AppendLine($"<th>{WebUtility.HtmlEncode(key)}</th>");
			}

			stringBuilder.AppendLine("</thead>\n<tbody>");

			for (int i = 0; i < data.First().Value.Count; i++)
			{
				stringBuilder.Append("<tr>");
				foreach (List<string> list in data.Values)
				{
					stringBuilder.AppendLine($"<td>{WebUtility.HtmlEncode(list.ElementAt(i))}</td>");
				}
				stringBuilder.AppendLine("</tr>");
			}
			stringBuilder.AppendLine("</tbody>\n</table>");
		}


		///////////////////////////////
		/// WRITING REPORT TO FILE  ///
		///////////////////////////////

		if (!root.TryGetProperty("time", out JsonElement time_result))
		{
			ExitOnError("'time' property doesn't exist");
		}

		if (time_result.EnumerateArray().Count() <= 0)
		{
			ExitOnError("'time' property doesn't contain elements");
		}

		string ReportFilePath = generalSettings["report_file_path"];

		if (!Directory.Exists(generalSettings["report_file_path"]))
		{
			ReportFilePath = AppDomain.CurrentDomain.BaseDirectory;
		}

		string date = time_result.EnumerateArray().ElementAt(0).GetString() ?? "Unkown_Date";
		foreach (char c in Path.GetInvalidFileNameChars())
			date = date.Replace(c, '_');

		string reportFileName = $"report_{date}.{ReportType}";
		string OutputFilePath = Path.Combine(ReportFilePath, reportFileName);


		try
		{
			File.WriteAllText(OutputFilePath, stringBuilder.ToString());
		}
		catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is ArgumentException)
		{
			ExitOnError($"Failed to write report to '{OutputFilePath}': {ex.Message}");
		}


		if (AutoOpenReport)
		{
			Process.Start(new ProcessStartInfo { FileName = OutputFilePath, UseShellExecute = true });
		}

		Environment.Exit(0);
	}

	private static void ExitOnError(string errorMessage)
	{
		Console.Error.WriteLine(errorMessage);
		Console.WriteLine("Press any key to exit...");
		Console.ReadLine();
		Environment.Exit(1);
	}


}