# Install C# script support
cinst scriptcs

# Patch *.nuspec files
scriptcs Scripts\NuSpecPatch.csx

# Package
nuget pack CircuitDiagram\CircuitDiagramCore\CircuitDiagramCore.nuspec

# Publish
Get-ChildItem .\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
