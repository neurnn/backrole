#!/bin/sh

dotnet pack --configuration Release 
dotnet nuget push "src/Backrole.Crypto/bin/Release/Backrole.Crypto.1.0.0.nupkg" --source "github"
