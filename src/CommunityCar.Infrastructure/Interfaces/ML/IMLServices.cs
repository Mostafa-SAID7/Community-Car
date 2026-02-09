using System.Threading.Tasks;
using CommunityCar.Infrastructure.Models.ML;

namespace CommunityCar.Infrastructure.Interfaces.ML
{
    public interface IMLPipelineService
    {
        Task TrainModelsAsync();
    }

    public interface IPredictionService
    {
        IntentPrediction PredictIntent(string text);
    }

    public interface ISentimentAnalysisService
    {
        SentimentPrediction AnalyzeSentiment(string text);
    }
}
