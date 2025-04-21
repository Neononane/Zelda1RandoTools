module Sync

open System
open System.Net.Http
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR.Client
open Newtonsoft.Json
open System.Windows

// 1) Define your types
type Payload = { roomId: string; markType: string }
type MapUpdateEvent = {
  version:      int
  eventType:    string
  timestampUtc: DateTime
  payload:      Payload
}
type SignalRConnectionInfo = {
  [<JsonProperty("url")>]         Url: string
  [<JsonProperty("accessToken")>] AccessToken: string
}

let private httpClient = new HttpClient()

// 2) Send a single update
let sendUpdate (url:string) (evt:MapUpdateEvent) : Async<unit> =
  async {
    let json    = JsonConvert.SerializeObject(evt)
    use content = new StringContent(json, Encoding.UTF8, "application/json")
    let! resp   = httpClient.PostAsync(url, content) |> Async.AwaitTask
    resp.EnsureSuccessStatusCode() |> ignore
  }

// 3) Listen for remote updates
let startListenerAsync
    (negotiateUrl:string)
    (onUpdate: MapUpdateEvent -> unit)
    : Async<unit> =
  async {
    // 3a) negotiate
    let! r1   = httpClient.PostAsync(negotiateUrl, null) |> Async.AwaitTask
    r1.EnsureSuccessStatusCode() |> ignore
    let! body = r1.Content.ReadAsStringAsync() |> Async.AwaitTask
    let conn  = JsonConvert.DeserializeObject<SignalRConnectionInfo>(body)

    // 3b) build hub
    let hub =
      HubConnectionBuilder()
        .WithUrl(conn.Url, fun opts ->
          opts.AccessTokenProvider <- Func<Task<string>>(fun () -> Task.FromResult conn.AccessToken))
        .Build()

    // 3c) subscribe
    hub.On<string>("ReceiveMapUpdate", fun json ->
      let evt = JsonConvert.DeserializeObject<MapUpdateEvent>(json)
      // marshal to UI thread
      Application.Current.Dispatcher.Invoke(fun () -> onUpdate evt)
    ) |> ignore

    // 3d) start
    do! hub.StartAsync() |> Async.AwaitTask
  }
