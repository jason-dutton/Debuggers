using System.Net;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;
using Microsoft.ML.Data;
using Microsoft.Extensions.ML;

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
    public async Task<string> GetGoogleMapsKey()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/solarscore/googlemapskey");
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

    public async Task<string> GetImages(Coordinates coordinates, string solarScoreId)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            express + "/api/solarscore/getimages/" + solarScoreId
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
    public async Task<string> GetSolarScoreFromImage(string image)
    {
        Console.WriteLine("Image: " + image);
        // ImageClassifier imageClassifier = new ImageClassifier();
        var prediction = ImageClassifier.Predict(image);
        Console.WriteLine("Prediction: " + prediction);
        return prediction;
    }

    //create solar score
    public async Task<string> CreateSolarScore(string solarScoreId, string solarScore)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, express + "/api/solarscore/create");
        var content = new StringContent("{\r\n    \"solarScoreId\" : \"" + solarScoreId + "\",\r\n    \"solarScore\": \"" + solarScore + "\"\r\n}", null, "application/json");
        request.Content = content;
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return "Created solar score";
        }
        else
        {
            throw new Exception("Error creating solar score");
        }
    }

    //delete solar score
    public async Task<string> DeleteSolarScore(string solarScoreId)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Delete, express + "/api/solarscore/delete/" + solarScoreId);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return "Deleted solar score";
        }
        else
        {
            throw new Exception("Error deleting solar score");
        }
    }

    //GetSolarScore
    public async Task<List<SolarScore>> GetSolarScore(string solarScoreId)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, express + "/api/solarscore/get/" + solarScoreId);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            List<SolarScore> ss = JsonSerializer.Deserialize<List<SolarScore>>(data);
            return ss;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            List<SolarScore> ss = new List<SolarScore>();
            return ss;
        }
        else
        {
            throw new Exception("Error getting solar score");
        }
    }

}


class ModelInput
{
    public byte[] Image { get; set; }

    public UInt32 LabelAsKey { get; set; }

    public string ImagePath { get; set; }

    public string Label { get; set; }
}

class ModelOutput
{
    public string ImagePath { get; set; }

    public string Label { get; set; }

    public string PredictedLabel { get; set; }
}

class ImageData
{
    public string ImagePath { get; set; }

    public string Label { get; set; }
}

class ImageClassifier
{
    public static string Predict(string imageFilePath)
    {
        Console.WriteLine("Predicting...");
        var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../apps/api"));
        //Console.WriteLine(projectDirectory);
        var assetsRelativePath = Path.Combine(projectDirectory, "assets");

        MLContext mlContext = new MLContext();
        Console.WriteLine("MLContext created");
        Console.WriteLine(projectDirectory + "/model.zip");
        // Load the model
        DataViewSchema modelSchema;
        Console.WriteLine("Loading model");
        ITransformer trainedModel = mlContext.Model.Load(projectDirectory + "/model.zip", out modelSchema);
        Console.WriteLine("Model loaded");
        // Preprocess the input image
        IEnumerable<ImageData> images = LoadSingleImage(imageFilePath, assetsRelativePath);
        IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
        IDataView preProcessedData = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Label", outputColumnName: "LabelAsKey")
                            .Append(mlContext.Transforms.LoadRawImageBytes(outputColumnName: "Image", imageFolder: assetsRelativePath, inputColumnName: "ImagePath"))
                            .Fit(imageData)
                            .Transform(imageData);

        Console.WriteLine("Preprocessed data");
        // Make predictions
        var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);
        ModelInput image = mlContext.Data.CreateEnumerable<ModelInput>(preProcessedData, reuseRowObject: true).First();
        // Console.WriteLine("LabelAsKey: " + image.LabelAsKey);
        // Console.WriteLine("ImagePath: " + image.ImagePath);
        // Console.WriteLine("Label: " + image.Label);
        //Console.WriteLine("Image: " + image.Image.ToString());
        ModelOutput prediction = predictionEngine.Predict(image);
        Console.WriteLine("Prediction made");
        return prediction.PredictedLabel;
    }

    private static IEnumerable<ImageData> LoadSingleImage(string imageFilePath, string assetsRelativePath)
    {
        //Console.WriteLine("Loading image: " + imageFilePath);
        if (!File.Exists(imageFilePath))
        {
            Console.WriteLine("Image file not found.");
            yield break; // Use yield break to end the iteration without returning any value.
        }

        // We will use the filename as a dummy label since the actual label is not available.
        string label = Path.GetFileName(imageFilePath);
        string imagePathRelativeToAssets = Path.GetRelativePath(assetsRelativePath, imageFilePath);

        yield return new ImageData()
        {
            ImagePath = imagePathRelativeToAssets,
            Label = label
        };
    }
}
