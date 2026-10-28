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
        var m = await service.AddCacheAsync(q, cancellationToken);
        if (m is not null)
        {
            console.Append(ConsoleColor.Green, "Added. \t");
            console.WriteLine(m.Name);
            console.Append("Would you like to add another? Enter the corresponding key or press ");
            console.Append(ConsoleColor.Yellow, "Enter");
            console.WriteLine(" to skip.");
        }
        else
        {
            console.WriteLine(ConsoleColor.Red, "Not found.");
            console.Append("Try again? Enter the corresponding key or press ");
            console.Append(ConsoleColor.Yellow, "Enter");
            console.WriteLine(" to cancel.");
        }

        console.Write("> ");
        q = console.ReadLine();
        if (string.IsNullOrWhiteSpace(q) || IsToExit(q) || q == "no") return;
        m = await service.AddCacheAsync(q, cancellationToken);
        if (m is not null)
        {
            console.Append(ConsoleColor.Green, "Added. \t");
            console.WriteLine(m.Name);
        }
        else
        {
            console.WriteLine(ConsoleColor.Red, "Not found.");
        }
    }

    public static async IAsyncEnumerable<T> GenerateAsync<T>(StyleConsole? console, BaseResourceGeneration<T> service)
    {
        console ??= StyleConsole.Default;
        while (true)
        {
            console.Write("Type the key to generate a new icon; or press ");
            console.Append(ConsoleColor.Yellow, "Enter");
            console.WriteLine(" to quit.");
            console.Write("> ");
            var name = console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) yield break;
            console.WriteLine("Type the theme and requirements for the icon.");
            console.Write("> ");
            var q = console.ReadLine();
            if (string.IsNullOrWhiteSpace(q) || IsToExit(q)) yield break;
            console.Write("Generating…");
            T? m;
            try
            {
                m = await service.ProcesssAsync(q, name);
                console.Clear(StyleConsole.RelativeAreas.Line);
                console.BackspaceToBeginning();
                console.WriteLine(ConsoleColor.Green, "Generated.");
            }
            catch (Exception ex)
            {
                console.Clear(StyleConsole.RelativeAreas.Line);
                console.BackspaceToBeginning();
                console.Append(ConsoleColor.Red, "Error \t");
                console.WriteLine(ex.Message);
                throw;
            }

            console.WriteLine();
            if (m is not null) yield return m;
        }
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
