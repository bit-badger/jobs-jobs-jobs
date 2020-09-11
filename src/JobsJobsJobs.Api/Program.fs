module JobsJobsJobs.Api.App

// Learn more about F# at http://fsharp.org

open System
open Suave

[<EntryPoint>]
let main argv =
  { defaultConfig with
      bindings         = [ HttpBinding.createSimple HTTP "127.0.0.1" 3002; HttpBinding.createSimple HTTP "::1" 3002 ]
      // errorHandler     = Handlers.Error.error
      // serverKey        = config.serverKey
      // cookieSerialiser = FSharpJsonCookieSerialiser()
      // homeFolder   = Some "./wwwroot/"
    }
  |> (flip startWebServer) Handlers.webApp
  0
