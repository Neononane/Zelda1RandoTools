module CoopSync

open Newtonsoft.Json
open System.Threading.Tasks
open Z1R_Sync
open System.Security.Cryptography
open System.Text
open System
open System.Collections.Concurrent

let mutable lastSentPlayerProgressJson = ""
let mutable lastSentStartingItemsJson = ""
let mutable lastSentItemsJson = ""
let mutable lastSentOverworldJson = ""
let mutable lastSentDungeonJson = ""
let mutable lastReceivedDungeonJson = ""
let mutable lastSentTriforceJson = ""
let mutable lastReceivedItemsHash: string option = None
let mutable lastReceivedHashes = System.Collections.Concurrent.ConcurrentDictionary<string, string>()
let mutable lastAppliedHashes = System.Collections.Concurrent.ConcurrentDictionary<string, string>()
let mutable lastSentHashes = ConcurrentDictionary<string, string>()
let mutable lastSentBlockersJson = ""


let computeHash (s: string) =
    using (SHA256.Create()) (fun sha ->
        s |> Encoding.UTF8.GetBytes
          |> sha.ComputeHash
          |> Convert.ToBase64String
    )

let alreadyReceived (messageType: string) (payloadJson: string) =
    let newHash = computeHash payloadJson
    match lastReceivedHashes.TryGetValue(messageType) with
    | true, oldHash when oldHash = newHash -> true
    | _ ->
        lastReceivedHashes.[messageType] <- newHash
        false

let shouldSend (messageType: string) (payloadJson: string) =
    let newHash = computeHash payloadJson
    match lastSentHashes.TryGetValue(messageType) with
    | true, oldHash when oldHash = newHash -> false  // same as last time, skip it
    | _ ->
        lastSentHashes.[messageType] <- newHash
        true

let shouldApplyUpdate (messageType: string) (payloadJson: string) =
    let newHash = computeHash payloadJson
    match lastAppliedHashes.TryGetValue(messageType) with
    | true, oldHash when oldHash = newHash -> false
    | _ ->
        lastAppliedHashes.[messageType] <- newHash
        true


[<AllowNullLiteral>]
type DungeonsTriforceState() =
    member val Triforces: bool[] = null with get, set


let sendPlayerProgressUpdate (myConsoleId: string) =
    async {
        try
            let payload = SaveAndLoad.PlayerProgressAndTakeAnyHeartsModel.Create()
            let jsonPayload = JsonConvert.SerializeObject(payload)
            if jsonPayload <> lastSentPlayerProgressJson then
                lastSentPlayerProgressJson <- jsonPayload
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending PlayerProgress update: %s" jsonPayload)
                if (TrackerModelOptions.CoopSyncOptions.GetEnableCoop()) then
                    do! SyncManager.Send("PlayerProgress", jsonPayload, myConsoleId) |> Async.AwaitTask
                else
                    TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Coop sync is disabled, not sending PlayerProgress update")
        with ex ->
            TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send PlayerProgress update: %s" ex.Message)
    }

let subscribeToPlayerProgressChanges(myConsoleId: string) =
    let lastChanged = TrackerModel.LastChangedTime()

    let markChanged () = lastChanged.SetNow()

    // Listen to the TakeAnyHeart array changes
    TrackerModel.playerProgressAndTakeAnyHearts.TakeAnyHeartChanged.Add(fun _ -> markChanged())

    // Listen to all BoolProperties
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBoomBook.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasWoodSword.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasWoodArrow.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBlueRing.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBlueCandle.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasMagicalSword.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasDefeatedGanon.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasRescuedZelda.Changed.Add(fun _ -> markChanged())
    TrackerModel.playerProgressAndTakeAnyHearts.PlayerHasBombs.Changed.Add(fun _ -> markChanged())

    let mutable lastSentTime = System.DateTime.MinValue

    let rec loop () =
        async {
            do! Async.Sleep(500)
            let currentTime = lastChanged.Time
            if currentTime > lastSentTime then
                lastSentTime <- currentTime
                do! sendPlayerProgressUpdate myConsoleId
            return! loop ()
        }

    Async.StartImmediate(loop ())

