using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;


namespace ConfigEditor;

public partial class MainWindow : Window
{
	string CONFIG_PATH = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
	private Dictionary<string, string> generalSettings;
	private Dictionary<string, string> variableSettings;

	public MainWindow()
	{
		InitializeComponent();

		ParseConfig();

		if (generalSettings.ContainsKey("location"))
		{
			location.Text = generalSettings["location"].Trim();
		}

		if (generalSettings.ContainsKey("forecast_days"))
		{
			if (int.TryParse(generalSettings["forecast_days"], out int ForecastDays))
			{
				forecast_days.SelectedIndex = (ForecastDays > 0) ? ForecastDays - 1 : 0;
			}
		}

		if (generalSettings.ContainsKey("auto_open_report"))
		{
			if (bool.TryParse(generalSettings["auto_open_report"], out bool AutoOpenReport))
			{
				auto_open_report.IsChecked = AutoOpenReport;
			}
		}

		if (generalSettings.ContainsKey("report_file_path"))
		{
			report_file_path.Text = generalSettings["report_file_path"].Trim();
		}


		if (generalSettings.ContainsKey("report_type"))
		{
			report_type.SelectedIndex = generalSettings["report_type"].Trim().ToUpper() == "CSV" ? 1 : 0;
		}


		foreach (KeyValuePair<string, string> entry in variableSettings)
		{
			CheckBox variable = (CheckBox)this.FindName(entry.Key);

			if (variable != null)
			{
				if (bool.TryParse(entry.Value, out bool BoolValue))
				{
					variable.IsChecked = BoolValue;
				}
				else
				{
					variable.IsChecked = false;
				}
			}
		}

	}

	private void SaveConfig(object sender, RoutedEventArgs e)
	{
		generalSettings["location"] = location.Text;
		generalSettings["forecast_days"] = forecast_days.SelectedIndex + 1 + "";
		generalSettings["auto_open_report"] = (auto_open_report.IsChecked + "").ToLowerInvariant();
		generalSettings["report_file_path"] = report_file_path.Text;
		generalSettings["report_type"] = report_type.SelectedIndex == 0 ? "HTML" : "CSV";

		foreach (KeyValuePair<string, string> entry in variableSettings)
		{
			CheckBox variable = (CheckBox) FindName(entry.Key);
			variableSettings[entry.Key] = variable.IsChecked != null ? ("" + variable.IsChecked).ToLowerInvariant() : "false";
		}

		StringBuilder stringBuilder = new StringBuilder();

		stringBuilder.AppendLine("[general]");
		foreach (KeyValuePair<string, string> setting in generalSettings)
		{
			stringBuilder.AppendLine($"{setting.Key} = {setting.Value}");
		}


		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[include_variables]");
		foreach (KeyValuePair<string, string> setting in variableSettings)
		{
			stringBuilder.AppendLine($"{setting.Key} = {setting.Value}");
		}

		File.WriteAllText(CONFIG_PATH, stringBuilder.ToString());
		System.Windows.MessageBox.Show ("Config Saved", "Saved", MessageBoxButton.OK, MessageBoxImage.None);
	}

	private void GetOutputPath(object sender, RoutedEventArgs e)
	{
		using var dialog = new FolderBrowserDialog();
		dialog.Description = "Select a folder";

		DialogResult result = dialog.ShowDialog();
		if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
		{
			report_file_path.Text = dialog.SelectedPath;
		}
	}

	private void ParseConfig()
	{
		if (!File.Exists(CONFIG_PATH))
		{
			DisplayFatalErrorMessage("Config File not found in /dist folder");
		}

		var lines = File.ReadAllLines(CONFIG_PATH);
		string section = "";
		Dictionary<string, Dictionary<string, string>> config = [];

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

		if (!config.ContainsKey("general"))
		{
			DisplayFatalErrorMessage("Config File doesn't contain 'general' section");
		}

		if (!config.ContainsKey("include_variables"))
		{
			DisplayFatalErrorMessage("Config File doesn't contain 'include_variables' section");
		}

		generalSettings = config["general"];
		variableSettings = config["include_variables"];
	}

	private void DisplayFatalErrorMessage(string ErrorMessage)
	{
		System.Windows.MessageBox.Show (ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		Application.Current.Shutdown();
		return;
	}
	
}