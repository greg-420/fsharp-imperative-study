// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

// LexBuffers are for use with automatically generated lexical analyzers,
// in particular those produced by 'fslex'.

namespace FSharp.Compiler.Text

/// Represents an input to the F# compiler
type ISourceText =

    /// Gets a character in an input based on an index of characters from the start of the file
    abstract Item: index: int -> char with get

    /// Gets a line of an input by index
    abstract GetLineString: lineIndex: int -> string

    /// Gets the count of lines in the input
    abstract GetLineCount: unit -> int

    /// Gets the last character position in the input, returning line and column
    abstract GetLastCharacterPosition: unit -> int * int

    /// Gets a section of the input
    abstract GetSubTextString: start: int * length: int -> string

    /// Checks if a section of the input is equal to the given string
    abstract SubTextEquals: target: string * startIndex: int -> bool

    /// Gets the total length of the input in characters
    abstract Length: int

    /// Checks if one input is equal to another
    abstract ContentEquals: sourceText: ISourceText -> bool

    /// Copies a section of the input to the given destination ad the given index
    abstract CopyTo: sourceIndex: int * destination: char[] * destinationIndex: int * count: int -> unit

/// Functions related to ISourceText objects
module SourceText =

    /// Creates an ISourceText object from the given string
    val ofString: string -> ISourceText

//
// NOTE: the code in this file is a drop-in replacement runtime for Lexing.fsi from the FsLexYacc repository
// and is referenced by generated code for the three FsLex generated lexers in the F# compiler.
// The underlying table format interpreted must precisely match the format generated by FsLex.
namespace Internal.Utilities.Text.Lexing

open System.Collections.Generic
open FSharp.Compiler.Text
open FSharp.Compiler.Features

/// Position information stored for lexing tokens
[<Struct>]
type internal Position =
    /// The file index for the file associated with the input stream, use <c>fileOfFileIndex</c> to decode
    val FileIndex: int

    /// The line number in the input stream, assuming fresh positions have been updated
    /// for the new line by modifying the EndPos property of the LexBuffer.
    val Line: int

    /// The line number for the position in the input stream, assuming fresh positions have been updated
    /// using for the new line.
    val OriginalLine: int

    /// The character number in the input stream.
    val AbsoluteOffset: int

    /// Return absolute offset of the start of the line marked by the position.
    val StartOfLineAbsoluteOffset: int

    /// Return the column number marked by the position,
    /// i.e. the difference between the <c>AbsoluteOffset</c> and the <c>StartOfLineAbsoluteOffset</c>
    member Column: int

    /// Given a position just beyond the end of a line, return a position at the start of the next line.
    member NextLine: Position

    /// Given a position at the start of a token of length n, return a position just beyond the end of the token.
    member EndOfToken: n: int -> Position

    /// Gives a position shifted by specified number of characters.
    member ShiftColumnBy: by: int -> Position

    /// Same line, column -1.
    member ColumnMinusOne: Position

    /// Apply a #line directive.
    member ApplyLineDirective: fileIdx: int * line: int -> Position

    /// Get an arbitrary position, with the empty string as file name.
    static member Empty: Position

    static member FirstLine: fileIdx: int -> Position

/// Input buffers consumed by lexers generated by <c>fslex.exe</c>.
/// The type must be generic to match the code generated by FsLex and FsYacc (if you would like to
/// fix this, please submit a PR to the FsLexYacc repository allowing for optional emit of a non-generic type reference).
[<Sealed>]
type internal LexBuffer<'Char> =
    /// The start position for the lexeme.
    member StartPos: Position with get, set

    /// The end position for the lexeme.
    member EndPos: Position with get, set

    /// The currently matched text as a Span, it is only valid until the lexer is advanced
    member LexemeView: System.ReadOnlySpan<'Char>

    /// Get single character of matched string
    member LexemeChar: int -> 'Char

    /// Determine if Lexeme contains a specific character
    member LexemeContains: 'Char -> bool

    /// Fast helper to turn the matched characters into a string, avoiding an intermediate array.
    static member LexemeString: LexBuffer<char> -> string

    /// Dynamically typed, non-lexically scoped parameter table.
    member BufferLocalStore: IDictionary<string, obj>

    /// True if the refill of the buffer ever failed , or if explicitly set to True.
    member IsPastEndOfStream: bool with get, set

    /// Determines if the parser can report FSharpCore library-only features.
    member ReportLibraryOnlyFeatures: bool

    /// Get the language version being supported
    member LanguageVersion: LanguageVersion

    /// True if the specified language feature is supported.
    member SupportsFeature: LanguageFeature -> bool

    /// Logs a recoverable error if a language feature is unsupported, at the specified range.
    member CheckLanguageFeatureErrorRecover: LanguageFeature -> range -> unit

    /// Create a lex buffer suitable for Unicode lexing that reads characters from the given array.
    /// Important: does take ownership of the array.
    static member FromChars: reportLibraryOnlyFeatures: bool * langVersion: LanguageVersion * char[] -> LexBuffer<char>

    /// Create a lex buffer that reads character or byte inputs by using the given function.
    static member FromFunction:
        reportLibraryOnlyFeatures: bool * langVersion: LanguageVersion * ('Char[] * int * int -> int) ->
            LexBuffer<'Char>

    /// Create a lex buffer backed by source text.
    static member FromSourceText:
        reportLibraryOnlyFeatures: bool * langVersion: LanguageVersion * ISourceText -> LexBuffer<char>

/// The type of tables for an unicode lexer generated by <c>fslex.exe</c>.
[<Sealed>]
type internal UnicodeTables =

    /// Create the tables from raw data
    static member Create: uint16[][] * uint16[] -> UnicodeTables

    /// Interpret tables for a unicode lexer generated by <c>fslex.exe</c>.
    member Interpret: initialState: int * LexBuffer<char> -> int