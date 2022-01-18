@echo off

dotnet pack --configuration Release 
dotnet nuget push "src/Backrole.Core/bin/Release/Backrole.Core.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Core.Abstractions/bin/Release/Backrole.Core.Abstractions.1.0.0.nupkg" --source "github"

pause