#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open System

let cwd = __SOURCE_DIRECTORY__ 
Environment.CurrentDirectory <- cwd

let solutionFile = "Looper.sln"
let outDir = "output"
let binDir = outDir @@ "bin"
let testsDir = outDir @@ "tests"
let publishDir = outDir @@ "publish"

let configuration = 
    match getBuildParam "Configuration" with
    | "" -> environVarOrDefault "Configuration" "Debug"
    | c  -> c

Target "Clean" (fun _ -> 
    CleanDirs [outDir]
    CleanDirs !! "./**/bin/"
    CleanDirs !! "./**/obj/"
)

let build includes () =
    includes
    |> MSBuild "" "Build" ["Configuration", configuration]
    |> Log ("AppBuild-" + configuration + " Output: ")

Target "Build" (!! solutionFile |> build)

Target "Xunit" (fun _ ->
    CreateDir testsDir
    let xunitOut = cwd @@ testsDir @@ "xunit.html"
    
    !! (binDir @@ "*.Test.exe")
    |> xUnit2 (fun p -> 
        { p with HtmlOutputPath = Some xunitOut
                 Parallel = ParallelMode.All })
)

Target "List" PrintTargets
Target "Default" DoNothing

"Clean"
    ==> "Build" 
    ==> "Default"
    ==> "Xunit"

RunTargetOrDefault "Default"