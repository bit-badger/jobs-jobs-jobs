module JobsJobsJobs.Data.Json

open System
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

/// Convert a wrapped GUID to/from its string representation
type WrappedIdJsonConverter<'T> (wrap : Guid -> 'T, unwrap : 'T -> Guid) =
    inherit JsonConverter<'T> ()
    override _.Read(reader, _, _) =
        wrap (Guid.Parse (reader.GetString ())) 
    override _.Write(writer, value, _) =
        writer.WriteStringValue ((unwrap value).ToString ())


/// JsonSerializer options that use the custom converters
let options =
    let opts = JsonSerializerOptions ()
    [   WrappedIdJsonConverter (CitizenId,   CitizenId.value) :> JsonConverter
        WrappedIdJsonConverter (ContinentId, ContinentId.value)
        WrappedIdJsonConverter (ListingId,   ListingId.value)
        WrappedJsonConverter   (Text,        MarkdownString.toString)
        WrappedIdJsonConverter (SkillId,     SkillId.value)
        WrappedIdJsonConverter (SuccessId,   SuccessId.value)
        JsonFSharpConverter    ()
    ]
    |> List.iter opts.Converters.Add
    opts
