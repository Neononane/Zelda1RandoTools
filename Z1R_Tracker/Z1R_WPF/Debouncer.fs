module Debouncer

type Debouncer(delayMs: int) =
    let mutable lastPayload = ""
    let mutable currentCts : System.Threading.CancellationTokenSource option = None

    member this.Trigger(payload: string, action: unit -> unit) =
        if payload = lastPayload then
            // Drop silently — same data already pending
            ()
        else
            lastPayload <- payload
            currentCts |> Option.iter (fun cts -> cts.Cancel())
            let newCts = new System.Threading.CancellationTokenSource()
            currentCts <- Some newCts

            async {
                do! Async.Sleep delayMs
                if not newCts.IsCancellationRequested then
                    action()
            } |> Async.Start
