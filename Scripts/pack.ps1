# Install C# script support
cinst scriptcs

# Patch *.nuspec files
scriptcs NuSpecPatch.csx

# Package
nuget pack CircuitDiagram/CircuitDiagramCore/CircuitDiagramCore.nuspec

# Publish
Get-ChildItem .\CircuitDiagram\CircuitDiagramCore\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
