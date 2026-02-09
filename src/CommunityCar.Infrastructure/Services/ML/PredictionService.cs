using Microsoft.ML;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Models.ML;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.ML
{
    public class PredictionService : IPredictionService
    {
        private readonly ILogger<PredictionService> _logger;
        private static ITransformer? _model;
        private static MLContext _mlContext = new MLContext();

        public PredictionService(ILogger<PredictionService> logger)
        {
            _logger = logger;
        }

        public static void SetModel(ITransformer model)
        {
            _model = model;
        }

        public IntentPrediction PredictIntent(string text)
        {
            if (_model == null)
            {
                _logger.LogWarning("Intent model is not trained yet. Returning default.");
                return new IntentPrediction { PredictedIntent = "General" };
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ChatData, IntentPrediction>(_model);
            return predictionEngine.Predict(new ChatData { Text = text });
        }
    }
}
