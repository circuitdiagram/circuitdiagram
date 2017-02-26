using System.IO;

string version = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_VERSION");
string buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");
string branch = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH");

version = version.Substring(0, version.LastIndexOf("."));

string packageVersion = version + "-b" + buildNumber + "-" + branch;

Console.WriteLine($"Package version: {packageVersion}");

var nuspecFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.nuspec", SearchOption.AllDirectories);

Console.WriteLine(Environment.CurrentDirectory);
Console.WriteLine(nuspecFiles);

foreach(var nuspec in nuspecFiles)
{
    Console.WriteLine($"Patching {nuspec}");

    var content = File.ReadAllText(nuspec);
    content = content.Replace("$version$", packageVersion);
    File.WriteAllText(nuspec, content);
}
