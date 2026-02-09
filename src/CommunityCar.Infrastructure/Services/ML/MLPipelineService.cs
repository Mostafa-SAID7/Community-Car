using Microsoft.ML;
using Microsoft.ML.Data;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Models.ML;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.ML
{
    public class MLPipelineService : CommunityCar.Infrastructure.Interfaces.ML.IMLPipelineService
    {
        private readonly ILogger<MLPipelineService> _logger;
        private readonly MLContext _mlContext = new MLContext(seed: 0);

        public MLPipelineService(ILogger<MLPipelineService> logger)
        {
            _logger = logger;
        }

        public async Task TrainModelsAsync()
        {
            _logger.LogInformation("Starting ML Model training...");

            try
            {
                await Task.Run(() =>
                {
                    TrainIntentModel();
                    TrainSentimentModel();
                });
                _logger.LogInformation("ML Model training completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ML Model training.");
            }
        }

        private void TrainIntentModel()
        {
            var trainingData = new List<ChatData>
            {
                new ChatData { Text = "hi", Intent = "Greeting" },
                new ChatData { Text = "hello", Intent = "Greeting" },
                new ChatData { Text = "hey", Intent = "Greeting" },
                
                new ChatData { Text = "tell me about cars", Intent = "Cars" },
                new ChatData { Text = "how to choose a car", Intent = "Cars" },
                new ChatData { Text = "best sedans 2024", Intent = "Cars" },
                
                new ChatData { Text = "how much does it cost", Intent = "Price" },
                new ChatData { Text = "car prices", Intent = "Price" },
                new ChatData { Text = "repair cost", Intent = "Price" },
                
                new ChatData { Text = "how to fix engine", Intent = "Maintenance" },
                new ChatData { Text = "oil change frequency", Intent = "Maintenance" },
                new ChatData { Text = "brake check", Intent = "Maintenance" },

                new ChatData { Text = "how are you", Intent = "General" },
                new ChatData { Text = "help me", Intent = "General" }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(_mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);
            PredictionService.SetModel(model);
        }

        private void TrainSentimentModel()
        {
            var trainingData = new List<SentimentData>
            {
                new SentimentData { Text = "I love this app!", Sentiment = true },
                new SentimentData { Text = "This is great.", Sentiment = true },
                new SentimentData { Text = "I hate this.", Sentiment = false },
                new SentimentData { Text = "Too slow and buggy.", Sentiment = false }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "Text")
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = pipeline.Fit(dataView);
            SentimentAnalysisService.SetModel(model);
        }
    }
}
