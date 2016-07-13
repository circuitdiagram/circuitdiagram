using System.IO;

if (false && Environment.GetEnvironmentVariable("APPVEYOR") == null)
{
    Console.WriteLine("Not running on AppVeyor");
    return;
}

string version = Environment.GetEnvironmentVariable("APPVEYOR_BULID_VERSION");
string branch = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH");
string packageVersion = version + "-" + branch;

var nuspecFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.nuspec", SearchOption.AllDirectories);

foreach(var nuspec in nuspecFiles)
{
    Console.WriteLine($"Patching {nuspec}");

    var content = File.ReadAllText(nuspec);
    content = content.Replace("$version$", packageVersion);
    File.WriteAllText(nuspec, content);
}
