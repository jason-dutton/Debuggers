using System.Net;
using System.Text.Json;

namespace Api.Repository;

//Create class
public class SystemsRepository
{
  public async Task<List<Systems>> GetAllSystems()
  {
    try
    {
      var client = new HttpClient();
      var request = new HttpRequestMessage(
          HttpMethod.Get,
          "http://localhost:3333/api/system/all"
      );
      var response = await client.SendAsync(request);

      if (response.IsSuccessStatusCode)
      {
        var data = await response.Content.ReadAsStringAsync();
        // Console.WriteLine(data);
        var systems = JsonSerializer.Deserialize<List<Systems>>(data);
        if (systems != null)
        {
          return systems;
        }
        return new List<Systems>();
      }
      else
      {
        //return empty list
        Console.WriteLine("Error");
        return new List<Systems>();
      }
    }
    catch (Exception e)
    {
      throw new Exception("Database Connection Error");
    }
  }

  public async Task<Systems> createSystems(
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
      var request = new HttpRequestMessage(
          HttpMethod.Post,
          "http://localhost:3333/api/system/create"
      );
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
        Systems sys = new Systems();
        sys.systemId = -1;
        sys.inverterOutput = inverterOutput;
        sys.numberOfPanels = numberOfPanels;
        sys.batterySize = batterySize;
        sys.numberOfBatteries = numberOfBatteries;
        sys.solarInput = solarInput;
        return sys;
      }
      else
      {
        throw new Exception("Error creating system");
      }
    }
    catch (Exception e)
    {
      throw new Exception("Database Error: " + e.Message);
    }
  }

  public async Task<Systems> updateSystems(
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
          "http://localhost:3333/api/system/update/" + systemId
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
        return sys;
      }
      else
      {
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        throw new Exception("Error updating system");
      }
    }
    catch (Exception e)
    {
      throw new Exception("Database Error: " + e.Message);
    }
  }

  //Delete system
  public async Task<bool> deleteSystems(int systemId)
  {
    try
    {
      var client = new HttpClient();
      var request = new HttpRequestMessage(
          HttpMethod.Delete,
          "http://localhost:3333/api/system/delete/" + systemId
      );
      var response = await client.SendAsync(request);
      if (response.IsSuccessStatusCode)
      {
        return true;
      }
      if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return false;
      }
      else
      {
        throw new Exception("Error deleting system");
      }
    }
    catch (Exception e)
    {
      throw new Exception("Database Error: " + e.Message);
    }
  }

  //get system by id
  public async Task<Systems> getSystemById(int systemId)
  {
    try
    {
      var client = new HttpClient();
      var request = new HttpRequestMessage(
          HttpMethod.Get,
          "http://localhost:3333/api/system/" + systemId
      );
      var response = await client.SendAsync(request);

      if (response.IsSuccessStatusCode)
      {
        var data = await response.Content.ReadAsStringAsync();
        //Console.WriteLine(data);
        var system = JsonSerializer.Deserialize<Systems>(data);
        if (system != null)
        {
          return system;
        }
        return new Systems();
      }
      else
      {
        Console.WriteLine("Error");
        throw new Exception("Error getting system");
      }
    }
    catch (Exception e)
    {
      throw new Exception("Database Error: " + e.Message);
    }
  }
}
