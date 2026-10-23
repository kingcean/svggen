using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Json.Serialization;

namespace Violty.SvgGenerator.Settings;

/// <summary>
/// Stores chat service settings and provides a loader for environment variables.
/// </summary>
public class ChatSettings
{
    /// <summary>
    /// Gets or sets the API key used to authenticate with the chat service.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the chat service endpoint URI.
    /// </summary>
    [JsonPropertyName("apiEndpoint")]
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the chat model identifier.
    /// </summary>
    [JsonPropertyName("chatModel")]
    public string? ChatModel { get; set; }

    /// <summary>
    /// Loads settings from OPENAI_API_KEY, OPENAI_API_ENDPOINT, and OPENAI_API_CHAT_MODEL.
    /// </summary>
    /// <returns>Settings with missing or unreadable environment values left as <see langword="null"/>.</returns>
    public static ChatSettings FromEnvironment()
        => new()
        {
            ApiKey = GetEnv("OPENAI_API_KEY"),
            ApiEndpoint = GetEnv("OPENAI_API_ENDPOINT"),
            ChatModel = GetEnv("OPENAI_API_CHAT_MODEL"),
        };

    private static string? GetEnv(string variable)
    {
        try
        {
            return Environment.GetEnvironmentVariable(variable);
        }
        catch (SecurityException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (NotImplementedException)
        {
            return null;
        }
        catch (ExternalException)
        {
            return null;
        }
    }
}
