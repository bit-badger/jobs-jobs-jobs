module JobsJobsJobs.Data.Json

open System.Text.Json
open System.Text.Json.Serialization
open JobsJobsJobs.Domain

/// Convert a wrapped DU to/from its string representation
type WrappedJsonConverter<'T> (wrap : string -> 'T, unwrap : 'T -> string) =
    inherit JsonConverter<'T> ()
    override _.Read(reader, _, _) =
        wrap (reader.GetString ()) 
    override _.Write(writer, value, _) =
        writer.WriteStringValue (unwrap value)

open NodaTime
open NodaTime.Serialization.SystemTextJson

/// JsonSerializer options that use the custom converters
let options =
    let opts = JsonSerializerOptions ()
    [   WrappedJsonConverter (CitizenId.ofString,      CitizenId.toString) :> JsonConverter
        WrappedJsonConverter (ContactType.parse,       ContactType.toString)
        WrappedJsonConverter (ContinentId.ofString,    ContinentId.toString)
        WrappedJsonConverter (ListingId.ofString,      ListingId.toString)
        WrappedJsonConverter (Text,                    MarkdownString.toString)
        WrappedJsonConverter (OtherContactId.ofString, OtherContactId.toString)
        WrappedJsonConverter (SuccessId.ofString,      SuccessId.toString)
        JsonFSharpConverter    ()
    ]
    |> List.iter opts.Converters.Add
    let _ = opts.ConfigureForNodaTime DateTimeZoneProviders.Tzdb
    opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opts
