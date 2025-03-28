﻿module OptionsMenu

open System.Windows.Controls
open System.Windows.Media
open System.Windows

open CustomComboBoxes.GlobalFlag

let voice = new System.Speech.Synthesis.SpeechSynthesizer()
let defaultVoice = try voice.Voice.Name with _ -> ""
let InitializeVoice() =
    try 
        voice.Volume <- TrackerModelOptions.Volume
        voice.SelectVoice(TrackerModelOptions.PreferredVoice)
    with _ -> ()

let mutable microphoneFailedToInitialize = false
let mutable gamepadFailedToInitialize = false

let broadcastWindowOptionChanged = new Event<unit>()
let mouseMagnifierWindowOptionChanged = new Event<unit>()
let BOARDInsteadOfLEVELOptionChanged = new Event<unit>()
let secondQuestDungeonsOptionChanged = new Event<unit>()
let showBasementInfoOptionChanged = new Event<unit>()
let bookForHelpfulHintsOptionChanged = new Event<unit>()
let requestRedrawOverworldEvent = new Event<unit>()
let hideTimerChanged = new Event<unit>()
let dungeonSunglassesChanged = new Event<unit>()

let link(cb:CheckBox, b:TrackerModelOptions.Bool, needFU, otherEffect) =
    let effect() = 
        if needFU then 
            TrackerModel.forceUpdate()
        otherEffect()
    cb.IsChecked <- System.Nullable.op_Implicit b.Value
    cb.Checked.Add(fun _ -> b.Value <- true; effect())
    cb.Unchecked.Add(fun _ -> b.Value <- false; effect())

let data1o(isStandardHyrule) = 
    [|
    // These features are disabled by the app when not(isStandardHyrule)
    if isStandardHyrule then yield "Draw routes", "Constantly display routing lines when mousing over overworld tiles", TrackerModelOptions.Overworld.DrawRoutes, true, (fun()->()), None
    if isStandardHyrule then yield "Show screen scrolls", "Routing lines assume the player can screen scroll\nScreen scrolls appear as curved lines", TrackerModelOptions.Overworld.RoutesCanScreenScroll, true, (fun()->()), Some(Thickness(20.,0.,0.,0.))
    if isStandardHyrule then yield "Highlight nearby", "Highlight nearest unmarked gettable overworld tiles when mousing", TrackerModelOptions.Overworld.HighlightNearby, false, (fun()->()), None
    if isStandardHyrule then yield "Show magnifier", "Display magnified view of overworld tiles when mousing", TrackerModelOptions.Overworld.ShowMagnifier, false, (fun()->()), None
    // Mirror overworld is not useful when not(isStandardHyrule), but if the user has it checked, we want them to be able to uncheck it
// now a clickable feature of top tracker area
//    yield "Mirror overworld", "Flip the overworld map East<->West", TrackerModelOptions.Overworld.MirrorOverworld, true, (fun()->()), None
    yield "Shops before dungeons", "In the overworld map tile popup, the grid starts with shops when this is checked\n(starts with dungeons when unchecked)", TrackerModelOptions.Overworld.ShopsFirst, false, (fun()->()), None
    |]

let data1d = [|
    "BOARD instead of LEVEL", "Check this to change the dungeon column labels to BOARD-N instead of LEVEL-N", TrackerModelOptions.BOARDInsteadOfLEVEL, false, BOARDInsteadOfLEVELOptionChanged.Trigger
// now a clickable feature of top tracker area
//    "Second quest dungeons", "Check this if dungeon 4, rather than dungeon 1, has 3 items (no effect when Hidden Dungeon Numbers)", TrackerModelOptions.IsSecondQuestDungeons, false, secondQuestDungeonsOptionChanged.Trigger
    "Show basement info", "Check this if empty dungeon item boxes should suggest whether they are found as\nbasement items rather than floor drops (no effect when Hidden Dungeon Numbers)", TrackerModelOptions.ShowBasementInfo, false, showBasementInfoOptionChanged.Trigger
    "Do door inference", "Check this to mark a green door when you mark a new room, if the point of entry can be inferred", TrackerModelOptions.DoDoorInference, false, fun()->()
    "Book for Helpful Hints", "Check this if both 'Book To Understand Old Men' flag is on, and\n'Helpful' hints are available. The tracker will let you left-click\nOld Man Hint rooms to toggle whether you have read them yet.", TrackerModelOptions.BookForHelpfulHints, false, bookForHelpfulHintsOptionChanged.Trigger
    "Left-drag auto-inverts", "Painting maps: When checked, If your first drag is with left-click,\nand you've not yet inverted OffTheMap with Unmarked, then\nauto-invert when left-click-dragging, to immediately start painting a map.", TrackerModelOptions.LeftClickDragAutoInverts, false, (fun()->())
    "Default to NonDescript", "Room default: When checked, clicking an Unmarked room will mark it as\nNonDescript (empty box) rather than MaybePushBlock (box with two dots)", TrackerModelOptions.DefaultRoomPreferNonDescriptToMaybePushBlock, false, (fun()->())
    "Dungeon 'sunglasses'", "The dungeon tracker has high contrast (bright colors against black);\nTurn this on to darken the colors somewhat to reduce bright contrast", TrackerModelOptions.GiveDungeonTrackerSunglasses, false, dungeonSunglassesChanged.Trigger
    |]

