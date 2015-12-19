

let referenceBinaries = [ "Suave.dll" ; "Suave.Testing.dll" ]
let website = "http://suave.io"
let githubLink = "https://github.com/SuaveIO/suave"
let info =
  [ "project-name", "Suave.IO"
    "project-author", "Henrik Feldt, Ademar Gonzalez"
    "project-summary", "An lightweight web framework in F#."
    "project-github", githubLink
    "project-nuget", "http://www.nuget.org/packages/Suave" ]

#I "../../src/Suave/bin/Release"
#I "../../src/Suave.Testing/bin/Release"

// --------------------------------------------------------------------------------------
// For typical project, no changes are needed below
// --------------------------------------------------------------------------------------


#I "../../packages/docs/FSharp.Compiler.Service/lib/net40"
#I "../../packages/docs/FAKE/tools"
#load "../../packages/docs/FSharp.Formatting/FSharp.Formatting.fsx"
#r "FakeLib.dll"
#r "FSharp.Markdown.dll"
#r "FSharp.Literate.dll"
#r "FSharp.CodeFormat.dll"
#r "CSharpFormat.dll"
#r "FSharp.MetadataFormat.dll"
open System.IO
open Fake
open Fake.FileHelper
open FSharp.Literate
open FSharp.MetadataFormat

let (@@) x y = System.IO.Path.Combine(x,y)

let root = website

// Paths with template/source/output locations
let content    = __SOURCE_DIRECTORY__ @@ ".." @@ "content"
let output     = __SOURCE_DIRECTORY__ @@ ".." @@ "output"
let files      = __SOURCE_DIRECTORY__ @@ ".." @@ "files"
let formatting = __SOURCE_DIRECTORY__ @@ "../../packages/docs/FSharp.Formatting/"

let layoutRoots =
  [ files @@ "_fs_formatting"
    files @@ "_fs_formatting/reference" ]

// Copy static files and CSS + JS from F# Formatting
let copyFiles () =
  CopyRecursive files output true |> Log "Copying file: "
  ensureDirectory (output @@ "content")
  CopyRecursive (formatting @@ "styles") (output @@ "content") true
    |> Log "Copying styles and scripts: "

System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
// Build API reference from XML comments
let buildReference () =
  CleanDir (output @@ "reference")
  ensureDirectory output
  let binaries =
    referenceBinaries
    |> List.map (fun lib-> __SOURCE_DIRECTORY__ @@ ".." @@ ".." @@ "src" @@ Path.GetFileNameWithoutExtension lib @@ "bin" @@ "Release" @@ lib)

  let dirs =
    referenceBinaries
    |> List.map (fun lib-> __SOURCE_DIRECTORY__ @@ ".." @@ ".." @@ "src" @@ Path.GetFileNameWithoutExtension lib @@ "bin" @@ "Release")

  MetadataFormat.Generate
    ( binaries , output @@ "reference", layoutRoots,
      parameters = ("root", root)::info,
      sourceRepo = githubLink @@ "tree/master",
      sourceFolder = __SOURCE_DIRECTORY__ @@ ".." @@ "..",
      libDirs = dirs,
      publicOnly = true,
      markDownComments = false)

// Build documentation from `fsx` and `md` files in `docs/content`
(*
let buildDocumentation () =
  let subdirs = Directory.EnumerateDirectories(content, "*", SearchOption.AllDirectories)
  for dir in Seq.append [content] subdirs do
    let sub = if dir.Length > content.Length then dir.Substring(content.Length + 1) else "."
    Literate.ProcessDirectory
      ( dir, docTemplate, output @@ sub, replacements = ("root", root)::info,
        layoutRoots = layoutRoots, generateAnchors = true )
*)

// Generate
CleanDir output
CreateDir output
copyFiles()
//buildDocumentation()
buildReference()
