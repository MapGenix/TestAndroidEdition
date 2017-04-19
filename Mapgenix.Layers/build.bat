MSBuild /target:Clean
del *.nupkg
MSBuild
nuget setApiKey Your-API-Key
nuget pack -Symbols
nuget push *.nupkg -Source c:\third_party\NuGetFeed