let sendStartingItemsAndExtrasUpdate (myConsoleId: string) =
    async {
        try
            let payload = SaveAndLoad.StartingItemsAndExtrasModel.Create()
            let jsonPayload = JsonConvert.SerializeObject(payload)
            if jsonPayload <> lastSentStartingItemsJson then
                lastSentStartingItemsJson <- jsonPayload
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending StartingItemsAndExtras update: %s" jsonPayload)
                if (TrackerModelOptions.CoopSyncOptions.GetEnableCoop()) then
                    do! SyncManager.Send("StartingItems", jsonPayload, myConsoleId) |> Async.AwaitTask
                else
                    TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Coop sync is disabled, not sending StartingItemsAndExtras update")
        with ex ->
            TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send StartingItemsAndExtras update: %s" ex.Message)
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

let sendBlockersUpdate (myConsoleId: string) =
    async {
        try
            let payload =
                Array.init 8 (fun i ->
                    Array.init TrackerModel.DungeonBlockersContainer.MAX_BLOCKERS_PER_DUNGEON (fun j ->
                        let jsonStr = TrackerModel.DungeonBlockersContainer.AsJsonString(i, j)
                        JsonConvert.DeserializeObject<SaveAndLoad.Blocker>(jsonStr)
                    )
                )
            let jsonPayload = JsonConvert.SerializeObject(payload)
            if shouldSend "Blockers" jsonPayload then
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending Blockers update: %s" jsonPayload)
                if TrackerModelOptions.CoopSyncOptions.GetEnableCoop() then
                    do! SyncManager.Send("Blockers", jsonPayload, myConsoleId) |> Async.AwaitTask
                else
                    TrackerModelOptions.DebugConfig.Log("[Sync] Coop sync is disabled, not sending Blockers update")
            else
                TrackerModelOptions.DebugConfig.Log("[Sync] Skipped sending Blockers update — no change")
        with ex ->
            TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send Blockers update: %s" ex.Message)
    }

let subscribeToBlockersChanges (myConsoleId: string) =
    TrackerModel.DungeonBlockersContainer.AnyBlockerChanged.Add(fun _ ->
        async {
            do! sendBlockersUpdate myConsoleId
        } |> Async.StartImmediate
    )



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
                    let trackerDungeon = TrackerModel.GetDungeon(i)
                    
                    d.Triforce <- false // EXCLUDE this value — syncing Triforce separately
                    d.Color <- trackerDungeon.Color
                    d.LabelChar <- trackerDungeon.LabelChar.ToString()
                    d.PlayerHasMap <- trackerDungeon.PlayerHasMapOfThisDungeon
                    d.Boxes <- 
                        [| for box in trackerDungeon.Boxes -> toSerializableBox box |]
                    d |]

            let jsonPayload = JsonConvert.SerializeObject(model)

            if shouldSend "Items" jsonPayload then
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending Items update (excluding Triforce): %s" jsonPayload)
                if (TrackerModelOptions.CoopSyncOptions.GetEnableCoop()) then
                    do! SyncManager.Send("Items", jsonPayload, myConsoleId) |> Async.AwaitTask
                else
                    TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Coop sync is disabled, not sending Items update")
            else
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Skipped sending Items update — no change")
        with ex ->
            TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send Items update: %s" ex.Message)
    }

let subscribeToItemsChanges(myConsoleId: string) =
    // Shared timestamp that all item-related changes update
    let lastChanged = TrackerModel.LastChangedTime()

    // Helper to mark the model as changed
    let markChanged () = lastChanged.SetNow()

    // Top-level boxes
    TrackerModel.sword2Box.Changed.Add(fun _ -> markChanged())
    TrackerModel.ladderBox.Changed.Add(fun _ -> markChanged())
    TrackerModel.armosBox.Changed.Add(fun _ -> markChanged())

    // Dungeon boxes
    for i = 0 to 8 do
        let dungeon = TrackerModel.GetDungeon(i)
        for box in dungeon.Boxes do
            box.Changed.Add(fun _ -> markChanged())

    // Polling loop to debounce updates
    let mutable lastSentTime = System.DateTime.MinValue

    let rec loop () =
        async {
            do! Async.Sleep(500)
            let currentTime = lastChanged.Time
            if currentTime > lastSentTime then
                lastSentTime <- currentTime
                do! sendItemsUpdate myConsoleId
            return! loop ()
        }

    Async.StartImmediate(loop ())

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



//let sendOverworldUpdate (myConsoleId: string) =
//    async {
//        try
//            let model = new SaveAndLoad.Overworld()
//            model.MirrorOverworld <- TrackerModel.MirrorOverworld
//            model.StartIconX <- TrackerModel.startIconX
//            model.StartIconY <- TrackerModel.startIconY
//            model.CustomWaypointX <- TrackerModel.customWaypointX
//            model.CustomWaypointY <- TrackerModel.customWaypointY

//            let mapData = Array.zeroCreate<int> (16 * 8 * 3)

