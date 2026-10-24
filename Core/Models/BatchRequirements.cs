using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trivial.Collection;

namespace Violty.SvgGenerator.Models;

/// <summary>
/// Describes a resource to generate as part of a batch.
/// </summary>
public class BatchRequirementItem
{
    /// <summary>
    /// Gets or sets the resource key used as the output file name without an extension.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the resource name used to build a prompt when no description is provided.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the generation prompt, or a blank value to build it from the name and categories.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets optional categories included in the prompt when no description is provided.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    /// <summary>
    /// Gets or sets additional JSON properties not mapped to declared members.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Description)) return Description;
        var sb = new StringBuilder(Name);
        var categories = Categories;
        if (categories is not null && categories.Count > 0)
        {
            sb.Append(" (categories: ");
            sb.Append(string.Join("; ", categories));
            sb.Append(')');
        }

        return sb.ToString();
    }
}

/// <summary>
/// Associates a batch requirement with its generated resource output.
/// </summary>
/// <typeparam name="T">The type of the generated resource output.</typeparam>
public class BatchRequirementItemResult<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRequirementItemResult{T}"/> class.
    /// </summary>
    public BatchRequirementItemResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRequirementItemResult{T}"/> class with a requirement and generated output.
    /// </summary>
    /// <param name="input">The batch requirement associated with the result.</param>
    /// <param name="output">The generated resource output.</param>
    public BatchRequirementItemResult(BatchRequirementItem input, T output)
    {
        Input = input;
        Output = output;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRequirementItemResult{T}"/> class with a requirement and generated output.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="name">The name.</param>
    /// <param name="output">The generated resource output.</param>
    public BatchRequirementItemResult(string key, string name, T output)
    {
        Input = new BatchRequirementItem
        {
            Key = key,
            Name = name
        };
        Output = output;
    }

    /// <summary>
    /// Gets the associated requirement's resource key, or <see langword="null"/> if no requirement is assigned.
    /// </summary>
    [JsonIgnore]
    public string? Key => Input?.Key;

    /// <summary>
    /// Gets the associated requirement's resource name, or <see langword="null"/> if no requirement is assigned.
    /// </summary>
    [JsonIgnore]
    public string? Name => Input?.Name;

    /// <summary>
    /// Gets the associated requirement's categories, or <see langword="null"/> if no requirement or categories are assigned.
    /// </summary>
    [JsonIgnore]
    public List<string>? Categories => Input?.Categories;

    /// <summary>
    /// Gets or sets the batch requirement associated with the generated output.
    /// </summary>
    [JsonPropertyName("input")]
    public BatchRequirementItem Input { get; set; }

    /// <summary>
    /// Gets or sets the generated resource output.
    /// </summary>
    [JsonPropertyName("output")]
    public T Output { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => Input?.ToString() ?? (Output is null ? "Empty" : $"[{Output.GetType().Name}]");
}
