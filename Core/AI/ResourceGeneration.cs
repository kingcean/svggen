using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trivial.Data;
using Trivial.Text;
using Violty.SvgGenerator.AI;
using Violty.SvgGenerator.Models;
using Violty.SvgGenerator.Settings;

namespace Violty.SvgGenerator;

/// <summary>
/// Provides chat-based resource generation and processing for individual and batch requests.
/// </summary>
/// <typeparam name="T">The model containing the generated resource data.</typeparam>
public abstract class BaseResourceGeneration<T>
{
    private readonly IChatClient? chat;
    private readonly Dictionary<string, string> rules = new();
    private readonly DataCacheCollection<BatchRequirementItemResult<T>> recent = new()
    {
        Expiration = TimeSpan.FromHours(1),
        MaxCount = 100,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseResourceGeneration{T}"/> class.
    /// </summary>
    /// <param name="json">The chat and workspace settings, or <see langword="null"/> to use defaults and environment-based chat settings.</param>
    public BaseResourceGeneration(JsonObjectNode? json)
    {
        json ??= [];
        if (!json.TryDeserializeValue<ChatSettings>("chat", null, out var cache) || cache is null) cache = ChatSettings.FromEnvironment();
        if (!string.IsNullOrWhiteSpace(cache?.ApiKey))
        {
            var options = new OpenAIClientOptions
            {
                Endpoint = new(cache.ApiEndpoint ?? "https://api.openai.com/v1/chat", UriKind.RelativeOrAbsolute),
            };
            var client = new OpenAIClient(new ApiKeyCredential(cache.ApiKey), options);
            chat = new ChatClientBuilder(client.GetChatClient(cache.ChatModel ?? "gpt-6.1-sol").AsIChatClient()).UseFunctionInvocation().Build();
        }

        if (!json.TryDeserializeValue<WorkspaceSettings>("workspace", null, out var workspace)) workspace = null;
        var rules = json.TryGetObjectValue("rules") ?? [];
        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Key) || rule.Value is null || rule.Value.ValueKind != JsonValueKind.String || rule.Value is not IJsonValueNode<string> s) continue;
            this.rules[rule.Key] = s.Value;
        }

        Workspace = workspace;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseResourceGeneration{T}"/> class.
    /// </summary>
    /// <param name="json">A JSON settings string or settings file path, or <see langword="null"/> to look for config.json in the current and application directories.</param>
    public BaseResourceGeneration(string? json = null)
        : this(ResourceGenerationHelper.ParseJson(json))
    {
    }

    /// <summary>
    /// Gets the JSON property name containing batch requirements and the default workspace subdirectory name.
    /// </summary>
    protected abstract string Key { get; }

    /// <summary>
    /// Gets the workspace settings, or <see langword="null"/> if they are not specified.
    /// </summary>
    public WorkspaceSettings? Workspace { get; }

    /// <summary>
    /// Gets the configured workspace directory without creating it.
    /// </summary>
    /// <param name="root">true if returns the root directory of work space; otherwise, false.</param>
    /// <returns>The configured directory, or a subdirectory named by <see cref="Key"/> under the current directory.</returns>
    public DirectoryInfo GetWorkspaceDirectory(bool root = false)
    {
        var dir = Workspace?.Directory ?? Path.Combine(Environment.CurrentDirectory, "artifacts");
        if (!root && Workspace is not null)
        {
            if (Workspace.Folders is not null && Workspace.Folders.TryGetValue(Key, out var folder) && !string.IsNullOrWhiteSpace(folder))
                dir = Path.Combine(dir, folder);
            else if (!string.IsNullOrWhiteSpace(Workspace.OtherResourceFolder))
                dir = Path.Combine(dir, Workspace.OtherResourceFolder.Trim());
        }

        return new DirectoryInfo(dir);
    }

    /// <summary>
    /// Requests resource data from the chat service and deserializes the response.
    /// </summary>
    /// <param name="q">The theme and requirements for the resource.</param>
    /// <param name="cancellationToken">The token used to cancel the chat request.</param>
    /// <returns>The generated model, or the default value of <typeparamref name="T"/> if the request is blank, no chat client is configured, the response has fewer than ten characters, or deserialization returns the default value.</returns>
    /// <exception cref="JsonException">The response is not valid JSON for <typeparamref name="T"/>.</exception>
    public async Task<T?> GetAsync(string q, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || chat is null) return default;
        var messages = chat.GetStreamingResponseAsync([
            new(ChatRole.System, GetPrompt()),
            new(ChatRole.User, q),
        ], ResourceGenerationHelper.CreateOptions(), cancellationToken);
        var sb = new StringBuilder();
        await foreach (var message in messages)
        {
            sb.Append(message.Text);
        }

        if (sb.Length < 10) return default;
        var m = JsonSerializer.Deserialize<T>(sb.ToString());
        return m;
    }

    /// <summary>
    /// Generates some ideas.
    /// </summary>
    /// <param name="q">The input ideas.</param>
    /// <param name="loadAllReferences">true if load all items registered; otherwise, false.</param>
    /// <param name="cancellationToken">The token used to cancel generation and processing.</param>
    /// <returns>The requirement collection.</returns>
    public async Task<List<BatchRequirementItem>> IdeasAsync(string q, bool loadAllReferences, CancellationToken cancellationToken = default)
    {
        var s = await IdeasPromptAsync(q, null, loadAllReferences, cancellationToken);
        if (s is null) return [];
        var m = JsonSerializer.Deserialize<List<BatchRequirementItem>>(s);
        return m ?? [];
    }

    /// <summary>
    /// Generates some ideas.
    /// </summary>
    /// <param name="q">The input ideas.</param>
    /// <param name="cancellationToken">The token used to cancel generation and processing.</param>
    /// <returns>The requirement collection.</returns>
    public Task<List<BatchRequirementItem>> IdeasAsync(string q, CancellationToken cancellationToken = default)
        => IdeasAsync(q, false, cancellationToken);

    /// <summary>
    /// Generates resource data from a request and processes the resulting model into output files.
    /// </summary>
    /// <param name="q">The theme and requirements for the resource.</param>
    /// <param name="fileName">The output file name without a directory path. The .svg extension is appended if missing.</param>
    /// <param name="cancellationToken">The token used to cancel generation and processing.</param>
    /// <returns>The processed model, or the default value of <typeparamref name="T"/> when generation produces no model.</returns>
    /// <exception cref="ArgumentNullException">A model is generated and <paramref name="fileName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A model is generated and <paramref name="fileName"/> is blank, invalid, contains a directory path, or consists solely of an extension.</exception>
    public async Task<T?> ProcesssAsync(string q, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var model = await GetAsync(q, cancellationToken);
        if (model is null) return default;
        await ProcesssAsync(model, fileName, cancellationToken);
        recent.Add(fileName, new(fileName, q, model));
        return model;
    }

    /// <summary>
    /// Generates and processes resources from a JSON batch requirements file.
    /// </summary>
    /// <param name="path">The path to the JSON file containing requirements under <see cref="Key"/>; or null, if load the one in workspace.</param>
    /// <param name="rewrite"><see langword="true"/> to skip items whose output already exists; <see langword="false"/> to process all items with a nonblank key.</param>
    /// <param name="cancellationToken">The token used to cancel existence checks, generation, and processing.</param>
    /// <returns>An asynchronous sequence pairing each successfully generated model with its batch requirement.</returns>
    public async IAsyncEnumerable<BatchRequirementItemResult<T>> BatchProcessAsync(string? path, bool rewrite, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var content = GetItems(path);
        if (content is null) yield break;
        foreach (var item in content)
        {
            if (string.IsNullOrWhiteSpace(item?.Key)) continue;
            if (!rewrite && await ExistsAsync(item, cancellationToken)) continue;
            var sb = new StringBuilder();
            item.ToString(sb);
            if (!string.IsNullOrWhiteSpace(item.Prompt))
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.Append(item.Prompt);
            }

            var model = await GetAsync(sb.ToString(), cancellationToken);
            if (model is null) continue;
            await ProcesssAsync(model, item.Key, cancellationToken);
            if (model is null) continue;
            recent.Add(item.Key, new(item, model));
            yield return new(item, model);
        }
    }

    /// <summary>
    /// Generates and processes all batch items with a nonblank key without checking for existing output.
    /// </summary>
    /// <param name="path">The path to the JSON file containing requirements under <see cref="Key"/>; or null, if load the one in workspace.</param>
    /// <param name="cancellationToken">The token used to cancel generation and processing.</param>
    /// <returns>An asynchronous sequence pairing each successfully generated model with its batch requirement.</returns>
    public IAsyncEnumerable<BatchRequirementItemResult<T>> BatchProcessAsync(string? path = null, CancellationToken cancellationToken = default)
        => BatchProcessAsync(path, false, cancellationToken);

    /// <summary>
    /// Gets the design rule associated with the specified key; uses the current Key when no key is provided.
    /// </summary>
    /// <param name="key">The rule key to look up; uses the current Key if null.</param>
    /// <returns>The rule if found and its value is not null, empty, or whitespace; otherwise, null.</returns>
    public string? GetDesignRule(string? key)
        => rules.TryGetValue(key ?? Key, out var rule) && !string.IsNullOrWhiteSpace(rule) ? rule : null;

    /// <summary>
    /// Gets the recent item generated.
    /// </summary>
    /// <returns>A collection of items.</returns>
    public IEnumerable<BatchRequirementItemResult<T>> Recent()
        => recent.ToList().OrderByDescending(item => item.UpdateDate).Select(item => item.Value).Where(item => item is not null);

    /// <summary>
    /// Adds an exist model to cache.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="cancellationToken">The token used to cancel writing the output.</param>
    /// <returns>The item result added; or null, if not exist.</returns>
    public async Task<BatchRequirementItemResult<T>?> AddCache(string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        var item = await GetFromLocal(fileName, cancellationToken);
        if (item is null) return null;
        recent.Add(item.Key, item);
        return item;
    }
    /// <summary>
    /// Generates some ideas.
    /// </summary>
    /// <param name="q">The input ideas.</param>
    /// <param name="console">true if used to console output; otherwise, false.</param>
    /// <param name="loadAllReferences">true if load all items registered; otherwise, false.</param>
    /// <param name="cancellationToken">The token used to cancel generation and processing.</param>
    /// <returns>The requirement collection.</returns>
    internal async Task<string?> IdeasPromptAsync(string q, Action<string>? console, bool loadAllReferences, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || chat is null) return null;
        var prompt = DefaultPrompts.Structs;
        var line = console is null
            ? "- Do not include any conversational text or explanations in the output."
            : "- Represent each creative idea as a JSON object, with each object on a single line preceded by four spaces of indentation.";
        var col = GetItems();
        if (col is not null)
        {
            if (!loadAllReferences) col = col.Take(100).ToList();
            prompt = string.Concat(prompt, Environment.NewLine, line, Environment.NewLine, Environment.NewLine, "## References", Environment.NewLine, Environment.NewLine, "```json", Environment.NewLine, JsonSerializer.Serialize(col), Environment.NewLine, "```");
        }
        else
        {
            prompt = string.Concat(prompt, Environment.NewLine, line);
        }

        var messages = chat.GetStreamingResponseAsync([
            new(ChatRole.System, prompt),
            new(ChatRole.User, q),
        ], ResourceGenerationHelper.CreateOptions(), cancellationToken);
        var sb = new StringBuilder();
        if (console is not null)
        {
            await foreach (var message in messages)
            {
                sb.Append(message.Text);
                console(message.Text);
            }
        }
        else
        {
            await foreach (var message in messages)
            {
                sb.Append(message.Text);
            }
        }

        var s = sb.ToString().Trim();
        if (s.Length < 10) return null;
        return s;
    }

    /// <summary>
    /// Gets the requirement items in batch file.
    /// </summary>
    /// <param name="path">The path of the batch config file; or null, if load the one in workspace.</param>
    /// <returns>A collection of the requirement items in the batch file.</returns>
    protected List<BatchRequirementItem>? GetItems(string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = Path.Combine(GetWorkspaceDirectory(false).FullName, "content.json");
        var file = new FileInfo(path);
        if (!file.Exists) return null;
        var json = JsonObjectNode.TryParse(file);
        return json.DeserializeValue<List<BatchRequirementItem>>(Key);
    }

    /// <summary>
    /// Gets the generation prompt for LLM.
    /// </summary>
    /// <returns>The system prompt.</returns>
    protected abstract string GetPrompt();

    /// <summary>
    /// Determines whether the output for a batch requirement already exists.
    /// </summary>
    /// <param name="item">The batch requirement whose output is checked.</param>
    /// <param name="cancellationToken">The token used to cancel the check.</param>
    /// <returns><see langword="true"/> if the expected output exists; otherwise, <see langword="false"/>.</returns>
    protected abstract Task<bool> ExistsAsync(BatchRequirementItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a generated resource model to the output directory.
    /// </summary>
    /// <param name="model">The generated resource model to write.</param>
    /// <param name="container">The output directory path.</param>
    /// <param name="fileName">The validated output file name, including the .svg extension.</param>
    /// <param name="cancellationToken">The token used to cancel writing the output.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    protected abstract Task OnProcessAsync(T model, string container, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an exist model to cache.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="cancellationToken">The token used to cancel writing the output.</param>
    /// <returns>The item result added; or null, if not exist.</returns>
    protected abstract Task<BatchRequirementItemResult<T>?> GetFromLocal(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes resource data into output files in the configured workspace or the default resource directory.
    /// </summary>
    /// <param name="model">The resource model to process.</param>
    /// <param name="fileName">The output file name without a directory path. The .svg extension is appended if missing.</param>
    /// <param name="cancellationToken">The token used to cancel processing.</param>
    /// <returns>The processed model.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="model"/> or <paramref name="fileName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="fileName"/> is blank, invalid, contains a directory path, or consists solely of an extension.</exception>
    private async Task ProcesssAsync(T model, string fileName, CancellationToken cancellationToken = default)
    {
        if (fileName is "." or ".." ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            fileName.Contains('/') || fileName.Contains('\\') ||
            fileName.EndsWith('.') || fileName.EndsWith(' '))
            throw new ArgumentException("A valid file name must be provided and must not include a directory path.", nameof(fileName));
        if (!fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            fileName += ".svg";
        if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(fileName)))
            throw new ArgumentException("The file name must not consist solely of an extension.", nameof(fileName));
        var dir = GetWorkspaceDirectory().FullName;
        Directory.CreateDirectory(dir);
        await OnProcessAsync(model, dir, fileName, cancellationToken);
    }
}
