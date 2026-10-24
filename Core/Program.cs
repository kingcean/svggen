using System.Drawing;
using Trivial.CommandLine;
using Violty.SvgGenerator;

var console = StyleConsole.Default;
console.WriteLine(
    new LinearGradientConsoleStyle(ConsoleColor.Magenta, Color.FromArgb(64, 128, 240), Color.FromArgb(216, 64, 240))
    {
        Bold = true,
    },
    "SVG Icon Generator by AI");
console.WriteLine();
console.Append(ConsoleColor.Blue, "· ");
console.Write("Type the name or the config file:  ");
var name = console.ReadLine();
if (string.IsNullOrEmpty(name)) return;
IconGeneration service;
if (name.Contains('.') || name.Contains(Path.PathSeparator))
{
    service = new(name)!;
    if (service is null)
    {
        console.WriteLine(ConsoleColor.Yellow, "Empty.");
        service = new();
    }
    else
    {
    }

    console.Append(ConsoleColor.Blue, "· ");
    console.Write("Type the icon name:  ");
    name = console.ReadLine();
    if (string.IsNullOrEmpty(name)) return;
}
else
{
    service = new();
}

console.Append(ConsoleColor.Blue, "· ");
console.Append("Type the theme and requirements for the icon, then press ");
console.Append(ConsoleColor.Yellow, "Enter");
console.WriteLine(" to generate.");
console.Write("> ");
var q = console.ReadLine();
if (string.IsNullOrWhiteSpace(q)) return;
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
while(true)
{
    console.Write("Type the icon name:  ");
    name = console.ReadLine();
    if (string.IsNullOrWhiteSpace(name)) return;
    console.Append("Type the theme and requirements for the icon, then press ");
    console.Append(ConsoleColor.Yellow, "Enter");
    console.WriteLine(" to generate.");
    console.Write("> ");
    q = console.ReadLine();
    if (string.IsNullOrWhiteSpace(q)) return;
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
