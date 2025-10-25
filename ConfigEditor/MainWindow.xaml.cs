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

namespace ConfigEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}


	/*

	[general]
	location = Nea Smyrni, Greece
	forecast_days = 7
	auto_open_report = true
	report_file_path = 
	report_type = html


	[include_variables]
	weather_code = false
	temperature_2m_max = true
	temperature_2m_min = true
	temperature_2m_mean = false
	sunrise = false
	sunset = false
	daylight_duration = false
	sunshine_duration = false
	uv_index_max = false
	uv_index_clear_sky_max = false
	rain_sum = true
	showers_sum = false
	snowfall_sum = true
	precipitation_sum = true
	precipitation_hours = false
	precipitation_probability_max = false
	wind_speed_10m_max = false
	wind_gusts_10m_max = true
	wind_direction_10m_dominant = false
	shortwave_radiation_sum = true
	cape_mean = false
	cape_max = false
	cape_min = false
	cloud_cover_mean = false
	cloud_cover_max = false
	cloud_cover_min = false
	dew_point_2m_mean = false
	dew_point_2m_max = false
	dew_point_2m_min = false
	et0_fao_evapotranspiration = true
	growing_degree_days_base_0_limit_50 = false
	leaf_wetness_probability_mean = false
	precipitation_probability_mean = false
	precipitation_probability_min = false
	relative_humidity_2m_mean = false
	relative_humidity_2m_max = false 
	relative_humidity_2m_min = false
	snowfall_water_equivalent_sum = false
	pressure_msl_mean = false
	pressure_msl_max = false
	pressure_msl_min = false
	surface_pressure_mean = false
	surface_pressure_max = false
	surface_pressure_min = false
	updraft_max = false
	wind_gusts_10m_mean = false
	wind_speed_10m_mean = false
	wind_speed_10m_min = false
	wind_gusts_10m_min = false
	wet_bulb_temperature_2m_mean = false
	wet_bulb_temperature_2m_max = false
	wet_bulb_temperature_2m_min = false
	vapour_pressure_deficit_max = false

	*/


	private void SaveConfig(object sender, RoutedEventArgs e)
	{
		MessageBox.Show("Saved :)");
	}
	
	private void BrowseButton_Click(object sender, RoutedEventArgs e)
{
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.Filter = "Config files (*.txt;*.json)|*.txt;*.json|All files (*.*)|*.*";
    if (openFileDialog.ShowDialog() == true)
    {
        ReportFilePath.Text = openFileDialog.FileName;
    }
}
}