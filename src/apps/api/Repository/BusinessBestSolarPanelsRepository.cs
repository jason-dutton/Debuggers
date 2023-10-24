using Newtonsoft.Json;

namespace Api.Repository;

public class BusinessBestSolarPanelsRepository
{
    
    private SharedUtils.locationDataClass locationDataClass = new SharedUtils.locationDataClass();
    private DataHandlers.SolarDataHandler solarCalculator = new DataHandlers.SolarDataHandler();
    private DataHandlers.RooftopDataHandler rooftopDataHandler = new DataHandlers.RooftopDataHandler();
    private string locationName = "";
    private string? mapboxAccessToken = Environment.GetEnvironmentVariable("MAP_BOX_API_KEY");

   
    public async Task<List<BestSolarPanelsOutput>> GetProcessedDataAsync(BestSolarPanelsInput bestSolarPanelsInput)
    {
        var returnDataJson = new object();
        LocationDataModel? currentLocationData = new LocationDataModel();
        try
        {
            int numPanels = bestSolarPanelsInput.total_panels ?? 0;
            double latitude = bestSolarPanelsInput.latitude;
            double longitude = bestSolarPanelsInput.longitude;

            var client = new HttpClient();
            var dataTypeResponse = new HttpResponseMessage();

            if(numPanels==0)
            {
                throw new Exception("Number of panels cannot be left out, or be zero.");
            } else if(numPanels<0)
            {
                throw new Exception("Number of panels cannot be negative.");
            }

            LocationDataModel? locationData = await locationDataClass.GetLocationData(latitude, longitude);
            locationName = await GetLocationNameFromCoordinates(latitude, longitude);
            if (locationData == null)
            {                
                await locationDataClass.CreateLocationData(latitude, longitude, locationName);
            }
            List<BestSolarPanelsOutput> solarPanels = await GetBestSolarPanelsOutputAsync(numPanels, latitude, longitude);
            return solarPanels;
        }
        catch (Exception)
        {
            throw new Exception("Could not create Solar Panel Orientation Data. NOTE that a valid address should be used.");
 
        }
    }

    private async Task<List<BestSolarPanelsOutput>> GetBestSolarPanelsOutputAsync(int numPanels, double latitude, double longitude)
    {
        LocationDataModel? locationData = await locationDataClass.GetLocationData(latitude, longitude);
        List<Solarpanel> solarPanels = solarCalculator.getBestSolarPanels(numPanels, locationData!.solarPanelsData!)!;
        List<BestSolarPanelsOutput> bestSolarPanelsOutputs = convertSolarPanelsToBestSolarPanelsOutput(solarPanels);
        return bestSolarPanelsOutputs;
    }
    private List<BestSolarPanelsOutput> convertSolarPanelsToBestSolarPanelsOutput(List<Solarpanel> solarPanels)
    {
        List<BestSolarPanelsOutput> bestSolarPanels = new List<BestSolarPanelsOutput>();
        foreach(Solarpanel solarPanel in solarPanels)
        {
            BestSolarPanelsOutput bestSolarPanel = new BestSolarPanelsOutput();
            bestSolarPanel.orientation = solarPanel.orientation!;
            bestSolarPanel.yearlyEnergyDcKwh = solarPanel.yearlyEnergyDcKwh!;
            bestSolarPanel.latitude = solarPanel.center!.latitude!;
            bestSolarPanel.longitude = solarPanel.center!.longitude!;
            bestSolarPanels.Add(bestSolarPanel);
        }
        return bestSolarPanels;
    }
    private async Task<LocationDataModel> GetLocationDataModel(double latitude, double longitude)
    {
        LocationDataModel? locationData = await locationDataClass.GetLocationData(latitude, longitude);
        if(locationData==null){
            locationData = await locationDataClass.CreateLocationData(latitude, longitude, locationName);
        }
        return locationData!;
    }
     public async Task<string> GetLocationNameFromCoordinates(double latitude, double longitude)
    {
        string baseUrl = "https://api.mapbox.com/geocoding/v5/mapbox.places/";
        string requestUrl =
            $"{baseUrl}{longitude.ToString().Replace(",", ".")},{latitude.ToString().Replace(",", ".")}.json?&access_token={mapboxAccessToken}";

        try
        {
            HttpClient httpClient = new HttpClient();
            var mapResponse = await httpClient.GetFromJsonAsync<GeocodingResponse>(requestUrl);
            return mapResponse?.Features[0].Place_Name ?? "";
        }
        catch (Exception ex)
        {
            // Handle any errors or exceptions
            Console.WriteLine(ex.Message);
            return "";
        }
        
    }
    private class GeocodingResponse
    {
        public List<LocationSuggestion> Features { get; set; } = new List<LocationSuggestion>();
    }
}
