module ASTAnalyzer

open System
open System.IO
open FSharp.Formatting.CodeFormat

// may have to use the environment's new line
let repos = (File.ReadAllText("flagged_repos_filelist.txt")).Split("\r\n")

printfn "%A" repos
(*let source = File.ReadAllText(__SOURCE_DIRECTORY__ + """\in\test\test2.fs""")*)

printfn "read file - converting to snippets"

// IO count will only read IO once for each time IO appears.
let isSystemIO (ts: ToolTipSpans) =
    let aux (t: ToolTipSpan) =
        match t with
        | Literal s -> s = "namespace System.IO"
        | _ -> false

    aux (List.head ts)

let getSnippets (source: string) = async{
    return CodeFormatter.ParseAndCheckSource("C:\\snippet.fsx", source, None, None, ignore)
}

let parseLines (source: string) (path: string) =
    let mutable MutableCount = 0
    let mutable DoCount = 0
    let mutable LoopCount = 0
    let mutable ArrayCount = 0
    let mutable ExceptionCount = 0
    let mutable IOCount = 0
    let mutable TokenCount = 0
    let snippets, _ = CodeFormatter.ParseAndCheckSource("C:\\snippet.fsx", source, None, None, ignore)
        
    let (Snippet (title, lines)) = snippets |> Seq.head

    for (Line (_, tokens)) in lines do
        for token in tokens do
            TokenCount <- TokenCount + 1

            match token with
            | TokenSpan.Token (kind, code, tip) ->
                //printfn "Token: %A" token // for debugging
                match kind with
                | TokenKind.Keyword ->
                    match code with
                    | "do" -> DoCount <- DoCount + 1
                    | "mutable" -> MutableCount <- MutableCount + 1
                    | "for"
                    | "while" -> LoopCount <- LoopCount + 1
                    | "<-" -> MutableCount <- MutableCount + 1
                    | "try"
                    | "raise" -> ExceptionCount <- ExceptionCount + 1
                    | _ -> ()
                | TokenKind.Punctuation ->
                    match code with
                    | "[|" -> ArrayCount <- ArrayCount + 1
                    | _ -> ()
                | TokenKind.Function ->
                    match code with
                    | "ref" -> MutableCount <- MutableCount + 1
                    | "failwith" -> ExceptionCount <- ExceptionCount + 1
                    | _ -> ()
                | TokenKind.Identifier ->
                    match code with
                    | "IO" ->
                        if Option.isSome tip then
                            if (isSystemIO (Option.get tip)) then
                                IOCount <- IOCount + 1
                    | "failwith"
                    | "failwithf" -> ExceptionCount <- ExceptionCount + 1
                    | _ -> ()
                | TokenKind.Operator ->
                    match code with
                    | ":=" -> MutableCount <- MutableCount + 1
                    | _ -> ()
                | _ -> ()
            | _ -> ()

    (path, MutableCount, DoCount, LoopCount, ArrayCount, ExceptionCount, IOCount, TokenCount)

// given a path, recursively visit all *.fs files, then output a csv file of the result
let writeCSV (in_path:string) (out_name:string) =
    use sw = File.CreateText($"{out_name}.csv")
    // return an iterator of the paths
    
    let paths =
        Directory.EnumerateFiles(($"../in/flagged_repos/{in_path}"), "*.fs", SearchOption.AllDirectories)

    do
        sw.WriteLine(
            "Path, MutableCount, DoCount, LoopCount, ArrayCount, ExceptionCount, IOCount, TokenCount"
        )

    for path in paths do
        printfn "Parsing %s" path
        let result = parseLines (File.ReadAllText(path)) path

        let writeLine
            (
                path: string,
                mutableCount,
                doCount,
                loopCount,
                arrayCount,
                exceptionCount,
                ioCount,
                tokenCount
            ) =

            let toWrite =
                $"'{path}', {mutableCount}, {doCount}, {loopCount}, {arrayCount}, {exceptionCount}, {ioCount}, {tokenCount}"

            sw.WriteLine(toWrite)
        writeLine result

for repo in repos do
    if not (repo.StartsWith("#")) && not (System.String.IsNullOrWhiteSpace(repo))
    then
        let s1 = System.String.Concat(repo, "/before")
        let s2 = System.String.Concat("out/", repo, "_before")
        let s3 = System.String.Concat(repo, "/after")
        let s4 = System.String.Concat("out/", repo, "_after")
        do printfn "writing output for %s" repo
        try
            writeCSV s1 s2
            writeCSV s3 s4
        with 
            | :? System.Exception as ex -> printfn "Exception occured: %s" (ex.ToString())
    
