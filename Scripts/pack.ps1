# Install C# script support
cinst scriptcs

"Patching *.nuspec files"
scriptcs Scripts\NuSpecPatch.csx

"Packaging *.nuspec files"
Get-ChildItem -recurse .\*.nuspec | % { nuget pack $_.FullName }

"Publishing *.nuspec files"
Get-ChildItem .\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
