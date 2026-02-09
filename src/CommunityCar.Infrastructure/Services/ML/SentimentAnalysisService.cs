using Microsoft.ML;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Models.ML;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.ML
{
    public class SentimentAnalysisService : ISentimentAnalysisService
    {
        private readonly ILogger<SentimentAnalysisService> _logger;
        private static ITransformer? _model;
        private static MLContext _mlContext = new MLContext();

        public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
        {
            _logger = logger;
        }

        public static void SetModel(ITransformer model)
        {
            _model = model;
        }

        public SentimentPrediction AnalyzeSentiment(string text)
        {
            if (_model == null)
            {
                _logger.LogWarning("Sentiment model is not trained yet. Returning default.");
                return new SentimentPrediction { Text = text, Prediction = true, Probability = 0.5f };
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            return predictionEngine.Predict(new SentimentData { Text = text });
        }
    }
}