//            for j = 0 to 7 do
//                for i = 0 to 15 do
//                    let idx = (j * 16 + i) * 3
//                    let mark = TrackerModel.overworldMapMarks.[i,j].Current()
//                    let extra = 
//                        if mark <> -1 && mark <= TrackerModel.MapSquareChoiceDomainHelper.DARK_X then
//                            try TrackerModel.getOverworldMapExtraData(i,j,mark)
//                            with _ -> 0
//                        else 0
//                    let circ = TrackerModel.overworldMapCircles.[i,j]
//                    mapData.[idx] <- mark
//                    mapData.[idx + 1] <- extra
//                    mapData.[idx + 2] <- circ

//            model.Map <- mapData

//            let json = JsonConvert.SerializeObject(model)
//            printfn "[Sync] Sending Overworld update: %s" json
//            do! SyncManager.Send("Overworld", json, myConsoleId) |> Async.AwaitTask
//        with ex ->
//            printfn "[Sync] Failed to send Overworld update: %s" ex.Message
//    }
let mutable debounceOverworldSend: System.Threading.CancellationTokenSource option = None
let overworldDebouncer = Debouncer.Debouncer(200)

let sendOverworldUpdateDebounced (myConsoleId: string) =
    overworldDebouncer.Trigger(fun() ->
        async {
            try
                let model = createSerializableOverworldModel()
                let jsonPayload = JsonConvert.SerializeObject(model)
                if jsonPayload <> lastSentOverworldJson then
                    lastSentOverworldJson <- jsonPayload
                    TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending Overworld update: %s" jsonPayload)
                    if (TrackerModelOptions.CoopSyncOptions.GetEnableCoop()) then
                        do! SyncManager.Send("Overworld", jsonPayload, myConsoleId) |> Async.AwaitTask
                    else
                        TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Coop sync is disabled, not sending Overworld update")
            with ex ->
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send Overworld update: %s" ex.Message)
        }
        |> Async.Start
    )



let subscribeToOverworldChanges (myConsoleId: string) =
    let mutable lastSentTime = System.DateTime.MinValue

    let rec loop () =
        async {
            do! Async.Sleep(500)  // check every 500ms
            let currentTime = TrackerModel.mapLastChangedTime.Time
            if currentTime > lastSentTime then
                lastSentTime <- currentTime
                sendOverworldUpdateDebounced myConsoleId
            return! loop ()
        }
    Async.StartImmediate(loop ())

(*let createSerializableDungeonArray () =
    [| for i in 0 .. 8 ->
        let d = TrackerModel.GetDungeon(i)
        let model = SaveAndLoad.Dungeon()
        model.Triforce <- d.PlayerHasTriforce()
        model.Color <- d.Color
        model.LabelChar <- string d.LabelChar
        model.PlayerHasMap <- d.PlayerHasMapOfThisDungeon
        //model.Boxes <- d.Boxes |> Array.map (fun b -> 
            //let box = SaveAndLoad.Box()
            //box.CellCurrent <- b.CellCurrent()
            //box.PlayerHas <- b.PlayerHas().AsInt()
            //box)
        model
    |]

let sendDungeonUpdate (myConsoleId: string) =
    async {
        try
            let modelArray = createSerializableDungeonArray()
            let json = JsonConvert.SerializeObject(modelArray)
            if json = lastSentDungeonJson then
                printfn "[Sync] Skipped sending Dungeon update — no change"
            else
                lastSentDungeonJson <- json
                printfn "[Sync] Sending Dungeon update: %s" json
                do! SyncManager.Send("Dungeon", json, myConsoleId) |> Async.AwaitTask
        with ex ->
            printfn "[Sync] Failed to send Dungeon update: %s" ex.Message
    }

let subscribeToDungeonChanges (myConsoleId: string) =
    let sendUpdate () =
        async {
            do! sendDungeonUpdate myConsoleId
        } |> Async.StartImmediate

    for i = 0 to 8 do
        let dungeon = TrackerModel.GetDungeon(i)
        for box in dungeon.Boxes do
            box.Changed.Add(fun _ -> sendUpdate())*)


let sendDungeonTriforceUpdate (myConsoleId: string) =
    async {
        try
            let model = DungeonsTriforceState()
            model.Triforces <- [| for i in 0 .. 8 -> TrackerModel.GetDungeon(i).PlayerHasTriforce() |]
            let json = JsonConvert.SerializeObject(model)
            if shouldSend "DungeonTriforce" json then
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Sending DungeonTriforce update: %s" json)
                if (TrackerModelOptions.CoopSyncOptions.GetEnableCoop()) then
                    do! SyncManager.Send("DungeonTriforce", json, myConsoleId) |> Async.AwaitTask
                else
                    TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Coop sync is disabled, not sending DungeonTriforce update")
            else
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Skipped sending DungeonTriforce update — no change")
        with ex ->
            printf "[Sync] Failed to send DungeonTriforce update: %s" ex.Message
    }

