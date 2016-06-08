#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing

let binDir = "bin"
let debugDir = binDir @@ "Debug"
let releaseDir = binDir @@ "Release"
let testArtifacts = binDir @@ "test"
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
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "Test" (fun _ ->
    //tracefn "%A" (debugDir @@ "*.Test.exe")
    !! (debugDir @@ "*.Test.exe")
    |> xUnit2 (fun p -> { p with HtmlOutputPath = Some(testArtifacts @@ "xunit.html") })
)

Target "Default" DoNothing

Target "List" PrintTargets

"Clean" ==> "Core" ==> "Default"

"Clean" ==> "All"

"Clean" ==> "Tests" ==> "Test"

RunTargetOrDefault "Default"

