using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
using Microsoft.ML.Vision;

namespace DeepLearning_ImageClassification
{
  class Program
  {
    static void Main(string[] args)
    {
      TrainAndTestModel();


    }

    public static void TrainAndTestModel()
    {
      var projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../apps/machine-learning/Blue-Skies-ML/Blue-Skies-ML"));
      var workspaceRelativePath = Path.Combine(projectDirectory, "workspace");
      var assetsRelativePath = Path.Combine(projectDirectory, "assets");

      MLContext mlContext = new MLContext();

      Console.WriteLine(projectDirectory);

      IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: assetsRelativePath, useFolderNameAsLabel: true);

      IDataView imageData = mlContext.Data.LoadFromEnumerable(images);

      IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

      var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(
              inputColumnName: "Label",
              outputColumnName: "LabelAsKey")
          .Append(mlContext.Transforms.LoadRawImageBytes(
              outputColumnName: "Image",
              imageFolder: assetsRelativePath,
              inputColumnName: "ImagePath"));

      IDataView preProcessedData = preprocessingPipeline
                          .Fit(shuffledData)
                          .Transform(shuffledData);

      TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: 0.3);
      TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet);

      IDataView trainSet = trainSplit.TrainSet;
      IDataView validationSet = validationTestSplit.TrainSet;
      IDataView testSet = validationTestSplit.TestSet;

      var classifierOptions = new ImageClassificationTrainer.Options()
      {
        FeatureColumnName = "Image",
        LabelColumnName = "LabelAsKey",
        ValidationSet = validationSet,
        ValidationSetBottleneckCachedValuesFileName = Path.Combine(workspaceRelativePath, "validationSetBottleneckFile.csv"),
        TrainSetBottleneckCachedValuesFileName = Path.Combine(workspaceRelativePath, "trainSetBottleneckFile.csv"),
        Arch = ImageClassificationTrainer.Architecture.InceptionV3,
        MetricsCallback = (metrics) => Console.WriteLine(metrics),
        WorkspacePath = workspaceRelativePath,
        FinalModelPrefix = Path.Combine(workspaceRelativePath, "model"),
        EarlyStoppingCriteria = null,
        TestOnTrainSet = false,
        ReuseTrainSetBottleneckCachedValues = true,
        ReuseValidationSetBottleneckCachedValues = true,
        Epoch = 200,
        LearningRate = 0.05f,
        BatchSize = 50
      };

      var trainingPipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(classifierOptions)
          .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

      ITransformer trainedModel = trainingPipeline.Fit(trainSet);

      ClassifySingleImage(mlContext, testSet, trainedModel);

      ClassifyImages(mlContext, testSet, trainedModel);

      Console.ReadKey();
    }

    public static void ClassifySingleImage(MLContext mlContext, IDataView data, ITransformer trainedModel)
    {
      PredictionEngine<ModelInput, ModelOutput> predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

      ModelInput image = mlContext.Data.CreateEnumerable<ModelInput>(data, reuseRowObject: true).First();

      ModelOutput prediction = predictionEngine.Predict(image);

      Console.WriteLine("Classifying single image");
      OutputPrediction(prediction);
    }

    public static void ClassifyImages(MLContext mlContext, IDataView data, ITransformer trainedModel)
    {
      IDataView predictionData = trainedModel.Transform(data);

      IEnumerable<ModelOutput> predictions = mlContext.Data.CreateEnumerable<ModelOutput>(predictionData, reuseRowObject: true).Take(10);

      var correct = 0;
      //Keep track of how far off the data is from the right value - Possible outputs are Very_Bad, Bad, Average, Good, Very_Good
      var error = new int[3];

      Console.WriteLine("Classifying multiple images");
      foreach (var prediction in predictions)
      {
        OutputPrediction(prediction);
        if (prediction.Label == prediction.PredictedLabel)
          correct++;
        else
          error[getLabelIndex(prediction.Label)]++;
      }
      Console.WriteLine($"Correctly classified {correct} out of {predictions.Count()} images");
      //Console.WriteLine($"Very_Bad: {error[0]}");
      Console.WriteLine($"Bad: {error[0]}");
      Console.WriteLine($"Average: {error[1]}");
      Console.WriteLine($"Good: {error[2]}");
     // Console.WriteLine($"Very_Good: {error[4]}");
    }

    private static int getLabelIndex(string label)
    {
      switch (label)
      {
        case "Bad":
          return 0;
        case "Average":
          return 1;
        case "Good":
          return 2;     
        default:
          return -1;
      }
    }

    private static void OutputPrediction(ModelOutput prediction)
    {
      string imageName = Path.GetFileName(prediction.ImagePath);
      Console.WriteLine($"Image: {imageName} | Actual Value: {prediction.Label} | Predicted Value: {prediction.PredictedLabel}");
    }

    public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
    {
      var files = Directory.GetFiles(folder, "*",
          searchOption: SearchOption.AllDirectories);

      foreach (var file in files)
      {
        if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
          continue;

        var label = Path.GetFileName(file);

        if (useFolderNameAsLabel)
          label = Directory.GetParent(file).Name;
        else
        {
          for (int index = 0; index < label.Length; index++)
          {
            if (!char.IsLetter(label[index]))
            {
              label = label.Substring(0, index);
              break;
            }
          }
        }

        yield return new ImageData()
        {
          ImagePath = file,
          Label = label
        };
      }
    }
  }

  class ImageData
  {
    public string ImagePath { get; set; }

    public string Label { get; set; }
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
}
