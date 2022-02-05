#!/bin/sh

dotnet pack --configuration Release 
dotnet nuget push "src/Backrole.Orp/bin/Release/Backrole.Orp.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Orp.Abstractions/bin/Release/Backrole.Orp.Abstractions.1.0.0.nupkg" --source "github"
