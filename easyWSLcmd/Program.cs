// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.Diagnostics;
using easyWslCmd;
using easyWslLib;

var rootCommand = new RootCommand("Easy WSL");

var distroName = new Option<string>("--name", "Name to assign to the new WSL distro");
var imageName = new Option<string>("--image", "dockerhub image to base new distro on");
var outputPath = new Option<DirectoryInfo>("--output", "Where to install the distro");
outputPath.SetDefaultValue(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "easyWSL")));

var import = new Command("import", "Import a WSL distro from a dockerhub image");
import.Add(distroName);
import.Add(imageName);
import.Add(outputPath);
rootCommand.AddCommand(import);



import.SetHandler(async (name, image, output) =>
{
    if (string.IsNullOrWhiteSpace(name))
    {
        throw new ArgumentException("missing required parameter", nameof(name));
    }

    if (string.IsNullOrWhiteSpace(image))
    {
        throw new ArgumentException("missing required parameter", nameof(image));
    }
    var tempPath = Path.Combine(Path.GetTempPath(), "easyWSL");
    var downloader = new DockerDownloader(tempPath, new PlatformHelpers());

    output = output.CreateSubdirectory(name);


    Console.WriteLine($"Downloading {image}");
    await downloader.DownloadImage(image);
    Console.WriteLine("Combining layers");
    await downloader.CombineLayers();
    Console.WriteLine("Registering distro");
    Process.Start("wsl.exe", new[] { $"--import", name, output.FullName, Path.Combine(tempPath, "install.tar.bz") }).WaitForExit();
}, distroName, imageName, outputPath);

return await rootCommand.InvokeAsync(args);