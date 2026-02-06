namespace CommunityCar.Web.Exceptions;

public class WebException : Exception
{
    public int StatusCode { get; }

    public WebException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}
