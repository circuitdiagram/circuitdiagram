# Install C# script support
cinst scriptcs

# Patch *.nuspec files
scriptcs Scripts\NuSpecPatch.csx

# Package
Get-ChildItem -recurse .\*.nuspec | % { nuget pack $_.FullName }

# Publish
Get-ChildItem .\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
