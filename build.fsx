#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO


let solutionFile  = "Looper.sln"

Target "Clean" (fun _ -> CleanDirs ["bin"])

Target "Build" (fun _ ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Default" DoNothing

"Clean"
  ==> "Build"
  ==> "Default"

RunTargetOrDefault "Default"