using Microsoft.ML.Data;

namespace CommunityCar.Infrastructure.Models.ML
{
    public class ChatData
    {
        [LoadColumn(0)]
        public string Text { get; set; } = string.Empty;

        [LoadColumn(1), ColumnName("Label")]
        public string Intent { get; set; } = string.Empty;
    }

    public class SentimentData
    {
        [LoadColumn(0)]
        public string Text { get; set; } = string.Empty;

        [LoadColumn(1), ColumnName("Label")]
        public bool Sentiment { get; set; }
    }

    public class IntentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedIntent { get; set; } = string.Empty;

        public float[] Score { get; set; } = System.Array.Empty<float>();
    }

    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
