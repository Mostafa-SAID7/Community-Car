namespace CommunityCar.Domain.Interfaces.Common;

/// <summary>
/// Interface for sending SMS messages
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// Sends an SMS message to the specified phone number
    /// </summary>
    /// <param name="phoneNumber">The recipient's phone number</param>
    /// <param name="message">The message content</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendSmsAsync(string phoneNumber, string message);
}
