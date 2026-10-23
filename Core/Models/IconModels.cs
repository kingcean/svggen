using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Violty.SvgGenerator.Models;

/// <summary>
/// Contains SVG markup for four style variants of the same icon design.
/// </summary>
public class IconSet
{
    /// <summary>
    /// Gets or sets the SVG markup for the single-color line icon.
    /// </summary>
    [JsonPropertyName("lineIcon")]
    public string Line { get; set; }

    /// <summary>
    /// Gets or sets the SVG markup for the single-color solid icon.
    /// </summary>
    [JsonPropertyName("solidIcon")]
    public string Solid { get; set; }

    /// <summary>
    /// Gets or sets the SVG markup for the color icon intended for a light background.
    /// </summary>
    [JsonPropertyName("lightIcon")]
    public string Light { get; set; }

    /// <summary>
    /// Gets or sets the SVG markup for the color icon intended for a dark background.
    /// </summary>
    [JsonPropertyName("darkIcon")]
    public string Dark { get; set; }
}
