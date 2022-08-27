﻿module JobsJobsJobs.Data.Json

open System.Text.Json
open System.Text.Json.Serialization
open JobsJobsJobs.Domain

/// Convert a wrapped GUID to/from its string representation
type WrappedJsonConverter<'T> (wrap : string -> 'T, unwrap : 'T -> string) =
    inherit JsonConverter<'T> ()
    override _.Read(reader, _, _) =
        wrap (reader.GetString ()) 
    override _.Write(writer, value, _) =
        writer.WriteStringValue (unwrap value)

/// JsonSerializer options that use the custom converters
let options =
    let opts = JsonSerializerOptions ()
    [   WrappedJsonConverter (CitizenId.ofString,   CitizenId.toString) :> JsonConverter
        WrappedJsonConverter (ContinentId.ofString, ContinentId.toString)
        WrappedJsonConverter (ListingId.ofString,   ListingId.toString)
        WrappedJsonConverter (Text,                 MarkdownString.toString)
        WrappedJsonConverter (SkillId.ofString,     SkillId.toString)
        WrappedJsonConverter (SuccessId.ofString,   SuccessId.toString)
        JsonFSharpConverter    ()
    ]
    |> List.iter opts.Converters.Add
    opts
