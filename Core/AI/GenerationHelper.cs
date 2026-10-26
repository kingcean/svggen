using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using Trivial.Text;
using Violty.SvgGenerator.Models;
using static Trivial.Reflection.ExceptionHandler;

namespace Violty.SvgGenerator.AI;

internal class ResourceGenerationHelper
{
    /// <summary>
    /// Deserializes generation settings from JSON containing nested configuration objects.
    /// </summary>
    /// <param name="s">The JSON format string of settings, or the absolute or relative JSON file path.</param>
    /// <returns>The deserialized settings, without environment fallback. Missing properties remain null.</returns>
    public static JsonObjectNode ParseJson(string? s)
    {
        if (!string.IsNullOrWhiteSpace(s) && s.Contains('{'))
        {
            var obj = JsonObjectNode.TryParse(s);
            if (obj is not null) return obj;
            if (!s.Contains('.') && !s.Contains(Path.PathSeparator)) return [];
        }

        var current = LoadJson(Path.Combine(Environment.CurrentDirectory, "config.json"));
        var application = LoadJson(Path.Combine(AppContext.BaseDirectory, "config.json"));
        var json = LoadJson(s, out var file);
        var chat = json?.TryGetObjectValue("chat") ?? current?.TryGetObjectValue("chat") ?? application?.TryGetObjectValue("chat");
        var workspace = json?.TryGetObjectValue("workspace") ?? current?.TryGetObjectValue("workspace") ?? application?.TryGetObjectValue("workspace") ?? [];
        var dir = workspace.TryGetStringTrimmedValue("dir", true);
        if (dir is null || dir == "./" || dir == ".")
        {
            if (json is not null && file?.Directory is not null && file.Directory.Exists)
            {
                dir = file.Directory.FullName;
                workspace.SetValue("dir", dir);
            }
        }

        return new JsonObjectNode
        {
            { "chat", chat },
            { "workspace", workspace },
        };
    }

    public static JsonObjectNode? LoadJson(string? path, out FileInfo? file)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            file = null;
            return null;
        }

        try
        {
            file = new FileInfo(path);
            if (!file.Exists) return null;
            return JsonObjectNode.TryParse(file);
        }
        catch (Exception ex)
        when (ex is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or NotSupportedException or NotImplementedException or ExternalException)
        {
            file = null;
            return null;
        }
    }

    public static JsonObjectNode? LoadJson(string path)
        => LoadJson(path, out _);

    /// <summary>
    /// Creates chat request options with medium reasoning effort.
    /// </summary>
    /// <returns>A new options instance for a chat request.</returns>
    public static ChatOptions CreateOptions()
        => new()
        {
            Reasoning = new()
            {
                Effort = ReasoningEffort.Medium,
            }
        };

    /// <summary>
    /// Creates the system prompt part for recent items reference.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    /// <param name="sb">The string builder for generating system prompt.</param>
    /// <param name="items">The recent items.</param>
    public static void CreatePromptByRecent<T>(StringBuilder sb, IEnumerable<T> items)
    {
        sb.AppendLine("## Recent generated");
        sb.AppendLine();
        sb.AppendLine("> The following was generated recently and can be used as a reference to maintain stylistic consistency as much as possible.");
        sb.AppendLine();
        sb.AppendLine("```jsonl");
        foreach (var item in items)
        {
            sb.AppendLine(JsonSerializer.Serialize(item));
        }

        sb.AppendLine("```");
        sb.AppendLine();
    }

    /// <summary>
    /// Gets the item by key.
    /// </summary>
    /// <param name="items">The collection of requirement items.</param>
    /// <param name="key">The key used to search.</param>
    /// <returns>The item.</returns>
    public static BatchRequirementItem? Get(IEnumerable<BatchRequirementItem>? items, string key)
    {
        if (items is null) return null;
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item?.Key) && item.Key == key) return item;
        }

        return null;
    }
}
