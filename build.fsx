#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing

let binDir = "bin"
let debugDir = binDir @@ "Debug"
let releaseDir = binDir @@ "Release"
let solutionFile = "Looper.sln"

let configuration = 
    match getBuildParam "Configuration" with
    | "" -> environVarOrDefault "Configuration" "Debug"
    | c  -> c

Target "Clean" (fun _ -> 
    CleanDirs [binDir]
    CleanDirs !! "./**/bin/"
    CleanDirs !! "./**/obj/"
)

let build includes () =
    includes
    |> MSBuild "" "Build" ["Configuration", configuration]
    |> Log "AppBuild-Output: "

Target "Core" (!! "src/**/LooperAnalyzer.csproj" |> build)

Target "Tests" (!! "tests/**/*.csproj" |> build)

Target "All" (!! solutionFile |> build)

Target "Xunit" (fun _ ->
    !! (debugDir @@ "*.Test.exe")
    |> xUnit2 (fun p -> 
        { p with HtmlOutputPath = Some(debugDir @@ "xunit.html")
                 Parallel = ParallelMode.All })
)

Target "Default" DoNothing

Target "List" PrintTargets

"Clean" ==> "Core" ==> "Default"

"Clean" ==> "All"

"Clean" ==> "Tests" ==> "Xunit"

RunTargetOrDefault "Default"

