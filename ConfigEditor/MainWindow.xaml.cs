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
using System.Windows.Forms;

namespace ConfigEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private Dictionary<string, Dictionary<string, string>> config;

	public MainWindow()
	{
		InitializeComponent();

		// Parse Config
		string iniPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

		var lines = File.ReadAllLines(iniPath);
		string section = "";
		config = new Dictionary<string, Dictionary<string, string>>();

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

		// Set UI Field Values to current Config Values
		location.Text = config["general"]["location"];
		forecast_days.SelectedIndex = int.Parse(config["general"]["forecast_days"]) - 1;
		auto_open_report.IsChecked = bool.Parse(config["general"]["auto_open_report"]);
		report_file_path.Text = config["general"]["report_file_path"];
		report_type.SelectedIndex = config["general"]["report_type"].ToUpper() == "HTML" ? 0 : 1;

		Dictionary<string, string> variables = config["include_variables"];

		foreach (KeyValuePair<string, string> entry in variables)
		{
			System.Windows.Controls.CheckBox variable = (System.Windows.Controls.CheckBox)this.FindName(entry.Key);
			variable.IsChecked = bool.Parse(entry.Value);
		}

	}

	private void SaveConfig(object sender, RoutedEventArgs e)
	{
		config["general"]["location"] = location.Text;
		config["general"]["forecast_days"] = forecast_days.SelectedIndex + 1 + "";
		config["general"]["auto_open_report"] = auto_open_report.IsChecked + "";
		config["general"]["report_file_path"] = report_file_path.Text;
		config["general"]["report_type"] = report_type.SelectedIndex == 0 ? "HTML" : "CSV";

		Dictionary<string, string> variables = config["include_variables"];

		foreach (KeyValuePair<string, string> entry in variables)
		{
			System.Windows.Controls.CheckBox variable = (System.Windows.Controls.CheckBox)this.FindName(entry.Key);
			variables[entry.Key] = variable.IsChecked != null ? variable.IsChecked + "" : "false";
		}

		string iniPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
		StringBuilder stringBuilder = new StringBuilder();

		foreach (KeyValuePair<string, Dictionary<string, string>> section in config)
		{
			stringBuilder.AppendLine($"[{section.Key}]");
			foreach (KeyValuePair<string, string> entry in config[section.Key])
			{
				stringBuilder.AppendLine($"{entry.Key} = {entry.Value}");
			}
			stringBuilder.AppendLine();
		}

		System.IO.File.WriteAllText(iniPath, stringBuilder.ToString());
	}

	private void GetOutputPath(object sender, RoutedEventArgs e)
	{
		using (var dialog = new FolderBrowserDialog())
		{
			dialog.Description = "Select a folder";

			DialogResult result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
			{
				string folderPath = dialog.SelectedPath;
				// Set folder path in your UI field
				report_file_path.Text = folderPath;
			}
		}
	}
}