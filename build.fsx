#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing

let binDir = "bin"
let debugDir = binDir @@ "Debug"
let releaseDir = binDir @@ "Release"
let solutionFile = "Looper.sln"

Target "Clean" (fun _ -> CleanDirs [binDir])

Target "Core" (fun _ ->
    !! "src/**/LooperAnalyzer.csproj"
        |> MSBuildDebug "" "Build"
        |> Log "AppBuild-Output: "
)

Target "Tests" (fun _ ->
    !! "tests/**/*.csproj"
        |> MSBuildDebug "" "Build"
        |> Log "AppBuild-Output: "    
)

Target "All" (fun _ ->
    !! solutionFile
    |> MSBuildDebug "" "Rebuild"
    |> ignore
)

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

