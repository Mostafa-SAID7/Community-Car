using CommunityCar.Domain.Interfaces.Common;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Common;

/// <summary>
/// SMS sender implementation
/// Note: This is a placeholder implementation. In production, integrate with services like:
/// - Twilio
/// - AWS SNS
/// - Azure Communication Services
/// - Vonage (Nexmo)
/// </summary>
public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // TODO: Implement actual SMS sending logic
        // Example with Twilio:
        // var client = new TwilioRestClient(accountSid, authToken);
        // await client.Messages.CreateAsync(
        //     to: new PhoneNumber(phoneNumber),
        //     from: new PhoneNumber(fromPhoneNumber),
        //     body: message
        // );

        _logger.LogInformation("SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
        
        // Simulate async operation
        await Task.CompletedTask;
    }
}
