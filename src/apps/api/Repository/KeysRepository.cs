using System.Net;
using System.Text.Json;

namespace Api.Repository;

//Create class
public class KeysRepository
{
    private string express = "http://localhost:3333";

    public KeysRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }

    //Get all keys
    public async Task<List<KeyModel>> GetAllKeys()
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/key/all");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var systems = JsonSerializer.Deserialize<List<KeyModel>>(data);
                if (systems != null)
                {
                    Console.WriteLine(".NET: get all keys system");
                    return systems;
                }

                Console.WriteLine(".NET: system is null error");
                return new List<KeyModel>();
            }
            else
            {
                //return empty list
                Console.WriteLine(".NET: Database Connection Error in function GetAllKeys");
                return new List<KeyModel>();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Connection Error");
        }
    }

    //Get all business keys
    public async Task<List<KeyModel>> GetAllBusinessKeys()
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/key/allBusiness");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var systems = JsonSerializer.Deserialize<List<KeyModel>>(data);
                if (systems != null)
                {
                    Console.WriteLine(".NET: get all keys system");
                    return systems;
                }

                Console.WriteLine(".NET: system is null error");
                return new List<KeyModel>();
            }
            else
            {
                //return empty list
                Console.WriteLine(".NET: Database Connection Error in function GetAllBusinessKeys");
                return new List<KeyModel>();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Connection Error");
        }
    }

    public async Task<KeyModel> CreateBusinessKey(
        string owner,
        int remainingCalls,
        int suspended,
        string description,
        string location,
        string website,
        string phoneNumber,
        string email
    )
    {
        try
        {
            string APIKey = Guid.NewGuid().ToString();
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                express + "/api/key/createBusiness"
            );
            var content = new StringContent(
                "{\r\n    \"owner\": \""
                    + owner
                    + "\",\r\n    \"APIKey\" : \""
                    + APIKey
                    + "\",\r\n    \"remainingCalls\" : "
                    + remainingCalls
                    + ",\r\n    \"description\" : \""
                    + description
                    + "\",\r\n    \"location\" : \""
                    + location
                    + "\",\r\n    \"website\" : \""
                    + website
                    + "\",\r\n    \"phoneNumber\" : \""
                    + phoneNumber
                    + "\",\r\n    \"email\" : \""
                    + email
                    + "\"\r\n}",
                null,
                "application/json"
            );
            request.Content = content;
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                KeyModel ans = new KeyModel();
                ans.owner = owner;
                ans.APIKey = APIKey;
                ans.remainingCalls = remainingCalls;
                ans.suspended = suspended;
                ans.isBusiness = 1;
                ans.description = description;
                ans.location = location;
                ans.website = website;
                ans.phoneNumber = phoneNumber;
                ans.email = email;

                Console.WriteLine(".NET: api key created");
                return ans;
            }
            else
            {
                //return empty list
                Console.WriteLine(".NET: API key not created. Database Connection Error");
                return new KeyModel();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: API key not created. Database Connection Error: " + e.Message);
            throw new Exception("Database Connection Error");
        }
    }

    //Delete Key
    public async Task<bool> DeleteKeys(int id)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                express + "/api/key/delete/" + id
            );
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(".NET: API key deleted");
                return true;
            }
            //Check Status code
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine(".NET: API key not found");
                return false;
            }
            else
            {
                Console.WriteLine(".NET: " + response.StatusCode);
                throw new Exception("Could not delete Key");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }

    // Update Key
    public async Task<KeyModel> UpdateKeys(
        int id,
        string owner,
        string APIKey,
        int remainingCalls,
        int suspended
    )
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                express + "/api/key/update/" + id
            );
            var content = new StringContent(
                "{\r\n "
                    + "\"owner\" : \""
                    + owner
                    + "\",\r\n"
                    + "\"APIKey\" : \""
                    + APIKey
                    + "\",\r\n"
                    + "\"remainingCalls\" : "
                    + remainingCalls
                    + ",\r\n"
                    + "\"suspended\" : "
                    + suspended
                    + "\r\n}",
                null,
                "application/json"
            );
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                KeyModel key = new KeyModel();
                key.keyId = id;
                key.owner = owner;
                key.APIKey = APIKey;
                key.remainingCalls = remainingCalls;
                key.suspended = suspended;

                Console.WriteLine(".NET: API key updated");
                return key;
            }
            else
            {
                Console.WriteLine(".NET: API key not updated");
                throw new Exception("Could not update Key");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }

    public async Task<KeyModel> UpdateBusinessKeys(
        int id,
        string owner,
        string APIKey,
        int remainingCalls,
        int suspended,
        int isBusiness,
        string description,
        string location,
        string website,
        string phoneNumber,
        string email
    )
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Patch,
                express + "/api/key/updateBusiness/" + id
            );
            var content = new StringContent(
                "{\r\n "
                    + "\"owner\" : \""
                    + owner
                    + "\",\r\n"
                    + "\"APIKey\" : \""
                    + APIKey
                    + "\",\r\n"
                    + "\"remainingCalls\" : "
                    + remainingCalls
                    + ",\r\n"
                    + "\"suspended\" : "
                    + suspended
                    + ",\r\n"
                    + "\" isBusiness\" : "
                    + isBusiness
                    + ",\r\n"
                    + "\" description\" : \""
                    + description
                    + "\",\r\n"
                    + "\" location\" : \""
                    + location
                    + "\",\r\n"
                    + "\" website\" : \""
                    + website
                    + "\",\r\n"
                    + "\" phoneNumber\" : \""
                    + phoneNumber
                    + "\",\r\n"
                    + "\" email\" : \""
                    + email
                    + "\"\r\n}",
                null,
                "application/json"
            );
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                KeyModel key = new KeyModel();
                key.owner = owner;
                key.APIKey = APIKey;
                key.remainingCalls = remainingCalls;
                key.suspended = suspended;
                key.isBusiness = 1;
                key.description = description;
                key.location = location;
                key.website = website;
                key.phoneNumber = phoneNumber;
                key.email = email;

                Console.WriteLine(".NET: API key updated");
                return key;
            }
            else
            {
                Console.WriteLine(".NET: API key not updated");
                throw new Exception("Could not update Key");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(".NET: Database Connection Error: " + e.Message);
            throw new Exception("Database Error: " + e.Message);
        }
    }
}
