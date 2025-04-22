module CoopSync

open Newtonsoft.Json
open System.Threading.Tasks
open Z1R_Sync

let mutable lastSentPlayerProgressJson = ""
let mutable lastSentStartingItemsJson = ""
let mutable lastSentItemsJson = ""
let mutable lastSentOverworldJson = ""


let sendPlayerProgressUpdate (myConsoleId: string) =
    async {
        try
            let payload = SaveAndLoad.PlayerProgressAndTakeAnyHeartsModel.Create()
            let jsonPayload = JsonConvert.SerializeObject(payload)
            if jsonPayload <> lastSentPlayerProgressJson then
                lastSentPlayerProgressJson <- jsonPayload
                printfn "[Sync] Sending PlayerProgress update: %s" jsonPayload
                do! SyncManager.Send("PlayerProgress", jsonPayload, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send PlayerProgress update: %s" ex.Message
    }

let subscribeToPlayerProgressChanges(myConsoleId: string) =
    let sendUpdate() =
        async {
            do! sendPlayerProgressUpdate myConsoleId
        } |> Async.StartImmediate

    // Listen to the TakeAnyHeart array changes
    TrackerModel.playerProgressAndTakeAnyHearts.TakeAnyHeartChanged.Add(fun _ -> sendUpdate())

    // Listen to all BoolProperties
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBoomBook.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasWoodSword.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasWoodArrow.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBlueRing.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBlueCandle.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasMagicalSword.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasDefeatedGanon.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasRescuedZelda.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBombs.Changed.Add(fun _ -> sendUpdate())

let sendStartingItemsAndExtrasUpdate (myConsoleId: string) =
    async {
        try
            let payload = SaveAndLoad.StartingItemsAndExtrasModel.Create()
            let jsonPayload = JsonConvert.SerializeObject(payload)
            if jsonPayload <> lastSentStartingItemsJson then
                lastSentStartingItemsJson <- jsonPayload
                printfn "[Sync] Sending StartingItemsAndExtras update: %s" jsonPayload
                do! SyncManager.Send("StartingItems", jsonPayload, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send StartingItemsAndExtras update: %s" ex.Message
    }


let subscribeToStartingItemsAndExtrasChanges(myConsoleId: string) =
    let sendUpdate() =
        async {
            do! sendStartingItemsAndExtrasUpdate myConsoleId
        } |> Async.StartImmediate

    TrackerModel.startingItemsAndExtras.PlayerHasWhiteSword.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasMagicalSword.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasSilverArrow.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasBow.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasWand.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasRedCandle.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasBoomerang.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasMagicBoomerang.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasRedRing.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasPowerBracelet.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasLadder.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasRaft.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasRecorder.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasAnyKey.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.startingItemsAndExtras.PlayerHasBook.Changed.Add(fun _ -> sendUpdate())



let toSerializableBox (b: TrackerModel.Box) =
    let box = new SaveAndLoad.Box()
    box.CellCurrent <- b.CellCurrent()
    box.PlayerHas <- b.PlayerHas().AsInt()
    box






let sendItemsUpdate (myConsoleId: string) =
    async {
        try
            let model = new SaveAndLoad.Items()
            model.HiddenDungeonNumbers <- TrackerModel.IsHiddenDungeonNumbers()
            model.SecondQuestDungeons <- TrackerModel.IsSecondQuestDungeons
            model.WhiteSwordBox <- toSerializableBox TrackerModel.sword2Box
            model.LadderBox <- toSerializableBox TrackerModel.ladderBox
            model.ArmosBox <- toSerializableBox TrackerModel.armosBox

            model.Dungeons <- 
                [| for i in 0 .. 8 -> 
                    let d = new SaveAndLoad.Dungeon()
                    d.Boxes <- [| for j in TrackerModel.GetDungeon(i).Boxes -> toSerializableBox j |]
                    d |]

            let jsonPayload = JsonConvert.SerializeObject(model)
            if jsonPayload <> lastSentItemsJson then
                lastSentItemsJson <- jsonPayload
                printfn "[Sync] Sending Items update: %s" jsonPayload
                do! SyncManager.Send("Items", jsonPayload, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send Items update: %s" ex.Message
    }


let subscribeToItemsChanges(myConsoleId: string) =
    let sendUpdate () =
        async {
            do! sendItemsUpdate myConsoleId
        } |> Async.StartImmediate

    // Top-level boxes
    TrackerModel.sword2Box.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.ladderBox.Changed.Add(fun _ -> sendUpdate())
    TrackerModel.armosBox.Changed.Add(fun _ -> sendUpdate())

    // Dungeon boxes
    for i = 0 to 8 do
        let dungeon = TrackerModel.GetDungeon(i)
        for box in dungeon.Boxes do
            box.Changed.Add(fun _ -> sendUpdate())

let createSerializableOverworldModel () =
    let model = SaveAndLoad.Overworld()
    model.MirrorOverworld <- TrackerModel.MirrorOverworld
    model.StartIconX <- TrackerModel.startIconX
    model.StartIconY <- TrackerModel.startIconY
    model.CustomWaypointX <- TrackerModel.customWaypointX
    model.CustomWaypointY <- TrackerModel.customWaypointY

    let map = Array.zeroCreate (16 * 8 * 3)
    for j = 0 to 7 do
        for i = 0 to 15 do
            let index = (j * 16 + i) * 3
            let cur = TrackerModel.overworldMapMarks.[i,j].Current()
            let ed =
                if cur >= 0 && cur <= TrackerModel.MapSquareChoiceDomainHelper.DARK_X then
                    try TrackerModel.getOverworldMapExtraData(i,j,cur)
                    with _ -> 0
                else
                    0
            let circ = TrackerModel.overworldMapCircles.[i,j]
            map.[index] <- cur
            map.[index + 1] <- ed
            map.[index + 2] <- circ
    model.Map <- map

    model



let sendOverworldUpdate (myConsoleId: string) =
    async {
        try
            let model = new SaveAndLoad.Overworld()
            model.MirrorOverworld <- TrackerModel.MirrorOverworld
            model.StartIconX <- TrackerModel.startIconX
            model.StartIconY <- TrackerModel.startIconY
            model.CustomWaypointX <- TrackerModel.customWaypointX
            model.CustomWaypointY <- TrackerModel.customWaypointY

            let mapData = Array.zeroCreate<int> (16 * 8 * 3)

            for j = 0 to 7 do
                for i = 0 to 15 do
                    let idx = (j * 16 + i) * 3
                    let mark = TrackerModel.overworldMapMarks.[i,j].Current()
                    let extra = 
                        if mark <> -1 && mark <= TrackerModel.MapSquareChoiceDomainHelper.DARK_X then
                            try TrackerModel.getOverworldMapExtraData(i,j,mark)
                            with _ -> 0
                        else 0
                    let circ = TrackerModel.overworldMapCircles.[i,j]
                    mapData.[idx] <- mark
                    mapData.[idx + 1] <- extra
                    mapData.[idx + 2] <- circ

            model.Map <- mapData

            let json = JsonConvert.SerializeObject(model)
            printfn "[Sync] Sending Overworld update: %s" json
            do! SyncManager.Send("Overworld", json, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send Overworld update: %s" ex.Message
    }
let mutable debounceOverworldSend: System.Threading.CancellationTokenSource option = None

let sendOverworldUpdateDebounced (myConsoleId: string) =
    async {
        try
            let model = createSerializableOverworldModel()
            let jsonPayload = JsonConvert.SerializeObject(model)
            if jsonPayload <> lastSentOverworldJson then
                lastSentOverworldJson <- jsonPayload
                printfn "[Sync] Sending Overworld update: %s" jsonPayload
                do! SyncManager.Send("Overworld", jsonPayload, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send Overworld update: %s" ex.Message
    }



let subscribeToOverworldChanges (myConsoleId: string) =
    let mutable lastSentTime = System.DateTime.MinValue

    let rec loop () =
        async {
            do! Async.Sleep(500)  // check every 500ms
            let currentTime = TrackerModel.mapLastChangedTime.Time
            if currentTime > lastSentTime then
                lastSentTime <- currentTime
                do! sendOverworldUpdateDebounced myConsoleId
            return! loop ()
        }
    Async.StartImmediate(loop ())