let data2 = [|
    TrackerModel.ReminderCategory.DungeonFeedback.DisplayName, "Note when dungeons are located/completed, triforces obtained, and go-time", 
        TrackerModelOptions.VoiceReminders.DungeonFeedback, TrackerModelOptions.VisualReminders.DungeonFeedback
    TrackerModel.ReminderCategory.SwordHearts.DisplayName, "Remind to consider white/magical sword when you get 4-6 or 10-14 hearts", 
        TrackerModelOptions.VoiceReminders.SwordHearts,     TrackerModelOptions.VisualReminders.SwordHearts
    TrackerModel.ReminderCategory.CoastItem.DisplayName, "Reminder to fetch to coast item when you have the ladder", 
        TrackerModelOptions.VoiceReminders.CoastItem,       TrackerModelOptions.VisualReminders.CoastItem
    TrackerModel.ReminderCategory.RecorderPBSpotsAndBoomstickBook.DisplayName, "Periodic reminders of how many recorder/power-bracelet spots remain, or that the boomstick is available", 
        TrackerModelOptions.VoiceReminders.RecorderPBSpotsAndBoomstickBook, TrackerModelOptions.VisualReminders.RecorderPBSpotsAndBoomstickBook
    TrackerModel.ReminderCategory.HaveKeyLadder.DisplayName, "One-time reminder, a little while after obtaining these items, that you have them", 
        TrackerModelOptions.VoiceReminders.HaveKeyLadder,   TrackerModelOptions.VisualReminders.HaveKeyLadder
    TrackerModel.ReminderCategory.Blockers.DisplayName, "Reminder when you may have become unblocked on a previously-aborted dungeon", 
        TrackerModelOptions.VoiceReminders.Blockers,        TrackerModelOptions.VisualReminders.Blockers
    TrackerModel.ReminderCategory.DoorRepair.DisplayName, "Each time you uncover a door repair charge, remind the count of how many you have found", 
        TrackerModelOptions.VoiceReminders.DoorRepair,        TrackerModelOptions.VisualReminders.DoorRepair
    TrackerModel.ReminderCategory.OverworldOverwrites.DisplayName, "Each time you make a destructive change to an overworld mark, remind the change, in case it was accidental", 
        TrackerModelOptions.VoiceReminders.OverworldOverwrites, TrackerModelOptions.VisualReminders.OverworldOverwrites
    |]

