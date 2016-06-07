#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO


let buildDir = "bin"
let solutionFile = "Looper.sln"

Target "Clean" (fun _ -> CleanDirs [buildDir])

Target "Core" (fun _ ->
    !! "src/**/LooperAnalyzer.csproj"
        |> MSBuildDebug "" "Build"
        |> Log "AppBuild-Output: "
)

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Default" DoNothing

"Clean"
    ==> "Core"
    ==> "Default"

"Clean"
    ==> "Build"

RunTargetOrDefault "Default"