using System.Text.Json.Serialization;

namespace Violty.SvgGenerator.Settings;

/// <summary>
/// Stores workspace settings for generated output.
/// </summary>
public class WorkspaceSettings
{
    /// <summary>
    /// Gets or sets the root directory path for saving generated files, or null if it is not specified.
    /// </summary>
    [JsonPropertyName("dir")]
    public string? Directory { get; set; }
}
