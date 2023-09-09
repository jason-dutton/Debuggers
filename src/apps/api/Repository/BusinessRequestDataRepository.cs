using System.Net;
using System;
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;

namespace Api.Repository;

public class BusinessRequestDataRepository
{
    private SharedUtils.locationDataClass locationDataClass = new SharedUtils.locationDataClass();
    private SharedUtils.otherDataClass otherDataClass = new SharedUtils.otherDataClass();
    private string express = "http://localhost:3333";
    private string? API_PORT = Environment.GetEnvironmentVariable("API_PORT");
    public BusinessRequestDataRepository()
    {
        var backendexpress = Environment.GetEnvironmentVariable("EXPRESS_BACKEND");
        if (backendexpress != null)
        {
            express = backendexpress;
        }
    }
    
    public async Task<string> GetProcessedDataAsync(BusinessRequestData requestData)
    {
        LocationDataModel? currentLocationData = new LocationDataModel();
        try
        {
            var key = requestData.key;
            var data = requestData.data;
            var latitude = requestData.latitude;
            var longitude = requestData.longitude;
            
            //create data if not created yet

            var client = new HttpClient();
            var dataTypeResponse = new HttpResponseMessage();
           
            LocationDataModel exists = await locationDataClass.GetLocationData(latitude, longitude);
                    if (exists.data == null)
                    {
                        
                        var initialDataModel = await locationDataClass.GetInitialData(latitude, longitude);
                        byte[] imageBytes = await locationDataClass.DownloadImageFromGoogleMapsService(latitude, longitude);
                        var location = await otherDataClass.GetLocationNameFromCoordinates(latitude, longitude);
                       
                        await locationDataClass.CreateLocationData(latitude, longitude, (float)initialDataModel.averageSunlightHours, Convert.ToBase64String(imageBytes), location);
                    }

            switch(data!.ToLower()){
                case "solar score" : 
                    var content = await GetSolarScore((double)latitude, (double)longitude);
                   
                    dataTypeResponse.Content = new StringContent(content);
                    break;
                default : 
                    throw new Exception("Error: Not a valid option chosen for data type");
            }
            
           return await dataTypeResponse.Content.ReadAsStringAsync();
           
        }
        catch (System.Exception)
        {

            throw new Exception("Could not create solar irradiation");
        }
    }

    private async Task<String> solarScore(HttpClient client, double latitude, double longitude){
        var numYears = 3;
        var numDaysPerYear = 48;
        var dataType = new HttpRequestMessage(
                        HttpMethod.Get, 
                        API_PORT + "/locationData/getLocationDataWithoutImage/" + latitude.ToString().Replace(",",".") + "/" + longitude.ToString().Replace(",",".") 
                    );

        client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, API_PORT + "/locationData/getSolarIrradiationData");
        var content = new StringContent("{\r\n    \"latitude\": "+ latitude.ToString().Replace(",",".") + ",\r\n    \"longitude\": " + longitude.ToString().Replace(",",".") + ",\r\n    \"numYears\": "+ numYears + ",\r\n    \"numDaysPerYear\": "+ numDaysPerYear +"\r\n}", null, "application/json");
        request.Content = content;
        var response = await client.SendAsync(request);
        var data = await response.Content.ReadAsStringAsync();
        return data;
    }
    private async Task<String> GetSolarScore(double latitude, double longitude) 
    {
        DataHandlers.SolarDataHandler solarCalculator = new DataHandlers.SolarDataHandler();
        solarCalculator.reset();
        int solarScore=0;
        
        while(solarCalculator.remainingCalls > 0 && solarCalculator.timesNotUpdated < 10) {
            solarScore = await solarCalculator.GetSolarScoreFromData(latitude, longitude, 0);
            Console.WriteLine("Remaining calls: " + solarCalculator.remainingCalls + " timesNotUpdated: " + solarCalculator.timesNotUpdated + " solarScore: " + solarScore);
            await Task.Delay(3000);

        }
        return solarScore.ToString();
    }
}