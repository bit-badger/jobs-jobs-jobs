#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.JavaScript.Npm
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open Fake.Core.TargetOperators

/// The path to the Vue client
let clientPath = "src/JobsJobsJobs/App"

/// The path to the API server
let serverPath = "src/JobsJobsJobs/Server"

Target.initEnvironment ()

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    ++ $"{serverPath}/wwwroot"
    |> Shell.cleanDirs
)

Target.create "BuildClient" (fun _ ->
    let inClientPath (opts : Npm.NpmParams) = { opts with WorkingDirectory = clientPath;  }
    Npm.exec "i --legacy-peer-deps" inClientPath
    Npm.run  "build"                inClientPath
)

Target.create "BuildServer" (fun _ ->
    DotNet.build (fun opts -> { opts with NoLogo = true }) serverPath
)

Target.create "RunServer" (fun _ ->
    DotNet.exec (fun opts -> { opts with WorkingDirectory = serverPath }) "run" "" |> ignore
)

Target.create "Publish" (fun _ ->
    DotNet.publish
        (fun opts -> { opts with Runtime = Some "linux-x64"; SelfContained = Some false; NoLogo = true })
        serverPath
)

Target.create "BuildAndRun" ignore
Target.create "All" ignore

"Clean"
  ==> "All"
"Clean"
  ==> "Publish"
"Clean"
  ?=> "BuildClient"
"Clean"
  ==> "BuildAndRun"

"BuildClient"
  ==> "All"
"BuildClient"
  ?=> "BuildServer"
"BuildClient"
  ?=> "RunServer"
"BuildClient"
  ==> "BuildAndRun"
"BuildClient"
  ==> "Publish"

"BuildServer"
  ==> "All"

"RunServer"
  ==> "BuildAndRun"

Target.runOrDefault "All"
