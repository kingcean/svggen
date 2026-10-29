using System.Drawing;
using Trivial.Collection;
using Trivial.CommandLine;
using Violty.SvgGenerator;
using Violty.SvgGenerator.CommandLine;
using Violty.SvgGenerator.Models;

var console = StyleConsole.Default;
console.WriteLine(
    new LinearGradientConsoleStyle(ConsoleColor.Magenta, Color.FromArgb(64, 128, 240), Color.FromArgb(216, 64, 240))
    {
        Bold = true,
    },
    "SVG Icon Generator by AI");
console.WriteLine();
var path = CliHelper.GetPath(console);
var service = path is null ? new IconGeneration() : new IconGeneration(path);

while (true)
{
    var select = console.Select(new SelectionData<string>
    {
        { 'g', "gen\tGenerate one", "gen" },
        { 'r', "memory\tAdd an existed to memory for reference", "rem" },
        { 'b', "batch\tBatch generate", "batch" },
        { 'q', "search\tSearch items", "search" },
        { 'i', "ideas\tProvide icon generation ideas", "ideas" },
    }, CliHelper.GetSelectionOptions());
    var sel = (select?.Data ?? select?.Value)?.Trim()?.ToLowerInvariant();
    console.WriteLine();
    if (string.IsNullOrWhiteSpace(sel) || CliHelper.IsToExit(sel)) return;
    switch (sel)
    {
        case "generate":
        case "gen":
        case "g":
            {
                var col = CliHelper.GenerateAsync(console, service);
                await col.CountAsync();
                break;
            }
        case "memory":
        case "rem":
        case "r":
            await CliHelper.AddCache(console, service);
            break;
        case "batch":
        case "b":
            {
                var col = service.BatchProcessAsync();
                var i = 0;
                await foreach (var item in col)
                {
                    i++;
                    console.Append(ConsoleColor.Blue, i);
                    console.Append(ConsoleColor.Blue, i > 9 ? ". " : ".  ");
                    console.Append(ConsoleColor.Green, "Generated ");
                    console.Append(ConsoleColor.Yellow, item.Key);
                    if (!string.IsNullOrWhiteSpace(item.Name))
                    {
                        console.Append(" - ");
                        console.Append(item.Name);
                    }

                    console.WriteLine();
                }

                console.WriteLine("Done.");
            }

            break;
        case "search":
        case "query":
        case "q":
            {
                console.WriteLine("Type the query to search.");
                console.Write("> ");
                var q = console.ReadLine();
                var col = service.SearchItems(q);
                var selection = new SelectionData<BatchRequirementItem>();
                console.WriteLine();
                foreach (var item in col)
                {
                    selection.Add($"{item.Key}\t{item.Name}", item);
                }

                var selected = console.Select(selection, CliHelper.GetSelectionOptions());
                if (selected.Data is null)
                {
                    console.WriteLine(ConsoleColor.Red, "Not found.");
                    break;
                }

                console.WriteLine();
                console.WriteLine(selected.Data.Name);
                console.WriteLine(ConsoleColor.Yellow, selected.Data.Key);
                if (!string.IsNullOrWhiteSpace(selected.Data.Description))
                    console.WriteLine(selected.Data.Description);
                if (selected.Data.Categories is not null && selected.Data.Categories.Count > 0)
                {
                    console.Append("Categories: ");
                    console.Append(string.Join("; ", selected.Data.Categories));
                    console.WriteLine('.');
                }

                console.WriteLine();
                console.Write("Need add to memory?  [y/N]  ");
                var key = console.ReadKey();
                console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.Y:
                    case ConsoleKey.A:
                    case ConsoleKey.C:
                        await service.AddCacheAsync(selected.Data.Key);
                        break;
                    default:
                        break;
                }
            }

            break;
        case "ideas":
        case "i":
            {
                console.WriteLine("Type the ideas.");
                console.Write("> ");
                var q = console.ReadLine();
                var result = await service.IdeasPromptAsync(q, w => console.Write(w), true);
                console.WriteLine();
            }

            break;
        default:
            console.WriteLine("Not supported command.");
            return;
    }

    console.WriteLine();
}
