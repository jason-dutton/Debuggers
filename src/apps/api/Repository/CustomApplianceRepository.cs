using System.Net;
using System.Text.Json;

namespace Api.Repository;

public class CustomApplianceRepository
{
    private string express = "http://localhost:3333";

    // Constructor

    public CustomApplianceRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }

    // Get All Custom Appliances
    public async Task<List<CustomAppliance>> GetAllCustomAppliances()
    {
        //$"{express}/api/customAppliances/all"
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{express}/api/customAppliance/all");
        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (body is null)
            {
                return new List<CustomAppliance>();
            }
            List<CustomAppliance> customAppliances = JsonSerializer.Deserialize<List<CustomAppliance>>(body)!;
            return customAppliances!;
        }
        //Bad Request
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception("Bad Request");
        }
        else
        {
            throw new Exception("Unable to retrieve custom appliances");
        }
    }

    // Create custom appliance
    public async Task<CustomAppliance> CreateCustomAppliance(string type, string model, int powerUsage)
    {
        Console.WriteLine("Creating custom appliance");
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{express}/api/customAppliance/create");
        var body = JsonSerializer.Serialize(new
        {
            type = type,
            model = model,
            powerUsage = powerUsage
        });
        request.Content = new StringContent(body, null, "application/json");
        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            CustomAppliance cp = new CustomAppliance();
            cp.type = type;
            cp.model = model;
            cp.powerUsage = powerUsage;
            cp.customApplianceId = -1;
            return cp;
        }
        //Bad Request
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception("Bad Request");
        }
        else
        {
            throw new Exception("Unable to create custom appliance");
        }
    }

    // Delete custom appliance
    public async Task<bool> DeleteCustomAppliance(int customApplianceId)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{express}/api/customAppliance/delete/{customApplianceId}");
        var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }
        //Bad Request
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return false;
        }
        else
        {
            throw new Exception("Unable to delete custom appliance");
        }
    }

}