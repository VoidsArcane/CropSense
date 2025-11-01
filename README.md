# CropSense
CropSense is a desktop tool for hobbyist gardeners that automatically collects and presents weather data in a visual form to be able to easily spot key conditions like frost and wind gusts. It uses the Open-Meteo API and its Geocoding API.

CropSense is run as an executable or from the console, reads a user-defined Config file and generates an HTML or CSV report.

CropSense also ships with a visual ConfigEditor for easily setting up the Config file.


## Download Standalone Apps (Windows 10/11)

Download the latest release (both apps and shared config):

[CropSense_Standalone.zip](https://github.com/VoidsArcane/CropSense/releases/tag/v1.0)



## How to Build from Source (Windows 10/11)

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2. Clone the repository:
	```bash
	git clone https://github.com/VoidsArcane/CropSense.git
   	cd CropSense
	```
3. Build the projects:
	```bash
	dotnet build CropSense/CropSense.csproj -c Release
	dotnet build ConfigEditor/ConfigEditor.csproj -c Release
	```
4. The executables will appear in the dist/ folder
5. Run the apps:
	```bash
	dotnet run -c Release --project CropSense/CropSense.csproj
	dotnet run -c Release --project ConfigEditor/ConfigEditor.csproj
	```

# Config Section Info

## General
| Field            | Format     | Description                                         |
|------------------|------------|-----------------------------------------------------|
| location         | text       | Location Name eg. Irvine, California                |
| forecast_days    | int (1-14) | Days of weather to be generated (incl. current day) |
| auto_open_report | true/false | Automatically opens report when it gets generated   |
| report_file_path | text       | Directory where generated reports should be placed  |
| report_type      | text       | Type of report to be generated (HTML or CSV)        |


## Include Variables
#### All variables can take true/false values, if true is set the variable will be included in report. At least one variable should be included
| Field                          | Unit     | Description                              | 
|--------------------------------|----------|------------------------------------------|
| weather_code                   | WMO code | Most severe weather condition of the day |
| temperature_2m_max             | °C       | Maximum daily temperature                |
| temperature_2m_min             | °C       | Minimum daily temperature                |
| temperature_2m_mean            | °C       | Average daily temperature                |
| sunrise                        | iso8601  | Sunrise time                             |
| sunset                         | iso8601  | Sunset time                              |
| daylight_duration              | seconds  | Daylight                                 |
| sunshine_duration              | seconds  | Daily Sunshine (Irradiance >=120 W/m^2)  |
| uv_index_max                   | index    | Maximum Daily UV Index                   |
| uv_index_clear_sky_max         | index    | Maximum Daily UV Index (clear sky)       |
| rain_sum                       | mm       | Sum of daily rain                        |
| showers_sum                    | mm       | Sum of daily showers                     |
| snowfall_sum                   | cm       | Sum of daily snowfall                    |
| precipitation_sum              | mm       | Sum of daily rain, showers and snowfall  |
| precipitation_hours            | hours    | Hours of precipitation                   |
| precipitation_probability_max  | %        | Maximum Precipitation Probability        |
| precipitation_probability_mean | %        | Average Precipitation Probability        |
| precipitation_probability_min  | %        | Minimum Precipitation Probability        |
| wind_speed_10m_max             | km/h     | Maximum daily wind speed                 |
| wind_speed_10m_mean            | km/h     | Average daily wind speed                 |
| wind_speed_10m_min             | km/h     | Minimum daily wind speed                 |
| wind_gusts_10m_max             | km/h     | Maximum daily wind gust speed            |
| wind_gusts_10m_min             | km/h     | Minimum daily wind gust speed            |
| wind_gusts_10m_mean            | km/h     | Average daily wind gust speed            |
| wind_direction_10m_dominant    | degrees  | Dominant wind direction                  |
| shortwave_radiation_sum        | MJ/m^2   | Sum of Solar radiation                   |
| cloud_cover_mean               | %        | Average cloud coverage                   |
| cloud_cover_max                | %        | Maximum cloud coverage                   |
| cloud_cover_min                | %        | Minimum cloud coverage                   |
| et0_fao_evapotranspiration     | mm       | Evapotranspiration                       |
| relative_humidity_2m_mean      | %        | Average relative humidity                |
| relative_humidity_2m_max       | %        | Maximum relative humidity                |
| relative_humidity_2m_min       | %        | Minimum relative humidity                |

