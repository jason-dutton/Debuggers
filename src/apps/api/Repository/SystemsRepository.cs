using System.Net;
using System.Text.Json;

namespace Api.Repository;

//Create class
public class SystemsRepository
{
    private string express = "http://localhost:3333";

    public SystemsRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }

    public async Task<List<Systems>> GetAllSystems()
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/system/all");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(data);
                var systems = JsonSerializer.Deserialize<List<Systems>>(data);
                if (systems != null)
                {
                    Console.WriteLine(".NET: get all systems success");
                    return systems;
                }

                Console.WriteLine(".NET: system is null error");
                return new List<Systems>();
            }
            else
            {
                //return empty list
                Console.WriteLine(".NET: Database Connection Error in function GetAllSystems");
                return new List<Systems>();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Connection Error");
        }
    }

    public async Task<Systems> CreateSystems(
        int inverterOutput,
        int numberOfPanels,
        int batterySize,
        int numberOfBatteries,
        int solarInput
    )
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, express + "/api/system/create");
            var content = new StringContent(
                "{\r\n    \"inverterOutput\" : "
                    + inverterOutput
                    + ",\r\n    \"numberOfPanels\" : "
                    + numberOfPanels
                    + ",\r\n    \"batterySize\" : "
                    + batterySize
                    + ",\r\n    \"numberOfBatteries\" : "
                    + numberOfBatteries
                    + ",\r\n    \"solarInput\" : "
                    + solarInput
                    + "\r\n}",
                null,
                "application/json"
            );
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var system = JsonSerializer.Deserialize<Systems>(data);
                Systems sys = new Systems();
                sys.systemId = system!.systemId;
                sys.inverterOutput = inverterOutput;
                sys.numberOfPanels = numberOfPanels;
                sys.batterySize = batterySize;
                sys.numberOfBatteries = numberOfBatteries;
                sys.solarInput = solarInput;

                Console.WriteLine(".NET: system created successfully");
                return sys;
            }
            else
            {
                Console.WriteLine(".NET: Error creating system");
                throw new Exception("Error creating system");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }

    public async Task<Systems> UpdateSystems(
        int systemId,
        int inverterOutput,
        int numberOfPanels,
        int batterySize,
        int numberOfBatteries,
        int solarInput
    )
    {
        try
        {
            Console.WriteLine("Updating system from the API");
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                express + "/api/system/update/" + systemId
            );
            var content = new StringContent(
                "{\r\n\"inverterOutput\" : "
                    + inverterOutput
                    + ",\r\n"
                    + "\"numberOfPanels\" : "
                    + numberOfPanels
                    + ",\r\n"
                    + "\"batterySize\" : "
                    + batterySize
                    + ",\r\n"
                    + "\"numberOfBatteries\" : "
                    + numberOfBatteries
                    + ",\r\n"
                    + "\"solarInput\" : "
                    + solarInput
                    + "\r\n}",
                null,
                "application/json"
            );
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Systems sys = new Systems();
                sys.systemId = systemId;
                sys.inverterOutput = inverterOutput;
                sys.numberOfPanels = numberOfPanels;
                sys.batterySize = batterySize;
                sys.numberOfBatteries = numberOfBatteries;
                sys.solarInput = solarInput;

                Console.WriteLine(".NET: system updated successfully");
                return sys;
            }
            else
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Console.WriteLine(".NET: Error updating system");
                throw new Exception("Error updating system");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }

    //Delete system
    public async Task<bool> DeleteSystems(int systemId)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                express + "/api/system/delete/" + systemId
            );
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(".NET: System deleted");
                return true;
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine(".NET: System not found");
                return false;
            }
            else
            {
                Console.WriteLine(".NET: Error deleting system");
                throw new Exception("Error deleting system");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }

    //get system by id
    public async Task<Systems> GetSystemById(int systemId)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/system/" + systemId
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(data);
                var system = JsonSerializer.Deserialize<Systems>(data);
                if (system != null)
                {
                    Console.WriteLine(".NET: system found by id");
                    return system;
                }
                Console.WriteLine(".NET: System not found");
                return new Systems();
            }
            else
            {
                Console.WriteLine(".NET: Error getting system");
                throw new Exception("Error getting system");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }
}
