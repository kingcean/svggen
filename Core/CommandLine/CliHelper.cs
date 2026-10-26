using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Trivial.CommandLine;

namespace Violty.SvgGenerator.CommandLine;

internal static class CliHelper
{
    public static string? GetPath(StyleConsole? console)
    {
        console ??= StyleConsole.Default;
        var path = Path.Combine(Environment.CurrentDirectory, "config.json");
        if (File.Exists(path)) return path;
        console.Write("Type the path of config file, or press ");
        console.Append(ConsoleColor.Yellow, "Enter");
        console.WriteLine(" to use local one.");
        console.Write("> ");
        path = console.ReadLine();
        if (string.IsNullOrWhiteSpace(path) || IsToExit(path)) return null;
        return path;
    }

    public static async Task AddCache<T>(StyleConsole? console, BaseResourceGeneration<T> service, CancellationToken cancellationToken = default)
    {
        if (service is null) return;
        console ??= StyleConsole.Default;
        console.Append("Would you like to add any existing references? If so, enter the corresponding key; otherwise, press ");
        console.Append(ConsoleColor.Yellow, "Enter");
        console.WriteLine('.');
        console.Write("> ");
        var q = console.ReadLine();
        if (string.IsNullOrWhiteSpace(q) || IsToExit(q) || q == "no") return;
        await service.AddCache(q, cancellationToken);
        console.Append("Would you like to add another? Enter the corresponding key or press ");
        console.Append(ConsoleColor.Yellow, "Enter");
        console.WriteLine(" to skip.");
        console.Write("> ");
        q = console.ReadLine();
        if (string.IsNullOrWhiteSpace(q) || IsToExit(q) || q == "no") return;
        await service.AddCache(q, cancellationToken);
    }

    public static SelectionConsoleOptions GetSelectionOptions()
        => new()
        {
            Prefix = "· ",
            SelectedPrefix = "→ ",
            SelectedForegroundConsoleColor = ConsoleColor.Cyan,
            SelectedForegroundRgbColor = Color.FromArgb(0x00, 0xD6, 0xFF),
            SelectedBackgroundConsoleColor = null,
            SelectedBackgroundRgbColor = null,
            MaxRow = 20,
        };

    public static bool IsToExit(string? command)
    {
        command = command?.Trim()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(command)) return false;
        return command == "exit" || command == "quit" || command == "close" || command == "esc" || command == "bye" || command == "goodbye" || command == "tuichu" || command == "退出" || command == "关闭" || command == "再见" || command == "结束";
    }
}
