using CommunityCar.Domain.Interfaces.Common;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Common;

/// <summary>
/// SMS service implementation
/// Note: This is a mock implementation. In production, integrate with services like:
/// - Twilio
/// - AWS SNS
/// - Azure Communication Services
/// - Vonage (Nexmo)
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly Dictionary<string, string> _otpStorage = new(); // In-memory storage for demo

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // TODO: Implement actual SMS sending logic with your provider
            // Example with Twilio:
            // await _twilioClient.Messages.CreateAsync(
            //     to: new PhoneNumber(phoneNumber),
            //     from: new PhoneNumber(_twilioPhoneNumber),
            //     body: message
            // );

            _logger.LogInformation("SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
            
            // Simulate async operation
            await Task.Delay(100);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string code)
    {
        try
        {
            var message = $"Your verification code is: {code}. This code will expire in 10 minutes.";
            
            // Store OTP for verification (in production, use Redis or database with expiration)
            _otpStorage[phoneNumber] = code;
            
            var result = await SendSmsAsync(phoneNumber, message);
            
            if (result)
            {
                _logger.LogInformation("OTP {Code} sent to {PhoneNumber}", code, phoneNumber);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public string GenerateOtp(int length = 6)
    {
        var random = new Random();
        var otp = string.Empty;
        
        for (int i = 0; i < length; i++)
        {
            otp += random.Next(0, 10).ToString();
        }
        
        return otp;
    }

    /// <summary>
    /// Verify OTP code (for demo purposes)
    /// In production, this should be in a separate service with proper expiration handling
    /// </summary>
    public bool VerifyOtp(string phoneNumber, string code)
    {
        if (_otpStorage.TryGetValue(phoneNumber, out var storedCode))
        {
            if (storedCode == code)
            {
                _otpStorage.Remove(phoneNumber); // Remove after successful verification
                return true;
            }
        }
        
        return false;
    }
}
