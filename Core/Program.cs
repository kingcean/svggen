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
var select = console.Select(new SelectionData<string>
{
    { "gen\tGenerate an icon by bootstrapper", "gen" },
    { "batch\tBatch generate icons", "batch" },
    { "ideas\tProvide icon generation ideas", "ideas" }
}, CliHelper.GetSelectionOptions());
var sel = (select?.Data ?? select?.Value)?.Trim()?.ToLowerInvariant();
if (string.IsNullOrWhiteSpace(sel) || sel == "bye") return;
switch (sel)
{
    case "gen":
        await CliHelper.AddCache(console, service);
        while (true)
        {
            console.Write("Type the key to generate a new icon; or press ");
            console.Append(ConsoleColor.Yellow, "Enter");
            console.WriteLine(" to quit.");
            console.Write("> ");
            var name = console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) return;
            console.WriteLine("Type the theme and requirements for the icon.");
            console.Write("> ");
            var q = console.ReadLine();
            if (string.IsNullOrWhiteSpace(q) || CliHelper.IsToExit(q)) return;
            console.Write("Generating…");
            try
            {
                var m = await service.ProcesssAsync(q, name);
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
            }

            console.WriteLine();
        }
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
        }

        break;
    case "ideas":
        {
            console.WriteLine("Type the ideas.");
            console.Write("> ");
            var q = console.ReadLine();
            var result = await service.IdeasPromptAsync(q, w => console.Write(w), true);
            console.WriteLine();
        }

        break;
}
