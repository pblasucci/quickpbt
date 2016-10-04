#I __SOURCE_DIRECTORY__
#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing
open Fake.Testing.XUnit2
open System
open System.Diagnostics
open System.IO
open System.Text

(* -------------------------------------------------------------------------- *)

let [<Literal>] TestReport = "TestResults.html"

/// configures execution of the test suite
let configure (args :XUnit2Params) =
  { args with
      ToolPath        = "packages/xunit.runner.console/tools/xunit.console.exe"
      IncludeTraits   = "section" 
                        |> splitEnvironVar
                        |> List.map (fun v -> ("section",v))
      HtmlOutputPath  = Some TestReport }

/// launched HTML report (after doing a bit of reformating)
let show () = 
  let oldLines =  File.ReadAllLines (TestReport,Encoding.UTF8)
  let newLines =  oldLines
                  |> Seq.map (fun l -> l.Replace (@"\r\n","<br/>"))
                  |> Seq.toArray
  
  File.WriteAllLines (TestReport,newLines,Encoding.UTF8)

  TestReport
  |> Process.Start
  |> ignore
  
/// attempts to run tests and will, conditionally, launch an HTML report
let run report paths = 
  try
    paths |> xUnit2 configure
    if report then show ()
  with
    | :? FailedTestsException -> if report then show ()
    | x                       -> traceError x.Message

(* -------------------------------------------------------------------------- *)

Target "build" (fun _ ->
  !! "quickpbt.sln"
  |> MSBuildRelease "" "Rebuild"
  |> ignore)

Target "all_tests" (fun _ -> 
  !! "**/bin/Release/quickpbt_??.dll" 
  |> run (getEnvironmentVarAsBool "report"))

Target "fs_tests" (fun _ -> 
  !! "**/bin/Release/quickpbt_fs.dll" 
  |> run (getEnvironmentVarAsBool "report"))

Target "cs_tests" (fun _ -> 
  !! "**/bin/Release/quickpbt_cs.dll" 
  |> run (getEnvironmentVarAsBool "report"))

Target "vb_tests" (fun _ -> 
  !! "**/bin/Release/quickpbt_vb.dll" 
  |> run (getEnvironmentVarAsBool "report"))

Target "show_report" (fun _ -> show ())

(* -------------------------------------------------------------------------- *)

"build" ==> "all_tests"
"build" ==> "fs_tests"
"build" ==> "cs_tests"
"build" ==> "vb_tests"

(* -------------------------------------------------------------------------- *)

RunTargetOrDefault "all_tests"
