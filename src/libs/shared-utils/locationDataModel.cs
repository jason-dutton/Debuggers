﻿// Import json

using System;
using System.Text.Json;
namespace SharedUtils;

public class locationDataModel
{
    private string? API_PORT = Environment.GetEnvironmentVariable("API_PORT");

    /// <summary>
    /// <para>Gets the elevation data for the given location.</para>
    /// <paramref name="latitude"/> The latitude of the location.
    /// <paramref name="longitude"/> The longitude of the location.
    /// </summary>
    public async Task<string> getElevationData(double latitude, double longitude) {
        string url = $"https://api.globalsolaratlas.info/data/horizon?loc={latitude},{longitude}";
        Console.WriteLine("URL: " + url);
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// <list type="number">
    ///     <item>Creates a new row in the database for the location data. </item>
    ///     <item>Calls the python API to get the solar irradiation data for that location.</item>
    /// </list>
    /// <paramref name="latitude"/> The latitude of the current location.
    /// <paramref name="longitude"/> The longitude of the current location.
    /// <paramref name="daylightHours"/> The daylight hours for the current location.
    /// <paramref name="image"/> The image of the current location.
    /// <paramref name="location"/> The name of the current location.
    /// </summary>

    public async Task CreateLocationData(double latitude, double longitude, float daylightHours, string image, string location) 
    {
        string  elevationData = await getElevationData(latitude, longitude);
        Console.WriteLine("Elevation data: " + elevationData);
        var numYears = 3;
        var numDaysPerYear = 48;
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, API_PORT + "/locationData/create");
        var content = new StringContent("{\r\n    \"latitude\": \"" 
                                    + latitude 
                                    + "\",\r\n    \"longitude\": \"" 
                                    + longitude 
                                    + "\",\r\n    \"location\": \"" 
                                    + location 
                                    + "\",\r\n    \"daylightHours\" : \"" 
                                    + daylightHours 
                                    + "\",\r\n    \"image\": \"" 
                                    + image 
                                    + "\" ,\r\n    \"elevationData\": \"" 
                                    + elevationData +"\"\r\n}", null, "application/json");
        
        request.Content = content;
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            client = new HttpClient();
            request = new HttpRequestMessage(HttpMethod.Get, API_PORT + "/locationData/getSolarIrradiationData");
            content = new StringContent("{\r\n    \"latitude\": "+ latitude + ",\r\n    \"longitude\": " + longitude + ",\r\n    \"numYears\": "+ numYears + ",\r\n    \"numDaysPerYear\": "+ numDaysPerYear +"\r\n}", null, "application/json");
            request.Content = content;
            response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully called python file");

            } else {
                Console.WriteLine("Failed to call python file");
            }
        } else {
            Console.WriteLine("Failed to create row table in database for solarIrradiation data");
        }
    }

    /// <summary>
    /// <para>Calls the weather visualcrossing api to get quick initial data.</para>
    /// <paramref name="latitude"/> The latitude of the current location.
    /// <paramref name="longitude"/> The longitude of the current location.
    /// <returns> averageSunlightHours and averageSolarIrradiation</returns>
    /// </summary>
    public async Task<float[]> getInitialData(double latitude, double longitude)
    {
        var data = new List<WeatherData>();
        // Initial api key to get the last 15 day's of information:
        var client = new HttpClient();
        var apiKey = Environment.GetEnvironmentVariable("VISUAL_CROSSING_WEATHER_API_KEY");
        var apiUrl = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{latitude},{longitude}?unitGroup=metric&include=days&key={apiKey}&elements=sunrise,sunset,temp,solarenergy,solarradiation,datetime";
        data.Add(await FetchWeatherDataAsync(client, apiUrl));
        
        //  11 other calls to get the last 10 month's data
        var currentMonth = DateTime.Now.Month;
        var tasks = new List<Task<WeatherData>>();

        for (var i = 1; i < 12; i++)
        {
            var month = currentMonth - i;
            if (month < 1)
            {
                month = 12 + month;
            }
            var year = DateTime.Now.Year;
            if (month > currentMonth)
            {
                year = year - 1;
            }
            var monthString = month.ToString();
            if (month < 10)
            {
                monthString = "0" + monthString;
            }
            client = new HttpClient();
            apiUrl = $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{latitude},{longitude}/{year}-{monthString}-15/{year}-{monthString}-15?unitGroup=metric&include=days&key={apiKey}&elements=sunrise,sunset,temp,solarenergy,solarradiation,datetime";

            // Asynchronously fetch the data and add the task to the list
            tasks.Add(FetchWeatherDataAsync(client, apiUrl));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Extract the results from completed tasks
        var completedResults = tasks
            .Where(task => task.Status == TaskStatus.RanToCompletion)
            .Select(task => task.Result)
            .ToList();

        // Now, completedResults contains the fetched data for all 10 months
        foreach (var result in completedResults)
        {
            if (result != null)
            {
                data.Add(result);
            }
        }

        // Get averageSolarIrradiation and daylightHours from the data
        var averageSunlightHours = 0f;
        var averageSolarIrradiation = 0.0;
        for(var i = 0; i < data.Count; i++) {
            if(data[i] == null || data[i].days == null) {
                Console.WriteLine("Data is null");
                continue;
            }

            var averageSolarIrradiationMonth = 0.0;
            var monthSunlightHours = 0f;

            #pragma warning disable CS8602
            for(var j = 0; j < data[i].days.Count; j++) {
                var sunrise = data[i].days[j].sunrise;
                var sunset = data[i].days[j].sunset;
                var sunriseSplit = sunrise.Split(":");
                var sunsetSplit = sunset.Split(":");
                var sunriseTotalHours =  float.Parse(sunriseSplit[0]) + (float.Parse(sunriseSplit[1]) / 60);
                var sunsetTotalHours = float.Parse(sunsetSplit[0]) + (float.Parse(sunsetSplit[1]) / 60);
                var daylightHours = sunsetTotalHours - sunriseTotalHours;
                monthSunlightHours += daylightHours;

                averageSolarIrradiationMonth += data[i].days[j].solarradiation;
            }
            monthSunlightHours = monthSunlightHours / data[i].days.Count;
            averageSunlightHours += monthSunlightHours;

            averageSolarIrradiationMonth = averageSolarIrradiationMonth / data[i].days.Count;
            averageSolarIrradiation += averageSolarIrradiationMonth;
        }
        averageSunlightHours = averageSunlightHours / data.Count;
        averageSunlightHours = (float)Math.Round(averageSunlightHours, 2);
        averageSolarIrradiation = averageSolarIrradiation / data.Count;

        return new float[] { averageSunlightHours, (float)averageSolarIrradiation };
    }

    /// <summary>
    /// <para>Fetches the weather data from the visualcrossing api.</para>
    /// <paramref name="client"/> The http client.
    /// <paramref name="apiUrl"/> The url to the api.
    /// </summary>
    #pragma warning disable CS8603
    private async Task<WeatherData> FetchWeatherDataAsync(HttpClient client, string apiUrl)
    {
        try
        {
            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (result != null && result.Length > 0 && result[0] == '{')
                {
                    return JsonSerializer.Deserialize<WeatherData>(result);
                }
            }
            else
            {
                Console.WriteLine($"Failed to call the API. Status code: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }

        // If any error occurred, return null or an appropriate default value
        return null;
    }
}
