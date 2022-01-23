#!/bin/sh

dotnet pack --configuration Release 
dotnet nuget push "src/Backrole.Http/bin/Release/Backrole.Http.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Http.Abstractions/bin/Release/Backrole.Http.Abstractions.1.0.0.nupkg" --source "github"

dotnet nuget push "src/Backrole.Http.Transports.Nova/bin/Release/Backrole.Transports.Nova.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Http.Transports.HttpSys/bin/Release/Backrole.Transports.HttpSys.1.0.0.nupkg" --source "github"