let makeOptionsCanvas(cm:CustomComboBoxes.CanvasManager, includePopupExplainer, isStandardHyrule) = 
    let width = cm.AppMainCanvas.Width
    let all = new Border(BorderThickness=Thickness(2.), BorderBrush=Brushes.DarkGray, Background=Brushes.Black)
    let optionsAllsp = new StackPanel(Orientation=Orientation.Horizontal, Width=width, Background=Brushes.Black)
    let AddStyle(e:FrameworkElement) = 
        let style = new Style(typeof<TextBox>)
        style.Setters.Add(new Setter(TextBox.BorderThicknessProperty, Thickness(0.)))
        style.Setters.Add(new Setter(TextBox.BorderBrushProperty, Brushes.DarkGray))
        style.Setters.Add(new Setter(TextBox.FontSizeProperty, 16.))
        style.Setters.Add(new Setter(TextBox.ForegroundProperty, Brushes.Orange))
        style.Setters.Add(new Setter(TextBox.BackgroundProperty, Brushes.Black))
        e.Resources.Add(typeof<TextBox>, style)
        let style = new Style(typeof<CheckBox>)
        style.Setters.Add(new Setter(CheckBox.HeightProperty, 22.))
        e.Resources.Add(typeof<CheckBox>, style)
    AddStyle(all)

    let header(tb:TextBox) = 
        tb.Margin <- Thickness(0., 0., 0., 6.)
        tb.BorderThickness <- Thickness(0., 0., 0., 1.)
        tb
    let options1sp = new StackPanel(Orientation=Orientation.Vertical, Margin=Thickness(10.,0.,10.,0.))
    let tb = new TextBox(Text="Overworld settings", IsReadOnly=true, FontWeight=FontWeights.Bold) |> header
    options1sp.Children.Add(tb) |> ignore
    for text,tip,b,needFU,oe,marginOpt in data1o(isStandardHyrule) do
        let cb = new CheckBox(Content=new TextBox(Text=text,IsReadOnly=true))
        if marginOpt.IsSome then
            cb.Margin <- marginOpt.Value
        cb.ToolTip <- tip
        ToolTipService.SetShowDuration(cb, 10000)
        link(cb, b, needFU, oe)
        options1sp.Children.Add(cb) |> ignore
    let moreButton = Graphics.makeButton(" More settings... ",None,None)
    moreButton.HorizontalAlignment <- HorizontalAlignment.Left
    options1sp.Children.Add(moreButton) |> ignore
    do  // scope local mutable popupIsActive variable to this one button
        let mutable popupIsActive = false  // second level of popup, need local copy
        let mutable changedGlobal = false
        moreButton.Click.Add(fun _ ->
            if not popupIsActive then
                popupIsActive <- true
                // the options menu is not popped up on the start screen, but is in the main app, so we need to make this consistent for both
                if not CustomComboBoxes.GlobalFlag.popupIsActive then
                    CustomComboBoxes.GlobalFlag.popupIsActive <- true  // must be start screen
                    changedGlobal <- true
                let wh = new System.Threading.ManualResetEvent(false)
                let sp = new StackPanel(Orientation=Orientation.Vertical)
                let tb = new TextBox(Text="Overworld marks to hide", IsReadOnly=true, FontWeight=FontWeights.Bold) |> header
                sp.Children.Add(tb) |> ignore
                let desc = "Sometimes you want to mark certain map tiles (e.g. Door Repairs) so the tracker can help you (e.g. by keeping count), but " +
                            "you don't want to clutter your overworld map with icons (e.g. Door icons) that you don't need to see or come back to.  " +
                            "In these cases, you can opt to 'hide' certain icons, so that they appear like \"Don't Care\" spots (just grayed out tile " +
                            "with no icon) rather than with an icon on the map.\n\n" + 
                            "Check each tile that you would prefer to hide after you mark it."
                let tb = new TextBox(Text=desc, IsReadOnly=true, TextWrapping=TextWrapping.Wrap)
                sp.Children.Add(tb) |> ignore
                let firstThird = TrackerModel.MapSquareChoiceDomainHelper.TilesThatSupportHidingOverworldMarks.[0..3]
                let secondThird = TrackerModel.MapSquareChoiceDomainHelper.TilesThatSupportHidingOverworldMarks.[4..7]
                let finalThird = TrackerModel.MapSquareChoiceDomainHelper.TilesThatSupportHidingOverworldMarks.[8..11]
                let boxes = new StackPanel(Orientation=Orientation.Horizontal, Margin=Thickness(20.,5.,0.,5.))
                let first = new StackPanel(Orientation=Orientation.Vertical)
                let second = new StackPanel(Orientation=Orientation.Vertical)
                let third = new StackPanel(Orientation=Orientation.Vertical)
                boxes.Children.Add(first) |> ignore
                boxes.Children.Add(second) |> ignore
                boxes.Children.Add(third) |> ignore
                sp.Children.Add(boxes) |> ignore
                let addTo(sp:StackPanel, a) = 
                    for tile in a do
                        let desc = let _,_,_,s = TrackerModel.dummyOverworldTiles.[tile] in s
                        let desc = 
                            let i = desc.IndexOf('\n')
                            if i <> -1 then
                                desc.Substring(0, i)
                            else
                            desc
                        let cb = new CheckBox(Content=new TextBox(Text=desc,IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
                        let b = TrackerModel.MapSquareChoiceDomainHelper.AsTrackerModelOptionsOverworldTilesToHide(tile)
                        link(cb, b, false, requestRedrawOverworldEvent.Trigger)
                        sp.Children.Add(cb) |> ignore
                addTo(first, firstThird)
                addTo(second, secondThird)
                addTo(third, finalThird)
                let desc = "Note that even when hidden, certain tiles can be toggled 'bright' by left-clicking them.  For example, a Hint Shop where " +
                            "you have not yet bought out all the hints, but intend to return later, could be left-clicked to toggle it from dark to " +
                            "bright.  This behavior is retained even if you choose to hide the tile: left-clicking toggles between a hidden icon and " +
                            "a bright icon in that case.\n\n" +
                            "You can also hide no-longer-relevant shop items (key shop after you get the Magic Key, ring shop after you obtain either blue or red ring, " +
                            "candle shop after you obtain blue or red candle, arrow shop after you buy it or obtain silvers, boomstick book after you buy it) " +
                            "by checking the box below."
                let tb = new TextBox(Text=desc, IsReadOnly=true, TextWrapping=TextWrapping.Wrap, Margin=Thickness(0.,0.,0.,5.))
                sp.Children.Add(tb) |> ignore
                let cb = new CheckBox(Content=new TextBox(Text="Hide no-longer-relevant shop items",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
                let b = TrackerModel.MapSquareChoiceDomainHelper.AsTrackerModelOptionsOverworldTilesToHide(TrackerModel.MapSquareChoiceDomainHelper.SHOP)
                link(cb, b, false, requestRedrawOverworldEvent.Trigger)
                sp.Children.Add(cb) |> ignore
                let desc = "Note that bomb shops and shield shops always stay visible.\n\nMeat shops also are always visible, unless you also click the box below, to make " +
                            "them always be invisible.  If you check this box, then to see the meat shops, you must either (1) uncheck this box, (2) mouse hover Zelda (see below), " +
                            "(3) mark a meat Blocker and then mouse-hover the Blocker, or (4) place a HungryGoriya room in a dungeon and hover that room " +
                            "(options (3) or (4) are best).\nThis option is for folks who like to mark meat shops but also know that " +
                            "in 99% of seed, they'll be unnecessary and just clutter the map.  In the 1% of seeds that have a meat block, mark the Blocker and then mouse hover it to find the shop."
                let tb = new TextBox(Text=desc, IsReadOnly=true, TextWrapping=TextWrapping.Wrap)
                sp.Children.Add(tb) |> ignore
                let cb = new CheckBox(Content=new TextBox(Text="When hiding no-longer-relevant shop items, always hide meat shops",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
                let b = TrackerModelOptions.OverworldTilesToHide.AlwaysHideMeatShops
                link(cb, b, false, requestRedrawOverworldEvent.Trigger)
                sp.Children.Add(cb) |> ignore
                let tb = new TextBox(Text="", IsReadOnly=true, TextWrapping=TextWrapping.Wrap) |> header
                sp.Children.Add(tb) |> ignore
                let desc = "\nYou can mouse-hover the Zelda icon in the top of the tracker to temporarily make all hidden icons re-appear, if desired. And when you finish the seed and click Zelda, all icons reappear."
                let tb = new TextBox(Text=desc, IsReadOnly=true, TextWrapping=TextWrapping.Wrap)
                sp.Children.Add(tb) |> ignore
                AddStyle(sp)
                let sv = new ScrollViewer(Content=sp, VerticalScrollBarVisibility=ScrollBarVisibility.Auto, HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled)
                let b = new Border(Child=sv, BorderThickness=Thickness(2.), BorderBrush=Brushes.DarkGray, Background=Brushes.Black, Padding=Thickness(5.), 
                                    Width=720., MaxHeight=cm.Height-90., HorizontalAlignment=HorizontalAlignment.Right)
                async {
                    do! CustomComboBoxes.DoModalDocked(cm, wh, Dock.Bottom, b)
                    popupIsActive <- false
                    if changedGlobal then
                        CustomComboBoxes.GlobalFlag.popupIsActive <- false
                } |> Async.StartImmediate
            )

    let tb = new TextBox(Text="Dungeon settings", IsReadOnly=true, FontWeight=FontWeights.Bold) |> header
    options1sp.Children.Add(tb) |> ignore
    for text,tip,b,needFU,oe in data1d do
        let cb = new CheckBox(Content=new TextBox(Text=text,IsReadOnly=true))
        cb.ToolTip <- tip
        ToolTipService.SetShowDuration(cb, 10000)
        link(cb, b, needFU, oe)
        options1sp.Children.Add(cb) |> ignore
    optionsAllsp.Children.Add(options1sp) |> ignore

    //This section starts the reminders panel in the initial startup screen
    //The stack panel here is the middle column of content. Everything needs to be added to options2sp in order to render in this section. it is generally added top to bottom
    let options2sp = new StackPanel(Orientation=Orientation.Vertical, Margin=Thickness(10.,2.,10.,0.))
    let tb = new TextBox(Text="Reminders", IsReadOnly=true, FontWeight=FontWeights.Bold) |> header
    //This is an example of how to add a text box to the middle column of content
    options2sp.Children.Add(tb) |> ignore
    // volume and slider
    // This is a new StackPenel uniquely for the volume slider
    let options2Topsp = new StackPanel(Orientation=Orientation.Horizontal, Margin=Thickness(0.,0.,0.,6.))
    let volumeText = new TextBox(Text="Volume",IsReadOnly=true, Margin=Thickness(0.))
    options2Topsp.Children.Add(volumeText) |> ignore
    let slider = new Slider(Orientation=Orientation.Horizontal, Maximum=100., TickFrequency=10., TickPlacement=Primitives.TickPlacement.Both, IsSnapToTickEnabled=true, Width=200.)
    slider.Value <- float TrackerModelOptions.Volume
    //The below is an important control for the UI. The slider.ValueChanged.Add(fun _ ->  is the event handler for the slider. It is triggered when the slider value changes.
    //The code inside the event handler is executed when the slider value changes.
    //This specifically keys to data that is part of the TrackerModelOptions class. ALSO the voice.Volume <- TrackerModelOptions.Volume is important for the voice to actually change.
    //The Graphics.volumeChanged.Trigger seems redundant but is important for the UI to update correctly.
    slider.ValueChanged.Add(fun _ -> 
        TrackerModelOptions.Volume <- int slider.Value
        Graphics.volumeChanged.Trigger(TrackerModelOptions.Volume)
        if not(TrackerModelOptions.IsMuted) then 
            voice.Volume <- TrackerModelOptions.Volume
        )
    let dp = new DockPanel(VerticalAlignment=VerticalAlignment.Center, Margin=Thickness(0.))
    dp.Children.Add(slider) |> ignore
    options2Topsp.Children.Add(dp) |> ignore
    options2sp.Children.Add(options2Topsp) |> ignore
    // stop
    let muteCB = new CheckBox(Content=new TextBox(Text="Disable all",IsReadOnly=true))
    muteCB.ToolTip <- "Turn off all reminders (but you can still view them by\nclicking the reminder log in the upper-right of the timeline)"
    muteCB.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.IsMuted
    muteCB.Checked.Add(fun _ -> TrackerModelOptions.IsMuted <- true; voice.Volume <- 0)
    muteCB.Unchecked.Add(fun _ -> TrackerModelOptions.IsMuted <- false; voice.Volume <- TrackerModelOptions.Volume)
    options2sp.Children.Add(muteCB) |> ignore
    // other settings
    let options2Grid = new Grid()
    for i = 1 to 3 do
        options2Grid.ColumnDefinitions.Add(new ColumnDefinition(Width=GridLength.Auto))
    for i = 0 to data2.Length do
        options2Grid.RowDefinitions.Add(new RowDefinition(Height=GridLength.Auto))
    let voiceTB = new TextBox(Text="Voice",IsReadOnly=true)
    voiceTB.ToolTip <- "The reminder will be spoken aloud"
    Graphics.gridAdd(options2Grid, voiceTB, 0, 0)
    let visualTB = new TextBox(Text="Visual",IsReadOnly=true)
    visualTB.ToolTip <- "The reminder will be displayed as icons in the upper right of the Timeline"
    Graphics.gridAdd(options2Grid, visualTB, 1, 0)
    let mutable row = 1
    for text,tip,bVoice,bVisual in data2 do
        if row%2=1 then
            let backgroundColor() = new DockPanel(Background=Graphics.almostBlack)
            Graphics.gridAdd(options2Grid, backgroundColor(), 0, row)
            Graphics.gridAdd(options2Grid, backgroundColor(), 1, row)
            Graphics.gridAdd(options2Grid, backgroundColor(), 2, row)
        let cbVoice = new CheckBox(HorizontalAlignment=HorizontalAlignment.Center)
        link(cbVoice, bVoice, false, fun()->())
        Graphics.gridAdd(options2Grid, cbVoice, 0, row)
        let cbVisual = new CheckBox(HorizontalAlignment=HorizontalAlignment.Center)
        link(cbVisual, bVisual, false, fun()->())
        Graphics.gridAdd(options2Grid, cbVisual, 1, row)
        let tb = new TextBox(Text=text,IsReadOnly=true, Background=Brushes.Transparent)
        tb.ToolTip <- tip
        ToolTipService.SetShowDuration(tb, 10000)
        Graphics.gridAdd(options2Grid, tb, 2, row)
        row <- row + 1

    options2sp.Children.Add(options2Grid) |> ignore
    if voice.GetInstalledVoices() |> Seq.filter (fun v -> v.Enabled) |> Seq.length > 1 then
        let changeVoiceButton = Graphics.makeButton("Change voice",None,None)
        changeVoiceButton.HorizontalAlignment <- HorizontalAlignment.Left
        do  // scope local mutable popupIsActive variable to this one button
            let mutable popupIsActive = false  // second level of popup, need local copy
            let mutable changedGlobal = false
            changeVoiceButton.Click.Add(fun _ ->
                if not popupIsActive then
                    popupIsActive <- true
                    // the options menu is not popped up on the start screen, but is in the main app, so we need to make this consistent for both
                    if not CustomComboBoxes.GlobalFlag.popupIsActive then
                        CustomComboBoxes.GlobalFlag.popupIsActive <- true  // must be start screen
                        changedGlobal <- true
                    let wh = new System.Threading.ManualResetEvent(false)
                    let sp = new StackPanel(Orientation=Orientation.Vertical)
                    AddStyle(sp)
                    sp.Children.Add(new TextBox(Text="Select preferred voice",IsReadOnly=true)) |> ignore
                    for v in voice.GetInstalledVoices() do
                        if v.Enabled then
                            let name = v.VoiceInfo.Name
                            let r = new StackPanel(Orientation=Orientation.Horizontal)
                            r.Children.Add(new TextBox(Text=name,IsReadOnly=true,Width=250.)) |> ignore
                            let testSpeech(f) =   // ensure we test aloud
                                let wasMuted = TrackerModelOptions.IsMuted
                                let wasVolumeOtherwiseZero = not wasMuted && TrackerModelOptions.Volume=0
                                if wasMuted || wasVolumeOtherwiseZero then
                                    voice.Volume <- 30  // default
                                f()
                                if wasVolumeOtherwiseZero || wasMuted then
                                    voice.Volume <- 0
                            let sb = Graphics.makeButton("Test it",None,None)
                            sb.Click.Add(fun _ -> testSpeech(fun() -> voice.SelectVoice(name); voice.Speak("Hello")))
                            r.Children.Add(sb) |> ignore
                            let sb = Graphics.makeButton("Choose this",None,None)
                            sb.Click.Add(fun _ -> testSpeech(fun() -> voice.SelectVoice(name); voice.Speak("Voice chosen"); TrackerModelOptions.PreferredVoice <- name; wh.Set() |> ignore))
                            r.Children.Add(sb) |> ignore
                            sp.Children.Add(r) |> ignore
                    async {
                        do! CustomComboBoxes.DoModalDocked(cm, wh, Dock.Bottom, new Border(Child=sp, BorderBrush=Brushes.Gray, BorderThickness=Thickness(3.), HorizontalAlignment=HorizontalAlignment.Center))
                        try
                            voice.SelectVoice(TrackerModelOptions.PreferredVoice)
                        with _ -> 
                            try
                                voice.SelectVoice(defaultVoice)
                            with _ -> ()
                        popupIsActive <- false
                        if changedGlobal then
                            CustomComboBoxes.GlobalFlag.popupIsActive <- false
                    } |> Async.StartImmediate
                )
        options2sp.Children.Add(changeVoiceButton) |> ignore

    optionsAllsp.Children.Add(new DockPanel(Width=2.,Background=Brushes.Gray)) |> ignore
    optionsAllsp.Children.Add(options2sp) |> ignore
    optionsAllsp.Children.Add(new DockPanel(Width=2.,Background=Brushes.Gray)) |> ignore

    let options3sp = new StackPanel(Orientation=Orientation.Vertical, Margin=Thickness(10.,2.,0.,0.))
    let tb = new TextBox(Text="Other", IsReadOnly=true, FontWeight=FontWeights.Bold) |> header
    options3sp.Children.Add(tb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Animate tile changes",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.AnimateTileChanges.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.AnimateTileChanges.Value <- true)
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.AnimateTileChanges.Value <- false)
    cb.ToolTip <- "When you change an overworld map spot or a dungeon room type, briefly animate the rectangle to highlight what changed"
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Animate shop highlights",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.AnimateShopHighlights.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.AnimateShopHighlights.Value <- true)
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.AnimateShopHighlights.Value <- false)
    cb.ToolTip <- "When you mouse hover certain icons to highlight corresponding shops on the map, briefly animate the rectangle"
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Save on completion",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.SaveOnCompletion.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.SaveOnCompletion.Value <- true)
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.SaveOnCompletion.Value <- false)
    cb.ToolTip <- "When you click Zelda to complete the seed, automatically save the full tracker state to a file"
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Snoop for seed&flags",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.SnoopSeedAndFlags.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.SnoopSeedAndFlags.Value <- true; SaveAndLoad.MaybePollSeedAndFlags())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.SnoopSeedAndFlags.Value <- false)
    cb.ToolTip <- "Periodically check for other system windows (e.g. fceux)\nthat appear to have a seed and flag in the title, to\ninclude with save data and optionally display"
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Display seed&flags",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.DisplaySeedAndFlags.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.DisplaySeedAndFlags.Value <- true; SaveAndLoad.seedAndFlagsUpdated.Trigger())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.DisplaySeedAndFlags.Value <- false; SaveAndLoad.seedAndFlagsUpdated.Trigger())
    cb.ToolTip <- "Display seed & flags (if known) in the bottom corner of Notes box"
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Listen for speech",IsReadOnly=true))
    if microphoneFailedToInitialize then
        cb.IsEnabled <- false
        cb.IsChecked <- System.Nullable.op_Implicit false
        cb.ToolTip <- "Disabled (microphone was not initialized properly during startup)"
        ToolTipService.SetShowDuration(cb, 10000)
        ToolTipService.SetShowOnDisabled(cb, true)
    else
        cb.ToolTip <- "Use the microphone to listen for spoken map update commands\nExample: say 'tracker set bomb shop' while hovering an unmarked map tile"
        ToolTipService.SetShowDuration(cb, 10000)
        link(cb, TrackerModelOptions.ListenForSpeech, false, fun()->())
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Confirmation sound",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    if microphoneFailedToInitialize then
        cb.IsEnabled <- false
        cb.IsChecked <- System.Nullable.op_Implicit false
        cb.ToolTip <- "Disabled (microphone was not initialized properly during startup)"
        ToolTipService.SetShowDuration(cb, 10000)
        ToolTipService.SetShowOnDisabled(cb, true)
    else
        cb.ToolTip <- "Play a confirmation sound whenever speech recognition is used to make an update to the tracker"
        ToolTipService.SetShowDuration(cb, 10000)
        link(cb, TrackerModelOptions.PlaySoundWhenUseSpeech, false, fun()->())
    options3sp.Children.Add(cb) |> ignore

(*  // this is not (yet) a fully supported feature, so don't publish it on the options menu
    let cb = new CheckBox(Content=new TextBox(Text="Require PTT",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    if microphoneFailedToInitialize then
        cb.IsEnabled <- false
        cb.IsChecked <- System.Nullable.op_Implicit false
        cb.ToolTip <- "Disabled (microphone was not initialized properly during startup)"
        ToolTipService.SetShowDuration(cb, 10000)
        ToolTipService.SetShowOnDisabled(cb, true)
    elif gamepadFailedToInitialize then
        cb.IsEnabled <- false
        cb.IsChecked <- System.Nullable.op_Implicit false
        cb.ToolTip <- "Disabled (gamepad was not initialized properly during startup)"
        ToolTipService.SetShowDuration(cb, 10000)
        ToolTipService.SetShowOnDisabled(cb, true)
    else
        link(cb, TrackerModelOptions.RequirePTTForSpeech, false)
        cb.ToolTip <- "Only listen for speech when Push-To-Talk button is held (SNES gamepad left shoulder button)"
        ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore
*)

    let cb = new CheckBox(Content=new TextBox(Text="Broadcast window",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.ShowBroadcastWindow.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.ShowBroadcastWindow.Value <- true; broadcastWindowOptionChanged.Trigger())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.ShowBroadcastWindow.Value <- false; broadcastWindowOptionChanged.Trigger())
    cb.ToolTip <- "Open a separate, smaller window, for stream capture.\nYou still interact with the original large window,\nbut the smaller window will focus the view on either the overworld or\nthe dungeon tabs, based on your mouse position."
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let rb3 = new RadioButton(Content=new TextBox(Text="Full size broadcast",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    let rb2 = new RadioButton(Content=new TextBox(Text="2/3 size broadcast",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    let rb1 = new RadioButton(Content=new TextBox(Text="1/3 size broadcast",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    match TrackerModelOptions.BroadcastWindowSize with
    | 3 -> rb3.IsChecked <- System.Nullable.op_Implicit true
    | 2 -> rb2.IsChecked <- System.Nullable.op_Implicit true
    | 1 -> rb1.IsChecked <- System.Nullable.op_Implicit true
    | _ -> failwith "impossible BroadcastWindowSize"
    rb3.Checked.Add(fun _ -> TrackerModelOptions.BroadcastWindowSize <- 3; broadcastWindowOptionChanged.Trigger())
    rb2.Checked.Add(fun _ -> TrackerModelOptions.BroadcastWindowSize <- 2; broadcastWindowOptionChanged.Trigger())
    rb1.Checked.Add(fun _ -> TrackerModelOptions.BroadcastWindowSize <- 1; broadcastWindowOptionChanged.Trigger())
    options3sp.Children.Add(rb3) |> ignore
    options3sp.Children.Add(rb2) |> ignore
    options3sp.Children.Add(rb1) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Include overworld magnifier",IsReadOnly=true), Margin=Thickness(20.,0.,0.,0.))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.BroadcastWindowIncludesOverworldMagnifier.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.BroadcastWindowIncludesOverworldMagnifier.Value <- true; broadcastWindowOptionChanged.Trigger())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.BroadcastWindowIncludesOverworldMagnifier.Value <- false; broadcastWindowOptionChanged.Trigger())
    cb.ToolTip <- "Whether to include the overworld magnifier when it is on-screen, which will obscure some other elements"
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Mouse magnifier window",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.ShowMouseMagnifierWindow.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.ShowMouseMagnifierWindow.Value <- true; mouseMagnifierWindowOptionChanged.Trigger())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.ShowMouseMagnifierWindow.Value <- false; mouseMagnifierWindowOptionChanged.Trigger())
    cb.ToolTip <- "Open a separate, resizable window that shows a\nzoomed-in area around the mouse.\nCan be useful when using the app on a small screen\nwhere precise mouse targetting is hard to see."
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    let cb = new CheckBox(Content=new TextBox(Text="Hide timer",IsReadOnly=true))
    cb.IsChecked <- System.Nullable.op_Implicit TrackerModelOptions.HideTimer.Value
    cb.Checked.Add(fun _ -> TrackerModelOptions.HideTimer.Value <- true; hideTimerChanged.Trigger())
    cb.Unchecked.Add(fun _ -> TrackerModelOptions.HideTimer.Value <- false; hideTimerChanged.Trigger())
    cb.ToolTip <- "Don't display the bright green timer in the upper right corner."
    ToolTipService.SetShowDuration(cb, 10000)
    options3sp.Children.Add(cb) |> ignore

    optionsAllsp.Children.Add(options3sp) |> ignore

    let total = new StackPanel(Orientation=Orientation.Vertical)
    if includePopupExplainer then
        let tb1 = new TextBox(Text="Options Menu", IsReadOnly=true, FontWeight=FontWeights.Bold, HorizontalAlignment=HorizontalAlignment.Center)
        let tb2 = new TextBox(Text="options are automatically applied and saved when dismissing this popup (by clicking outside it)", 
                                IsReadOnly=true, Margin=Thickness(0.,0.,0.,6.), HorizontalAlignment=HorizontalAlignment.Center)
        total.Children.Add(tb1) |> ignore
        total.Children.Add(tb2) |> ignore
        total.Children.Add(new DockPanel(Height=2.,Background=Brushes.Gray)) |> ignore

    total.Children.Add(optionsAllsp) |> ignore

    all.Child <- total
    all