/// Concrete WPF / Windows implementations of the PlatformServices interfaces.
/// Call register() once at app startup (before any service is used).
module WpfPlatformServices

open System
open System.Windows.Media

// ─── Audio ────────────────────────────────────────────────────────────────

type WpfAudioPlayer() =
    let confirmPlayer  = new MediaPlayer()
    let reminderPlayer = new MediaPlayer()
    do
        confirmPlayer.Volume  <- float TrackerModelOptions.Volume / 300.
        confirmPlayer.Open(new Uri("confirm_speech.wav", UriKind.Relative))
        reminderPlayer.Volume <- float TrackerModelOptions.Volume / 300.
        reminderPlayer.Open(new Uri("reminder_clink.wav", UriKind.Relative))
        Graphics.volumeChanged.Publish.Add(fun v ->
            confirmPlayer.Volume  <- float v / 300.
            reminderPlayer.Volume <- float v / 300.)
    interface PlatformServices.IAudioPlayer with
        member _.PlayConfirmSpeech() =
            confirmPlayer.Position <- TimeSpan(0L)
            confirmPlayer.Play()
        member _.PlayReminderClink() =
            reminderPlayer.Position <- TimeSpan(0L)
            reminderPlayer.Play()
        member _.PlaySystemAsterisk() =
            System.Media.SystemSounds.Asterisk.Play()

// ─── Shell open ──────────────────────────────────────────────────────────

type WpfShellOpen() =
    interface PlatformServices.IShellOpen with
        member _.OpenFolder(path) =
            // If path is a file, open Explorer with /Select so the file is highlighted.
            // If path is a directory, open it directly.
            let arg =
                if System.IO.Directory.Exists(path) then path
                else sprintf "/Select, \"%s\"" path
            let psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe", arg)
            System.Diagnostics.Process.Start(psi) |> ignore
        member _.OpenUrl(url) =
            // Process.Start with a URL invokes the default browser on Windows.
            let psi = new System.Diagnostics.ProcessStartInfo(url)
            psi.UseShellExecute <- true
            System.Diagnostics.Process.Start(psi) |> ignore

// ─── Gamepad ──────────────────────────────────────────────────────────────

type WpfGamepadInput() =
    interface PlatformServices.IGamepadInput with
        member _.IsLeftShoulderButtonDown()            = Gamepad.IsLeftShoulderButtonDown()
        member _.LeftShoulderButtonMostRecentRelease   = Gamepad.LeftShoulderButtonMostRecentRelease
        member _.ControllerFailureEvent                = Gamepad.ControllerFailureEvent.Publish :> IEvent<exn>
        member _.Initialize()                          = Gamepad.Initialize()

// ─── TTS engine ──────────────────────────────────────────────────────────

type WpfTtsEngine() =
    let synth = new System.Speech.Synthesis.SpeechSynthesizer()
    interface PlatformServices.ITtsEngine with
        member _.DefaultVoiceName        = try synth.Voice.Name with _ -> ""
        member _.Volume
            with get()  = synth.Volume
            and  set(v) = synth.Volume <- v
        member _.SelectVoice(name)       = try synth.SelectVoice(name) with _ -> ()
        member _.Speak(text)             = synth.Speak(text)
        member _.GetInstalledVoiceNames() =
            synth.GetInstalledVoices()
            |> Seq.filter (fun v -> v.Enabled)
            |> Seq.map (fun v -> v.VoiceInfo.Name)

// ─── Speech recognition ──────────────────────────────────────────────────

