module JobsJobsJobs.Data.Json

open System
open System.Text.Json
open System.Text.Json.Serialization
open JobsJobsJobs.Domain

/// Convert citizen IDs to their string-GUID representation
type CitizenIdJsonConverter () =
    inherit JsonConverter<CitizenId> ()
    override this.Read(reader, _, _) =
        CitizenId (Guid.Parse (reader.GetString ())) 
    override this.Write(writer, value, _) =
        writer.WriteStringValue ((CitizenId.value value).ToString ())


/// JsonSerializer options that use the custom converters
let options =
    let opts = JsonSerializerOptions ()
    [   CitizenIdJsonConverter () :> JsonConverter
        JsonFSharpConverter    ()
    ]
    |> List.iter opts.Converters.Add
    opts
