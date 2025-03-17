using System.Collections.Generic;
using System.Linq;

namespace BricklinkSharp.Client;

public class BricklinkCredentials
{
    public string? ConsumerKey { get; set; }

    public string? ConsumerSecret { get; set; }

    public string? TokenValue { get; set; }

    public string? TokenSecret { get; set; }

    internal void ValidateThrowException()
    {
        var missingParams = new List<string>();

        if (string.IsNullOrEmpty(ConsumerKey?.Trim()))
        {
            missingParams.Add(nameof(ConsumerKey));
        }

        if (string.IsNullOrEmpty(ConsumerSecret?.Trim()))
        {
            missingParams.Add(nameof(ConsumerSecret));
        }

        if (string.IsNullOrEmpty(TokenValue?.Trim()))
        {
            missingParams.Add(nameof(TokenValue));
        }

        if (string.IsNullOrEmpty(TokenSecret?.Trim()))
        {
            missingParams.Add(nameof(TokenSecret));
        }

        if (missingParams.Any())
        {
            throw new BricklinkMissingCredentialsException(missingParams);
        }
    }
}