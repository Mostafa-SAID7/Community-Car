using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CommunityCar.Domain.Interfaces.Services;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Services.ML;

namespace CommunityCar.Infrastructure.Services
{
    public class AssistantService : CommunityCar.Domain.Interfaces.Services.IAssistantService
    {
        private readonly ILogger<AssistantService> _logger;
        private readonly IPredictionService _predictionService;
        private readonly ISentimentAnalysisService _sentimentService;

        public AssistantService(
            ILogger<AssistantService> logger,
            IPredictionService predictionService,
            ISentimentAnalysisService sentimentService)
        {
            _logger = logger;
            _predictionService = predictionService;
            _sentimentService = sentimentService;
        }

        public async Task<string> GetChatResponseAsync(string userId, string message)
        {
            try
            {
                // Simple deliberation delay to feel more "AI"
                await Task.Delay(800);

                var sentiment = _sentimentService.AnalyzeSentiment(message);
                var intent = _predictionService.PredictIntent(message);

                _logger.LogInformation("ML Assistant: Intent={Intent}, Sentiment={Sentiment}", intent.PredictedIntent, sentiment.Prediction);

                if (!sentiment.Prediction && sentiment.Probability > 0.7)
                {
                    return "I'm sorry you're feeling frustrated. How can I better assist you with the Community Car platform?";
                }

                return intent.PredictedIntent switch
                {
                    "Greeting" => "Hello! I'm your Community Car Assistant. How can I help you today?",
                    "Cars" => "Community Car is all about connecting car enthusiasts! You can find detailed info in our 'Guides' or join 'Groups' for specific car models.",
                    "Price" => "Pricing for services or parts can vary. Check our 'Reviews' section to see what other community members have paid and their recommendations.",
                    "Maintenance" => "Proper maintenance is key! Our 'Guides' section has many DIY articles on oil changes, brake checks, and engine maintenance.",
                    "General" => "I'm here to help you navigate! You can manage friends, join groups, or check out the latest news on your feed.",
                    _ => "That's interesting! I'm still learning, but feel free to ask me about car maintenance, prices, or how to use the site."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
                return "I'm sorry, I encountered a bit of a glitch. Could you try asking that again?";
            }
        }
    }
}
