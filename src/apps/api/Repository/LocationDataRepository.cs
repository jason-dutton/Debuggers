using System.Net;
using System.Text.Json;
using System;

namespace Api.Repository;

public class LocationDataRepository
{
    private readonly string express = "http://localhost:3333";

    public LocationDataRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }
    public async Task<string> GetChatbotApiKey()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/locationData/chatbotapikey"
        );
        var response = await client.SendAsync(request);
        // response.EnsureSuccessStatusCode();
        // Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new Exception("Error getting chatbot api key");
        }
    }
    public async Task<string> GetMapBoxApiKey()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/locationData/mapboxkey"
        );
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

    public async Task<string> GetGoogleMapsKey()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/locationData/googlemapskey"
        );
        var response = await client.SendAsync(request);
        // response.EnsureSuccessStatusCode();
        // Console.WriteLine(await response.Content.ReadAsStringAsync());
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new Exception("Error getting google maps key");
        }
    }

    //Create Solar Irradiation
    public async Task<string> CreateLocationData(
        double latitude,
        double longitude,
        string locationName,
        string solarPanelsData,
        byte[] satteliteImageData,
        byte[] annualFluxData,
        byte[] monthlyFluxData,
        byte[] maskData,
        string horisonElevationData
    )
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                express + "/api/locationData/create"
            );
            string satteliteImageDataBase64 = Convert.ToBase64String(satteliteImageData);
            string annualFluxDataBase64 = Convert.ToBase64String(annualFluxData);
            string monthlyFluxDataBase64 = Convert.ToBase64String(monthlyFluxData);
            string maskDataBase64 = Convert.ToBase64String(maskData);

            var postData = new
            {
                latitude = latitude.ToString(),
                longitude = longitude.ToString(),
                locationName = locationName,
                solarPanelsData = solarPanelsData,
                satteliteImageData = satteliteImageDataBase64,
                annualFluxData = annualFluxDataBase64,
                monthlyFluxData = monthlyFluxDataBase64,
                maskData = maskDataBase64,
                horisonElevationData = horisonElevationData
            };

            var json = JsonSerializer.Serialize(postData);
            request.Content = new StringContent(json, null, "application/json");
            // Console.WriteLine(await request.Content.ReadAsStringAsync());
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(".NET LocationData created successfully");
                return "LocationData created successfully";
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine(".NET Bad Request When Creating LocationData");
                // Get error
                string data = response.Content.ReadAsStringAsync().Result;
                return data;
            }
            else
            {
                Console.WriteLine(".NET Error creating LocationData");
                throw new Exception("Error creating LocationData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not create locationData");
        }
    }

    //Get solar irradiation
    public async Task<LocationDataModel> GetLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/" + latitude + "/" + longitude
            );
            Console.WriteLine("Getting LocationData for " + latitude + ", " + longitude);
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = locationDataTemp.locationName,
                    solarPanelsData = JsonSerializer.Deserialize<RooftopInformationModel>(locationDataTemp.solarPanelsData!),
                    satteliteImageData = Convert.FromBase64String(locationDataTemp.satteliteImageData!),
                    annualFluxData = Convert.FromBase64String(locationDataTemp.annualFluxData!),
                    monthlyFluxData = Convert.FromBase64String(locationDataTemp.monthlyFluxData!),
                    maskData = Convert.FromBase64String(locationDataTemp.maskData!),
                    dateCreated = locationDataTemp.dateCreated,
                    horisonElevationData = locationDataTemp.horisonElevationData
                };
                return locationData!;
            }else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine(".NET LocationData not found");
                return null!;
            }
            else
            {
                Console.WriteLine(".NET Error getting LocationData");
                throw new Exception("Error getting LocationData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get LocationData");
        }
    }


    // EssentialLocationData
    public async Task<LocationDataModel> EssentialLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/essentialData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = locationDataTemp.locationName,
                    solarPanelsData = JsonSerializer.Deserialize<RooftopInformationModel>(locationDataTemp.solarPanelsData!),
                    satteliteImageData = Convert.FromBase64String(locationDataTemp.satteliteImageData!),
                    annualFluxData = null,
                    monthlyFluxData = null,
                    maskData = null,
                    dateCreated = locationDataTemp.dateCreated,
                    horisonElevationData = null
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting essentialData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get essentialData");
        }
    }

    public async Task<LocationDataModel> InitialLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/initialData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = locationDataTemp.locationName,
                    solarPanelsData = JsonSerializer.Deserialize<RooftopInformationModel>(locationDataTemp.solarPanelsData!),
                    satteliteImageData = null,
                    annualFluxData = null,
                    monthlyFluxData = null,
                    maskData = null,
                    dateCreated = locationDataTemp.dateCreated,
                    horisonElevationData = locationDataTemp.horisonElevationData
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting initialData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get initialData");
        }
    }


