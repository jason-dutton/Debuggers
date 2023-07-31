using System.Net;
using System.Text.Json;

namespace Api.Repository;

public class SolarScoreRepository
{
    private readonly string express = "http://localhost:3333";

    public SolarScoreRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }

    public async Task<string> GetMapBoxApiKey()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/solarscore/mapboxkey");
        var response = await client.SendAsync(request);
        // response.EnsureSuccessStatusCode();
        // Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new Exception("Error getting mapbox key");
        }
    }

    public async Task<string> GetSolarScore(Coordinates coordinates)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/solarscore/getimages/"
        );
        var content = new StringContent(
            "{\r\n    \"latitude\": "
                + coordinates.latitude
                + ",\r\n    \"longitude\": "
                + coordinates.longitude
                + "\r\n}",
            null,
            "application/json"
        );
        request.Content = content;
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return "Got solar Score";
        }
        else
        {
            throw new Exception("Error getting solar score");
        }
    }

    //Get sun times
    public async Task<string> GetSunTimes(Coordinates coordinates)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/solarscore/getsuntimes/"
        );
        var content = new StringContent(
            "{\r\n    \"latitude\": "
                + coordinates.latitude
                + ",\r\n    \"longitude\": "
                + coordinates.longitude
                + "\r\n}",
            null,
            "application/json"
        );
        request.Content = content;
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            string data = response.Content.ReadAsStringAsync().Result;
            return data;

        }
        else
        {
            throw new Exception("Error getting sun times");
        }
    }


}