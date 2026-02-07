namespace CommunityCar.Domain.Interfaces.Common;

/// <summary>
/// Service for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number</param>
    /// <param name="message">The message to send</param>
    /// <returns>True if the message was sent successfully</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Sends an OTP code to the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number</param>
    /// <param name="code">The OTP code to send</param>
    /// <returns>True if the code was sent successfully</returns>
    Task<bool> SendOtpAsync(string phoneNumber, string code);

    /// <summary>
    /// Generates a random OTP code
    /// </summary>
    /// <param name="length">The length of the OTP code (default: 6)</param>
    /// <returns>The generated OTP code</returns>
    string GenerateOtp(int length = 6);
}
