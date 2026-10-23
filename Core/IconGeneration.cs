using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trivial.Text;
using Violty.SvgGenerator.AI;
using Violty.SvgGenerator.Models;
using Violty.SvgGenerator.Settings;

namespace Violty.SvgGenerator;

/// <summary>
/// The service to generate SVG icons.
/// </summary>
public class IconGeneration : BaseResourceGeneration<IconSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IconGeneration"/> class.
    /// </summary>
    /// <param name="json">The chat and workspace settings, or <see langword="null"/> to use defaults and environment-based chat settings.</param>
    public IconGeneration(JsonObjectNode? json)
        : base(json)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IconGeneration"/> class.
    /// </summary>
    /// <param name="json">A JSON settings string or settings file path, or <see langword="null"/> to look for config.json in the current and application directories.</param>
    public IconGeneration(string? json = null)
        : base(json)
    {
    }

    /// <inheritdoc />
    protected override string Key => "icons";

    /// <inheritdoc />
    protected override string GetPrompt()
    {
        var rule = GetDesignRule("icon") ?? DefaultPrompts.IconDesign;
        var sb = new StringBuilder(DefaultPrompts.Icon);
        sb.AppendLine();
        sb.AppendLine();
        var items = Recent().Take(5).ToList();
        if (items.Count > 0)
        {
            sb.AppendLine("# References");
            sb.AppendLine();
            ResourceGenerationHelper.CreatePromptByRecent(sb, items);
        }

        sb.Append(rule);
        return sb.ToString();
    }

    /// <inheritdoc />
    protected override async Task OnProcessAsync(IconSet model, string dir, string fileName, CancellationToken cancellationToken = default)
    {
        (string Name, string? Svg)[] icons =
        [
            (nameof(IconSet.Line), model.Line),
            (nameof(IconSet.Solid), model.Solid),
            (nameof(IconSet.Light), model.Light),
            (nameof(IconSet.Dark), model.Dark),
        ];

        foreach (var (name, svg) in icons)
        {
            if (string.IsNullOrWhiteSpace(svg)) continue;
            var directory = Path.Combine(dir, name.ToLowerInvariant());
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(Path.Combine(directory, fileName), svg, Encoding.UTF8, cancellationToken);
        }
    }

    /// <inheritdoc />
    protected override Task<bool> ExistsAsync(BatchRequirementItem item, CancellationToken cancellationToken = default)
    {
        var dir = GetWorkspaceDirectory().FullName;
        foreach (var name in new[] { "line", "solid", "light", "dark" })
        {
            var path = Path.Combine(dir, name, item.Key + ".svg");
            if (!File.Exists(path)) return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