type WpfSpeechService() =
    let speechRecognizer = new System.Speech.Recognition.SpeechRecognitionEngine()
    let wakePhrase = "tracker set"
    // mutable state set by Configure()
    let mutable currentKind    = TrackerModel.DungeonTrackerInstanceKind.DEFAULT
    let mutable mapStatePhrases: System.Collections.Generic.IDictionary<string,int> =
        upcast dict [||]

    let buildPhrases kind =
        let coda = [|
            "level nine"        ,  8
            "any road"          , 12  // 9 10 11 12
            "sword three"       , 13
            "sword two"         , 14
            "sword one"         , 15
            "arrow shop"        , 16
            "bomb shop"         , 17
            "book shop"         , 18
            "candle shop"       , 19
            "blue ring shop"    , 20
            "meat shop"         , 21
            "key shop"          , 22
            "shield shop"       , 23
            "unknown secret"    , 24
            "large secret"      , 25
            "medium secret"     , 26
            "small secret"      , 27
            "door repair"       , 28
            "money making game" , 29
            "the letter"        , 30
            "arm owes"          , 31  // armos
            "hint shop"         , 32
            "take any"          , 33
            "potion shop"       , 34
            "don't care"        , 35
            "nothing"           , 35
            |]
        match kind with
        | TrackerModel.DungeonTrackerInstanceKind.HIDE_DUNGEON_NUMBERS ->
            dict [|
                yield "level" , 0   // maps to 0..7 via CanAddUse search
                yield! coda |]
        | _ ->
            dict [|
                yield "level one"  , 0
                yield "level two"  , 1
                yield "level three", 2
                yield "level four" , 3
                yield "level five" , 4
                yield "level six"  , 5
                yield "level seven", 6
                yield "level eight", 7
                yield! coda |]

    interface PlatformServices.ISpeechRecognitionService with

        member _.Configure(kind) =
            currentKind    <- kind
            mapStatePhrases <- buildPhrases kind
            let gb = new System.Speech.Recognition.GrammarBuilder(wakePhrase)
            gb.Append(new System.Speech.Recognition.Choices(
                          mapStatePhrases.Keys |> Seq.toArray))
            speechRecognizer.LoadGrammar(new System.Speech.Recognition.Grammar(gb))

        member _.Start() =
            speechRecognizer.SetInputToDefaultAudioDevice()
            speechRecognizer.RecognizeAsync(
                System.Speech.Recognition.RecognizeMode.Multiple)

        member _.OnRecognized(callback) =
            speechRecognizer.SpeechRecognized.Add(fun r ->
                if TrackerModelOptions.ListenForSpeech.Value then
                    let gamepadActive =
                        match PlatformServices.gamepadInput with
                        | Some g ->
                            g.IsLeftShoulderButtonDown() ||
                            (DateTime.Now - g.LeftShoulderButtonMostRecentRelease)
                                < TimeSpan.FromSeconds(1.0)
                        | None -> false
                    if not TrackerModelOptions.RequirePTTForSpeech.Value || gamepadActive then
                        let threshold =
                            if TrackerModelOptions.RequirePTTForSpeech.Value then 0.90f
                            else 0.94f
                        if r.Result.Confidence > threshold then
                            callback r.Result.Text)

        member _.ConvertPhraseToCell(phrase) =
            // Strip the wake phrase prefix
            let body = phrase.Substring(wakePhrase.Length + 1)
            match mapStatePhrases.TryGetValue(body) with
            | false, _ -> None
            | true, 12 ->   // "any road" — find first available any-road cell
                [9;10;11;12]
                |> List.tryFind (fun i ->
                    TrackerModel.mapSquareChoiceDomain.CanAddUse(i))
            | true, newState ->
                if currentKind = TrackerModel.DungeonTrackerInstanceKind.HIDE_DUNGEON_NUMBERS
                        && newState = 0 then
                    // "level" maps to 0-7; find first available
                    [0..7]
                    |> List.tryFind (fun i ->
                        TrackerModel.mapSquareChoiceDomain.CanAddUse(i))
                else
                    if TrackerModel.mapSquareChoiceDomain.CanAddUse(newState)
                    then Some newState
                    else None

// ─── Registration ─────────────────────────────────────────────────────────

/// Call once at startup, before any platform service is used.
let register() =
    PlatformServices.audioPlayer  <- WpfAudioPlayer()
    PlatformServices.shellOpen    <- WpfShellOpen()

    let gamepad = WpfGamepadInput()
    PlatformServices.gamepadInput <- Some gamepad

    let tts = WpfTtsEngine()
    PlatformServices.ttsEngine   <- Some tts
    // Let OptionsMenu read the default voice name after the engine is live.
    OptionsMenu.defaultVoice <- (tts :> PlatformServices.ITtsEngine).DefaultVoiceName

    PlatformServices.speechService <- Some(WpfSpeechService())