public async Task<LocationDataModel> SatelliteImageLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/satelliteImageData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = null,
                    solarPanelsData = null,
                    satteliteImageData = Convert.FromBase64String(locationDataTemp.satteliteImageData!),
                    annualFluxData = null,
                    monthlyFluxData = null,
                    maskData = null,
                    dateCreated = null,
                    horisonElevationData = null
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting satelliteImageData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get satelliteImageData");
        }
    }

    public async Task<LocationDataModel> MaskLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/maskData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = null,
                    solarPanelsData = null,
                    satteliteImageData = null,
                    annualFluxData = null,
                    monthlyFluxData = null,
                    maskData = Convert.FromBase64String(locationDataTemp.maskData!),
                    dateCreated = null,
                    horisonElevationData = null
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting maskData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get maskData");
        }
    }

    public async Task<LocationDataModel> AnnualFluxLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/annualFluxData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = null,
                    solarPanelsData = null,
                    satteliteImageData = null,
                    annualFluxData = Convert.FromBase64String(locationDataTemp.annualFluxData!),
                    monthlyFluxData = null,
                    maskData = null,
                    dateCreated = null,
                    horisonElevationData = null
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting annualFluxData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get annualFluxData");
        }
    }

    public async Task<LocationDataModel> MonthlyFluxLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/monthlyFluxData/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                LocationDataModelTemp locationDataTemp = JsonSerializer.Deserialize<LocationDataModelTemp>(data)!;
                LocationDataModel locationData = new LocationDataModel()
                {
                    latitude = locationDataTemp.latitude,
                    longitude = locationDataTemp.longitude,
                    locationName = null,
                    solarPanelsData = null,
                    satteliteImageData = null,
                    annualFluxData = null,
                    monthlyFluxData = Convert.FromBase64String(locationDataTemp.monthlyFluxData!),
                    maskData = null,
                    dateCreated = null,
                    horisonElevationData = null
                };
                return locationData!;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null!;
            }
            else
            {
                throw new Exception("Error getting monthlyFluxData");
            }

        }
        catch (System.Exception)
        {
            throw new Exception("Could not get monthlyFluxData");
        }
    }

    //DeleteLocationData
    public async Task<string> DeleteLocationData(double latitude, double longitude)
    {
        try
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                express + "/api/locationData/delete/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return "LocationData deleted successfully";
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return "LocationData not found";
            }
            else
            {
                throw new Exception("Error deleting LocationData");
            }
        }
        catch (System.Exception)
        {
            throw new Exception("Could not delete LocationData");
        }
    }

    //CheckIfLocationDataExists
    public async Task<bool> CheckIfLocationDataExists(double latitude, double longitude)
    {
        try
        {
            Console.WriteLine("Checking if LocationData exists for " + latitude + ", " + longitude);
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                express + "/api/locationData/checkIfLocationDataExists/" + latitude + "/" + longitude
            );
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(".NET LocationData exists");
                return true;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine(".NET LocationData does not exist");
                return false;
            }
            else
            {
                Console.WriteLine(".NET Error checking if LocationData exists");
                throw new Exception("Error checking if LocationData exists");
            }
        }
        catch (System.Exception)
        {
            throw new Exception("Could not check if LocationData exists");
        }
    }
}

public class LocationDataModelTemp
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public string? locationName { get; set; }
    public string? solarPanelsData { get; set; }
    public string? satteliteImageData { get; set; }
    public string? annualFluxData { get; set; }
    public string? monthlyFluxData { get; set; }
    public string? maskData { get; set; }
    public DateTime? dateCreated { get; set; }
    public string? horisonElevationData { get; set; }
}