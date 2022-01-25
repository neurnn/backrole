@echo off

dotnet pack --configuration Release 
dotnet nuget push "src/Backrole.Http/bin/Release/Backrole.Http.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Http.Abstractions/bin/Release/Backrole.Http.Abstractions.1.0.0.nupkg" --source "github"

dotnet nuget push "src/Backrole.Http.Transports.Nova/bin/Release/Backrole.Http.Transports.Nova.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Http.Transports.HttpSys/bin/Release/Backrole.Http.Transports.HttpSys.1.0.0.nupkg" --source "github"

dotnet nuget push "src/Backrole.Http.Routings/bin/Release/Backrole.Http.Routings.1.0.0.nupkg" --source "github"
dotnet nuget push "src/Backrole.Http.Routings.Abstractions/bin/Release/Backrole.Http.Routings.Abstractions.1.0.0.nupkg" --source "github"

dotnet nuget push "src/Backrole.Http.StaticFiles/bin/Release/Backrole.Http.StaticFiles.1.0.0.nupkg" --source "github"

pause