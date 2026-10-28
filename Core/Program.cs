using System.Drawing;
using Trivial.Collection;
using Trivial.CommandLine;
using Violty.SvgGenerator;
using Violty.SvgGenerator.CommandLine;

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
        { "gen\tGenerate one", "gen" },
        { "rem\tAdd an existed to memory for reference", "rem" },
        { "batch\tBatch generate", "batch" },
        { "ideas\tProvide icon generation ideas", "ideas" }
    }, CliHelper.GetSelectionOptions());
    var sel = (select?.Data ?? select?.Value)?.Trim()?.ToLowerInvariant();
    console.WriteLine();
    if (string.IsNullOrWhiteSpace(sel) || CliHelper.IsToExit(sel)) return;
    switch (sel)
    {
        case "gen":
            {
                var col = CliHelper.GenerateAsync(console, service);
                await col.CountAsync();
                break;
            }
        case "rem":
            await CliHelper.AddCache(console, service);
            break;
        case "batch":
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
        case "ideas":
            {
                console.WriteLine("Type the ideas.");
                console.Write("> ");
                var q = console.ReadLine();
                var result = await service.IdeasPromptAsync(q, w => console.Write(w), true);
            }

            break;
        default:
            console.WriteLine("Not supported command.");
            return;
    }

    console.WriteLine();
}