let subscribeToDungeonTriforceChanges (myConsoleId: string) =
    let mutable lastSent: bool[] = Array.init 9 (fun _ -> false)
    let rec loop () =
        async {
            do! Async.Sleep(750) // poll every ¾ second
            let current = [| for i in 0 .. 8 -> TrackerModel.GetDungeon(i).PlayerHasTriforce() |]
            if current <> lastSent then
                lastSent <- current
                do! sendDungeonTriforceUpdate myConsoleId
            return! loop ()
        }
    Async.StartImmediate(loop())

let transposeHorizontalDoors (input: int[][]) : int[][] =
    if isNull input then
        [||]
    else
        [| for i in 0 .. 7 -> [| for j in 0 .. 6 -> input.[j].[i] |] |]

let transposeVerticalDoors (input: int[][]) : int[][] =
    if isNull input then
        [||]
    else
        [| for i in 0 .. 6 -> [| for j in 0 .. 7 -> input.[j].[i] |] |]

let transposeRoomStates (input: DungeonSaveAndLoad.DungeonRoomModel[][]) : DungeonSaveAndLoad.DungeonRoomModel[][] =
    if isNull input then
        [||]
    else
        [| for i in 0 .. 7 -> [| for j in 0 .. 7 -> input.[j].[i] |] |]

let cloneAndFixDungeonModel (dm: DungeonSaveAndLoad.DungeonModel) : DungeonSaveAndLoad.DungeonModel =
    let newDM = new DungeonSaveAndLoad.DungeonModel()
    newDM.HorizontalDoors <- transposeHorizontalDoors dm.HorizontalDoors
    newDM.VerticalDoors <- transposeVerticalDoors dm.VerticalDoors
    newDM.RoomIsCircled <- dm.RoomIsCircled
    newDM.RoomStates <- transposeRoomStates dm.RoomStates
    newDM.VanillaMapOverlay <- dm.VanillaMapOverlay
    newDM

// Create a mutable to debounce updates
let mutable lastDungeonMapsPayload = ""
let dungeonMapsDebouncer = Debouncer.Debouncer(200)

let sendDungeonMapsUpdate (myConsoleId: string) =
    dungeonMapsDebouncer.Trigger(fun() ->
        async {
            if TrackerModel.DungeonTrackerInstance.TheDungeonTrackerInstanceOption.IsNone then
                TrackerModelOptions.DebugConfig.Log("[Sync] Skipping DungeonMaps update: TrackerModel not yet initialized.")
                return ()
            try
                let dungeonModels = 
                    [|
                        for i = 0 to 8 do
                            let exportFunction = DungeonUI.exportFunctionsLarge.[i]
                            let dm = exportFunction()
                            yield cloneAndFixDungeonModel dm
                    |]
            
                let jsonPayload = JsonConvert.SerializeObject(dungeonModels, Formatting.None)
                if jsonPayload <> lastDungeonMapsPayload then
                    lastDungeonMapsPayload <- jsonPayload
                    if shouldSend "DungeonMaps" jsonPayload then
                        TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] DungeonMaps update: %s" jsonPayload)
                        if TrackerModelOptions.CoopSyncOptions.GetEnableCoop() then
                            do! SyncManager.Send("DungeonMaps", jsonPayload, myConsoleId) |> Async.AwaitTask
                        else
                            TrackerModelOptions.DebugConfig.Log("[Sync] Coop sync is disabled, not sending DungeonMaps update")
                    else
                        TrackerModelOptions.DebugConfig.Log("[Sync] Skipped sending DungeonMaps update — no change")
                else
                    TrackerModelOptions.DebugConfig.Log("[Sync] Skipped sending DungeonMaps update — no change")
            with ex ->
                TrackerModelOptions.DebugConfig.Log(sprintf "[Sync] Failed to send DungeonMaps update: %s" ex.Message)
        }
        |> Async.Start
    )

let subscribeToDungeonMapsChanges (myConsoleId: string) =
    let mutable lastSent = System.DateTime.MinValue
    let rec loop () =
        async {
            do! Async.Sleep(500)
            if TrackerModel.dungeonRoomModelChanged.Time > lastSent then
                lastSent <- System.DateTime.Now
                sendDungeonMapsUpdate myConsoleId
            return! loop ()
        }
    Async.StartImmediate(loop())
