module DungeonModelSync

open SaveAndLoad
open DungeonRoomState
open DungeonSaveAndLoad

// Helper: Compare two DungeonRoomModels
let shouldUpdateRoom (current: DungeonRoomModel) (incoming: DungeonRoomModel) =
    current.IsCompleted <> incoming.IsCompleted ||
    current.RoomType <> incoming.RoomType ||
    current.MonsterDetail <> incoming.MonsterDetail ||
    current.FloorDropDetail <> incoming.FloorDropDetail ||
    current.FloorDropShouldAppearBright <> incoming.FloorDropShouldAppearBright

// Helper: Apply properties from one DungeonRoomModel to another
let applyRoomModel (current: DungeonRoomModel) (incoming: DungeonRoomModel) =
    current.IsCompleted <- incoming.IsCompleted
    current.RoomType <- incoming.RoomType
    current.MonsterDetail <- incoming.MonsterDetail
    current.FloorDropDetail <- incoming.FloorDropDetail
    current.FloorDropShouldAppearBright <- incoming.FloorDropShouldAppearBright

// Main function to apply a single DungeonModel update
open DungeonSaveAndLoad
open DungeonRoomState

/// Applies a DungeonModel to the current dungeon map for the given level (1-indexed)
let applyDungeonModelToLevel (level:int) (dm:DungeonSaveAndLoad.DungeonModel) =
    if level < 1 || level > 9 then
        failwithf "Invalid level: %d. Level must be between 1 and 9." level
    else
        // Use the existing import function for this level
        DungeonUI.importFunctions.[level-1](dm)



// Full function to apply an entire array of DungeonModels
let applyAllDungeonModels (incomingModels: DungeonModel[]) =
    if TrackerModel.DungeonTrackerInstance.TheDungeonTrackerInstanceOption.IsSome then
        for level = 0 to 8 do
            let incoming = incomingModels.[level]
            let current = TrackerModel.DungeonTrackerInstance.TheDungeonTrackerInstance.Dungeons(level)
            applyDungeonModelToLevel level incoming
    else
        printfn "[Sync] Skipping dungeon sync: TrackerModel not yet initialized."


