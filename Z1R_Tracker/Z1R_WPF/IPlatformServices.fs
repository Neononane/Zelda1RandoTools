/// Platform service interfaces and default no-op implementations.
///
/// At startup, WpfPlatformServices.register() replaces the no-ops with
/// concrete WPF/Windows implementations.  Future Avalonia/Linux builds will
/// supply their own implementations without touching any other source file.
module PlatformServices

// ─── Interface definitions ────────────────────────────────────────────────

/// Plays audio cues that are tied to tracker events.
type IAudioPlayer =
    abstract PlayConfirmSpeech:  unit -> unit
    abstract PlayReminderClink:  unit -> unit
    abstract PlaySystemAsterisk: unit -> unit

/// Opens a folder in the native file manager, or a URL in the default browser.
type IShellOpen =
    abstract OpenFolder: path:string -> unit
    abstract OpenUrl:    url:string  -> unit

/// Gamepad state used for push-to-talk support.
type IGamepadInput =
    abstract IsLeftShoulderButtonDown:           unit -> bool
    abstract LeftShoulderButtonMostRecentRelease: System.DateTime
    abstract ControllerFailureEvent:             IEvent<exn>
    abstract Initialize:                         unit -> bool

/// Text-to-speech synthesis (voice confirmation of tracker commands).
type ITtsEngine =
    abstract DefaultVoiceName:   string
    abstract Volume:             int with get, set
    abstract SelectVoice:        name:string -> unit
    abstract Speak:              text:string -> unit
    abstract GetInstalledVoiceNames: unit -> string seq

/// Voice-command speech recognition for the tracker.
type ISpeechRecognitionService =
    /// Configure grammar for the given tracker kind (call before Start).
    abstract Configure:          kind:TrackerModel.DungeonTrackerInstanceKind -> unit
    /// Start listening (SetInputToDefaultAudioDevice + RecognizeAsync).
    abstract Start:              unit -> unit
    /// Register a callback fired when a phrase is recognized above the
    /// confidence threshold.  Called on an arbitrary thread — caller marshals.
    abstract OnRecognized:       callback:(string -> unit) -> unit
    /// Map a recognized phrase to a tracker map-cell index, or None if unavailable.
    abstract ConvertPhraseToCell: phrase:string -> int option

// ─── Default no-op implementations (replaced at startup) ─────────────────

let mutable audioPlayer : IAudioPlayer =
    { new IAudioPlayer with
        member _.PlayConfirmSpeech()  = ()
        member _.PlayReminderClink()  = ()
        member _.PlaySystemAsterisk() = () }

let mutable shellOpen : IShellOpen =
    { new IShellOpen with
        member _.OpenFolder _ = ()
        member _.OpenUrl    _ = () }

let mutable gamepadInput  : IGamepadInput option            = None
let mutable ttsEngine     : ITtsEngine    option            = None
let mutable speechService : ISpeechRecognitionService option = None
