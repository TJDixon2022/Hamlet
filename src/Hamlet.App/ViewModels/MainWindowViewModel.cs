using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Hamlet.App.Licensing;
using Hamlet.App.Settings;
using Hamlet.App.Startup;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Solar;
using Hamlet.RadioEngine.Training;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.App.ViewModels;

/// <summary>
/// Shell ViewModel. One source of truth — <see cref="FrequencyHz"/> — that
/// the digits, ribbon and tape all bind to two-way. UI-origin changes are
/// throttled out to the rig; rig-origin changes (the physical knob) flow in
/// without echoing back out.
/// </summary>
/// <remarks>
/// It also owns the happening-now feed's clock (HM-DEC-020): a refresh timer
/// on the operator's interval, a one-second timer that keeps every displayed
/// age honest, and a pause when the window is not on screen. The freshness
/// rule itself lives in <see cref="SpotFreshness"/> as pure functions, so the
/// thresholds are testable without waiting for real minutes to pass.
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// The no-hardware entry in the port list — something to choose on
    /// purpose rather than a fallback (HM-DEC-026).
    /// </summary>
    public const string TrainingRadio = "Training radio (no hardware)";

    /// <summary>How long a newly-arrived spot wears its "new" tag.</summary>
    public static readonly TimeSpan NewSpotTagLifetime = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan RigSendThrottle = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan AgeTick = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How long the dial has to sit still before the mode follows it.
    /// </summary>
    /// <remarks>
    /// Long enough that a drag across three neighborhoods is one change rather
    /// than three (HM-DEC-056). Changing mode is not like nudging a frequency:
    /// the radio mutes for an instant each time, so a flurry of them through one
    /// gesture would sound broken.
    /// </remarks>
    private static readonly TimeSpan ModeSettleDelay = TimeSpan.FromMilliseconds(600);

    /// <summary>How often the dial is looked at to see whether it has stopped.</summary>
    /// <remarks>
    /// **THE CONDITION IS AN UNCHANGED FREQUENCY ACROSS CONSECUTIVE LOOKS**
    /// (work instruction 050, task 4), so there has to be a look even while
    /// nothing is happening. The frequency raises a change notification only when
    /// it changes, and a dial standing still raises none at all — a settle that
    /// listened to changes alone could never observe stillness. Four times a
    /// second is the rig poll's own cadence (HM-DEC-050).
    /// </remarks>
    private static readonly TimeSpan DwellLook = TimeSpan.FromMilliseconds(250);

    private readonly DispatcherTimer _rigSendTimer;
    private readonly DispatcherTimer _modeSettleTimer;
    private readonly DispatcherTimer _dwellTimer;

    /// <summary>What Hamlet last set on the receive side (work instruction 042).</summary>
    private ReceiverSetupMemory _receiverMemory = ReceiverSetupMemory.Empty;

    /// <summary>
    /// The block whose conditions have been established, by its lower edge.
    /// </summary>
    /// <remarks>
    /// **ONCE PER TUNE-IN, THEN HANDS OFF.** Keyed by the block rather than by
    /// the dial, because nudging the VFO a hundred hertz inside an FT8 block is
    /// not arriving somewhere new, and re-asserting the noise controls every time
    /// the knob moved would be the app fighting the operator for his own radio.
    /// </remarks>
    private long? _conditionsSetForBlockHz;

    /// <summary>What the last tune-in did to the receive side.</summary>
    internal IReadOnlyList<ConditionResult> LastReceiverSetup { get; private set; }
        = Array.Empty<ConditionResult>();
    private readonly DispatcherTimer _spotRefreshTimer;
    private readonly DispatcherTimer _ageTimer;
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;
    private readonly List<ActivitySpot> _allBandSpots = new();

    /// <summary>
    /// Spots the operator tuned to in this session.
    /// </summary>
    /// <remarks>
    /// The store keeps the durable record; this covers the moments between an
    /// operator clicking Tune and the next write reaching the disk, so a card
    /// they have just been to does not reappear under "what's new" one refresh
    /// later (HM-DEC-057).
    /// </remarks>
    /// <summary>
    /// Whether the dial has sat still long enough to be worth remembering
    /// (HM-DEC-072).
    /// </summary>
    private readonly DwellTracker _dwell = new();

    /// <summary>
    /// A callsign the operator arrived on, and the frequency it belongs to.
    /// </summary>
    /// <remarks>
    /// TIED TO ITS FREQUENCY ON PURPOSE (§0.0). A call held loose would still be
    /// attached after the dial had moved somewhere else, and the recent list
    /// would name a station on a frequency nobody ever heard one on. Tying the
    /// two together means the pairing expires by itself the moment it stops
    /// being true.
    /// </remarks>
    private string _arrivedOnStation = "";
    private long _arrivedOnHz = -1;

    private readonly HashSet<string> _actedOnSpots =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Which of the two questions the happening-now list is answering
    /// (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// Not observable on its own: the two booleans below are what the segmented
    /// control binds to, because a toggle needs to know whether it is the one
    /// that is down.
    /// </remarks>
    private SpotLens _lens = SpotLens.BestChance;

    /// <summary>True once the operator has chosen a lens themselves.</summary>
    /// <remarks>
    /// The whole of "may never override the operator afterward". Inference runs
    /// while this is false and never once it is true.
    /// </remarks>
    private bool _lensChosenByOperator;
    private AggregateActivitySource _activitySource;
    private RbnActivitySource? _rbn;
    private IDisposable[] _ownedSources = Array.Empty<IDisposable>();
    private TrainingSpectrumSource? _trainingSpectrum;
    private readonly DispatcherTimer _decodeTimer;

    /// <summary>Asks the time servers how far the machine's clock is out.</summary>
    /// <remarks>
    /// **EVERY TEN MINUTES, AND THE INTERVAL IS REASONED.** A PC clock the
    /// operating system is already disciplining does not wander measurably in
    /// ten minutes, and the answer is only needed to place fifteen-second slot
    /// boundaries. Asking more often is a request a minute to somebody else's
    /// volunteer-run pool for a number that has not changed (HM-DEC-024); asking
    /// less often means an evening's session runs on one reading taken before
    /// the radio warmed up.
    /// </remarks>
    private readonly DispatcherTimer _clockTimer;

    private bool _clockQueryRunning;

    /// <summary>Notices when a fifteen-second FT8 slot has closed.</summary>
    /// <remarks>
    /// <para>**IT RIDES `_decodeTimer` RATHER THAN A TIMER OF ITS OWN** (unit
    /// 225). That timer already ticks four times a second while the decoder is
    /// listening, which is sixty looks inside every fifteen-second slot, and the
    /// watch's own de-duplication means the tick rate does not decide how many
    /// decodes happen. A second timer would be another thing to start, stop and
    /// dispose for no gain, and it would have to be kept in step with this one
    /// anyway, because the watch needs the same tap.</para>
    /// <para>**IT LOOKS ONLY WHILE THE DIGITAL TAB IS ON SCREEN**, and that is
    /// one boolean per tick. Decoding slots the operator cannot see would be a
    /// defensible choice, but it is a core burnt on a background tab for a table
    /// nobody is reading, and the watch re-arms when the tab goes away so that
    /// coming back never claims a slot that closed while nothing was watching.
    /// </para>
    /// </remarks>
    private readonly Ft8SlotWatch _slotWatch = new();

    /// <summary>True while a slot is being decoded off the UI thread.</summary>
    private bool _slotDecodeRunning;

    private RigStateMonitor? _rigMonitor;
    private IAudioSource? _audioInput;
    private CwDecoder? _decoder;

    /// <summary>Where the decoder's counters stood, recently, on the audio clock.</summary>
    private CwCounterTrail? _counters;

    /// <summary>
    /// Whether Hamlet can hear keying at all, said independently of the decoder.
    /// </summary>
    private CwKeyingMeter? _keyingMeter;

    /// <summary>The meter's work, off the interface thread.</summary>
    private Task<KeyingReading>? _meterWork;

    /// <summary>When the meter last looked.</summary>
    private DateTime _meterLastUtc = DateTime.MinValue;

    /// <summary>When the current decoder began listening, or null when none is.</summary>
    private DateTime? _decoderStartedUtc;
    private readonly Audio.ModeAudioPlayer _audio = new();
    private readonly PrivilegePlan _privileges = new();
    private CancellationTokenSource? _licenseLookup;
    private IRig? _rig;

    /// <summary>The serial port the connected radio is on, or null where none is.</summary>
    /// <remarks>
    /// <para>**THE APPLICATION ALREADY BUILT THIS AND THREW IT AWAY.**
    /// <see cref="CreateRig"/> constructed a <see cref="SystemSerialPort"/> inside
    /// the expression that handed it to <c>Ic7300Rig</c>, which keeps it in a
    /// private field and exposes no accessor, so the send path had no transport
    /// and <c>_armedSend</c> was assigned only by a test seam
    /// (`docs/unit260-route-trace.md` questions 3 and 5). This is the smaller of
    /// the two seams: a field beside <see cref="_rig"/> is visible to the send
    /// path and to nothing else, where an accessor on the rig would hand the port
    /// to everything holding one.</para>
    /// <para>**IT IS SET WHERE <see cref="_rig"/> IS SET AND NULLED IN THE SAME
    /// `finally`**, so there is no state in which Hamlet believes it has a port
    /// and has no radio.</para>
    /// <para>**THE TRAINING RADIO SETS NOTHING.** <c>TrainingRig</c> is a
    /// simulator with no port at all, and a simulator must never become a route
    /// to a keying frame.</para>
    /// </remarks>
    private ISerialPort? _rigPort;

    /// <summary>Why the send path cannot transmit, in the operator's terms, or "".</summary>
    /// <remarks>
    /// **SAID ON THE CLICK, NEVER GUESSED AROUND.** Empty means nothing has been
    /// attempted yet, and the refusal keeps the wording it had before this unit.
    /// </remarks>
    private string _transmitRefusal = "";

    private bool _updatingFromRig;
    private bool _rigSendPending;
    private ModeFollowState _modeFollow = ModeFollowState.Armed(false);

    /// <summary>Whether the dial has come to rest where it is (work instruction 050).</summary>
    private ModeDwell _modeDwell = ModeDwell.Nowhere;

    /// <summary>Why mode-follow last declined, for the diagnostics screen.</summary>
    /// <remarks>
    /// **KEPT RATHER THAN SAID** (work instruction 051, task 5). The status line
    /// stays silent, because a commentary on writes that nearly happened is noise
    /// on the one line the operator reads; this is what rig diagnostics shows
    /// somebody who has come looking for why nothing happened.
    /// </remarks>
    private string _modeFollowNote = "";
    private bool _settingModeOurselves;
    private CivMode? _lastKnownMode;
    private bool _windowVisible = true;
    private bool _spotsEverLoaded;
    private IReadOnlyList<StoredSpot> _bandHistory = Array.Empty<StoredSpot>();
    private RigSpectrumSource? _rigSpectrum;
    private int _lastNewSpotCount;

    /// <summary>
    /// Which mode the operating area is showing — CW, Digital or Voice.
    /// </summary>
    /// <remarks>
    /// <para>**THE HEADER DOES NOT READ THIS AND THAT IS THE POINT** (Tim's
    /// ruling of 2026-08-27). The band plan, the neighborhood and the radio are
    /// the same in every mode and sit above the divider, so they are outside the
    /// tab region entirely and cannot be re-created when this changes. A test
    /// holds that, because it is the kind of thing a later layout edit undoes
    /// without anybody noticing.</para>
    /// <para>The tray is outside the tab region too: the widgets are shared
    /// across modes and are dragged onto whichever panel is showing.</para>
    /// </remarks>
    [ObservableProperty]
    private string _operatingMode = "CW";

    /// <summary>The three modes the operating area offers.</summary>
    public IReadOnlyList<string> OperatingModes { get; } =
        new[] { "CW", "Digital", "Voice" };

    /// <summary>The three tabs, each knowing whether it is the one showing.</summary>
    /// <remarks>
    /// See <see cref="ModeTabViewModel"/> for why selection is state on the tab
    /// rather than a comparison done in a converter at render time.
    /// </remarks>
    public IReadOnlyList<ModeTabViewModel> ModeTabs { get; }

    /// <summary>True while the CW tab is the one showing.</summary>
    public bool IsCwMode => OperatingMode == "CW";

    /// <summary>True while the Digital tab is the one showing.</summary>
    public bool IsDigitalMode => OperatingMode == "Digital";

    /// <summary>True while the Voice tab is the one showing.</summary>
    public bool IsVoiceMode => OperatingMode == "Voice";

    /// <summary>
    /// Which digital sub-mode the operator last chose, or null where he has
    /// chosen none.
    /// </summary>
    /// <remarks>
    /// <para>**A PREFERENCE AND NOT A READING** (unit 251 task 3). The strip's
    /// lit chip says where the dial is and this says what he asked for. They are
    /// separate properties, separate flags on the chip and separate appearances,
    /// because a remembered press drawn as a lit chip would claim the radio is
    /// somewhere nobody measured (§0.0, HM-DEC-092).</para>
    /// <para>**NULL IS A REAL VALUE.** A fresh `settings.json` has no sub-mode in
    /// it, and until he presses one the strip has nothing chosen. That is
    /// different from having chosen FT8, and it is stored differently.</para>
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalModeChips))]
    private string? _chosenDigitalMode;

    partial void OnChosenDigitalModeChanged(string? value)
    {
        _settings.LastDigitalSubMode = value;
        SettingsStore.Save(_settings);
    }

    /// <summary>Which mode the licence card should answer for.</summary>
    /// <remarks>
    /// **THE TAB, AND NOT THE BLOCK THE DIAL IS IN** (unit 251 task 5). The two
    /// are different questions: the map says what other people are doing at this
    /// frequency, and this says what the operator would be doing if he
    /// transmitted. Somebody sitting in the FT8 block on the CW tab is asking
    /// about Morse, and 97.305(a) has a different answer for him than for the man
    /// on the Digital tab in the same place.
    /// </remarks>
    private TransmitMode LicenceModeForTheTab => OperatingMode switch
    {
        "Digital" => TransmitMode.Data,
        "Voice" => TransmitMode.Phone,
        _ => TransmitMode.Cw,
    };

    partial void OnOperatingModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsCwMode));
        OnPropertyChanged(nameof(IsDigitalMode));
        OnPropertyChanged(nameof(IsVoiceMode));

        // **THE LICENCE CARD FOLLOWS THE TAB** (unit 251 task 5). Changing tab
        // changes what the operator would be transmitting, so it changes what his
        // licence has to say about where he is.
        UpdatePrivileges();

        // **REMEMBERED THE MOMENT IT CHANGES, NOT AT SHUTDOWN** (unit 251 task
        // 3). A preference written only on a clean exit is a preference lost
        // whenever the app is closed the way people actually close it.
        _settings.LastOperatingMode = value;
        SettingsStore.Save(_settings);

        // The mode can be set from either end — a tab press, or code — and the
        // tabs follow it either way.
        foreach (var tab in ModeTabs)
        {
            tab.Follow(tab.Name == value);
        }

        // **ARRIVING ON A TAB IS ARRIVING SOMEWHERE, AND THE RADIO HAS TO BE
        // ABLE TO WORK THERE** (work instruction 041, task 2). On 2026-08-28 the
        // operator reported that switching to CW put the radio in CW and
        // switching back to Digital did not restore USB-D. **The two directions
        // were not asymmetric — neither of them did anything.** This handler
        // raised three property notifications and synchronised the tab strip,
        // and touched the radio nowhere; every mode write in the application
        // came from the dial moving.
        //
        // **WHAT IS WRITTEN IS WHAT THE MAP SAYS LIVES AT THE DIAL**, generated
        // from the band-plan row rather than from the tab's name (§0). A tab is
        // not a mode: the Digital tab at an FT8 frequency wants USB-D, and at a
        // frequency the map calls Morse it wants what the map says, because the
        // map is the source of truth about the band and the tab is a view of it.
        //
        // **HM-DEC-056 IS UNTOUCHED AND STILL GOVERNS.** This goes through the
        // same settle timer the dial does, so the operator's own hand still
        // wins, the suspension is still visible, and a value the radio did not
        // confirm is still unknown rather than assumed.
        ScheduleModeFollow();
    }


    /// <summary>What the operator has composed to send.</summary>
    /// <remarks>
    /// <para>**IT COMPOSES AND IT DOES NOT KEY.** Nothing in this view model
    /// reaches the transmitter from here, and that is the whole of what this
    /// panel is for today: somewhere to put the words while the parts that would
    /// carry them are still being argued about. §0.2 is untouched and so is
    /// HM-DEC-098 — a transmit path is a separate ruling taken after every
    /// interlock has been watched to fire into a dummy load.</para>
    /// <para>The buttons fill this line rather than sending it, so the operator
    /// can see exactly what would go out before anything ever does.</para>
    /// </remarks>
    [ObservableProperty]
    private string _sendText = "";

    /// <summary>Put a call in the send line, in the operator's own callsign.</summary>
    [RelayCommand]
    private void ComposeCq()
    {
        var mine = _settings.Operator.Callsign?.Trim();

        // **THE MACROS FILL THE LINE THE SEND BUTTON SENDS** (Tim, 2026-08-27).
        // `SendText` used to be a box of its own that nothing transmitted; the
        // line is `Transmit.OwnWords.Message` now, which is what `PressCommand`
        // puts on the air.
        Transmit.OwnWords.Message = string.IsNullOrWhiteSpace(mine)
            ? "CQ CQ DE ... ... K"
            : $"CQ CQ DE {mine} {mine} K";
    }

    /// <summary>Put a signal report in the send line.</summary>
    /// <remarks>
    /// **599 IS A POLITE FICTION AND HAMLET DOES NOT PRETEND OTHERWISE**
    /// (HM-DEC-042). It is what nearly every contest exchange says whatever was
    /// actually heard, and it is offered here as the phrase people use rather
    /// than as a measurement of anything.
    /// </remarks>
    [RelayCommand]
    private void ComposeRst() => Transmit.OwnWords.Message = "RST 599 599";

    /// <summary>Put a sign-off in the send line.</summary>
    [RelayCommand]
    private void ComposeSeventyThree()
        => Transmit.OwnWords.Message = "73 TU E E";

    /// <summary>Clear the send line.</summary>
    /// <remarks>
    /// **THE SEND BUTTON DOES NOT SEND** and says so where the operator can see
    /// it. Wiring it to the transmitter is outside this unit and outside every
    /// unit so far; a button that looks live and is not would be worse than no
    /// button, so it clears the line and the panel states plainly what it does.
    /// </remarks>
    [RelayCommand]
    private void ComposeClear() => Transmit.OwnWords.Message = "";

    /// <summary>True when "Best chance" is the lens in use.</summary>
    [ObservableProperty]
    private bool _isBestChance = true;

    /// <summary>True when "What's new" is the lens in use.</summary>
    [ObservableProperty]
    private bool _isWhatsNew;

    /// <summary>What the active lens is for, on hover.</summary>
    [ObservableProperty]
    private string _lensQuestion = "";

    /// <summary>
    /// What is stopping the radio's scope from reaching the waterfall, or "".
    /// </summary>
    [ObservableProperty]
    private string _scopeNote = "";

    /// <summary>The three family chips, with their counts (HM-DEC-061).</summary>
    public ObservableCollection<FamilyChipViewModel> FamilyChips { get; } = new();

    /// <summary>Which families are switched on.</summary>
    private HashSet<ModeFamily> _families = new(FamilyFilter.All);
    private DateTime _lastSpotLoadUtc = DateTime.UtcNow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalReadinessLine))]
    [NotifyPropertyChangedFor(nameof(HasDigitalReadiness))]
    [NotifyPropertyChangedFor(nameof(DigitalModeChips))]
    private long _frequencyHz;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusOnScreen))]
    [NotifyPropertyChangedFor(nameof(StatusSpeaks))]
    [NotifyPropertyChangedFor(nameof(StatusTip))]
    [NotifyPropertyChangedFor(nameof(HasStatusTip))]
    private string _statusText = "Pick a port and connect";

    /// <summary>
    /// **Whether the status line is something gone wrong, or Hamlet narrating.**
    /// </summary>
    /// <remarks>
    /// <para>**IT DEFAULTS TO SPEAKING, AND THAT DIRECTION IS THE WHOLE DESIGN**
    /// (work instruction 282 task 1). Thirty-one sites write this line, and a
    /// classification that guessed wrong in the quiet direction would hide a fault,
    /// which is §0.0 broken by omission. A site is a fault unless it was deliberately
    /// marked as narration through <see cref="Narrate"/>, so the cost of a
    /// misjudgement is a sentence he did not need rather than one he did — and a new
    /// call site added by a session that never read this file speaks.</para>
    /// <para>**ONE PIECE OF STATE, NOT TWO.** The bar and the mark read the same
    /// string through two properties, so there is no second copy to drift and every
    /// line this application has ever put on the bar is reachable on the hover,
    /// whichever kind it was.</para>
    /// </remarks>
    private bool _statusIsNarration;

    /// <summary>What the bar draws where the line is a narration, usually nothing.</summary>
    /// <remarks>
    /// **A LINE CAN BE MOSTLY NARRATION AND STILL CARRY AN ADMISSION.**
    /// `ReceiverSetupVoice` composes one string out of five kinds of clause, three
    /// of which are Hamlet saying it does not know something. Moving the whole
    /// string to a hover would take those with it, so <see cref="Narrate"/> takes
    /// what must still speak and this is where it goes.
    /// </remarks>
    private string _statusSpoken = "";

    /// <summary>True where the line is something the operator has to act on.</summary>
    public bool StatusSpeaks => StatusOnScreen.Length > 0;

    /// <summary>What the status bar draws, which for narration is nothing.</summary>
    /// <remarks>
    /// **TIM'S RULING, 2026-09-08**: text only where he intentionally hovers, and a
    /// fault is the one exception. Before this the bar carried an 884-character
    /// paragraph on every tune-in to a CW block — measured rather than estimated,
    /// from <see cref="ReceiverSetupVoice.Say"/> over the nine conditions the CW row
    /// of `mode-receiver-conditions.json` states.
    /// </remarks>
    public string StatusOnScreen
        => _statusIsNarration ? _statusSpoken : StatusText;

    /// <summary>The whole line, whichever kind it is, for the mark beside the count.</summary>
    /// <remarks>
    /// **NOTHING IS DELETED** (§0.0, HM-DEC-092). Every line is behind the mark
    /// including the ones that are also on the screen: a hover that sometimes says
    /// nothing teaches somebody not to bother hovering.
    /// </remarks>
    public string StatusTip => StatusText;

    /// <summary>True while there is anything behind the mark.</summary>
    public bool HasStatusTip => StatusText.Length > 0;

    /// <summary>
    /// **Say something that is Hamlet describing itself, not something gone wrong.**
    /// </summary>
    /// <param name="line">The sentence.</param>
    /// <remarks>
    /// <para>It goes behind the mark rather than onto the bar. Use it only where the
    /// line reports something that worked; anything that refused, failed, could not
    /// be read, or wants him to do something stays a plain assignment and
    /// speaks.</para>
    /// <para>**THE FLAG IS SET AFTER THE ASSIGNMENT AND THE NOTIFICATIONS ARE
    /// RE-RAISED**, because the generated setter resets it on the way through. That
    /// is the ordering that keeps the default safe: the reset is automatic and the
    /// exception is deliberate.</para>
    /// </remarks>
    /// <param name="speaks">
    /// The part of <paramref name="line"/> that is an admission and must be read
    /// without hovering, or "" where the whole line is a narration.
    /// </param>
    private void Narrate(string line, string speaks = "")
    {
        StatusText = line;

        _statusIsNarration = true;
        _statusSpoken = speaks;

        OnPropertyChanged(nameof(StatusSpeaks));
        OnPropertyChanged(nameof(StatusOnScreen));
    }

    /// <summary>The same door the tune-in uses, for a test.</summary>
    /// <param name="line">The whole line.</param>
    /// <param name="speaks">The part of it that is an admission, or "".</param>
    /// <remarks>
    /// **THE SAME IDIOM AS <see cref="AddSentRowForTests"/>.** Driving a real
    /// tune-in to prove which of two strings the bar draws would need a rig, a
    /// neighborhood and a settled dial, for a question about two bindings. Nothing
    /// in `src/` calls this.
    /// </remarks>
    internal void NarrateForTests(string line, string speaks)
        => Narrate(line, speaks);

    /// <summary>Every plain assignment is a fault until <see cref="Narrate"/> says otherwise.</summary>
    partial void OnStatusTextChanging(string value)
    {
        _statusIsNarration = false;
        _statusSpoken = "";
    }

    [ObservableProperty]
    private string _modeLineText = "";

    [ObservableProperty]
    private bool _isInsideCwSegment = true;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectButtonText = "Connect";

    [ObservableProperty]
    private string _selectedPort = TrainingRadio;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapSummary))]
    [NotifyPropertyChangedFor(nameof(MapLowHz))]
    [NotifyPropertyChangedFor(nameof(MapHighHz))]
    private BandButtonViewModel _selectedBand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalReadinessLine))]
    [NotifyPropertyChangedFor(nameof(HasDigitalReadiness))]
    [NotifyPropertyChangedFor(nameof(DigitalModeChips))]
    private IReadOnlyList<Neighborhood> _neighborhoods = Array.Empty<Neighborhood>();

    /// <summary>
    /// The lowest frequency the neighborhood map draws.
    /// </summary>
    /// <remarks>
    /// Wider than the band on purpose (HM-DEC-055). The map that stopped exactly
    /// at the band edge showed a wall as the end of the picture, so the operator
    /// who tuned to the very top of 20 m had no way to see that a little further
    /// up there is no amateur spectrum at all. The dial tape and the waterfall
    /// keep the band's own edges, because those are zoomed views of where the
    /// radio is rather than pictures of the whole band.
    /// </remarks>
    public long MapLowHz => SelectedBand.Band.LowHz - NeighborhoodPlan.MarginHz(SelectedBand.Band);

    /// <summary>The highest frequency the neighborhood map draws.</summary>
    public long MapHighHz => SelectedBand.Band.HighHz + NeighborhoodPlan.MarginHz(SelectedBand.Band);

    [ObservableProperty]
    private IReadOnlyList<Controls.ActivityDot> _activityDots =
        Array.Empty<Controls.ActivityDot>();

    /// <summary>
    /// The mode the radio is actually in, or empty when it has not been read.
    /// </summary>
    /// <remarks>
    /// THIS WAS THE LITERAL "CW" IN THE WINDOW until HM-DEC-050. The rig display
    /// showed CW whatever the radio was set to, which meant the screen lied the
    /// moment somebody switched to sideband. Empty is the honest starting point:
    /// nobody has asked yet.
    /// </remarks>
    [ObservableProperty]
    private string _rigModeText = "";

    /// <summary>The filter designator, or empty. It always read FIL2 before.</summary>
    [ObservableProperty]
    private string _rigFilterText = "";

    /// <summary>
    /// Where the S-meter sits, 0 to 1, or null when there is no reading.
    /// </summary>
    /// <remarks>
    /// Null rather than zero, all the way to the control. A needle at rest looks
    /// exactly like a measurement of a quiet band (§0.0).
    /// </remarks>
    [ObservableProperty]
    private double? _sMeterLevel;

    /// <summary>
    /// The filter's width in words, or empty when it is not known.
    /// </summary>
    /// <remarks>
    /// Shown beside the decoder's speed readout, because this is the number that
    /// explains a bad decode: a passband wide open at 3 kHz puts several signals
    /// into the decoder at once.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFilterBandwidth))]
    private string _filterBandwidthText = "";

    /// <summary>
    /// True while the radio is transmitting and the decoder is standing down.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuspendedNote))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _decodingIsSuspended;

    /// <summary>
    /// True while the decoder is refilling a window it emptied to follow
    /// somebody else.
    /// </summary>
    /// <remarks>
    /// Carried as a property of its own, the way the suspended state is, so the
    /// sentence below is reachable from the screen rather than only from the
    /// decoder: the region that shows it is the thing that has gone missing
    /// before, and a test can only prove it by driving this.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FollowedNote))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _listeningAfresh;

    /// <summary>
    /// The one advisory the terminal is showing, by priority.
    /// </summary>
    /// <remarks>
    /// <para>**THE TRANSCRIPT MUST NOT MOVE UNDER HIM.** Below it sat a stack of
    /// boxes that each appeared and vanished on their own, and every one of them
    /// reflowed everything around it. He watches that screen for half an hour at
    /// a time while tuning across a band, and the thing he is reading moved.</para>
    /// <para>**SO THERE IS ONE REGION OF FIXED HEIGHT AND ITS CONTENT SWAPS.**
    /// The messages are genuinely alternatives: several of them were saying
    /// versions of the same thing at the same time, that nothing is being read.
    /// Nothing was removed; what changed is that only the most useful one is on
    /// screen, and the region occupies its space whether or not it has anything
    /// to say.</para>
    /// <para>**THE ORDER IS BY HOW WRONG THE OPERATOR WOULD BE WITHOUT IT.**
    /// Suspension first, because a terminal that has stopped without saying why
    /// reads as a quiet band. Then the two that mean somebody else started
    /// sending, then what Hamlet can see when it is producing nothing, then the
    /// notes about its own limits, and last the ones about the settings.</para>
    /// <para>**TO ADD A MESSAGE**, put it in this list at the place its urgency
    /// earns and give it a tone. Do not add a panel below the transcript, and do
    /// not make an existing one conditional: either of those brings the jump
    /// back.</para>
    /// </remarks>
    public string AdvisoryNote
    {
        get
        {
            foreach (var text in Advisories())
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return "";
        }
    }

    /// <summary>
    /// Whether the keying meter's own block is shown beneath the advisory.
    /// </summary>
    /// <remarks>
    /// <para>**TWO VOICES SAYING DIFFERENT THINGS AT THE SAME TIME IS WORSE THAN
    /// EITHER OF THEM.** On the evening of 2026-08-25 the advisory said a clear
    /// tone was present and the meter's block underneath it said there was no
    /// keying, the two disagreeing about the pitch by fifty hertz — and the
    /// block's advice sends the operator across the room to change a setting on
    /// the radio for what is a decoder problem.</para>
    /// <para>**THE METER IS NOT RETIRED AND MUST NOT BE.** It is the one
    /// instrument that shares nothing with the decoder, and its whole value is
    /// that it can contradict it (HM-DEC-091). On `cw-2026-08-22-012823` it found
    /// the right frequency while the decoder took the wrong one. What is
    /// suppressed is only its block *while the advisory has something to say*,
    /// which is the case where a second, quieter, less reliable voice can only
    /// confuse: agreed with independent measurement six times and contradicted it
    /// eleven, across everything analysed from 2026-08-22 to 2026-08-25.</para>
    /// <para>When the advisory is silent the meter speaks, exactly as before.</para>
    /// </remarks>
    public bool ShowKeyingMeter
        => _settings.ShowKeyingSweep
            && IsDecoding
            && string.IsNullOrWhiteSpace(AdvisoryNote);

    /// <summary>Every advisory the terminal can show, most urgent first.</summary>
    private IEnumerable<string> Advisories()
    {
        yield return SuspendedNote;

        // **SECOND ONLY TO HIM SENDING**, because it is the one condition that
        // stops the band being readable at all and it is one press away from
        // being fixed.
        yield return OverflowAdvice;

        // **AND WHAT ELSE ON THE RECEIVE SIDE IS IN THE WAY**, which sits below
        // an overloading front end because that one stops the band being
        // readable at all while these degrade it. Read-only, and each names the
        // control on the front of the radio rather than stopping at the
        // diagnosis (HM-DEC-148).
        yield return ReceiveObstructionText;

        // **WHAT THE DECODER IS LISTENING TO, WHEN IT IS BEING HELD THERE.** It
        // sits below what is in the way because it is a state the operator chose
        // rather than a fault he needs to fix.
        yield return PitchLockText;

        // **WHY THE SCREEN JUST WENT QUIET.** Following somebody empties the
        // decoder's window, and twelve seconds of nothing with no explanation
        // reads as a dead band at the one moment it certainly is not one.
        yield return FollowedNote;

        yield return CaptureNote;
        yield return DecoderStory;
    }

    /// <summary>What the terminal says while it refills after following somebody.</summary>
    /// <remarks>
    /// The window holds twelve seconds and all of it was listened to at the other
    /// station's pitch, so it is thrown away rather than decoded as a mixture
    /// (HM-DEC-009). The cost is real and is stated rather than hidden.
    /// </remarks>
    public string FollowedNote
        => ListeningAfresh
            ? "somebody else has started sending and Hamlet has moved across to "
              + "them, so it has let go of what it was holding, because those "
              + "twelve seconds were listened to at the other station's pitch and "
              + "reading them now would put one operator's letters in the other's "
              + "mouth. Give it a few seconds to fill up again and the text picks "
              + "up where the new station is."
            : "";

    /// <summary>What the terminal says while the operator is sending.</summary>
    /// <remarks>
    /// **A TERMINAL THAT HAS STOPPED WITHOUT SAYING WHY IS ITS OWN CONFIDENT
    /// WRONG ANSWER.** An empty screen reads as a quiet band, and the one moment
    /// it is guaranteed not to be quiet is while the operator has his hand on the
    /// key.
    /// </remarks>
    public string SuspendedNote
        => DecodingIsSuspended
            ? "You are sending, so Hamlet is listening to you rather than to the "
              + "band. Whatever you key is yours and never appears here as "
              + "somebody else's. It picks the band up again a moment after you "
              + "stop."
            : "";

    /// <summary>The sending speed the decoder is tracking, or 0.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalSpeedText))]
    [NotifyPropertyChangedFor(nameof(HasDetectedSpeed))]
    private int _detectedWpm;

    /// <summary>Whether the decoder is listening to anything.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalIdleText))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _isDecoding;

    /// <summary>What the decoder is listening to, in words.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalIdleText))]
    private string _audioInputName = "";

    [ObservableProperty]
    private string _storyTitle = "";

    [ObservableProperty]
    private string _storyBadge = "";

    [ObservableProperty]
    private string _storyBody = "";

    [ObservableProperty]
    private long _storyTuneHz;

    [ObservableProperty]
    private string _spotsSummary = "loading…";

    [ObservableProperty]
    private FreshnessLevel _spotsFreshness = FreshnessLevel.Fresh;

    /// <summary>The one written suggestion above the list (HM-DEC-025).</summary>
    [ObservableProperty]
    private LeadSuggestion _lead = new(
        false, "Looking…", "Hamlet is asking the spot networks what is on the air.",
        "", 0, "");

    /// <summary>The band-conditions claim and its evidence (HM-DEC-025).</summary>
    [ObservableProperty]
    private ConditionsLine _conditions = new(
        "Checking the bands…", "", ConditionsConfidence.Thin, null);

    /// <summary>
    /// The spectrum the waterfall draws, or null when nothing is receiving.
    /// The control subscribes to it directly; pixels never travel through
    /// binding (HM-DEC-006).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SignalsAreSimulated))]
    [NotifyPropertyChangedFor(nameof(WaterfallSummary))]
    [NotifyPropertyChangedFor(nameof(SpectrumNotice))]
    private ISpectrumSource? _spectrumSource;

    /// <summary>
    /// The Digital tab's own waterfall, an FFT of the received audio.
    /// </summary>
    /// <remarks>
    /// <para>**A SECOND SOURCE, NOT A SHARED ONE** (Tim's ruling of 2026-08-28).
    /// The CW waterfall draws the radio's own scope stream, which is band-wide
    /// RF in kilohertz; FT8 lives in a fifty hertz sliver of a three kilohertz
    /// audio passband, so the two pictures are of different things and neither
    /// substitutes for the other.</para>
    /// <para>**IT RIDES THE SAME AUDIO THE CW DECODER LISTENS ON** and cannot
    /// disturb it: SamplesReady is a multicast event and this consumer never
    /// starts or stops the source.</para>
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigitalWaterfallSummary))]
    [NotifyPropertyChangedFor(nameof(DigitalReadinessLine))]
    [NotifyPropertyChangedFor(nameof(HasDigitalReadiness))]
    private AudioSpectrumSource? _digitalSpectrum;

    /// <summary>How far the PC clock is from UTC, measured and never corrected.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClockOffsetLine))]
    [NotifyPropertyChangedFor(nameof(ClockIsConcerning))]
    [NotifyPropertyChangedFor(nameof(DigitalWaterfallSummary))]
    [NotifyPropertyChangedFor(nameof(DigitalReadinessLine))]
    [NotifyPropertyChangedFor(nameof(HasDigitalReadiness))]
    private ClockOffset _clockOffset = ClockOffset.Unknown;

    /// <summary>What the Digital tab says about the clock.</summary>
    public string ClockOffsetLine => ClockOffset.Describe(DateTime.UtcNow);

    /// <summary>Whether the clock is far enough out to draw in amber.</summary>
    public bool ClockIsConcerning
        => Hamlet.RadioEngine.Audio.ClockOffset.IsConcerning(ClockOffset);

    /// <summary>
    /// The Digital waterfall panel's collapsed summary, reporting what is really
    /// being drawn.
    /// </summary>
    /// <remarks>
    /// **IT STOPPED BEING STATIC IN UNIT 038.** A collapsed panel still carries
    /// its news (HM-DEC-021), and a summary saying the band and the slots while
    /// nothing is arriving is a claim rather than a caption.
    /// </remarks>
    public string DigitalWaterfallSummary
    {
        get
        {
            if (DigitalSpectrum is null)
            {
                return "not listening yet";
            }

            var band = $"{AudioSpectrumSource.LowHz}–"
                + $"{AudioSpectrumSource.HighHz} Hz";

            var grid = ClockOffset.IsKnown
                ? "15 s slots"
                : "no slot grid until the clock is checked";

            return DigitalSpectrum.IsSimulated
                ? $"{band} · {grid} · simulated"
                : $"{band} · {grid}";
        }
    }

    /// <summary>
    /// The first thing standing between the operator and a decode, or "".
    /// </summary>
    /// <remarks>
    /// <para>**IT IS EMPTY AND INVISIBLE WHEN NOTHING IS WRONG**, and that is
    /// asserted by a test. A quiet band is not a fault, the decoded panel's own
    /// idle line already covers it, and a readiness line that always says
    /// something is one the operator stops reading.</para>
    /// <para>**THE ORDER AND THE WORDS ARE `DigitalReadiness`', WHICH IS PURE.**
    /// This assembles the five facts from the surfaces that already hold them:
    /// whether anything is listening, whether the source is the training radio,
    /// the measured clock offset, what the radio says its mode is, and which
    /// neighborhood the dial is in. Every one of them was already on this tab
    /// somewhere and none of them was in one place.</para>
    /// </remarks>
    public string DigitalReadinessLine
        => DigitalReadiness.FirstProblem(
            DigitalSpectrum is not null,
            DigitalSpectrum?.IsSimulated == true,
            ClockOffset,
            RigState.Mode,
            RigState.DataVariant,
            Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz)));

    /// <summary>The mode strip's four chips, with whichever one is lit.</summary>
    /// <remarks>
    /// **IT STOPPED BEING STATIC IN UNIT 228.** FT8 was lit in the markup and
    /// the other three greyed there, from work instruction 037, so the strip
    /// asserted the dial was in FT8 territory wherever the dial was. The
    /// reasoning is `DigitalModeChip`'s, and it is the map that answers.
    /// </remarks>
    public IReadOnlyList<DigitalModeChip> DigitalModeChips
        => DigitalModeChip.For(
            Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz)),
            ChosenDigitalMode);

    /// <summary>What the last sub-mode press did, or "" when there is nothing to say.</summary>
    /// <remarks>
    /// **IT IS ABOUT THE PRESS AND NOT ABOUT THE BAND**, which is what separates
    /// it from the readiness strip above the panels and from
    /// <see cref="DigitalModeStripLine"/> beside it. Those describe a standing
    /// state; this describes one thing the operator just did, and it is empty
    /// until he does it.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDigitalTuneLine))]
    private string _digitalTuneLine = "";

    /// <summary>True while the last press has something to report.</summary>
    public bool HasDigitalTuneLine => DigitalTuneLine.Length > 0;

    /// <summary>
    /// True where the last press did not put the radio where it was asked to.
    /// </summary>
    /// <remarks>
    /// **DRAWN DIFFERENTLY, BECAUSE IT IS A DIFFERENT KIND OF FACT.** *The dial
    /// is now at 14.074* and *the dial did not move and here is why* must not
    /// look the same, or the second gets read as the first at a glance.
    /// </remarks>
    [ObservableProperty]
    private bool _digitalTuneFailed;

    /// <summary>
    /// Pick a digital sub-mode from the strip, and go there.
    /// </summary>
    /// <param name="label">FT8, FT4, PSK31 or WSPR.</param>
    /// <remarks>
    /// <para>**IMMEDIATELY, WITH NO CONFIRMATION** (Tim, 2026-09-05: *"Five click
    /// FT8. That means I want to go to FT8. Tune immediately."*). There is no
    /// dialog and no second press.</para>
    /// <para>**THE DISPLAY MOVES ON THE READ-BACK AND NEVER ON THE COMMAND**
    /// (§0.0). Set over CI-V, read over CI-V 03, and show what came back. A
    /// frequency Hamlet commanded is a request; a frequency the radio reported is
    /// a measurement, and only one of those belongs on a display the whole
    /// application trusts. If the read-back disagrees or does not arrive, the
    /// display keeps the frequency it had and says the tune did not take.</para>
    /// <para>**THE FREQUENCY COMES OUT OF THE TREE**
    /// (<see cref="DigitalCallingFrequencies"/>), from the same cited rows the
    /// Neighborhood map draws `FT8 city` from. Nothing here holds a number.</para>
    /// <para>**A BAND WITH NO ROW FOR THAT MODE MOVES NOTHING.** It says so, and
    /// says which bands do have one. Guessing a frequency or hopping to another
    /// band would both be Hamlet deciding where the operator meant to be.</para>
    /// <para>**§0.2 IS UNTOUCHED.** This is a receive-frequency change and keys
    /// nothing.</para>
    /// </remarks>
    [RelayCommand]
    private async Task ChooseDigitalModeAsync(string? label)
    {
        var picked = DigitalModeChip.Canonical(label);

        if (picked is null)
        {
            return;
        }

        // **THE CHOICE IS RECORDED WHETHER OR NOT THE TUNE TAKES.** He asked for
        // FT8; that is true even on a band with no FT8 in it, and the strip draws
        // the chip as chosen-with-the-dial-elsewhere rather than forgetting he
        // pressed it.
        ChosenDigitalMode = picked;

        await TuneToDigitalModeAsync(picked).ConfigureAwait(true);
    }

    /// <summary>Take the dial to a sub-mode's block, and confirm it landed.</summary>
    /// <param name="picked">One of the four, already canonical.</param>
    private async Task TuneToDigitalModeAsync(string picked)
    {
        var bandName = SelectedBand.Band.Name;
        var block = DigitalCallingFrequencies.Find(bandName, picked);

        if (block is null)
        {
            // **NOTHING MOVES AND THE SCREEN SAYS WHY.** As of 2026-09-05 the
            // cited data has no WSPR row on any band and no FT4 on 30 m or 17 m.
            // A number invented to fill the gap would be the one thing §0.2.1
            // names outright.
            var elsewhere = DigitalCallingFrequencies.BandsWith(picked);

            DigitalTuneFailed = true;
            DigitalTuneLine = elsewhere.Count == 0
                ? $"Hamlet has no {picked} frequency for any band, so the dial "
                  + "has not moved. Nothing here is written from memory, and "
                  + $"there is no cited row for {picked} in the band data."
                : $"There is no {picked} on {bandName}, so the dial has not "
                  + $"moved. {Listed(elsewhere)} {(elsewhere.Count == 1 ? "has" : "have")} one.";

            return;
        }

        var target = block.JumpHz;

        if (_rig is null || !IsConnected)
        {
            DigitalTuneFailed = true;
            DigitalTuneLine =
                $"Nothing is connected, so the dial has not moved. {picked} on "
                + $"{bandName} is {Megahertz(target)} MHz when a radio is.";

            return;
        }

        // **THE SAME STAMP THE ORDINARY SEND TICK SETS.** A frequency report that
        // crossed this write on the wire describes where the dial was, and the
        // display must not follow it back (`DialGuard`).
        var was = FrequencyHz;
        NoteTuneWritten(target, was, DateTime.UtcNow);

        AppEvents.TuneRequested(_telemetry, target, "digital_mode_press");

        long readBack;

        try
        {
            await _rig.SetFrequencyHzAsync(target).ConfigureAwait(true);

            // **READ, RATHER THAN WAIT FOR THE BROADCAST.** The radio volunteers
            // frequency changes and that path still works, but a press has to be
            // able to say *this did not take*, and silence is indistinguishable
            // from a broadcast that has not arrived yet. Asking gets an answer or
            // an exception, and both are results.
            readBack = await _rig.GetFrequencyHzAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppEvents.TuneWritten(_telemetry, target, "failed", null);

            DigitalTuneFailed = true;
            DigitalTuneLine =
                $"The tune to {picked} did not take: {ex.Message}. The dial is "
                + $"still showing {Megahertz(was)} MHz.";

            return;
        }

        if (readBack != target)
        {
            // **THE DISPLAY STAYS WHERE IT WAS.** Showing the target would assert
            // a frequency the radio did not accept; showing the read-back would
            // be right about the radio and would silently swallow the fact that
            // the press did something other than what it said. So the display
            // does not move and the line names both numbers.
            AppEvents.TuneWritten(_telemetry, target, "unconfirmed", null);

            DigitalTuneFailed = true;
            DigitalTuneLine =
                $"The tune to {picked} did not take. Hamlet asked for "
                + $"{Megahertz(target)} MHz and the radio came back with "
                + $"{Megahertz(readBack)} MHz, so the display is left where it "
                + "was.";

            return;
        }

        AppEvents.TuneWritten(_telemetry, target, "confirmed", null);

        // **THE READ-BACK IS WHAT GOES ON SCREEN**, applied through the same door
        // a report from the radio uses, so no second write goes out behind it.
        ApplyRigFrequency(readBack);

        DigitalTuneFailed = false;
        DigitalTuneLine =
            $"{picked} on {bandName} — the radio confirmed {Megahertz(readBack)} MHz.";
    }

    /// <summary>Hertz as megahertz, to the same six places the rig display uses.</summary>
    private static string Megahertz(long hz)
        => (hz / 1_000_000.0).ToString("0.000000", CultureInfo.InvariantCulture);

    /// <summary>A list of band names as English rather than as a comma run.</summary>
    private static string Listed(IReadOnlyList<string> names)
        => names.Count switch
        {
            0 => "",
            1 => names[0],
            2 => names[0] + " and " + names[1],
            _ => string.Join(", ", names.Take(names.Count - 1))
                 + " and " + names[^1],
        };

    /// <summary>Whether the readiness line has anything to say.</summary>

    /// <summary>Whether the readiness line has anything to say.</summary>
    /// <remarks>
    /// **THE MARKUP HIDES THE WHOLE ROW RATHER THAN SHOWING AN EMPTY ONE.** A
    /// blank strip where a warning sometimes appears is a gap the operator
    /// learns to read past, and the space it holds is space the waterfall wants.
    /// </remarks>
    public bool HasDigitalReadiness => DigitalReadinessLine.Length > 0;

    /// <summary>
    /// What the Digital tab's decoded table is showing.
    /// </summary>
    /// <remarks>
    /// <para>**REAL SINCE UNIT 224, AND EMPTY UNTIL SOMETHING DECODES.** Work
    /// instruction 037 put four literal rows in the markup so the operator could
    /// argue with a finished-looking session before there was a decoder; they came
    /// out in the same change that put this in, because a table showing both is
    /// worse than a table showing neither.</para>
    /// <para>**NOTHING ARRIVES HERE THAT DID NOT PASS ITS OWN CHECKSUM.** The
    /// library returns a message only after the codeword's CRC has been verified,
    /// so a line on this table is a line somebody sent (§0.0).</para>
    /// </remarks>
    public ObservableCollection<DigitalDecodeRow> DigitalDecodes { get; } = new();

    /// <summary>The rows the operator's toggles actually asked for.</summary>
    /// <remarks>
    /// <para>**THIS IS WHAT THE TABLE BINDS TO SINCE UNIT 252** (Tim's ruling,
    /// 2026-09-06: *what is not selected is not on the list — not dimmed, not
    /// faded, gone*). <see cref="DigitalDecodes"/> stays the whole table, because
    /// the row cap, the duplicate keys and the arrival order all count what was
    /// heard rather than what is being shown, and a filter must not change any of
    /// them.</para>
    /// <para>**THE SPLIT IS ALSO WHAT KEEPS THE VIEW STILL.**
    /// `FollowingScroll` watches the collection it is bound to, so a row the
    /// toggles do not want raises no change here at all and there is nothing for
    /// the view to follow. The reflow unit 251 was right to worry about is
    /// answered by the view not moving rather than by the rows staying.</para>
    /// <para>**AND IT IS SYNCED IN PLACE RATHER THAN REBUILT PER SLOT.** A clear
    /// and refill every fifteen seconds would raise a reset, and a reset is what
    /// sends a list back to the top under the operator's eyes. Only an explicit
    /// action he took — a toggle, the order button, clear — rebuilds it.</para>
    /// </remarks>
    public ObservableCollection<DigitalDecodeRow> DigitalVisibleDecodes { get; } = new();

    /// <summary>Everything addressed to the operator, always, with no filter.</summary>
    /// <remarks>
    /// <para>**THE RIGHT-HAND SIDE OF THE SPLIT** (Tim's ruling, 2026-09-07:
    /// *that way I am always sure that I am seeing what is for me*). With his own
    /// traffic finally worth watching for, `mine` and `CQ` were competing for one
    /// list, so choosing either lost sight of the other and a message addressed to
    /// him landed among fifty that were not.</para>
    /// <para>**IT HAS NO TOGGLE AND CANNOT BE FILTERED**, which is the whole point
    /// of it: a control that can hide this is a control that can bury the one
    /// thing he is waiting for.</para>
    /// <para>**AND A MESSAGE IS ON EXACTLY ONE SIDE.** Anything here is absent
    /// from <see cref="DigitalVisibleDecodes"/>, so the two counts add up to what
    /// was heard rather than overlapping.</para>
    /// </remarks>
    public ObservableCollection<DigitalDecodeRow> DigitalMineDecodes { get; } = new();

    /// <summary>Every row in the order the decoder produced it.</summary>
    /// <remarks>
    /// <para>**THE DISPLAY ORDER IS A VIEW OF THIS AND NEVER THE RECORD OF
    /// IT.** `DigitalDecodes` is whichever way round the operator asked for;
    /// this is what actually happened, and it is what the direction is applied
    /// to when he changes his mind. Deriving the display from arrival order
    /// means flipping the toggle twice returns exactly the rows that were
    /// there, which reversing the display in place would not.</para>
    /// <para>**IT IS ALSO WHAT THE TRIM READS.** The oldest row is the oldest
    /// arrival wherever it currently sits on screen, and dropping "the first
    /// one in the collection" would drop the newest under a newest-first
    /// ordering.</para>
    /// </remarks>
    private readonly List<DigitalDecodeRow> _digitalArrivals = new();

    /// <summary>What has passed with each station heard on the Digital tab.</summary>
    /// <remarks>
    /// **OPENED WHEN THE FIRST ROW ARRIVES AND NOT IN THE CONSTRUCTOR**, because
    /// a ledger is kept from somebody's station and the operator's callsign may
    /// be empty until he has typed one into Settings.
    /// </remarks>
    private Ft8ContactLedger? _contacts;

    /// <summary>Whose callsign <see cref="_contacts"/> was opened for.</summary>
    private string _contactsFor = "";

    /// <summary>
    /// How many rows the table keeps before the oldest fall off.
    /// </summary>
    /// <remarks>
    /// <para>**A NIGHT AT 14.074 IS 5760 SLOTS AND AN UNBOUNDED COLLECTION BOUND
    /// TO AN `ItemsControl` IS A MEMORY LEAK WITH A SCROLLBAR** (unit 225).</para>
    /// <para>**FIVE HUNDRED, AND THE NUMBER IS THE MARKUP'S RATHER THAN
    /// MEMORY'S.** At a handful of messages a slot, five hundred rows is roughly
    /// the last half hour of a busy band, which is further back than an operator
    /// scrolls. The binding is a plain `ItemsControl` inside a `ScrollViewer` and
    /// does not virtualise, so every row is five live `TextBlock`s whether or not
    /// it is on screen — the cost that bites first is layout, not bytes, and a
    /// bound chosen for bytes would be several thousand and would make the panel
    /// crawl.
    /// </para>
    /// </remarks>
    /// <remarks>
    /// <para>**FIVE HUNDRED, MEASURED AGAINST THE RATE RATHER THAN LEFT AT A
    /// ROUND NUMBER** (unit 241 tasks 1.3 and 6). The shack machine's first FT8
    /// slot, 2026-09-04 at 21:41 UTC, held fourteen messages. At four slots a
    /// minute that is 3,360 rows an hour and 16,800 across a five-hour evening,
    /// so this cap fills in about nine minutes and then drops a row for every
    /// row that arrives, all evening - 33.6 times over.</para>
    /// <para>**IT IS NOT RAISED, AND THE REASON IS THE ROWS RATHER THAN THE
    /// BYTES.** The list does not virtualise, so every retained row is a live
    /// grid of controls whether or not it is on screen, and task 5 made a row
    /// more expensive rather than less - the message cell is now five text
    /// blocks instead of one. Nine minutes of the busiest band anybody has
    /// recorded here is a reasonable amount of scrollback for a panel that also
    /// has a clear button.</para>
    /// <para>**WHAT WAS ACTUALLY WRONG WAS THE SILENCE.** The cap and its trim
    /// were already here; the operator was never told either existed. A list
    /// that discards rows he believes are still there is §0.0's fault whatever
    /// the number is, so the summary says how many have gone.</para>
    /// </remarks>
    internal const int MaxDigitalDecodes = 500;

    /// <summary>
    /// How far the dial may move before the table is cleared.
    /// </summary>
    /// <remarks>
    /// <para>**THE TABLE MAY NOT PUT TWO PLACES UNDER ONE HEADING** (§0.0.1).
    /// Rows decoded on 7.074 sitting above rows decoded on 14.074 is a picture
    /// asserting that those stations were all heard here, and the operator acting
    /// on it would be wrong for a reason the screen gave him.</para>
    /// <para>**THREE KILOHERTZ, WHICH IS THE RECEIVER'S OWN AUDIO PASSBAND.**
    /// Inside it the same transmissions are still arriving through the same
    /// filter, so a nudge of a few hundred hertz has not changed what the rows
    /// describe and clearing on it would throw away a session every time the rig
    /// reported a slightly different dial reading. Outside it the rows are about
    /// a different piece of spectrum. **Clearing is the cheapest honest answer**
    /// and it is what this does.</para>
    /// </remarks>
    internal const long DigitalRetuneClearsBeyondHz = 3000;

    /// <summary>What is already on the table, so nothing is shown twice.</summary>
    /// <remarks>
    /// **A TRANSMISSION APPEARS ONCE, WHATEVER ROUTE IT ARRIVED BY** (§0.0). The
    /// running watch and the capture press can both reach the same slot — a press
    /// keeps the last thirty seconds, which is two whole slots the watch has
    /// usually already read — and a table showing a message twice says two
    /// stations sent it.
    /// </remarks>
    private readonly HashSet<string> _digitalDecodeKeys = new(StringComparer.Ordinal);

    /// <summary>The same keys in row order, so the oldest can be dropped.</summary>
    private readonly List<string> _digitalDecodeKeyOrder = new();

    /// <summary>Where the dial was when the rows on the table were decoded.</summary>
    private long _digitalRowsTunedAtHz;

    /// <summary>Why the running watch is not producing slots, or "".</summary>
    /// <remarks>
    /// **IT OUTRANKS THE ROW COUNT ON THE PANEL SUMMARY**, because the state it
    /// describes is the one where a table full of old rows is most misleading: the
    /// clock has gone unmeasured or the audio has stopped, nothing new can arrive,
    /// and a summary still reporting yesterday's rows reads as a working session.
    /// </remarks>
    private string _digitalRefusal = "";

    /// <summary>Whether the decoded table has anything to show.</summary>
    /// <remarks>
    /// **WHEN IT IS FALSE THE PANEL SHOWS ITS OWN IDLE LINE** (HM-DEC-021). An
    /// empty panel is indistinguishable from a broken one, and one message for the
    /// whole tab is lost the moment a panel is collapsed.
    /// </remarks>
    public bool HasDigitalDecodes => DigitalDecodes.Count > 0;

    /// <summary>Whether the newest slot is shown at the top.</summary>
    /// <remarks>
    /// **IT OPENS NEWEST-FIRST** (Tim, 2026-09-04), and persists beside the
    /// panel's expand state.
    /// </remarks>
    public bool DigitalNewestFirst
    {
        get => _digitalNewestFirst;
        set
        {
            if (_digitalNewestFirst == value)
            {
                return;
            }

            _digitalNewestFirst = value;
            _settings.DecodedNewestFirst = value;
            SettingsStore.Save(_settings);

            Reorder();

            OnPropertyChanged();
            OnPropertyChanged(nameof(DigitalOrderLabel));
            OnPropertyChanged(nameof(DigitalDecodedSummary));
        }
    }

    private bool _digitalNewestFirst = true;

    private bool _digitalShowCq;

    /// <summary>Whether the table is showing calls to anyone.</summary>
    /// <remarks>
    /// **AN INDEPENDENT TOGGLE SINCE UNIT 252** (Tim's ruling, 2026-09-06),
    /// superseding unit 251's three exclusive buttons. Persisted beside the sort
    /// direction and the panel's expand state.
    /// </remarks>
    public bool ShowsCqOnly
    {
        get => _digitalShowCq;
        set
        {
            if (_digitalShowCq == value)
            {
                return;
            }

            _digitalShowCq = value;
            _settings.DecodedShowCq = value;
            SettingsStore.Save(_settings);

            ApplyDecodedFilter();

            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowsEverything));
        }
    }

    /// <summary>True while nothing is filtered.</summary>
    /// <remarks>
    /// **`everything` IS A STATE AND NOT A THIRD CHOICE** (Tim's ruling,
    /// 2026-09-06). It is what neither toggle being on *is*, so it cannot
    /// disagree with them, and the button that appears to select it clears both.
    /// </remarks>
    public bool ShowsEverything => !ShowsCqOnly;

    /// <summary>What the active toggles are called, for the summary line.</summary>
    /// <remarks>
    /// **BOTH NAMES WHEN BOTH ARE ON**, because the operator has to be able to
    /// read back off the summary which of the two is holding rows away — a line
    /// saying rows were held back *by the filter* names nothing he can turn off.
    /// </remarks>
    private string FilterLabel()
    {
        return ShowsCqOnly ? "CQ" : "everything";
    }

    /// <summary>Turn the calls-to-anyone toggle over.</summary>
    [RelayCommand]
    private void ToggleDecodedCq() => ShowsCqOnly = !ShowsCqOnly;

    /// <summary>Stop filtering, by clearing both toggles.</summary>
    /// <remarks>
    /// **PRESSING IT WHILE IT IS ALREADY THE STATE DOES NOTHING**, rather than
    /// being disabled. A control that goes grey when the panel is doing the
    /// ordinary thing teaches the operator that this strip greys out, and grey is
    /// reserved for what genuinely cannot be used (§0.5.1, HM-DEC-087).
    /// </remarks>
    [RelayCommand]
    private void ShowEveryDecode() => ShowsCqOnly = false;

    /// <summary>How many rows are on the left-hand list.</summary>
    public int DigitalShownCount { get; private set; }

    /// <summary>How many messages are addressed to the operator.</summary>
    public int DigitalMineCount { get; private set; }

    /// <summary>True while anything at all has been addressed to him.</summary>
    public bool HasDigitalMineDecodes => DigitalMineCount > 0;

    /// <summary>What the mine side says when nobody has called him.</summary>
    /// <remarks>
    /// <para>**EMPTY, WITH A LINE SAYING SO, AND NEVER BLANK** (Tim's ruling,
    /// 2026-09-07). A blank column says nothing about whether it is working, and
    /// this is the column he will be watching hardest — on the evening it stays
    /// empty he needs to know that is the band and not the panel.</para>
    /// <para>**IT NEVER SHOWS HIS OWN TRANSMISSIONS.** He ruled the side is for
    /// what is addressed to him, and a sent message is not a decode: putting it in
    /// a decoded list would be the panel showing him his own words back as though
    /// somebody had sent them (§0.0).</para>
    /// <para>**AND WHERE HAMLET DOES NOT KNOW HIS CALLSIGN IT SAYS THAT
    /// INSTEAD**, because nothing can be addressed to a callsign the app has never
    /// been told, and *nobody has called you* would be a claim about the band when
    /// the truth is a gap in Settings.</para>
    /// </remarks>
    public string DigitalMineIdle
        => string.IsNullOrWhiteSpace(_settings.Operator.Callsign)
            ? "Hamlet does not know your callsign yet, so it cannot tell which "
              + "messages are for you. Put it in Settings, under Operator, and "
              + "anything addressed to you will appear here."
            : "Nothing addressed to you yet. Anything a station sends to your "
              + "callsign lands here, and it stays out of the list on the left so "
              + "it can never be buried.";

    /// <summary>The mine panel's own summary, in its header.</summary>
    /// <remarks>
    /// **IT SAYS WHEN IT IS EMPTY RATHER THAN SAYING NOTHING** (HM-DEC-021, and
    /// Tim's ruling of 2026-09-07). A collapsed panel still carries its summary,
    /// and a shut panel that goes silent on the one column he is waiting on is
    /// §0.0 broken by omission.
    /// </remarks>
    /// <remarks>
    /// **IT COUNTS EVERY MESSAGE ADDRESSED TO HIM AND NOT THE ROWS ON SHOW**
    /// (§0.0, HM-DEC-092). The panel draws one conversation, so counting what is
    /// drawn would say *1 for you* on an evening when two stations were calling,
    /// and a collapsed panel reading that would be a false picture of how busy he
    /// is. The others are on the waiting strip with their own count; this number
    /// is the total, which is what a summary is for.
    /// </remarks>
    public string DigitalMineSummary
        => _mineAll.Count == 0
            ? "nothing for you yet"
            : _mineAll.Count == 1
                ? "1 for you"
                : $"{_mineAll.Count} for you";

    /// <summary>How many rows the toggles are holding off the list.</summary>
    /// <remarks>
    /// **NEVER OMITTED, AND IT IS THE WHOLE DEFENCE AGAINST §0.0 HERE** (Tim's
    /// ruling, 2026-09-06). A filter that removes can make a busy band look like a
    /// quiet one, which is a false picture and binds as hard as a false sentence
    /// (HM-DEC-092). The count of what is held back is what stops it, so the
    /// summary carries it whenever it is not zero, expanded or collapsed.
    /// </remarks>
    public int DigitalHiddenCount { get; private set; }

    /// <summary>Whether a row is addressed to the operator.</summary>
    /// <param name="row">The row.</param>
    /// <returns>True when it belongs on the right-hand list.</returns>
    /// <remarks>
    /// **THE SAME QUESTION THE CONTACT COLUMN ASKS**, and asked of the one place
    /// that answers it — `Ft8MessageSplit.IsAddressedTo` — rather than restated
    /// here (§0). Two copies of *is this for him* would disagree on the screen: a
    /// row on this side with a blank contact column, or a contact state beside a
    /// row he cannot find.
    /// </remarks>
    private bool IsForHim(DigitalDecodeRow row)
        => Ft8MessageSplit.IsAddressedTo(row.Message, _settings.Operator.Callsign);

    /// <summary>Whether a row belongs on the left-hand list.</summary>
    /// <param name="row">The row.</param>
    /// <returns>True when the left list should carry it.</returns>
    /// <remarks>
    /// <para>**A MESSAGE IS ON EXACTLY ONE SIDE** (Tim's ruling, 2026-09-07).
    /// Anything addressed to him is on the right and **not also here**: two copies
    /// of one message is how a table starts disagreeing with itself, and each
    /// summary would then be counting something that overlaps the other.</para>
    /// <para>**`mine` IS NO LONGER A TOGGLE**, so this side reads one flag:
    /// everything, or calls to anyone. `DecodedFilterRule.Wants` is still the rule
    /// for that half and is called with its `mine` argument false, because what
    /// that argument used to select is now a side of the panel rather than a
    /// filter.</para>
    /// </remarks>
    private bool WantsRow(DigitalDecodeRow row)
        => !IsForHim(row) && DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee);

    /// <summary>Keep the visible table in step with the whole one.</summary>
    /// <param name="sender">The whole table.</param>
    /// <param name="e">What happened to it.</param>
    /// <remarks>
    /// <para>**INCREMENTAL FOR AN ARRIVAL, WHOLESALE FOR A RESET.** A slot's rows
    /// arrive one at a time, and each one that the toggles want is inserted where
    /// it belongs; a filtered-out one produces no change on the visible collection
    /// at all, which is what leaves the scroll position untouched. A reset is the
    /// order toggle or a clear, both of which the operator asked for, so rebuilding
    /// there costs nothing he did not choose.</para>
    /// <para>**THE INSERT POSITION IS COUNTED FROM THE WHOLE TABLE.** The visible
    /// table is the whole table with rows taken out, so a row's place in it is the
    /// number of wanted rows sitting before it — which is what keeps the two in
    /// the same order under newest-first, where a row goes neither at the top nor
    /// at the bottom but after the rows already in its own slot.</para>
    /// </remarks>
    private void OnDigitalDecodesChanged(
        object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_reordering)
        {
            return;
        }

        if (e.Action is NotifyCollectionChangedAction.Reset
            or NotifyCollectionChangedAction.Move
            or NotifyCollectionChangedAction.Replace)
        {
            ApplyDecodedFilter();
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (DigitalDecodeRow row in e.OldItems)
            {
                DigitalVisibleDecodes.Remove(row);

                // **OUT OF THE MASTER AS WELL AS OFF THE PANEL.** The trim drops
                // the oldest rows, and one left behind here would keep a station
                // on the waiting list with nothing to show for it.
                if (_mineAll.Remove(row))
                {
                    DigitalMineDecodes.Remove(row);
                }
            }
        }

        var rebuild = false;

        if (e.NewItems is not null)
        {
            var at = e.NewStartingIndex;

            foreach (DigitalDecodeRow row in e.NewItems)
            {
                // **EXACTLY ONE SIDE**, which is why these are two arms of one
                // decision rather than two independent tests. Written as two
                // tests, a message addressed to him under an `everything` left
                // list would land on both.
                if (IsForHim(row))
                {
                    // **COLLECTED, THEN THE CONVERSATION IS REBUILT.** Which rows
                    // are on the panel now depends on which station he is
                    // following and on what came immediately before each row, so
                    // there is no position to compute for a row on its own.
                    _mineAll.Add(row);
                    rebuild = true;
                }
                else if (WantsRow(row))
                {
                    DigitalVisibleDecodes.Insert(SideIndexOf(at, WantsRow), row);
                }

                at++;
            }
        }

        if (rebuild)
        {
            RebuildConversation();
        }

        RecountDecodedFilter();
    }

    /// <summary>The slot his own transmission is going out in, or null.</summary>
    /// <remarks>
    /// <para>**IT IS A READING AND NOTHING ACTS ON IT** (§0.2). Nothing arms on it,
    /// nothing cancels on it, and its running out sends nothing: it is set where the
    /// boundary hands the transmission over and cleared where the run comes back,
    /// and the only line that reads it is a sentence on a panel.</para>
    /// <para>**WRITTEN OFF THE UI THREAD AND READ ON IT.** `AtSlotBoundaryAsync`
    /// runs on the pool; a `DateTime?` is not torn, and the tick that reads it four
    /// times a second will see the write on its next pass. A slot late is a quarter
    /// of a second on a fifteen-second beat.</para>
    /// </remarks>
    private DateTime? _sendingSlotUtc;

    /// <summary>True where that transmission was stopped partway.</summary>
    /// <remarks>
    /// **STOPPED IS NOT THE SAME AS FINISHED.** Part of a message went out and the
    /// rest did not, so a line saying the slot ran its course would hide that from
    /// him at the one moment it matters.
    /// </remarks>
    private bool _sendWasStopped;

    /// <summary>Every slot Hamlet actually transmitted in this session.</summary>
    /// <remarks>
    /// <para>**A SLOT HE USED WAS NEVER SEARCHED** (HM-DEC-147). Hamlet suspends
    /// decoding while the radio is transmitting, so a slot he keyed produces no
    /// decodes **by design** - and the census was describing that deliberate
    /// suspension as *the search found no place in it that looked like the start
    /// of an FT8 transmission*, which is a claim about the band that Hamlet never
    /// made a measurement to support (§0.0).</para>
    /// <para>**NOTHING NEEDED MEASURING TO FIX IT.** The transmission is already
    /// in `ft8_transmission` telemetry with its `slotStartUtc`; this is the same
    /// fact kept where the census can read it.</para>
    /// <para>**IT IS A READING AND NOTHING ACTS ON IT.** Nothing arms, cancels or
    /// decodes differently because a slot is in here; one line of prose changes.
    /// </para>
    /// </remarks>
    private readonly HashSet<DateTime> _transmittedSlots = new();

    /// <summary>The beat, as the panel last read it.</summary>
    private Ft8Turn _turn = new(Ft8TurnState.NoClock, null, null);

    /// <summary>**Whose slot this is and how much of it is left**, in one sentence.</summary>
    /// <remarks>
    /// **IT SAYS AND IT DOES NOT ACT** (§0.2). Nothing reads this to decide
    /// anything; it is text on a panel. The countdown reaching its last second
    /// arms nothing, queues nothing and sends nothing, because there is no line
    /// anywhere that watches it.
    /// </remarks>
    public string DigitalTurnLine => _turn.Line();

    /// <summary>How far round the ring is still drawn, in degrees.</summary>
    /// <remarks>
    /// <para>**THE RING DRAINS AND DOES NOTHING ELSE** (§0.2). Nothing reads it,
    /// nothing arms on it, and reaching zero sends nothing: a countdown that fires
    /// is automatic sequencing wearing a clock's face, and this phase forbids it
    /// outright.</para>
    /// <para>**TWO LENGTHS SHARE ONE SHAPE, AND THE RING IS DRAWN AS A FRACTION**
    /// rather than as seconds, which is what lets them. A slot is 15 s and a
    /// transmission is 12.64 s, so a full ring means *all of whatever this is*
    /// either way. **What says which it is, is not the ring**: the on-air state is
    /// drawn thicker and captioned with the message going out, so nobody has to
    /// infer the length from the arc.</para>
    /// </remarks>
    public double TurnRingSweep
    {
        get
        {
            if (_turn.SecondsLeft is not { } left)
            {
                return 0;
            }

            var whole = _turn.State == Ft8TurnState.Transmitting
                ? Ft8Slots.TransmissionSeconds
                : Ft8Slots.SlotSeconds;

            return Math.Clamp(left / whole, 0, 1) * 360.0;
        }
    }

    /// <summary>The number inside the ring, or a question mark.</summary>
    /// <remarks>
    /// **A QUESTION MARK IS NOT A NUMBER AND MUST NOT LOOK LIKE ONE** (§0.0).
    /// Before a station has transmitted there is no parity to derive, so there is
    /// no countdown to show either, and a zero there would be a reading nobody
    /// took.
    /// </remarks>
    public string TurnRingCount
        => _turn.State == Ft8TurnState.NoClock
            || _turn.State == Ft8TurnState.NoStationYet
            || _turn.SecondsLeft is null
                ? "?"
                : _turn.SecondsLeft.Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>What is beside the ring, which for four of six states is nothing.</summary>
    /// <remarks>
    /// <para>**TEXT ONLY WHERE HE HOVERS** (Tim's ruling, 2026-09-08, work
    /// instruction 281 task 3). *click a reply*, *yours is next* and *nothing heard
    /// yet* are gone from the screen and are in <see cref="TurnRingTip"/>. The ring
    /// carries those states on its own.</para>
    /// <para>**TWO OF THE SIX STAY, AND BOTH ARE FAULTS.** A clock that has not been
    /// measured and a transmission that stopped partway are things gone wrong, and a
    /// fault speaks unasked — the single exception to the ruling. Hiding either
    /// behind a hover would be §0.0 broken by omission.</para>
    /// <para>**THE ON-AIR LINE IS NOT A CAPTION.** It is the message going out,
    /// which is a fact and the most useful thing on the screen while his carrier is
    /// up.</para>
    /// <para>**AND REMOVING THE WORDS MOVED THE GRAYSCALE CARRIER RATHER THAN
    /// DROPPING IT** (§0.6). His slot and theirs used to separate by hue and by
    /// caption; with the caption gone the hue would have been alone, so the two are
    /// now drawn at different stroke thicknesses. Colour is still never the only
    /// carrier — it is now never a carrier at all.</para>
    /// </remarks>
    public string TurnRingCaption
        => _turn.State switch
        {
            Ft8TurnState.Transmitting => _armedText.Length > 0 ? _armedText : "on air",
            Ft8TurnState.Stopped => "stopped partway",
            Ft8TurnState.NoClock => "clock not measured",
            _ => string.Empty,
        };

    /// <summary>The whole sentence for whichever state the ring is in.</summary>
    /// <remarks>
    /// **NOTHING WAS DELETED, IT MOVED** (§0.0, HM-DEC-092). Every state answers,
    /// including the three that still show words, because a hover that sometimes
    /// says nothing teaches somebody not to bother hovering.
    /// </remarks>
    public string TurnRingTip
        => _turn.State switch
        {
            Ft8TurnState.Mine =>
                "This slot is yours and nothing is going out in it. Right-click a "
                + "decoded message to answer whoever sent it, or press CQ.",
            Ft8TurnState.Theirs =>
                "The station you are working has this slot, so the next one is "
                + "yours. The number is how many seconds are left of theirs.",
            Ft8TurnState.Transmitting =>
                "Your carrier is up and this is what is going out. The number is "
                + "how much of the transmission is left.",
            Ft8TurnState.Stopped =>
                "You stopped a transmission partway through, so what went out was "
                + "not a whole message and nobody will decode it.",
            Ft8TurnState.NoClock =>
                "Hamlet has not measured this computer's clock against the slot "
                + "boundaries yet, so it does not know where a slot starts and "
                + "cannot work out whose turn it is.",
            _ =>
                "Nothing has been heard on this frequency yet, so there is nobody "
                + "to take a turn with and no turn to work out.",
        };

    /// <summary>True where the ring is drawn dashed and empty.</summary>
    /// <remarks>
    /// **THE UNKNOWN STATES KEEP THEIR DASHES AND THEIR QUESTION MARK** and never
    /// pick a side (unit 277's rule). The dash pattern is what separates them in
    /// grayscale from the two that are known.
    /// </remarks>
    public bool TurnRingIsUnknown
        => _turn.State is Ft8TurnState.NoClock or Ft8TurnState.NoStationYet;

    /// <summary>True while his own carrier is up.</summary>
    /// <remarks>
    /// **DRAWN THICKER, WHICH IS THE GRAYSCALE CARRIER FOR THIS STATE.** It has its
    /// own colour, neither the accent nor the muted one, and a colour alone would
    /// not survive being printed.
    /// </remarks>
    public bool TurnRingIsOnAir => _turn.State == Ft8TurnState.Transmitting;

    /// <summary>True where the slot running is his own and free.</summary>
    public bool TurnRingIsHis => _turn.State == Ft8TurnState.Mine;

    /// <summary>True where the slot running belongs to the other station.</summary>
    /// <remarks>
    /// **THE GRAYSCALE CARRIER THE CAPTION USED TO BE** (§0.6, work instruction 281
    /// task 3). *yours is next* left the screen, and hue alone cannot tell his slot
    /// from theirs on a printed page or to one man in twelve, so theirs is drawn at
    /// a thinner stroke. Four states, four appearances, none of them a colour: thin
    /// is theirs, ordinary is his, thick is on air, dashed is not known.
    /// </remarks>
    public bool TurnRingIsTheirs => _turn.State == Ft8TurnState.Theirs;

    /// <summary>True where a turn has actually been derived.</summary>
    /// <remarks>
    /// **NOT AN INVITATION** (§0.0). It is false while the clock is unmeasured and
    /// false while nothing has been heard, so anything drawing emphasis from it
    /// stays quiet in exactly the cases where a confident line would be a guess.
    /// </remarks>
    public bool DigitalTurnIsKnown => _turn.IsKnown;

    /// <summary>True where the slot now running is the operator's own.</summary>
    public bool DigitalTurnIsMine => _turn.MineNow;

    /// <summary>Seconds left of the slot now running, or 0 where none is placed.</summary>
    public int DigitalTurnSecondsLeft => _turn.SecondsLeft ?? 0;

    /// <summary>
    /// **Read the beat again**, from the corrected clock and the station he is working.
    /// </summary>
    /// <remarks>
    /// <para>**IT RIDES THE TICK THAT WAS ALREADY THERE.** `_decodeTimer` runs four
    /// times a second, which is sixty looks inside every fifteen-second slot and
    /// ample for a countdown reading in whole seconds. A timer of its own would be
    /// a second timing source, and the instruction that asked for this forbids one.
    /// </para>
    /// <para>**THE PARITY IS THE STATION'S AND NOT THE PANEL'S.** It comes from the
    /// most recent message actually heard from the station he is working, so a
    /// panel with nothing on it says it does not know rather than defaulting to a
    /// half.</para>
    /// </remarks>
    private void RefreshTurn()
    {
        var was = _turn;

        // **THE STOPPED SENTENCE LASTS UNTIL THE NEXT SLOT AND NO LONGER.** It is
        // news about the slot it happened in, and carrying it further would have it
        // describing a slot it has nothing to do with.
        // **CLEARED HERE RATHER THAN AT THE BOUNDARY DRIVER**, which returns early
        // when nothing is armed - and after a stop nothing is. Put there, the
        // sentence would have stood for the rest of the evening.
        if (_sendWasStopped
            && _sendingSlotUtc is { } stoppedIn
            && Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset) is { } nowUtc
            && Ft8Slots.SlotStart(nowUtc) > stoppedIn)
        {
            _sendWasStopped = false;
            _sendingSlotUtc = null;
        }

        _turn = Ft8Turn.Read(
            DateTime.UtcNow, ClockOffset, TheirLastSlot(),
            _sendingSlotUtc, _sendWasStopped);

        if (was == _turn)
        {
            return;
        }

        OnPropertyChanged(nameof(DigitalTurnLine));
        OnPropertyChanged(nameof(DigitalTurnIsKnown));
        OnPropertyChanged(nameof(DigitalTurnIsMine));
        OnPropertyChanged(nameof(DigitalTurnSecondsLeft));
        OnPropertyChanged(nameof(TurnRingSweep));
        OnPropertyChanged(nameof(TurnRingCount));
        OnPropertyChanged(nameof(TurnRingCaption));
        OnPropertyChanged(nameof(TurnRingTip));
        OnPropertyChanged(nameof(TurnRingIsUnknown));
        OnPropertyChanged(nameof(TurnRingIsOnAir));
        OnPropertyChanged(nameof(TurnRingIsHis));
        OnPropertyChanged(nameof(TurnRingIsTheirs));
    }

    /// <summary>The slot of the last thing heard from the station he is working.</summary>
    /// <returns>The boundary, or null where that station has sent nothing.</returns>
    /// <remarks>
    /// **ONE STATION'S PARITY, NOT THE PANEL'S NEWEST ROW.** Two stations calling
    /// him can be using opposite halves of the minute, so reading the newest row
    /// whoever sent it would flip the turn line every time the other one
    /// transmitted. The station is whoever most recently called him, which is who
    /// the panel is following.
    /// </remarks>
    private DateTime? TheirLastSlot()
    {
        var station = ConversationStation();

        if (station.Length == 0)
        {
            return null;
        }

        DateTime? best = null;

        foreach (var row in DigitalMineDecodes)
        {
            if (row.IsSent
                || row.SlotStartUtc == default
                || !string.Equals(row.Sender, station, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (best is null || row.SlotStartUtc > best)
            {
                best = row.SlotStartUtc;
            }
        }

        return best;
    }

    /// <summary>Remember a slot as one he transmitted in, for a test.</summary>
    /// <remarks>
    /// **THE REAL ROUTE NEEDS A RADIO.** A slot is booked where a run says the
    /// whole transmission went out and the radio unkeyed, which needs a port, an
    /// audio endpoint and a boundary to arrive - for a question about one line of
    /// prose. This sets what that path sets and nothing else.
    /// </remarks>
    internal void RememberTransmittedSlotForTests(DateTime slotStartUtc)
        => _transmittedSlots.Add(slotStartUtc);

    /// <summary>What the census would say about one slot, for a test.</summary>
    /// <param name="slotStartUtc">The slot.</param>
    /// <param name="candidates">Places that looked like the start of a signal.</param>
    /// <param name="codewords">How many of those came out as valid codewords.</param>
    /// <param name="checksums">How many of those carried their own checksum.</param>
    /// <returns>The line, exactly as the panel would show it.</returns>
    /// <remarks>
    /// **THE SAME METHOD THE PANEL USES**, handed one slot rather than a whole
    /// reception, so what is tested is the wording rule and not a second copy of
    /// it. Synthesising audio to reach one sentence would spend a minute of
    /// decode on a question about a string.
    /// </remarks>
    internal string CensusForTests(
        DateTime slotStartUtc, int candidates, int codewords = 0, int checksums = 0)
    {
        var reception = new Ft8Reception([], 1, candidates, "")
        {
            Slots =
            [
                new Ft8SlotCensus(
                    slotStartUtc, candidates, codewords, checksums, 0, 0, [], 48_000),
            ],
        };

        return DescribeCensus(reception);
    }

    /// <summary>Hold one turn state, for a test that has no clock to drive.</summary>
    /// <param name="turn">The state the ring should draw.</param>
    /// <param name="sending">What is going out, for the on-air caption.</param>
    /// <remarks>
    /// **THE RING'S FOUR STATES CANNOT ALL BE REACHED BY WAITING.** Two of them
    /// need a transmission in flight and one needs an unmeasured clock, and driving
    /// a real send to look at a caption would need a radio, an audio endpoint and a
    /// slot boundary to arrive. This sets what `RefreshTurn` would have set and
    /// touches nothing else.
    /// </remarks>
    internal void UseTurnForTests(Ft8Turn turn, string sending = "")
    {
        _turn = turn;
        _armedText = sending;

        OnPropertyChanged(nameof(DigitalTurnLine));
        OnPropertyChanged(nameof(TurnRingSweep));
        OnPropertyChanged(nameof(TurnRingCount));
        OnPropertyChanged(nameof(TurnRingCaption));
        OnPropertyChanged(nameof(TurnRingTip));
        OnPropertyChanged(nameof(TurnRingIsUnknown));
        OnPropertyChanged(nameof(TurnRingIsOnAir));
        OnPropertyChanged(nameof(TurnRingIsHis));
        OnPropertyChanged(nameof(TurnRingIsTheirs));
    }

    /// <summary>Read the beat once, for a test, without waiting on a timer.</summary>
    internal void RefreshTurnForTests() => RefreshTurn();

    /// <summary>His own transmissions, oldest first, as rows.</summary>
    /// <remarks>
    /// <para>**THE ROWS ARE KEPT HERE AND NOT IN <see cref="DigitalDecodes"/>**,
    /// because a transmitted message is not a decode: it did not come out of the
    /// decoder, it has no signal report and no time offset, and the decoded
    /// table's counts are about what the band was doing. Putting it there would
    /// have made the shown, hidden and mine totals disagree with the panel they
    /// describe.</para>
    /// <para>**THE TEXT ITSELF IS ALREADY KEPT BY THE ENGINE'S LEDGER** —
    /// `Ft8ContactLedger.RecordSent` books the message, its fields and its slot
    /// against the station it was addressed to. This is the view's copy, in the
    /// shape the list draws, and it is built from the same call that books the
    /// ledger entry so the two cannot disagree about what went out.</para>
    /// <para>**NOTHING HERE REACHES TELEMETRY.** HM-DEC-018 stands untouched:
    /// `TransmitRecord` has no string parameter and cannot carry a message, and
    /// this list is application state on the operator's own machine that is
    /// never written to the record and never uploaded.</para>
    /// </remarks>
    private readonly List<DigitalDecodeRow> _digitalSent = new();

    /// <summary>Where a row belongs in the For you list, by its own slot.</summary>
    /// <param name="row">The row being placed.</param>
    /// <returns>The index to insert it at.</returns>
    /// <remarks>
    /// <para>**THIS SIDE ORDERS ITSELF, BECAUSE HALF ITS ROWS ARE NOT ON THE
    /// DECODED TABLE.** <see cref="SideIndexOf"/> counts a row's position by
    /// walking `DigitalDecodes`, which is exactly right for a row that is in it
    /// and has nothing to say about a sent one. So placement here is by the slot
    /// the message occupied, which both kinds of row carry.</para>
    /// <para>**IT FOLLOWS THE ORDER BUTTON**, so the conversation reads the same
    /// way round as the table beside it. Within one slot the arrival order is
    /// preserved, for `InsertAt`'s reason: the air did not put two messages in a
    /// sequence and neither may this.</para>
    /// <para>**A ROW WITH NO SLOT GOES WHERE A NEW ROW GOES** rather than being
    /// sorted to one end. `SlotStartUtc` is `default` on anything decoded before
    /// it was carried, and treating that as the beginning of time would file it
    /// under the year one.</para>
    /// </remarks>
    private int MineIndexFor(DigitalDecodeRow row)
    {
        if (row.SlotStartUtc == default)
        {
            // **OLDEST FIRST, ALWAYS**, so a row with no slot goes where a new row
            // goes: at the end. The conversation does not follow the sort toggle.
            return DigitalMineDecodes.Count;
        }

        var at = 0;

        while (at < DigitalMineDecodes.Count)
        {
            var here = DigitalMineDecodes[at].SlotStartUtc;

            if (here == default)
            {
                at++;
                continue;
            }

            var before = here <= row.SlotStartUtc;

            if (!before)
            {
                break;
            }

            at++;
        }

        return at;
    }

    /// <summary>Every message addressed to him, whoever sent it, in arrival order.</summary>
    /// <remarks>
    /// **THE PANEL SHOWS ONE CONVERSATION AND THIS HOLDS THEM ALL** (Tim's ruling,
    /// 2026-09-08). Nothing is hidden by showing one: a station that called him is
    /// on the waiting list the moment it is not the one being shown, and everything
    /// it sent is still here to come back to.
    /// </remarks>
    private readonly List<DigitalDecodeRow> _mineAll = new();

    /// <summary>Whose conversation he has chosen, or "" to follow the last caller.</summary>
    /// <remarks>
    /// **EMPTY IS A REAL STATE AND IS THE DEFAULT.** Before he has clicked
    /// anything the panel follows whoever most recently called him, which is what
    /// somebody working a run wants and needs no decision from him. The moment he
    /// clicks a waiting station it stays put, because a panel that jumped away
    /// while he was reading it would be worse than one that never moved.
    /// </remarks>
    private string _conversationWith = "";

    /// <summary>The station whose conversation is on the panel, or "".</summary>
    /// <remarks>
    /// **HIS CHOICE WINS WHILE THE STATION IS STILL THERE**, and the trim can take
    /// it away: a chosen station whose every message has aged off the table leaves
    /// nothing to show, so the panel falls back to following rather than sitting
    /// empty with a callsign at the top of it.
    /// </remarks>
    internal string ConversationStation()
    {
        if (_conversationWith.Length > 0 && HasAnythingFrom(_conversationWith))
        {
            return _conversationWith;
        }

        // **A SLOT IS FOR ORDERING AND IS NOT A CONDITION OF MEMBERSHIP.**
        // `SlotStartUtc` is `default` on anything decoded before it was carried,
        // and requiring one here emptied the whole panel for every such row: the
        // station came back as "" and nothing matched it. So the newest by slot
        // wins where any row has one, and arrival order decides where none does.
        DigitalDecodeRow? newest = null;
        DigitalDecodeRow? lastArrived = null;

        foreach (var row in _mineAll)
        {
            if (row.Sender.Length == 0)
            {
                continue;
            }

            lastArrived = row;

            if (row.SlotStartUtc == default)
            {
                continue;
            }

            if (newest is null || row.SlotStartUtc > newest.SlotStartUtc)
            {
                newest = row;
            }
        }

        if ((newest ?? lastArrived) is { } caller)
        {
            return caller.Sender;
        }

        // **BEFORE ANYBODY ANSWERS, HE IS STILL WORKING SOMEBODY.** The
        // reconstruction of 2026-09-08 caught this: at the first moment of the
        // evening he had transmitted to K9XP and the panel was empty, because the
        // station is derived from what has been heard and nothing had been. His
        // own call is the evidence of who he is calling, so the last station he
        // transmitted to is the conversation until one answers.
        for (var at = _digitalSent.Count - 1; at >= 0; at--)
        {
            if (_digitalSent[at].Addressee.Length > 0)
            {
                return _digitalSent[at].Addressee;
            }
        }

        return "";
    }

    /// <summary>Whether that station has anything left on the table.</summary>
    private bool HasAnythingFrom(string callsign)
    {
        foreach (var row in _mineAll)
        {
            if (string.Equals(row.Sender, callsign, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Which station a row belongs to, whichever way it went.</summary>
    /// <remarks>
    /// **A CONVERSATION HAS TWO DIRECTIONS AND ONE OTHER PARTY.** For something
    /// heard that is whoever sent it; for something he transmitted it is whoever it
    /// was addressed to. Reading the sender of both would file every one of his own
    /// messages under his own callsign, which is a conversation with himself.
    /// </remarks>
    private static string StationOf(DigitalDecodeRow row)
        => row.IsSent ? row.Addressee : row.Sender;

    /// <summary>The stations that have called him and are not the one on show.</summary>
    /// <remarks>
    /// **NOTHING IS HIDDEN** (Tim's ruling, 2026-09-08). Every station that called
    /// him is either the conversation or a row here, and a row here says how long
    /// it has been quiet so a stale one is obvious at a glance.
    /// </remarks>
    public ObservableCollection<Ft8WaitingStation> DigitalWaiting { get; } = new();

    /// <summary>True where somebody else is waiting.</summary>
    public bool HasDigitalWaiting => DigitalWaiting.Count > 0;

    /// <summary>What the waiting strip says about itself when collapsed.</summary>
    /// <remarks>
    /// §0.5: a shut panel still carries its news, and a count is the news here.
    /// </remarks>
    public string DigitalWaitingSummary
        => DigitalWaiting.Count switch
        {
            0 => "",
            1 => "1 other station is calling you",
            _ => DigitalWaiting.Count.ToString(CultureInfo.InvariantCulture)
                + " other stations are calling you",
        };

    /// <summary>**Show this station's conversation instead.**</summary>
    /// <param name="callsign">Whose, from the waiting row he clicked.</param>
    /// <remarks>
    /// **IT SWITCHES A VIEW AND NOTHING ELSE.** No message is composed, nothing is
    /// armed, and the station he was reading keeps every message it sent: it moves
    /// to the waiting list, which is where it was before he clicked.
    /// </remarks>
    [RelayCommand]
    private void ShowConversation(string? callsign)
    {
        var wanted = (callsign ?? "").Trim();

        if (wanted.Length == 0 || !HasAnythingFrom(wanted))
        {
            return;
        }

        _conversationWith = wanted;

        RebuildConversation();
        RecountDecodedFilter();
        RefreshTurn();
    }

    /// <summary>
    /// **Draw one conversation, and list whoever else is calling him.**
    /// </summary>
    /// <remarks>
    /// <para>**THE WHOLE SIDE IS REBUILT RATHER THAN PATCHED.** Which rows are on
    /// the panel depends on the station he is following and on what came
    /// immediately before each row, so there is no position to compute for a row on
    /// its own. It is affordable because this side carries his own traffic, which
    /// is a handful of rows where the left list carries the band.</para>
    /// <para>**THE FOLD RUNS FORWARDS IN TIME, INSIDE ONE CONVERSATION.** A repeat
    /// is the same message from the same station with nothing between it and the
    /// one before, and *nothing between* means nothing in this conversation: a
    /// third station calling in the gap does not make two identical reports into
    /// separate news.</para>
    /// </remarks>
    private void RebuildConversation()
    {
        var station = ConversationStation();

        DigitalMineDecodes.Clear();

        var rows = new List<DigitalDecodeRow>();

        foreach (var row in _mineAll)
        {
            row.RepeatCount = 1;

            if (string.Equals(StationOf(row), station, StringComparison.OrdinalIgnoreCase))
            {
                rows.Add(row);
            }
        }

        foreach (var row in _digitalSent)
        {
            row.RepeatCount = 1;

            if (string.Equals(StationOf(row), station, StringComparison.OrdinalIgnoreCase))
            {
                rows.Add(row);
            }
        }

        DigitalDecodeRow? last = null;

        foreach (var row in rows.OrderBy(r => r.SlotStartUtc))
        {
            // **THE SAME MESSAGE AGAIN, WITH NOTHING IN BETWEEN.** A repeat that
            // arrives after he transmitted is a different fact from one that
            // arrives before: it says the station did not hear his answer, which is
            // the most useful thing this panel has to tell him. Folding it back
            // above his own transmission would destroy exactly that.
            if (last is not null
                && !last.IsSent
                && !row.IsSent
                && row.SlotStartUtc != default
                && string.Equals(last.Message, row.Message, StringComparison.Ordinal))
            {
                last.RepeatCount++;
                continue;
            }

            last = row;
            DigitalMineDecodes.Add(row);
        }

        // **THE CONVERSATION READS FORWARDS AND THE TOGGLE DOES NOT REACH IT**
        // (Tim's ruling, 2026-09-08). It inherited the decoded list's newest-first
        // sort, which put `R+02` above `+27` above `FN00` - the exchange in
        // reverse. **The two lists do different jobs**: the left one is for
        // scanning a band, where the newest line is the one you want at the top,
        // and this one is a conversation, which runs forwards or it is not one.
        //
        // The rows are built oldest-first above and the reverse that used to sit
        // here is gone rather than made conditional, because there is no state in
        // which a conversation should be read backwards.

        RebuildWaiting(station);
    }

    /// <summary>Turn a bound collection over in place.</summary>
    /// <remarks>
    /// **BUILT OLDEST FIRST AND TURNED OVER**, rather than built in the display
    /// direction, because the fold above can only be done walking forwards in time
    /// and doing both at once is how the order button un-folded the panel once
    /// already.
    /// </remarks>
    private static void Reverse(ObservableCollection<DigitalDecodeRow> rows)
    {
        for (var at = 1; at < rows.Count; at++)
        {
            rows.Move(at, 0);
        }
    }

    /// <summary>List everybody calling him who is not on the panel.</summary>
    /// <param name="station">Whose conversation is being shown.</param>
    private void RebuildWaiting(string station)
    {
        var order = new List<string>();
        var last = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        var count = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in _mineAll)
        {
            var who = row.Sender;

            if (who.Length == 0
                || string.Equals(who, station, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!count.ContainsKey(who))
            {
                order.Add(who);
                count[who] = 0;
                last[who] = row.SlotStartUtc;
            }

            count[who]++;

            if (row.SlotStartUtc > last[who])
            {
                last[who] = row.SlotStartUtc;
            }
        }

        DigitalWaiting.Clear();

        // **THE ONE WHO SPOKE MOST RECENTLY IS AT THE TOP**, because that is who
        // is most likely still there.
        foreach (var who in order.OrderByDescending(w => last[w]))
        {
            DigitalWaiting.Add(new Ft8WaitingStation(
                who, count[who], Quiet(last[who]), last[who]));
        }

        OnPropertyChanged(nameof(HasDigitalWaiting));
        OnPropertyChanged(nameof(DigitalWaitingSummary));
    }

    /// <summary>How long a station has been quiet, in slots, or "".</summary>
    /// <remarks>
    /// <para>**IN SLOTS, BECAUSE THE SLOT IS THE UNIT THE BEAT IS IN.** *Four slots
    /// ago* tells him the station has had four chances to hear him and taken none;
    /// *a minute ago* makes him do the arithmetic that matters.</para>
    /// <para>**NO CLOCK MEANS NO AGE** (§0.0). Without a measured offset there is
    /// no way to place a boundary, so counting slots would be counting from a
    /// reading nobody took.</para>
    /// </remarks>
    private string Quiet(DateTime lastSlotUtc)
    {
        if (lastSlotUtc == default
            || Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset) is not { } trueUtc)
        {
            return "";
        }

        var slots = (int)Math.Floor(
            (Ft8Slots.SlotStart(trueUtc) - lastSlotUtc).TotalSeconds / Ft8Slots.SlotSeconds);

        return slots switch
        {
            <= 0 => "this slot",
            1 => "1 slot ago",
            _ => slots.ToString(CultureInfo.InvariantCulture) + " slots ago",
        };
    }

    /// <summary>
    /// **Keep a message this station transmitted, and put it in the conversation.**
    /// </summary>
    /// <param name="message">The text, exactly as it went on the air.</param>
    /// <param name="slotStartUtc">The slot it occupied, in corrected UTC.</param>
    /// <returns>The row, so a test can look at what became of it.</returns>
    /// <remarks>
    /// <para>**CALLED WHERE THE LEDGER IS TOLD, AND NOWHERE ELSE.** The one call
    /// site is beside `RecordSent` in <see cref="AtSlotBoundaryAsync"/>, which
    /// runs only where the transmission actually went out and the radio unkeyed.
    /// A row written at the moment of arming would show him a message a licence
    /// refusal, a stop or a missed boundary meant nobody ever heard.</para>
    /// <para>**IT TRANSMITS NOTHING AND ARMS NOTHING.** This is a list and a
    /// string; the send happened before it was called.</para>
    /// </remarks>
    private DigitalDecodeRow KeepSentRow(string message, DateTime slotStartUtc)
    {
        // **THROUGH THE SAME DOOR AS EVERY OTHER ROW** (work instruction 281 task
        // 6). `DigitalDecodeRow.Sent` built its row with the grid blank and this
        // method does not call `PlaceRow`, so a sent row was the one row in the
        // application the operator's own grid never reached.
        var row = WithOperatorGrid(DigitalDecodeRow.Sent(message, slotStartUtc));

        _digitalSent.Add(row);

        RebuildConversation();
        RecountDecodedFilter();
        RefreshTurn();

        return row;
    }

    /// <summary>
    /// **The one place the operator's own grid reaches a row.**
    /// </summary>
    /// <param name="row">The row, however it was built.</param>
    /// <returns>The same row carrying the grid from Settings.</returns>
    /// <remarks>
    /// <para>**IT IS A METHOD BECAUSE IT WAS NOT ONE PLACE** (work instruction 281
    /// task 6). `PlaceRow`'s own remarks have said *the one place the operator's own
    /// grid reaches a row* since unit 252, and it stopped being true the moment
    /// `KeepSentRow` was added: that method builds its row through
    /// <see cref="DigitalDecodeRow.Sent"/>, which passes `ObserverGrid: ""`, and it
    /// does not call `PlaceRow`. So a message the operator transmitted carried a
    /// blank grid while Settings held `FN00DJ`, verified from callook.info, and the
    /// tooltip on his own CQ told him Hamlet needed a grid square he had already
    /// given it.</para>
    /// <para>**IT IS THE THIRD OF THIS SHAPE** — the menu on the wrong list, the Log
    /// item gated where it could never fire, and now this. All three are a second
    /// construction site added later that does not go through the door the first one
    /// uses, and in all three the value was plainly present the whole time. **A
    /// remark claiming there is one place is not one place**; a method that has to
    /// be called is closer, and the test that goes with this asserts it of a sent
    /// row and a received row together.</para>
    /// <para>**READ FRESH EVERY TIME**, so a grid typed in Settings shows up on the
    /// next row rather than at the next launch, and so no second copy of it lives
    /// anywhere. `OperatorProfile.GridSquare` stays the only one.</para>
    /// </remarks>
    private DigitalDecodeRow WithOperatorGrid(DigitalDecodeRow row)
        => row with { ObserverGrid = _settings.Operator.GridSquare ?? "" };

    /// <summary>The same door the send path uses, for a test.</summary>
    /// <param name="message">The text that went out.</param>
    /// <param name="slotStartUtc">The slot it occupied.</param>
    /// <returns>The row.</returns>
    /// <remarks>
    /// **THE SAME IDIOM AS <see cref="AddDecodeRowForTests"/>.** Driving a real
    /// transmission to prove where a row lands would need a radio, an audio
    /// endpoint and a slot boundary to arrive, for a question about placement.
    /// Nothing in `src/` calls this.
    /// </remarks>
    internal DigitalDecodeRow AddSentRowForTests(string message, DateTime slotStartUtc)
        => KeepSentRow(message, slotStartUtc);

    /// <summary>Where a row at this place on the whole table sits on one side.</summary>
    /// <param name="index">The row's index in <see cref="DigitalDecodes"/>.</param>
    /// <param name="belongs">Which side is being counted.</param>
    /// <returns>Its index in that side's own collection.</returns>
    /// <remarks>
    /// **THE POSITION IS COUNTED FROM THE WHOLE TABLE.** Either side is the whole
    /// table with rows taken out, so a row's place in it is the number of that
    /// side's rows sitting before it — which is what keeps both in the same order
    /// under newest-first, where a row goes neither at the top nor at the bottom
    /// but after the rows already in its own slot.
    /// </remarks>
    private int SideIndexOf(int index, Func<DigitalDecodeRow, bool> belongs)
    {
        var at = 0;

        for (var i = 0; i < index && i < DigitalDecodes.Count; i++)
        {
            if (belongs(DigitalDecodes[i]))
            {
                at++;
            }
        }

        return at;
    }

    /// <summary>
    /// Rebuild the visible table from the whole one, and count both halves.
    /// </summary>
    /// <remarks>
    /// <para>**REMOVED, NOT DIMMED** (Tim's ruling, 2026-09-06, superseding unit
    /// 251's arbiter choice). That choice reasoned carefully and reasoned to the
    /// wrong answer: the operator's problem was never that he could not see the
    /// band's texture, it is that fourteen rows a slot at four slots a minute
    /// scrolls past faster than he can read. Dimming left every one of them on the
    /// list and moving.</para>
    /// <para>**A FULL REBUILD IS ONLY EVER AN ANSWER TO SOMETHING HE DID** — a
    /// toggle, the order button, a clear. Rows arriving take the incremental path
    /// in `PlaceRow`, because clearing and refilling raises a reset and a reset is
    /// what throws a list back to the top while he is reading it.</para>
    /// <para>**THE COUNTS ARE THE NON-VISUAL CARRIER AND NOW THEY ARE THE ONLY
    /// ONE** (§0.6, §0.0). While rows were dimmed, a reader who could not make out
    /// the opacity could still count the rows. They are gone now, so the summary's
    /// hidden count is the single thing standing between a filtered list and a
    /// band that reads as quiet.</para>
    /// </remarks>
    private void ApplyDecodedFilter()
    {
        DigitalVisibleDecodes.Clear();
        DigitalMineDecodes.Clear();

        // **THE CONVERSATION IS REBUILT IN THE ORDER IT HAPPENED AND THEN SORTED**,
        // rather than in the order the decoded table happens to be in. Folding a
        // repeat is a statement about which message came next, so it can only be
        // done walking forwards in time; done down a newest-first table it would
        // find every pair in the wrong order and fold nothing at all.
        _mineAll.Clear();

        foreach (var row in DigitalDecodes)
        {
            if (IsForHim(row))
            {
                _mineAll.Add(row);
            }
            else if (WantsRow(row))
            {
                DigitalVisibleDecodes.Add(row);
            }
        }

        RebuildConversation();

        RecountDecodedFilter();

        OnPropertyChanged(nameof(DigitalDecodedSummary));
    }

    /// <summary>Bring the counts level with the three tables.</summary>
    /// <remarks>
    /// **A ROW ON THE MINE SIDE IS NOT HIDDEN.** It is on the screen, in its own
    /// column, so counting it as hidden would make the left summary claim the band
    /// was busier than what it is showing by exactly the number of messages the
    /// operator most wanted to see. That is the §0.0 fault the hidden count exists
    /// to prevent, arrived at from the other direction.
    /// </remarks>
    private void RecountDecodedFilter()
    {
        DigitalShownCount = DigitalVisibleDecodes.Count;
        DigitalMineCount = DigitalMineDecodes.Count;

        // **THE HIDDEN COUNT IS ARITHMETIC ON THE DECODED TABLE, AND A SENT ROW
        // IS NOT IN IT.** Subtracting the whole mine count would take his own
        // transmissions off a total they were never part of, so the first send
        // of the evening would make the left summary claim one fewer message was
        // hidden than is, and enough sends would drive it negative. Only the
        // decoded rows on that side are subtracted.
        // **EVERY MESSAGE ADDRESSED TO HIM, WHOEVER SENT IT AND WHATEVER THE
        // PANEL IS SHOWING.** The hidden count answers *what did the band do that
        // I cannot see*, and a station on the waiting list is not hidden: it is on
        // the screen with a count beside it and one click away. Counting only the
        // conversation would make the left summary claim the band was busier than
        // what it is showing by exactly the number of messages he most wanted.
        // **AND A FOLDED REPEAT IS STILL EVERY MESSAGE IT STANDS FOR**, which is
        // why this counts `_mineAll` rather than the rows on the panel.
        DigitalHiddenCount =
            DigitalDecodes.Count - DigitalShownCount - _mineAll.Count;

        OnPropertyChanged(nameof(DigitalShownCount));
        OnPropertyChanged(nameof(DigitalMineCount));
        OnPropertyChanged(nameof(DigitalHiddenCount));
        OnPropertyChanged(nameof(HasDigitalMineDecodes));
        OnPropertyChanged(nameof(DigitalMineSummary));
    }

    /// <summary>What the order button says it will do.</summary>
    /// <remarks>
    /// **IT NAMES THE STATE, NOT THE ACTION.** A button reading "oldest first"
    /// is ambiguous about whether that is what you have or what you will get,
    /// and this panel is being read by somebody who has never seen FT8.
    /// </remarks>
    public string DigitalOrderLabel
        => _digitalNewestFirst ? "newest first" : "oldest first";

    /// <summary>What the decoded panel says before anything has decoded.</summary>
    public string DigitalDecodedIdle => DigitalIdleText.Decoded;

    /// <summary>What the plain-English panel says, which is its idle line.</summary>
    /// <remarks>
    /// **IT SAYS NOTHING ELSE YET, ON PURPOSE.** Unit 224 removed the three
    /// invented cards that stood there — a station answering another, a distance,
    /// a signal strength, none of it heard — because the table above them became
    /// real. **What replaces them is Tim's to word** (§12.1), so this unit took a
    /// claim out and did not put one in.
    /// </remarks>

    /// <summary>
    /// The one line of status on the Digital mode strip.
    /// </summary>
    /// <remarks>
    /// **IT WAS `reading it · 9 messages this slot` UNTIL UNIT 224**, written as a
    /// placeholder by work instruction 037 and kept by 038 because the clock
    /// beside it was then the only measured value on the strip. There are two now,
    /// and a strip claiming nine messages beside a table showing what was really
    /// decoded is the mixture this unit exists to end. Before a press it carries
    /// the strip's own idle line, written in August.
    /// </remarks>
    public string DigitalModeStripLine
        => _digitalRefusal.Length > 0
            ? _digitalRefusal
            : _digitalDecodeNote.Length > 0
                ? _digitalDecodeNote
                : DigitalIdleText.ModeStrip;

    /// <summary>The decoded panel's collapsed summary.</summary>
    /// <remarks>
    /// **IT REPORTS WHAT IS REALLY THERE** (HM-DEC-021), including the case the
    /// operator most needs to be told about: audio was decoded and nothing came
    /// out of it, which is a different fact from never having pressed the button.
    /// </remarks>
    public string DigitalDecodedSummary
    {
        get
        {
            // **A REASON NOTHING IS ARRIVING BEATS A COUNT OF WHAT ALREADY
            // ARRIVED** (§0.0.1). An unmeasured clock over a full table is the
            // state that reads most like a working session and is not one.
            if (_digitalRefusal.Length > 0)
            {
                return _digitalRefusal;
            }

            if (DigitalDecodes.Count > 0)
            {
                // **THE LAST ROW AND NOT THE FIRST** (unit 225). While the table
                // was one press' worth these were the same row. It grows now, so
                // reading row zero would leave the summary naming a slot from an
                // hour ago while messages arrived underneath it.
                // **THE NEWEST DECODE, WHICHEVER END IT IS NOW AT.** This
                // read `DigitalDecodes[^1]` when there was only one possible
                // order, and would have named the oldest row the moment the
                // toggle below was built. The arrival list always ends at the
                // newest, whatever the display is doing.
                var newest = _digitalArrivals.Count > 0
                    ? _digitalArrivals[^1].Utc
                    : DigitalDecodes[^1].Utc;

                // **AND WHICH WAY ROUND IT IS ORDERED, BECAUSE A COLLAPSED
                // PANEL STILL CARRIES ITS SUMMARY** (§0.5). Somebody who shuts
                // the panel and opens it later should not have to work out from
                // the rows which end is the live one.
                // **WHAT IS SHOWN AND WHAT IS HIDDEN, BOTH** (Tim's ruling,
                // 2026-09-05, and it carries more weight since his ruling of
                // 2026-09-06 made the filter remove rather than dim). A filter
                // must never be able to make him think the band went quiet, and
                // the count of what it held back is the only thing that stops it
                // now that the rows themselves are gone — in the summary rather
                // than only in the table, because a collapsed panel is exactly
                // where the mistake would be made (§0.5).
                //
                // **IT NAMES THE TOGGLE THAT DID IT.** A line saying rows were
                // held back by "the filter" names nothing he can turn off.
                var hidden = DigitalHiddenCount > 0
                    ? $"{DigitalHiddenCount} hidden by {FilterLabel()} · "
                    : "";

                return $"{newest} UTC · {DigitalShownCount} shown · " + hidden
                    + DigitalOrderLabel + TrimNote();
            }

            return _digitalDecodeNote.Length > 0
                ? _digitalDecodeNote
                : "nothing decoded yet";
        }
    }

    /// <summary>What the last press made of the audio, in one line.</summary>
    private string _digitalDecodeNote = "";

    /// <summary>The census of the last slot that decoded and produced no text.</summary>
    private string _digitalCensusLine = "";

    /// <summary>What the audio path delivered, taken at the moment it is read.</summary>
    /// <remarks>
    /// **TAKEN HERE RATHER THAN CARRIED**, because a ratio is a fact about a
    /// span of wall clock and one carried from earlier would describe a
    /// different span than the line it is printed on.
    /// </remarks>
    private AudioArrival MeasureArrival()
    {
        var tap = _decoder?.Tap;

        if (tap is null)
        {
            return AudioArrival.None;
        }

        var wasapi = _audioInput as WasapiAudioSource;
        var slotSpan = TimeSpan.FromSeconds(Ft8Slots.SlotSeconds);
        var now = DateTime.UtcNow;

        return new AudioArrival(
            tap.ArrivalRatio(slotSpan),
            tap.ArrivalRatioBetween(now - slotSpan, now),
            _decoder?.DecodeQueueDroppedChunks ?? 0,
            _decoder?.DecodeQueueDroppedSamples ?? 0,
            wasapi?.CallbackFailures ?? 0,
            wasapi?.EmptyBuffers ?? 0,
            wasapi?.LongestCallbackMicroseconds ?? 0,
            tap.SamplesSeen,
            // **ZERO HERE MEANS THE PERIOD WAS NOT READ, NOT THAT IT IS ZERO.**
            // The training radio and the WAV replay source are not WASAPI, so
            // they have no device period, and `AudioArrival` says so in words
            // rather than reporting a budget of nothing (§0.0).
            wasapi?.BufferPeriodMicroseconds ?? 0,
            wasapi?.CallbacksOverPeriod ?? 0,
            wasapi?.CallbacksOverHalfPeriod ?? 0,
            wasapi?.CallbacksTimed ?? 0,
            DigitalSpectrum?.DroppedFrames ?? 0,
            DigitalSpectrum?.LongestFrameMicroseconds ?? 0);
    }

    /// <summary>What the census line says about the audio that reached it.</summary>
    /// <remarks>
    /// <para>**IT IS ON THE CENSUS BECAUSE THE CENSUS IS WHERE THE OPERATOR
    /// LOOKS WHEN NOTHING DECODED.** A census that lists candidates and parity
    /// failures without saying whether the audio was whole sends him to the
    /// decoder, which is where three units went.</para>
    /// <para>**IT SAYS NOTHING WHEN NOTHING WAS MEASURED** (§0.0), and it says
    /// nothing when the audio was whole either - a line that always speaks is
    /// one that stops being read, which is the reasoning the readiness strip
    /// already carries.</para>
    /// </remarks>
    private static string ArrivalSuffix(AudioArrival arrival)
    {
        if (double.IsNaN(arrival.RecentRatio))
        {
            return "";
        }

        if (!arrival.FellShort(Ft8SlotWatch.LeastArrival))
        {
            return "";
        }

        var line = "  The sound card delivered " + arrival.RecentText
            + " of the last fifteen seconds, so this slot is fragments.";

        // **AND WHETHER THE CALLBACK WAS THE REASON**, which is the one thing
        // the operator can act on from here: a card that is delivering short
        // because its callbacks are running past their budget is a different
        // fault from a card that is not delivering at all, and until unit 239
        // the census could not tell him which he had. It is added only when the
        // count is nonzero, because a line that always speaks is one he stops
        // reading.
        if (arrival.CallbacksOverPeriod > 0)
        {
            line += "  " + arrival.CallbacksOverPeriod
                + " audio callbacks ran past the "
                + arrival.BufferPeriodMicroseconds.ToString(
                    "0", System.Globalization.CultureInfo.InvariantCulture)
                + " us the device allows them.";
        }

        // **AND WHETHER THE WATERFALL FELL BEHIND, WHICH IS A DIFFERENT FAULT
        // AND NO LONGER THE SAME ONE** (unit 240). Before this unit the picture
        // and the audio shared a thread, so a slow frame was lost audio and the
        // operator could not tell the two apart. Rows dropping now means the
        // picture is behind and the audio is not, and saying so keeps him from
        // chasing the sound card over a stuttering waterfall.
        if (arrival.DroppedFrames > 0)
        {
            line += "  The waterfall dropped " + arrival.DroppedFrames
                + " row(s) keeping up, which costs the picture and not the audio.";
        }

        return line;
    }

    /// <summary>
    /// One line under the decoded table naming the stage that refused, or "".
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS ABOUT A DECODE THAT HAPPENED**, which is what separates it
    /// from unit 228's readiness line above the panels — that one is about the
    /// setup *before* a decode, and the two never describe the same moment.</para>
    /// <para>**IT IS PRESENT ONLY WHILE A SLOT HAS BEEN DECODED AND PRODUCED NO
    /// TEXT.** A line that always says something is one the operator stops reading,
    /// which is the reasoning the readiness strip already carries.</para>
    /// <para>**IT COUNTS AND IT DOES NOT INTERPRET** (`CLAUDE.md` §12.1). It names
    /// the stage the numbers point at and stops there. It does not say the band was
    /// quiet, does not say a station was weak, and does not say what to do.</para>
    /// </remarks>
    public string DigitalCensusLine => _digitalCensusLine;

    /// <summary>True while there is a census worth showing.</summary>
    public bool HasDigitalCensus => _digitalCensusLine.Length > 0;

    /// <summary>Waterfall display gain — a setting, not per-frame data.</summary>
    [ObservableProperty]
    private double _waterfallGain = 1.35;

    /// <summary>
    /// Where this license class may transmit across the band on screen.
    /// </summary>
    /// <remarks>
    /// Computed once, here, from the cited Part 97 data, and handed to every
    /// surface that shows privileges. The band map binds to it today; if the
    /// waterfall or the dial tape ever shows privileges they take this same
    /// list rather than computing their own, so two pictures of one law
    /// cannot disagree (HM-DEC-029).
    /// Empty means the class is unknown and nothing is drawn.
    /// </remarks>
    [ObservableProperty]
    private IReadOnlyList<PrivilegeSpan> _privilegeSpans = Array.Empty<PrivilegeSpan>();

    /// <summary>The line under the band map (HM-DEC-029).</summary>
    [ObservableProperty]
    private PrivilegeStatus _privilegeStatus = new(
        PrivilegeTone.Unknown, "", "", "", "", "");

    /// <summary>The upgrade ladder, shown only while the operator asks for it.</summary>
    [ObservableProperty]
    private IReadOnlyList<string> _upgradeLadder = Array.Empty<string>();

    /// <summary>True while the upgrade panel is open.</summary>
    [ObservableProperty]
    private bool _upgradeLadderVisible;

    /// <summary>
    /// A lookup disagreeing with a hand-set class, or null.
    /// </summary>
    /// <remarks>
    /// Non-null puts the choice on screen. Nothing is written to the profile
    /// while this is set — the operator decides (HM-DEC-028).
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LicenseMismatchNarration))]
    private LicenseResolution? _licenseMismatch;

    /// <summary>
    /// What the disagreement says, or "" when there is none (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// **FLATTENED RATHER THAN REACHED THROUGH.** A binding that walks into a
    /// null object logs a binding error on every evaluation, and a binding error
    /// is a defect rather than a diagnostic (§0.5.1). The panel is hidden when
    /// there is nothing to ask, and hidden is not the same as not evaluated.
    /// </remarks>
    public string LicenseMismatchNarration => LicenseMismatch?.Narration ?? "";

    /// <summary>One line naming which sources answered, for the panel header.</summary>
    [ObservableProperty]
    private string _sourcesSummary = "";

    [ObservableProperty]
    private bool _mapExpanded = true;

    [ObservableProperty]
    private bool _tapeExpanded = true;

    [ObservableProperty]
    private bool _waterfallExpanded = true;

    /// <remarks>
    /// **THE DIGITAL TAB'S THREE PANELS** (work instruction 037). Separate from
    /// the CW waterfall's state for the reason `PanelKeys.DigitalWaterfall`
    /// gives, and open by default like every other panel whose key is unknown
    /// (HM-DEC-021).
    /// </remarks>
    [ObservableProperty]
    private bool _digitalWaterfallExpanded = true;

    [ObservableProperty]
    private bool _digitalDecodedExpanded = true;

    [ObservableProperty]
    private bool _scanExpanded = true;

    /// <summary>Whether the calling panel is open (§0.5).</summary>
    [ObservableProperty]
    private bool _autoCallExpanded = true;

    [ObservableProperty]
    private bool _terminalExpanded = true;

    [ObservableProperty]
    private bool _storyExpanded = true;

    [ObservableProperty]
    private bool _guideExpanded = true;

    [ObservableProperty]
    private bool _spotsExpanded = true;

    [ObservableProperty]
    private bool _leadExpanded = true;

    [ObservableProperty]
    private bool _contactExpanded = true;

    [ObservableProperty]
    private bool _transmitExpanded = true;

    [ObservableProperty]
    private bool _phrasebookExpanded = true;

    [ObservableProperty]
    private bool _heardExpanded = true;

    /// <summary>Who heard the operator, newest first (HM-DEC-075).</summary>
    public ObservableCollection<HeardReport> HeardReports { get; } = new();

    /// <summary>What Hamlet says about whether anybody heard him.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeardSummary))]
    [NotifyPropertyChangedFor(nameof(HeardDetail))]
    [NotifyPropertyChangedFor(nameof(HasHeardReports))]
    private HeardSummary _heard = HeardWatch.Describe(
        null, Array.Empty<HeardReport>(), DateTime.UtcNow);

    /// <summary>The panel's collapsed summary (§0.5).</summary>
    public string HeardSummary => Heard.Headline;

    /// <summary>The paragraph under it.</summary>
    public string HeardDetail => Heard.Detail;

    /// <summary>True when there is a list to draw.</summary>
    public bool HasHeardReports => HeardReports.Count > 0;

    /// <summary>When the operator last called, or null.</summary>
    private DateTime? _calledAtUtc;

    /// <summary>
    /// A skimmer line arrived. Is it about him?
    /// </summary>
    /// <remarks>
    /// Runs on the feed's reader thread, so everything that touches the UI is
    /// posted. The match is exact: telling this operator he was heard when the
    /// machine heard a different station would be the cruelest bug in the
    /// application (HM-DEC-075).
    /// </remarks>
    private void OnRbnSpotParsed(RbnSpot spot)
    {
        if (!HeardWatch.IsMine(spot, _settings.Operator.Callsign))
        {
            return;
        }

        var report = HeardWatch.From(spot);

        // KEPT BEFORE IT IS SHOWN. The screen for a history of these comes
        // later, and a record that only started when somebody built that screen
        // would have missed the first one (HM-DEC-075).
        if (_spotStore is SqliteSpotStore store && !store.RecordHeard(report))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            HeardReports.Insert(0, report);
            OnPropertyChanged(nameof(HasHeardReports));
            RefreshHeard(DateTime.UtcNow);
        });
    }

    /// <summary>
    /// The transmit precondition verdict changed, so the record says so
    /// (HM-DEC-077).
    /// </summary>
    /// <param name="readiness">The verdict, carrying what decided it.</param>
    /// <param name="context">The state it was decided from.</param>
    /// <param name="trigger">What caused the evaluation.</param>
    /// <remarks>
    /// AND IT REACHES THE OPERATOR TOO, not only the file. A file somebody has
    /// to upload is the second line of defense; the first is the screen, and the
    /// evening this was written the screen said nothing at all.
    /// </remarks>
    private void OnReadinessChanged(
        CwReadiness readiness, TransmitContext context, string trigger)
    {
        AppEvents.TransmitReadinessEvaluated(
            _telemetry, readiness, context.State, trigger);

        Decisions.Note(
            "Can I send",
            readiness.Reason,
            readiness.MaySend ? Outcome.Proceeded : Outcome.Refused,
            readiness.Detail,
            DateTime.UtcNow);
    }

    /// <summary>
    /// The send buttons changed state, so the record says what the operator saw
    /// (HM-DEC-078).
    /// </summary>
    /// <param name="enabled">Whether they can be pressed now.</param>
    /// <param name="readiness">The verdict behind it, or null.</param>
    private void OnSendEnabledChanged(bool enabled, CwReadiness? readiness)
    {
        AppEvents.SendButtonsEnabledChanged(_telemetry, enabled, readiness);

        Decisions.Note(
            "Send buttons",
            enabled ? "usable" : readiness?.Reason ?? "no_verdict",
            enabled ? Outcome.Proceeded : Outcome.Refused,
            enabled
                ? "The send buttons are live."
                : "The send buttons are off, and the reason is beside them.",
            DateTime.UtcNow);
    }

    /// <summary>
    /// A send produced a real SWR reading for the first time (HM-DEC-081).
    /// </summary>
    /// <remarks>
    /// Persisted so the note about the back of the radio does not come back on
    /// restart. It has earned its place by then: Hamlet has measured something
    /// about the socket and the operator has read the number.
    /// </remarks>
    private void OnSwrMeasured()
    {
        if (_settings.HasMeasuredSwr)
        {
            return;
        }

        _settings.HasMeasuredSwr = true;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// How many skimmers were reporting on this band, or null (HM-DEC-082).
    /// </summary>
    /// <remarks>
    /// Null rather than zero when the feed is not answering, because an absent
    /// number reads as zero to somebody who has been disappointed before, and
    /// those are opposite facts about the evening.
    /// </remarks>
    private int? SkimmersOnThisBand()
        => _rbn?.SkimmersReporting(
            SelectedBand.Band.LowHz, SelectedBand.Band.HighHz);

    /// <summary>
    /// Everything measured about a send, kept with its record (HM-DEC-082).
    /// </summary>
    /// <param name="evidence">The chain.</param>
    private void OnChainReported(TransmitEvidence evidence)
    {
        AppEvents.TransmitChain(
            _telemetry,
            TransmitChain.BrokeAt(evidence)?.ToString() ?? "none",
            evidence.KeyedSeconds,
            evidence.PowerReading,
            evidence.SwrReading,
            evidence.SkimmersListening,
            evidence.Reports);

        Decisions.Note(
            "Transmit chain",
            TransmitChain.BrokeAt(evidence)?.ToString().ToLowerInvariant() ?? "whole",
            TransmitChain.BrokeAt(evidence) is null
                ? Outcome.Proceeded
                : Outcome.Degraded,
            TransmitChain.Describe(evidence),
            DateTime.UtcNow);
    }

    /// <summary>
    /// A message went to the radio (HM-DEC-079).
    /// </summary>
    /// <param name="message">What is going out. Recorded by length, never by text.</param>
    /// <param name="context">Where and in what mode.</param>
    private void OnSendStarted(string message, TransmitContext context)
    {
        AppEvents.SendStarted(
            _telemetry, message.Length, CwMessage.PieceCount(message),
            context.FrequencyHz,
            context.State[RigField.Mode] is { IsKnown: true } mode ? mode.Text : "");

        Decisions.Note(
            "Sending", "started", Outcome.Proceeded,
            $"{message.Length} characters going out.", DateTime.UtcNow);
    }

    /// <summary>
    /// The radio finished sending, one way or another (HM-DEC-079, HM-DEC-085).
    /// </summary>
    /// <param name="message">What went. Recorded by length, never by text.</param>
    /// <param name="context">Where.</param>
    /// <param name="outcome">What became of it, or null.</param>
    /// <param name="elapsed">How long the radio really keyed.</param>
    /// <param name="end">How the end of it was established.</param>
    /// <remarks>
    /// **COMPLETION MEANS THE RADIO FINISHED SENDING, NOT THAT THE BYTES WERE
    /// ACCEPTED** (HM-DEC-085). This used to be measured from the send call, so
    /// an eighteen-second transmission was recorded as a hundredth of a second,
    /// and that figure was not only wrong in the file: it reached the operator as
    /// "the radio keyed for 0 seconds" in the account of what happened. How the
    /// end was established is recorded beside it, because a duration Hamlet
    /// watched and one it worked out are different kinds of fact (§0.0).
    /// </remarks>
    private void OnSendFinished(
        string message,
        TransmitContext context,
        TransmitOutcome? outcome,
        TimeSpan elapsed,
        TransmissionEnd end)
    {
        var seconds = elapsed.TotalSeconds;
        var result = outcome?.Result;
        var what = result?.Outcome.ToString() ?? "Unknown";

        AppEvents.SendFinished(
            _telemetry, message.Length, what,
            result?.PiecesSent ?? 0, result?.PiecesTotal ?? 0,
            seconds, context.FrequencyHz, end.ToString());

        Decisions.Note(
            "Sending", what.ToLowerInvariant(),
            outcome?.Sent == true ? Outcome.Proceeded : Outcome.Failed,
            end == TransmissionEnd.Expected
                ? $"Ran about {seconds:0.0} seconds by the arithmetic. This radio "
                  + "does not report whether it is keying, so that is a calculation "
                  + "and not something Hamlet watched."
                : $"The radio keyed for {seconds:0.0} seconds.",
            DateTime.UtcNow);
    }

    /// <summary>
    /// "I can hear it and Hamlet can't" (HM-DEC-084).
    /// </summary>
    /// <remarks>
    /// SETTINGS ARE CONSEQUENCES OF INTENT AND NEVER CONTROLS. There is no noise
    /// blanker toggle anywhere in this application and there never will be:
    /// there is a button that names a problem the operator has, and behind it
    /// the handful of changes that usually cause it.
    /// </remarks>
    public ReceiveHelpViewModel ReceiveHelp { get; }

    /// <summary>Whether the panel is open (§0.5).</summary>
    [ObservableProperty]
    private bool _receiveHelpExpanded = true;

    /// <summary>
    /// What the radio is telling Hamlet right now, in one sentence.
    /// </summary>
    /// <remarks>
    /// The header of the panel: three figures and then a reading of them
    /// together. Every one of them measured or absent (§0.0).
    /// </remarks>
    public string ReceiveHeadline
    {
        get
        {
            var state = RigState;

            if (!IsConnected)
            {
                return "Nothing is connected, so there is nothing to look at yet.";
            }

            var heard = DetectedWpm > 0 || !Transcript.IsEmpty;
            var noise = state[RigField.NoiseBlanker].IsKnown
                        || state[RigField.RfGain].IsKnown;

            if (!noise)
            {
                return "Hamlet has not read enough from the radio yet to say how "
                    + "it is set up. It asks on connect, so give it a moment.";
            }

            return heard
                ? "The radio is hearing something and letters are coming through, "
                  + "so the path from the antenna to the decoder is working."
                : "The antenna is hearing something, so the radio is connected to "
                  + "the world. Nothing has resolved into letters yet.";
        }
    }

    /// <summary>How long a quiet decoder waits before offering to look.</summary>
    /// <remarks>
    /// Two minutes. Long enough that an ordinary gap between stations does not
    /// trigger it, short enough that somebody staring at an empty terminal gets
    /// the offer while they are still staring (HM-DEC-084).
    /// </remarks>
    private static readonly TimeSpan QuietBeforeOffering = TimeSpan.FromMinutes(2);

    /// <summary>When the decoder last produced a character.</summary>
    private DateTime _lastDecodeUtc = DateTime.MinValue;

    /// <summary>
    /// When a character was last actually read, for the mode-follow guard.
    /// </summary>
    /// <remarks>
    /// **SEPARATE FROM `_lastDecodeUtc` BECAUSE THAT ONE IS SEEDED AT THE START
    /// OF LISTENING** and this one must not be: a decoder that has just been
    /// switched on has read nothing, and treating that as somebody working Morse
    /// is the whole defect being fixed here.
    /// </remarks>
    private DateTime _lastCharacterUtc = DateTime.MinValue;

    /// <summary>
    /// How long after a character the operator still counts as working Morse.
    /// </summary>
    /// <remarks>
    /// Half a minute. An exchange has gaps of several seconds between overs and a
    /// slow sender leaves long ones inside a message, so anything much shorter
    /// would call him idle in the middle of a contact. Much longer and a station
    /// that finished five minutes ago would still be pinning the mode.
    /// </remarks>
    private static readonly TimeSpan CopyingMorseFor = TimeSpan.FromSeconds(30);

    /// <summary>
    /// True while somebody's Morse is actually coming through.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS WHAT `IsDecoding` WAS BEING ASKED TO MEAN AND DOES NOT.**
    /// That property is true from the moment the decoder starts listening until
    /// it stops, which is the whole session: it says the decoder is switched on
    /// and nothing about whether anybody is sending. Mode-follow read it as
    /// evidence the operator was working Morse, so **every target that was not CW
    /// was refused, permanently**, and the radio stayed in CW at 14.243 MHz where
    /// the map says upper sideband.</para>
    /// <para>The guard it feeds is right and stays. On 2026-08-18 mode-follow
    /// wrote USB with the data variant on, over and over, while the operator sat
    /// on CW main street with a signal decoding, and the send controls refused
    /// `not_in_morse` for sixty-six seconds: **he could not answer a station
    /// because the app had moved his radio out from under him.** What was wrong
    /// was the evidence, not the rule.</para>
    /// </remarks>
    private bool IsCopyingMorse
        => _lastCharacterUtc != DateTime.MinValue
           && DateTime.UtcNow - _lastCharacterUtc < CopyingMorseFor;

    /// <summary>Whether the operator has waved the offer away.</summary>
    private bool _receiveOfferDismissed;

    /// <summary>
    /// A quiet line in the terminal offering to have a look (HM-DEC-084).
    /// </summary>
    /// <remarks>
    /// <para>**A POPUP SOMEBODY HAS TO KNOW TO OPEN IS A POPUP THEY WILL NOT
    /// OPEN WHEN THEY ARE FRUSTRATED**, which is exactly when it is needed. So
    /// the offer also appears where the problem shows: an empty terminal on a
    /// frequency where the app expected something.</para>
    /// <para>One line, not a banner, and dismissible. It says nothing at all
    /// when there is nothing to change, because an offer to fix a radio that is
    /// already right teaches somebody to ignore the next one.</para>
    /// </remarks>
    public string ReceiveOffer
    {
        get
        {
            if (_receiveOfferDismissed || !IsConnected || !IsDecoding)
            {
                return "";
            }

            if (DateTime.UtcNow - _lastDecodeUtc < QuietBeforeOffering)
            {
                return "";
            }

            return ReceiveHelp.Rows.Any(r => r.WouldChange)
                ? "Nothing has come through for a while. Hamlet can see a few "
                  + "things about the radio that usually cause that, and it can "
                  + "put them back afterward."
                : "";
        }
    }

    /// <summary>True when the offer has something to say.</summary>
    public bool HasReceiveOffer => ReceiveOffer.Length > 0;

    /// <summary>Wave the offer away for this session.</summary>
    [RelayCommand]
    private void DismissReceiveOffer()
    {
        _receiveOfferDismissed = true;
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));
    }

    /// <summary>
    /// **False, because the panel this would open is not on any screen.**
    /// </summary>
    /// <remarks>
    /// <para>**A CONTROL'S RESTING APPEARANCE SAYS IT CAN BE PRESSED** (§0.5.1,
    /// HM-DEC-087), and this one could not do anything at all. `widget.receiveHelp`
    /// is one of thirteen data templates in `MainWindow.axaml` that nothing
    /// references, so setting <see cref="ReceiveHelpExpanded"/> expands a panel that
    /// is not in the tree. **Pressing it looked exactly like pressing a button that
    /// works**, which is the fault that rule exists for and the one
    /// `BindingHealthTests` cannot see, because the binding resolves perfectly onto
    /// a property nothing renders.</para>
    /// <para>**IT IS DISABLED RATHER THAN REMOVED, ON PURPOSE.** Whether to rehome
    /// the widget, delete the templates or fold the advice somewhere else is
    /// `HM-OPEN-087` and Tim's to rule; taking the button away would tidy the
    /// evidence out of sight before he has seen it. The exception §0.5.1 carries is
    /// for a control that genuinely cannot be used, and it still has to say why —
    /// which the tooltip does.</para>
    /// </remarks>
    public bool CanOpenReceiveHelp => false;

    /// <summary>Why the offer's button cannot be pressed.</summary>
    /// <remarks>
    /// **IT NAMES NO PLACE TO GO INSTEAD, BECAUSE THERE IS NOT ONE.** A first
    /// draft of this sentence sent him to the Radio menu; that menu carries
    /// Connect, Favorites and Recent and has never carried the receive help. A
    /// tooltip pointing at a screen that does not exist is the same fault as the
    /// button, one level along (§0.0).
    /// </remarks>
    public const string ReceiveHelpUnreachable =
        "The panel this opens is not on any screen at the moment, so this cannot "
        + "do anything. Hamlet has still worked out what it would change, and the "
        + "line beside this button says what it noticed.";

    /// <summary>Open the panel from the offer.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenReceiveHelp))]
    private void OpenReceiveHelp()
    {
        ReceiveHelpExpanded = true;
        _receiveOfferDismissed = true;
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));
    }

    /// <summary>Write one documented setting to the radio (HM-DEC-084).</summary>
    private Task<RigWriteResult> WriteSettingAsync(CivWrite write, int value)
        => _rig is null
            ? Task.FromResult(RigWriteResult.NotSupported("nothing is connected"))
            : _rig.SetSettingAsync(write, value);

    /// <summary>Every change Hamlet made is announced (HM-DEC-084).</summary>
    /// <param name="change">What changed.</param>
    private void OnSettingChanged(SettingChange change)
    {
        // A setting Hamlet changed, successfully, at the operator's request.
        Narrate(change.Says);

        AppEvents.SettingChanged(
            _telemetry, change.Write.Field.ToString(), change.Write.Label,
            change.Was, change.Now, change.Outcome.ToString());

        Decisions.Note(
            "Changed a setting",
            change.Confirmed ? change.Write.Field.ToString() : "unconfirmed",
            change.Confirmed ? Outcome.Proceeded : Outcome.Failed,
            change.Says,
            change.AtUtc);
    }

    /// <summary>What Hamlet has recently decided (HM-DEC-077).</summary>
    public DecisionLogViewModel Decisions { get; } = new();

    /// <summary>**Show him his own log**, which nothing in the app ever did.</summary>
    /// <remarks>
    /// <para>**IT OPENS A READER AND NOTHING ELSE.** The window has no handler of
    /// its own, so there is no path from it to the file at all: a log record is a
    /// statement the operator made and this is not the place to revise one.</para>
    /// <para>**THE READ IS THE ONE THE MARK ALREADY DOES**, taken through
    /// `ContactLogStore` so there is one route to the file and one parser behind
    /// it, rather than a window that learns the format a second time.</para>
    /// </remarks>
    [RelayCommand]
    private void OpenContactLog()
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is null)
        {
            return;
        }

        new Views.ContactLogWindow
        {
            DataContext = new ContactLogViewModel(
                ContactLogStore.ReadRecords(), ContactLogStore.LogPath),
        }.ShowDialog(owner);
    }

    /// <summary>Open the record of what Hamlet decided.</summary>
    [RelayCommand]
    private void OpenDecisionLog()
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is null)
        {
            return;
        }

        new Views.DecisionLogWindow { DataContext = Decisions }.ShowDialog(owner);
    }

    /// <summary>
    /// Something reached the air, so start watching for whoever heard it.
    /// </summary>
    /// <remarks>
    /// A call starts a fresh watch, because "did anybody hear me" is a question
    /// about this call. An answer or an exchange does not: the reports from the
    /// call are still the answer, and clearing them mid-contact would throw away
    /// the thing he came for (HM-DEC-075).
    /// </remarks>
    private void OnSomethingWentOut(SendOption option)
    {
        if (option.Stage == ContactStage.Calling)
        {
            NoteCallWentOut(DateTime.UtcNow);
        }

        // SOME WIDGETS ARRIVE ON THEIR OWN (HM-DEC-086), and the phrasebook is
        // the first of them. A contact beginning is exactly when somebody needs
        // to know what people say, and a contact ending is exactly when they do
        // not, so it comes out on the first thing that reaches the air and goes
        // away again after the sign-off.
        //
        // If the operator has moved it in the meantime it is theirs, and Hamlet
        // stops taking it away.
        // **THE PHRASEBOOK NO LONGER ARRIVES OR LEAVES ON ITS OWN**, because
        // there is nowhere for it to arrive (Tim, 2026-08-27: the canvas is
        // gone). It is on `ABANDONED_WIDGETS.md` with the rest, and if it comes
        // back it comes back as a panel somebody put there on purpose.
        _ = option;
    }

    /// <summary>When the last rig heartbeat went into the record.</summary>
    private DateTime _lastHeartbeatUtc = DateTime.MinValue;

    /// <summary>What the last heartbeat reported, so the next one is a delta.</summary>
    private RigState? _lastHeartbeatState;

    /// <summary>How often a quiet session writes its spine (HM-DEC-077).</summary>
    /// <remarks>
    /// A minute. The session that prompted this ran nearly two hours with its
    /// last human action in the first five minutes, and nothing in between said
    /// what the radio was doing.
    /// </remarks>
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(1);

    /// <summary>Put the rig state in the record on a slow interval.</summary>
    /// <param name="nowUtc">The moment.</param>
    private void Heartbeat(DateTime nowUtc)
    {
        if (nowUtc - _lastHeartbeatUtc < HeartbeatInterval)
        {
            return;
        }

        _lastHeartbeatUtc = nowUtc;

        var state = RigState;

        AppEvents.RigHeartbeat(_telemetry, _lastHeartbeatState, state);

        // **THIS EVENT WAS BUILT BY HM-DEC-092 AND NOTHING EVER CALLED IT.**
        // The link's own health — sent, answered, unanswered, and now what
        // arrived unasked — has never once reached the record, which is why two
        // sessions argued about whether the radio broadcasts from a telemetry
        // field that cannot express the answer. Ruled, built, never invoked, for
        // the third time in this repository.
        if (_rig is Ic7300Rig radio)
        {
            AppEvents.CivLink(
                _telemetry, radio.Link,
                _rigSpectrum?.SweepCount ?? 0,
                _rigSpectrum?.DroppedCount ?? 0);
        }

        _lastHeartbeatState = state;
    }

    /// <summary>Recompute what the heard panel says.</summary>
    /// <param name="nowUtc">The moment, passed in so the states are testable.</param>
    internal void RefreshHeard(DateTime nowUtc)
        => Heard = HeardWatch.Describe(_calledAtUtc, HeardReports.ToList(), nowUtc);

    /// <summary>
    /// A call went out, so start watching (HM-DEC-075).
    /// </summary>
    /// <remarks>
    /// The reports from before this moment are cleared, because the question is
    /// whether anybody heard THIS call. Leaving the last one's answers up would
    /// tell him he had been heard when nothing had come back yet, which is the
    /// feature inflating a silence and the one thing it may never do (§0.0).
    /// </remarks>
    internal void NoteCallWentOut(DateTime nowUtc)
    {
        _calledAtUtc = nowUtc;
        HeardReports.Clear();
        OnPropertyChanged(nameof(HasHeardReports));
        RefreshHeard(nowUtc);
    }

    /// <summary>The frequencies the operator saved (HM-DEC-060).</summary>
    public ObservableCollection<Favorite> Favorites { get; } = new();

    /// <summary>Where the operator has been, most recent first (HM-DEC-072).</summary>
    public ObservableCollection<RecentStation> Recent { get; } = new();

    /// <summary>The Radio menu's Favorites submenu (HM-DEC-060, HM-DEC-072).</summary>
    /// <remarks>
    /// EACH ITEM CARRIES ITS OWN COMMAND, which is not a style choice. A menu
    /// opens in its own popup, and a popup is a separate visual tree, so a
    /// binding that walks up to the window for the command resolves to nothing
    /// and the item silently does nothing when clicked. Nothing about that fails
    /// to compile and nothing about it looks wrong on screen, which is the worst
    /// combination there is.
    /// </remarks>
    public ObservableCollection<TuneMenuItem> FavoriteMenu { get; } = new();

    /// <summary>The Radio menu's Recent submenu (HM-DEC-072).</summary>
    public ObservableCollection<TuneMenuItem> RecentMenu { get; } = new();

    /// <summary>Rebuild the two submenus from the two lists.</summary>
    private void RebuildMenus()
    {
        FavoriteMenu.Clear();
        foreach (var favorite in Favorites)
        {
            var target = favorite;
            FavoriteMenu.Add(new TuneMenuItem(target.Name, () => TuneToFavorite(target)));
        }

        RecentMenu.Clear();
        foreach (var entry in Recent)
        {
            var target = entry;
            RecentMenu.Add(new TuneMenuItem(target.Label, () => TuneToRecent(target)));
        }

        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(HasRecent));
    }

    /// <summary>
    /// The recent place picked from the dropdown, which tunes there.
    /// </summary>
    /// <remarks>
    /// Cleared straight after, exactly as the favorites box is, so it reads
    /// "recent" again rather than showing a stale selection once the dial has
    /// moved on.
    /// </remarks>
    [ObservableProperty]
    private RecentStation? _selectedRecent;

    /// <summary>True when there is anywhere to go back to.</summary>
    public bool HasRecent => Recent.Count > 0;

    /// <summary>
    /// What the star says, inside the display: one short word (HM-DEC-070).
    /// </summary>
    [ObservableProperty]
    private string _favoriteLabel = "save";

    /// <summary>
    /// The name of the favorite the dial is sitting on, or "".
    /// </summary>
    /// <remarks>
    /// Shown on the strip under the display rather than in the black, because a
    /// name is as long as somebody made it and the LCD has a mode badge at one
    /// end and a clock at the other (HM-DEC-070).
    /// </remarks>
    [ObservableProperty]
    private string _favoriteHere = "";

    /// <summary>
    /// The favorite picked from the dropdown, which tunes there.
    /// </summary>
    /// <remarks>
    /// Cleared straight after, so the box reads "favorites" again rather than
    /// showing a stale selection once the dial has moved on.
    /// </remarks>
    [ObservableProperty]
    private Favorite? _selectedFavorite;

    /// <summary>True when there is anything in the list to show.</summary>
    /// <remarks>
    /// A dropdown with nothing in it is a control that looks broken, so it is
    /// absent until there is something to pick.
    /// </remarks>
    public bool HasFavorites => Favorites.Count > 0;

    /// <summary>True when the dial is sitting on a saved frequency.</summary>
    /// <remarks>
    /// The star is filled here and hollow elsewhere, and pressing it on a
    /// favorite un-saves, so it is a toggle rather than two controls.
    /// </remarks>
    [ObservableProperty]
    private bool _isFavorite;

    /// <summary>
    /// The worked contact, both sides, in the operator's own callsign
    /// (HM-DEC-043).
    /// </summary>
    public ContactShapeViewModel ContactShape { get; }

    /// <summary>Sending Morse (HM-DEC-059).</summary>
    public CwTransmitViewModel Transmit { get; }

    /// <summary>The phrases people actually send (HM-DEC-059).</summary>
    public PhrasebookViewModel Phrasebook { get; }

    /// <summary>
    /// Everything the transmit guard and the break-in precondition need.
    /// </summary>
    /// <remarks>
    /// Read at the moment somebody presses rather than captured earlier, because
    /// the frequency, the mode and the break-in setting can all have moved since
    /// the panel was drawn and the guard has to answer about now (§0.2).
    /// </remarks>
    private TransmitContext BuildTransmitContext()
        => new(
            _settings.Operator.LicenseClass,
            FrequencyHz,
            _settings.RestrictTransmitToPrivileges,
            IsConnected,
            _rig?.Capabilities,
            RigState);

    /// <summary>How long spot history is kept before it is pruned.</summary>
    /// <remarks>
    /// A few days is plenty for anything the app does today, and the store
    /// must never grow without bound (HM-DEC-045).
    /// </remarks>
    public static readonly TimeSpan HistoryRetention = TimeSpan.FromDays(3);

    /// <summary>How often pruning runs.</summary>
    private static readonly TimeSpan PruneInterval = TimeSpan.FromHours(6);

    private readonly ISpotStore _spotStore;
    private DateTime _lastPruneUtc = DateTime.MinValue;

    /// <summary>The field guide entries, each with its samples.</summary>
    public IReadOnlyList<ModeCardViewModel> ModeCards { get; } =
        ModeGuide.Modes.Select(m => new ModeCardViewModel(m)).ToList();

    /// <summary>Happening-now spots, plain language, source-labeled.</summary>
    public ObservableCollection<SpotViewModel> Spots { get; } = new();

    /// <summary>Phase 1 bands with best-bet ranking for the current hour.</summary>
    public ObservableCollection<BandButtonViewModel> Bands { get; }

    /// <summary>The training radio plus every serial port on this machine.</summary>
    public ObservableCollection<string> AvailablePorts { get; }

    /// <summary>The operator's license class, or Unknown.</summary>
    public LicenseClass LicenseClass => _settings.Operator.LicenseClass;

    /// <summary>Provenance for the class, shown in Settings and the About box.</summary>
    public string LicenseProvenance => LicenseResolver.DescribeProvenance(_settings.Operator);

    /// <summary>
    /// The Shakespeare line under the wordmark, or "" when there is none
    /// (HM-DEC-039).
    /// </summary>
    [ObservableProperty]
    private string _byline = "";

    /// <summary>Which play the line was bent out of; shown on hover.</summary>
    [ObservableProperty]
    private string _bylineSource = "";

    /// <summary>True when there is a line to show at all.</summary>
    public bool HasByline => Byline.Length > 0;

    partial void OnBylineChanged(string value) => OnPropertyChanged(nameof(HasByline));

    /// <summary>Collapsed-header line for the neighborhood map (HM-DEC-021).</summary>
    public string MapSummary => string.Create(CultureInfo.InvariantCulture,
        $"CW main street · {SelectedBand.Band.CwLowHz / 1e6:0.000}"
        + $"–{SelectedBand.Band.CwHighHz / 1e6:0.000}");

    /// <summary>
    /// True when what the waterfall is drawing was synthesised rather than
    /// received off the air.
    /// </summary>
    /// <remarks>
    /// Derived on every read from the source itself, which has no setter and
    /// neither does this. That is the whole of HM-DEC-026: connection state
    /// IS the mode, so the label cannot drift out of step with what is on
    /// screen, and there is no setting anywhere that could put synthetic
    /// signals up unlabeled.
    /// </remarks>
    public bool SignalsAreSimulated => SpectrumSource?.IsSimulated == true;

    /// <summary>The persistent label the waterfall panel carries.</summary>
    public string SpectrumNotice
        => SignalsAreSimulated
            ? "Simulated signals, from the training radio rather than the air"
            : "";

    /// <summary>Collapsed-header line for the waterfall (HM-DEC-021).</summary>
    public string WaterfallSummary
    {
        get
        {
            if (SpectrumSource is null)
            {
                return "not yet receiving";
            }

            if (SignalsAreSimulated)
            {
                return $"simulated signals · {SelectedBand.Band.Name}";
            }

            // A shut panel may not claim to be receiving while nothing has
            // arrived, which is the collapsed-summary half of HM-DEC-067: a
            // panel that goes quiet about a problem is §0.5 broken by
            // omission.
            if (_rigSpectrum is { PartsReceived: 0 })
            {
                return $"no data has ever arrived · {SelectedBand.Band.Name}";
            }

            if (_rigSpectrum is { SweepCount: 0 })
            {
                return $"parts arriving, no complete sweep · {SelectedBand.Band.Name}";
            }

            // **RECEIVING MEANS FRAMES ARE ARRIVING NOW, NOT THAT ONE ONCE DID.**
            // This used to be the cumulative sweep count, so the first sweep of
            // an evening bought the word for the rest of it: the cable could
            // come out and the summary would go on saying "receiving" until the
            // app was restarted. §0.0 is broken by a single word there, and it
            // is the same word HM-DEC-093 was raised about.
            return ScopeIsFlowing
                ? $"receiving · {SelectedBand.Band.Name}"
                : $"nothing arriving now · {SelectedBand.Band.Name}";
        }
    }

    /// <summary>True when spectrum data is arriving right now.</summary>
    /// <remarks>
    /// A measurement of the last part's age rather than a count of parts ever
    /// seen, because those two answer different questions and only one of them
    /// is "is this working" (§0.0).
    /// </remarks>
    public bool ScopeIsFlowing => WhichScopeStage() == ScopeStage.Flowing;

    /// <summary>
    /// What the scope path has actually done, stage by stage (HM-DEC-093).
    /// </summary>
    /// <remarks>
    /// <para>**AN EMPTY WATERFALL IS A CLAIM AND BLACK IS NOT A STATE**
    /// (HM-DEC-092). "Receiving frames and the band is quiet" and "no frame has
    /// ever arrived" paint exactly the same picture, and they are completely
    /// different facts. Three sessions reported this feature working while the
    /// second was true, and nothing on screen or in the log could have told them
    /// apart.</para>
    /// <para>So the counters are on the display itself: what came off the wire,
    /// what parsed, what was thrown away and why, and what was handed to the
    /// drawing. The first zero is the address of the fault.</para>
    /// </remarks>
    public string ScopeStages
    {
        get
        {
            if (_rigSpectrum is not { } stream)
            {
                return "";
            }

            var quiet = stream.LastPartUtc is { } last
                ? (int)(DateTime.UtcNow - last).TotalSeconds
                : 0;

            // **THE DECIDING IS THE ENGINE'S AND THE DRAWING IS THIS FILE'S**
            // (§0.1). A stage count is a radio fact, and the same fault this
            // exists to catch was invisible for weeks because nothing measured
            // it, so the measurement lives where a test can reach it.
            return ScopeFlow.Say(WhichScopeStage(), stream.PartsReceived,
                stream.PartsParsed, quiet);
        }
    }

    /// <summary>Which stage of the scope path is wrong, or none.</summary>
    private ScopeStage WhichScopeStage()
        => _rigSpectrum is not { } stream
            ? ScopeStage.NotAttached
            : ScopeFlow.Check(
                attached: true,
                stream.PartsReceived,
                stream.PartsParsed,
                stream.SweepsDelivered,
                stream.LastPartUtc,
                DateTime.UtcNow);

    /// <summary>
    /// The stage counts, whether or not anything is wrong (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **HIDING THE ROW MAY NOT MEAN LOSING THE NUMBERS.** The counters are what
    /// proved the scope path was discarding 2,740 parts, so they stay one hover
    /// away rather than being deleted along with the row that shouted them.
    /// </remarks>
    public string ScopeCounts
    {
        get
        {
            if (_rigSpectrum is not { } stream)
            {
                return "Nothing is attached to the waterfall yet.";
            }

            var rejected = stream.PartsRejected == 0
                ? ""
                : $", {stream.PartsRejected} thrown away";

            var since = stream.LastPartUtc is { } last
                ? $", last one {(int)(DateTime.UtcNow - last).TotalSeconds} seconds ago"
                : ", and none has ever arrived";

            return $"{stream.PartsReceived} parts in, {stream.PartsParsed} read"
                + $"{rejected}, {stream.SweepsDelivered} sweeps drawn{since}.";
        }
    }

    /// <summary>True when a stage is wrong and the row has to be on screen.</summary>
    public bool HasScopeStages => ScopeStages.Length > 0;

    /// <summary>
    /// Why the first part Hamlet could not read was rejected, or "".
    /// </summary>
    public string ScopeRejection => _rigSpectrum?.FirstRejection ?? "";

    /// <summary>True when at least one part could not be read.</summary>
    public bool HasScopeRejection => ScopeRejection.Length > 0;

    /// <summary>The decoded Morse, on its way to the terminal.</summary>
    /// <remarks>
    /// Held here and read by the control directly rather than bound as a
    /// string, which is the arrangement HM-DEC-006 settled for the waterfall and
    /// applies for the same reason: at speed this fills at about forty
    /// characters a second.
    /// </remarks>
    public CwTranscript Transcript { get; } = new();

    /// <summary>
    /// The receiver's front end, in one chip beside the filter width.
    /// </summary>
    /// <remarks>
    /// <para>**HE WAS LOOKING AT THIS SCREEN WHILE IT HAPPENED.** On 20 metres in
    /// daylight at S9 with nothing readable, the radio was reporting
    /// `Overflow: overloading` with preamp 1 on, and Hamlet was reading it every
    /// quarter of a second and saying nothing. Overload compresses the whole
    /// passband together, so there is no tone standing above anything and the
    /// decoder measures amplitude: the ear can still take rhythm out of a
    /// compressed mess and the decoder cannot.</para>
    /// <para>**A VALUE NEVER READ SAYS SO** (HM-DEC-009). Not a blank and not a
    /// default: a panel asserting the preamp is off when the read failed is worse
    /// than one saying it does not know.</para>
    /// <para>**AND `RfGain` IS NOT HERE.** The operator has seen it report
    /// 100 per cent with the knob at noon, and a figure he has already watched
    /// contradict his own radio does not go on a panel (§0.0).</para>
    /// </remarks>
    [ObservableProperty]
    private string _frontEndText = "preamp unknown · attenuator unknown";

    /// <summary>The preamplifier setting in the radio's own words, or unknown.</summary>
    [ObservableProperty]
    private string _preampText = "preamp unknown";

    /// <summary>The attenuator setting in the radio's own words, or unknown.</summary>
    [ObservableProperty]
    private string _attenuatorText = "attenuator unknown";

    /// <summary>True while the preamplifier is switched on.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverflowAdvice))]
    private bool _preampIsOn;

    /// <summary>True while the radio reports its front end overloading.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverflowAdvice))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _frontEndIsOverloading;

    /// <summary>What to do about an overloading front end, in terms of a knob.</summary>
    /// <remarks>
    /// <para>**"YOUR RECEIVER IS OVERLOADING" IS A DIAGNOSIS AND NOT HELP.** The
    /// operator this application is for has never thought about front-end
    /// overload, and what he needs is the name of the button. On the IC-7300 the
    /// preamp and the attenuator share **P.AMP/ATT**, and each press cycles
    /// preamp 1, preamp 2 and off (§4, Full Manual `A7292-4EX-6`).</para>
    /// <para>**THE ATTENUATOR IS MENTIONED ONLY ONCE THE PREAMP IS ALREADY
    /// OFF**, because advice about a knob already in the right position is noise.
    /// And nothing here mentions RF gain: Hamlet's read of it is not trusted, so
    /// it advises on nothing it cannot see.</para>
    /// <para>**IT SAYS IT ONCE AND LETS IT STAND.** The sentence does not change
    /// while the condition holds, so nothing blinks or re-announces itself at him
    /// four times a second.</para>
    /// </remarks>
    public string OverflowAdvice => OverflowAdviceFor(FrontEndIsOverloading, PreampIsOn);

    /// <summary>
    /// What else on the receive side is standing in the way, in one line.
    /// </summary>
    /// <remarks>
    /// <para>**HM-DEC-148 DID THIS FOR THE PREAMP AND STOPPED THERE**, and the
    /// noise blanker, the noise reduction and the filter width are the same class
    /// of fault: Hamlet reads all three from the radio, writes them into the
    /// capture sidecar, and has never mentioned one of them on the screen the
    /// operator is looking at. A thing Hamlet knows and does not say is the same
    /// defect as a decode with no signal behind it.</para>
    /// <para>**READ-ONLY.** Nothing here writes to the radio; the rule it comes
    /// from has nowhere to put a command. A later unit may offer a button he
    /// presses.</para>
    /// <para>The rule itself is in the engine, because it is radio knowledge
    /// (§0.1), and this is the seam that shows it.</para>
    /// </remarks>
    [ObservableProperty]
    private string _receiveObstructionText = "";

    /// <summary>
    /// Whether the decoder's pitch is held, and what it is held at.
    /// </summary>
    /// <remarks>
    /// <para>**A LOCK THE OPERATOR CANNOT SEE IS A LOCK HE CANNOT TRUST.** A
    /// wandering decode and a held one look identical on screen, so without this
    /// the operator has no way to tell whether the thing he pressed did
    /// anything, and no way to tell later that it is still holding.</para>
    /// <para>It says the pitch to a tenth of a hertz because that is what the
    /// lock actually holds — an interpolated peak, not a bin — and rounding it to
    /// a whole number on the panel would make two different locks look like the
    /// same one.</para>
    /// <para>Empty while the tracker is steering, so nothing is said when there
    /// is nothing to say (HM-DEC-148's precedent for the advisory area).</para>
    /// </remarks>
    [ObservableProperty]
    private string _pitchLockText = "";

    /// <summary>What the lock control reads right now.</summary>
    [ObservableProperty]
    private string _pitchLockLabel = "Hold this pitch";

    /// <summary>
    /// Hold the decoder's pitch where the station is, or let it follow again.
    /// </summary>
    /// <remarks>
    /// <para>**THE TRACKER IS MEASURABLY THE LARGEST SOURCE OF SOUP IN THIS
    /// DECODER**, and until now the operator had no way to take it out of the
    /// path. Unit 002 put a clean generated station through the production path
    /// and got twenty-two characters that were never sent, and through the same
    /// window with the pitch nailed it got none.</para>
    /// <para>**IT LOCKS TO THE MEASURED PEAK AND NOT TO THE RADIO'S CW PITCH.**
    /// A capture from 2026-08-24 carries `CwPitch 600 Hz` while the station in it
    /// sat at 439.81, so a lock to the radio's setting would have pointed the
    /// filter at empty spectrum and held it there.</para>
    /// <para>Where nothing can be measured it refuses and says so, rather than
    /// holding a pitch nobody found (§0.0).</para>
    /// </remarks>
    [RelayCommand]
    private void TogglePitchLock()
    {
        if (_decoder is not { } decoder)
        {
            return;
        }

        if (decoder.IsLocked)
        {
            decoder.Unlock();
            PitchLockLabel = "Hold this pitch";
            PitchLockText = "";

            return;
        }

        var locked = decoder.Lock();

        if (double.IsNaN(locked))
        {
            PitchLockText =
                "There is not enough measured yet to hold a pitch, so nothing "
                + "was locked and the decoder is still following. Give it a few "
                + "seconds of a station and press again.";

            return;
        }

        PitchLockLabel = "Follow again";
    }

    /// <summary>The rule itself, so the test reads it rather than a copy (§0).</summary>
    /// <param name="overloading">Whether the radio says its front end is overloading.</param>
    /// <param name="preampIsOn">Whether the preamplifier is switched on.</param>
    /// <returns>What to say, or "" when there is nothing to say.</returns>
    internal static string OverflowAdviceFor(bool overloading, bool preampIsOn)
    {
        if (!overloading)
        {
            return "";
        }

        return preampIsOn
            ? "The radio says its front end is overloading, which means the signal "
              + "coming in is stronger than the receiver can handle and everything "
              + "in the passband is being squashed together. Nothing will decode "
              + "until that stops. Press P.AMP/ATT on the front of the radio until "
              + "the preamp reads off."
            : "The radio says its front end is overloading, and the preamp is "
              + "already off, so the next thing to try is the attenuator. Hold "
              + "P.AMP/ATT for a moment to bring it in. A strong band in daylight "
              + "can do this on its own.";
    }

    /// <summary>What the chip beside the filter width reads.</summary>
    /// <param name="overloading">Whether the radio says its front end is overloading.</param>
    /// <param name="preamp">The preamplifier in the radio's words, or unknown.</param>
    /// <param name="attenuator">The attenuator in the radio's words, or unknown.</param>
    /// <returns>The chip's text.</returns>
    /// <remarks>
    /// While it is overloading the chip leads with that and keeps the preamp
    /// beside it, because the preamp is the thing he is about to change and the
    /// attenuator is one step further on.
    /// </remarks>
    internal static string FrontEndTextFor(
        bool overloading, string preamp, string attenuator)
        => overloading ? $"overloading · {preamp}" : $"{preamp} · {attenuator}";

    /// <summary>The preamplifier, named rather than just valued.</summary>
    /// <param name="setting">0, 1 or 2, or null when it has never been read.</param>
    /// <returns>The label.</returns>
    /// <remarks>
    /// **"on" WOULD NOT DO.** Preamp 1 and preamp 2 are different settings on this
    /// radio and an operator deciding whether his front end is overloading needs
    /// to know which one is in. And a setting never read says so rather than
    /// defaulting to off (HM-DEC-009).
    /// </remarks>
    internal static string PreampLabel(int? setting) => setting switch
    {
        0 => "preamp off",
        1 => "preamp 1",
        2 => "preamp 2",
        null => "preamp unknown",
        _ => $"preamp {setting}",
    };

    /// <summary>The attenuator, named rather than just valued.</summary>
    /// <param name="decibels">The attenuation, or null when never read.</param>
    /// <returns>The label.</returns>
    internal static string AttenuatorLabel(int? decibels) => decibels switch
    {
        null => "att unknown",
        0 => "att off",
        _ => $"att {decibels} dB",
    };

    /// <summary>True once the filter width has been read from the radio.</summary>
    public bool HasFilterBandwidth => FilterBandwidthText.Length > 0;

    /// <summary>Everything Hamlet currently knows about the radio.</summary>
    /// <remarks>
    /// <para>**THE MONITOR IS THE SOURCE OF TRUTH WHILE ONE IS ATTACHED**, and
    /// the fallback is the last reading that came through
    /// <see cref="ApplyRigState"/>, which HM-DEC-078 makes the only seam rig
    /// state enters the UI by. In the running application the two are the same
    /// value at every observable moment: the monitor is created before any state
    /// can arrive, and <see cref="StopRigMonitor"/> applies
    /// <see cref="RigState.Empty"/> on the way out, so a disconnected Hamlet
    /// still reports knowing nothing rather than the last radio's readings
    /// (§0.0).</para>
    /// <para>**WHAT IT CHANGES IS THAT THE SEAM CAN BE DRIVEN WITHOUT A RADIO**
    /// (unit 228). This property used to answer `Empty` forever unless a real
    /// monitor was polling a real port, which put anything reading the mode
    /// beyond the reach of a test. The readiness line reads the mode, and a
    /// condition no test can reach is a condition nobody has checked.</para>
    /// </remarks>
    public RigState RigState => _rigMonitor?.State ?? _rigStateApplied;

    /// <summary>The last state that came through the one seam.</summary>
    private RigState _rigStateApplied = RigState.Empty;

    /// <summary>True once the decoder is tracking a speed worth showing.</summary>
    public bool HasDetectedSpeed => DetectedWpm > 0;

    /// <summary>The live speed readout on the terminal's header.</summary>
    public string TerminalSpeedText
        => DetectedWpm > 0 ? $"{DetectedWpm} WPM" : "";

    /// <summary>
    /// Why the speed field is empty, or empty itself (HM-OPEN-022).
    /// </summary>
    /// <remarks>
    /// <para>**A BLANK BOX AND A BLANK BOX THAT SAYS WHY ARE DIFFERENT THINGS**
    /// (§0.0.1). No speed is named until a clock has been proved, because a
    /// number between two stations describes neither of them, and the field goes
    /// quiet across a handover. Left as a bare gap it reads as something broken;
    /// this is the sentence that makes it read as Hamlet working.</para>
    /// </remarks>
    public string SpeedReacquiringText
        => SpeedIsReacquiring ? "working out the speed" : "";

    /// <summary>True while no speed has been proved.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedReacquiringText))]
    private bool _speedIsReacquiring;

    /// <summary>
    /// What just changed about who is sending, or empty (HM-DEC-096).
    /// </summary>
    /// <remarks>
    /// **A SPEED CHANGE AND A TRACKER SWITCH BOTH MEAN SOMEBODY ELSE STARTED
    /// TRANSMITTING**, which is the single most useful thing the decoder knows
    /// and the one it never said. Annotated rather than silently absorbed,
    /// because the alternative is a transcript in which two stations run into
    /// one another with nothing marking the seam.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHandover))]
    private string _handoverNote = "";

    /// <summary>True when there is a handover worth marking.</summary>
    public bool HasHandover => HandoverNote.Length > 0;

    /// <summary>
    /// What the leading edge is reading ahead of the settled pass.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTip))]
    private string _tipText = "";

    /// <summary>True when the leading edge is ahead of the settled pass.</summary>
    public bool HasTip => TipText.Length > 0;

    // **THE SETTLED PASS'S OWN READOUTS WENT WITH IT.** The tip mark, the
    // ceiling note and the revision count were all one pass reporting on
    // another: whether the second was refusing behind the leading edge, whether
    // its window had hit the ceiling thirty elements wanted, and how often it
    // changed the first one's mind. There is one pass now, and a line describing
    // a decoder that is gone is the fault this removal exists to end
    // (HM-DEC-091).

    /// <summary>What the terminal shows before anything has been decoded.</summary>
    public string TerminalIdleText
        => !IsDecoding
            ? "not listening yet. Connect a radio, or pick the training radio, and this fills in."
            : $"listening to {AudioInputName}. Nothing decoded yet.";

    /// <summary>
    /// Collapsed-header line for the CW terminal (HM-DEC-021).
    /// </summary>
    /// <remarks>
    /// A SHUT PANEL STILL HAS TO TELL THE TRUTH (§0.5). If the decoder is
    /// struggling, that is exactly the moment somebody would shut the panel and
    /// conclude the app does not work, so the note travels into the summary
    /// rather than being hidden with the detail.
    /// </remarks>
    public string TerminalSummary
    {
        get
        {
            if (!IsDecoding)
            {
                return "not listening";
            }

            if (Transcript.IsEmpty)
            {
                return "listening";
            }

            var speed = DetectedWpm > 0 ? $"{DetectedWpm} WPM · " : "";
            var tail = Transcript.Tail(28);

            return $"{speed}{tail}";
        }
    }

    /// <summary>Collapsed-header line for the field guide.</summary>
    public string GuideSummary => $"{ModeCards.Count} modes · hear each one";

    /// <summary>Designer constructor.</summary>
    public MainWindowViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="settings">Live settings; panel and feed state persist here.</param>
    /// <param name="telemetry">The writer, or null.</param>
    public MainWindowViewModel(AppSettings settings, JsonlTelemetry? telemetry)
    {
        _settings = settings;
        _telemetry = telemetry;

        // **THE DRIVE CONTROL UNDER THE WATERFALL OPENS ON THE LEVEL IN FORCE**
        // (work instruction 269, task 2), read the same way
        // `SettingsViewModel.cs:163` reads it, off the one `AppSettings` both
        // screens share. Assigned to the field rather than the property so that
        // opening the window does not count as the operator moving the control
        // and re-save a level nothing changed.
        _transmitDrivePercent = TransmitDrive.PercentFor(settings.TransmitDrivePeak);

        // **THE VISIBLE TABLE MIRRORS THE WHOLE ONE, RATHER THAN EVERY CALLER
        // REMEMBERING TO FILL BOTH** (unit 252 task 2). There are four places a
        // row leaves or joins `DigitalDecodes` — the decoder's own door, the row
        // cap, the order toggle's rebuild and the two clears — and a second
        // collection maintained by hand at each of them is four chances for the
        // two to disagree about what was heard. Subscribing once means a row on
        // the table is on the visible table exactly when the toggles want it,
        // whatever put it there.
        DigitalDecodes.CollectionChanged += OnDigitalDecodesChanged;

        // **THE TABS BEFORE ANYTHING THAT COULD CHANGE THE MODE**, because the
        // mode's own change handler walks them.
        ModeTabs = OperatingModes
            .Select(m => new ModeTabViewModel(m, name => OperatingMode = name))
            .ToList();

        // **THE APP OPENS IN THE MODE IT WAS LAST IN** (Tim's ruling,
        // 2026-09-05, unit 251 task 3).
        //
        // **THE FIELD IS SET RATHER THAN THE PROPERTY**, so the change handler
        // does not run during construction. It would walk tabs that are not yet
        // wired to a window and schedule a mode follow before the settings this
        // constructor is still reading are all in place. The tab strip is put in
        // step directly on the next line, which is the only thing the handler
        // would have done that matters here.
        //
        // **AN UNKNOWN OR MISSING VALUE IS CW.** A settings file naming a tab
        // that does not exist must not leave the window with no workspace
        // showing — the blank-screen failure `ModeTabViewModel` records from
        // 2026-08-27, which is exactly what an unmatched mode string produces.
        _operatingMode = OperatingModes.FirstOrDefault(
            m => string.Equals(m, settings.LastOperatingMode?.Trim(),
                StringComparison.OrdinalIgnoreCase))
            ?? "CW";

        foreach (var tab in ModeTabs)
        {
            tab.Follow(tab.Name == _operatingMode);
        }

        // **AND WITHIN DIGITAL, THE SUB-MODE.** What he chose, which is not where
        // the dial is; see `DigitalModeChip.For`. Nothing is tuned by reading it.
        _chosenDigitalMode = DigitalModeChip.Canonical(settings.LastDigitalSubMode);

        Bands = new ObservableCollection<BandButtonViewModel>(
            HfBands.Bands.Select(b => new BandButtonViewModel(b)));

        _selectedBand = Bands.FirstOrDefault(b => b.Band.Name == settings.LastBand)
                        ?? Bands.First(b => b.Band.Name == "40 m");
        _frequencyHz = _selectedBand.Band.JumpHz;

        _mapExpanded = settings.IsPanelExpanded(PanelKeys.Map);
        _tapeExpanded = settings.IsPanelExpanded(PanelKeys.Tape);
        _waterfallExpanded = settings.IsPanelExpanded(PanelKeys.Waterfall);
        _digitalWaterfallExpanded = settings.IsPanelExpanded(PanelKeys.DigitalWaterfall);
        _digitalDecodedExpanded = settings.IsPanelExpanded(PanelKeys.DigitalDecoded);
        _digitalNewestFirst = settings.DecodedNewestFirst;

        // **TWO FLAGS SINCE UNIT 252, AND A MISSING ONE IS OFF**, which is
        // `everything` and is where this panel has always started. A settings
        // file written by unit 251 carries one exclusive choice instead, and
        // `SettingsMigrations` has already turned it into these two by the time
        // this reads them, so an operator who left the panel on `CQ only` finds
        // it on `CQ` rather than back at everything.
        _digitalShowCq = settings.DecodedShowCq;
        _scanExpanded = settings.IsPanelExpanded(PanelKeys.Scan);
        _autoCallExpanded = settings.IsPanelExpanded(PanelKeys.AutoCall);
        _terminalExpanded = settings.IsPanelExpanded(PanelKeys.Terminal);
        _storyExpanded = settings.IsPanelExpanded(PanelKeys.Story);
        _guideExpanded = settings.IsPanelExpanded(PanelKeys.Guide);
        _spotsExpanded = settings.IsPanelExpanded(PanelKeys.Spots);
        _leadExpanded = settings.IsPanelExpanded(PanelKeys.Lead);
        _contactExpanded = settings.IsPanelExpanded(PanelKeys.Contact);
        _transmitExpanded = settings.IsPanelExpanded(PanelKeys.Transmit);
        _phrasebookExpanded = settings.IsPanelExpanded(PanelKeys.Phrasebook);
        _heardExpanded = settings.IsPanelExpanded(PanelKeys.Heard);

        ContactShape = new ContactShapeViewModel(settings.Operator.Callsign);

        // Everything the guard and the break-in precondition need, read at the
        // moment somebody presses rather than captured when the panel was built
        // (HM-DEC-059).
        Transmit = new CwTransmitViewModel(
            BuildTransmitContext, OnSomethingWentOut, OnReadinessChanged,
            OnSendEnabledChanged, OnSendStarted, OnSendFinished, OnSwrMeasured,
            SkimmersOnThisBand,
            () => SelectedBand.Band.Name,
            OnChainReported)
        {
            YourCall = settings.Operator.Callsign,
            Qth = settings.Operator.Location,
        };

        Phrasebook = new PhrasebookViewModel();

        foreach (var saved in settings.Favorites)
        {
            Favorites.Add(new Favorite(
                saved.FrequencyHz, saved.Name, saved.Mode, saved.BandName,
                saved.Neighborhood, saved.SavedUtc, saved.Note));
        }

        foreach (var saved in settings.Recent)
        {
            // A NAME WITH NO RECORDED SOURCE CAME FROM A SPOT FEED, because
            // that was the only way one could get in before provenance existed
            // (HM-DEC-073). That is a fact about the file rather than a guess
            // about the entry, so nothing is invented by reading it that way.
            var source = Enum.TryParse<StationSource>(saved.StationSource, out var parsed)
                ? parsed
                : saved.Station.Length > 0
                    ? StationSource.SpotFeed
                    : StationSource.None;

            Recent.Add(new RecentStation(
                saved.FrequencyHz, saved.Station, saved.Mode, saved.BandName,
                saved.Neighborhood, saved.VisitedUtc, source,
                // A profile written before HM-DEC-134 has no count and the
                // property defaults to one, which is what an entry in this list
                // has always meant. Nothing is migrated because nothing is lost.
                Math.Max(1, saved.Visits)));
        }

        ReceiveHelp = new ReceiveHelpViewModel(
            () => RigState, WriteSettingAsync, OnSettingChanged);

        _receiveHelpExpanded = settings.IsPanelExpanded(PanelKeys.ReceiveHelp);

        RebuildMenus();


        // A stored lens is the operator's own last answer rather than a guess,
        // so it is restored and inference never runs against it (HM-DEC-057).
        if (Enum.TryParse<SpotLens>(settings.SpotLens, out var storedLens))
        {
            _lens = storedLens;
            _lensChosenByOperator = true;
        }

        _families = new HashSet<ModeFamily>(FamilyFilter.Parse(settings.SpotFamilies));

        _isBestChance = _lens == SpotLens.BestChance;
        _isWhatsNew = _lens == SpotLens.WhatsNew;
        _lensQuestion = SpotLensView.Question(_lens);

        // History, or an honest substitute for it. A store that cannot be
        // opened is a nuisance, never a reason not to start (§8, HM-DEC-045).
        _spotStore = SqliteSpotStore.TryOpen(
            System.IO.Path.Combine(SettingsStore.DataFolder, SqliteSpotStore.FileName))
            ?? (ISpotStore)new MemorySpotStore();

        if (!_spotStore.IsPersistent)
        {
            AppEvents.SpotHistoryUnavailable(_telemetry);
        }

        AvailablePorts = new ObservableCollection<string> { TrainingRadio };
        foreach (var name in SafePortNames())
        {
            AvailablePorts.Add(name);
        }

        if (settings.LastPort is not null && AvailablePorts.Contains(settings.LastPort))
        {
            _selectedPort = settings.LastPort;
        }

        _rigSendTimer = new DispatcherTimer(
            RigSendThrottle, DispatcherPriority.Background, OnRigSendTick);
        _rigSendTimer.Stop();

        _modeFollow = ModeFollowState.Armed(settings.ModeFollowsTheMap);
        _modeSettleTimer = new DispatcherTimer(
            ModeSettleDelay, DispatcherPriority.Background, OnModeSettleTick);
        _modeSettleTimer.Stop();

        // **THIS ONE RUNS WHETHER OR NOT ANYTHING IS HAPPENING**, because
        // stillness is what it is looking for and stillness raises no events.
        _dwellTimer = new DispatcherTimer(
            DwellLook, DispatcherPriority.Background, OnDwellLook);
        _dwellTimer.Start();

        _spotRefreshTimer = new DispatcherTimer(
            TimeSpan.FromMinutes(AppSettings.DefaultSpotRefreshMinutes),
            DispatcherPriority.Background, OnSpotRefreshTick);
        _spotRefreshTimer.Stop();

        _ageTimer = new DispatcherTimer(AgeTick, DispatcherPriority.Background, OnAgeTick);

        // The decoder runs on whichever thread the audio arrives on, so the
        // readouts it feeds are refreshed here rather than raised from there.
        // Four times a second is faster than a speed estimate moves and slower
        // than anything it would be worth interrupting the UI for.
        _decodeTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, OnDecodeTick);

        _clockTimer = new DispatcherTimer(
            TimeSpan.FromMinutes(10), DispatcherPriority.Background, OnClockTick);
        _clockTimer.Start();

        // The first reading is wanted before ten minutes have passed, and the
        // query is awaited nowhere: it lands when it lands.
        _ = QueryTheClockAsync();
        _ageTimer.Stop();

        _activitySource = BuildSources();

        Neighborhoods = NeighborhoodPlan.WithEdges(_selectedBand.Band);
        ShowNeighborhood(Neighborhoods.First(n => n.Contains(FrequencyHz)));
        UpdateModeLine();
        UpdateSpotFreshness();

        _hasTunedByWheel = settings.HasTunedByWheel;

        PickByline();

        // THE SCANNER'S FACE (HM-DEC-107, §0.2.1). It is built before the
        // canvas because the canvas places it, and it stays detached from any
        // radio until one connects: a scanner that could move the dial with no
        // rig behind it would be a scanner with nothing to abort against.
        // **THE PREDICATE OUTLIVES THE ORDER THEY ARE BUILT IN**, and the null
        // is real rather than papered over: between these two statements there is
        // no calling cycle to be running, so "not transmitting" is the true
        // answer in that window and not a convenient one.
        Scan = new ScanViewModel(
            line => StatusText = line,
            tune: TuneTo,
            transmitting: () => _autoCall?.IsCalling ?? false);

        // **THE TWO ASK EACH OTHER RATHER THAN TRACKING EACH OTHER**
        // (HM-DEC-098). The scanner moves the dial and this transmits on
        // it, so a stale copy of "is the other one running" is exactly the
        // state that would let Hamlet transmit mid-tune on a frequency
        // neither component believes it is on.
        AutoCall = _autoCall = new AutoCallViewModel(
            line => StatusText = line, () => Scan.IsScanning);

        // THE OPERATOR'S OWN SCAN FILE, WRITTEN ONCE (§0.2.1). It cannot be
        // edited until it exists, and nothing else in the app was going to
        // create it. Never overwritten afterwards: what he wrote is his.
        ScanSegments.WriteDefaultIfMissing(SettingsStore.ScanSegmentsPath);


        // **THE FIRST SPOT LOAD WAITS FOR THE RADIO** (HM-DEC-118). It used to
        // run from here, before anything was connected, so RBN was filtered and
        // the skimmer watch scoped to whatever band was last remembered
        // (HM-DEC-024, HM-DEC-075). An empty panel asserts nothing and a
        // wrong-band panel asserts something false, which is the distinction
        // §0.0 exists to draw, and the cost of waiting is a second or two of
        // empty on a screen just opened.
        //
        // It is kicked off by `ReconnectOnStartupAsync` instead, which runs from
        // the window's Opened event and knows where the radio is. Asking the
        // radio from here stays rejected: it would put a serial read on the path
        // that builds the window.
        _ = ResolveProfileAsync();
        ApplyFeedTimers();
    }

    /// <summary>
    /// Write the canvas out, whatever else is happening (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// The arrangement already saves on every change, so this is the belt beside
    /// the braces: called as the window closes, because rebuilding a workspace by
    /// hand is the one loss the operator would actually feel.
    /// </remarks>
    public void KeepTheCanvas()
    {
        // **THERE IS NOTHING LEFT TO KEEP** (Tim, 2026-08-27: "I don't care when
        // it destroys. We're abandoning all of that."). The three workspaces hold
        // permanent panels, so a workspace cannot be rebuilt by hand and there is
        // no arrangement to save on the way out.
        //
        // The method survives its own body because `MainWindow` calls it as the
        // window closes, and a shutdown path is a poor place to discover a
        // rename.
    }



    /// <summary>The scanner, and the stop control §0.2.1 requires.</summary>
    public ScanViewModel Scan { get; }

    /// <summary>Calling CQ on a cycle, into a dummy load (HM-DEC-098).</summary>
    public AutoCallViewModel AutoCall { get; }

    /// <summary>The same object, for the scanner's predicate to read safely.</summary>
    private readonly AutoCallViewModel? _autoCall;

    /// <summary>
    /// Choose the line under the wordmark, avoiding last launch's
    /// (HM-DEC-039).
    /// </summary>
    /// <remarks>
    /// The chosen index is saved immediately rather than at shutdown, because
    /// an app that is killed rather than closed would otherwise show the same
    /// line forever — and this is meant to be a small surprise, not a fixture.
    /// </remarks>
    private void PickByline()
    {
        var picked = Bylines.Pick(_settings.LastBylineIndex);

        if (picked is not { } choice)
        {
            // No file, or nothing in it. There is simply no byline; a
            // placeholder would be worse than the silence.
            return;
        }

        Byline = choice.Line.Text;
        BylineSource = choice.Line.Source;

        _settings.LastBylineIndex = choice.Index;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// Assemble the live sources the operator has switched on (HM-DEC-024).
    /// </summary>
    /// <returns>The aggregate to poll.</returns>
    /// <remarks>
    /// <para>Order is preference order: the aggregate keeps the first version
    /// of a duplicate spot, and an activation carries far more meaning for a
    /// newcomer than the same station seen bare by a skimmer, so POTA and
    /// SOTA lead.</para>
    /// <para>RBN is left out entirely when the operator has not set a
    /// callsign. Its telnet login is the callsign — there is no anonymous
    /// access — and inventing one would be lying to the service on the
    /// operator's behalf. Settings says so next to the switch.</para>
    /// </remarks>
    private AggregateActivitySource BuildSources()
    {
        DisposeSources();

        var version = AboutViewModel.AppVersion;
        var callsign = _settings.Operator.Callsign?.Trim() ?? "";
        var owned = new List<IDisposable>();
        var sources = new List<IActivitySource>();

        var pota = new PotaActivitySource(version, callsign);
        sources.Add(pota);
        owned.Add(pota);

        var sota = new SotaActivitySource(version, callsign);
        sources.Add(sota);
        owned.Add(sota);

        if (callsign.Length > 0)
        {
            _rbn = new RbnActivitySource(callsign);
            _rbn.SpotParsed += OnRbnSpotParsed;
            sources.Add(_rbn);
            owned.Add(_rbn);
        }
        else
        {
            _rbn = null;
        }

        sources.Add(new FakeActivitySource());

        _ownedSources = owned.ToArray();

        var aggregate = new AggregateActivitySource(sources, _settings.IsSourceEnabled);
        aggregate.SetContext(BuildContext());
        return aggregate;
    }

    /// <summary>
    /// What the sources need to know about the operator and the band on
    /// screen.
    /// </summary>
    private ActivityContext BuildContext() => new()
    {
        BandName = SelectedBand.Band.Name,
        BandLowHz = SelectedBand.Band.LowHz,
        BandHighHz = SelectedBand.Band.HighHz,
        HomeDistrict = OperatorLocation.HomeDistrict(_settings.Operator.Location),
        HomeInNorthAmerica =
            OperatorLocation.IsNorthAmerica(_settings.Operator.GridSquare)
            || OperatorLocation.HomeDistrict(_settings.Operator.Location) is not null
            || CallsignRegions.Classify(_settings.Operator.Callsign).Region
                is CallsignRegion.UnitedStates or CallsignRegion.Canada,
    };

    private void DisposeSources()
    {
        foreach (var source in _ownedSources)
        {
            try
            {
                source.Dispose();
            }
            catch (Exception)
            {
                // Tearing down a feed is best-effort; never fatal (§8).
            }
        }

        _ownedSources = Array.Empty<IDisposable>();
    }

    /// <summary>
    /// Window visibility, pushed in by the view. The feed stops polling when
    /// nobody is looking and refreshes the moment the window comes back
    /// (HM-DEC-020) — free today against a fixture, and ordinary politeness
    /// once RBN and POTA are behind the same seam.
    /// </summary>
    /// <param name="visible">True when the window is on screen.</param>
    public void SetWindowVisible(bool visible)
    {
        if (visible == _windowVisible)
        {
            return;
        }

        _windowVisible = visible;
        ApplyFeedTimers();

        // CI-V is a slow bus shared with the radio's own transceive stream, and
        // a minimized window has no S-meter on screen to justify asking for one
        // four times a second. Same politeness the spot feeds observe
        // (HM-DEC-020, HM-DEC-050).
        if (_rigMonitor is not null)
        {
            _rigMonitor.IsWatching = visible;
        }

        // Nothing is watching a hidden window, and twenty-five frames a
        // second of synthesis for nobody is the same rudeness HM-DEC-020
        // named — here it is only the operator's own CPU being spent.
        if (_trainingSpectrum is not null)
        {
            if (visible)
            {
                _trainingSpectrum.Start();
            }
            else
            {
                _trainingSpectrum.Stop();
            }
        }

        if (visible)
        {
            _ = ReloadSpotsAsync("resume");
        }
    }

    /// <summary>Re-read feed settings after the Settings dialog closes.</summary>
    public void ApplyFeedTimers()
    {
        var minutes = _settings.SpotRefreshMinutes;
        var shouldRun = minutes > 0 && _windowVisible;

        _spotRefreshTimer.Stop();
        if (shouldRun)
        {
            _spotRefreshTimer.Interval = TimeSpan.FromMinutes(minutes);
            _spotRefreshTimer.Start();
        }

        // The age line keeps ticking whenever the window is up, even with
        // auto-refresh off: an operator who switched refreshing off still
        // needs to see the data getting old.
        if (_windowVisible)
        {
            if (!_ageTimer.IsEnabled)
            {
                _ageTimer.Start();
            }
        }
        else
        {
            _ageTimer.Stop();
        }

        AppEvents.SpotTimerChanged(_telemetry, shouldRun, minutes);
        UpdateSpotFreshness();
    }

    /// <summary>Show a neighborhood's story in the Explorer card.</summary>
    [RelayCommand]
    private void ShowNeighborhood(Neighborhood hood)
    {
        AppEvents.NeighborhoodClicked(_telemetry, hood.Name);
        StoryTitle = hood.Name;
        StoryBadge = hood.Vibe;

        // **THE CARD NAMES THE BLOCK AS WELL AS THE DIAL** (work instruction
        // 040). Where every signal is an audio tone above the dial, the dial
        // itself sounds dead — which is the correct behaviour of a correctly
        // tuned radio and has twice been read as a broken one.
        var where = hood.WhereTheSignalsAre();

        StoryBody = where.Length == 0
            ? hood.Blurb
            : hood.Blurb + Environment.NewLine + Environment.NewLine + where;

        StoryTuneHz = hood.JumpHz;
    }

    /// <summary>Show a mode's field-guide story in the Explorer card.</summary>
    [RelayCommand]
    private void ShowMode(ModeInfo mode)
    {
        AppEvents.ModeCardOpened(_telemetry, mode.Name);
        StoryTitle = $"{mode.Name} · {mode.Tagline}";
        StoryBadge = mode.Difficulty;
        StoryBody = $"{mode.Why} Sounds like: {mode.Sound}. Learn its waterfall "
            + "fingerprint and the void stops being static.";
        StoryTuneHz = mode.LivesAt40mHz ?? SelectedBand.Band.JumpHz;
    }

    /// <summary>
    /// Switch the happening-now list between its two questions (HM-DEC-057).
    /// </summary>
    /// <param name="lensName">The lens, as its enum name.</param>
    /// <remarks>
    /// LEAVING "WHAT'S NEW" IS WHAT MOVES THE WATERMARK, not arriving at it.
    /// Moving it on arrival would empty the list the instant somebody opened it,
    /// which is the one thing a delta must not do. So the list stays still while
    /// they are reading it, and is a fresh delta the next time they come back.
    /// </remarks>
    [RelayCommand]
    private void SelectLens(string lensName)
    {
        if (!Enum.TryParse<SpotLens>(lensName, out var lens) || lens == _lens)
        {
            return;
        }

        if (_lens == SpotLens.WhatsNew)
        {
            MarkLookedAtWhatsNew();
        }

        SetLens(lens);
        _lensChosenByOperator = true;

        _settings.SpotLens = lens.ToString();
        SettingsStore.Save(_settings);

        AppEvents.SpotLensChosen(_telemetry, lens.ToString());
        ApplyLens(DateTime.UtcNow);
    }

    /// <summary>
    /// Switch a mode family on or off in the happening-now list (HM-DEC-061).
    /// </summary>
    /// <param name="familyName">The family, as its enum name.</param>
    /// <remarks>
    /// THE CHIPS FILTER AND THEY NEVER DELETE. This is one more view over the
    /// same store the lenses read, so a chip changes what is drawn and changes
    /// nothing about what Hamlet holds.
    /// </remarks>
    [RelayCommand]
    private void ToggleFamily(string familyName)
    {
        if (!Enum.TryParse<ModeFamily>(familyName, out var family)
            || !FamilyFilter.Offered.Contains(family))
        {
            return;
        }

        if (!_families.Remove(family))
        {
            _families.Add(family);
        }

        _settings.SpotFamilies = _families.Select(f => f.ToString()).ToList();
        SettingsStore.Save(_settings);

        AppEvents.SpotFamilyToggled(
            _telemetry, family.ToString(), _families.Contains(family));

        ApplyLens(DateTime.UtcNow);
    }

    /// <summary>Redraw the chips with their counts.</summary>
    /// <remarks>
    /// The count is over everything the lens has rather than over what survives
    /// the filter, which is the whole teaching: somebody who filters to Morse
    /// and still sees forty-one voice stations learns the band is full of people
    /// they could talk to.
    /// </remarks>
    private void RebuildFamilyChips(IEnumerable<ActivitySpot> beforeFiltering)
    {
        FamilyChips.Clear();

        foreach (var chip in FamilyFilter.Chips(beforeFiltering, _families))
        {
            FamilyChips.Add(new FamilyChipViewModel(chip));
        }
    }

    /// <summary>Record that the operator has now seen what was new.</summary>
    private void MarkLookedAtWhatsNew()
    {
        _settings.SpotsLastLookedUtc = DateTime.UtcNow;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// Save where the dial is, or un-save it (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// SAVING CAPTURES CONTEXT AUTOMATICALLY. The frequency, the mode, the band
    /// and what the map says lives there, so a favorite reads "14.074, where the
    /// digital modes gather" with nothing typed. The radio's own memory channels
    /// are numbered slots whose meaning you have to remember, which is the
    /// problem rather than the answer.
    /// </remarks>
    [RelayCommand]
    private void ToggleFavorite()
    {
        var existing = RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz);

        if (existing is not null)
        {
            Favorites.Remove(existing);
            AppEvents.FavoriteRemoved(_telemetry, existing.BandName);
        }
        else
        {
            if (Favorites.Count >= RadioEngine.Explore.Favorites.Maximum)
            {
                StatusText = "That is as many favorites as Hamlet keeps. Remove one "
                           + "from Radio, Manage favorites, and this will save.";
                return;
            }

            var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));

            var favorite = RadioEngine.Explore.Favorites.From(
                FrequencyHz, RigModeText, here, DateTime.UtcNow);

            Favorites.Add(favorite);
            AppEvents.FavoriteSaved(_telemetry, favorite.BandName);
            Narrate($"Saved as \"{favorite.Name}\".");
        }

        PersistFavorites();
        UpdateFavoriteState();
    }

    /// <summary>Tune to a saved frequency.</summary>
    [RelayCommand]
    private void TuneToFavorite(Favorite? favorite)
    {
        if (favorite is null)
        {
            return;
        }

        AppEvents.FavoriteTuned(_telemetry, favorite.BandName);
        TuneTo(favorite.FrequencyHz);
    }

    /// <summary>Rename, reorder and delete favorites.</summary>
    [RelayCommand]
    private void ManageFavorites()
    {
        var window = new Views.FavoritesWindow
        {
            DataContext = new FavoritesViewModel(
                Favorites, PersistFavorites, Recent, StarRecent, ForgetRecent),
        };

        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } owner)
        {
            window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }

    /// <summary>Write the list back to settings.json.</summary>
    private void PersistFavorites()
    {
        _settings.Favorites = Favorites
            .Select(f => new SavedFavorite
            {
                FrequencyHz = f.FrequencyHz,
                Name = f.Name,
                Mode = f.Mode,
                BandName = f.BandName,
                Neighborhood = f.Neighborhood,
                SavedUtc = f.SavedUtc,
                Note = f.Note,
            })
            .ToList();

        SettingsStore.Save(_settings);
        RebuildMenus();
        UpdateFavoriteState();
    }

    partial void OnSelectedFavoriteChanged(Favorite? value)
    {
        if (value is null)
        {
            return;
        }

        var picked = value;
        SelectedFavorite = null;
        TuneToFavorite(picked);
    }

    /// <summary>Light or unlight the star for where the dial is now.</summary>
    private void UpdateFavoriteState()
    {
        var here = RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz);

        IsFavorite = here is not null;
        FavoriteLabel = RadioEngine.Explore.Favorites.StarLabel(here);
        FavoriteHere = RadioEngine.Explore.Favorites.NameHere(here);

        // **WRITTEN WITHOUT GOING BACK THROUGH THE SETTER**, which would save
        // the file on every tune and, worse, write the previous frequency's note
        // onto this one on the way past.
        _favoriteNote = here?.Note ?? "";
        OnPropertyChanged(nameof(FavoriteNote));
    }

    /// <summary>
    /// Why this favorite, in the operator's own words (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// <para>**THE NAME SAYS WHERE AND THIS SAYS WHY.** Hamlet can name the block
    /// from the map and it cannot know that this is the net that meets on
    /// Tuesdays. Nothing derives this, suggests it or fills it in: it is the one
    /// part of a favorite that is entirely the operator's (§0.0).</para>
    /// <para>It sits in the strip beside the name, which is where a favorite
    /// already speaks (HM-DEC-070), and it is there from the moment the star
    /// lights rather than behind a management window — a box somebody has to go
    /// and find is a box that never gets written. Empty is the ordinary state and
    /// stays empty.</para>
    /// </remarks>
    public string FavoriteNote
    {
        get => _favoriteNote;

        set
        {
            var text = (value ?? "").Trim();

            if (text == _favoriteNote)
            {
                return;
            }

            _favoriteNote = text;
            OnPropertyChanged();

            if (RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz)
                is not { } here)
            {
                // No favorite here to carry it. Nothing is saved and nothing is
                // lost, because there is nothing yet for a note to belong to.
                return;
            }

            var index = Favorites.IndexOf(here);
            Favorites[index] = here with { Note = text };

            PersistFavorites();
        }
    }

    private string _favoriteNote = "";

    /// <summary>Tune the rig (and the whole UI) to a target — the payoff
    /// click on every story and spot.</summary>
    [RelayCommand]
    private void TuneTo(long hz)
    {
        AppEvents.TuneRequested(_telemetry, hz, "story_or_spot");
        var arrivedOn = MarkActedOn(hz);
        var band = HfBands.BandFor(hz);
        if (band is not null && band.Name != SelectedBand.Band.Name)
        {
            SelectedBand = Bands.First(b => b.Band.Name == band.Name);
        }

        FrequencyHz = hz;

        // AFTER THE MOVE, NEVER BEFORE IT. Switching bands can land the dial on
        // that band's own default on the way past, and the frequency handler
        // expires an arrival that no longer matches where the dial is. Setting
        // this first meant the callsign was thrown away in transit
        // (HM-DEC-072).
        _arrivedOnStation = arrivedOn;
        _arrivedOnHz = arrivedOn.Length > 0 ? hz : -1;
    }

    partial void OnSelectedBandChanged(BandButtonViewModel value)
    {
        Neighborhoods = NeighborhoodPlan.WithEdges(value.Band);

        // A band change is a fresh start rather than a continuation, and
        // somebody who took the wheel on 40 m did not mean to keep it forever
        // (HM-DEC-056).
        _modeFollow = _modeFollow.Rearmed();

        // The receive side is re-armed with it, for the same reason: a band
        // change is a fresh start, and the noise blanker he switched on to get
        // through an electric fence on 40 m says nothing about 20 (HM-DEC-056).
        _receiverMemory = _receiverMemory.Rearmed();
        _conditionsSetForBlockHz = null;

        ModeFollowSuspended = false;
        ScheduleModeFollow();

        _settings.LastBand = value.Band.Name;
        SettingsStore.Save(_settings);
        AppEvents.BandChanged(_telemetry, value.Band.Name);

        // The list, the dots, the lead card and the conditions line are all
        // about the band on screen, so changing band re-asks rather than
        // leaving the previous band's answers up.
        _ = ReloadSpotsAsync("band_changed");

        // The training radio synthesises one band at a time, and its signals
        // are placed against that band's neighborhood map.
        if (_trainingSpectrum is not null)
        {
            StartTrainingSpectrum();
        }
    }

    /// <summary>
    /// Tune to a dot on the neighborhood map (HM-DEC-023). Separate from
    /// <see cref="TuneToCommand"/> only so the telemetry can tell a map click
    /// from a card click.
    /// </summary>
    [RelayCommand]
    private void TuneToDot(long hz)
    {
        AppEvents.MapDotTuned(_telemetry, hz);
        TuneTo(hz);
    }

    /// <summary>
    /// Tune to a spot marker on the dial tape. The same spots the map draws,
    /// separated here only so the telemetry can tell the two gestures apart.
    /// </summary>
    [RelayCommand]
    private void TuneToTapeMarker(long hz)
    {
        AppEvents.TapeMarkerTuned(_telemetry, hz);
        TuneTo(hz);
    }

    partial void OnSelectedPortChanged(string value)
    {
        _settings.LastPort = value;
        SettingsStore.Save(_settings);
    }

    partial void OnTransmitExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Transmit, value);

    partial void OnPhrasebookExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Phrasebook, value);

    partial void OnMapExpandedChanged(bool value) => PersistPanel(PanelKeys.Map, value);

    partial void OnTapeExpandedChanged(bool value) => PersistPanel(PanelKeys.Tape, value);

    private void OnClockTick(object? sender, EventArgs e)
        => _ = QueryTheClockAsync();

    /// <summary>Ask once, and record whatever comes back.</summary>
    /// <remarks>
    /// <para>**OFF THE UI THREAD, AND A DEAD SERVER NEVER STALLS THE TAB.** The
    /// query has its own timeout and every failure inside it returns unknown, so
    /// nothing here can throw and nothing here can block (§8).</para>
    /// <para>**A FAILED QUERY LEAVES THE OFFSET UNKNOWN RATHER THAN ZERO**
    /// (HM-DEC-009). Unknown has its own words on the strip and it stops slots
    /// being cut, which is the honest behaviour: a clock nobody could check is
    /// not a clock that is right.</para>
    /// <para>**AND A FAILURE DOES NOT ERASE AN EARLIER GOOD READING.** A network
    /// that drops out for one poll has not made the last measurement untrue; it
    /// has only made it older, and the age is already displayed.</para>
    /// </remarks>
    private async Task QueryTheClockAsync()
    {
        if (_clockQueryRunning)
        {
            return;
        }

        _clockQueryRunning = true;

        try
        {
            var measured = await SntpClock
                .QueryAsync(DateTime.UtcNow)
                .ConfigureAwait(true);

            if (measured.IsKnown || !ClockOffset.IsKnown)
            {
                ClockOffset = measured;
            }
        }
        finally
        {
            _clockQueryRunning = false;
        }
    }

    partial void OnWaterfallExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Waterfall, value);

    partial void OnDigitalWaterfallExpandedChanged(bool value)
        => PersistPanel(PanelKeys.DigitalWaterfall, value);

    partial void OnDigitalDecodedExpandedChanged(bool value)
        => PersistPanel(PanelKeys.DigitalDecoded, value);

    partial void OnScanExpandedChanged(bool value) => PersistPanel(PanelKeys.Scan, value);

    partial void OnAutoCallExpandedChanged(bool value)
        => PersistPanel(PanelKeys.AutoCall, value);

    partial void OnTerminalExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Terminal, value);

    partial void OnStoryExpandedChanged(bool value) => PersistPanel(PanelKeys.Story, value);

    partial void OnGuideExpandedChanged(bool value) => PersistPanel(PanelKeys.Guide, value);

    partial void OnReceiveHelpExpandedChanged(bool value)
        => PersistPanel(PanelKeys.ReceiveHelp, value);

    partial void OnSpotsExpandedChanged(bool value) => PersistPanel(PanelKeys.Spots, value);

    partial void OnLeadExpandedChanged(bool value) => PersistPanel(PanelKeys.Lead, value);

    partial void OnContactExpandedChanged(bool value) => PersistPanel(PanelKeys.Contact, value);

    private void PersistPanel(string key, bool expanded)
    {
        _settings.SetPanelExpanded(key, expanded);
        SettingsStore.Save(_settings);
        AppEvents.PanelToggled(_telemetry, key, expanded);
    }

    /// <summary>Reload the happening-now feed by hand. Always works, whatever
    /// the interval setting says, and resets the timer (HM-DEC-020).</summary>
    [RelayCommand]
    private async Task RefreshSpotsAsync()
    {
        await ReloadSpotsAsync("manual");
        ApplyFeedTimers();
    }

    /// <summary>Open the settings dialog.</summary>
    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        AppEvents.SettingsOpened(_telemetry);
        var window = new Views.SettingsWindow
        {
            DataContext = new SettingsViewModel(_settings, _telemetry),
        };
        await window.ShowDialog(desktop.MainWindow);

        // **THE OTHER DRIVE CONTROL MAY HAVE MOVED WHILE THE DIALOG WAS UP**
        // (work instruction 269, task 2). Two controls over one setting have to
        // agree in both directions, and the tab's one is not watching the
        // settings object. Setting the property rather than the field is
        // deliberate: it is a no-op when nothing changed, because the generated
        // setter compares first.
        TransmitDrivePercent = TransmitDrive.PercentFor(_settings.TransmitDrivePeak);
        OnPropertyChanged(nameof(TransmitDriveNote));

        // Source switches and the callsign may both have changed while the
        // dialog was open, and the callsign is RBN's login, so the sources are
        // rebuilt rather than reconfigured.
        _activitySource = BuildSources();
        _ = ReloadSpotsAsync("settings");

        // The callsign, the class or the grid may have changed while the
        // dialog was open, so the lazy resolve gets another chance
        // (HM-DEC-028, HM-DEC-037).
        OnPropertyChanged(nameof(GridProvenance));
        UpdateBandCharacter(DateTime.UtcNow);
        UpdateSpotDistances();
        _ = ResolveProfileAsync();
        UpdatePrivileges();

        // The interval may have changed while the dialog was open.
        ApplyFeedTimers();
    }

    /// <summary>Open %AppData%\Hamlet in the file browser.</summary>
    [RelayCommand]
    private void OpenDataFolder() => SettingsStore.OpenDataFolder();

    /// <summary>Show the About window.</summary>
    [RelayCommand]
    private async Task OpenAboutAsync()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        AppEvents.AboutOpened(_telemetry);
        var window = new Views.AboutWindow
        {
            DataContext = new AboutViewModel(_settings, _telemetry),
        };
        await window.ShowDialog(desktop.MainWindow);
    }

    /// <summary>
    /// Point the waterfall at a freshly synthesised band.
    /// </summary>
    /// <remarks>
    /// Rebuilt per band rather than retuned, because the signals are placed
    /// against that band's own neighborhood map — practicing on 20 m has to
    /// teach 20 m (HM-DEC-026).
    /// </remarks>
    /// <summary>
    /// Listen to the radio's own spectrum scope (HM-DEC-062, HM-DEC-005).
    /// </summary>
    /// <remarks>
    /// <para>THE RADIO COMPUTES THIS AND HAMLET DOES NOT. The panadapter is
    /// already running and already band-wide, so the app never computes a
    /// wideband transform the radio has finished.</para>
    /// <para>Listening costs the bus nothing: the radio pushes these once its
    /// own output is switched on, so nothing is polled for and the existing loop
    /// cannot be starved (HM-DEC-050).</para>
    /// <para>Nothing here turns the scope on. That is a write, and Hamlet reads
    /// the two settings and says what is missing instead, because the stream
    /// also needs two radio menu settings there is no command for at all.</para>
    /// </remarks>
    private void StartRigSpectrum(IRig rig)
    {
        StopTrainingSpectrum();

        if (rig is not Ic7300Rig radio || !rig.Capabilities.HasSpectrumScope)
        {
            return;
        }

        _rigSpectrum = new RigSpectrumSource(radio);
        _rigSpectrum.Start();

        SpectrumSource = _rigSpectrum;
        AppEvents.SpectrumSourceChanged(
            _telemetry, "rig", SelectedBand.Band.Name, simulated: false);

        // **NOTHING HERE TURNS THE SCOPE ON, WHICH IS WHAT HM-DEC-062 ALREADY
        // SAID.** `_ = AskForTheSpectrumAsync(radio)` stood on this line from
        // 8c2abf3, version 1.8.0, and wrote `27 11` at every connect against a
        // standing ruling that this path is reads only. Taking it out restores
        // that ruling rather than departing from one.
        //
        // The arithmetic is why it matters rather than being tidy. A waveform
        // sweep is 475 points in 11 parts, on the order of six hundred bytes,
        // and 115200 8N1 carries about eleven and a half thousand bytes a
        // second. Nineteen sweeps a second is the whole cable, and the dial's own
        // announcements share it. HM-OPEN-042 then found the readback could not
        // confirm this write, so Hamlet has been reporting it refused without
        // knowing, and it may have been succeeding at every connect since 1.8.0.
        //
        // Reading `27 10` and `27 11` to say what is on stays. That is the read
        // HM-DEC-062 allows, and it is what the panel needs to explain itself.
    }

    /// <summary>
    /// How the conversation with the radio is going, in one sentence (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **THIS IS THE LINE THAT WOULD HAVE SAVED TWO BUILDS.** The operator turned
    /// his dial and watched Hamlet follow thirty seconds later, and the
    /// application said nothing about it: the frequency was drawn confidently
    /// four times a second while being a minute old. Everything needed to say so
    /// was already in the app and nothing assembled it.
    /// </remarks>
    public string LinkCheckLine => Link().Headline;

    /// <summary>The check, or the one a test handed over.</summary>
    /// <remarks>
    /// **THE SAME IDIOM AS <see cref="NarrateForTests"/>.** Composing this from a
    /// real rig needs a radio, a link and a poll history, for a question about which
    /// of two strings the strip draws. Nothing in `src/` calls the setter.
    /// </remarks>
    /// <returns>The check the strip is showing.</returns>
    private LinkCheck Link()
        => _linkCheckForTests ?? LinkSelfCheck.Describe(
            (_rig as Ic7300Rig)?.Link, RigState, DateTime.UtcNow, IsConnected);

    private LinkCheck? _linkCheckForTests;

    /// <summary>Show one link check, for a test.</summary>
    /// <param name="check">What the strip should be drawing.</param>
    internal void UseLinkCheckForTests(LinkCheck check)
    {
        _linkCheckForTests = check;

        OnPropertyChanged(nameof(LinkCheckLine));
        OnPropertyChanged(nameof(LinkCheckDetail));
        OnPropertyChanged(nameof(LinkCheckConcern));
        OnPropertyChanged(nameof(HasLinkCheck));
        OnPropertyChanged(nameof(HasLinkCheckConcern));
    }

    /// <summary>The numbers behind that sentence, for the diagnostics screen.</summary>
    public string LinkCheckDetail => Link().Detail;

    /// <summary>
    /// The part of the link check that is something gone wrong, or "".
    /// </summary>
    /// <remarks>
    /// <para>**THE PARAGRAPH THREE UNITS LOOKED PAST** (work instruction 283). This
    /// line is drawn in the top strip, under the frequency readout, permanently and
    /// on every tab — **not on the status bar**, which is where units 280, 281 and
    /// 282 were all looking. Its longest branch is 304 characters and it is the
    /// branch this operator's own radio takes, because `CivTransceive` is off and
    /// HM-DEC-138 measured 5,499 frames in sixty-one seconds with
    /// `inboundTransceive` zero.</para>
    /// <para>**A FAULT SPEAKS UNASKED AND THE REST WAITS TO BE ASKED.** A stale
    /// frequency and a frequency nobody has heard are both this class's own reason
    /// for existing; a radio that is being polled instead of announcing is Hamlet
    /// describing itself working.</para>
    /// </remarks>
    public string LinkCheckConcern => Link().Concern;

    /// <summary>True while something about the link has to be read unhovered.</summary>
    public bool HasLinkCheckConcern => LinkCheckConcern.Length > 0;

    /// <summary>True while there is a link check worth showing.</summary>
    public bool HasLinkCheck => LinkCheckLine.Length > 0;

    /// <summary>
    /// How old the frequency on screen is, said rather than counted, or "".
    /// </summary>
    /// <remarks>
    /// **A PROVENANCE LABEL CARRIES ITS AGE** (HM-DEC-111). That ruling came from
    /// a capture sidecar asserting a freshness it did not have, and the rig
    /// display has been doing the same thing to the one number every other
    /// surface trusts. Empty while the reading is current, because a fresh value
    /// needs no apology and a caption on every screen is a caption nobody reads.
    /// </remarks>
    public string FrequencyAgeNote
    {
        get
        {
            if (!IsConnected)
            {
                return "";
            }

            var value = RigState[RigField.Frequency];

            if (!value.IsKnown)
            {
                return "";
            }

            return value.Age(DateTime.UtcNow) is { } age
                   && age >= LinkSelfCheck.FrequencyIsOldAfter
                ? "where the radio was a moment ago, not where it is now"
                : "";
        }
    }

    /// <summary>True while that note has something to say.</summary>
    public bool HasFrequencyAgeNote => FrequencyAgeNote.Length > 0;

    /// <summary>
    /// What became of the scope's data output, as far as Hamlet knows.
    /// </summary>
    /// <remarks>
    /// **FALSE FOREVER NOW, AND KEPT RATHER THAN THREADED OUT.** Hamlet does not
    /// ask the radio to send its spectrum any more, so it cannot be refused one:
    /// `AskForTheSpectrumAsync`, the five second wait it needed before writing,
    /// and the undo that put the setting back all went with the write. The
    /// readiness check still takes the flag, and passing a false it can rely on
    /// is cheaper to read than a signature change that would have to be undone
    /// the day the write is ruled back in (HM-DEC-062, HM-DEC-092).
    /// </remarks>
    private const bool ScopeWriteRefused = false;

    private void StartTrainingSpectrum()
    {
        StopTrainingSpectrum();

        _trainingSpectrum = new TrainingSpectrumSource(
            SelectedBand.Band, callsign: _settings.Operator.Callsign);
        _trainingSpectrum.Start();

        SpectrumSource = _trainingSpectrum;
        AppEvents.SpectrumSourceChanged(
            _telemetry, "training", SelectedBand.Band.Name, simulated: true);
    }

    /// <summary>
    /// Wipe the terminal, and nothing else.
    /// </summary>
    /// <remarks>
    /// <para>WHAT THIS DOES NOT TOUCH IS THE POINT (HM-DEC-051). Tuning around
    /// leaves a pile of half-decoded garbage above whatever is arriving now, and
    /// there was no way to start fresh. So this clears what is displayed, and
    /// stops there.</para>
    /// <para>The decoder keeps running. It keeps its speed estimate, its
    /// adapted noise floor and its tone tracking, because those took real
    /// seconds of signal to arrive at and throwing them away mid-decode is
    /// exactly what nobody wants while chasing a marginal one. A clear that
    /// quietly reset the decoder would look like the app losing the signal at
    /// the moment the operator asked for a tidy screen.</para>
    /// </remarks>
    [RelayCommand]
    private void ClearTerminal()
    {
        _clearedUtc = DateTime.UtcNow;

        Transcript.Clear();
        OnPropertyChanged(nameof(TerminalSummary));
    }

    /// <summary>
    /// Open the screen that says what the radio is doing.
    /// </summary>
    /// <remarks>
    /// Under Tools rather than buried, because the moment somebody needs it is
    /// the moment something is wrong and they are already frustrated
    /// (HM-DEC-050).
    /// </remarks>
    [RelayCommand]
    private void OpenRigDiagnostics()
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is null)
        {
            return;
        }

        var window = new Views.RigDiagnosticsWindow
        {
            DataContext = new RigDiagnosticsViewModel(
                _rigMonitor, RigState, (_rig as Ic7300Rig)?.Link,
                _modeFollowNote),
        };

        AppEvents.RigDiagnosticsOpened(_telemetry, RigState.KnownCount);
        window.ShowDialog(owner);
    }

    /// <summary>
    /// Begin keeping track of what the radio is doing.
    /// </summary>
    /// <remarks>
    /// The monitor polls on its own thread and raises state changes from it, so
    /// everything it hands over is marshalled onto the UI thread here rather
    /// than each surface remembering to.
    /// </remarks>
    private void StartRigMonitor(IRig rig)
    {
        StopRigMonitor();

        _rigMonitor = new RigStateMonitor(rig);
        _rigMonitor.StateChanged += OnRigStateChanged;
        _rigMonitor.IsWatching = _windowVisible;
        _rigMonitor.Start();
    }

    private void StopRigMonitor()
    {
        if (_rigMonitor is null)
        {
            return;
        }

        _rigMonitor.StateChanged -= OnRigStateChanged;
        _rigMonitor.Dispose();
        _rigMonitor = null;

        // Back to knowing nothing, rather than leaving the last radio's
        // readings on screen as though they were this one's (§0.0).
        ApplyRigState(RigState.Empty);
    }

    /// <summary>
    /// THE SEAM WHERE RIG STATE ENTERS THE UI, AND THE ONLY ONE (HM-DEC-078).
    /// </summary>
    /// <remarks>
    /// <para>The monitor raises this from the serial read loop, so everything
    /// downstream would otherwise be touching bindable properties from a
    /// background thread. Marshalling here rather than at each consumer means a
    /// property added next month is safe without anybody remembering, and there
    /// is one line to read to know that it is.</para>
    /// <para>It fires every poll cycle whether anything changed or not, four
    /// times a second, which is why everything it reaches has to be cheap and
    /// idempotent. A consumer that rebuilds controls on this cadence destroys
    /// them out from under the operator's finger, and that is exactly what
    /// killed two live attempts (HM-DEC-078).</para>
    /// </remarks>
    private void OnRigStateChanged(object? sender, RigStateChangedEventArgs e)
        => Dispatcher.UIThread.Post(() => ApplyRigState(e.State));

    /// <summary>
    /// Push what the radio said onto the surfaces that show it.
    /// </summary>
    /// <remarks>
    /// Every one of these is empty or null when the value is not known, and
    /// none of them substitutes a default. That is the whole point of the model
    /// underneath (HM-DEC-050).
    /// </remarks>
    internal void ApplyRigState(RigState state)
    {
        // EVERYTHING BELOW TOUCHES BINDABLE STATE, so arriving here off the UI
        // thread would mean notifications the framework may silently drop
        // (HM-DEC-078). The one caller posts; this is what keeps a second caller
        // from quietly not doing so.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ApplyRigState(state));
            return;
        }

        _rigStateApplied = state;

        NoticeOperatorModeChange(state);

        // **THE SWEPT FREQUENCY CORRECTS THE DISPLAY** (HM-DEC-109). A broadcast
        // missed at startup used to leave the band on screen wrong until
        // somebody next turned the dial, and the band scopes what RBN is
        // filtered to and what the skimmer watch listens for. Sweeping the model
        // and leaving the screen alone would fix the half nobody looks at.
        //
        // **THE GUARD IS THE POINT, AND IT HAD A HOLE THE WIDTH OF THE WRITE.**
        // A reading that arrived while the operator's own tune was still queued
        // would drag the dial back to where the radio has not got to yet, which
        // is the app taking the knob out of his hand (§0.2.1).
        //
        // `_rigSendPending` covered the queue and stopped at the send: it is
        // cleared before the write is awaited, so from that instant until a
        // reading taken *after* the write comes back, the model still holds the
        // frequency the radio was on before. This block then applied it, and the
        // display snapped back to where the operator had just left. He watched it
        // do that on every tune from the app, and hold for about thirty seconds,
        // which is the session sweep that used to be the frequency's only
        // refresh (ad93fb4).
        //
        // **SO THE TEST IS THE READING'S OWN AGE AGAINST THE WRITE, AND IT IS
        // BOUNDED BY THE WRITE RATHER THAN BY A TIMER.** A reading taken before
        // Hamlet moved the dial cannot say where the dial is now, whatever else
        // is true. One taken after it may, including when it disagrees, because
        // the radio is always right about its own frequency (§0.0).
        if (!_rigSendPending
            && !_writeInFlight
            && !_updatingFromRig
            && state[RigField.Frequency] is { IsKnown: true, Number: { } swept }
            && (long)swept != FrequencyHz
            && IsAfterOurOwnTune(state[RigField.Frequency]))
        {
            // **A FREQUENCY THAT GOES BACKWARDS IS THE SIGNATURE OF THIS BUG.**
            // Returning to a value the display held moments ago, right after a
            // tune, is not an ordinary observation, and nothing anywhere said so
            // while it was happening on every click for two builds.
            if (DialGuard.WouldGoBackwards((long)swept, _tunedFromHz, _tunedToHz))
            {
                AppEvents.FrequencyWentBackwards(
                    _telemetry, (long)swept, _tunedToHz ?? 0,
                    state[RigField.Frequency].Source,
                    _tunedAtUtc is { } when
                        ? (DateTime.UtcNow - when).TotalSeconds
                        : null);
            }

            ApplyRigFrequency((long)swept);
        }
        // **THE VARIANT COMES WITH THE MODE** (work instruction 041, task 3).
        // This read RigField.Mode alone, so a radio Hamlet knew was in USB-D
        // showed as USB on the readout while the diagnostics window said
        // `Data mode: on` from the same poll.
        RigModeText = state.ModeWithVariant ?? "";
        RigFilterText = state[RigField.FilterSelection] is { IsKnown: true } filter
            ? filter.Text
            : "";

        SMeterLevel = state.SMeterFraction;

        FilterBandwidthText = state[RigField.FilterBandwidth] is { IsKnown: true } width
            ? width.Text
            : "";

        // **THE RECEIVER'S FRONT END, WHERE HE IS ALREADY LOOKING** (HM-DEC-091).
        // Hamlet read all three of these from the radio on the evening it could
        // not hear anything and showed him none of them: he found `Overflow:
        // overloading` in a text file the next day. A setting standing between
        // the operator and a contact, which the app already knows about and does
        // not mention, is squarely in the way of what this application is for.
        var overflow = state[RigField.Overflow];
        var preamp = state[RigField.Preamp];
        var attenuator = state[RigField.Attenuator];

        FrontEndIsOverloading = overflow is { IsKnown: true, Number: 1 };

        // **THE RADIO'S OWN WORD FOR BOTH OF THESE IS "off"**, so the chip read
        // `off · off` and said that two things were off without saying which two.
        // A reading nobody can interpret is the same failure as a reading nobody
        // can find, so the label is composed here from the number rather than
        // taken from the radio's text.
        PreampText = PreampLabel(preamp.IsKnown ? (int?)preamp.Number : null);
        AttenuatorText = AttenuatorLabel(
            attenuator.IsKnown ? (int?)attenuator.Number : null);

        FrontEndText = FrontEndTextFor(
            FrontEndIsOverloading, PreampText, AttenuatorText);

        // The preamp is the control to reach for first, and the attenuator only
        // once it is already off: advice about a knob that is already in the
        // right position is noise.
        PreampIsOn = preamp is { IsKnown: true } && preamp.Number is 1 or 2;

        // **THE THREE THE RULING NAMED, AND ONLY WHEN THEY ARE IN THE WAY.** The
        // filter is mentioned on a measurement rather than on a width: a
        // competing station the survey actually found is a fact, and asserting
        // that some width is too wide for a signal Hamlet has not measured would
        // be a judgement nobody has ruled (§0.0).
        ReceiveObstructionText = string.Join(
            " ",
            ReceiveObstructions.For(
                state,
                state.Mode is { } inMode && CivValues.IsCw(inMode),
                _decoder?.Report.Competitor is not null)
                .Select(one => one.Says));

        OnPropertyChanged(nameof(RigState));
        OnPropertyChanged(nameof(TerminalSummary));

        // **THE MODE IS ONE OF THE FIVE THE READINESS LINE ORDERS** (unit 228),
        // and it is the one that arrives from the radio rather than from
        // anything the operator did, so nothing else would re-ask it.
        OnPropertyChanged(nameof(DigitalReadinessLine));
        OnPropertyChanged(nameof(HasDigitalReadiness));

        // The link check and the age caption both read the clock, so they are
        // re-asked on the same beat as everything else here (HM-DEC-078).
        OnPropertyChanged(nameof(LinkCheckLine));
        OnPropertyChanged(nameof(LinkCheckDetail));
        OnPropertyChanged(nameof(HasLinkCheck));
        OnPropertyChanged(nameof(LinkCheckConcern));
        OnPropertyChanged(nameof(HasLinkCheckConcern));
        OnPropertyChanged(nameof(FrequencyAgeNote));
        OnPropertyChanged(nameof(HasFrequencyAgeNote));

        // A waterfall that sat empty without saying why would be the app looking
        // broken while the answer was four menu screens away (HM-DEC-062). The
        // sweep count goes in because settings that read as on and a waterfall
        // that stays blank is the case somebody actually sits and stares at
        // (HM-DEC-067).
        var scope = ScopeReadiness.Check(
            _rig?.Capabilities, state, _rigSpectrum?.SweepCount ?? -1,
            (_rig as Ic7300Rig)?.Link, ScopeWriteRefused);

        ScopeNote = _rig is null || _rig.IsSimulated || scope.IsReady
            ? ""
            : string.IsNullOrEmpty(scope.WhereToLook)
                ? scope.Detail
                : $"{scope.Detail} {scope.WhereToLook}";

        OnPropertyChanged(nameof(WaterfallSummary));
        OnPropertyChanged(nameof(ScopeStages));
        OnPropertyChanged(nameof(HasScopeStages));
        OnPropertyChanged(nameof(ScopeRejection));
        OnPropertyChanged(nameof(HasScopeRejection));
        OnPropertyChanged(nameof(ScopeCounts));
        OnPropertyChanged(nameof(ScopeIsFlowing));

        // The break-in setting and the mode both live in here, and both decide
        // whether a send would reach the air. So the panel re-asks whenever the
        // radio says anything, and somebody reads "break-in is off" before they
        // press rather than after (HM-DEC-059).
        Transmit.Refresh();
        ReceiveHelp.Refresh();
    }

    /// <summary>
    /// A mode Hamlet did not ask for is the operator's own hand, so it stands
    /// down until the next band change.
    /// </summary>
    /// <remarks>
    /// SUSPENDED IS A VISIBLE STATE AND NEVER A SILENT ONE (HM-DEC-056). An app
    /// that quietly stopped doing a thing it had been doing is worse than one
    /// that never did it, because the operator has no way to tell whether it is
    /// standing down or broken. So it says so, once, in the status line.
    /// </remarks>
    private void NoticeOperatorModeChange(RigState state)
    {
        if (_settingModeOurselves
            || _modeFollow.Suspended
            || !_modeFollow.Enabled
            || state[RigField.Mode] is not { IsKnown: true } mode)
        {
            return;
        }

        var was = _lastKnownMode;
        _lastKnownMode = (CivMode)(int)mode.Number!.Value;

        // The first reading of a session is not a change; it is the answer to
        // Hamlet asking what mode the radio was already in.
        if (was is null || was == _lastKnownMode)
        {
            return;
        }

        _modeFollow = _modeFollow.SuspendedByOperator();
        ModeFollowSuspended = true;
        Narrate($"You set the radio to {mode.Text}, so Hamlet will leave the "
                + "mode alone until you next change band.");
    }

    /// <summary>
    /// True while the operator is driving the mode and Hamlet is not.
    /// </summary>
    /// <remarks>
    /// Shown on screen rather than kept in a field, because the operator always
    /// has to know who is driving (HM-DEC-056).
    /// </remarks>
    [ObservableProperty]
    private bool _modeFollowSuspended;

    /// <summary>
    /// Start listening, and decoding what is heard.
    /// </summary>
    /// <remarks>
    /// <para>The training radio makes its own Morse, so somebody with no
    /// hardware at all still gets a working terminal (HM-DEC-026). A real radio
    /// gets whichever capture device the operator chose, which on a connected
    /// IC-7300 is its own USB codec.</para>
    /// <para>A machine with no sound device, or one that refuses to open, leaves
    /// the terminal saying it is not listening. Nothing here throws: the
    /// Explorer, the map and the training radio all work perfectly well without
    /// audio, and refusing to start would be a spectacular punishment for an
    /// unplugged cable (§8).</para>
    /// </remarks>
    private void StartDecoding()
    {
        StopDecoding();

        try
        {
            _audioInput = OpenAudioInput();
        }
        catch (Exception)
        {
            _audioInput = null;
        }

        if (_audioInput is null)
        {
            AudioInputName = "";
            IsDecoding = false;
            return;
        }

        // WHAT WINDOWS IS DOING TO THE INPUT, read once when listening starts
        // (HM-DEC-088). It is a third gain nobody can see, after the radio's
        // speaker level and its quite separate USB output level.
        _capture = WasapiAudioDevices.Health(_settings.AudioInputDeviceId);

        _decoder = new CwDecoder(_audioInput.SampleRate, _settings.CwPitchHz)
        {
            // His switch, off unless he throws it (Tim's ruling of 2026-08-27).
            UseJointCutter = _settings.UseJointDecoder,
        };

        // **A COUNT WRITTEN BESIDE A RECORDING IS READ AS BEING ABOUT THE
        // RECORDING** (HM-DEC-091). The decoder's counters run from here until
        // listening stops, so a capture taken seven hours in carried a character
        // count earned hours earlier on another band. This keeps a short history
        // of them against the audio clock, so a figure can be quoted for the
        // thirty seconds in the file rather than for the evening.
        _decoderStartedUtc = DateTime.UtcNow;
        _counters = new CwCounterTrail(
            (long)_audioInput.SampleRate * AudioTap.SecondsKept * 2);

        // **THE INSTRUMENT FOR THE FAULT NOBODY HAS FOUND YET** (HM-DEC-091).
        // The operator hears stations Hamlet does not, and finds out the next
        // morning from a roster. This says so while he is sitting at the radio,
        // so he can turn the gain, change the filter or retune and watch the
        // number answer. It reads the same tap the decoder reads and shares
        // nothing else with it.
        _keyingMeter = new CwKeyingMeter();
        _meterWork = null;
        _meterLastUtc = DateTime.MinValue;
        PublishKeying(KeyingReading.None);
        // **THE TWO PASSES BOTH REACH THE SCREEN NOW, AND THEY ARE NOT
        // RIVALS** (HM-DEC-096). The leading edge answers while somebody is
        // still sending and is never final; the settled pass runs a few seconds
        // behind with the whole stretch in hand and is what the transcript
        // keeps. Wiring only the first is the entire two-stage design being
        // invisible, and showing a provisional reading as though it were final
        // is §0.0 broken by omission.
        // **THE LEADING EDGE IS REPLACED, NOT APPENDED TO.** The decoder decides
        // late on purpose, so the tail of what it has read can change when the
        // next character arrives, and the terminal shows that rather than
        // stacking up every version of it.
        _decoder.LeadingEdge += Transcript.OfferEdge;
        _decoder.CharacterSettled += Transcript.Settle;

        // WHEN SOMETHING LAST CAME THROUGH, which is what the quiet offer waits
        // on (HM-DEC-084). Set here rather than polled, so an empty terminal is
        // measured from the last real character rather than from a timer.
        _decoder.CharacterDecoded += _ =>
        {
            _lastDecodeUtc = DateTime.UtcNow;
            _lastCharacterUtc = DateTime.UtcNow;
        };

        // **NOT SEEDED.** A decoder that has just started listening has read
        // nothing, and the mode-follow guard must not read that as somebody
        // working Morse.
        _lastCharacterUtc = DateTime.MinValue;
        _lastDecodeUtc = DateTime.UtcNow;
        _decoder.Listen(_audioInput);

        // **THE DIGITAL WATERFALL RIDES ALONG** (work instruction 038). It
        // subscribes to the same event and never starts or stops the source, so
        // nothing on the Digital tab can silence the CW decoder.
        DigitalSpectrum?.Dispose();
        DigitalSpectrum = new AudioSpectrumSource(
            _audioInput.SampleRate, _audioInput.IsSimulated);
        DigitalSpectrum.Listen(_audioInput);
        DigitalSpectrum.Start();

        _audioInput.Start();

        AudioInputName = _audioInput.DeviceName;
        IsDecoding = true;
        _decodeTimer.Start();

        AppEvents.DecoderStarted(
            _telemetry,
            _audioInput.IsSimulated,
            _audioInput.SampleRate,
            _settings.CwPitchHz,
            _deviceChoice,
            _deviceLooksLikeRadio,
            _captureDevicesOffered);
    }

    /// <summary>
    /// The source to listen to, or null when there is nothing to listen with.
    /// </summary>
    private IAudioSource? OpenAudioInput()
    {
        // **CLEARED FIRST, SO A SECOND LISTEN CANNOT INHERIT THE FIRST'S REASON**
        // (unit 236). The training radio opens no device at all, and a stale
        // branch beside it would say Hamlet chose a sound card it never looked at.
        _deviceChoice = null;
        _captureDevicesOffered = null;
        _deviceLooksLikeRadio = null;

        if (_rig?.IsSimulated == true)
        {
            // Real Morse at a known speed, with nothing plugged in. Twelve words
            // a minute is a patient operator, which is where somebody learning
            // to copy should start (HM-DEC-026).
            return new TrainingAudioSource(
                MorseCode.CqCall(_settings.Operator.Callsign),
                wordsPerMinute: 12,
                toneHz: _settings.CwPitchHz);
        }

        var devices = new WasapiAudioDevices().List();

        // **WHY THIS DEVICE, KEPT FOR THE RECORD** (unit 236). Four of the five
        // rules below are Hamlet guessing on the operator's behalf, and unit 235
        // measured that the one that is his own choice has never been set. Which
        // rule ran is written beside `DecoderStarted`; the device itself is not
        // written anywhere (HM-DEC-018).
        var choice = AudioDeviceChoice.ChooseWithReason(
            devices, _settings.AudioInputDeviceId);

        _deviceChoice = choice.Reason;
        _captureDevicesOffered = devices.Count;
        _deviceLooksLikeRadio = choice.Device?.LooksLikeRadio;

        return choice.Device is null ? null : new WasapiAudioSource(choice.Device);
    }

    /// <summary>
    /// Which rule picked the capture device this listen, or null where none was
    /// chosen (unit 236).
    /// </summary>
    private AudioDeviceChoiceReason? _deviceChoice;

    /// <summary>How many capture devices the machine offered, or null.</summary>
    private int? _captureDevicesOffered;

    /// <summary>
    /// Whether the chosen device's name matches the radio's codec, or null.
    /// </summary>
    /// <remarks>
    /// **THE BOOLEAN AND NOT THE NAME**, which is the shape HM-DEC-018 requires
    /// and the one <c>AppEvents.AudioDeviceChosen</c> has had since it was written.
    /// </remarks>
    private bool? _deviceLooksLikeRadio;

    /// <summary>Stop listening and put the decoder away.</summary>
    private void StopDecoding()
    {
        _decodeTimer.Stop();

        if (_decoder is not null)
        {
            _decoder.LeadingEdge -= Transcript.OfferEdge;
            _decoder.CharacterSettled -= Transcript.Settle;
            _decoder.Listen(null);
            _decoder = null;
        }

        // The trail belongs to one decoder. A new one starts at nought samples
        // with nought counted, and a history carried across the seam would make
        // a window straddle two of them.
        _counters = null;
        _decoderStartedUtc = null;
        _keyingMeter = null;
        _meterWork = null;
        PublishKeying(KeyingReading.None);

        DigitalSpectrum?.Stop();
        DigitalSpectrum?.Dispose();
        DigitalSpectrum = null;

        _audioInput?.Stop();
        _audioInput?.Dispose();
        _audioInput = null;

        IsDecoding = false;
        DetectedWpm = 0;
        AudioInputName = "";
        Transcript.Clear();
        OnPropertyChanged(nameof(TerminalSummary));
    }

    /// <summary>
    /// Bring the readouts up to date with what the decoder is doing.
    /// </summary>
    private void OnDecodeTick(object? sender, EventArgs e)
    {
        if (_decoder is null)
        {
            return;
        }

        // ONE GUARDED ANSWER (HM-DEC-090). Zero means nothing has earned the
        // right to name a speed, and every surface that shows one reads this.
        DetectedWpm = _decoder.WordsPerMinute ?? 0;

        // **THE RADIO SAYS WHETHER IT IS TRANSMITTING, AND THE DECODER IS TOLD**
        // (HM-DEC-091). Hamlet has read `1C 00` for months, the diagnostics
        // screen has shown it correctly for months, and nothing consumed it: the
        // terminal decoded the operator's own sending and showed it as somebody
        // else's, which is HM-DEC-009 in the one place nobody guarded.
        //
        // Unknown leaves decoding running. A link that has gone quiet must not
        // silence the band, because a screen that stops without a reason reads as
        // an empty band (§0.0).
        var keyed = RigState[RigField.TransmitStatus];

        _decoder.RadioIsTransmitting(
            keyed.IsKnown ? keyed.Number == 1 : null, DateTime.UtcNow);

        DecodingIsSuspended = _decoder.DecodingSuspended;
        ListeningAfresh = _decoder.ListeningAfresh;

        // **THE SETTLED-PASS READOUTS WENT WITH THE SETTLED PASS.** The tip
        // mark, the ceiling note, the handover note and the revisions count all
        // described a second pass overtaking a first, and there is one pass now.
        // A readout describing a decoder that no longer exists is the defect
        // removing it was meant to end (HM-DEC-091).
        //
        // What survives of them is the one fact that is still true and still
        // useful: the decoder will not name a speed while its window straddles
        // two stations, and the panel says so rather than going quietly blank.
        SpeedIsReacquiring = _decoder.SpeedIsReacquiring;
        TipText = Transcript.TipText;

        // WHAT IS ARRIVING, WHETHER OR NOT ANYTHING DECODES (HM-DEC-088). A
        // strong signal that will not resolve and an empty band used to produce
        // the same screen, and they are different problems.
        DecodeReport = _decoder.Report;

        // **THE LOCK'S STATE, ON THE SAME TICK AS EVERYTHING ELSE.** It reads
        // the decoder rather than remembering what was pressed, so a lock that
        // refused to engage cannot leave the panel claiming one is held.
        // The advisory is recomputed from several of these on every tick, and
        // the keying meter's block follows it (task 5): one voice at a time.
        OnPropertyChanged(nameof(AdvisoryNote));
        OnPropertyChanged(nameof(ShowKeyingMeter));

        PitchLockText = _decoder.IsLocked
            ? $"The decoder is holding {_decoder.LockedToneHz:0.0} hertz and is "
              + "not following the tracker. Press the lock again to let it follow."
            : "";

        RunKeyingMeter();

        // Sampled here, on the same tick as the readouts, so the two ends of any
        // window a capture asks about are each accurate to one tick.
        _counters?.Note(new CwCounterSample(
            _decoder.Tap.SamplesSeen,
            DecodeReport.ElementsSeen,
            DecodeReport.ElementsResolved,
            DecodeReport.CharactersEmitted,
            DecodeReport.CharactersUnsure));

        // **A STALLED AUDIO PIPELINE USED TO LOOK EXACTLY LIKE A QUIET BAND**
        // (HM-DEC-090). Nothing anywhere said the samples had stopped arriving,
        // so the capture went on handing over the same thirty seconds and the
        // decoder went on reporting what it made of them. Whatever stops the
        // stream, this notices within a couple of seconds and says so (§0.0.1).
        var seen = _decoder.Tap.SamplesSeen;

        if (seen != _lastSamplesSeen)
        {
            _lastSamplesSeen = seen;
            _audioLastMovedUtc = DateTime.UtcNow;
        }

        AudioHasStalled = seen > 0
            && DateTime.UtcNow - _audioLastMovedUtc > AudioStallAfter;
        OnPropertyChanged(nameof(TerminalSummary));
        OnPropertyChanged(nameof(InputLevelText));
        OnPropertyChanged(nameof(InputLevelFraction));
        OnPropertyChanged(nameof(DecoderStory));
        OnPropertyChanged(nameof(HasDecoderStory));
        OnPropertyChanged(nameof(KeyingAdviceIsUseful));
        OnPropertyChanged(nameof(CaptureNote));
        OnPropertyChanged(nameof(HasCaptureNote));

        NoteDecodeQuality();

        // OFFERED, NEVER ASSERTED (HM-DEC-059, HM-OPEN-006). The decoder
        // measured what the other station is sending at, so Hamlet may say so.
        // It has never asked what speed this operator can copy, so it may not
        // claim that this one suits them.
        // NO DECODE MEANS NO STATION, SO THE LINE IS ABSENT (HM-DEC-090). It
        // read "they are sending at about 62 words a minute" with nobody
        // sending, which is the phantom speed reaching a third surface.
        Transmit.HeardWpm = _decoder.WordsPerMinute;

        // **AND THE FT8 SLOT WATCH RIDES THE SAME TICK** (unit 225). Four looks a
        // second is sixty inside every fifteen-second slot, and exactly one of
        // them can produce a decode.
        OnSlotTick();

        // **AND SO DOES THE BEAT** (Tim's ruling, 2026-09-08). It reads the clock
        // and two lists and raises a change only where the sentence would differ,
        // so a countdown standing still costs nothing. **It arms nothing**: this
        // is the read, and there is no line anywhere watching it reach zero.
        RefreshTurn();
    }

    /// <summary>
    /// True once the operator has tuned with the wheel (HM-DEC-141).
    /// </summary>
    /// <remarks>
    /// Persisted, because a hint that came back every launch would not have
    /// retired at all.
    /// </remarks>
    [ObservableProperty]
    private bool _hasTunedByWheel;

    partial void OnHasTunedByWheelChanged(bool value)
    {
        if (value && !_settings.HasTunedByWheel)
        {
            _settings.HasTunedByWheel = true;
            SettingsStore.Save(_settings);
        }
    }

    /// <summary>
    /// True when audio has stopped arriving while the decoder is listening
    /// (HM-DEC-090).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InputLevelText))]
    private bool _audioHasStalled;

    /// <summary>How long silence from the sound card counts as a stall.</summary>
    /// <remarks>
    /// Two seconds. Audio arrives in chunks many times a second, so nothing
    /// legitimate is quiet for that long, and a shorter window would cry wolf
    /// over ordinary scheduling.
    /// </remarks>
    private static readonly TimeSpan AudioStallAfter = TimeSpan.FromSeconds(2);

    private long _lastSamplesSeen = -1;
    private DateTime _audioLastMovedUtc = DateTime.UtcNow;

    /// <summary>What the decoder is hearing and making of it (HM-DEC-088).</summary>
    [ObservableProperty]
    private CwDecodeReport _decodeReport = CwDecodeReport.None;

    /// <summary>
    /// The input level in words, always, even when nothing is decoding.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE-GLANCE ANSWER TO THE WHOLE COMPLAINT.** If the level is
    /// down at the bottom while the operator is listening to a perfectly good
    /// signal, the two audio paths have come apart and no amount of decoder work
    /// will fix it.
    /// </remarks>
    public string InputLevelText
    {
        get
        {
            if (!IsDecoding)
            {
                return "";
            }

            if (AudioHasStalled)
            {
                return "audio has stopped arriving";
            }

            var level = DecodeReport.Level;

            if (level.Clipping)
            {
                return "input overloading";
            }

            if (level.NearlySilent)
            {
                return "almost nothing arriving";
            }

            return $"input peaking at {level.PeakDb:0} dB, noise around "
                + $"{level.FloorDb:0} dB";
        }
    }

    /// <summary>The input level as a bar, zero to one.</summary>
    public double InputLevelFraction
    {
        get
        {
            var peak = DecodeReport.Level.PeakDb;

            return Math.Clamp(
                (peak - AudioLevel.SilenceDb)
                    / (AudioLevel.FullScaleDb - AudioLevel.SilenceDb),
                0, 1);
        }
    }

    /// <summary>What the decoder can see, when it is producing nothing.</summary>
    public string DecoderStory => CwDecodeStory.Describe(DecodeReport, IsDecoding);

    /// <summary>True when there is something to say about it.</summary>
    public bool HasDecoderStory => DecoderStory.Length > 0;

    /// <summary>
    /// Whether the keying sweep's advice about the antenna is worth showing.
    /// </summary>
    /// <remarks>
    /// <para>**TWO PANELS ASSERTED OPPOSITE THINGS ABOUT THE SAME BAND AND THE
    /// ADVICE SENT HIM TO THE RADIO FOR A DECODER CONDITION.** The line above
    /// said a clear tone was present; this block said no keying here, fifty hertz
    /// away; and its paragraph told him the signal was being lost between the
    /// antenna and Hamlet and to try the gain, the filter and the tuning. On the
    /// evening of 2026-08-25 he went and did that, and nothing was wrong with the
    /// radio.</para>
    /// <para>**THE ADVICE IS ONLY EVER TRUE WHERE NOTHING FOUND A TONE.** Where
    /// the decoder has one, the sweep disagreeing with it is a fault in the
    /// sweep — measured on this tree's own corpus, its calibration sits inside an
    /// overlap rather than in a gap — and telling him to go and turn knobs is
    /// acting on the wrong one of two instruments (§0.0).</para>
    /// <para>The word and the numbers stay where the sweep is shown at all; what
    /// retires is the instruction to go to the radio.</para>
    /// </remarks>
    public bool KeyingAdviceIsUseful => !DecodeReport.HasTone;

    /// <summary>What Windows is doing to the input, where it could be read.</summary>
    public string CaptureNote => CaptureAdvice.Describe(_capture);

    /// <summary>True when Windows is doing something worth saying.</summary>
    public bool HasCaptureNote => CaptureNote.Length > 0;

    /// <summary>The standing note about enhancements, which Hamlet cannot read.</summary>
    public static string EnhancementsNote => CaptureAdvice.EnhancementsNote;

    private CaptureHealth _capture = CaptureHealth.Unknown;

    /// <summary>The last decode-quality figures written, so an unchanged one is not.</summary>
    private CwDecodeReport _lastQuality = CwDecodeReport.None;

    /// <summary>When the last one was written.</summary>
    private DateTime _lastQualityUtc = DateTime.MinValue;

    /// <summary>
    /// How rarely the decode-quality figures may be written.
    /// </summary>
    /// <remarks>
    /// **THE LAST TELEMETRY FILE WROTE THE SAME UNCHANGED STATE TWICE PER MORSE
    /// ELEMENT** and buried everything that mattered under it (HM-DEC-077). Ten
    /// seconds, and only when something actually moved.
    /// </remarks>
    private static readonly TimeSpan QualityInterval = TimeSpan.FromSeconds(10);

    /// <summary>Put the decode-quality figures in the record, rarely.</summary>
    private void NoteDecodeQuality()
    {
        var now = DateTime.UtcNow;

        if (now - _lastQualityUtc < QualityInterval)
        {
            return;
        }

        var report = DecodeReport;

        // WHAT DECIDES IT IS WHICH NUMBERS MOVED, which is why the rate limit
        // lives here and not inside the event (§8.1).
        var moved = report.CharactersEmitted != _lastQuality.CharactersEmitted
            || report.HasTone != _lastQuality.HasTone
            || report.Clipping != _lastQuality.Clipping
            || report.NearlySilent != _lastQuality.NearlySilent
            || Math.Abs(report.Level.PeakDb - _lastQuality.Level.PeakDb) >= 3;

        if (!moved)
        {
            return;
        }

        _lastQualityUtc = now;
        _lastQuality = report;

        AppEvents.DecodeQuality(_telemetry, report, "sampled");
    }

    /// <summary>
    /// Keep the last half minute the decoder heard, as a file (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// <para>**EVERYTHING ELSE ABOUT THE FAINT-SIGNAL PROBLEM IS A HYPOTHESIS
    /// UNTIL ONE OF THESE EXISTS.** A wrong decode with its input attached is a
    /// regression test; a wrong decode without one is an argument that runs for
    /// three sessions (§0.0.1, HM-DEC-007).</para>
    /// <para>The state at the moment of capture is written beside it, because a
    /// recording whose filter width and keyer speed are unknown can be listened
    /// to and cannot be reasoned about.</para>
    /// </remarks>
    [RelayCommand]
    private async Task CaptureAudioAsync()
    {
        // **ONE SNAPSHOT, TAKEN AT THE PRESS** (HM-DEC-091, HM-DEC-111). The
        // sidecar used to read the live report again after awaiting the radio,
        // and the decode poll runs four times a second in between: the terminal
        // said a tone at 500 hertz and the file written moments later said 400,
        // from **the same property at two instants**, with nothing on the sheet
        // saying which instant either belonged to.
        var pressed = DecodeReport;

        // **THE PRESS NOW ASSERTS A STATION, NOT ONLY A CASE** (Tim's ruling of
        // 2026-08-26). The operator saying he can hear one is evidence that one
        // is there, and it is the only evidence in this system that has never
        // been wrong. Six families of admission statistic have now been measured
        // and none of them can find a station he can hear, so waiting for the
        // survey to agree with him is waiting for something that does not
        // happen.
        //
        // He supplies the keying and Hamlet supplies the frequency: the loudest
        // bin in the band at this instant, held until he clears it or the dial
        // moves. HM-DEC-095 is untouched — it forbids Hamlet choosing a note by
        // loudness on its own judgement, and this is his judgement.
        //
        // **THE AUTOMATIC PATH IS NOT CHANGED BY THIS.** An empty band still
        // produces nothing when nobody has pressed anything.
        // **OFF THE UI THREAD, BECAUSE IT SWEEPS THE BAND.** The keying sweep
        // reads twenty-five pitches across half a minute of audio, which is the
        // same work the keying meter already does on a background task rather
        // than in front of the operator.
        // **THE PRESS BANKS THE AUDIO AND NO LONGER SETS THE PITCH** (Tim's
        // ruling of 2026-08-27). Unit 1.11.21 gave it the pitch behaviour
        // because six families of admission statistic could not find a station
        // he could plainly hear, and that was a workaround dressed as a feature:
        // it asked him to press a button whose meaning was never explained and
        // called his judgement a setting.
        //
        // **THE ENGINE KEEPS THE CAPABILITY.** `CwDecoder.AssertStation` and
        // `AssertAt` are untouched and still reachable by tests, so the
        // measurement that unit produced is not lost. What has gone is the panel
        // using it behind his back.


        // **ASK THE RADIO WHERE IT IS BEFORE WRITING DOWN WHERE IT WAS**
        // (HM-DEC-107 phase 6 of the UI order). The frequency is never polled,
        // because the radio broadcasts a change and asking as well would spend
        // bus traffic on a fact already in hand (HM-DEC-050). What that ruling
        // provides for instead is the on-demand read, and this is exactly the
        // moment for one: a sidecar is evidence somebody will reason from
        // months later, and a broadcast missed at startup would otherwise put a
        // frequency in it that the radio was never on.
        if (_rigMonitor is not null)
        {
            await _rigMonitor.RefreshAsync(RigField.Frequency);
        }

        var tap = _decoder?.Tap;
        var audio = tap?.Snapshot();

        if (tap is null || audio is null)
        {
            StatusText =
                "There is no audio to keep just now, so the case is on the roster "
                + "with nothing behind it.";

            MarkCase(wav: "", refusal: "no audio was arriving");
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
            return;
        }

        // **A CAPTURE THAT CANNOT PROVE IT IS FRESH IS NOT WRITTEN**
        // (HM-DEC-090). Three presses inside seventy seconds produced
        // byte-identical files with identical analysis, beside rig state that
        // differed on every one, and the operator reasoned from one recording
        // presented as three. Evidence that looks specific and is not is worse
        // than no evidence at all (§0.0.1).
        var seen = tap.SamplesSeen;

        if (seen == _lastCaptureSamples)
        {
            StatusText =
                "No new audio has arrived since the last time you kept some, so "
                + "there is nothing fresh to write. The recording would have been "
                + "the same file over again, and the case is on the roster saying "
                + "so.";

            // **THE GUARD IS NOT WEAKENED AND ITS REFUSAL STOPS BEING INVISIBLE**
            // (HM-DEC-090). It exists because three presses inside seventy seconds
            // once produced byte-identical files that were reasoned about as three
            // pieces of evidence. What changes here is only that the refusal
            // becomes a row: the case happened, the operator heard something, and
            // a denominator that quietly dropped it would flatter the score.
            MarkCase(wav: "", refusal: "no new audio since the last one");
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
            return;
        }

        try
        {
            var folder = CaptureFolder;
            Directory.CreateDirectory(folder);

            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
            var wav = Path.Combine(folder, $"cw-{stamp}.wav");

            WavAudio.Write(wav, audio);
            File.WriteAllText(
                Path.Combine(folder, $"cw-{stamp}.txt"),
                CaptureNotes(audio, seen, pressed));

            _lastCaptureSamples = seen;

            // **AND SAY WHAT THE PRESS DID TO THE DECODER**, because it now does
            // two things and the second one is the one he pressed it for. The
            // sentence says the pitch is the loudest bin rather than a station
            // Hamlet found, so nothing here implies more than happened (§0.0).
            Narrate(
                $"Kept the last {audio.Duration.TotalSeconds:0} seconds of what the "
                + "decoder heard, with what the radio was doing beside it.");

            MarkCase(
                wav: Path.GetFileName(wav),
                refusal: "",
                inRecording: _counters?.Over(seen, audio.Samples.Length));

            AppEvents.AudioCaptured(
                _telemetry, audio.Duration.TotalSeconds, CapturedHz, worked: true);
        }
        catch (Exception)
        {
            // A capture that cannot be written loses a recording and nothing
            // else (§8).
            StatusText = "Hamlet could not write the recording.";
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
        }
    }
    /// <summary>How much audio had arrived when the last capture was written.</summary>
    private long _lastCaptureSamples = -1;

    /// <summary>Everything worth knowing about a capture, beside it.</summary>
    /// <param name="audio">What was written.</param>
    /// <param name="samplesSeen">How much audio has ever arrived.</param>
    /// <param name="report">
    /// What the decoder was reporting at the moment of the press, taken once so
    /// every figure on the sheet belongs to one instant (HM-DEC-091).
    /// </param>
    private string CaptureNotes(
        MonoAudio audio, long samplesSeen, CwDecodeReport report)
    {
        var state = RigState;

        var lines = new List<string>
        {
            $"captured   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",

            // TWO FIGURES THAT MAKE A FROZEN CAPTURE OBVIOUS (HM-DEC-090). The
            // running total says whether any audio arrived since last time, and
            // the fingerprint says whether this is the same recording, without
            // anybody having to compare files by hand.
            $"audioSeen  {samplesSeen} samples",
            $"fingerprint {Fingerprint(audio)}",
            $"seconds    {audio.Duration.TotalSeconds:0.0}",
            $"sampleRate {audio.SampleRate}",
            // **ONE SOURCE, AND IT SAYS WHICH** (HM-DEC-091). This line read
            // 7.030 MHz in a file whose own rig block, four lines further down,
            // read 14.055: the header took the app's idea of where it was tuned
            // and the block took the radio's. Where the radio has been read, the
            // radio is the answer, and where it has not, the header says so
            // rather than presenting a guess in the same shape as a measurement.
            $"frequency  {CapturedFrequency()}",

            // **AND THE BAND COMES FROM THE SAME READING AS THE FREQUENCY**
            // (HM-DEC-096, phase 6). This line took the band button the operator
            // last pressed, which is the app's idea of where it is rather than
            // the radio's, and it is how three captures came to say 40 m in a
            // header whose own rig block read 14.055 MHz. Two fields describing
            // one fact from two sources is the fault, not the wrong value.
            // That fix reached only the branch where the radio had been read;
            // the other one still fell back to the button, which is how
            // 14.028 MHz came to be labelled 40 m afterwards (HM-DEC-091).
            $"band       {CapturedBand()}",

            // **THE SETTING SAYS ONE THING AND THE CABLE SAYS ANOTHER**
            // (HM-DEC-091). `SHACK_FACTS.md` records CI-V Transceive measured
            // off, five and a half thousand frames in a minute with none of them
            // the radio volunteering anything, and captures the same week carried
            // the setting read back as on. A setting's name and a link's observed
            // behaviour are different facts and only the second is evidence, so a
            // capture now carries both and nobody has to reason from one alone.
            // Nothing here writes to the radio or advises anybody to change it.
            $"broadcast  {BroadcastDuringCapture(audio)}",
            "",
            // THE RECORDING'S OWN PEAK, not the meter's last fifth of a second
            // (HM-DEC-094). Those differed by eight decibels on a file that was
            // nearly clipping while the sidecar said there was headroom.
            $"inputPeak  {AudioTap.PeakOf(audio):0.0} dBFS  (over the whole recording)",
            $"meterPeak  {report.Level.PeakDb:0.0} dBFS  (the moment it was kept)",
            $"inputFloor {report.Level.FloorDb:0.0} dBFS",
            $"clipping   {report.Clipping}",
            // **TWO PITCHES ON ONE SHEET, AND THEY ARE NOT THE SAME
            // MEASUREMENT** (HM-DEC-091). This one and the `keying` line below
            // differ by up to 250 Hz on the same file, which reads as two
            // instruments contradicting each other and is not: this is the bin
            // the decoder is following right now, moved to continuously from
            // wherever it started and confirmed by two agreeing surveys
            // (HM-DEC-095), and the other is a fresh sweep of the whole range in
            // 25 Hz steps over the last six seconds that shares nothing with the
            // decoder. Where they disagree, the decoder is reading one pitch
            // while something louder or better keyed sits at another, and that is
            // worth knowing rather than worth hiding.
            // **A BANK CENTRE IS NOT A MEASUREMENT AND THIS SHEET USED TO PRINT
            // IT AS ONE** (§0.0, HM-DEC-009). Until the survey admits a keying
            // candidate the tracker answers with the middle of whatever bank it
            // is pointed at, and that number went out here to a tenth of nothing:
            // measured across the corpus it read 300 Hz on a station at 499.8 and
            // 825 Hz on a recording holding nothing at all. The pitch is now
            // written to a tenth of a hertz because that is what it is measured
            // to, and an unmeasured one says which bank it came from instead.
            $"toneHz     {ToneForTheRecord(report)}",
            // **THIS FIELD WAS CALLED `snrDb` AND IT IS NOT ONE** (HM-DEC-091:
            // one source, and it says which). It is a held peak of how far the
            // tracked bin stood above the noise beside it, rising at once and
            // falling about a decibel a second, which is what HM-DEC-090 built it
            // to be so that a station keying for a second and a half inside
            // thirty would not average away to nothing. What it is not is a
            // figure about this recording, and read as one it is badly wrong:
            // measured across this repository's captures it rates
            // `cw-2026-08-20-014854` at 41.7 and `cw-2026-08-20-014935` at 38.4,
            // neither of which holds keying at any pitch, above
            // `cw-2026-08-17-013347` at 34.7, which is the one this decoder reads
            // a callsign out of. **A work order was written from that reading.**
            //
            // The number is not deleted and not changed, because it measures
            // something real and something else was built on it. It says what it
            // measures instead, and the two figures on this sheet that do
            // separate a station from an empty band sit beside it: the
            // `inputPeak` and `inputFloor` pair the terminal shows, and the swing
            // on the `keying` line.
            $"tonePeak   {(double.IsNaN(report.SnrDb) ? "unread" : report.SnrDb.ToString("0.0"))}"
                + "  (the highest the tracked tone ever stood above the noise "
                + "beside it, held and decaying; not a figure about this "
                + "recording)",

            // **THE FIGURE FOR THIS RECORDING, WHICH IS WHAT EVERY NUMBER ON THIS
            // SHEET IS READ AS BEING** (HM-DEC-091). Derived, by taking the
            // decoder's own counters at the two ends of the audio in this file.
            // A count that cannot be derived says so and does not print a number.
            $"inThis     {InThisRecording(audio, samplesSeen)}",

            // **TASK 5 OF WORK INSTRUCTION 034.** The conjunction nothing on this
            // sheet could state: characters on screen from a pitch the survey
            // never admitted keying at.
            $"unkeyed    {EmittedWithoutKeying(report)}",

            // **AND THE RUNNING TOTALS, WHICH NOW SAY WHAT THEY COVER.** They
            // were always cumulative from the moment listening started; what they
            // never did was admit it. A capture written seven hours into an
            // evening carried a character count earned hours earlier on another
            // band, and nothing beside it said the number was not about the
            // thirty seconds it sat next to.
            $"elements   {report.ElementsSeen} seen, {report.ElementsResolved} resolved"
                + $"  ({CountsCover()})",
            $"characters {report.CharactersEmitted} emitted, "
                + $"{report.CharactersUnsure} unsure  ({CountsCover()})",

            // **THE SPEED THE DECODER WAS TRACKING**, which is the first thing
            // anybody asks of a recording Hamlet could not read. Unread stays
            // unread: a fixture labelled with a speed nobody measured is worse
            // than one labelled with nothing (§0.0, HM-DEC-090).
            // **`not tracking` AND `the number was withdrawn` ARE DIFFERENT
            // FACTS AND THIS FIELD SAID THE FIRST FOR BOTH** (HM-DEC-091). The
            // panel showed 29 words a minute and the file written moments later
            // said the decoder was not tracking, which reads as two instruments
            // disagreeing and is not: the guard on the speed had withdrawn the
            // number between the two, and nothing on the sheet could say so.
            //
            // The reading is taken here, at the press, from the same decoder the
            // rest of this sheet comes from, rather than from the polled snapshot
            // the header happens to be holding. And where there is no number it
            // says which of the guard's conditions was not met, because that is
            // the difference between nothing being on the air and a station being
            // heard whose speed had not yet been proved.
            $"decoderWpm {SpeedForTheRecord()}",

            // **THE SIDECAR RECORDED COUNTS AND NEVER A CHARACTER OF TEXT**, so
            // nothing beside a kept recording said what Hamlet had made of it.
            // The whole transcript goes here rather than the roster's tail,
            // because a file read by a person has no one-line constraint.
            //
            // It is called `text` and not `read` deliberately: `read` is the name
            // of the roster's own column, which is the operator's verdict and is
            // never written by Hamlet. Two fields one letter apart, one a machine's
            // output and one a person's judgement, is a confusion waiting for the
            // evening somebody scores thirty of them.
            $"text       {CwCaseRoster.Readable(Transcript.PlainText)}",

            // **AND THE TRANSCRIPT HAS THE SAME SHAPE OF PROBLEM AS THE COUNTS**,
            // so it gets the same treatment. It is everything read since
            // listening started, not what was read from this recording, and a
            // reader who takes it for the second has been misled by the layout.
            $"textCovers everything read {CountsCover()}",

            // **AND EVERY CHARACTER'S OWN EVIDENCE BESIDE IT, WHICH NOTHING ON
            // THIS SHEET HAS EVER CARRIED.** `reading` gives the window's
            // likelihood ratio, which is one number for everything read out of
            // that window, so a letter lifted out of a clean fade and a letter
            // assembled from the gaps between two other stations arrive here
            // looking identical. The per-character figure is measured over that
            // character's own marks against the key having been up throughout
            // them, so a wrong decode now comes with the evidence that produced
            // it and can be argued about with numbers (§0.0.1, HM-DEC-007).
            //
            // Large and positive is a character with a signal behind it. Near
            // zero is one that all-key-up explains just as well. `unmeasured` is
            // a pass that does not compute it, which is not the same as nought.
            $"spanLlr    {SpanRatiosForTheRecord()}",

            // **WHETHER SOMEBODY ELSE WAS KEYING IN THE SAME PASSBAND**, which
            // the survey has always known and no sheet has ever carried. Two
            // stations inside one filter arrive in one envelope, and amplitude is
            // what the decoder measures, so a recording that reads badly with a
            // competitor in it and a recording that reads badly on its own are
            // different faults that have looked identical on every sheet written
            // so far.
            //
            // **`none found` IS NOT `THE FREQUENCY WAS CLEAR`** (HM-DEC-009). The
            // survey wants three seconds and eight clean marks before it admits
            // anything, so a station that had just started is absent here and was
            // present on the air.
            $"competing  {CompetitorForTheRecord(report)}",

            // **WHETHER HAMLET COULD HEAR KEYING AT ALL, BESIDE WHAT IT READ**
            // (HM-DEC-091). The two answer different questions and only one of
            // them has ever been on a sheet. A capture where the operator heard a
            // station and this line says no keying is the signal going missing
            // before the decoder saw it, which is a fault nothing else here can
            // point at. Measured by sweeping this recording's own pitches and
            // sharing nothing with the decoder.
            // **HOW GOOD THE CLOCK FIT WAS, WHICH HAS NEVER BEEN ON A SHEET.**
            // A speed is one number out of a fit, and a fit that is not a fist
            // produces one just as readily as a fit that is. These are the three
            // figures that tell them apart, and every one of them is measured
            // rather than judged: nothing in the decoder reads them (§0.0.1).
            // **WHAT THE WORKING DECODER DID**, and it says so rather than
            // carrying a fitted dah-to-dit ratio that belongs to a decoder whose
            // output nobody sees (HM-DEC-091).
            $"reading    {FitLine()}",

            $"keying     {KeyingLine(_keyingReading)}"
                + "  (an independent sweep of 400 to 1200 Hz in 25 Hz steps over "
                + "the last six seconds, sharing nothing with the decoder)",

            // **THE ONE NUMBER THAT SORTED THE EVENING OF 2026-08-25 AND WAS
            // NOWHERE ON THIS SHEET.** Thirteen captures, one band, one input
            // level, the tone locked within a few hertz on twelve of them; sorted
            // by how much of the recording had the key down, the outcomes sort
            // themselves. Ten between 38 and 47 per cent read back with nought to
            // eight characters unsure. One at 24 per cent buried its real content
            // in forty-eight characters of noise. One at 18 per cent gave eight
            // seconds of station and twenty-two of invented text.
            //
            // **IT IS MEASURED OVER THE AUDIO IN THIS FILE**, at the pitch the
            // decoder was following, which is what every other figure on this
            // sheet is read as being and what the `keying` line above is not.
            // **And it is not written at all where the pitch was never measured**
            // (§0.0): the duty at the middle of whatever bank the decoder happens
            // to be pointed at is a fact about a bank rather than about a station,
            // and this sheet has printed one of those before.
            $"duty       {DutyForTheRecord(audio, report)}",

            // **WHAT PITCH EACH ELEMENT WAS ACTUALLY SENT AT**, which nothing on
            // this sheet has ever carried and which the decoder could not have
            // told it until work instruction 056. Every other pitch here is a
            // pitch for the whole recording, so two operators a few hertz apart
            // arrive as one number and the sheet cannot say they were two.
            //
            // **IT IS A MEASUREMENT AND NOT A VERDICT** (see `CwStreamSplit`,
            // whose verdict is withheld because no criterion measured across this
            // corpus divides the two-sender case from the clean ones). The line
            // says how far the elements spread and how far apart the two heaps
            // stand, and it says nothing at all about whether they are two people.
            $"elementHz  {ElementPitchLine(audio, report)}",
            "",
        };

        // WHAT THE DECODER HAS DONE SINCE THE LAST CAPTURE, beside the totals.
        // The totals are cumulative over the whole session, so two captures
        // showing the same ones mean nothing was decoded in between, and a reader
        // should not have to work that out by subtraction.
        // **THE ONE FIELD ON THE OLD SHEET THAT WAS ABOUT THE CAPTURE**, and
        // it is why it stays: it read `0 characters` on the press that mattered,
        // which was the truth nobody read because three bare totals beside it
        // looked more like measurements. It now says which interval it covers
        // too, because on the first capture of a session there is no previous one
        // and the difference is the whole session.
        lines.Add(
            $"sinceLast  {report.CharactersEmitted - _lastCaptureCharacters} characters, "
            + $"{report.ElementsSeen - _lastCaptureElements} elements  "
            + (_hasPreviousCapture
                ? "(since the previous capture)"
                : $"({CountsCover()}; this is the first capture of the session)"));

        lines.Add("");

        _lastCaptureCharacters = report.CharactersEmitted;
        _lastCaptureElements = report.ElementsSeen;
        _hasPreviousCapture = true;

        // EVERY FIELD WITH ITS PROVENANCE, and unread stays unread rather than
        // becoming a zero somebody later reasons from (HM-DEC-050).
        foreach (var value in state.All())
        {
            lines.Add(
                $"{value.Field,-20} "
                + (value.IsKnown
                    ? value.Text
                    : value.State.ToString().ToLowerInvariant()));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Where the radio actually is, for the capture header (HM-DEC-091).
    /// </summary>
    /// <returns>The frequency and where it came from.</returns>
    /// <remarks>
    /// The stale-frequency fault has now appeared four times in this project, and
    /// every one of them was two sources for one fact. The radio's own reading
    /// wins whenever there is one; Hamlet's own is labeled as Hamlet's.
    /// </remarks>
    /// <summary>
    /// The frequency a capture is labelled with, from one source.
    /// </summary>
    /// <remarks>
    /// **ONE FILE HAD TWO PATHS TO ONE FACT AND ONLY ONE WAS RIGHT** (HM-DEC-111).
    /// The sidecar read the radio and the telemetry event beside it was handed
    /// `FrequencyHz`, which is Hamlet's own idea of where the dial is, so a
    /// capture could carry `7025400` in one and `14028000` in the other. Fixing
    /// the sidecar's wording without fixing this would have left the same defect
    /// with better prose on one of its two halves.
    /// </remarks>
    private long CapturedHz
        => RigState[RigField.Frequency] is { IsKnown: true, Number: { } hz }
            ? (long)hz
            : FrequencyHz;

    private string CapturedFrequency()
    {
        var read = RigState[RigField.Frequency];

        if (read is not { IsKnown: true, Number: not null })
        {
            return $"{CapturedHz} Hz  (Hamlet's own, the radio was not read)";
        }

        // **A PROVENANCE LABEL CARRIES ITS AGE** (HM-DEC-111). That ruling came
        // from this very line: it wrote "read from the radio" beside a value that
        // had been read sixty seconds and two tunings earlier, and the label
        // asserted a freshness it did not have. The frequency is polled at the
        // live rate now (HM-DEC-138) and this command asks for it again before
        // writing, so the number should be a fraction of a second old — **and
        // saying so is what makes that checkable months later** rather than
        // something a reader has to take on trust.
        var age = read.Age(DateTime.UtcNow);

        var when = age is not { } old
            ? "read from the radio, age unknown"
            : old < TimeSpan.FromSeconds(2)
                ? "read from the radio a moment ago"
                : $"read from the radio {old.TotalSeconds:0} seconds before this capture";

        return $"{CapturedHz} Hz  ({when})";
    }

    /// <summary>
    /// Which band the capture was made on, derived from the frequency that was
    /// actually read (HM-DEC-096, phase 6).
    /// </summary>
    /// <returns>The band's name, with where it came from.</returns>
    /// <remarks>
    /// **DERIVED, NOT SELECTED.** The band a capture was made on is a fact about
    /// the frequency, and the frequency is a fact about the radio. Taking it from
    /// the button the operator last pressed makes it a fact about the app, which
    /// is how a header came to disagree with the rig block four lines under it.
    /// Where the radio has not been read, this says so rather than presenting a
    /// guess in the same shape as a measurement (§0.0).
    /// </remarks>
    private string CapturedBand()
    {
        // **BOTH BRANCHES DERIVE FROM THE FREQUENCY THIS FILE ALSO PRINTS**
        // (HM-DEC-091). The first version of this fixed the read case and left
        // the unread one falling back to the band button, so a header could still
        // carry a frequency from one source and a band from another: 14.028 MHz
        // labelled 40 m, which is the original defect surviving in the branch
        // nobody looked at. The band is a fact about the frequency, and there is
        // exactly one frequency on this sheet.
        var read = RigState[RigField.Frequency];
        var name = HfBands.BandFor(CapturedHz)?.Name
                   ?? "outside every band Hamlet knows";

        return read is { IsKnown: true, Number: not null }
            ? $"{name}  (from the frequency the radio reported)"
            : $"{name}  (from Hamlet's own frequency, the radio was not read)";
    }

    /// <summary>Whether any capture has already been written this session.</summary>
    private bool _hasPreviousCapture;

    /// <summary>How often the keying meter looks.</summary>
    /// <remarks>
    /// Once a second, which is fast enough to feel like an answer when the
    /// operator turns a knob and slow enough that one update's work, about
    /// seventy milliseconds of it, is a small share of a core.
    /// </remarks>
    private static readonly TimeSpan KeyingMeterEvery = TimeSpan.FromSeconds(1);

    /// <summary>What the keying meter is willing to say, in one word.</summary>
    [ObservableProperty]
    private string _keyingWord = "";

    /// <summary>The measurements behind that word.</summary>
    [ObservableProperty]
    private string _keyingDetail = "";

    /// <summary>Whether the meter is holding a verdict through a quiet stretch.</summary>
    [ObservableProperty]
    private bool _keyingIsHeld;

    /// <summary>Whether the meter can hear somebody keying.</summary>
    [ObservableProperty]
    private bool _keyingIsPresent;

    /// <summary>Whether the meter has settled on nothing being keyed.</summary>
    [ObservableProperty]
    private bool _keyingIsAbsent;

    /// <summary>Whether the meter has not seen enough to say (HM-DEC-091).</summary>
    [ObservableProperty]
    private bool _keyingIsUndecided;

    /// <summary>What the meter said, for the sidecar and the roster.</summary>
    private KeyingReading _keyingReading = KeyingReading.None;

    /// <summary>
    /// Let the keying meter look, off the interface thread (HM-DEC-091).
    /// </summary>
    /// <remarks>
    /// <para>**SEVENTY MILLISECONDS IS A VISIBLE HITCH ON THE INTERFACE THREAD**,
    /// and this runs every second for as long as the terminal is open, so it runs
    /// on a worker. **THE SEAM IS HERE AND NOWHERE ELSE**: the meter's own state
    /// is touched only by that worker, one at a time, and is read back here only
    /// after the task has completed, so the completion is what orders the two.
    /// </para>
    /// <para>A window is taken on this thread rather than inside the worker,
    /// because the tap's lock is held by the audio thread and a worker queueing
    /// behind it would drift out of step with the second it is meant to be
    /// keeping.</para>
    /// </remarks>
    private void RunKeyingMeter()
    {
        if (_meterWork is { IsCompleted: true })
        {
            if (_meterWork.IsCompletedSuccessfully)
            {
                PublishKeying(_meterWork.Result);
            }

            _meterWork = null;
        }

        if (_keyingMeter is null || _decoder is null || _meterWork is not null)
        {
            return;
        }

        if (DateTime.UtcNow - _meterLastUtc < KeyingMeterEvery)
        {
            return;
        }

        _meterLastUtc = DateTime.UtcNow;

        var meter = _keyingMeter;
        var tap = _decoder.Tap;

        // **THE READ MOVED INSIDE THE TASK IN UNIT 239, AND IT IS NOT A TIDY-UP.**
        // It used to happen right here, on the UI thread: six seconds of audio,
        // 1.15 MB, allocated once a second on the thread that draws. The meter
        // now owns that buffer and reads into it, so this costs the UI thread
        // nothing and the large object heap nothing.
        //
        // **ONE UPDATE RUNS AT A TIME AND THAT IS WHAT MAKES A SHARED BUFFER
        // SAFE.** The guard above returns while `_meterWork` is not null, so a
        // second read cannot start while the first is still reading.
        _meterWork = Task.Run(() => meter.Update(tap));
    }

    /// <summary>Put a reading on the screen.</summary>
    /// <param name="reading">What the meter said.</param>
    /// <remarks>
    /// **THE NUMBERS ARE THE POINT AND THE WORD IS THE SUMMARY** (§0.0). He is
    /// going to chase a fault by turning a knob, and a figure that moves is worth
    /// more to him than a word that changes.
    /// </remarks>
    private void PublishKeying(KeyingReading reading)
    {
        _keyingReading = reading;

        KeyingWord = reading.Verdict switch
        {
            KeyingVerdict.Keying => "somebody is keying",
            KeyingVerdict.NoKeying => "no keying here",
            _ => "listening",
        };

        KeyingIsPresent = reading.Verdict == KeyingVerdict.Keying;
        KeyingIsAbsent = reading.Verdict == KeyingVerdict.NoKeying;
        KeyingIsUndecided = reading.Verdict == KeyingVerdict.Listening;
        KeyingIsHeld = reading.Held;

        KeyingDetail = KeyingDetailFor(reading);
    }

    /// <summary>What the meter measured, or why there is nothing to show.</summary>
    /// <param name="reading">The reading.</param>
    /// <returns>The detail line.</returns>
    /// <remarks>
    /// **A HELD VERDICT PRINTS THE VERDICT AND NO MEASUREMENTS.** While the meter
    /// is coasting through a gap between overs, the newest window it has is the
    /// gap, so the figures beside the word are measurements of silence wearing
    /// the station's label. On the evening of 2026-08-20 that put `9 ms key down`
    /// on screen and in a capture sidecar for a station the other recordings of
    /// the same operator measure at about ninety, and a work order was written
    /// from it. The verdict is the thing being held and it is still worth
    /// printing; the numbers are not, because they are not about what the word
    /// says.
    /// </remarks>
    private static string KeyingDetailFor(KeyingReading reading)
    {
        if (reading.Held)
        {
            return "holding through a quiet stretch, so there is nothing fresh "
                   + "to measure";
        }

        return reading.ToneHz <= 0
            ? "nothing measured yet"
            : $"{reading.ToneHz:0} Hz, key down {reading.MedianMs:0} ms, "
              + $"{reading.SwingDb:0} dB between quiet and loud, "
              + $"{reading.Runs} key-downs";
    }

    /// <summary>
    /// Each recent character with the evidence for its own span, for the sidecar.
    /// </summary>
    /// <remarks>
    /// <para>**A WRONG DECODE WITH ITS EVIDENCE ATTACHED IS A REGRESSION TEST**
    /// (HM-DEC-007). Until this line the sheet said what was read and how loud
    /// the window was, and those two together cannot tell a character read out
    /// of a signal from one the path assembled out of noise. This can, because it
    /// is measured over that character's marks and nothing else.</para>
    /// <para>**IT COVERS WHAT THE TRANSCRIPT'S RECENT TAIL COVERS AND SAYS SO.**
    /// The same caveat `textCovers` carries applies here, and the count is
    /// printed so a reader can see when the tail is shorter than the recording
    /// rather than inferring it.</para>
    /// <para>A word gap is not a character and carries no marks, so it is left
    /// out rather than printed as a nought somebody later reasons from.</para>
    /// </remarks>
    /// <returns>The characters and their span ratios, or why there are none.</returns>
    /// <summary>One likelihood figure, inside the range the record can carry.</summary>
    /// <param name="value">The figure.</param>
    /// <returns>The figure, or a marked bound where it ran past one.</returns>
    /// <remarks>
    /// **THE SHEET HAS PRINTED QUADRILLIONS AND NOBODY READS THE REST OF SUCH A
    /// SHEET.** The `6:27306879.3` family is a per-hop log-likelihood on a
    /// recording whose noise estimate went to nothing. A clamp is a statement
    /// about what the record can carry rather than about the measurement, so it
    /// says it clamped rather than quietly writing a smaller number.
    /// </remarks>
    internal static string Clamped(double value)
    {
        if (double.IsNaN(value))
        {
            return "unmeasured";
        }

        var widest = CwCharacter.WidestRecordedLlr;

        if (value > widest)
        {
            return $">{widest:0}";
        }

        return value < -widest ? $"<-{widest:0}" : $"{value:0.0}";
    }

    /// <summary>The margin's share of the span, for the sheet.</summary>
    /// <param name="value">The quotient, or NaN where there was none.</param>
    /// <returns>Three decimals, or why there is no figure.</returns>
    /// <remarks>
    /// Three decimals rather than one, because the whole distribution measured
    /// across this repository's captures sits between −0.05 and +0.12 at the
    /// tenth and ninetieth percentiles; at one decimal almost every character
    /// would print `0.0`.
    /// </remarks>
    internal static string Share(double value)
        => double.IsNaN(value) ? "unmeasured" : $"{value:0.000}";

    private string SpanRatiosForTheRecord()
        => SpanRatioLine(Transcript.Recent(), CountsCover());

    /// <summary>The span-ratio line itself, from the characters it describes.</summary>
    /// <param name="recent">The transcript's recent tail, word gaps included.</param>
    /// <param name="covers">What the tail covers, in the sheet's own words.</param>
    /// <returns>The characters and their span ratios, or why there are none.</returns>
    /// <remarks>
    /// Static and separate from the view model for the reason
    /// <see cref="KeyingLine"/> is: what a record a person reads months later
    /// says is worth a test of its own, and a test that has to build a window to
    /// read one line will not be written.
    /// </remarks>
    public static string SpanRatioLine(
        IReadOnlyList<CwCharacter> recent, string covers)
    {
        var measured = recent
            .Where(character => !character.IsWordGap
                && !double.IsNaN(character.SpanLogLikelihoodRatio))
            .ToArray();

        if (measured.Length == 0)
        {
            return recent.Count == 0
                ? "nothing read yet"
                : "unmeasured (no character carried a span ratio)";
        }

        var body = string.Join(
            " ",
            measured.Select(character =>
                $"{CwCaseRoster.Readable(character.Text)}"
                + $":{Clamped(character.SpanLogLikelihoodRatio)}"
                + $"/{Clamped(character.MarginLlr)}"
                // **AND THE QUOTIENT, BECAUSE THE CLAMP DESTROYS IT.** Both
                // figures above are clamped at a million before they are
                // printed, and on the captures where the raw margin runs to
                // hundreds of millions that is exactly what happens — so the
                // one form of this quantity that means the same thing on two
                // recordings cannot be recovered from the two beside it.
                + $"/{Share(character.MarginShareForRecord)}"));

        return $"{measured.Length} of the last {recent.Count} characters read, "
               + "each against the key having been up throughout its own span "
               + $"({covers})"
               + Environment.NewLine
               + "           " + body;
    }

    /// <summary>
    /// Somebody else keying in the same passband, for the sheet.
    /// </summary>
    /// <param name="report">The decoder's reading at the moment of the press.</param>
    /// <returns>What was found, or that nothing was.</returns>
    /// <remarks>
    /// **THE FACT AND ITS CONSEQUENCE, NOT THE ADVICE.** The sentence naming the
    /// filter and the passband controls belongs on the screen, where the operator
    /// is sitting in front of the radio; a file read the next morning wants the
    /// measurement (HM-DEC-148 is the ruling that a diagnosis in a text file is
    /// not help).
    /// </remarks>
    private static string CompetitorForTheRecord(CwDecodeReport report)
    {
        if (report.Competitor is { } other)
        {
            return $"{Math.Abs(other.OffsetHz):0} Hz {other.Side} at "
                + $"{other.RelativeDb:+0.0;-0.0} dB relative "
                + $"({other.ToneHz:0} Hz)";
        }

        // **A FIELD THAT ALWAYS SAYS THE SAME THING IS WORSE THAN NO FIELD.**
        // Every sidecar written this week said `none found`, including files with
        // eight admitted tones and a station 2.4 dB from the tracked one, because
        // the competitor search only looks at candidates the survey admitted and
        // the survey admits almost nothing. The absence was real and the sentence
        // was useless.
        //
        // So it now says what the survey did see. The strongest thing in the band
        // is not a competitor — nobody has judged it to be keying — and saying so
        // is the difference between "the frequency is clear" and "nothing here
        // passed the bar that would have made it a competitor" (§0.0).
        if (report.Interference is { } loudest)
        {
            return "none admitted, and the survey is not silent: the loudest "
                + $"thing in the band is at {loudest.ToneHz:0} Hz, "
                + $"{loudest.LiftDb:+0.0;-0.0} dB over the band floor, keyed "
                + $"{loudest.PresentFraction * 100:0}% of the time. Nothing has "
                + "judged it to be a station";
        }

        return "none found, and the survey found nothing else either — which is "
            + "not the same as the frequency being clear";
    }

    /// <summary>
    /// What pitch each element was sent at, spread and heaps, or why there is
    /// nothing to say.
    /// </summary>
    /// <param name="audio">The recording being written.</param>
    /// <param name="report">What the decoder had at the moment of the press.</param>
    /// <returns>The line, in the sheet's own voice.</returns>
    /// <remarks>
    /// <para>**MEASURED OVER THE AUDIO IN THIS FILE**, at the pitch the decoder
    /// was following, like every other figure on this sheet that is about the
    /// recording rather than about the evening.</para>
    /// <para>**AND NOT WRITTEN AT ALL WHERE THE PITCH WAS NEVER MEASURED**
    /// (§0.0). An element pitch taken relative to the middle of whatever bank the
    /// decoder happens to be pointed at is a fact about a bank, and this sheet has
    /// printed one of those before.</para>
    /// <para>**IT REPORTS AND DOES NOT CONCLUDE.** `CwStreamSplit` returns no
    /// split today, on evidence recorded in its own remarks, so the line gives the
    /// two heaps and their separation and leaves the question where it is. Saying
    /// two people are sending on the strength of an untested criterion is the
    /// guess dressed as an answer §0.0 exists to forbid.</para>
    /// </remarks>
    public static string ElementPitchLine(MonoAudio audio, CwDecodeReport report)
    {
        ArgumentNullException.ThrowIfNull(audio);
        ArgumentNullException.ThrowIfNull(report);

        if (double.IsNaN(report.ToneHz) || report.ToneHz <= 0)
        {
            return "not measured  (no pitch was measured, so there is nothing "
                   + "for an element's own pitch to be measured against)";
        }

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, report.ToneHz);

        var read = CwProbabilisticDecoder.Decode(
            envelope, report.ToneHz, null, null, false);

        var measured = CwElementPitch.MeasureAll(
            read.Elements, audio.Samples, audio.SampleRate, report.ToneHz,
            CwProbabilisticDecoder.HopMilliseconds);

        var division = CwStreamSplit.Divide(measured);

        if (division.Trusted < 2 * CwStreamSplit.LeastTrustedMarks)
        {
            return $"{division.Trusted} elements were long enough to measure a "
                   + "pitch from, which is too few to say anything about how they "
                   + "spread";
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} elements measured, gathering at {1:0.0} and {2:0.0} Hz, "
            + "{3:0.0} Hz apart with {4:0.0} Hz of scatter inside them  "
            + "(measured over this recording; whether that is one operator or two "
            + "is not something Hamlet can yet tell you)",
            division.Trusted,
            division.LowerHz,
            division.UpperHz,
            division.ApartHz,
            division.ScatterHz);
    }

    /// <summary>
    /// How much of this recording had the key down, at the pitch the decoder was
    /// following, or why there is no figure.
    /// </summary>
    /// <param name="audio">The audio in this file, and nothing else.</param>
    /// <param name="report">What the decoder believed at the press.</param>
    /// <returns>The line for the sheet.</returns>
    /// <remarks>
    /// **A TENTH OF A PER CENT, BECAUSE THAT IS WHAT SEPARATES THE OUTCOMES.**
    /// The evening this was written for spread from 18 to 47 per cent across
    /// thirteen recordings and the boundary between readable and mostly invented
    /// sat around a quarter.
    /// </remarks>
    private static string DutyForTheRecord(MonoAudio audio, CwDecodeReport report)
    {
        if (!report.HasTone || !report.PitchWasMeasured)
        {
            return "not measured  (no pitch was measured, so there is no station "
                + "to measure the keying of)";
        }

        var profile = KeyingEnvelope.Measure(audio, report.ToneHz);

        return $"{profile.Duty * 100:0.0}%  (of the {audio.Duration.TotalSeconds:0.0} seconds in "
            + $"this file, the key was down at {report.ToneHz:0.0} Hz)";
    }

    /// <summary>The pitch the decoder was following, and whether it measured it.</summary>
    /// <param name="report">The decoder's reading at the moment of the press.</param>
    /// <returns>The pitch, or where the unmeasured number came from.</returns>
    /// <remarks>
    /// **UNREAD IS NOT NOUGHT AND A STARTING POINT IS NOT A READING.** The three
    /// states a reader has to be able to tell apart are a measured pitch, a bank
    /// centre nobody keyed at, and no tone at all.
    /// </remarks>
    private static string ToneForTheRecord(CwDecodeReport report)
    {
        if (!report.HasTone)
        {
            return "none";
        }

        // **THE OPERATOR'S OWN ASSERTION IS SAID AS ONE** (Tim's ruling of
        // 2026-08-26). Nothing here may read as Hamlet having found what a human
        // found: it says the pitch is the loudest bin, that a person supplied the
        // evidence there was a station on it, and that Hamlet measured no keying.
        if (report.PitchWasAsserted)
        {
            return $"{report.ToneHz:0.0} Hz  (NOT MEASURED: you said you could "
                + "hear a station, so this is the loudest bin in the band at that "
                + "moment. Hamlet did not find keying here)";
        }

        // **THE RANKING IS SAID FIRST, BECAUSE IT IS WHAT SUPPLIED THE NUMBER**
        // (Tim's ruling of 2026-08-28). The survey may well have admitted keying
        // somewhere too, but the mixer was run at the ranking's winner and the
        // sheet has to report the pitch the decode used. **Both scores go on the
        // line**, so a pick that only just beat its runner-up can be told from
        // one that walked it (§0.0.1).
        if (report.Rank is { } rank)
        {
            return $"{report.ToneHz:0.0} Hz  (ranked: the band was decoded at "
                + $"every candidate pitch and this one read best, at "
                + $"{rank.Score:0.00} against {rank.RunnerUpScore:0.00} for "
                + $"{rank.RunnerUpHz:0.0} Hz. Scoring measures keying and not "
                + "loudness, and it is not the survey admitting a station)";
        }

        if (report.PitchWasMeasured)
        {
            return $"{report.ToneHz:0.0} Hz  (measured from the keying the "
                + "survey admitted, interpolated between bins)";
        }

        // **"THE MIDDLE OF THE BANK" STOPPED BEING TRUE ON 2026-08-27** and this
        // sheet went on saying it. Tim's ruling of that date lets the strongest
        // bin choose the note at acquisition, so an unmeasured pitch is now
        // sometimes a bin that was picked for being the loudest thing in the
        // band and sometimes still a bank centre nobody chose. **Those are
        // different claims and a sheet that blurs them is the fault this whole
        // field exists to prevent** (§0.0).
        return report.PitchChoice == CwPitchChoice.StrongestBin
            ? $"{report.ToneHz:0.0} Hz  (NOT MEASURED: the survey has admitted "
              + "no keying, so this is the loudest bin in the band rather than a "
              + "station)"
            : $"{report.ToneHz:0.0} Hz  (NOT MEASURED: the survey has admitted "
              + "no keying and nothing has chosen a bin, so this is the middle "
              + "of the bank the decoder is pointed at rather than a station)";
    }

    /// <summary>
    /// Whether characters reached the screen from a pitch nobody measured, and
    /// what was behind them.
    /// </summary>
    /// <remarks>
    /// <para>**THE LINE THAT MAKES THE OPERATOR'S JUNK CAPTURABLE** (work
    /// instruction 034 task 5). He is watching an empty frequency fill with
    /// characters and **no recording in this repository reproduces it** — both
    /// that hold nothing emit nought through the real decoder. So the next time
    /// it happens, the capture has to carry enough to say what state produced
    /// it.</para>
    /// <para>**IT IS A CONJUNCTION AND THAT IS THE POINT.** Characters from a
    /// measured pitch are an ordinary decode. Characters from a pitch the survey
    /// never admitted keying at are the case worth catching, and until now the
    /// sheet recorded both halves and never the pair.</para>
    /// </remarks>
    private static string EmittedWithoutKeying(CwDecodeReport report)
    {
        if (report.CharactersEmitted == 0)
        {
            return "nothing emitted";
        }

        if (report.PitchWasMeasured)
        {
            return $"no  ({report.CharactersEmitted} characters, and the survey "
                + "admitted keying at this pitch)";
        }

        var how = report.PitchChoice switch
        {
            CwPitchChoice.OperatorAssertion => "you said you could hear a station",
            CwPitchChoice.StrongestBin => "the loudest bin in the band",
            CwPitchChoice.Ranked => "decoding at every candidate and keeping the best",
            CwPitchChoice.Keying => "keying, though the pitch reads unmeasured",
            _ => "the middle of the bank, which nothing chose",
        };

        return $"YES  ({report.CharactersEmitted} characters reached the screen "
            + $"from a pitch chosen by {how}, with no keying admitted here. "
            + "This is the sheet to send back)";
    }

    /// <summary>
    /// The speed at the moment of the press, or why there is not one.
    /// </summary>
    /// <remarks>
    /// The guard on <see cref="CwDecoder.WordsPerMinute"/> withholds a number
    /// until a tone has been located, a character has resolved, the clock is not
    /// being re-acquired, and the settled pass has proved a dit. All four
    /// failures used to print the same three words (HM-DEC-091).
    /// </remarks>
    private string SpeedForTheRecord()
    {
        if (_decoder is not { } decoder)
        {
            return "not tracking (nothing is listening)";
        }

        if (decoder.WordsPerMinute is { } wpm)
        {
            return $"{wpm}";
        }

        var report = decoder.Report;
        var rolling = decoder.Reading.WordsPerMinute > 0
            ? $"the decoder's own best hypothesis was "
              + $"{decoder.Reading.WordsPerMinute:0} WPM"
            : "the decoder had no hypothesis worth naming";

        if (!report.HasTone)
        {
            return $"not tracking (no tone was located; {rolling})";
        }

        if (report.CharactersEmitted == 0)
        {
            return $"not proved (a tone but no resolved character; {rolling})";
        }

        return decoder.SpeedIsReacquiring
            ? $"withdrawn (the clock is being re-acquired; {rolling})"
            : $"not proved (the settled pass has no clock; {rolling})";
    }

    /// <summary>What the clock fit looked like, as one line.</summary>
    /// <remarks>
    /// **THE RATIO IS NOT A VERDICT.** A dah of four and a quarter dits is a real
    /// fist somebody sent on the air and this project read by hand (HM-DEC-144),
    /// so a number far from three is a thing to look at rather than a fault. What
    /// it sits beside is the separation, which is what HM-DEC-095 measured as the
    /// statistic that tells a fist from a smear.
    /// </remarks>
    private string FitLine()
    {
        if (_decoder is not { } decoder)
        {
            return "not fitted";
        }

        var reading = decoder.Reading;

        if (reading.WordsPerMinute <= 0)
        {
            return "nothing fitted yet";
        }

        // **THIS LINE USED TO QUOTE A DECODER NOBODY CAN SEE THE OUTPUT OF.** It
        // read `CwSpeedEstimator`, which fits a clock by clustering run lengths
        // and has decoded nothing since the decoder was replaced, so a sheet
        // reported a dah of 15.7 dits beside text produced by something else
        // entirely — and four evenings of captures were read as evidence about
        // the clock behind the words. They were not.
        //
        // **THE WORKING DECODER HAS NO FITTED RATIO TO REPORT**, and that is not
        // a gap: it never measures one. A dah is three dits in its model, the
        // speed is whichever hypothesis explained the audio best, and how well
        // that explanation did is the likelihood ratio against silence. Those are
        // the numbers behind the text and they are what this line carries now.
        // **A WINNER AT EITHER END OF THE SEARCH SAYS SO** (§0.0). A hypothesis
        // at the edge of a range wins by default rather than on evidence,
        // because there is nothing beyond it to lose to. On 2026-08-25 two
        // operators measured 30.9 and 30.8 words a minute and this line said 32
        // for both, which was the top of the grid, and nothing on the sheet
        // could tell a ceiling from a measurement.
        var atEdge =
            reading.WordsPerMinute >= CwProbabilisticDecoder.FastestWpm - 1e-9
                ? "  (AT THE TOP OF THE SEARCH: the sender may be faster than "
                  + "Hamlet can look)"
                : reading.WordsPerMinute <= CwProbabilisticDecoder.SlowestWpm + 1e-9
                    ? "  (AT THE BOTTOM OF THE SEARCH: the sender may be slower "
                      + "than Hamlet can look)"
                    : "";

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:0} WPM won out of {1} to {2}, {3:0.0} better than silence per "
            + "hop against a gate of {4:0}{5}  (this is the last {6:0} second "
            + "window alone, at the moment of the press, and not the whole "
            + "recording)",
            reading.WordsPerMinute,
            CwProbabilisticDecoder.SlowestWpm,
            CwProbabilisticDecoder.FastestWpm,
            reading.LikelihoodRatio,
            CwProbabilisticDecoder.Gate,
            atEdge,
            CwProbabilisticStream.WindowSeconds);
    }

    /// <summary>What the meter said, as one line for a record.</summary>
    /// <param name="reading">The reading.</param>
    /// <returns>The line.</returns>
    internal static string KeyingLine(KeyingReading reading)
    {
        var word = reading.Verdict switch
        {
            KeyingVerdict.Keying => "keying",
            KeyingVerdict.NoKeying => "no keying",
            _ => "listening",
        };

        if (reading.ToneHz <= 0)
        {
            return "not measured";
        }

        // **A HELD VERDICT CARRIES NO MEASUREMENTS INTO THE RECORD EITHER.** The
        // sidecar is the more dangerous of the two places, because a figure
        // written beside a recording is read months later as a fact about it.
        // **THE KEY-DOWN LENGTH PRINTED HERE USED TO BE ONE NOBODY COULD SEND**
        // (§0.0). It was the middle of every threshold crossing, and a threshold
        // is crossed by noise hundreds of times, so on a recording holding a real
        // station the chatter outnumbered the elements several to one and the
        // number landed among the chatter: four milliseconds beside an
        // adjudicated `VA3VRR`, three beside an adjudicated `N4L`. A dit at sixty
        // words a minute is twenty and sixty is faster than a hand sends, so
        // those were not measurements that had gone wrong. They were
        // measurements of something that is not Morse, printed where a reader
        // takes them for a fist.
        //
        // **THE VERDICT IS STILL CALIBRATED ON THE OLD FIGURE AND IS UNCHANGED
        // HERE.** Moving the verdict onto this one was built and measured: it
        // takes the meter from ten recordings right of twenty-three to seventeen,
        // and it costs the silence property, because in single six-second windows
        // `cw-2026-08-20-014854` scores above the bar and would then read as
        // keying. That trade is not this session's to make.
        if (reading.Held)
        {
            return word + " (held through a quiet stretch, so nothing was measured)";
        }

        var length = reading.ElementMedianMs > 0
            ? string.Format(
                CultureInfo.InvariantCulture,
                "{0:0} ms key down",
                reading.ElementMedianMs)
            : "no key-down was element length";

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} at {1:0} Hz, {2}, {3:0} dB swing, {4} key-downs",
            word,
            reading.ToneHz,
            length,
            reading.SwingDb,
            reading.Runs);
    }

    /// <summary>
    /// What the decoder made of the audio in this file, and nothing else
    /// (HM-DEC-091).
    /// </summary>
    /// <param name="audio">The recording being written.</param>
    /// <param name="samplesSeen">Where the audio clock stood when it was taken.</param>
    /// <returns>The figures, or why they could not be derived.</returns>
    private string InThisRecording(MonoAudio audio, long samplesSeen)
    {
        var window = _counters?.Over(samplesSeen, audio.Samples.Length);

        if (window is not { } inIt)
        {
            return "not derived (the decoder's own history does not reach back "
                   + $"over these {audio.Duration.TotalSeconds:0.0} seconds)";
        }

        return $"{inIt.CharactersEmitted} characters emitted, "
               + $"{inIt.CharactersUnsure} unsure, "
               + $"{inIt.ElementsSeen} elements seen, "
               + $"{inIt.ElementsResolved} resolved  "
               + $"(in the {audio.Duration.TotalSeconds:0.0} seconds of audio in this file)";
    }

    /// <summary>What the decoder's running totals cover, in words.</summary>
    /// <returns>The interval, as a clause.</returns>
    /// <remarks>
    /// **A BARE NUMBER BESIDE A RECORDING CLAIMS TO BE ABOUT THE RECORDING**
    /// (HM-DEC-091). These are honest fields once they say what they count, and
    /// the age is spoken rather than counted out, because a reader wants to know
    /// whether this is a fresh figure or one from hours ago (§0.7).
    /// </remarks>
    private string CountsCover()
    {
        // **AFTER A CLEAR, THE TEXT DOES NOT START WHERE THE COUNTS DO.** The
        // sheet said "everything read since the decoder started listening" beside
        // a transcript that began at the clear, which is a caption describing
        // audio the reader cannot see (§0.0). The counters are cumulative and
        // stay so; what changes is that the sentence names the moment the text
        // actually starts from.
        if (_clearedUtc is { } cleared)
        {
            return $"since the transcript was cleared at {cleared:HH:mm:ss} UTC, "
                + SpokenAge(DateTime.UtcNow - cleared);
        }

        if (_decoderStartedUtc is not { } started)
        {
            return "since the decoder started listening";
        }

        return "since the decoder started listening, "
               + SpokenAge(DateTime.UtcNow - started);
    }

    /// <summary>When the operator last cleared the transcript, if he has.</summary>
    private DateTime? _clearedUtc;

    /// <summary>How long ago something was, said rather than counted.</summary>
    /// <param name="age">How long.</param>
    /// <returns>The phrase.</returns>
    private static string SpokenAge(TimeSpan age)
    {
        if (age < TimeSpan.FromMinutes(1))
        {
            return "less than a minute ago";
        }

        if (age < TimeSpan.FromMinutes(2))
        {
            return "about a minute ago";
        }

        if (age < TimeSpan.FromMinutes(90))
        {
            return $"about {age.TotalMinutes:0} minutes ago";
        }

        var hours = age.TotalHours;

        return hours < 2.25
            ? "about two hours ago"
            : $"about {hours:0} hours ago";
    }

    /// <summary>
    /// Whether the radio volunteered anything while this recording was made.
    /// </summary>
    /// <param name="audio">The recording, whose length is the window asked about.</param>
    /// <returns>The observation, beside the setting as it was read.</returns>
    /// <remarks>
    /// <para>**THE SETTING AND THE BEHAVIOUR ARE DIFFERENT FACTS** (HM-DEC-091).
    /// `SHACK_FACTS.md` records CI-V Transceive measured off, and captures the
    /// same week carried it read back as on. Which of those is true is the
    /// operator's to rule; what a capture can do is carry the measurement beside
    /// the name, so the next argument about it starts from evidence.</para>
    /// <para>**THIS OBSERVES AND NOTHING MORE.** It writes nothing to the radio
    /// and advises nobody to change anything (§0.2).</para>
    /// </remarks>
    private string BroadcastDuringCapture(MonoAudio audio)
    {
        var setting = RigState[RigField.CivTransceive];
        var said = setting.IsKnown
            ? setting.Text
            : setting.State.ToString().ToLowerInvariant();

        if ((_rig as Ic7300Rig)?.Link is not { } health || health.Inbound == 0)
        {
            return $"the setting reads {said}, and no frame has arrived on the "
                   + "link to check it against";
        }

        var opened = DateTime.UtcNow - audio.Duration;
        var during = health.LastTransceiveUtc is { } last && last >= opened;

        return during
            ? $"the radio volunteered a change of its own while this recording "
              + $"was being made, and the setting reads {said}"
            : $"the radio volunteered nothing while this recording was being "
              + $"made, and the setting reads {said}; {health.InboundTransceive} "
              + $"of {health.Inbound} frames since the link came up were the "
              + "radio announcing something";
    }

    /// <summary>What the decoder had emitted at the last capture.</summary>
    private int _lastCaptureCharacters;

    /// <summary>What the decoder had measured at the last capture.</summary>
    private int _lastCaptureElements;

    /// <summary>
    /// A short fingerprint of the audio, so two identical captures are visibly
    /// identical (HM-DEC-090).
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>Twelve hexadecimal characters.</returns>
    private static string Fingerprint(MonoAudio audio)
    {
        var bytes = new byte[audio.Samples.Length * sizeof(float)];
        Buffer.BlockCopy(audio.Samples, 0, bytes, 0, bytes.Length);

        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(bytes))[..12].ToLowerInvariant();
    }

    /// <summary>Stop listening to the radio's scope.</summary>
    private void StopRigSpectrum()
    {
        if (_rigSpectrum is null)
        {
            return;
        }

        _rigSpectrum.Stop();
        _rigSpectrum.Dispose();
        _rigSpectrum = null;

        if (SpectrumSource is RigSpectrumSource)
        {
            SpectrumSource = null;
        }
    }

    private void StopTrainingSpectrum()
    {
        if (_trainingSpectrum is null)
        {
            return;
        }

        SpectrumSource = null;
        _trainingSpectrum.Dispose();
        _trainingSpectrum = null;
    }

    /// <summary>
    /// Play one of the field guide's generated samples.
    /// </summary>
    /// <param name="sample">Which sample the operator asked for.</param>
    /// <remarks>
    /// Fire-and-forget on purpose: generation runs off the UI thread inside
    /// the player, and a field guide that froze while it built six seconds of
    /// SSB would be teaching patience rather than radio (HM-DEC-027).
    /// </remarks>
    [RelayCommand]
    private async Task PlaySampleAsync(ModeSampleButton? sample)
    {
        if (sample is null)
        {
            return;
        }

        AppEvents.ModeSamplePlayed(
            _telemetry, sample.Request.Mode.ToString(), sample.Request.WordsPerMinute);

        await _audio.PlayAsync(sample.Request);
    }

    /// <summary>Stop any sample that is playing.</summary>
    [RelayCommand]
    private void StopSample() => _audio.Stop();

    /// <summary>
    /// Release the training radio and the audio device on the way out.
    /// </summary>
    /// <remarks>
    /// Called from the shell's shutdown path. Leaving a sample playing after
    /// the window closes would be a small thing that feels broken.
    /// </remarks>
    public void ShutDownTraining()
    {
        StopRigMonitor();
        StopDecoding();
        StopTrainingSpectrum();
        _audio.Dispose();

        // The history store holds a file handle, so it closes with the window
        // rather than waiting for the process to end (HM-DEC-045).
        _spotStore.Dispose();
    }

    /// <summary>Close the app.</summary>
    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>Jump to a band's CW watering hole.</summary>
    [RelayCommand]
    private void SelectBand(BandButtonViewModel band)
    {
        SelectedBand = band;
        FrequencyHz = band.Band.JumpHz;
    }

    [RelayCommand]
    private async Task ToggleConnectAsync()
    {
        if (IsConnected)
        {
            await TearDownRigAsync();
            StatusText = "Disconnected";
            return;
        }

        await ConnectToAsync(SelectedPort);
    }

    /// <summary>
    /// Connect to one port, reporting what happened in the status line.
    /// </summary>
    /// <param name="port">A COM port name, or the training radio entry.</param>
    /// <param name="remember">
    /// Whether this port becomes the one to reconnect to next time. False for
    /// the startup fallback: landing on the training radio because a COM port
    /// was missing must not erase the radio the operator actually owns, or one
    /// evening with the rig switched off would quietly cost them the setting.
    /// </param>
    /// <returns>True when the radio answered.</returns>
    /// <remarks>
    /// Shared by the Connect button and the startup reconnect, so the two
    /// cannot drift into connecting differently (HM-DEC-052).
    /// </remarks>
    private async Task<bool> ConnectToAsync(string port, bool remember = true)
    {
        var (rig, rigPort) = CreateRig(port);
        var rigType = port == TrainingRadio ? "simulated" : "IC-7300";
        // Progress, not a fault. It is replaced a moment later either way.
        Narrate($"Connecting to {port}…");

        if (!await rig.ConnectAsync())
        {
            (rig as IDisposable)?.Dispose();
            AppEvents.ConnectFailed(_telemetry, port, rigType, "no_response");
            // THE THIRD CAUSE IS THE ONE NOBODY GUESSES (HM-DEC-069). The
            // radio has a single setting for what leaves its USB port, and set
            // to RTTY Decode that port carries decoded text instead of control
            // messages. Every frame Hamlet sends is then correct and answered
            // by nothing, which looks exactly like a bad cable (§0.0.1).
            StatusText = $"No answer on {port}. It is usually the cable, the baud "
                       + "rate or the CI-V address, and there is one more that "
                       + "catches people out: with USB Serial Function set to RTTY "
                       + "Decode, that port carries decoded text rather than "
                       + "control messages and nothing Hamlet asks will be answered";
            return false;
        }

        var keep = _settings.LastPort;
        SelectedPort = port;

        if (!remember && keep != port)
        {
            // The dropdown shows where Hamlet ended up, and the file still holds
            // where it was trying to go.
            _settings.LastPort = keep;
            SettingsStore.Save(_settings);
        }

        _rig = rig;

        // **THE PORT IS KEPT BESIDE THE RADIO, AND THE SEND PATH IS BUILT FROM
        // IT** (work instruction 260 task 3). Both happen here, past the early
        // return above, so a connect that failed leaves both unset.
        _rigPort = rigPort;
        BuildTheArmedSend(rigPort);

        AppEvents.ConnectOk(_telemetry, SelectedPort, rigType);
        rig.FrequencyChanged += OnRigFrequencyChanged;
        IsConnected = true;
        ConnectButtonText = "Disconnect";

        // Connection state IS the mode (HM-DEC-026). A simulated rig gets the
        // synthesiser and a real one gets the radio's own scope, and neither
        // source has a setter for whether it is simulated: the waterfall's label
        // is read off whichever one is attached, so real data arriving cannot
        // weaken it and synthetic data cannot arrive unlabeled (HM-DEC-062).
        if (rig.IsSimulated)
        {
            StartTrainingSpectrum();
        }
        else
        {
            StartRigSpectrum(rig);
        }

        // Audio, on the other hand, both radios can supply: the training radio
        // makes its own Morse and a real one arrives through the capture device
        // the operator chose. So the terminal fills in either way.
        StartDecoding();
        StartRigMonitor(rig);

        // The one door to the transmitter, and it only exists while a radio is
        // connected (§0.2, HM-DEC-059).
        Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig)));

        // THE SCANNER GETS ITS RADIO, AND THE DIAL GOES BACK IF A SCAN DIED
        // MID-RUN (§0.2.1). The note is written before the first tune for
        // exactly this case, and connecting is the only moment Hamlet can act
        // on it: until there is a radio there is nothing to put back.
        Scan.Attach(rig, _rigMonitor, _decoder, SpectrumSource);
        AutoCall.Attach(rig, _rigMonitor, _decoder);
        await Scan.RestoreHomeAsync();

        var hz = await rig.GetFrequencyHzAsync();
        ApplyRigFrequency(hz);
        StatusText = port == TrainingRadio
            ? "On the training radio, with synthesised signals and nothing on the air"
            : $"Connected to the IC-7300 on {port}";

        return true;
    }

    /// <summary>
    /// Reconnect to the last radio when the app opens, if that is wanted.
    /// </summary>
    /// <remarks>
    /// <para>FAILS QUIETLY AND LEGIBLY, because failing is the normal case
    /// (HM-DEC-052). A radio switched off, unplugged, or on a COM port Windows
    /// has renumbered is what happens every time somebody opens Hamlet at their
    /// desk rather than in the shack. None of that is an error and none of it
    /// gets a dialog: the status line says what happened, in a sentence, and
    /// the app carries on with the training radio so it is still worth having
    /// open.</para>
    /// <para>A MISSING PORT IS NAMED SPECIFICALLY, because renumbering is the
    /// single most common cause and saying "COM3 isn't on this computer any
    /// more" saves somebody twenty minutes of checking a cable that was fine.
    /// </para>
    /// <para>Once, and never in a loop. If the radio arrives later the operator
    /// can click Connect, and a background loop reopening a COM port is exactly
    /// the kind of thing that upsets other software sharing it.</para>
    /// </remarks>
    public async Task ReconnectOnStartupAsync()
    {
        // Nothing here is allowed to take the app down with it (§8). This runs
        // unawaited off the window's Opened event, so an exception escaping it
        // would surface as a crash with no stack anybody could connect to the
        // radio being unplugged.
        try
        {
            await ReconnectCoreAsync().ConfigureAwait(true);
        }
        catch (Exception)
        {
            StatusText = ReconnectPlan.CouldNotOpen();
        }
        finally
        {
            // **WHATEVER HAPPENED, THE BAND ON SCREEN IS NOW THE BEST ANSWER
            // THERE IS** (HM-DEC-118). A radio that answered has set it from
            // the dial; one that did not leaves the remembered band, which is
            // the same guess as before and is now the only guess available
            // rather than a guess made in preference to asking.
            await ReloadSpotsAsync("startup").ConfigureAwait(true);
        }
    }

    private async Task ReconnectCoreAsync()
    {
        var plan = ReconnectPlan.Decide(
            _settings.ReconnectOnStartup,
            IsConnected,
            _settings.LastPort,
            AvailablePorts,
            TrainingRadio);

        switch (plan.Step)
        {
            case ReconnectStep.Nothing:
                return;

            case ReconnectStep.TrainingRadio:
                if (plan.Explanation is not null)
                {
                    AppEvents.ConnectFailed(
                        _telemetry, _settings.LastPort ?? string.Empty,
                        "IC-7300", "port_absent");
                }

                await ConnectToAsync(plan.Port, remember: plan.Explanation is null);

                if (plan.Explanation is not null)
                {
                    StatusText = plan.Explanation;
                }

                return;

            case ReconnectStep.RememberedPort:
                if (await ConnectToAsync(plan.Port))
                {
                    return;
                }

                // Once, and never in a loop. The radio is off, and a background
                // retry reopening a COM port is exactly what upsets the other
                // software sharing it.
                await ConnectToAsync(TrainingRadio, remember: false);
                StatusText = ReconnectPlan.NoAnswer(plan.Port);
                return;
        }
    }

    /// <summary>UI-origin frequency changes: clamp to band, refresh the mode
    /// line, and schedule a throttled rig send so tape drags don't flood the
    /// CI-V bus.</summary>
    /// <summary>
    /// How far the dial has to move before the decoder starts fresh, in hertz.
    /// </summary>
    /// <remarks>
    /// **PROVISIONAL, AND THE NUMBER IS TIM'S** (§12.4, work instruction 043
    /// task 5). Five hundred is the CW filter's own width: inside it the
    /// receiver is passing the same signal, so the station a nudge is aimed at
    /// is the station already being read.
    /// </remarks>
    public const long NudgeHz = 500;

    /// <summary>Where the decoder was last told the dial had moved to.</summary>
    private long _decoderTunedAtHz = long.MinValue;

    partial void OnFrequencyHzChanged(long value)
    {
        // THE DIAL REACHES PAST THE BAND EDGE, on purpose (HM-DEC-055). It used
        // to stop dead at the edge, which is a locked control standing in for an
        // explanation and is what HM-DEC-029 says not to do. The stop is now the
        // end of the picture rather than the end of the band, so somebody who
        // tunes off the top of 20 m sees what is out there and reads why it is
        // not theirs, instead of finding the knob refusing to turn. Nothing here
        // is about privileges: it is about what the map on screen can show.
        //
        // And it binds to the operator's own tuning only. A frequency the radio
        // reported is a measurement, and a measurement that has been clamped to
        // fit a picture is a wrong number about the one thing every other
        // surface trusts (§0.0).
        var clamped = _updatingFromRig ? value : Math.Clamp(value, MapLowHz, MapHighHz);
        if (clamped != value)
        {
            FrequencyHz = clamped;
            return;
        }

        UpdateModeLine();
        UpdateFavoriteState();
        ScheduleModeFollow();

        // **THE DECODED TABLE MAY NOT CARRY TWO FREQUENCIES UNDER ONE HEADING**
        // (§0.0.1, unit 225). Rows from 7.074 above rows from 14.074 assert that
        // every one of those stations was heard here.
        ClearDigitalDecodesOnRetune(clamped);

        // The dwell clock restarts wherever the dial lands, from any source
        // (HM-DEC-072). A callsign the operator arrived on stops applying the
        // moment he is somewhere else, which is what keeps the recent list from
        // naming a station on a frequency nobody heard one on.
        // The near miss is recorded here as well as on the tick, because this is
        // the path the operator's own hand takes and the tick only sees what is
        // left over (HM-OPEN-039). Whichever notices the move first reports it;
        // the other gets null, because a place is only abandoned once.
        var left = _dwell.Moved(clamped, DateTime.UtcNow);

        if (left is not null)
        {
            AppEvents.RecentDwellShort(
                _telemetry, left.FrequencyHz, left.ShortBySeconds);
        }

        // The forget button belongs to where the dial is, so it comes and goes
        // with the dial (HM-DEC-134).
        OnPropertyChanged(nameof(IsSomewhereRemembered));

        // **AND THE DECODER LETS GO OF THE PITCH IT MEASURED SOMEWHERE ELSE.**
        // The tracker holds its last measured pitch through the gaps in a slow
        // sender's keying, which is what makes a slow fist readable at all and
        // is untouched while the dial stays put. What it could not do was let
        // go: on 2026-08-26 the operator tuned here from twenty-four minutes and
        // one QSY away, and the decoder went on mixing at the 300 Hz it had
        // measured there while the station in front of him keyed above 400. It
        // refused everything, correctly, because nothing was keyed at 300.
        //
        // It hangs on the frequency rather than on a clock because that is when
        // the evidence stops existing. A station is entitled to pause for as
        // long as it likes; it is not entitled to be heard on a frequency the
        // receiver has left.
        // **A SMALL NUDGE IS NOT A MOVE** (Tim's ruling of 2026-08-29). This
        // fired on every change to the dial, including a ten-hertz one, so
        // fine-tuning a station threw away the pitch the survey had just
        // measured on it, the held peak, and the window being read — which is
        // the opposite of what a reset is for. The station a nudge is aimed at
        // is the station already being read.
        //
        // **THE FIGURE IS THE CW FILTER'S OWN WIDTH AND IT IS PROVISIONAL**
        // (§12.4). Inside 500 Hz the receiver is still passing the same signal,
        // so a move that small cannot have left the station behind; beyond it
        // the audio in the window is about somewhere else. The operator's own
        // moves on 2026-08-29 were 8.8 kHz and 13.0 kHz, twenty times this.
        // **Three candidates are costed in the report and the number is his.**
        if (Math.Abs(clamped - _decoderTunedAtHz) >= NudgeHz)
        {
            _decoderTunedAtHz = clamped;
            _decoder?.Retuned();

            // **THE TRAIL IS A HISTORY OF COUNTERS THAT HAVE JUST RESTARTED**
            // (work instruction 055, task 1), so what it holds is about another
            // frequency. Dropping it is what makes the next window derivable
            // rather than merely non-negative; `CwCounterTrail.Over` refuses a
            // window whose counters went backwards, and this is what stops that
            // refusal being the answer for the next thirty seconds.
            _counters = _audioInput is { } input
                ? new CwCounterTrail(
                    (long)input.SampleRate * AudioTap.SecondsKept * 2)
                : null;
        }

        if (_arrivedOnHz != clamped)
        {
            _arrivedOnStation = "";
            _arrivedOnHz = -1;
        }

        if (_updatingFromRig || _rig is null || !IsConnected)
        {
            return;
        }

        _rigSendPending = true;
        if (!_rigSendTimer.IsEnabled)
        {
            _rigSendTimer.Start();
        }
    }

    /// <summary>When Hamlet last moved the dial itself, or null.</summary>
    /// <remarks>
    /// **THE MOMENT, NOT A WINDOW.** What makes a reading unusable is that it was
    /// taken before the radio was told to move, and that is a comparison rather
    /// than a duration. A timer here would either be too short on a busy link or
    /// leave the display frozen after it had already caught up, and both of those
    /// are the app deciding it knows better than the radio (§0.0).
    /// </remarks>
    private DateTime? _tunedAtUtc;

    /// <summary>Where the dial was before that tune, or null.</summary>
    private long _tunedFromHz;

    /// <summary>Where that tune was aiming, or null.</summary>
    private long? _tunedToHz;

    /// <summary>True while the frequency write is actually on the wire.</summary>
    /// <remarks>
    /// `_rigSendPending` covers the queue and is cleared the moment the send
    /// starts, which left the whole round trip unguarded. They are two different
    /// states and the display needs both.
    /// </remarks>
    private bool _writeInFlight;

    /// <summary>
    /// Whether a reading can speak about where the dial is now.
    /// </summary>
    /// <param name="value">The reading.</param>
    /// <returns>True when it was taken after Hamlet's own last tune.</returns>
    /// <remarks>
    /// True when Hamlet has not tuned at all, which is every case of the operator
    /// using the radio's own knob: nothing here slows down the path that was
    /// always right.
    /// </remarks>
    private bool IsAfterOurOwnTune(RigValue value)
        => DialGuard.MayFollow(value, _tunedAtUtc);

    /// <summary>Record a tune the way the send tick does, for tests.</summary>
    /// <param name="toHz">Where it was aimed.</param>
    /// <param name="fromHz">Where the dial was.</param>
    /// <param name="atUtc">When the command went out.</param>
    /// <remarks>
    /// The send tick needs a radio and a dispatcher timer; the rule it sets up is
    /// three fields and a comparison, and that is what the display depends on. So
    /// the fields are settable from the test project and the rule is exercised
    /// exactly as the tick leaves it (§5: determinism below the UI).
    /// </remarks>
    internal void NoteTuneWritten(long toHz, long fromHz, DateTime atUtc)
    {
        _tunedAtUtc = atUtc;
        _tunedFromHz = fromHz;
        _tunedToHz = toHz;
    }

    /// <summary>Put a radio behind the view model, for tests.</summary>
    /// <param name="rig">The radio, or null to detach.</param>
    /// <remarks>
    /// <para>**THE SAME ARGUMENT AS <see cref="NoteTuneWritten"/> ONE METHOD
    /// UP.** Going through `ConnectToAsync` would need a port, a dispatcher timer
    /// and a spectrum source to test a rule that is a set, a read and a
    /// comparison. This attaches the seam and nothing else, so the rule is
    /// exercised exactly as the press leaves it (§5: determinism below the UI).</para>
    /// <para>**IT DELIBERATELY DOES NOT DO WHAT CONNECTING DOES.** No spectrum
    /// starts, no frequency subscription is taken and no telemetry is written, so
    /// a test using it cannot accidentally be testing the connect path.</para>
    /// </remarks>
    internal void UseRigForTests(IRig? rig)
    {
        _rig = rig;
        IsConnected = rig is not null;
    }

    private async void OnRigSendTick(object? sender, EventArgs e)
    {
        if (!_rigSendPending || _rig is null || !IsConnected)
        {
            _rigSendTimer.Stop();
            return;
        }

        _rigSendPending = false;

        // **STAMPED BEFORE THE COMMAND GOES, NOT AFTER IT COMES BACK.** A reading
        // that crossed the write on the wire is exactly the one that must not be
        // believed, and it is in flight from this line onward.
        var from = _tunedFromHz;
        _tunedAtUtc = DateTime.UtcNow;
        _tunedFromHz = RigState[RigField.Frequency] is { IsKnown: true, Number: { } was }
            ? (long)was
            : from;
        _tunedToHz = FrequencyHz;
        _writeInFlight = true;

        var outcome = "proceeded";
        var target = FrequencyHz;

        try
        {
            await _rig.SetFrequencyHzAsync(target);
        }
        catch (Exception ex)
        {
            outcome = "failed";
            StatusText = $"Set frequency failed: {ex.Message}";
        }
        finally
        {
            _writeInFlight = false;
        }

        // **THERE WAS NO EVENT FOR THIS AT ALL** (§0.0.1). The record carried the
        // request to tune and nothing for the write that carried it out, so a
        // display disagreeing with the radio could not be placed on either side
        // of the one command in the middle.
        AppEvents.TuneWritten(
            _telemetry, target, outcome,
            _tunedAtUtc is { } when ? (DateTime.UtcNow - when).TotalSeconds : null);

        if (!_rigSendPending)
        {
            _rigSendTimer.Stop();
        }
    }

    /// <summary>
    /// Let the dial settle, then think about the mode.
    /// </summary>
    /// <remarks>
    /// Restarted on every move, so a drag across three neighborhoods produces
    /// one change and not three (HM-DEC-056).
    /// </remarks>
    /// <summary>
    /// How many times the mode decision has been rescheduled this session.
    /// </summary>
    /// <remarks>
    /// **A GUARD IN FRONT OF AN UNEXPLAINED LOOP IS A SYMPTOM TREATED**
    /// (HM-OPEN-041). The plan remembering its last confirmed write stops the
    /// radio being written to over and over; it does not explain why the decision
    /// was being recomputed eighteen times in an evening with the dial standing
    /// still. This counts the reschedules so a test can assert that nothing
    /// changing produces nothing recomputing, which is the thing that was never
    /// checked.
    /// </remarks>
    internal int ModeFollowReschedules { get; private set; }

    private void ScheduleModeFollow()
    {
        // **COUNTED BEFORE THE GUARD, BECAUSE THE QUESTION IS HOW OFTEN THIS IS
        // ASKED.** Counting after it would measure how often a radio was attached,
        // and the loop being chased happens whether or not one is.
        ModeFollowReschedules++;

        if (!_modeFollow.Enabled || _modeFollow.Suspended || _rig is null || !IsConnected)
        {
            return;
        }

        _modeSettleTimer.Stop();
        _modeSettleTimer.Start();
    }

    private async void OnModeSettleTick(object? sender, EventArgs e)
    {
        _modeSettleTimer.Stop();
        await FollowTheMapAsync();
    }

    /// <summary>
    /// Look at the dial, and follow the map where it has come to rest inside a
    /// block that was waiting for exactly that.
    /// </summary>
    /// <remarks>
    /// <para>**A DWELL MATURES ONCE AND THEN STOPS BEING NEWS** (work instruction
    /// 050, task 4). Without that, a dial nobody is touching would report a
    /// mature dwell four times a second and the write would go out on every one
    /// of them.</para>
    /// <para>Never-throw discipline (§8): this runs on a timer nobody is
    /// awaiting, so an exception here would take the process down with no
    /// operator action behind it.</para>
    /// </remarks>
    private async void OnDwellLook(object? sender, EventArgs e)
    {
        try
        {
            var atHz = FrequencyHz;
            var here = Neighborhoods.FirstOrDefault(n => n.Contains(atHz));

            // **SUPPRESSED ENTIRELY WHILE THE SCANNER RUNS.** A scan moves the
            // dial on its own, so every block it crosses would look like an
            // arrival, and §0.2.1 already has the scanner putting the dial back
            // where it found it rather than leaving the radio somewhere it
            // reconfigured on the way past.
            var (next, matured) = _modeDwell.Observe(
                here?.Name ?? "", atHz, DateTime.UtcNow, Scan.IsScanning);

            _modeDwell = next;

            if (matured && ModeFollowPlan.WaitsForDwell(ModeFollowPlan.TargetFor(here)))
            {
                await FollowTheMapAsync();
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Hamlet could not check the dial: {ex.Message}";
        }
    }

    /// <summary>Keep the reason mode-follow declined, for rig diagnostics.</summary>
    /// <param name="target">What the map called for, or null.</param>
    /// <param name="decision">What the plan decided.</param>
    /// <param name="waiting">Whether a data block is still waiting on the dwell.</param>
    /// <remarks>
    /// **KEPT, NOT SAID** (work instruction 051, task 5). The status line stays
    /// silent — a commentary on writes that nearly happened is noise on the one
    /// line the operator reads — and this is what the diagnostics screen shows
    /// somebody who came looking for why nothing happened (§8.1).
    /// </remarks>
    private void RememberWhyNothingHappened(
        ModeTarget? target, ModeFollowDecision decision, bool waiting)
        => _modeFollowNote = waiting && decision.Write
            ? "Mode-follow is waiting for the dial to come to rest before it "
              + "sets the mode for this block."
            : ModeFollowNote.Describe(
                target, RigState.Mode, RigState.DataVariant, decision.Because);

    /// <summary>
    /// Set the radio to the mode this stretch of band is worked in.
    /// </summary>
    /// <remarks>
    /// <para>NARRATED, ALWAYS (HM-DEC-056). A radio that changes itself silently
    /// is the "is it broken" confusion relocated rather than removed, and this
    /// operator has had enough of machines doing things without saying so.</para>
    /// <para>A write that is not confirmed leaves the mode unknown rather than
    /// assumed. The rig reports that itself, so the badge empties and the screen
    /// stops claiming to know something it does not (§0.0).</para>
    /// </remarks>
    /// <summary>Follow the map now, for a test, without waiting on a timer.</summary>
    /// <returns>A task that completes when the tune-in has been established.</returns>
    /// <remarks>
    /// <para>**THE REAL DOOR, NOT A STRING HANDED OVER** (work instruction 284 task
    /// 1). The tune-in composes its narration inside
    /// <see cref="EstablishReceiveConditionsAsync"/>, and reaching that from a test
    /// otherwise means waiting on the mode-settle timer or the dwell timer, neither
    /// of which runs headless.</para>
    /// <para>**IT IS THE SAME IDIOM AS <see cref="NarrateForTests"/> AND A WEAKER
    /// SEAM THAN IT**: this one hands over nothing at all, so what the bar ends up
    /// showing is composed by the application from the rig it is actually talking
    /// to. Nothing in `src/` calls it.</para>
    /// </remarks>
    internal Task FollowTheMapForTests() => FollowTheMapAsync();

    private async Task FollowTheMapAsync()
    {
        var rig = _rig;
        if (rig is null || !IsConnected)
        {
            return;
        }

        // Read once and carried, because the dial can move while the write is
        // in flight and the memory has to name the frequency it was made at.
        var atHz = FrequencyHz;

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(atHz));

        // **WHAT HE IS VISIBLY DOING BEATS WHAT THE MAP SAYS LIVES HERE**
        // (HM-DEC-056). The evidence is `ModeFollowPlan.WorkingCw`, which carries
        // its reasoning and the measurement behind it. It used to be an
        // expression on this line asking `IsInsideCwSegment`, which silenced
        // mode-follow across all 28 digital blocks and which no test could reach,
        // because every one of them supplied the value by hand.
        var target = ModeFollowPlan.TargetFor(here);
        var workingCw = ModeFollowPlan.WorkingCw(target, IsCopyingMorse);

        // **THE AGE OF EACH READING TRAVELS WITH IT** (work instruction 042,
        // task 1). A ledger value read before Hamlet's own write is the radio
        // not having been asked since, and one read after it is the radio
        // answering. Those are the snap-back and the operator's hand on the
        // knob, and by value alone they are the same picture.
        var decision = ModeFollowPlan.Decide(
            _modeFollow, RigState.Mode, RigState.DataVariant,
            target, atHz, workingCw,
            RigState[RigField.Mode].AtUtc,
            RigState[RigField.DataMode].AtUtc);

        // Data territory waits for the dial to come to rest; the rule is
        // `ModeDwell`. Not writing is silent, and the receive side still runs
        // below, because hearing a block is not being in its mode.
        var waiting = ModeFollowPlan.WaitsForDwell(target)
                      && !(_modeDwell.Spent
                           && _modeDwell.Block == (here?.Name ?? "")
                           && _modeDwell.FrequencyHz == atHz);

        RememberWhyNothingHappened(target, decision, waiting);

        if (!decision.Write || waiting)
        {
            // **THE MODE BEING RIGHT ALREADY IS NOT THE WHOLE OF ARRIVING**
            // (work instruction 042, task 3). He can be in USB-D on an FT8 block
            // with the noise blanker chopping the tones up, and that is exactly
            // the state this unit exists to end.
            await EstablishReceiveConditionsAsync(rig, here).ConfigureAwait(true);
            return;
        }

        _settingModeOurselves = true;
        try
        {
            // **THE FILTER GOES WITH THE MODE, IN THE SAME FRAME** (work
            // instruction 040). Where the block states how much passband it
            // needs, the write asks for the widest slot; where it states none,
            // nothing is claimed and the radio keeps choosing as before.
            //
            // **ASKING FOR THE WIDEST SLOT IS NOT KNOWING ITS WIDTH.** FIL1 is
            // whatever the operator configured it to be, so the passband is only
            // established by the readback, and until that arrives it is unknown
            // rather than assumed (HM-DEC-056, §0.0).
            var wantsPassband = here?.PassbandHz is not null;

            var result = await rig.SetModeAsync(
                decision.Mode,
                decision.DataMode,
                wantsPassband ? CivWrites.WidestFilterSlot : null);

            _lastKnownMode = result.Worked ? decision.Mode : null;

            // The write is remembered only where the radio confirmed it, so a
            // failed write is retried and a successful one is not repeated
            // (HM-OPEN-041).
            if (result.Worked)
            {
                _modeFollow = _modeFollow.Done(
                    atHz, decision.Mode, decision.DataMode, DateTime.UtcNow);
            }

            // A radio that has no such mode says so by having nothing to say,
            // and blanking the status line over it would wipe whatever the
            // operator was reading.
            //
            // **THE TWO ARMS ARE DIFFERENT KINDS** (work instruction 282 task 1).
            // `decision.Narration` is Hamlet saying what it did and goes behind
            // the mark; `result.Detail` is the radio declining and speaks.
            var say = result.Worked ? decision.Narration : result.Detail;
            if (say.Length > 0)
            {
                if (result.Worked)
                {
                    Narrate(say);
                }
                else
                {
                    StatusText = say;
                }
            }

            AppEvents.ModeFollowed(
                _telemetry, decision.Mode.ToString(), decision.DataMode,
                result.Outcome.ToString());
        }
        catch (Exception ex)
        {
            // Never-throw discipline (§8). A mode change that failed is a
            // sentence, not a crash.
            StatusText = $"Hamlet could not set the mode: {ex.Message}";
        }
        finally
        {
            _settingModeOurselves = false;
        }

        await EstablishReceiveConditionsAsync(rig, here).ConfigureAwait(true);
    }

    /// <summary>
    /// Set what would otherwise stop the operator hearing this block, and
    /// nothing else.
    /// </summary>
    /// <remarks>
    /// <para>**THE OPERATOR DOES NOT TOUCH THE RADIO** (work instruction 042).
    /// He was told three times in one afternoon to press buttons on the front of
    /// it, and what a mode needs of the receive side is a fact this project
    /// holds. He states an intent by tuning somewhere and the settings are the
    /// consequence; there is no row of switches and there is not going to be one
    /// (HM-DEC-050).</para>
    /// <para>Never-throw discipline (§8): a setting that could not be changed is
    /// a sentence rather than a crash.</para>
    /// </remarks>
    private async Task EstablishReceiveConditionsAsync(IRig rig, Neighborhood? here)
    {
        if (here is null || _conditionsSetForBlockHz == here.LowHz)
        {
            return;
        }

        var conditions = ReceiverConditions.ForBlock(here);

        // A block that states nothing produces no claim and no write, and the
        // block is marked as done either way so a settled dial is quiet.
        _conditionsSetForBlockHz = here.LowHz;

        if (conditions.Count == 0)
        {
            LastReceiverSetup = Array.Empty<ConditionResult>();
            return;
        }

        try
        {
            var (results, memory) = await ReceiverSetup
                .ApplyAsync(rig, conditions, _receiverMemory)
                .ConfigureAwait(true);

            _receiverMemory = memory;
            LastReceiverSetup = results;

            // **HE IS TOLD WHAT CHANGED AND WHY** (work instruction 042 task 4),
            // **AND SINCE 2026-09-08 HE IS TOLD ON HOVER** (work instruction 282
            // task 1). This is the paragraph he kept pointing at: nine conditions
            // on a CW block compose to 884 characters, full width, across the
            // bottom of the window on every tune-in. **Not one word of it is
            // deleted** - `Narrate` puts it behind the mark beside the count, and
            // it is a narration of things that worked rather than a fault.
            //
            // **THE FAULTS INSIDE IT STILL SPEAK.** A condition the radio did not
            // confirm, one Hamlet could not read, and one it cannot reach at all
            // are all folded into the same string by `ReceiverSetupVoice`, so
            // narrating the whole thing would have hidden three admissions behind
            // a hover. `Admissions` filters the same clauses out of the same
            // results and those go on the bar.
            var say = ReceiverSetupVoice.Say(results);

            if (say.Length > 0)
            {
                // **THE ADMISSIONS ARE THE SAME CLAUSES FROM THE SAME PLACE**,
                // filtered rather than written again, so what he hovers and what
                // he is shown cannot disagree.
                Narrate(say, ReceiverSetupVoice.Admissions(results));
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Hamlet could not set the receiver up: {ex.Message}";
        }
    }

    private async void OnSpotRefreshTick(object? sender, EventArgs e)
        => await ReloadSpotsAsync("timer");

    private void OnAgeTick(object? sender, EventArgs e)
    {
        UpdateSpotFreshness();

        var now = DateTime.UtcNow;

        NoteDwell(now);
        RefreshHeard(now);
        Heartbeat(now);

        // A SECOND WAY FOR THE TRANSMISSION TO END (HM-DEC-085). The latch is
        // normally released by the rig poll, which runs four times a second while
        // the window is up. If that stalls, on a disconnect or a window that goes
        // away mid-send, nothing else would ever look at the clock and the send
        // controls would stay unavailable until something happened to refresh
        // them. This ticks once a second regardless, so the arithmetic still runs
        // out on its own.
        if (Transmit.IsSending)
        {
            Transmit.Refresh();
        }
        OnPropertyChanged(nameof(ReceiveHeadline));
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));

        TellTheCanvasWhatItIsMissing();
    }

    /// <summary>
    /// Nothing is missing, because nothing can be away any more.
    /// </summary>
    /// <remarks>
    /// **HM-DEC-086'S "A WIDGET THAT IS NOT OUT STILL CARRIES ITS NEWS" HAS
    /// NOTHING LEFT TO SAY.** That rule existed because taking a panel off the
    /// canvas removed a display and never a subscription, so a quiet line said
    /// what was happening with one press to bring it back. The canvas is gone
    /// and the panels that remain are permanent, so no panel can be away and no
    /// news can accumulate off screen (Tim, 2026-08-27).
    ///
    /// The method survives its own body because the decode poll calls it four
    /// times a second, and a hot path is a poor place to discover a rename.
    /// </remarks>
    private void TellTheCanvasWhatItIsMissing()
    {
    }

    /// <summary>
    /// Remember where the operator has been, once he has actually stopped there
    /// (HM-DEC-072).
    /// </summary>
    /// <param name="nowUtc">The moment, passed in so the rule is testable.</param>
    /// <remarks>
    /// <para>DWELL RATHER THAN LANDING. The decision lives in
    /// <see cref="DwellTracker"/> and this only supplies the clock and the
    /// context, so the rule can be proved to the second without waiting twenty
    /// of them.</para>
    /// <para>The callsign goes on only where something identified one. Arriving
    /// by clicking a spot card counts, because the operator acted on a report of
    /// that station. Scroll-wheeling onto a frequency a spot happens to sit near
    /// does not, because nothing was checked and an entry that named a station
    /// then would be asserting a presence out of proximity (§0.0).</para>
    /// </remarks>
    internal void NoteDwell(DateTime nowUtc)
    {
        // SEEDED HERE RATHER THAN ONLY WHERE THE DIAL MOVES. The frequency the
        // app opens on was never announced by a change, so nothing started its
        // clock and the place somebody was already sitting on could never be
        // remembered. A move to where the tracker already is costs nothing, so
        // asking every tick is both cheap and the only version that cannot be
        // defeated by a path that sets the frequency quietly.
        var left = _dwell.Moved(FrequencyHz, nowUtc);

        if (left is not null)
        {
            AppEvents.RecentDwellShort(
                _telemetry, left.FrequencyHz, left.ShortBySeconds);
        }

        if (!_dwell.Settled(nowUtc))
        {
            return;
        }

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));

        // HAMLET'S OWN EARS FIRST (HM-DEC-073). A callsign the decoder read off
        // the air, here, with every character solid, is a stronger fact than a
        // report from somebody else's receiver minutes ago about a frequency
        // that may since have changed hands. Where both exist the decoder wins,
        // and either way the surface says which one it was.
        var station = CallsignResolver.StationHeard(Transcript.Recent());
        var source = StationSource.Decoder;

        if (station is null)
        {
            station = _arrivedOnHz == FrequencyHz ? _arrivedOnStation : "";
            source = station.Length > 0 ? StationSource.SpotFeed : StationSource.None;
        }

        var visit = RecentStations.From(
            FrequencyHz, station, RigModeText, here, nowUtc, source);

        // Read before the list is rebuilt, because afterwards there is no way
        // to tell a fold from a fresh entry: both leave one row (HM-OPEN-039).
        var alreadyHere = Recent.FirstOrDefault(
            e => RecentStations.IsSamePlace(e.FrequencyHz, visit.FrequencyHz));

        var wasThere = Recent.Select(e => e.FrequencyHz).ToList();

        var kept = RecentStations.Remember(Recent, visit);

        Recent.Clear();
        foreach (var entry in kept)
        {
            Recent.Add(entry);
        }

        if (alreadyHere is not null)
        {
            AppEvents.RecentFolded(
                _telemetry,
                visit.FrequencyHz,
                alreadyHere.FrequencyHz,
                Math.Abs(visit.FrequencyHz - alreadyHere.FrequencyHz),
                alreadyHere.Visits + 1);
        }
        else
        {
            AppEvents.RecentRemembered(
                _telemetry, visit.FrequencyHz, visit.IsIdentified);
        }

        // The one that fell off the end, where one did. Named by where it was
        // rather than counted, so the record says which place was lost.
        foreach (var gone in wasThere.Where(hz => kept.All(k => k.FrequencyHz != hz)))
        {
            if (alreadyHere is null || gone != alreadyHere.FrequencyHz)
            {
                AppEvents.RecentDropped(_telemetry, gone);
            }
        }

        PersistRecent();
    }

    /// <summary>How much of the transcript goes in a roster row.</summary>
    /// <remarks>
    /// A hundred and twenty characters, which is `CwTranscript.LongestTip`'s own
    /// figure and carries several overs at any speed. The whole transcript goes in
    /// the sidecar, which is not constrained to one line.
    /// </remarks>
    private const int RosterTextLength = 120;

    /// <summary>Where digital captures go.</summary>
    /// <remarks>
    /// **ITS OWN FOLDER, BY TIM'S RULING OF 2026-08-28.** WSJT-X has to be
    /// pointed at a folder of FT8 files, and hand-picking them out of the CW
    /// captures every time is the cost of mixing them. One capture root, one
    /// habit, one more line in `get-files`.
    /// </remarks>
    internal static string DigitalCaptureFolder
        => Path.Combine(CaptureFolder, "digital");

    /// <summary>
    /// Keep the last stretch of audio and the radio's state beside it.
    /// </summary>
    /// <remarks>
    /// <para>**ORDERED IN UNITS 038, 039 AND 040 AND DROPPED FROM ALL THREE**,
    /// because it was last in every one. Without it every complaint about the
    /// waterfall is a description of a picture, and a picture has now been read
    /// wrongly twice: once missing that the radio was in CW at 500 Hz, once
    /// concluding that no signal could produce what was drawn.</para>
    /// <para>**IT DOES NOT CALL `MarkCase` AND DOES NOT TOUCH `CwCaseRoster`**
    /// (Tim's ruling of 2026-08-28). Every row of that roster asserts the
    /// operator heard a station the CW decoder failed to read, and a digital
    /// press is not a CW case. The CW capture path is not edited by this at all;
    /// what is shared is the audio ring, read-only.</para>
    /// <para>**SAME RING, SAME WINDOW, NO TRIMMING.** A thirty-second grab
    /// starting mid-slot leaves WSJT-X two partial slots it cannot score, and
    /// that is accepted: these are diagnostic material, and the sheet says so.
    /// Trimming returns when scoring starts.</para>
    /// </remarks>
    [RelayCommand]
    private void CaptureDigital()
    {
        var tap = _decoder?.Tap;

        if (tap is null)
        {
            StatusText =
                "Nothing is listening, so there is no audio to keep. Connect a "
                + "radio or pick the training radio and press it again.";
            AppEvents.DigitalCaptureRefused(
                _telemetry, DigitalCaptureRefusal.NothingIsListening);
            return;
        }

        var audio = tap.Snapshot();

        if (audio is null || audio.Samples.Length == 0)
        {
            StatusText =
                "No audio has arrived yet, so there is nothing to keep.";
            AppEvents.DigitalCaptureRefused(
                _telemetry, DigitalCaptureRefusal.NoAudioYet);
            return;
        }

        // **THE DECODE COMES FIRST, BECAUSE A FULL DISK IS NOT A DEAF BAND.**
        // Writing the diagnostic material can fail for a dozen reasons that have
        // nothing to do with what was on the air, and the operator pressed this
        // button to find out what was on the air.
        var heard = ShowDecodes(audio, DateTime.UtcNow, ClockOffset);

        try
        {
            var folder = DigitalCaptureFolder;
            Directory.CreateDirectory(folder);

            var now = DateTime.UtcNow;
            var stamp = now.ToString("yyyy-MM-dd-HHmmss");

            // **THE NAME SAYS WHICH CORPUS IT BELONGS TO**, so a folder listing
            // is enough to tell diagnostic material from CW captures without
            // opening anything.
            var wav = Path.Combine(folder, $"ft8-{stamp}.wav");

            WavAudio.Write(wav, audio);

            var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));

            File.WriteAllText(
                Path.Combine(folder, $"ft8-{stamp}.txt"),
                DigitalCaptureSheet.Compose(
                    now,
                    audio.Duration.TotalSeconds,
                    audio.SampleRate,
                    RigState,
                    ClockOffset,
                    now,
                    here?.Name ?? "",
                    here?.PassbandHz,
                    DescribeAudioPath(),
                    heard.Slots,
                    heard.Refusal,
                    MeasureArrival(),

                    // **THE MESSAGES, SO THE SHEET CAN CARRY THEIR RATIOS**
                    // (unit 251). Already decoded above; nothing is decoded a
                    // second time to write this.
                    heard.Decodes));

            Narrate(
                $"{_digitalDecodeNote}. Kept the last "
                + $"{audio.Duration.TotalSeconds:0} seconds in {folder}, with "
                + "what the radio was doing beside it.");
        }
        catch (IOException error)
        {
            StatusText = $"Could not write the capture: {error.Message}";
            AppEvents.DigitalCaptureRefused(
                _telemetry, DigitalCaptureRefusal.IOException);
        }
        catch (UnauthorizedAccessException error)
        {
            StatusText = $"Could not write the capture: {error.Message}";
            AppEvents.DigitalCaptureRefused(
                _telemetry, DigitalCaptureRefusal.UnauthorizedAccessException);
        }
    }

    /// <summary>What the audio is actually coming through, for the sheet.</summary>
    /// <remarks>
    /// **§0.0.1, AND EVERY FIELD IS READ OR SAID TO BE UNREAD.** Nothing is
    /// defaulted: a source that cannot report its channel count leaves the row
    /// saying so rather than carrying a plausible 1. The channel count and the
    /// encoding come off <see cref="WasapiAudioSource"/> rather than off
    /// <see cref="IAudioSource"/>, because the training radio has neither and a
    /// seam widened to carry nulls for it would teach nothing.
    /// </remarks>
    private AudioPath DescribeAudioPath()
    {
        if (_audioInput is null)
        {
            return AudioPath.Unknown;
        }

        var wasapi = _audioInput as WasapiAudioSource;

        return new AudioPath(
            _audioInput.DeviceName,
            _audioInput.SampleRate,
            wasapi?.ChannelCount,
            wasapi?.Encoding ?? "",
            _audioInput.IsSimulated,
            _capture);
    }

    /// <summary>
    /// Decode a stretch of captured audio and put what came out on the table.
    /// </summary>
    /// <param name="audio">What the tap was holding.</param>
    /// <param name="endedAtPcUtc">When the press happened, by the PC clock.</param>
    /// <param name="offset">
    /// How far the PC clock is from UTC. **Passed rather than read off the view
    /// model**, so the whole path is a function of its arguments: the clock query
    /// runs on a timer and finishes whenever the network lets it, and a test
    /// reading the property would be a test that decodes on some evenings.
    /// </param>
    /// <remarks>
    /// <para>**THIS IS WHERE THE DIGITAL TAB STOPS BEING A PICTURE** (unit 224).
    /// Every row on that table from work instruction 037 until now was a literal
    /// string in the markup; from here it is whatever the band was doing when the
    /// button was pressed, or nothing.</para>
    /// <para>**THE PRESS AND THE RUNNING WATCH ARE ONE BEHAVIOUR NOW** (unit 225,
    /// task 4). Unit 224 made a press replace the table, because a press was then
    /// the only way anything reached it and one press is one question about one
    /// moment. The tab decodes continuously now, and a press that cleared the
    /// table would wipe a running session's history — so the press contributes its
    /// slots to the same table, through the same de-duplication, and this method
    /// is one line into <see cref="NoteSlot"/>.</para>
    /// <para>**WHICH IS WHY NOTHING APPEARS TWICE.** A press keeps the last thirty
    /// seconds, which is two whole slots the watch has usually already read; the
    /// key is the slot, the frequency and the text, so those arrive as the rows
    /// that are already there. **What §0.0.1 forbade was two moments under one
    /// heading, and every row carries its own** — the thing that would really mix
    /// two places is a retune, and that is handled where the dial moves.</para>
    /// <para>**AN UNKNOWN CLOCK IS SAID PLAINLY AND NOT SHOWN AS AN EMPTY TABLE.**
    /// FT8 needs the PC within about a second of UTC or nothing decodes, and it
    /// fails silently; a blank table and a wrong clock look identical, which is the
    /// commonest newcomer failure in this mode. The cutter's own refusal sentence
    /// is what reaches the operator.</para>
    /// <para>**NOTHING IS INTERPRETED HERE** (§12.1). The text goes on the table
    /// exactly as it was sent.</para>
    /// <para>Internal rather than private so a test can hand it a recording it
    /// built and read the rows back, the way the telemetry format is reached — a
    /// press needs a sound card, and the assertion that matters is about what
    /// lands on the table rather than about the button.</para>
    /// </remarks>
    internal Ft8Reception ShowDecodes(
        MonoAudio audio, DateTime endedAtPcUtc, ClockOffset offset)
    {
        var heard = Ft8Reader.Read(
            audio, endedAtPcUtc, offset,
            compareWithThePort: _settings.CompareWithThePort);

        NoteSlot(heard);

        // **RETURNED SO THE SHEET DOES NOT DECODE A SECOND TIME** (unit 233). The
        // press decodes before it writes, deliberately, so the census already
        // exists by the time `DigitalCaptureSheet.Compose` runs.
        return heard;
    }

    /// <summary>
    /// One look at the clock and the tap: has a slot closed, and what was in it.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS WHERE THE TAB STOPS SAMPLING THE BAND AND STARTS HEARING
    /// IT** (unit 225). Until now the count of slots decoded without somebody
    /// pressing a button was nought. FT8 opens a slot every fifteen seconds, four
    /// a minute and 240 an hour, and an operator pressing a button for each one is
    /// not watching a band.</para>
    /// <para>**THE CLOCK IS READ HERE AND NOWHERE DEEPER.** <see
    /// cref="Ft8SlotWatch"/> takes the moment as an argument, which is what makes
    /// the whole of the slot arithmetic assertable against a controllable clock.
    /// </para>
    /// <para>**OFF SCREEN IT COSTS ONE BOOLEAN.** The watch re-arms rather than
    /// running, so returning to the tab never claims a slot that closed while
    /// nothing was watching.</para>
    /// </remarks>
    private void OnSlotTick()
    {
        // **BEFORE THE MODE CHECK, AND IT CAN ONLY FIRE WHAT IS ALREADY ARMED.**
        // An operator who clicked and then changed tab still gets the one
        // transmission he asked for; nothing here can arm one (§0.2).
        DriveTheArmedSend();

        var tap = _decoder?.Tap;

        // **THE DECODER IS TOLD WHICH MODE IT IS IN, ON THE TICK THAT ALREADY
        // KNOWS.** Work instruction 238 task 5: in Digital the CW decode is
        // arithmetic nobody reads, so the audio is taken for the tap and the
        // decode is skipped, on HM-DEC-147's own path. It is set here rather
        // than watched from inside the engine because the mode is the shell's
        // fact, and the engine takes state from what it is told (§0.1).
        if (_decoder is not null)
        {
            _decoder.DigitalMode = IsDigitalMode;
        }

        if (!IsDigitalMode || tap is null)
        {
            _slotWatch.Rearm();
            return;
        }

        var offset = ClockOffset;
        var look = _slotWatch.Look(tap, DateTime.UtcNow, offset);

        if (look.Refusal != _digitalRefusal)
        {
            _digitalRefusal = look.Refusal;
            RaiseDigitalDecodeChanges();
        }

        if (look.Ready is { } ready && !_slotDecodeRunning)
        {
            _ = DecodeTheSlotAsync(ready, offset);
        }
    }

    /// <summary>Decode one completed slot and put what came out on the table.</summary>
    /// <param name="ready">The slot, whole, from the watch.</param>
    /// <param name="offset">The offset the slot was cut against.</param>
    /// <remarks>
    /// **OFF THE UI THREAD, IN THE MANNER `QueryTheClockAsync` ALREADY USES.** A
    /// slot decode is tens of milliseconds of signal processing, and running it on
    /// the dispatcher would stop the waterfall dead four times a minute. Nothing
    /// here can overlap: a decode takes far less than the fifteen seconds until
    /// the next slot, and the guard says so rather than assuming it.
    /// </remarks>
    private async Task DecodeTheSlotAsync(Ft8SlotReady ready, ClockOffset offset)
    {
        _slotDecodeRunning = true;

        try
        {
            var heard = await Task
                .Run(() => Ft8Reader.Read(
                    ready.Audio, ready.EndedAtPcUtc, offset,
                    compareWithThePort: _settings.CompareWithThePort))
                .ConfigureAwait(true);

            NoteSlot(heard);
        }
        finally
        {
            _slotDecodeRunning = false;
        }
    }

    /// <summary>
    /// Add what one completed slot gave up to the running table.
    /// </summary>
    /// <param name="heard">What came out of the slot.</param>
    /// <remarks>
    /// <para>**ROWS APPEND RATHER THAN REPLACE** (unit 225). Unit 224 made a press
    /// replace the table on purpose, because one press is one question about one
    /// moment. Continuous decoding is the other case: it is a session, and a
    /// session accumulates.</para>
    /// <para>**EVERY SLOT LEAVES A LINE, WHETHER OR NOT IT PRODUCED TEXT** (unit
    /// 233). This is the one funnel both the press and the running watch go
    /// through, so writing the census here is what makes an unattended morning at
    /// the radio readable afterwards. On 2026-09-03 the phase's closing line was
    /// performed and the machine kept no record of it at all.</para>
    /// <para>Internal rather than private so a test can drive several slots
    /// through it and read the rows back, in the manner <see cref="ShowDecodes"/>
    /// already is.</para>
    /// </remarks>
    internal void NoteSlot(Ft8Reception heard)
    {
        ArgumentNullException.ThrowIfNull(heard);

        // **THE CENSUS AND THE REFUSAL, NOT THE RECEPTION.** `AppEvents` is handed
        // only the things that cannot hold a decoded message, which is HM-DEC-018
        // enforced by the signature rather than remembered here.
        var arrival = MeasureArrival();

        AppEvents.Ft8SlotsRead(
            _telemetry, heard.Slots, heard.Refusal, heard.Offset, DateTime.UtcNow,
            arrival);

        if (heard.Refusal.Length > 0)
        {
            // The note as well as the refusal, because the capture press puts the
            // note into the status bar and a stale one there would describe the
            // last press rather than this one.
            _digitalRefusal = heard.Refusal;
            _digitalDecodeNote = heard.Refusal;

            // A refusal has its own sentence on the strip and no slot behind it,
            // so a census line under the table would be a second voice about the
            // same state.
            _digitalCensusLine = "";
            RaiseDigitalDecodeChanges();
            return;
        }

        _digitalRefusal = "";

        foreach (var decode in heard.Decodes)
        {
            AddDecodeRow(decode);
        }

        _digitalDecodeNote = DescribeDecodes(heard);
        _digitalCensusLine = DescribeCensus(heard) + ArrivalSuffix(arrival);

        RaiseDigitalDecodeChanges();
    }

    /// <summary>Put one decode on the table, unless it is already there.</summary>
    /// <param name="decode">What came out of a slot.</param>
    /// <returns>True when a row was added.</returns>
    /// <remarks>
    /// **THE OLDEST ROWS FALL OFF AT <see cref="MaxDigitalDecodes"/>**, and their
    /// keys go with them, so a message from an hour ago that comes round again is
    /// a new row rather than a silently swallowed one.
    /// </remarks>
    private bool AddDecodeRow(Ft8Decode decode)
    {
        var key = string.Create(
            CultureInfo.InvariantCulture,
            $"{decode.SlotStartUtc:o}|{decode.FrequencyHz:0}|{decode.Message}");

        if (!_digitalDecodeKeys.Add(key))
        {
            return false;
        }

        if (DigitalDecodes.Count == 0)
        {
            _digitalRowsTunedAtHz = FrequencyHz;
        }

        _digitalDecodeKeyOrder.Add(key);

        PlaceRow(DigitalDecodeRow.From(decode));

        while (_digitalArrivals.Count > MaxDigitalDecodes)
        {
            // **THE OLDEST ARRIVAL, NOT THE FIRST ROW ON SCREEN.** Under a
            // newest-first ordering the first row in the collection is the
            // newest, and the old `RemoveAt(0)` would have thrown away the row
            // that had just arrived.
            var oldest = _digitalArrivals[0];

            _digitalArrivals.RemoveAt(0);
            DigitalDecodes.Remove(oldest);
            _digitalDecodeKeys.Remove(_digitalDecodeKeyOrder[0]);
            _digitalDecodeKeyOrder.RemoveAt(0);

            _digitalTrimmed++;
        }

        return true;
    }

    /// <summary>Put a row on the table, and on the visible one if it is wanted.</summary>
    /// <param name="row">The row.</param>
    /// <returns>The same row, so a caller can look at what it became.</returns>
    /// <remarks>
    /// <para>**IT NEVER REACHES THE VISIBLE TABLE AT ALL IF THE TOGGLES DO NOT
    /// WANT IT** (unit 252, Tim's ruling). Unit 251 put every row on the one table
    /// and dimmed it; the row is simply not added now.</para>
    /// <para>**WHICH IS ALSO WHY THE VIEW DOES NOT MOVE.** `FollowingScroll`
    /// watches the collection the table is bound to. A filtered-out row raises no
    /// change on it, so there is nothing to follow and the scroll position is
    /// untouched — the instruction's requirement, met by the row never arriving
    /// rather than by a rule about scrolling.</para>
    /// <para>**THE INSERT POSITION IS COUNTED, NOT GUESSED.** The visible table is
    /// the whole table with rows taken out, so a row's place in it is the number of
    /// wanted rows that sit before it in `DigitalDecodes` — which is what keeps the
    /// two in the same order under newest-first, where a row does not go at either
    /// end.</para>
    /// </remarks>
    private DigitalDecodeRow PlaceRow(DigitalDecodeRow row)
    {
        // **THE OPERATOR'S OWN GRID** (unit 252 task 2), through the one call
        // that reads it. See <see cref="WithOperatorGrid"/> for why that call
        // exists rather than the read sitting inline here, which is where it was
        // until work instruction 281 task 6.
        row = WithOperatorGrid(row);

        // **AND THE ONE PLACE THE CONTACT STATE REACHES A ROW** (unit 258). Same
        // door, same reason: every row goes through here, so there is one place
        // that books what was heard and one place that reads back where the
        // contact stands.
        // **THE DIAL IS STAMPED ON THE ROW AT DECODE TIME** (work instruction 275
        // task 3). Unit 274's log dialog read the dial when he right-clicked and
        // said so on the field, because a row carried its slot and not its tuning
        // — so an entry logged after he had retuned would record a band he never
        // worked the station on. A log is the one artefact here that outlives
        // everything else, and a wrong band in it is wrong in ten years.
        //
        // **`_digitalRowsTunedAtHz` IS THE SAME READING THE RETUNE GUARD USES**,
        // taken when the slot was cut rather than read again now, so every row of
        // one slot carries one frequency and it is the frequency that slot was
        // heard at.
        row = row with
        {
            Contact = ContactTextFor(row),

            // **A ROW THAT ARRIVED WITH A DIAL KEEPS IT.** Only the decode path
            // leaves it unset, and that is where the slot's own tuning belongs.
            HeardOnHz = row.HeardOnHz > 0 ? row.HeardOnHz : _digitalRowsTunedAtHz,
        };

        // **THE LOG IS READ ONCE AND KEPT, NOT ONCE PER DECODE** (task 5's own
        // concern). Fourteen messages a slot at four slots a minute is fifty-six
        // of these a minute; a file read behind each would be free tonight and
        // slow in March. It is re-read only when a contact is logged, which is
        // the one thing in the application that changes the file.
        _workedBefore ??= ReadWorkedBefore();

        row.WorkedBefore = WorkedBeforeNote(row.Sender);

        _digitalArrivals.Add(row);
        DigitalDecodes.Insert(InsertAt(row), row);

        return row;
    }

    /// <summary>Put one row on the table without a decoder, for tests.</summary>
    /// <param name="utc">The slot, as `hhmmss`.</param>
    /// <param name="snr">The ratio cell.</param>
    /// <param name="dt">The offset cell.</param>
    /// <param name="hz">The tone cell.</param>
    /// <param name="message">The text.</param>
    /// <param name="slotStartUtc">
    /// The boundary of the slot the message was in, in true UTC. **The contact
    /// state is counted in slots and cannot be counted off the `HHmmss` cell**,
    /// so a test that wants a state on the row passes the real moment. Left
    /// unset, the row carries no contact text and every test written before unit
    /// 258 goes on asserting exactly what it did.
    /// </param>
    /// <param name="heardOnHz">
    /// The dial the row was heard on, or 0 for a row with none recorded. **Left
    /// unset it behaves exactly as every test written before work instruction 275
    /// expects**, which is a row whose log entry carries no frequency and no band.
    /// </param>
    /// <returns>The row, so a test can look at what became of it.</returns>
    /// <remarks>
    /// **THE SAME DOOR THE DECODER USES**, so what is tested is the placement
    /// rule and not a second copy of it. Synthesising audio for five rows whose
    /// only interesting property is their to-field would spend seconds of decode
    /// on a question about a string comparison (§5).
    /// </remarks>
    internal DigitalDecodeRow AddDecodeRowForTests(
        string utc, string snr, string dt, string hz, string message,
        DateTime slotStartUtc = default,
        long heardOnHz = 0)
    {
        // **THE DIAL IS SET BEFORE PLACING**, so a test can say what a row was
        // heard on without reaching inside the panel and without a replace on the
        // bound collection afterwards. `PlaceRow` fills it from the slot's own
        // tuning only where it arrives unset.
        var row = PlaceRow(new DigitalDecodeRow(
            utc, snr, dt, hz, message, ObserverGrid: "",
            SlotStartUtc: slotStartUtc, HeardOnHz: heardOnHz));

        RaiseDigitalDecodeChanges();

        return row;
    }

    /// <summary>Where the contact with this row's sender stands, as text.</summary>
    /// <param name="row">The row that has just arrived.</param>
    /// <returns>The state and its slot count, or "" where there is no station.</returns>
    /// <remarks>
    /// <para>**THE ROW SHOWS WHERE THE CONTACT STOOD IN ITS OWN SLOT**, which is
    /// what a table of decodes is: a record of moments. The state is read at the
    /// row's own slot boundary, so a row never restates itself as the evening
    /// goes on and a reader can see a contact progressing down the table.</para>
    /// <para>**THE ENGINE DOES NOT KNOW A TAB EXISTS** (§0.1). The ledger takes
    /// the operator's callsign as a constructor parameter; this is the app
    /// reading `OperatorProfile.Callsign` and handing it over, which is the same
    /// arrangement `ObserverGrid` has above.</para>
    /// <para>**NOTHING HERE SENDS AND NOTHING CALLS `RecordSent`.** Until step 5
    /// there is no send path to say what went out, so every row's state is
    /// derived from what was heard - which is correct rather than incomplete: a
    /// station Hamlet has not answered is a station it is *your move* on.</para>
    /// <para>**AND IT IS EMPTY RATHER THAN GUESSED.** No operator callsign, a
    /// message the splitter refuses, or a sender that is a call to anyone all
    /// give "", because there is no contact between two stations to report.</para>
    /// </remarks>
    private string ContactTextFor(DigitalDecodeRow row)
    {
        var mine = _settings.Operator.Callsign?.Trim() ?? "";

        // **NO SLOT, NO SLOT COUNT** (§0.0). A row that arrived without its true
        // UTC cannot say how many slots ago anything was, and a count measured
        // from `DateTime.MinValue` would be a number on the screen that means
        // nothing. It says nothing instead.
        if (mine.Length == 0 || row.SlotStartUtc == default)
        {
            return "";
        }

        // **A CHANGED CALLSIGN OPENS A NEW LEDGER RATHER THAN REWRITING THE OLD
        // ONE.** What passed with a station was addressed to whoever the operator
        // was at the time, and carrying it over would put somebody else's
        // exchange under his call.
        if (_contacts is null
            || !string.Equals(_contactsFor, mine, StringComparison.OrdinalIgnoreCase))
        {
            _contacts = new Ft8ContactLedger(mine);
            _contactsFor = mine;
        }

        // **THE LEDGER GOES ON BOOKING EVERYTHING IT HEARS** (unit 266, unchanged).
        // Every sender is recorded whoever the message was addressed to, because
        // *how long since he transmitted at all* is measured from all of it. What
        // work instruction 271 task 4 changed is one line below this one: what the
        // column shows.
        _contacts.RecordHeard(row.Message, row.SlotStartUtc);

        // **AND THE COLUMN SPEAKS ONLY ABOUT CONTACTS HE IS IN** (Tim, 2026-09-07).
        // The rule is `Ft8ContactStates.ColumnTextFor` and it is not restated here:
        // a CQ is an invitation and gets nothing, two other stations working each
        // other get nothing, and a state appears where the message is addressed to
        // his callsign. Before this, `K9TC KJ6IX RRR` read `your move, 0 slots` on
        // his screen and he is in none of it.
        return Ft8ContactStates.ColumnTextFor(
            row.Message, mine, _contacts.For(row.Sender), row.SlotStartUtc);
    }

    /// <summary>What passed with one station, out of the ledger the app kept.</summary>
    /// <param name="callsign">The station.</param>
    /// <returns>The record, or null where no ledger or no such station.</returns>
    /// <remarks>
    /// <para>**READ-ONLY, AND IT DECIDES NOTHING** (work instruction 264, task 2).
    /// <see cref="_contacts"/> is private and is built lazily by the first row
    /// that arrives, so a test that wants to say *the ledger holds two sent and
    /// three heard against this station* has no way to see the one the
    /// application actually kept. It could build its own and prove nothing about
    /// the join, which is the whole subject of that task.</para>
    /// <para>**THE SAME IDIOM AS <see cref="AddDecodeRowForTests"/> AND
    /// <c>UseArmedSendForTests</c>**, which are `internal` on this type for the
    /// same reason. Nothing in `src/` calls this, it writes nothing, and
    /// <see cref="Ft8StationRecord"/>'s own lists are already
    /// <c>IReadOnlyList</c>.</para>
    /// </remarks>
    internal Ft8StationRecord? ContactRecordForTests(string callsign)
        => _contacts?.For(callsign);

    // ---------------------------------------------------------------------
    // THE SEND PATH. One click, one message (ruled 2026-09-06).
    // ---------------------------------------------------------------------

    /// <summary>The one armed transmission, or null where nothing can transmit.</summary>
    /// <remarks>
    /// **NULL ON THIS MACHINE AND THAT IS THE HONEST STATE.** Building one needs
    /// an <see cref="ISerialPort"/> and an <see cref="ITransmitAudioSink"/>;
    /// <c>Ic7300Rig</c> keeps the port it is given in a private field and exposes
    /// no accessor, and <see cref="AppSettings"/> names no output endpoint, both
    /// measured in <c>docs/unit259-send-path-trace.md</c>. **So the send refuses
    /// with words rather than silently doing nothing**, which is the landing work
    /// instruction 259 task 3 names as acceptable.
    /// </remarks>
    private Ft8ArmedSend? _armedSend;

    /// <summary>How a transmit sink is made from an endpoint's name.</summary>
    /// <remarks>
    /// <para>**SUBSTITUTABLE SO THAT NO TEST EVER OPENS A DEVICE**
    /// (`SHACK_FACTS.md` FACT-004). The default is the only line in `src/` that
    /// constructs a <see cref="WasapiTransmitSink"/>, and it is reached only when
    /// a radio is connected and an endpoint is named in Settings - neither of
    /// which is true on a build machine.</para>
    /// <para>**IT IS NOT A SECOND ROUTE TO A KEYING FRAME.** A sink plays audio;
    /// what keys a radio is <see cref="Ft8TransmitSequence"/>, reached from
    /// <see cref="Ft8ArmedSend.AtBoundaryAsync"/> alone.</para>
    /// </remarks>
    internal Func<string, ITransmitAudioSink> TransmitSinkFactory { get; set; } =
        name => new WasapiTransmitSink(name);

    /// <summary>
    /// **Builds the one armed send where, and only where, both halves exist.**
    /// </summary>
    /// <param name="port">The connected radio's serial port, or null where none.</param>
    /// <remarks>
    /// <para>**BOTH, OR NOTHING.** A transmission needs a wire to key the radio
    /// with and a device to play the tones into. Where either is missing
    /// <c>_armedSend</c> stays null and the click lands on a refusal that says
    /// which one - **never on a guess.** In particular there is no fallback to
    /// the machine's default endpoint: <see cref="WasapiTransmitSink"/> refuses
    /// to pick one on purpose, because FT8 through the laptop speakers while the
    /// operator believes he is on the air is the failure that refusal exists to
    /// prevent (0.0).</para>
    /// <para>**IT IS BUILT AT CONNECT AND NOT AT THE CLICK, AND THAT IS
    /// DELIBERATE.** The sink's constructor throws where the named endpoint is
    /// gone - a device id survives a driver update but not being moved to another
    /// socket - and an exception raised inside a click handler would reach the
    /// operator as a crash while he was answering a CQ. Here it is caught, the
    /// send stays unarmed, and the reserved Send area says the named device was
    /// not found.</para>
    /// <para>**IT ADDS NO SECOND ROUTE TO <see cref="Ft8ArmedSend.Arm"/>.** It
    /// constructs one; the only line that arms it is <see cref="SendMessage"/>.</para>
    /// </remarks>
    internal void BuildTheArmedSend(ISerialPort? port)
    {
        _armedSend = null;
        _transmitRefusal = "";
        _transmitSampleRate = Ft8Composer.DefaultSampleRate;

        var endpoint = (_settings.AudioOutputDeviceId ?? "").Trim();

        if (port is null && endpoint.Length == 0)
        {
            // The wording every existing test of this refusal was written
            // against, kept verbatim for the case that has not changed.
            return;
        }

        if (port is null)
        {
            _transmitRefusal =
                "no radio with a serial port is connected. The training radio is a "
                + "simulator and has no port, so nothing can be keyed through it";
            return;
        }

        if (endpoint.Length == 0)
        {
            _transmitRefusal =
                "no transmit audio device is named in Settings. That is the radio's "
                + "own USB audio input, and Hamlet will not choose one for you: "
                + "playing FT8 into whatever the computer defaults to is not "
                + "transmitting";
            return;
        }

        ITransmitAudioSink sink;

        try
        {
            sink = TransmitSinkFactory(endpoint);
        }
        catch (Exception ex)
        {
            // **THE STALE NAME, CAUGHT WHERE IT IS CHEAP.** A click must not put
            // an exception in front of an operator.
            _transmitRefusal =
                "the transmit audio device named in Settings could not be opened: "
                + ex.Message;

            DigitalSendLine =
                "Hamlet cannot transmit: " + _transmitRefusal + ".";

            return;
        }

        // **THE RATE IS READ OFF THE SINK, HERE, AND NOT AT THE CLICK.**
        // `Ft8TransmitSequence` writes PTT on at its line 283 and reaches the
        // sink at 287, so an endpoint whose rate FT8 cannot be built at has to be
        // refused where the sink is constructed - one frame later and the radio
        // is already keyed when anybody finds out.
        var rate = sink.EndpointSampleRate;

        if (!Ft8Composer.RateIsUsable(rate, out var whyNot))
        {
            _transmitRefusal =
                "the transmit audio device named in Settings, \"" + endpoint
                + "\", speaks " + rate.ToString(CultureInfo.InvariantCulture)
                + " samples per second, and an FT8 transmission cannot be built at "
                + "that rate: " + whyNot
                + " Nothing can be sent through this device, so choose another "
                + "transmit audio device in Settings";

            DigitalSendLine =
                "Hamlet cannot transmit: " + _transmitRefusal + ".";

            return;
        }

        _transmitSampleRate = rate;

        // **THE ONE THING THIS VIEW MODEL KEEPS OF THE SINK, AND IT IS READ-ONLY**
        // (work instruction 269, task 3). The sink was a local here and was
        // dropped, so the two figures it measures on the way out - the peak the
        // endpoint was actually handed and how many samples had to be clamped -
        // reached nothing in `src/` and the operator was shown the composed peak
        // instead, which is his own drive setting read back.
        //
        // **IT IS NOT A SECOND ROUTE TO ANYTHING.** `ITransmitLevelReport` has
        // two properties and no methods; nothing on it is called while a
        // transmission is running, and it is asked only after
        // `AtBoundaryAsync` has already returned. What keys a radio is
        // `Ft8TransmitSequence`, reached from `Ft8ArmedSend.AtBoundaryAsync`
        // alone, and neither of those changed to carry this.
        _transmitLevelReport = sink as ITransmitLevelReport;

        _armedSend = new Ft8ArmedSend(
            new Ft8TransmitSequence(port, sink, _sendLicence, _telemetry));
    }

    /// <summary>The rate the armed send's endpoint declared, and composes at.</summary>
    /// <remarks>
    /// <para>**IT IS WHAT THE ENDPOINT SAID, NOT A SETTING AND NOT A DEFAULT**
    /// (work instruction 262). <see cref="BuildTheArmedSend"/> reads it off the
    /// sink it has just built; <see cref="SendMessage"/> composes at it. The two
    /// are separated by a click, which is why it is kept rather than fetched -
    /// nothing between them may open a device.</para>
    /// <para>**IT FALLS BACK TO THE DECODER'S RATE ONLY WHERE NOTHING IS ARMED**,
    /// which is the case where the click is refused in words before any audio is
    /// wanted. It is never a fallback on a live send path: where an endpoint's
    /// rate is unusable, no armed send is built at all.</para>
    /// </remarks>
    private int _transmitSampleRate = Ft8Composer.DefaultSampleRate;

    /// <summary>The text of what is armed, so the ledger can be told what went.</summary>
    private string _armedText = "";

    /// <summary>The last boundary handed to the armed send, so it is handed once.</summary>
    private DateTime _lastBoundaryDriven;

    /// <summary>
    /// The sink's own read-only report, where the sink in use offers one.
    /// </summary>
    /// <remarks>
    /// **Null is a real answer and is said out loud on the screen.** Nothing
    /// requires an <see cref="ITransmitAudioSink"/> to implement
    /// <see cref="ITransmitLevelReport"/>, and a readout that quietly showed the
    /// composed peak while implying it was what left the card would be worse than
    /// no readout at all (§0.0).
    /// </remarks>
    private ITransmitLevelReport? _transmitLevelReport;

    /// <summary>What the reserved Send area says when nothing has gone out.</summary>
    internal const string NothingHasBeenSent =
        // **SHORTENED ON 2026-09-08, AND THE HOW-TO MOVED TO HOVER** (Tim: show,
        // do not tell). The fact is that nothing has gone out; how to make
        // something go out is advice, and advice does not occupy the screen
        // while he operates. `SendIdleTip` carries it a hover away.
        "nothing sent yet";

    /// <summary>How to send, for the hover behind the send line.</summary>
    /// <remarks>
    /// **THE SENTENCE IS NOT THROWN AWAY, IT IS MOVED.** Removing words must
    /// not remove facts, and for somebody who has never worked a station this
    /// one is the difference between a quiet screen and a usable one.
    /// </remarks>
    public static string SendIdleTip
        => "Right-click a decoded row to choose a message, or press CQ.";

    /// <summary>
    /// What is being sent, and to whom - the reserved area's one line.
    /// </summary>
    /// <remarks>
    /// **FORMATTED HERE SO A TEST CAN ASSERT IT WITHOUT OPENING A WINDOW**, the
    /// precedent <see cref="DigitalDecodeRow"/> and unit 258's contact cell set.
    /// The markup binds and does no arithmetic.
    /// </remarks>
    [ObservableProperty]
    private string _digitalSendLine = NothingHasBeenSent;

    /// <summary>What the send line's own hover holds, which follows the line.</summary>
    /// <remarks>
    /// **A STATIC TOOLTIP COULD NOT FOLLOW IT** (work instruction 281 task 3). The
    /// idle line and the after-a-send line say different things, so one fixed
    /// sentence behind both would have been right about one of them. Idle it says
    /// what the line will carry; once something has gone out it says what the
    /// composed level is, and what it cannot tell him (<see cref="ComposedLevelTip"/>).
    /// </remarks>
    public string DigitalSendLineTip
        => DigitalSendLine == NothingHasBeenSent ? SendIdleTip : ComposedLevelTip;

    partial void OnDigitalSendLineChanged(string value)
        => OnPropertyChanged(nameof(DigitalSendLineTip));

    /// <summary>
    /// The peak amplitude Hamlet builds a transmission at, as a percentage of
    /// full scale - **the same setting the Settings screen writes, on the tab.**
    /// </summary>
    /// <remarks>
    /// <para>**WHY IT IS HERE AS WELL AS IN SETTINGS** (work instruction 269,
    /// task 2). `PHASE_PLAN.md` step D's first exit criterion is *"Tim sets the
    /// Transmit drive control and reads the dBFS and clip count under the
    /// waterfall"*. The only drive control was <c>TransmitDriveBox</c> in
    /// <c>SettingsWindow.axaml:262</c>, reached through
    /// <see cref="OpenSettingsAsync"/>, which shows the settings window with
    /// <c>ShowDialog</c> - a second top-level window over the waterfall, over the
    /// decode table and over <c>DigitalStopButton</c>, while the 250 ms
    /// <c>_decodeTimer</c> keeps driving slot boundaries behind it. Setting a
    /// drive between two fifteen-second slots meant hiding the band and the Stop
    /// button to do it.</para>
    /// <para>**THE SETTINGS CONTROL STAYS.** Nothing was taken off that screen;
    /// this is a second view of one setting, not a move. Both write
    /// <see cref="AppSettings.TransmitDrivePeak"/> through
    /// <see cref="TransmitDrive"/>, so the conversion, the composer's question
    /// and the note sentence are one piece of code and cannot drift apart.</para>
    /// <para>**PERCENT ON THE SCREEN, PEAK IN THE FILE.** A spinner showing
    /// `0.25` is a control an operator has to be taught to read and `25 %` is one
    /// he does not.</para>
    /// </remarks>
    [ObservableProperty]
    private double _transmitDrivePercent;

    partial void OnTransmitDrivePercentChanged(double value)
    {
        // ASKED OF THE COMPOSER, NOT DECIDED AGAIN HERE, AND NOT DECIDED TWICE:
        // this is the same call `SettingsViewModel` makes. A value it would
        // refuse is not written at all and the note says which level is still in
        // force.
        TransmitDrive.Write(_settings, value);

        OnPropertyChanged(nameof(TransmitDriveNote));
    }

    /// <summary>
    /// What the drive works out to in dBFS, and what it is for - **on the screen
    /// before anything has been transmitted.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE HALF OF STEP D'S FIRST CRITERION NOTHING IN THE TREE
    /// COULD DO.** The only level Hamlet showed anywhere was inside
    /// <see cref="DigitalSendLine"/>, which reads
    /// <see cref="NothingHasBeenSent"/> until a transmission has already gone
    /// out - so an operator setting a drive against his radio's ALC had to
    /// transmit once to find out what he had just set. This one is there as the
    /// control moves and before the radio is keyed.</para>
    /// <para>**IT IS THE LEVEL HAMLET WILL COMPOSE AT AND IT SAYS SO** - a
    /// setting, not a measurement. What was actually handed to the sound card is
    /// a different sentence, after a send, and the two are deliberately
    /// worded apart.</para>
    /// </remarks>
    public string TransmitDriveNote
        => TransmitDrive.NoteFor(TransmitDrivePercent, _settings.TransmitDrivePeak);

    /// <summary>How to set the drive, behind the Radio tips mark.</summary>
    /// <remarks>
    /// **MOVED, NOT DELETED** (work instruction 280 task 8). The advice is useful
    /// the first time somebody sets this and is noise every time after, so it is a
    /// hover rather than a paragraph on the screen he operates from.
    /// </remarks>
    public static string TransmitDriveTip => TransmitDrive.Tip;

    /// <summary>What the reserved Send area says before anything has gone out.</summary>
    internal const string NothingHasBeenMeasured =
        // **SHORTENED ON 2026-09-08** (Tim: show, do not tell). What this line
        // will say after a send is a description of itself, which is the purest
        // form of telling. It is on hover.
        "no level measured yet";

    /// <summary>What the level line will carry, for its hover.</summary>
    /// <remarks>
    /// **THE §0.0 BOUNDARY IS THE PART THAT MATTERS AND IT IS KEPT VERBATIM.**
    /// *What the sound card was actually handed* is a statement about what
    /// Hamlet can and cannot observe, not advice, and it is why this line exists
    /// at all.
    /// </remarks>
    public static string TransmitLevelIdleTip
        => "After a send this line says what the sound card was actually handed, "
            + "which is the last thing Hamlet can see before the radio.";

    /// <summary>
    /// **The level the sound card was actually handed, and what had to be
    /// clamped** - the measurement, as against the setting above it.
    /// </summary>
    /// <remarks>
    /// <para>**IT IS A DIFFERENT QUANTITY FROM <see cref="DigitalSendLine"/>'S
    /// AND THE TWO SENTENCES ARE WORDED APART** (work instruction 269, task 3).
    /// That line carries <c>LevelLine</c>, which is
    /// <see cref="Ft8Transmission.PeakSample"/> - the peak of the array the
    /// composer produced, which is the drive setting read back - and a clip count
    /// over that same array, which the composer built inside the rails and which
    /// is therefore zero by construction. This one is
    /// <see cref="ITransmitLevelReport.PeakWritten"/> and
    /// <see cref="ITransmitLevelReport.ClippedSamples"/>, measured by the sink on
    /// the way out to the endpoint. **At the radio that is the difference between
    /// a level he has verified and a number he typed**, and unit 265 recorded
    /// that every reader of those two figures in the repository was a test.</para>
    /// <para>**IT NEVER SHOWS A NUMBER WITHOUT SAYING WHICH NUMBER IT IS.** Where
    /// the sink offers no report the line says there is no measurement rather
    /// than falling back to the composed peak in a measurement's words - a
    /// readout that did that would be worse than no readout (§0.0).</para>
    /// <para>**WHAT LIES BEYOND IT IS STILL NAMED AND NOT FOLDED IN**: Windows'
    /// own volume for that endpoint, and then the radio's own USB input gain and
    /// its ALC. `SHACK_FACTS.md` FACT-004 - what the IC-7300's USB modulation
    /// input expects is not in this repository, so this is a measurement of what
    /// left Hamlet and never advice about a drive level.</para>
    /// </remarks>
    [ObservableProperty]
    private string _digitalTransmitLevelLine = NothingHasBeenMeasured;

    /// <summary>What the sink says it handed the endpoint, in the operator's terms.</summary>
    /// <param name="report">The sink's own report, or null where it offers none.</param>
    /// <returns>One sentence, naming the quantity and what the count counts.</returns>
    private static string MeasuredLevelLine(ITransmitLevelReport? report)
    {
        // **A DEVICE THAT WILL NOT SAY IS A FAULT AND SPEAKS UNASKED** (Tim,
        // 2026-09-08). Short, because a fault is a sentence and not a paragraph;
        // what the level above actually is stays with it, since reading a drive
        // setting as a measurement is the misreading this line exists to stop.
        if (report is null)
        {
            return "this transmit device reports no level";
        }

        var peak = report.PeakWritten;

        var level = peak > 0.0
            ? (20.0 * Math.Log10(peak)).ToString("0.0", CultureInfo.InvariantCulture) + " dBFS"
            : "silence";

        // **THE FACTS AND NOTHING ROUND THEM** (work instruction 281 task 3): what
        // the card got, and whether anything had to be clamped to hand it over. The
        // clamp sentence and the boundary past the sound card are in
        // <see cref="TransmitLevelTip"/>, one hover away.
        return report.ClippedSamples == 0
            ? "sound card got " + level + " · nothing clamped"
            : "sound card got " + level + " · "
              + report.ClippedSamples.ToString(CultureInfo.InvariantCulture)
              + " samples clamped";
    }

    /// <summary>What the measured level means, and what lies past it.</summary>
    /// <remarks>
    /// <para>**THE AUTHOR TOLD UNIT 280 TO KEEP THESE VISIBLE AND THAT WAS WRONG**
    /// (work instruction 281 task 3). They are facts, and a hover preserves a fact
    /// exactly as well as a paragraph does.</para>
    /// <para>**THE BOUNDARY IS THE HALF THAT MATTERS** (§0.0): what Hamlet handed
    /// the sound card is not what left the radio, because Windows' own volume for
    /// that endpoint and the radio's input gain are both past the last thing Hamlet
    /// can see. `SHACK_FACTS.md` FACT-004.</para>
    /// </remarks>
    public const string TransmitLevelTip =
        "That is the peak the sound card actually got, measured on the way out "
        + "after clamping, and not the level Hamlet composed at. Anything clamped "
        + "was a sample too loud to hand over, trimmed to fit. Beyond this point "
        + "are Windows' own volume for that device and the radio's input gain, "
        + "which Hamlet cannot see.";

    /// <summary>What the Send area says about a contact before anything is sent.</summary>
    internal const string NoContactStandsYet =
        // **SHORTENED ON 2026-09-08** (Tim: show, do not tell). Two hundred and
        // forty-two characters describing what a line would say if there were
        // anything to say. The description is on hover.
        "no contact yet";

    /// <summary>What the contact line will carry, for its hover.</summary>
    public static string ContactStandsIdleTip
        => "After a transmission this line says where that contact stands: the "
            + "station, the state, the slot count and the slot it was read at.";


    /// <summary>
    /// **Where the contact stands after the operator's own last transmission**,
    /// with the moment it was read at.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE THING NOTHING IN HAMLET COULD SAY** (work instruction 270).
    /// A great many contacts end on the operator's own message: he answers a CQ,
    /// the station comes back `KC3QIS W1ABC R-09` - a report and a roger in one
    /// field - and his `RRR` is what satisfies
    /// <see cref="Ft8ContactStates.IsComplete"/>. <see cref="PlaceRow"/> computes
    /// a row's contact cell once, at that row's own slot, and
    /// <see cref="Ft8ContactLedger.RecordSent"/> writes into a private ledger and
    /// touches nothing already on the table - **so the newest row on screen is
    /// the one placed before his last transmission and it still reads *your
    /// move*.** If the station then goes quiet, nothing said the contact
    /// finished, and at the rig that is a man re-sending into a contact that
    /// ended or waiting for a station that has finished with him.</para>
    /// <para>**THE PRESENT STATE GOES IN A LINE ABOUT THE PRESENT AND NOT INTO
    /// THE PAST.** No row already on the table is rewritten.
    /// <see cref="ContactTextFor"/>'s contract is that a row shows where the
    /// contact stood in its own slot and never restates itself, and a table of
    /// moments that edits its own moments is a worse instrument than one that is
    /// merely incomplete.</para>
    /// <para>**IT SAYS WHEN IT WAS READ** (§0.0). A screen that says *complete*
    /// with no idea when is the same class of fault as unit 269's readout showing
    /// a setting and calling it a measurement, so the slot is on the face of the
    /// sentence and the count of slots comes with the state.</para>
    /// <para>**IT REPORTS AND IT RULES NOTHING.** It closes nothing, hides
    /// nothing, greys nothing and suggests nothing: after `complete` the row's
    /// menu offers exactly what it offered before. Hamlet is not the radio
    /// police.</para>
    /// </remarks>
    [ObservableProperty]
    private string _digitalContactStandsLine = NoContactStandsYet;

    /// <summary>Where the contact the operator just transmitted into stands.</summary>
    /// <param name="message">What actually went out.</param>
    /// <param name="slotUtc">The slot it went out in, which is the moment read at.</param>
    /// <returns>One line, in the register the rest of this area uses.</returns>
    /// <remarks>
    /// <para>**IT ASKS AND IT DECIDES NOTHING** (§12.1). The addressee comes from
    /// <see cref="Ft8MessageSplit.Split(string?)"/> and
    /// <see cref="Ft8MessageSplit.IsCallToAnyone(string?)"/> - **the same two
    /// questions <see cref="Ft8ContactLedger.RecordSent"/> asks of the same two
    /// methods**, so this cannot come to a different answer about who a message
    /// was addressed to. The state and its slot count come from
    /// <see cref="Ft8ContactStates.Read"/>. Nothing about completeness, about the
    /// four words or about the shape of a message is worked out here.</para>
    /// <para>**AND IT INVENTS NO STATION** (§0.0). A call to anyone books nobody,
    /// which is what the ledger already does, and a message the splitter refuses
    /// names nobody either. In both cases the line says there is no contact to
    /// report on rather than reaching for the newest station it can find.</para>
    /// <para>**THE MOMENT IS THE SLOT THE TRANSMISSION WENT OUT IN**, not the
    /// wall clock at the post. It is the value <see cref="AtSlotBoundaryAsync"/>
    /// books the send at, and the same convention <see cref="ContactTextFor"/>
    /// uses for a row - two readers of one ledger that counted slots from
    /// different moments would print different numbers for the same contact.</para>
    /// </remarks>
    private string ContactStandsLine(string? message, DateTime slotUtc)
    {
        var slot = slotUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var fields = Ft8MessageSplit.Split(message);

        if (fields is null)
        {
            return "Hamlet cannot tell who that was addressed to - it is not a "
                + "standard three-field message - so it has no contact to report "
                + "on. Where a message names a station, this line says where that "
                + "contact stands.";
        }

        if (Ft8MessageSplit.IsCallToAnyone(fields.To))
        {
            return "That was a call to anyone, so it is addressed to no station "
                + "and there is no one contact to report on yet. This line says "
                + "where a contact stands after a message sent to a station.";
        }

        var record = _contacts?.For(fields.To);

        if (record is null)
        {
            return "Nothing is on file with " + fields.To + " yet, so there is "
                + "nothing to report about where that contact stands.";
        }

        var read = Ft8ContactStates.Read(record, slotUtc);

        return "Where the contact with " + read.Callsign + " stands: " + read.Text
            + ", read at the " + slot + " UTC slot. That is what passed between "
            + "you, counted in slots; it is not advice about what to send next, "
            + "and nothing is closed or withheld by it.";
    }

    /// <summary>Every message the operator may send to one row's station.</summary>
    /// <param name="row">The row he right-clicked.</param>
    /// <returns>The menu, or null where the row names no station.</returns>
    /// <remarks>
    /// <para>**NOTHING IS WITHHELD HERE EITHER.** This hands
    /// <see cref="Ft8SendOptions"/> what it needs and returns what it says; there
    /// is no filtering step between the two, on the contact state or on anything
    /// else.</para>
    /// <para>**THE REPORT IS THE ROW'S OWN MEASURED RATIO**, which is what a
    /// signal report is. A row whose ratio was not measured carries
    /// <see cref="DigitalDecodeRow.NoMeasurement"/> and the report-bearing
    /// messages are absent with the reason said, rather than a number being
    /// invented (§0.0).</para>
    /// </remarks>
    public Ft8SendMenu? SendMenuFor(DigitalDecodeRow? row)
    {
        // **A ROW HE SENT OFFERS NOTHING TO SEND** (Tim's ruling, 2026-09-08).
        // The sender of his own transmission is his own callsign, so without this
        // line the menu would compose replies addressed from him to him and offer
        // to transmit them. There is no station on that row to answer.
        if (row is not null && row.IsSent)
        {
            return null;
        }

        if (row is null || _contacts is null)
        {
            return null;
        }

        var record = _contacts.For(row.Sender);

        return record is null
            ? null
            : Ft8SendOptions.For(
                record,
                _settings.Operator.Callsign?.Trim() ?? "",
                _settings.Operator.GridSquare,
                MeasuredReport(row));
    }

    /// <summary>
    /// Every callsign already in the log, as logged, for the worked-before mark.
    /// </summary>
    /// <remarks>
    /// **READ ONCE AND KEPT, NOT READ PER DECODE** (work instruction 274 task 5's
    /// own concern). Fourteen messages a slot and four slots a minute is fifty-six
    /// checks a minute; a file read behind each of them is free tonight and slow in
    /// March. It is refreshed when a contact is logged, which is the only thing in
    /// the application that changes the file.
    /// </remarks>
    private Dictionary<string, AdifContact>? _workedBefore;

    /// <summary>Re-read the log after it changed.</summary>
    /// <remarks>
    /// **THE LAST ENTRY FOR A CALLSIGN WINS**, so the mark says when he last worked
    /// somebody rather than when he first did.
    /// </remarks>
    private void RefreshWorkedBefore()
    {
        _workedBefore = ReadWorkedBefore();

        // **ROWS ALREADY ON SCREEN PICK THE MARK UP.** He logs a contact and the
        // station's other rows from the same evening say *worked* at once, rather
        // than only the rows that arrive after it.
        foreach (var row in DigitalDecodes)
        {
            row.WorkedBefore = WorkedBeforeNote(row.Sender);
        }

        OnPropertyChanged(nameof(LoggedContacts));
        OnPropertyChanged(nameof(ContactCountLine));
        OnPropertyChanged(nameof(ContactBeltInk));
        OnPropertyChanged(nameof(ContactBeltProgress));
        OnPropertyChanged(nameof(ContactBeltTip));
        OnPropertyChanged(nameof(HasContactBadgeProgress));
        OnPropertyChanged(nameof(HasLoggedContacts));
        OnPropertyChanged(nameof(Milestones));
    }

    /// <summary>How many records are in the contact log.</summary>
    /// <remarks>
    /// <para>**IT IS THE NUMBER OF RECORDS AND NOTHING ELSE** (Tim's ruling,
    /// 2026-09-08). Not distinct callsigns, not confirmed contacts, not an
    /// estimate. Work the same station on three bands and that is three.</para>
    /// <para>**THE DICTIONARY BESIDE IT WOULD HAVE BEEN THE WRONG NUMBER.**
    /// `_workedBefore` is keyed by callsign and drops any record with no `CALL`,
    /// so its `Count` is distinct stations. Reaching for it would have understated
    /// his own operating on the screen and in whatever he told somebody about it,
    /// which is §0.0 exactly: a number that is not the number of records is a
    /// false claim, and it is the kind of number a person repeats.</para>
    /// <para>**A DAMAGED RECORD IS COUNTED.** He made that contact; the file was
    /// cut off or a field went bad afterwards. Leaving it out would make the count
    /// disagree with the log window standing beside it, which lists it. The window
    /// says separately how many could not be read whole.</para>
    /// </remarks>
    /// <para>**IT READS THE FILE ON FIRST ASK RATHER THAN AT STARTUP.** The
    /// status bar binds before a single decode has arrived, and the mark's own
    /// read is lazy for the same reason it always was, so the first of the two to
    /// be wanted pays for the read and the other gets it free.</para>
    public int LoggedContacts
    {
        get
        {
            _workedBefore ??= ReadWorkedBefore();

            return _loggedContacts;
        }
    }

    private int _loggedContacts;

    /// <summary>Where he stands against the badges.</summary>
    /// <remarks>
    /// **DERIVED, NEVER STORED.** Reading it off the count every time is what
    /// keeps a badge from outliving the records behind it.
    /// </remarks>
    public ContactMilestones Milestones => new(LoggedContacts);

    /// <summary>What Hamlet said about a badge, once, or "".</summary>
    /// <remarks>
    /// **ONCE, QUIETLY, AND NOT IN A DIALOG** (work instruction 278). It sits in
    /// the status bar beside the count. He is often mid-exchange with fifteen
    /// seconds to answer in, and a modal window there costs him the contact it is
    /// congratulating him for.
    /// </remarks>
    public string ContactBadgeLine
    {
        get
        {
            _workedBefore ??= ReadWorkedBefore();

            return _contactBadgeLine;
        }
    }

    /// <summary>True where there is a badge to mention.</summary>
    public bool HasContactBadge => ContactBadgeLine.Length > 0;

    private string _contactBadgeLine = "";

    /// <summary>Say it once if the count has crossed something.</summary>
    /// <param name="milestones">Where the count now stands.</param>
    /// <remarks>
    /// <para>**EVERY BADGE THE JUMP PASSED, NOT ONLY THE HIGHEST.** A quiet evening
    /// on FT8 crosses two of these at once, and naming only the top one would
    /// swallow a milestone he actually reached.</para>
    /// <para>**AND IT IS WRITTEN DOWN SO IT IS NOT SAID TWICE**, which is a fact
    /// about what Hamlet has said rather than about what the log holds. The badges
    /// themselves stay derived from the count, so nothing here can make one outlive
    /// its records.</para>
    /// </remarks>
    private void AnnounceBadges(ContactMilestones milestones)
    {
        var was = _settings.ContactBadgeAnnounced;

        // **THE FIRST LOOK SEEDS AND SAYS NOTHING** (measured in task 5). Hamlet
        // installed beside a log of ten thousand contacts would otherwise
        // congratulate him for all nine badges in one line, which is a wall of
        // text rather than the quiet acknowledgement he asked for. An
        // acknowledgement is for a milestone he has just passed, and Hamlet was
        // not there for the others.
        if (was < 0)
        {
            _settings.ContactBadgeAnnounced = milestones.Highest ?? 0;
            SettingsStore.Save(_settings);

            return;
        }

        var line = milestones.Announcement(was);

        if (line.Length == 0)
        {
            return;
        }

        _contactBadgeLine = line;
        _settings.ContactBadgeAnnounced = milestones.Highest ?? was;
        SettingsStore.Save(_settings);
    }

    /// <summary>True once anything has been logged.</summary>
    public bool HasLoggedContacts => LoggedContacts > 0;

    /// <summary>The count, as the main screen says it.</summary>
    /// <remarks>
    /// **QUIETLY** (work instruction 278). It sits in the status bar rather than
    /// anywhere it competes with the band, the frequency or a decode. A number
    /// that grows a few times an evening does not need to announce itself.
    /// </remarks>
    /// <para>**IT STOPPED BEING A WHISPER ON 2026-09-08** (Tim: *the contacts are
    /// understated*). It was a sentence at the end of a status bar carrying a
    /// paragraph. It is a number now, drawn large, and the words beside it are
    /// small: **what he is building is the count, so the count is what is big.**
    /// </para>
    public string ContactCountLine
        => LoggedContacts.ToString("N0", CultureInfo.InvariantCulture);

    /// <summary>The rank the count wears, as a hex colour for the ring.</summary>
    /// <remarks>
    /// **A BORDER AND NEVER A FILL** (HM-DEC-012, work instruction 281 task 4). The
    /// markup binds this to a `BorderBrush` with a transparent background; a filled
    /// disc in a status bar reads as a status light, which is a different claim.
    /// </remarks>
    public string ContactBeltInk => ContactBelt.For(LoggedContacts).Ink;

    /// <summary>How far the count is between this rank and the next, 0 to 1.</summary>
    public double ContactBeltProgress => ContactBelt.Progress(LoggedContacts);

    /// <summary>The rank, the next colour, and how many contacts away.</summary>
    /// <remarks>
    /// **THE WORDS THAT LEFT THE SCREEN** (Tim, 2026-09-08). *contacts* beside the
    /// number and *6 to 10* beside the bar are both in this sentence, and the number
    /// and the ring carry them unhovered.
    /// </remarks>
    public string ContactBeltTip => ContactBelt.Tip(LoggedContacts);

    /// <summary>True where there is a next rank to show progress toward.</summary>
    /// <remarks>
    /// **THE LABEL BECAME A BAR ON 2026-09-08** (work instruction 281 task 4). This
    /// used to gate the words *6 to 10*; it now gates
    /// <see cref="ContactBeltProgress"/>, which is the same fact drawn instead of
    /// written. At the gold there is nothing further to go and the bar is not drawn.
    /// </remarks>
    public bool HasContactBadgeProgress => ContactBelt.After(LoggedContacts) is not null;

    /// <summary>The log, by callsign, with the last entry for each winning.</summary>
    /// <remarks>
    /// **ONE READ OF THE FILE, TWO DERIVATIONS FROM IT** (work instruction 278
    /// task 3). The count and the mark answer different questions of the same
    /// records, and a second pass over the file to answer the second one would be
    /// a second place for the two to disagree.
    /// </remarks>
    private Dictionary<string, AdifContact> ReadWorkedBefore()
    {
        var records = ContactLogStore.ReadRecords();

        _loggedContacts = records.Count;

        var worked = new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in records)
        {
            // **THE MARK KEEPS THE SOURCE IT ALWAYS HAD.** It was built from
            // `Read`, which leaves out a record with no end marker, and a station
            // marked worked off a half-written line is a claim this unit was not
            // asked to make. The count is the thing that changed, not the mark.
            if (!record.Terminated)
            {
                continue;
            }

            var entry = record.Contact;

            if (!string.IsNullOrWhiteSpace(entry.Call))
            {
                worked[entry.Call.Trim()] = entry;
            }
        }

        AnnounceBadges(new ContactMilestones(_loggedContacts));

        return worked;
    }

    /// <summary>Hand the log in, for a test that has no file to read.</summary>
    /// <param name="worked">The log, by callsign, as `ReadWorkedBefore` builds it.</param>
    /// <remarks>
    /// <para>**THE DEVELOPMENT MACHINE HAS NO CONTACT LOG AND NEVER WILL** (Tim's
    /// ruling, 2026-09-08; `SHACK_FACTS.md`). Contacts are logged where the radio
    /// is, so a test whose subject is the *worked* mark has nothing to read here
    /// unless it writes a file first. Writing one is what
    /// `TheLogDialogAndTheWorkedMarkTests` does through the redirected data folder,
    /// and that is right where the file itself is the subject; where the subject is
    /// what the row does about it, a file is a slower way of saying the same thing.
    /// </para>
    /// <para>**IT SETS THE SAME FIELD THE READ SETS AND NOTHING ELSE.** No file is
    /// touched, and the rows already on screen pick the mark up exactly as they do
    /// after a contact is logged.</para>
    /// </remarks>
    internal void UseWorkedBeforeForTests(Dictionary<string, AdifContact> worked)
    {
        _workedBefore = worked;

        foreach (var row in DigitalDecodes)
        {
            row.WorkedBefore = WorkedBeforeNote(row.Sender);
        }
    }

    /// <summary>Re-read the log, for a test that wrote to it directly.</summary>
    internal void ReloadContactLogForTests() => RefreshWorkedBefore();

    /// <summary>What the mark says about a station, or "" where he has not worked it.</summary>
    /// <remarks>
    /// <para>**MATCHING IS ON THE CALLSIGN EXACTLY AS LOGGED, AND COMPOUND CALLS
    /// ARE DIFFERENT STATIONS.** `W4/YV7AXM` and `YV7AXM` are arguably the same
    /// operator and arguably not: the second is a Venezuelan station at home and
    /// the first is the same licensee transmitting from Florida, which is a
    /// different DXCC entity and a different contact to most award programmes. The
    /// instruction says that where the answer is not certain, treat them as
    /// different — so a portable form is **not** marked as already worked, and the
    /// mark under-claims rather than over-claims.</para>
    /// <para>**IT MARKS AND IT RULES NOTHING.** Working somebody twice is his
    /// choice, on another band or another day, and nothing here hides, disables or
    /// sorts away a row.</para>
    /// </remarks>
    private string WorkedBeforeNote(string? callsign)
    {
        var call = (callsign ?? "").Trim();

        if (call.Length == 0 || _workedBefore is null
            || !_workedBefore.TryGetValue(call, out var entry))
        {
            return "";
        }

        // **TIM'S OWN WORDING, 2026-09-08**, replacing unit 274's. The band came
        // out with it: a date is what he wants to know when a callsign looks
        // familiar, and the band was a third clause on a hover that already had
        // two.
        // **THE DATE IS HIS FORMAT AND NOT ISO** - `09/07/26` - because this is a
        // sentence he reads rather than a field anything parses.
        var when = entry.StartedUtc is { } at
            ? at.ToString("MM/dd/yy", CultureInfo.InvariantCulture)
            : "a date Hamlet did not record";

        return $"You logged a contact with {call} on {when}";
    }

    /// <summary>Whether this row is one the Log item belongs on.</summary>
    /// <param name="row">The row the mouse was over.</param>
    /// <returns>True where the message is addressed to the operator.</returns>
    /// <remarks>
    /// **THE SAME QUESTION THE MINE SIDE AND THE CONTACT COLUMN ASK**, and asked
    /// of the one place that answers it (§0). A Log item on a CQ would offer to
    /// write down a contact that has not happened.
    /// </remarks>
    public bool CanLogRow(DigitalDecodeRow? row)
        => row is not null
           && Ft8MessageSplit.IsAddressedTo(row.Message, _settings.Operator.Callsign);

    /// <summary>Open the Log dialog for one row, and write it if he saves.</summary>
    /// <param name="row">The row the mouse was over.</param>
    /// <remarks>
    /// <para>**IT TRANSMITS NOTHING AND TOUCHES NO SEND PATH.** Logging is
    /// bookkeeping over messages that already passed.</para>
    /// <para>**THE DIAL IS READ NOW AND SHOWN, NOT ASSUMED** (task 1's second
    /// finding). The frequency here is where the radio is **when he right-clicks**,
    /// which is not necessarily where the contact happened if he has retuned
    /// since. Rather than record a band he may not have worked the station on, the
    /// dialog shows the frequency it is about to write and he can see it before he
    /// saves. Hamlet does not know the contact's own dial: the row carries its slot
    /// and not the tuning, and inventing one would be §0.0 exactly.</para>
    /// <para>**CANCEL WRITES NOTHING**, which is guaranteed by this method writing
    /// only where the dialog says Save was pressed. The window never touches a
    /// file.</para>
    /// </remarks>
    [RelayCommand]
    private async Task LogContactAsync(DigitalDecodeRow? row)
    {
        if (row is null || _contacts is null || !CanLogRow(row))
        {
            return;
        }

        var record = _contacts.For(row.Sender);

        if (record is null)
        {
            return;
        }

        // **THE DIAL THE ROW WAS HEARD ON, NOT THE ONE THE RADIO IS ON NOW**
        // (work instruction 275 task 3). A contact logged after he retuned used to
        // record the band he had moved to.
        //
        // **ZERO MEANS NOT RECORDED AND THE FIELDS GO OUT ENTIRELY.** Anything
        // decoded before this change carries no dial, and a plausible frequency in
        // a permanent record is the fault §0.0 exists for.
        var hz = row.HeardOnHz;
        var band = hz > 0 ? HfBands.BandFor(hz) : null;

        var entry = Ft8ContactLogEntry.For(
            record,
            _settings.Operator.Callsign,
            new Ft8StationConditions(
                hz,
                band?.Name,
                "FT8",
                _settings.Operator.GridSquare));

        var model = new LogContactViewModel(
            entry,
            hz > 0
                ? (hz / 1_000_000.0).ToString("0.000000", CultureInfo.InvariantCulture)
                  + " MHz, where this was heard"
                : "");

        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        await new Views.LogContactWindow { DataContext = model }
            .ShowDialog(desktop.MainWindow);

        if (!model.Saved)
        {
            return;
        }

        var written = ContactLogStore.Append(model.Entry, AboutViewModel.AppVersion);

        // **A FAILED WRITE IS SAID OUT LOUD.** A log entry that silently did not
        // land is worse than one that never existed: he would believe the contact
        // was written down (§0.0.1).
        _digitalDecodeNote = written
            ? $"Logged {entry.Call} to {ContactLogStore.LogPath}."
            : "Hamlet could not write to the contact log at "
              + ContactLogStore.LogPath + ", so nothing was logged.";

        if (written)
        {
            RefreshWorkedBefore();
        }

        RaiseDigitalDecodeChanges();
    }

    /// <summary>The row's own ratio, in whole decibels, or null where none.</summary>
    private static int? MeasuredReport(DigitalDecodeRow row)
        => int.TryParse(
            row.Snr, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
            out var decibels)
            ? decibels
            : null;

    /// <summary>
    /// **THE ONE ENTRY POINT. Arms exactly one transmission for the next slot.**
    /// </summary>
    /// <param name="text">The message, exactly as it would go on the air.</param>
    /// <remarks>
    /// <para>**NO CONFIRMATION, NO DIALOG, NO SECOND CLICK** (ruled 2026-09-06).
    /// The click is the decision; there is no armed state the operator has to
    /// approve and nothing here opens a window.</para>
    /// <para>**NOTHING BUT A CLICK REACHES THIS.** No decode, no timer, no state
    /// change and no retry calls it: the slot tick can only *fire* what is
    /// already armed and has no way to arm anything, because
    /// <see cref="Ft8ArmedSend.Arm"/> is called from this method and from no
    /// other line in <c>src/</c>.</para>
    /// <para>**A SECOND CLICK REPLACES; IT NEVER ADDS.**
    /// <see cref="Ft8ArmedSend"/> holds one field, so two clicks a second apart
    /// are one transmission in the next slot.</para>
    /// <para>**THE CLOCK IS READ HERE BECAUSE THE CLICK IS WHEN.** Which slot is
    /// next is a fact about the moment the operator clicked, and this is the
    /// shell, where <see cref="OnSlotTick"/>'s own remark already puts the clock.
    /// Nothing deeper reads one.</para>
    /// </remarks>
    [RelayCommand]
    private void SendMessage(string? text)
    {
        var wanted = (text ?? "").Trim();

        if (wanted.Length == 0)
        {
            DigitalSendLine = "There was nothing to send.";
            return;
        }

        // **AT THE RATE THE ENDPOINT DECLARED, NOT AT THE DECODER'S.**
        // `_transmitSampleRate` was read off the sink when the radio connected.
        // Composing at `Ft8Waveform.DefaultSampleRate` here is what made every
        // send through a real endpoint throw after the radio was keyed (work
        // instruction 262, task 1).
        // **AND AT THE DRIVE LEVEL THE OPERATOR SET**, read off the settings the
        // same way `_transmitSampleRate` is. One line, one place: this is the
        // only `ComposeSignal` call site in the whole of `src/`, so the CQ button
        // and the right-click send are the same call and cannot get different
        // levels. Before unit 265 there was no argument here and every
        // transmission went out at unit amplitude - 0 dBFS - with nothing
        // downstream able to change it.
        var composed = Ft8Composer.ComposeSignal(
            wanted, _transmitSampleRate, Ft8Composer.DefaultBaseFrequencyHz, _settings.TransmitDrivePeak);

        if (!composed.Composed)
        {
            DigitalSendLine =
                "Hamlet did not send \"" + wanted + "\": " + composed.Explanation;
            return;
        }

        // **AND COMPOSED IS NOT THE SAME AS READABLE** (work instruction 272).
        // `Ft8Composer` measures, per transmission, whether a callsign travelled
        // as a 22-bit hash rather than as a callsign - and until tonight no line
        // of `src/` read that. The application composed, saw `Composed: true`,
        // armed and keyed, which is exactly what it did twice on 14.074 on
        // 2026-09-07: the level was right, the timing was right, the log said
        // `Sent`, and both slots decoded to nothing at all.
        // **THE RULE IS THE ENGINE'S AND NOT THIS FILE'S** (0.1). Nothing here
        // knows what a hashed callsign is, what the port's brackets mean, or
        // which callsigns are compound; it asks `Ft8ReadBack` and says what it
        // is told. **It is asked before the radio is**, because a message nobody
        // can read is unreadable whether or not anything is connected.
        // **AND IT IS NOT A CHANGE TO THE MENU.** This is the send path, where
        // the licence gate already refuses. `SendMenuFor` is untouched and
        // nothing is removed, greyed, hidden or reordered.
        var readBack = Ft8ReadBack.Check(composed.Transmission!);

        if (!readBack.WouldReachAnybody)
        {
            DigitalSendLine = Ft8ReadBack.SentNothing(wanted, readBack);
            return;
        }

        var trueUtc = Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset) ?? DateTime.UtcNow;
        var next = Ft8Slots.SlotStart(trueUtc).AddSeconds(Ft8Slots.SlotSeconds);

        if (_armedSend is null)
        {
            // REFUSED WITH WORDS, NEVER SILENTLY, AND IT NAMES WHICH HALF IS
            // MISSING. `_transmitRefusal` is written by `BuildTheArmedSend` at
            // the moment the radio connected; empty means nothing has connected
            // at all, and that case keeps the sentence it has always had.
            DigitalSendLine =
                "Hamlet composed \"" + wanted + "\" and sent nothing: "
                + (_transmitRefusal.Length == 0
                    ? "no radio is connected and no transmit audio device is named "
                      + "in Settings."
                    : _transmitRefusal + ".");
            return;
        }

        _armedText = wanted;

        _armedSend.Arm(new OperatorSend(
            composed.Transmission!,
            FrequencyHz,
            LicenseClass,
            _settings.RestrictTransmitToPrivileges,
            next,
            StartSecondsIntoSlot));

        DigitalSendLine = SendingLine(wanted, next);

        // **THE STOP NOW HAS SOMETHING TO STOP, AND SAYS SO.** Appearance only -
        // the button was pressable before this line and is pressable after it.
        RaiseStopControl();
    }

    /// <summary>Where in the slot the signal begins, as unit 255 recorded it.</summary>
    /// <remarks>
    /// **HALF A SECOND AFTER THE BOUNDARY**, which leaves 14.5 s for a 12.64 s
    /// transmission. <see cref="Ft8Slots.TransmissionFits"/> is what says that is
    /// enough, and it is asserted rather than assumed.
    /// </remarks>
    internal const double StartSecondsIntoSlot = 0.5;

    /// <summary>Hands the armed send its boundary, at most once per boundary.</summary>
    /// <remarks>
    /// **THIS CAN FIRE WHAT WAS ARMED AND CANNOT ARM ANYTHING.** It is the
    /// existing slot tick, which is where the clock is already read, and it is
    /// driven before the digital-mode check so an operator who clicked and then
    /// changed tab still gets the transmission he asked for.
    /// </remarks>
    private void DriveTheArmedSend()
    {
        if (_armedSend is null || !_armedSend.IsArmed)
        {
            return;
        }

        var trueUtc = Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset) ?? DateTime.UtcNow;
        var boundary = Ft8Slots.SlotStart(trueUtc);

        if (boundary == _lastBoundaryDriven)
        {
            return;
        }


        _lastBoundaryDriven = boundary;

        _ = AtSlotBoundaryAsync(boundary);
    }

    /// <summary>What one slot boundary did about the armed send.</summary>
    /// <param name="boundaryUtc">The boundary that has arrived, in true UTC.</param>
    /// <returns>What happened, or null where nothing could.</returns>
    /// <remarks>
    /// **THE LEDGER IS TOLD ONLY WHAT ACTUALLY WENT OUT.** `RecordSent` is called
    /// where the run says the whole transmission went and the radio unkeyed, and
    /// nowhere else - a message booked at the moment of arming would put a
    /// transmission in the ledger that a licence refusal, a cancel or a missed
    /// boundary meant never happened.
    /// </remarks>
    internal async Task<Ft8BoundaryResult?> AtSlotBoundaryAsync(DateTime boundaryUtc)
    {
        if (_armedSend is null)
        {
            return null;
        }

        var text = _armedText;

        // **THE TURN LINE STOPS SAYING HIS SLOT IS OPEN WHILE HE IS FILLING IT**
        // (work instruction 279 task 6). Set before the run and cleared after it,
        // so the sentence covers exactly the stretch his carrier is on the air.
        // **Nothing reads these to act.** No arm, no cancel, and the transmission
        // ending sends nothing: the only reader is `RefreshTurn`, which writes a
        // sentence.
        _sendingSlotUtc = boundaryUtc;
        _sendWasStopped = false;

        var result = await _armedSend.AtBoundaryAsync(boundaryUtc).ConfigureAwait(false);

        if (result.Outcome != Ft8ArmOutcome.Ran)
        {
            // Nothing went out, so nothing is in flight to describe.
            _sendingSlotUtc = null;

            if (result.Outcome == Ft8ArmOutcome.TooLate)
            {
                Dispatcher.UIThread.Post(() => DigitalSendLine =
                    "Hamlet did not send \"" + text + "\": its slot went by before "
                    + "the transmission could start, and it was discarded rather "
                    + "than sent in a slot you did not choose.");
            }

            // The boundary refused it, so nothing is waiting any more. Appearance
            // only, and posted because this runs off the UI thread.
            Dispatcher.UIThread.Post(RaiseStopControl);

            return result;
        }

        var run = result.Run!;

        // **STOPPED IS ITS OWN SENTENCE AND IT REPLACES THE COUNTDOWN.** Part of a
        // message went out and the rest did not; a line saying the slot ran its
        // course would hide that at the one moment it matters. It stands until the
        // next boundary, which is where `_sendWasStopped` is cleared.
        _sendWasStopped = run.Outcome == Ft8TransmitOutcome.Cancelled;
        _sendingSlotUtc = _sendWasStopped ? result.Send!.SlotStartUtc : null;

        if (run.Sent)
        {
            // **THE ONE CALL SITE OF `RecordSent` IN THE TREE**, which is the line
            // unit 258 left it unreachable for.
            _contacts?.RecordSent(text, result.Send!.SlotStartUtc);

            // **AND THE SLOT IS REMEMBERED AS ONE HE USED**, so the census does
            // not describe a deliberate suspension as a search that found
            // nothing. Booked here for RecordSent own reason: only where the
            // whole transmission went out and the radio unkeyed.
            _transmittedSlots.Add(result.Send!.SlotStartUtc);
        }

        // **READ AFTER THE BOUNDARY HAS RETURNED, WITH NOTHING KEYED** (work
        // instruction 269, task 3). Sent and Cancelled are the two outcomes where
        // audio actually reached the sink; the refusals key nothing and play
        // nothing, so the sink's figures would be the previous transmission's and
        // showing them would be the exact fault this line exists to prevent.
        var measured = run.Outcome is Ft8TransmitOutcome.Sent or Ft8TransmitOutcome.Cancelled
            ? MeasuredLevelLine(_transmitLevelReport)
            : "Nothing was transmitted, so there is no measured level. After a "
              + "send this line says what the sound card was actually handed.";

        // **THE SECOND READER OF THE ONE LEDGER THE ROWS ALREADY READ** (work
        // instruction 270, task 3), read here rather than in the post for the
        // same reason `RecordSent` is booked here: this is the thread that has
        // just finished with the ledger, and the moment is the slot that went
        // out and not the wall clock a few milliseconds later.
        var stands = ContactStandsLine(text, result.Send!.SlotStartUtc);

        Dispatcher.UIThread.Post(() =>
        {
            DigitalSendLine = WentLine(text, result);
            DigitalTransmitLevelLine = measured;
            DigitalContactStandsLine = stands;

            // **HIS OWN HALF OF THE CONVERSATION REACHES THE PANEL** (Tim's
            // ruling, 2026-09-08), posted rather than booked above because
            // `DigitalMineDecodes` is bound and this method runs off the UI
            // thread. It is inside the `run.Sent` arm's own post, so a refusal,
            // a stop or a missed boundary still puts nothing on screen.
            if (run.Sent)
            {
                KeepSentRow(text, result.Send!.SlotStartUtc);
            }

            // The boundary has been and gone, so there is nothing waiting for one
            // any more and the stop says so. Appearance only.
            RaiseStopControl();
        });

        return result;
    }

    /// <summary>What the Send area says once a boundary has been and gone.</summary>
    /// <param name="text">What was armed.</param>
    /// <param name="result">What the boundary did.</param>
    /// <returns>One line, in the register the rest of this area uses.</returns>
    /// <remarks>
    /// <para>**THE CANCELLED CASE IS UNIT 263'S AND IT HAS TO BE HERE, NOT ONLY IN
    /// <see cref="StopLine"/>.** The stop writes its sentence on the operator's
    /// thread the instant he clicks; the boundary's own line is posted when the
    /// run ends, a few milliseconds later, and **overwrites it**. So the last
    /// sentence he is left looking at is this one, and before this unit it read
    /// *"Hamlet did not send ...: the transmission was stopped after 40890 of
    /// 151680 samples"* - the engine's own words, in samples, at an operator.</para>
    /// <para>**AND IT SAYS WHAT BECAME OF THE CARRIER TOO**, from
    /// <see cref="TransmitRun.CameOutOfTransmit"/> rather than from an assumption,
    /// keeping unit 261's rule: a keyed radio with neither route out taken reads as
    /// something he has to act on and never as safe.</para>
    /// </remarks>
    private static string WentLine(string text, Ft8BoundaryResult result)
    {
        var run = result.Run!;
        var slot = result.Send!.SlotStartUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        if (run.Outcome == Ft8TransmitOutcome.Cancelled)
        {
            return "Stopped: \"" + text + "\" went out for " + HowFarItGot(run)
                + " in the slot at " + slot + " UTC, and the rest of it did not. "
                + (run.CameOutOfTransmit == UnkeyRoute.NothingReachedTheRadio
                    ? "Nothing Hamlet sent to stop the radio got out - if it is still "
                      + "transmitting, stop it at the radio."
                    : "The radio was told to stop transmitting.");
        }

        if (run.Outcome == Ft8TransmitOutcome.RefusedByLicence)
        {
            return "Hamlet did not send \"" + text + "\": " + run.Reason
                + " (" + run.Citation + ")";
        }

        if (!run.Sent)
        {
            return "Hamlet did not send \"" + text + "\": " + run.Reason;
        }

        return "Sent " + Addressed(text) + "\"" + text + "\" in the slot at "
            + slot + " UTC. " + LevelLine(result.Send.Transmission);
    }

    /// <summary>What level the transmission went out at, for the operator to read.</summary>
    /// <param name="transmission">The slot of audio that was played.</param>
    /// <returns>One sentence, naming the level and the clipping.</returns>
    /// <remarks>
    /// <para>**THE CRITERION THIS EXISTS FOR.** `PHASE_PLAN.md` step 2's fourth
    /// criterion asks for *level and clipping stated* and step 3's first asks for
    /// audio at the right *level*. Before unit 265 the operator was shown neither:
    /// <c>WasapiTransmitSink</c> measured both and **every reader of them in the
    /// repository was a test**. A level he is asked to set his radio's ALC by, and
    /// is never told, is a criterion nobody could have closed.</para>
    /// <para>**THIS IS THE LEVEL HAMLET COMPOSED AT, NOT THE LEVEL THAT LEFT THE
    /// MACHINE, AND THE SENTENCE SAYS SO.** It is
    /// <see cref="Ft8Transmission.PeakSample"/>, measured over the array every
    /// time it is asked for, so it is the peak of the very samples the sink was
    /// handed. **What lies between that and the antenna is not knowable from
    /// here** and is named rather than quietly folded in: Windows' own volume for
    /// that endpoint, and then the radio's own USB input gain and its ALC.
    /// `SHACK_FACTS.md` FACT-004 - no radio has ever been attached to the machine
    /// Hamlet was written on, and what the IC-7300's USB modulation input expects
    /// is not in this repository.</para>
    /// <para>**WHY NOT THE SINK'S OWN <c>PeakWritten</c>, WHICH IS THE BETTER
    /// NUMBER.** There is no route to it from what
    /// <see cref="Ft8TransmitSequence"/> returns (unit 265, task 1, question 3),
    /// and building one would mean changing the keying path - the key at
    /// <c>Ft8TransmitSequence.cs:283</c>, the sink call at <c>:287</c>, the
    /// <c>finally</c>, the abort and the stop - which units 255, 261 and 263
    /// proved and which no unit disturbs to carry a number. **The difference
    /// between the two is the clamping**, and the clamping is reported here from
    /// the same array: where nothing is outside the rails the two figures are the
    /// same number.</para>
    /// <para>**A NUMBER, NOT A CALLSIGN.** HM-DEC-018 and
    /// <c>TransmitRecord</c>'s rule govern telemetry and this is a screen line,
    /// but the principle is kept anyway: this sentence adds a level and a count
    /// and no string about who was worked.</para>
    /// </remarks>
    private static string LevelLine(Ft8Transmission transmission)
    {
        var peak = transmission.PeakSample;
        var clipped = 0;

        foreach (var sample in transmission.Samples)
        {
            if (sample is < -1.0f or > 1.0f)
            {
                clipped++;
            }
        }

        var dbfs = peak > 0.0f
            ? (20.0 * Math.Log10(peak)).ToString("0.0", CultureInfo.InvariantCulture)
            : "silent";

        // **THE FACTS, AND THE PARAGRAPH ON THE MARK** (work instruction 281 task
        // 3). This ran to 473 characters on the screen after every send, and task
        // 1's measurement never saw it because it only appears once something has
        // gone out. **What is a fact stays**: the level Hamlet built at, and
        // whether anything clipped getting there. What is a boundary statement or
        // a piece of advice is ComposedLevelTip, one hover away.
        return "composed at " + dbfs + " dBFS \u00b7 "
            + (clipped == 0
                ? "nothing clipped"
                : clipped.ToString(CultureInfo.InvariantCulture) + " samples clipped");
    }

    /// <summary>What the composed level means, and what it cannot tell him.</summary>
    /// <remarks>
    /// <para>**THE CLIP COUNT HERE CANNOT MOVE, AND SAYING SO IS THE POINT** (work
    /// instruction 269, task 4). Measured: at 1.0, the highest drive
    /// <c>Ft8Composer.DriveIsUsable</c> accepts, a whole slot composed at 48000 Hz
    /// is 606720 samples with a largest magnitude of exactly 1.000000 and 0 outside
    /// the rails. It is zero by construction, since the composer multiplies a
    /// unit-amplitude sine by the drive and refuses a drive above full scale, so an
    /// operator who read *nothing clipped* as evidence that his drive is safe would
    /// have been told something by a screen that cannot say it (§0.0). The count
    /// that can move is the sink's, and it is the other line.</para>
    /// <para>**MOVED TO A HOVER ON 2026-09-08 AND NOT DELETED** (work instruction
    /// 281 task 3). It is a boundary statement, and a hover preserves one exactly as
    /// well as a paragraph does.</para>
    /// </remarks>
    public const string ComposedLevelTip =
        "That is the level Hamlet built, before this machine's own volume for that "
        + "device and before the radio's input gain. Set the radio's drive against "
        + "its own ALC meter. The clipped count here is of the audio Hamlet built, "
        + "and the composer will not build above full scale, so on this path it is "
        + "always none. What the sound card actually had to clamp is the measured "
        + "line under the drive control.";

    /// <summary>How much of a stopped transmission went out, in seconds.</summary>
    /// <param name="run">What the sequence did.</param>
    /// <returns>Something like <c>"about 3.4 of its 12.6 seconds"</c>.</returns>
    /// <remarks>
    /// **SECONDS, BECAUSE THAT IS WHAT AN OPERATOR HAS** (§0.7). The number he
    /// wants is how much of his twelve seconds got onto the band before he stopped
    /// it, and "40890 of 151680 samples" is that number written in a unit belonging
    /// to the inside of the program. **"About", because it is honestly about**: the
    /// sink reports what the endpoint consumed, and the tail of it was still in
    /// the air when this sentence was written.
    /// </remarks>
    private static string HowFarItGot(TransmitRun run)
    {
        if (run.Played is not { } played || run.SamplesOffered <= 0)
        {
            return "part of its slot";
        }

        var seconds = played.SamplesPlayed / (double)run.SamplesOffered * run.SecondsOffered;

        return "about " + seconds.ToString("F1", CultureInfo.InvariantCulture) + " of its "
            + run.SecondsOffered.ToString("F1", CultureInfo.InvariantCulture) + " seconds";
    }

    /// <summary>What the Send area says while a transmission is armed.</summary>
    private static string SendingLine(string text, DateTime slotStartUtc)
        => "Sending " + Addressed(text) + "\"" + text + "\" in the slot at "
            + slotStartUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " UTC.";

    /// <summary>"to W1ABC, " where there is an addressee, or "" for a call to anyone.</summary>
    private static string Addressed(string text)
    {
        var fields = Ft8MessageSplit.Split(text);

        return fields is null || Ft8MessageSplit.IsCallToAnyone(fields.To)
            ? ""
            : "to " + fields.To + ", ";
    }

    /// <summary>The licence gate, asked for display and never for permission.</summary>
    /// <remarks>
    /// **THE REFUSAL THAT ACTUALLY STOPS A TRANSMISSION IS NOT HERE.** It is
    /// inside <see cref="Ft8TransmitSequence.RunAsync"/>, before anything that can
    /// key, and it is narrower there than <see cref="TransmitGuard.Check"/> is in
    /// general. This asks the same guard the same question **so the menu can say
    /// what it said**, which is criterion 6's first half - and it is a second
    /// *reader* of the rule, not a second copy of it.
    /// </remarks>
    private readonly TransmitGuard _sendLicence = new();

    /// <summary>What the licence says about transmitting here, or "".</summary>
    /// <remarks>
    /// **THE GUARD'S OWN WORDS**, its <c>Reason</c> and its <c>Citation</c>. It is
    /// empty exactly when the guard had nothing to say - a clean permit - so the
    /// line appears whenever anything at all stands between the operator and the
    /// air, including a guard he has switched off and a class Hamlet does not
    /// know.
    /// </remarks>
    public string DigitalSendLicenceLine
    {
        get
        {
            var decision = _sendLicence.Check(
                LicenseClass, FrequencyHz, TransmitMode.Data,
                _settings.RestrictTransmitToPrivileges);

            if (string.IsNullOrEmpty(decision.Reason))
            {
                return "";
            }

            return string.IsNullOrEmpty(decision.Citation)
                ? decision.Reason
                : decision.Reason + " (" + decision.Citation + ")";
        }
    }

    /// <summary>Whether the licence has anything to say about this frequency.</summary>
    public bool HasDigitalSendLicenceLine => DigitalSendLicenceLine.Length > 0;

    /// <summary>The call to anyone, from the operator's own settings.</summary>
    /// <remarks>
    /// **NO TYPING** (criterion 1). Callsign and grid straight out of
    /// <see cref="OperatorProfile"/>; with no grid set it is `CQ KC3QIS`, which is
    /// a legal FT8 message, and **no locator is invented** (§0.0).
    /// </remarks>
    public string CallToAnyoneText
        => Ft8SendOptions.CallToAnyone(
            _settings.Operator.Callsign?.Trim() ?? "", _settings.Operator.GridSquare);

    /// <summary>What is missing from Settings, said out loud, or "".</summary>
    public string DigitalSendUnset
        => string.IsNullOrWhiteSpace(_settings.Operator.GridSquare)
            ? "Your grid square is not set in Settings, so Hamlet calls CQ as \""
              + CallToAnyoneText + "\" and does not offer the messages that carry "
              + "a grid. It will not invent one."
            : "";

    /// <summary>Whether anything the send path needs is unset.</summary>
    public bool HasDigitalSendUnset => DigitalSendUnset.Length > 0;

    /// <summary>
    /// **The CQ button. Same command, same one route to a keying frame.**
    /// </summary>
    /// <remarks>
    /// It calls <see cref="SendMessage"/> and adds nothing: **one send path, not
    /// two**, so everything asserted about one click applies to this one without
    /// being asserted twice.
    /// </remarks>
    [RelayCommand]
    private void SendCallToAnyone() => SendMessage(CallToAnyoneText);

    /// <summary>
    /// **The stop. One click, and whatever was going to happen does not.**
    /// </summary>
    /// <remarks>
    /// <para>**IT IS ALWAYS THERE AND ALWAYS PRESSABLE** - no
    /// <c>CanExecute</c>, no <c>IsVisible</c> binding, nothing that reads a
    /// state. `PHASE_PLAN.md` step 1 says the abort *cannot be disabled,
    /// deferred, or made conditional*, and a button that greys out when the
    /// application believes nothing is happening is disabled at exactly the
    /// moment the application is wrong. **A control only visible while sending
    /// is useless before the boundary, and one only visible while armed is
    /// useless during the 12.64 seconds**; this is visible at both because it is
    /// visible always.</para>
    /// <para>**IT COMPOSES NOTHING, ARMS NOTHING AND OPENS NOTHING.** One call to
    /// <see cref="Ft8ArmedSend.StopNow"/> with the port that is already open, and
    /// a line of text. It is a route *out* of a transmission and there is no way
    /// through it into one.</para>
    /// <para>**AND IT ASKS NOTHING FIRST.** *Right-click sends immediately with
    /// no confirmation* is ruled; a stop that asks *are you sure* while the radio
    /// is keyed would be worse than the send that asks.</para>
    /// <para>**WHERE THERE IS NO SEND PATH AT ALL IT SAYS SO RATHER THAN
    /// PRETENDING.** <c>_armedSend</c> is null exactly when Hamlet has no way to
    /// key anything - no radio, or no transmit audio device named - because
    /// <see cref="Ft8TransmitSequence"/> is reachable through nothing else. So
    /// there is nothing this application can have started, and the line says
    /// which half is missing instead of claiming a frame went out.</para>
    /// </remarks>
    [RelayCommand]
    private void StopSending()
    {
        var stop = _armedSend?.StopNow(_rigPort);

        DigitalSendLine = StopLine(_armedText, stop, _transmitRefusal);
        RaiseStopControl();
    }

    /// <summary>
    /// **Whether there is anything for the stop to stop right now.**
    /// </summary>
    /// <remarks>
    /// <para>**IT CHANGES HOW THE CONTROL LOOKS AND NEVER WHETHER IT WORKS**
    /// (work instruction 271 task 5; `PHASE_PLAN.md` step 1). The abort cannot be
    /// disabled, deferred or made conditional, so nothing binds this to
    /// <c>IsEnabled</c>, to <c>IsVisible</c> or to a <c>CanExecute</c>: the button
    /// is pressable at every instant whatever this says. **A control only visible
    /// while sending is useless before the boundary**, and one that greys out when
    /// the application believes nothing is happening is dead at exactly the moment
    /// the application is wrong.</para>
    /// <para>**AND IT IS NEVER DRAWN GREY** (§0.5.1, HM-DEC-087). Grey is reserved
    /// for what genuinely cannot be used, and this can always be used. Resting and
    /// live differ by ink weight, by a border and **by the words on the face of
    /// it** - which is the non-colour carrier §0.6 requires.</para>
    /// <para>**IT ASKS THE ARMED SEND AND ASSERTS NOTHING OF ITS OWN.**
    /// <see cref="Ft8ArmedSend.IsArmed"/> is the same field
    /// <see cref="DriveTheArmedSend"/> reads, so the button and the boundary
    /// cannot disagree about whether something is waiting.</para>
    /// </remarks>
    public bool HasSomethingToStop => _armedSend?.IsArmed == true;

    /// <summary>What the stop control says on its face.</summary>
    /// <remarks>
    /// **THE MOST CONSEQUENTIAL CONTROL IN THE APPLICATION WAS AN UNLABELLED
    /// COLOURED BLOCK AND THE OPERATOR HAD TO ASK WHAT IT WAS** (Tim, 2026-09-07).
    /// It carries a word in both states and the word says which state it is in:
    /// there is nothing to stop before he presses CQ, and there is a transmission
    /// to stop after he does.
    /// </remarks>
    public string StopLabel => HasSomethingToStop ? "Stop transmitting" : "Stop";

    /// <summary>What the stop control says on hover, in both states.</summary>
    public string StopTip
        => HasSomethingToStop
            ? "Stops the transmission. It un-arms anything waiting for a slot and "
              + "tells the radio to stop transmitting, and it asks nothing first."
            : "Stops a transmission. Nothing is waiting for a slot at the moment, "
              + "so there is nothing to stop - but it is pressable at every "
              + "instant, because a stop that hides when Hamlet believes nothing "
              + "is happening is hidden exactly when Hamlet is wrong.";

    /// <summary>Bring the stop control level with what is armed.</summary>
    /// <remarks>
    /// Called where the armed send is armed, fired or cleared, and nowhere else.
    /// **It touches no transmit state**: it raises three property notifications.
    /// </remarks>
    private void RaiseStopControl()
    {
        OnPropertyChanged(nameof(HasSomethingToStop));
        OnPropertyChanged(nameof(StopLabel));
        OnPropertyChanged(nameof(StopTip));
    }

    /// <summary>What the Send area says after the stop was pressed.</summary>
    /// <param name="text">What was armed, so the operator is told what did not go.</param>
    /// <param name="stop">What the stop did, or null where there was no send path.</param>
    /// <param name="refusal">Which half is missing, as the connect wrote it.</param>
    /// <returns>One line, in the register the refusals in this area already use.</returns>
    /// <remarks>
    /// <para>**THE ONE THAT MATTERS IS THE THIRD.** An abort whose frames did not
    /// reach the radio is the only state in which the operator has to do something
    /// himself, and it is said plainly rather than folded into the success
    /// sentence - which is the defect unit 253 found in <c>AbortCw</c>, where a
    /// failed abort and a successful one left the same trace.</para>
    /// <para>**AND SINCE UNIT 263 IT SAYS WHAT HAPPENED TO BOTH HALVES.** Until
    /// then this line said *"Stopped"* while Hamlet went on feeding the rest of a
    /// 12.64 second transmission into a radio it had just unkeyed - **a sentence
    /// that says "stopped" when only half of it stopped**, which is §0.0 broken by
    /// a word. The two halves are named separately because they can genuinely
    /// differ: the frames can fail while the sound stops, and that is the state in
    /// which the operator has to walk to the radio.</para>
    /// <para>**"HAMLET STOPPED SENDING" RATHER THAN "THE SOUND STOPPED"** (§0.0).
    /// What the stop can honestly claim is that it told the audio path to stop, on
    /// the calling thread, and the sink notices at the top of its next loop -
    /// measured at 15 ms against the fake at real time. It is not in a position to
    /// assert that the last sample has left the sound card, so it does not say so.
    /// </para>
    /// </remarks>
    private static string StopLine(string text, Ft8StopResult? stop, string refusal)
    {
        if (stop is null)
        {
            return "There was nothing to stop: "
                + (refusal.Length == 0
                    ? "no radio is connected and no transmit audio device is named "
                      + "in Settings."
                    : refusal + ".")
                + " Nothing here has a way to key a radio.";
        }

        // THE TRANSMISSION IN PROGRESS OUTRANKS THE ONE WAITING FOR A SLOT,
        // because it is the one already going out over other people's band.
        var what = stop.AudioToldToStop
            ? "\"" + text + "\" was going out and Hamlet stopped sending it part way through"
            : stop.Unarmed
                ? "\"" + text + "\" was taken off before its slot and will not go out"
                : "nothing was waiting for a slot";

        if (stop.Abort is null)
        {
            return "Stopped: " + what
                + ". There is no radio connected, so there was nothing to tell.";
        }

        if (!stop.AnythingReachedTheRadio)
        {
            // THE HALF THAT WORKED IS STILL SAID, because "Hamlet stopped sending
            // the tones" and "the radio may still be keyed" are both true here and
            // the second is the one he has to act on.
            return (stop.AudioToldToStop
                    ? "Hamlet stopped sending the tones, but "
                    : "Hamlet ")
                + "told the radio to stop and neither frame got out: "
                + (stop.Abort.PttOff.Failure ?? stop.Abort.CwStop.Failure
                    ?? "the port took nothing")
                + ". If it is still transmitting, stop it at the radio.";
        }

        return "Stopped: " + what + ", and the radio was told to stop transmitting.";
    }

    /// <summary>Give the stop a port to write to, for tests.</summary>
    /// <param name="port">The fake wire a test built.</param>
    /// <remarks>
    /// **THE SEAM OPENS NOTHING** (`SHACK_FACTS.md` FACT-004), and it is not a
    /// route to a transmission: <c>_rigPort</c> is read by
    /// <see cref="StopSendingCommand"/> and by nothing that can key.
    /// <see cref="UseArmedSendForTests"/> is its neighbour and has the same
    /// reason.
    /// </remarks>
    internal void UseRigPortForTests(ISerialPort? port) => _rigPort = port;

    /// <summary>Give the send path something to transmit through, for tests.</summary>
    /// <param name="armed">The armed send over whatever fakes a test built.</param>
    /// <remarks>
    /// **THE SEAM EXISTS BECAUSE NO RADIO HAS EVER BEEN ATTACHED TO THIS MACHINE**
    /// (`SHACK_FACTS.md` FACT-004), the shape <see cref="UseRigForTests"/> already
    /// uses. It hands over an already-built <see cref="Ft8ArmedSend"/> and so
    /// opens nothing: **there is still exactly one route to a keying frame** and
    /// this is not a second one.
    /// </remarks>
    internal void UseArmedSendForTests(Ft8ArmedSend? armed) => _armedSend = armed;

    /// <summary>The slot the armed transmission is waiting for, or null.</summary>
    /// <remarks>
    /// **READ-ONLY, AND IT CANNOT ARM OR FIRE ANYTHING.** A test needs to know
    /// which boundary the click chose in order to hand that boundary over, and
    /// composing one from the clock would race the slot the click actually read.
    /// </remarks>
    internal DateTime? ArmedForSlotUtc => _armedSend?.Armed?.SlotStartUtc;

    /// <summary>Whether anything is armed at all.</summary>
    internal bool HasSomethingToTransmitThrough => _armedSend is not null;

    /// <summary>Where a newly arrived row belongs in the display order.</summary>
    /// <param name="row">The row that just arrived.</param>
    /// <returns>The index to insert it at.</returns>
    /// <remarks>
    /// <para>**WITHIN ONE SLOT, ORDER IS NOT THE SORT'S TO INVENT.** Fourteen
    /// messages can share one `HHmmss`, and the air did not put them in any
    /// sequence - they were all on at once and the decoder found them in the
    /// order it happened to search. The direction reverses **slots**; inside a
    /// slot the decoder's order is preserved exactly. Sorting those rows by
    /// frequency, or reversing them along with everything else, would assert a
    /// sequence that did not happen (§0.0).</para>
    /// <para>**SO NEWEST-FIRST DOES NOT MEAN INDEX NOUGHT.** It means after any
    /// rows already at the top that belong to the same slot. Oldest-first is a
    /// plain append, because a new row is always in the newest slot.</para>
    /// </remarks>
    private int InsertAt(DigitalDecodeRow row)
    {
        if (!_digitalNewestFirst)
        {
            return DigitalDecodes.Count;
        }

        var at = 0;

        while (at < DigitalDecodes.Count
            && string.Equals(DigitalDecodes[at].Utc, row.Utc, StringComparison.Ordinal))
        {
            at++;
        }

        return at;
    }

    /// <summary>Rebuild the display from the arrival order.</summary>
    /// <remarks>
    /// **A STABLE GROUPING AND NOT A REVERSE.** Reversing the list would turn
    /// each slot's rows back to front as well, which is the one thing the
    /// direction may not do. This walks the slots in the order they arrived,
    /// takes them newest-slot-first or oldest-slot-first, and inside each slot
    /// keeps the arrival order untouched.
    /// </remarks>
    /// <summary>True while <see cref="Reorder"/> is refilling the decoded table.</summary>
    /// <remarks>
    /// **A REORDER IS ONE EVENT WEARING THE COSTUME OF MANY** (task 3). It clears
    /// the table and adds every row back, so the mirror sees a reset followed by a
    /// hundred arrivals, each landing on the incremental path as though it had just
    /// come off the air. That was harmless while the two sides only filtered, and
    /// it stopped being harmless the moment a row's placement depended on the row
    /// before it: the rows come back in **display** order, which under newest-first
    /// is newest to oldest, so every repeat met its own successor rather than its
    /// predecessor and nothing folded at all. The panel silently un-folded itself
    /// on the first press of the order button, and the count on the folded row went
    /// with it.
    /// </remarks>
    private bool _reordering;

    private void Reorder()
    {
        var slots = new List<string>();
        var bySlot = new Dictionary<string, List<DigitalDecodeRow>>(StringComparer.Ordinal);

        foreach (var row in _digitalArrivals)
        {
            if (!bySlot.TryGetValue(row.Utc, out var rows))
            {
                rows = new List<DigitalDecodeRow>();
                bySlot[row.Utc] = rows;
                slots.Add(row.Utc);
            }

            rows.Add(row);
        }

        if (_digitalNewestFirst)
        {
            slots.Reverse();
        }

        // **THE MIRROR IS HELD OFF AND REBUILT ONCE AT THE END**, rather than
        // being driven a row at a time in display order. `ApplyDecodedFilter`
        // walks the whole table forwards in time, which is the only order a
        // repeat can be folded in.
        _reordering = true;

        try
        {
            DigitalDecodes.Clear();

            foreach (var slot in slots)
            {
                foreach (var row in bySlot[slot])
                {
                    DigitalDecodes.Add(row);
                }
            }
        }
        finally
        {
            _reordering = false;
        }

        ApplyDecodedFilter();
    }

    /// <summary>How many rows the cap has dropped since the panel was cleared.</summary>
    private long _digitalTrimmed;

    /// <summary>What the summary says about trimming, or nothing.</summary>
    /// <remarks>
    /// **A CAP HE CANNOT SEE IS WORSE THAN NO CAP** (§0.0). At the rate measured
    /// on 2026-09-04 the table fills in about nine minutes and then quietly
    /// drops a row for every row that arrives, all evening. A list that discards
    /// rows the operator believes are still there is the same fault as a decode
    /// with nothing behind it.
    /// </remarks>
    private string TrimNote()
        => _digitalTrimmed == 0
            ? ""
            : $" · oldest {_digitalTrimmed} dropped";

    /// <summary>Flip which end the newest slot is shown at.</summary>
    [RelayCommand]
    private void ToggleDigitalOrder() => DigitalNewestFirst = !DigitalNewestFirst;

    /// <summary>Empty the table, and nothing else.</summary>
    /// <remarks>
    /// **THE DISPLAY AND NOT THE RECORD.** Telemetry, the capture sidecars and
    /// the census are what this session is kept in; this clears what is on
    /// screen because the operator wants the screen back. Nothing that has been
    /// written down is touched.
    /// </remarks>
    [RelayCommand]
    private void ClearDigitalDecodes()
    {
        DigitalDecodes.Clear();
        _digitalArrivals.Clear();
        _digitalDecodeKeys.Clear();
        _digitalDecodeKeyOrder.Clear();
        _digitalTrimmed = 0;

        // **THE PANEL SAYS IT IS EMPTY RATHER THAN GOING BLANK** (HM-DEC-021).
        // `HasDigitalDecodes` is false now, so the idle line takes over, and the
        // refusal and the note are cleared with it so nothing left over
        // describes a table that is no longer there.
        _digitalDecodeNote = "";
        _digitalRefusal = "";

        RaiseDigitalDecodeChanges();
    }

    /// <summary>
    /// Throw the table away because the rows no longer describe where the radio
    /// is.
    /// </summary>
    /// <param name="nowHz">Where the dial has moved to.</param>
    /// <remarks>
    /// See <see cref="DigitalRetuneClearsBeyondHz"/> for why the move has to be a
    /// large one and why clearing rather than annotating is the answer.
    /// </remarks>
    private void ClearDigitalDecodesOnRetune(long nowHz)
    {
        if (DigitalDecodes.Count == 0)
        {
            _digitalRowsTunedAtHz = nowHz;
            return;
        }

        if (Math.Abs(nowHz - _digitalRowsTunedAtHz) <= DigitalRetuneClearsBeyondHz)
        {
            return;
        }

        DigitalDecodes.Clear();
        _digitalArrivals.Clear();
        _digitalDecodeKeys.Clear();
        _digitalDecodeKeyOrder.Clear();
        _digitalTrimmed = 0;
        _digitalDecodeNote = "";
        _digitalRefusal = "";

        // The census described a slot from where the radio used to be, and a
        // sentence about one place under a table cleared for another is §0.0.1's
        // two moments under one heading.
        _digitalCensusLine = "";
        _digitalRowsTunedAtHz = nowHz;

        // The audio in the ring is from where the radio used to be.
        _slotWatch.Rearm();

        RaiseDigitalDecodeChanges();
    }

    /// <summary>Everything on the tab that reads the decoded table.</summary>
    private void RaiseDigitalDecodeChanges()
    {
        // **THE COUNTS BEFORE THE SUMMARY THAT QUOTES THEM.** Rows arrive already
        // dimmed, so this is not re-deciding anything; what it does is bring the
        // shown and hidden totals level with the table before the summary is
        // asked for them.
        RecountDecodedFilter();

        OnPropertyChanged(nameof(HasDigitalDecodes));
        OnPropertyChanged(nameof(DigitalDecodedSummary));
        OnPropertyChanged(nameof(DigitalModeStripLine));
        OnPropertyChanged(nameof(DigitalCensusLine));
        OnPropertyChanged(nameof(HasDigitalCensus));
    }

    /// <summary>
    /// The census of a slot that decoded and produced no text, in words.
    /// </summary>
    /// <param name="heard">What came back from the reader.</param>
    /// <returns>One line, or "" when there is nothing of this kind to say.</returns>
    /// <remarks>
    /// <para>**IT NAMES THE STAGE AND IT DOES NOT DIAGNOSE** (§12.1). Candidates at
    /// zero is ahead of the decoder entirely; candidates with no parity is the soft
    /// symbols or the correction; parity with no checksum is a codeword that is not
    /// a message; checksum with no text is the message layer. Saying which of those
    /// the numbers point at is counting. Saying what to change about the radio is
    /// not, and is not done here.</para>
    /// <para>**THE WORST SLOT IN THE READING, NOT THE LAST.** A press hands over two
    /// slots and a watch hands over one; where a press holds one slot that read
    /// nothing and one that read something, the one worth explaining is the one that
    /// read nothing.</para>
    /// <para>**A COSTAS MATCH COUNT IS NOT A SIGNAL-TO-NOISE RATIO** (§0.0), so the
    /// scores are not shown here at all — a bare number on a screen beside the word
    /// *signal* is exactly how one gets read as decibels.</para>
    /// </remarks>
    private string DescribeCensus(Ft8Reception heard)
    {
        if (heard.Refusal.Length > 0 || heard.Slots.Count == 0)
        {
            return "";
        }

        Ft8SlotCensus? worst = null;

        foreach (var slot in heard.Slots)
        {
            if (slot.BecameTextCount > 0)
            {
                continue;
            }

            if (worst is null || slot.CandidateCount > worst.CandidateCount)
            {
                worst = slot;
            }
        }

        if (worst is null)
        {
            return "";
        }

        var at = worst.SlotStartUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        // **A SLOT HE TRANSMITTED IN WAS NOT SEARCHED, AND SAYS SO** (work
        // instruction 280 task 7). Hamlet suspends decoding while the radio is
        // transmitting (HM-DEC-147), so this slot produced nothing **by design**.
        // Reporting a candidate count for it would be a claim about the band
        // drawn from a measurement nobody took (§0.0), and it is what this line
        // said on 2026-09-08 about a slot at 15:16:45 while its neighbours
        // decoded two stations.
        if (_transmittedSlots.Contains(worst.SlotStartUtc))
        {
            return $"{at} UTC was yours - Hamlet was transmitting and did not "
                + "listen";
        }

        // **THE DECODER AND ITS STAGE LIST CAME OFF THE LINE ON 2026-09-08**
        // (Tim: show, do not tell). They belong in the sidecar and on hover,
        // not repeated four times a minute at somebody working a station.
        // **Nothing about what is decoded changed**; this is what the line says.
        if (worst.CandidateCount == 0)
        {
            // **A QUIET SLOT IS A NUMBER.** Zero candidates reads as zero
            // candidates.
            return $"{at} UTC · 0 candidates";
        }

        // **THE STAGES ARE COUNTS AND NOT CLAUSES.** Each number is where the
        // reading stopped: candidates found, of those how many were valid
        // codewords, of those how many carried their own checksum. Nothing here
        // diagnoses (§12.1) and **no number is lost** - the same figures are on
        // the line without the sentence around them.
        if (worst.ParitySatisfiedCount == 0)
        {
            return $"{at} UTC · {worst.CandidateCount} candidates · 0 codewords";
        }

        if (worst.ChecksumPassedCount == 0)
        {
            return $"{at} UTC · {worst.CandidateCount} candidates · "
                + $"{worst.ParitySatisfiedCount} codewords · 0 checksums";
        }

        return $"{at} UTC · {worst.CandidateCount} candidates · "
            + $"{worst.ChecksumPassedCount} checksums · 0 read";
    }

    /// <summary>What a press made of the audio, in one line.</summary>
    /// <param name="heard">What came back.</param>
    /// <returns>A sentence with no full stop, for the caller to place.</returns>
    /// <remarks>
    /// **FOUND-BUT-UNREAD IS A DIFFERENT ANSWER FROM NOTHING THERE** (§0.0.1), and
    /// the operator can act on the difference: signals found and none read is a
    /// gain, a filter or a clock question, where nothing found at all is a band or
    /// an antenna question.
    /// </remarks>
    private static string DescribeDecodes(Ft8Reception heard)
    {
        if (heard.Refusal.Length > 0)
        {
            return heard.Refusal;
        }

        var slots = heard.SlotsDecoded == 1 ? "one slot" : $"{heard.SlotsDecoded} slots";

        if (heard.Decodes.Count > 0)
        {
            var messages = heard.Decodes.Count == 1
                ? "one message"
                : $"{heard.Decodes.Count} messages";

            return $"{messages} out of {slots}";
        }

        return heard.CandidatesFound > 0
            ? $"{slots} decoded, and of {heard.CandidatesFound} places that "
                + "looked like a signal, not one came out as a message"
            : $"{slots} decoded, and nothing on the band looked like FT8 at all";
    }

    /// <summary>Where captures and the roster are written.</summary>
    /// <remarks>
    /// Settable so a test can point the whole path at a temporary folder, in the
    /// manner `LayoutStore.Path` already is. **The operator's own captures are not
    /// a test's to write into**, and the adjudicated fixture folder is not either.
    /// </remarks>
    internal static string CaptureFolder { get; set; }
        = Path.Combine(SettingsStore.DataFolder, "captures");

    /// <summary>
    /// Record that the operator heard a station here (the roster).
    /// </summary>
    /// <param name="wav">The recording written, or "" when none was.</param>
    /// <param name="refusal">Why none was, or "" when one was.</param>
    /// <param name="inRecording">
    /// What the decoder did over the audio that was kept, or null when there is
    /// no recording or its figures could not be derived.
    /// </param>
    /// <remarks>
    /// <para>**THE PRESS ASSERTS SOMETHING THE APPLICATION CANNOT KNOW**: that
    /// there was a station there to hear. Every other number Hamlet holds is
    /// downstream of its own decoder, so a station it misses is a case it never
    /// counts, and a score built from them would come out at a hundred per cent
    /// while the operator sat listening to somebody it could not read.</para>
    /// <para>Called on every exit from the capture command, including both
    /// refusals. **A case with no evidence is still a case** and belongs in the
    /// denominator.</para>
    /// </remarks>
    private void MarkCase(
        string wav, string refusal, CwCounterDelta? inRecording = null)
    {
        var report = _decoder?.Report;

        // **THE ROW IS SCORED, SO ITS COUNT HAS TO BE ABOUT THE CASE**
        // (HM-DEC-091). Where the recording's own figures could be derived they
        // go in; where there is no recording, or the decoder's history does not
        // reach back over it, the session totals go in **and the cell says so**
        // rather than passing for an answer about this station.
        var emitted = inRecording?.CharactersEmitted ?? report?.CharactersEmitted ?? 0;
        var unsure = inRecording?.CharactersUnsure ?? report?.CharactersUnsure ?? 0;

        var covers = inRecording is not null
            ? CwCountsCover.Recording
            : wav.Length == 0
                ? CwCountsCover.NoRecording
                : CwCountsCover.Session;

        CwCaseRoster.Append(
            CaptureFolder,
            new CwCase(
                DateTime.UtcNow,
                CapturedHz,
                CapturedBandName(),
                wav,
                refusal,
                report is { HasTone: true } tone ? tone.ToneHz : null,
                report is { } r && !double.IsNaN(r.SnrDb) ? r.SnrDb : null,
                // **ONE SOURCE, TAKEN HERE** (HM-DEC-091). This was the polled
                // snapshot the header happens to be holding, which is a different
                // instant from every other figure on the row.
                _decoder?.WordsPerMinute,
                emitted,
                unsure,

                // **THE TAIL AT THE MOMENT OF THE PRESS**, which is what he was
                // looking at when he decided there was a station there. A hundred
                // and twenty characters carries several overs at any speed and
                // still leaves the row one line in a text editor.
                Transcript.Tail(RosterTextLength),
                covers,
                KeyingLine(_keyingReading),

                // **THE SEED COLUMN IS ALWAYS EMPTY NOW.** The control that
                // filled it was inert and came out; the column stays so a roster
                // started before this build and one started after it are the same
                // shape, and a later ruling can retire it.
                null,

                // And what the fit behind the speed looked like, so a row with no
                // speed on it can be told from a row whose speed came out of a
                // fit that was not a fist.
                FitLine()));
    }

    /// <summary>
    /// The band the capture was made on, from the frequency that was read.
    /// </summary>
    /// <remarks>
    /// The same reading the sidecar's own band line uses, so a roster row and the
    /// sidecar beside it cannot disagree about where the radio was (HM-DEC-091).
    /// </remarks>
    private string CapturedBandName()
        => HfBands.BandFor(CapturedHz)?.Name ?? "outside every band Hamlet knows";

    /// <summary>Write the recent list back to settings.json.</summary>
    private void PersistRecent()
    {
        _settings.Recent = Recent
            .Select(r => new SavedRecentStation
            {
                FrequencyHz = r.FrequencyHz,
                Station = r.Station,
                Mode = r.Mode,
                BandName = r.BandName,
                Neighborhood = r.Neighborhood,
                VisitedUtc = r.VisitedUtc,
                StationSource = r.Source.ToString(),
                Visits = r.Visits,
            })
            .ToList();

        SettingsStore.Save(_settings);
        RebuildMenus();

        // Every route that changes the list comes through here, which is the
        // only reason the offer to forget where you are cannot go stale
        // (HM-DEC-134).
        OnPropertyChanged(nameof(IsSomewhereRemembered));
    }

    /// <summary>Take one place out of the recent list (HM-DEC-134).</summary>
    /// <param name="entry">The entry, or null.</param>
    /// <remarks>
    /// **THE LIST IS A RECORD OF WHERE HE WAS, AND HE IS ALLOWED TO SAY SOME OF
    /// IT WAS NOT WORTH KEEPING.** Nothing here asks him to confirm: a removed
    /// entry costs a visit to get back and the dwell that produced it was
    /// twenty seconds, so a dialog would be guarding something cheaper than the
    /// dialog.
    /// </remarks>
    [RelayCommand]
    private void ForgetRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        var kept = RecentStations.Remove(Recent, entry.FrequencyHz);

        if (kept.Count == Recent.Count)
        {
            return;
        }

        AppEvents.RecentRemoved(_telemetry, all: false, removed: 1);

        Recent.Clear();
        foreach (var row in kept)
        {
            Recent.Add(row);
        }

        PersistRecent();
    }

    /// <summary>True when where the dial is now is in the recent list.</summary>
    /// <remarks>
    /// **THE BUTTON IS ABSENT RATHER THAN GREY WHERE THERE IS NOTHING TO
    /// FORGET** (§0.5.1). Grey is reserved for what genuinely cannot be used,
    /// and a control offering to remove somewhere the operator has never been
    /// is not disabled, it is meaningless.
    /// </remarks>
    public bool IsSomewhereRemembered
        => Recent.Any(e => RecentStations.IsSamePlace(e.FrequencyHz, FrequencyHz));

    /// <summary>Forget the place the dial is on now (HM-DEC-134).</summary>
    [RelayCommand]
    private void ForgetHere()
        => ForgetRecent(
            Recent.FirstOrDefault(
                e => RecentStations.IsSamePlace(e.FrequencyHz, FrequencyHz)));

    /// <summary>Empty the recent list (HM-DEC-134).</summary>
    [RelayCommand]
    private void ForgetAllRecent()
    {
        if (Recent.Count == 0)
        {
            return;
        }

        AppEvents.RecentRemoved(_telemetry, all: true, removed: Recent.Count);

        Recent.Clear();
        PersistRecent();
    }

    /// <summary>Tune back to somewhere the operator has been.</summary>
    [RelayCommand]
    private void TuneToRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        AppEvents.TuneRequested(_telemetry, entry.FrequencyHz, "recent");
        TuneTo(entry.FrequencyHz);
    }

    /// <summary>
    /// Star a place he has been into a favorite (HM-DEC-072).
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <remarks>
    /// HOW MOST FAVORITES WILL ACTUALLY BE BORN. Somebody was somewhere good,
    /// did not think to save it, and wants it the following evening. What it
    /// captures is what a direct save captures, from the same function, so a
    /// favorite made this way is indistinguishable from one made at the star.
    /// </remarks>
    [RelayCommand]
    private void StarRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        if (RadioEngine.Explore.Favorites.At(Favorites, entry.FrequencyHz) is not null)
        {
            StatusText = $"{entry.Label} is already saved.";
            return;
        }

        if (Favorites.Count >= RadioEngine.Explore.Favorites.Maximum)
        {
            StatusText = "That is as many favorites as Hamlet keeps. Remove one "
                       + "from Radio, Manage favorites, and this will save.";
            return;
        }

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(entry.FrequencyHz));
        var favorite = RecentStations.ToFavorite(entry, here, DateTime.UtcNow);

        Favorites.Add(favorite);
        AppEvents.FavoriteSaved(_telemetry, favorite.BandName);
        Narrate($"Saved as \"{favorite.Name}\".");

        PersistFavorites();
    }

    partial void OnSelectedRecentChanged(RecentStation? value)
    {
        if (value is null)
        {
            return;
        }

        var picked = value;
        SelectedRecent = null;
        TuneToRecent(picked);
    }

    /// <summary>
    /// Reload the feed, preserving the reading position: spots that are still
    /// there keep their place in the list, departures drop out, and arrivals
    /// append with a "new" tag. Sorting the list afresh on every tick would
    /// move a card out from under the operator's cursor mid-read
    /// (HM-DEC-020).
    /// </summary>
    /// <summary>The lifetimes the operator has configured.</summary>
    private SpotLifetimeSettings Lifetimes => _settings.Lifetimes;

    /// <summary>
    /// Rank the bands from what Hamlet has actually heard (HM-DEC-046).
    /// </summary>
    /// <param name="nowUtc">The moment to judge against.</param>
    /// <returns>The ranking, best first.</returns>
    /// <remarks>
    /// The local hour is passed in rather than read inside, so the ranking
    /// stays a pure function and the tiebreaker is testable without waiting
    /// for a particular time of day (§5).
    /// </remarks>
    private BandRanking RankBands(DateTime nowUtc)
        => BandOpportunities.Rank(
            Bands.Select(b => b.Band).ToList(),
            _allBandSpots,
            nowUtc,
            DateTime.Now.Hour,
            Lifetimes);

    /// <summary>
    /// Move the best-bet badge to whichever band the ranking puts first.
    /// </summary>
    /// <param name="ranking">The shared ranking.</param>
    /// <remarks>
    /// The label travels with it, so a clock guess never wears the same words
    /// as an observation (§0.0). Nothing is marked at all when no band has a
    /// name in the ranking, which cannot happen today but would otherwise be
    /// a silent badge on the first band in the list.
    /// </remarks>
    private void ApplyBestBet(BandRanking ranking)
    {
        foreach (var button in Bands)
        {
            // The decision lives in the ranking, not here. This loop copies an
            // answer and is not allowed to form one of its own.
            var isBest = ranking.BadgeGoesOn(button.Band.Name);

            button.IsBestBet = isBest;

            if (isBest)
            {
                button.BestBetLabel = ranking.BadgeLabel;
                button.BestBetTooltip = ranking.BadgeTooltip;
            }
        }
    }

    /// <summary>
    /// Write what a refresh returned to history, and keep the file bounded.
    /// </summary>
    /// <remarks>
    /// Runs off the UI thread. The store never throws for storage reasons, so
    /// this needs no guard of its own; the worst case is that history stops
    /// growing and the app carries on with what it has (§8).
    /// </remarks>
    private void RecordAndPrune(IReadOnlyList<ActivitySpot> spots, DateTime now)
    {
        _spotStore.Record(spots, now);

        // Pruning on a schedule rather than every refresh, because deleting
        // nothing a hundred times an hour is just disk noise.
        if (now - _lastPruneUtc < PruneInterval)
        {
            return;
        }

        _lastPruneUtc = now;
        var gone = _spotStore.Prune(now - HistoryRetention);

        if (gone > 0)
        {
            AppEvents.SpotHistoryPruned(_telemetry, gone, _spotStore.Count());
        }
    }

    /// <summary>
    /// Everything still worth showing: what the feed just returned, plus
    /// anything in history still inside its own source's lifetime.
    /// </summary>
    /// <remarks>
    /// <para>THE FIX FOR THE ACTUAL COMPLAINT (HM-DEC-045). The feed only
    /// returns what its sources hold right now, and RBN in particular holds
    /// nothing at all on a fresh start. History fills that in.</para>
    /// <para>Live spots win over stored copies of themselves, because the live
    /// one may carry a newer report count or a better story. Identity is the
    /// same rule the store and the aggregate use.</para>
    /// </remarks>
    private IReadOnlyList<StoredSpot> LiveFromHistory(
        IReadOnlyList<ActivitySpot> live, DateTime now)
    {
        var merged = new List<StoredSpot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // The store first, because its rows carry the two facts the lenses need
        // and a live spot cannot: when Hamlet first saw this, and whether the
        // operator has already been to it (HM-DEC-057).
        foreach (var stored in _spotStore.Since(now - Lifetimes.Longest))
        {
            if (!SpotLifetime.IsLive(stored.Spot, now, Lifetimes))
            {
                continue;
            }

            if (seen.Add(SpotIdentity.KeyFor(stored.Spot)))
            {
                merged.Add(stored);
            }
        }

        // Anything the feed just returned that the store did not give back. It
        // was recorded a moment ago, so this is a store that could not be
        // written rather than a spot that is new, and the list still shows it
        // rather than losing it to a disk problem (§8).
        foreach (var spot in live)
        {
            if (SpotLifetime.IsLive(spot, now, Lifetimes)
                && seen.Add(SpotIdentity.KeyFor(spot)))
            {
                merged.Add(new StoredSpot(spot, now, now));
            }
        }

        merged.Sort((a, b) => b.Spot.HeardAtUtc.CompareTo(a.Spot.HeardAtUtc));
        return merged;
    }

    private async Task ReloadSpotsAsync(string trigger)
    {
        // **THE ON-DEMAND READ THAT USED TO BE HERE IS GONE** (HM-DEC-109).
        // It was closing a consequence rather than the cause: the band these
        // sources are scoped to derives from the frequency, and the frequency
        // could be wrong because a broadcast was missed at startup. The sweep
        // fixes that at the source, every thirty seconds, for everything that
        // reads the frequency rather than only for the two callers somebody
        // remembered. A spot refresh runs every one to fifteen minutes, so the
        // sweep is always the fresher of the two anyway.
        _activitySource.SetContext(BuildContext());

        IReadOnlyList<ActivitySpot> spots;
        try
        {
            spots = await _activitySource.GetSpotsAsync();
        }
        catch (Exception)
        {
            // An unreachable feed leaves the last set on screen, aging
            // visibly. Silence beats an invented refresh.
            return;
        }

        var now = DateTime.UtcNow;

        // Everything seen goes to history before anything is drawn, off the UI
        // thread, so a slow disk cannot stutter the window (HM-DEC-045).
        await Task.Run(() => RecordAndPrune(spots, now));

        // The display is a VIEW OVER HISTORY rather than a buffer that
        // forgets. What the feed just returned is merged with everything still
        // inside its source's lifetime, so a park activator spotted twenty
        // minutes ago is still an invitation instead of being discarded.
        var history = LiveFromHistory(spots, now);

        _allBandSpots.Clear();
        _allBandSpots.AddRange(history.Select(h => h.Spot));

        // The list shows the band on screen; the conditions line keeps the
        // whole spectrum, which is what lets it say "try 40 m" with a count.
        _bandHistory = history
            .Where(h => SelectedBand.Band.LowHz <= h.Spot.FrequencyHz
                        && h.Spot.FrequencyHz <= SelectedBand.Band.HighHz)
            .ToList();

        var onBand = _bandHistory.Select(h => h.Spot).ToList();
        var ranked = ApplyLens(now);
        var newCount = _lastNewSpotCount;

        UpdateBandActivity(now);
        ActivityDots = BuildDots(ranked, now);

        // ONE RANKING, READ BY BOTH (HM-DEC-046). The badge and the lead card
        // used to answer "which band is best" separately, and the badge
        // answered it from a clock table, so they could and did contradict
        // each other on the same screen.
        var ranking = RankBands(now);
        ApplyBestBet(ranking);

        Lead = LeadCard.Choose(
            ranked,
            SelectedBand.Band.Name,
            AnySourceAnswering(),
            ranking,
            _settings.Lifetimes.Longest);

        Conditions = BandConditions.Describe(
            SelectedBand.Band.Name, onBand, _allBandSpots, _activitySource.Statuses, now);

        _lastSpotLoadUtc = now;
        _spotsEverLoaded = true;
        UpdateSourceSummary();
        UpdateSpotFreshness();

        AppEvents.SpotsRefreshed(_telemetry, trigger, Spots.Count, newCount);
        AppEvents.LeadCardBuilt(
            _telemetry, Lead.HasSuggestion,
            ranked.Count > 0 && Lead.HasSuggestion ? ranked[0].Score : 0);
    }

    /// <summary>
    /// Put the current lens over the band's history and redraw the list.
    /// </summary>
    /// <param name="now">Reference time.</param>
    /// <returns>The ranked spots the lens shows.</returns>
    /// <remarks>
    /// <para>NOTHING IS DELETED HERE OR ANYWHERE BELOW IT (HM-DEC-057). This
    /// filters the history the store handed back and the store keeps every row
    /// either way, so switching lenses changes what is on screen and changes
    /// nothing about what Hamlet holds.</para>
    /// <para>Inference chooses the opening lens once, and only while the
    /// operator has not chosen one themselves. After that it is theirs.</para>
    /// </remarks>
    private IReadOnlyList<RankedSpot> ApplyLens(DateTime now)
    {
        var attention = new SpotAttention(_settings.SpotsLastLookedUtc, _actedOnSpots);

        if (!_lensChosenByOperator)
        {
            var unseen = _bandHistory.Count(h => SpotLensView.IsUnseen(h, attention));
            SetLens(SpotLensView.OpeningLens(attention.LastLookedUtc, now, unseen));
        }

        var lensed = SpotLensView.Apply(_lens, _bandHistory, attention, now, Lifetimes);

        // The chips count everything the lens has and draw only the families
        // that are on, which composes with the lens rather than fighting it
        // (HM-DEC-061).
        RebuildFamilyChips(lensed.Select(l => l.Spot));

        var shown = FamilyFilter.Apply(lensed.Select(l => l.Spot), _families).ToHashSet();
        var kept = lensed.Where(l => shown.Contains(l.Spot)).ToList();

        var prominence = kept.ToDictionary(
            l => SpotViewModel.KeyFor(l.Spot), l => l.Prominence, StringComparer.Ordinal);

        // The rank reads the lens's own liveness rather than measuring the
        // clock again, so the fade on a card and its place in the list are two
        // readings of one number (HM-DEC-058).
        var ranked = SpotRanking.Rank(kept, Lifetimes, _settings.CopySpeedWpm);

        _lastNewSpotCount = RebuildSpotList(ranked, now, prominence);
        UpdateSpotFreshness();

        return ranked;
    }

    /// <summary>
    /// Move the lens and tell the two toggles which of them is down.
    /// </summary>
    /// <param name="lens">The lens now in use.</param>
    /// <remarks>
    /// EVERY PATH THAT MOVES THE LENS COMES THROUGH HERE, which it did not at
    /// first: the operator's own click set the field and left the buttons
    /// bound to stale booleans, so the control showed the wrong one down until
    /// something else happened to refresh it. The toggles bind one way on
    /// purpose, since which lens is in use is the ViewModel's answer rather than
    /// the button's.
    /// </remarks>
    private void SetLens(SpotLens lens)
    {
        _lens = lens;
        IsBestChance = lens == SpotLens.BestChance;
        IsWhatsNew = lens == SpotLens.WhatsNew;
        LensQuestion = SpotLensView.Question(lens);
    }

    /// <summary>
    /// Mark every spot at this frequency as one the operator has been to.
    /// </summary>
    /// <param name="hz">Where they tuned.</param>
    /// <returns>
    /// The callsign of a station reported there, or "" when none of the spots
    /// named one (HM-DEC-072).
    /// </returns>
    /// <remarks>
    /// By frequency bucket rather than by card, because the click that tunes
    /// carries a frequency and two skimmers measuring the same carrier rarely
    /// agree to the hertz. It is the same bucket the store identifies a spot by
    /// (<see cref="SpotIdentity.FrequencyBucketHz"/>), so the two cannot drift.
    /// </remarks>
    private string MarkActedOn(long hz)
    {
        var bucket = hz / SpotIdentity.FrequencyBucketHz;
        var now = DateTime.UtcNow;
        var arrivedOn = "";

        foreach (var stored in _bandHistory)
        {
            if (stored.Spot.FrequencyHz / SpotIdentity.FrequencyBucketHz != bucket)
            {
                continue;
            }

            var key = SpotIdentity.KeyFor(stored.Spot);
            if (_actedOnSpots.Add(key))
            {
                _spotStore.MarkActedOn(key, now);
            }

            // WHO HE WENT TO SEE (HM-DEC-072). Only reached from a click on a
            // card, a dot or a story, so this is the operator acting on a report
            // of that station rather than the dial happening to be near one.
            if (arrivedOn.Length == 0 && !string.IsNullOrWhiteSpace(stored.Spot.DxCall))
            {
                arrivedOn = stored.Spot.DxCall.Trim();
            }
        }

        return arrivedOn;
    }

    /// <summary>
    /// Rebuild the card list in ranked order, reusing the cards already on
    /// screen so a surviving spot keeps its identity.
    /// </summary>
    /// <param name="ranked">Ranked spots, best first.</param>
    /// <param name="now">Reference time.</param>
    /// <param name="prominence">How strongly to draw each card, by key.</param>
    /// <returns>How many spots were not in the previous set.</returns>
    /// <remarks>
    /// HM-DEC-020 said the list is not re-sorted on every tick, because moving
    /// a card out from under a reading operator's cursor costs more than a
    /// perfect order. That still holds and is why the one-second age tick only
    /// re-ages text. Ranking reorders on a data refresh only — a deliberate,
    /// five-minutes-apart event where the content genuinely changed
    /// (HM-DEC-025 amends HM-DEC-020 to exactly this extent).
    /// </remarks>
    private int RebuildSpotList(
        IReadOnlyList<RankedSpot> ranked,
        DateTime now,
        IReadOnlyDictionary<string, double> prominence)
    {
        var existing = new Dictionary<string, SpotViewModel>(StringComparer.Ordinal);
        foreach (var vm in Spots)
        {
            existing[vm.Key] = vm;
        }

        var rebuilt = new List<SpotViewModel>(ranked.Count);
        var newCount = 0;

        foreach (var entry in ranked)
        {
            var key = SpotViewModel.KeyFor(entry.Spot);

            var distance = DescribeDistance(entry.Spot);

            // How strongly to draw it: the fade that lets the eye find what is
            // current without reading a timestamp (HM-DEC-057). A card the lens
            // did not measure is drawn plainly rather than dimmed on a guess.
            var drawAt = prominence.TryGetValue(key, out var p) ? p : 1.0;

            if (existing.TryGetValue(key, out var vm))
            {
                vm.Update(entry.Spot, now, entry.Reason, distance, Lifetimes, drawAt);
                rebuilt.Add(vm);
                continue;
            }

            // Nothing is "new" on the first load — everything would be.
            rebuilt.Add(new SpotViewModel(
                entry.Spot, now, isNew: _spotsEverLoaded, entry.Reason, distance,
                Lifetimes, drawAt));

            if (_spotsEverLoaded)
            {
                newCount++;
            }
        }

        if (!SameOrder(Spots, rebuilt))
        {
            Spots.Clear();
            foreach (var vm in rebuilt)
            {
                Spots.Add(vm);
            }
        }

        return newCount;
    }

    private static bool SameOrder(
        IReadOnlyList<SpotViewModel> current, IReadOnlyList<SpotViewModel> rebuilt)
    {
        if (current.Count != rebuilt.Count)
        {
            return false;
        }

        for (var i = 0; i < current.Count; i++)
        {
            if (!ReferenceEquals(current[i], rebuilt[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Turn ranked spots into map dots, with prominence following the rank so
    /// the map and the list agree about what matters (HM-DEC-023).
    /// </summary>
    private IReadOnlyList<Controls.ActivityDot> BuildDots(
        IReadOnlyList<RankedSpot> ranked, DateTime now)
    {
        if (ranked.Count == 0)
        {
            return Array.Empty<Controls.ActivityDot>();
        }

        var dots = new List<Controls.ActivityDot>(ranked.Count);

        for (var i = 0; i < ranked.Count; i++)
        {
            var entry = ranked[i];

            // Prominence falls away over the first ten places; past that every
            // dot is drawn at the floor rather than vanishing, because it is
            // still a real signal on a real frequency.
            var prominence = Math.Max(0.0, 1.0 - (i / 10.0));

            dots.Add(new Controls.ActivityDot(
                entry.Spot.FrequencyHz,
                entry.Spot.Story,
                entry.Spot.Mode,
                entry.Spot.Source,
                SpotFreshness.Describe(now - entry.Spot.HeardAtUtc),
                entry.Reason,
                prominence)
            {
                Distance = DescribeDistance(entry.Spot),
            });
        }

        return dots;
    }

    /// <summary>
    /// How far away a spot is, or "" when the app cannot justify a figure
    /// (HM-DEC-038).
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <returns>e.g. "480 miles northeast", or "".</returns>
    private string DescribeDistance(ActivitySpot spot)
        => SpotDistance.Describe(
            _settings.Operator.Position, spot, _settings.DistanceUnits);

    /// <summary>
    /// Recompute every visible distance.
    /// </summary>
    /// <remarks>
    /// Called when the grid arrives, when it changes, and when the units
    /// change — the moment a position becomes known, every card and dot on
    /// screen can say something it could not say a second ago.
    /// </remarks>
    private void UpdateSpotDistances()
    {
        foreach (var spot in Spots)
        {
            spot.Distance = DescribeDistance(spot.Spot);
        }

        // The dots are a record type rebuilt wholesale, so the cheapest
        // correct thing is to rebuild them from the spots that are already on
        // the list rather than hold a second copy of the ranking.
        if (ActivityDots.Count > 0)
        {
            ActivityDots = ActivityDots
                .Select(d => d with
                {
                    Distance = DistanceForFrequency(d.FrequencyHz),
                })
                .ToList();
        }
    }

    /// <summary>
    /// The distance for whatever spot sits on this frequency, or "".
    /// </summary>
    private string DistanceForFrequency(long frequencyHz)
    {
        var match = Spots.FirstOrDefault(s => s.FrequencyHz == frequencyHz);
        return match is null ? "" : DescribeDistance(match.Spot);
    }

    /// <summary>
    /// Refresh every band button's activity indicator (HM-DEC-031).
    /// </summary>
    /// <remarks>
    /// Computed from the whole-spectrum spot set and the source statuses, so
    /// a band button and the conditions line under the map are always
    /// counting the same minutes from the same evidence.
    /// </remarks>
    private void UpdateBandActivity(DateTime now)
    {
        var readings = BandActivity.Summarize(
            Bands.Select(b => b.Band).ToList(),
            _allBandSpots,
            _activitySource.Statuses,
            now);

        foreach (var reading in readings)
        {
            var button = Bands.FirstOrDefault(b => b.Band.Name == reading.BandName);
            if (button is not null)
            {
                button.Activity = reading;
            }
        }

        UpdateBandCharacter(now);
    }

    /// <summary>
    /// Refresh each card's look and its character text for the current hour
    /// (HM-DEC-033).
    /// </summary>
    /// <remarks>
    /// Sunrise and sunset are computed from the operator's grid square. With
    /// no grid there are no coordinates, so nothing is dimmed, the icons stay
    /// neutral and the text says how to fix that — Hamlet never guesses where
    /// somebody is.
    /// </remarks>
    private void UpdateBandCharacter(DateTime nowUtc)
    {
        var here = OperatorLocation.FromGrid(_settings.Operator.GridSquare);

        var sun = here is null
            ? SolarSnapshot.Unknown
            : SolarClock.At(here.Value.Latitude, here.Value.Longitude, nowUtc);

        var names = Bands.Select(b => b.Band.Name).ToList();

        foreach (var button in Bands)
        {
            button.Card = BandCardStyles.For(button.Band.Name, names, sun);
            button.Character = BandCharacter.Describe(
                button.Band.Name, sun, nowUtc.Month, here?.Latitude);
        }
    }

    private bool AnySourceAnswering()
        => _activitySource.Statuses.Any(s => s.State == SourceState.Ok);

    /// <summary>
    /// Name which sources answered, and confess the ones that did not.
    /// </summary>
    private void UpdateSourceSummary()
    {
        var statuses = _activitySource.Statuses;
        var ok = statuses.Where(s => s.State == SourceState.Ok).Select(s => s.Name).ToList();
        var down = statuses.Where(s => s.IsLetDown).ToList();

        foreach (var status in down)
        {
            AppEvents.SourceUnhealthy(_telemetry, status.Name, status.State.ToString());
        }

        SourcesSummary = ok.Count == 0
            ? "no sources answering"
            : string.Join(", ", ok)
              + (down.Count > 0 ? $" · {string.Join(", ", down.Select(d => d.Name))} down" : "");
    }

    /// <summary>Re-age everything on screen: the header line, its color, each
    /// card's own age, and the expiry of the "new" tags.</summary>
    private void UpdateSpotFreshness()
    {
        var now = DateTime.UtcNow;
        var since = now - _lastSpotLoadUtc;
        var interval = _settings.SpotRefreshMinutes;

        // THE LENS IS NAMED FIRST (§0.5, HM-DEC-057). A shut panel that has
        // silently changed which question it is answering is the prime
        // directive broken by omission: the operator reads a count and takes it
        // for a count of everything.
        // A shut panel never hides that it is filtering (§0.5, HM-DEC-061).
        var filtered = FamilyFilter.Summary(_families);

        SpotsSummary = SpotLensView.Summary(_lens, Spots.Count)
            + (filtered.Length > 0 ? $" · {filtered}" : "")
            + " · " + SpotFreshness.Tail(since, interval, _spotsEverLoaded)
            + (SourcesSummary.Length > 0 ? $" · {SourcesSummary}" : "");
        SpotsFreshness = _spotsEverLoaded
            ? SpotFreshness.Evaluate(since, interval)
            : FreshnessLevel.Fresh;

        foreach (var spot in Spots)
        {
            spot.Reage(now);
        }
    }

    private void OnRigFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
        => Dispatcher.UIThread.Post(() => ApplyRigFrequency(e.FrequencyHz));

    /// <summary>Rig-origin frequency (the physical knob): follow it, switch
    /// the selected band if the operator crossed one, and never echo it back
    /// out to the rig.</summary>
    /// <remarks>
    /// THE RADIO IS NEVER ARGUED WITH (§0.0, HM-DEC-055). A 7300 tunes right
    /// across the shortwave broadcast bands and somebody will do it, so when the
    /// reading is off every ham band the nearest one is put on screen and the
    /// display follows the radio out into it. Clamping here would show a
    /// frequency the radio is not on, which is a confident wrong answer about
    /// the one number every other surface trusts.
    /// </remarks>
    private void ApplyRigFrequency(long hz)
    {
        _updatingFromRig = true;
        try
        {
            var band = HfBands.BandFor(hz) ?? AmateurSpectrum.Nearest(hz);
            if (band is not null && band.Name != SelectedBand.Band.Name)
            {
                SelectedBand = Bands.First(b => b.Band.Name == band.Name);
            }

            FrequencyHz = hz;
        }
        finally
        {
            _updatingFromRig = false;
        }
    }

    private void UpdateModeLine()
    {
        IsInsideCwSegment = SelectedBand.Band.IsInCwSegment(FrequencyHz);

        // THE SAME FACT, FROM THE SAME PLACE (HM-DEC-055). This line used to say
        // "OUTSIDE the CW segment" above the top of 20 m, which is true and
        // wildly understates matters, because up there it is not a ham band at
        // all. The map, the card and this line now all read one derivation, so
        // no two of them can disagree about it.
        var standing = AmateurSpectrum.Describe(FrequencyHz);

        ModeLineText = standing.IsAmateur
            ? $"CW · {SelectedBand.Band.Name} · "
              + (IsInsideCwSegment ? "inside the CW segment" : "OUTSIDE the CW segment")
            : $"CW · past the edge of {SelectedBand.Band.Name} · not a ham band";

        UpdatePrivileges();
    }

    /// <summary>
    /// Recompute the privilege spans and the status line for where the
    /// operator is tuned.
    /// </summary>
    /// <remarks>
    /// <para>One computation feeding both the map's veil and the line beneath
    /// it, so the hatching and the words can never contradict each other
    /// (HM-DEC-029).</para>
    /// <para>**THE MODE IS NO LONGER A CONSTANT** (unit 251 task 5). It was
    /// `TransmitMode.Cw` from when this was a CW app, so the card read *Your
    /// General license covers Morse here* while the operator sat on the Digital
    /// tab at 14.074 working FT8. That is a licence statement about a mode he is
    /// not using, and **a false licence statement is worse than a stale
    /// frequency** — it is the one place a confident wrong answer has legal
    /// consequences (§0.0, HM-DEC-029). The card answers for the tab he is on.</para>
    /// <para>**AND IT ALREADY FOLLOWED THE DIAL AND STILL DOES.** This is called
    /// from `UpdateModeLine`, which is called from `OnFrequencyHzChanged`, so the
    /// frequency it answers for is whatever is on the display — which after a
    /// confirmed tune is the frequency the radio reported, not the one Hamlet
    /// asked for.</para>
    /// </remarks>
    private void UpdatePrivileges()
    {
        var cls = _settings.Operator.LicenseClass;

        PrivilegeSpans = _privileges.SpansFor(SelectedBand.Band, cls);
        // The card answers two questions at once: what the license allows, and
        // what is actually going on where the dial is pointing (HM-DEC-054).
        PrivilegeStatus = PrivilegeStatusLine.Build(
            _privileges, cls, FrequencyHz, LicenceModeForTheTab,
            Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz)));

        // A pending mismatch is a question about a class the operator has
        // since changed. Answering it by other means makes it moot, and a
        // panel still offering "keep Technician" after they picked General
        // would be asking about a world that no longer exists.
        if (LicenseMismatch is { } pending && pending.Existing != cls)
        {
            LicenseMismatch = null;
        }

        // The ladder was opened from the listen-only line, and the button
        // that opens it disappears once the frequency is theirs. Leaving the
        // panel behind would turn an invitation into permanent chrome — and
        // an upgrade pitch inside a green "yours to use" box reads as a nag
        // (HM-DEC-029).
        if (UpgradeLadderVisible && PrivilegeStatus.Tone != PrivilegeTone.ListenOnly)
        {
            UpgradeLadderVisible = false;
            UpgradeLadder = Array.Empty<string>();
        }
        else if (UpgradeLadderVisible)
        {
            UpgradeLadder = PrivilegeStatusLine.UpgradeLadder(
                _privileges, cls, SelectedBand.Band);
        }

        OnPropertyChanged(nameof(LicenseClass));
        OnPropertyChanged(nameof(LicenseProvenance));
    }

    /// <summary>
    /// Show or hide the upgrade ladder.
    /// </summary>
    /// <remarks>
    /// On click only, never permanent chrome: a restriction the operator
    /// asked about is motivation, and the same words shown unbidden are a nag
    /// (HM-DEC-029).
    /// </remarks>
    [RelayCommand]
    private void ToggleUpgradeLadder()
    {
        UpgradeLadderVisible = !UpgradeLadderVisible;

        UpgradeLadder = UpgradeLadderVisible
            ? PrivilegeStatusLine.UpgradeLadder(
                _privileges, _settings.Operator.LicenseClass, SelectedBand.Band)
            : Array.Empty<string>();

        if (UpgradeLadderVisible)
        {
            AppEvents.UpgradeLadderOpened(
                _telemetry, PrivilegePlan.Describe(_settings.Operator.LicenseClass));
        }
    }

    /// <summary>
    /// Look up the operator's license class when it is missing (HM-DEC-028).
    /// </summary>
    /// <remarks>
    /// <para>Lazy and automatic: attached to the fact rather than to a wizard
    /// screen, because people skip wizards and a callsign can arrive from
    /// Settings or a hand-edited file. Runs on startup and whenever the
    /// profile changes.</para>
    /// <para>Never blocks and never opens a dialog. The status bar narrates
    /// and the operator carries on; if the service is down the class stays
    /// unknown, the map draws no overlay, and Settings still takes a
    /// hand-picked answer.</para>
    /// </remarks>
    public async Task ResolveProfileAsync()
    {
        if (!ProfileResolver.NeedsLookup(_settings.Operator))
        {
            return;
        }

        var callsign = _settings.Operator.Callsign?.Trim() ?? "";
        if (callsign.Length == 0)
        {
            return;
        }

        _licenseLookup?.Cancel();
        _licenseLookup?.Dispose();
        var cts = new CancellationTokenSource();
        _licenseLookup = cts;

        // Narrate only when something is actually missing. A profile that is
        // complete still gets checked for a class disagreement, and announcing
        // that check on every startup would be the app talking about itself.
        var showNarration = LicenseResolver.NeedsResolution(_settings.Operator)
                            || GridResolver.NeedsResolution(_settings.Operator);

        if (showNarration)
        {
            Narrate(ProfileResolver.LookingUpNarration(callsign));
        }

        ProfileResolution resolution;
        try
        {
            using var lookup = new CallookCallsignLookup(
                AboutViewModel.AppVersion, callsign);
            var resolver = new ProfileResolver(lookup);
            resolution = await resolver.ResolveAsync(_settings.Operator, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            // Never fatal: an unresolved profile is a supported state (§8).
            return;
        }

        // A lookup that answered at all wrote its receipt onto the profile
        // (HM-DEC-044), whether or not either fact was adopted. That is a
        // change worth saving: without it the badges would be recomputed from
        // an empty record on the next launch, and the profile would ask again
        // forever.
        var changed = resolution.RecordedALookup;

        switch (resolution.License.Outcome)
        {
            case LicenseResolutionOutcome.Resolved:
                changed = true;
                AppEvents.LicenseClassResolved(
                    _telemetry, PrivilegePlan.Describe(resolution.License.Found),
                    resolution.License.SourceName);
                break;

            case LicenseResolutionOutcome.Mismatch:
                // Shown, never applied. Their license, their call.
                LicenseMismatch = resolution.License;
                AppEvents.LicenseClassMismatch(
                    _telemetry,
                    PrivilegePlan.Describe(resolution.License.Found),
                    PrivilegePlan.Describe(resolution.License.Existing));
                break;

            case LicenseResolutionOutcome.NotFound:
                AppEvents.LicenseClassLookupFailed(
                    _telemetry, resolution.License.Outcome.ToString());
                break;

            default:
                break;
        }

        if (resolution.Unavailable)
        {
            AppEvents.LicenseClassLookupFailed(
                _telemetry, LicenseResolutionOutcome.Unavailable.ToString());
        }

        switch (resolution.Grid.Outcome)
        {
            case GridResolutionOutcome.Resolved:
                changed = true;

                // The grid is what the band cards and every distance rest on,
                // so the moment it arrives the screen has to catch up: nothing
                // dimmed a second ago and the sun is known now (HM-DEC-033).
                OnPropertyChanged(nameof(GridProvenance));
                UpdateBandCharacter(DateTime.UtcNow);
                UpdateSpotDistances();
                break;

            case GridResolutionOutcome.Mismatch:
                GridMismatch = resolution.Grid;
                break;

            default:
                break;
        }

        if (changed)
        {
            SettingsStore.Save(_settings);
        }

        if (showNarration || resolution.License.NeedsOperatorDecision)
        {
            var line = resolution.Narration;
            if (line.Length > 0)
            {
                StatusText = line;
            }
        }

        UpdatePrivileges();
    }

    /// <summary>Provenance for the grid square, shown in Settings.</summary>
    public string GridProvenance => GridResolver.DescribeProvenance(_settings.Operator);

    /// <summary>
    /// A lookup that disagrees with a hand-entered grid, awaiting the
    /// operator's answer (HM-DEC-037). Null when there is nothing to ask.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GridMismatchNarration))]
    private GridResolution? _gridMismatch;

    /// <summary>What the grid disagreement says, or "" (HM-DEC-089).</summary>
    public string GridMismatchNarration => GridMismatch?.Narration ?? "";

    /// <summary>Take the looked-up grid in place of the hand-entered one.</summary>
    [RelayCommand]
    private void AcceptLookedUpGrid()
    {
        if (GridMismatch is not { } mismatch)
        {
            return;
        }

        var point = OperatorLocation.FromGrid(mismatch.Found);
        if (point is not null)
        {
            _settings.Operator.SetPositionFromLookup(
                point.Value, mismatch.SourceName, DateTime.UtcNow);
            SettingsStore.Save(_settings);
        }

        GridMismatch = null;
        OnPropertyChanged(nameof(GridProvenance));
        UpdateBandCharacter(DateTime.UtcNow);
        UpdateSpotDistances();
        Narrate(GridResolver.DescribeProvenance(_settings.Operator));
    }

    /// <summary>Keep the grid the operator typed, and stop asking.</summary>
    /// <remarks>
    /// Re-stamps it as hand-entered today, so the same disagreement does not
    /// reappear on every startup. Declining is an answer, and an app that asked
    /// again tomorrow would not have heard it.
    /// </remarks>
    [RelayCommand]
    private void KeepMyGrid()
    {
        if (GridMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetGridByHand(mismatch.Existing, DateTime.UtcNow);
        SettingsStore.Save(_settings);

        GridMismatch = null;
        OnPropertyChanged(nameof(GridProvenance));
        Narrate(GridResolver.DescribeProvenance(_settings.Operator));
    }

    /// <summary>Take the looked-up class in place of the hand-set one.</summary>
    [RelayCommand]
    private void AcceptLookedUpClass()
    {
        if (LicenseMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetLicenseClass(
            mismatch.Found, LicenseClassSource.LookedUp, mismatch.SourceName, DateTime.UtcNow);
        SettingsStore.Save(_settings);

        AppEvents.LicenseClassResolved(
            _telemetry, PrivilegePlan.Describe(mismatch.Found), mismatch.SourceName);

        LicenseMismatch = null;
        Narrate(LicenseResolver.DescribeProvenance(_settings.Operator));
        UpdatePrivileges();
    }

    /// <summary>Keep the class the operator set, and stop asking.</summary>
    /// <remarks>
    /// Re-stamps the profile as hand-set today, so the same disagreement does
    /// not reappear on every startup. Declining is an answer, and an app that
    /// asked again tomorrow would not have heard it.
    /// </remarks>
    [RelayCommand]
    private void KeepMyLicenseClass()
    {
        if (LicenseMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetLicenseClass(
            mismatch.Existing, LicenseClassSource.EnteredByOperator, "", DateTime.UtcNow);
        SettingsStore.Save(_settings);

        LicenseMismatch = null;
        Narrate(LicenseResolver.DescribeProvenance(_settings.Operator));
        UpdatePrivileges();
    }

    /// <summary>
    /// Put the radio down and give the operator their controls back.
    /// </summary>
    /// <remarks>
    /// THE UI COMES BACK WHATEVER THE RADIO DOES. The rig's own teardown is
    /// bounded and never throws, and this belt goes with that brace: the state
    /// that re-enables Disconnect and the port list is set in a finally, so no
    /// failure anywhere above it can leave somebody stuck with a dead button
    /// and an app that thinks it is still connected (HM-DEC-051).
    /// </remarks>
    private async Task TearDownRigAsync()
    {
        try
        {
            StopRigMonitor();
            StopDecoding();
            StopRigSpectrum();
            Transmit.Attach(null);
            StopTrainingSpectrum();
            _rigSendTimer.Stop();
            _rigSendPending = false;

            // The scanner loses its radio first, so nothing can be mid-tune
            // while the port is closing (§0.2.1).
            // **THE TRANSMITTER FIRST.** A disconnect while a cycle is running
            // is the case with the most at stake, and the stop code goes out
            // while there is still a port to send it on (§0.2).
            AutoCall.StopNow();
            AutoCall.Attach(null, null, null);
            Scan.StopNow();
            Scan.Attach(null, null, null, null);

            if (_rig is not null)
            {
                _rig.FrequencyChanged -= OnRigFrequencyChanged;
                await _rig.DisconnectAsync();
                (_rig as IDisposable)?.Dispose();
            }
        }
        catch (Exception)
        {
            // Nothing here is worth trapping the operator for (§8).
        }
        finally
        {
            _rig = null;

            // **THE PORT AND THE ARMED SEND GO WITH THE RADIO, IN THE SAME
            // `finally`.** There is no state in which Hamlet believes it can
            // transmit and has no radio, because the two are cleared together.
            _rigPort = null;
            _armedSend = null;
            _transmitRefusal = "";
            _transmitSampleRate = Ft8Composer.DefaultSampleRate;

            IsConnected = false;
            ConnectButtonText = "Connect";
        }
    }

    /// <summary>The radio for one selection, and the port it is on.</summary>
    /// <param name="selection">A COM port name, or the training radio entry.</param>
    /// <returns>The rig, and its port, or null where it has none.</returns>
    /// <remarks>
    /// **THE PORT IS KEPT NOW INSTEAD OF BEING DISCARDED.** It was constructed
    /// here and thrown away in the same expression, which is why nothing in the
    /// tree could ever build an <see cref="Ft8ArmedSend"/>. **The training entry
    /// still returns no port**, because a simulator has none and must never
    /// become a route to a keying frame.
    /// </remarks>
    internal static (IRig Rig, ISerialPort? Port) CreateRig(string selection)
    {
        if (selection == TrainingRadio)
        {
            return (new TrainingRig(), null);
        }

        var port = new SystemSerialPort(selection);

        return (new Ic7300Rig(port), port);
    }

    private static IReadOnlyList<string> SafePortNames()
    {
        try
        {
            return SystemSerialPort.GetPortNames();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }
}

/// <summary>The persisted panel ids (HM-DEC-021). Strings typed once here,
/// never at a call site, so a typo cannot silently lose a saved state.</summary>
public static class PanelKeys
{
    /// <summary>The neighborhood map.</summary>
    public const string Map = "map";

    /// <summary>The dial tape.</summary>
    public const string Tape = "tape";

    /// <summary>The waterfall.</summary>
    public const string Waterfall = "waterfall";

    /// <summary>The band scanner (HM-DEC-107).</summary>
    public const string Scan = "scan";

    /// <summary>The calling cycle (HM-DEC-098).</summary>
    public const string AutoCall = "autocall";

    /// <summary>The CW terminal.</summary>
    public const string Terminal = "terminal";

    /// <summary>The Digital tab's waterfall (work instruction 037).</summary>
    /// <remarks>
    /// **A KEY OF ITS OWN, NOT THE CW WATERFALL'S.** The two panels are on
    /// different tabs and an operator who collapses one has said nothing about
    /// the other, so sharing a key would make each one toggle the other from
    /// under him (HM-DEC-021).
    /// </remarks>
    public const string DigitalWaterfall = "digital.waterfall";

    /// <summary>The Digital tab's decoded text.</summary>
    public const string DigitalDecoded = "digital.decoded";

    /// <summary>The Digital tab's plain-English messages.</summary>

    /// <summary>The Explorer's story card.</summary>
    public const string Story = "story";

    /// <summary>The mode field guide.</summary>
    public const string Guide = "guide";

    /// <summary>The happening-now feed.</summary>
    public const string Spots = "spots";

    /// <summary>The lead card and the band-conditions line (HM-DEC-025).</summary>
    public const string Lead = "lead";

    /// <summary>The worked contact, both sides (HM-DEC-043).</summary>
    public const string Contact = "contact";

    /// <summary>Sending Morse (HM-DEC-059).</summary>
    public const string Transmit = "transmit";

    /// <summary>Did anybody hear me (HM-DEC-075).</summary>
    public const string Heard = "heard";

    /// <summary>I can hear it and Hamlet can't (HM-DEC-084).</summary>
    public const string ReceiveHelp = "receiveHelp";

    /// <summary>The phrasebook (HM-DEC-059).</summary>
    public const string Phrasebook = "phrasebook";
}

/// <summary>One band button: the band plus its best-bet ranking for the hour
/// the app started. FG-001 replaces the ranking with live spot data.</summary>
public partial class BandButtonViewModel : ObservableObject
{
    /// <summary>
    /// What is currently known about this band's activity (HM-DEC-031).
    /// </summary>
    /// <remarks>
    /// Starts as "no data" rather than as an empty band, so a button says
    /// nothing until there is something to say. Refreshed on every spot
    /// reload.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivityPips))]
    [NotifyPropertyChangedFor(nameof(ActivityUnknown))]
    [NotifyPropertyChangedFor(nameof(ActivityTooltip))]
    private BandActivityReading _activity;

    /// <summary>How this card looks with the sun where it is (HM-DEC-033).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CardWidth))]
    [NotifyPropertyChangedFor(nameof(CardIcon))]
    [NotifyPropertyChangedFor(nameof(CardIconTint))]
    [NotifyPropertyChangedFor(nameof(CardBar))]
    [NotifyPropertyChangedFor(nameof(CardOpacity))]
    private BandCardStyle _card;

    /// <summary>
    /// What the sun and the season are doing to this band, in plain words.
    /// </summary>
    /// <remarks>
    /// Editorial text from the engine. It says what the sun is doing and what
    /// the band tends to do; it never says the band is open, which is
    /// propagation and not something Hamlet can see (FG-007, HM-DEC-033).
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivityTooltip))]
    private string _character = "";

    /// <summary>Creates the button model.</summary>
    /// <param name="band">The band this button selects.</param>
    public BandButtonViewModel(CwBand band)
    {
        Band = band;
        _activity = new BandActivityReading(
            band.Name, BandActivityState.NoData, 0, 0, 0,
            "no data.", "Hamlet has not asked the spot sources yet.",
            ConditionsConfidence.Blind);

        _card = BandCardStyles.For(
            band.Name, new[] { band.Name }, SolarSnapshot.Unknown);
    }

    /// <summary>The band this button selects.</summary>
    public CwBand Band { get; }

    /// <summary>
    /// True on the band the shared ranking puts first (HM-DEC-046).
    /// </summary>
    /// <remarks>
    /// Observable and recomputed on every refresh. It used to be fixed at
    /// construction from a clock lookup table, which is how the badge came to
    /// sit on a band with no pips while the lead card was pointing somewhere
    /// else on the same screen.
    /// </remarks>
    [ObservableProperty]
    private bool _isBestBet;

    /// <summary>
    /// What the badge says: an observation, or an admitted guess.
    /// </summary>
    [ObservableProperty]
    private string _bestBetLabel = "best bet now";

    /// <summary>The evidence behind the badge, on hover.</summary>
    [ObservableProperty]
    private string _bestBetTooltip = "";

    /// <summary>Filled pips on the indicator.</summary>
    public int ActivityPips => Activity.Pips;

    /// <summary>True when the indicator should draw as unknown.</summary>
    public bool ActivityUnknown => Activity.IsUnknown;

    /// <summary>
    /// The hover text: what the sun is doing, what the season tends to do, and
    /// what was actually heard.
    /// </summary>
    /// <remarks>
    /// The character comes first because it is the part nobody ever told this
    /// operator, and the evidence sentence closes it because that is the part
    /// Hamlet can actually vouch for (HM-DEC-031, HM-DEC-033).
    /// </remarks>
    public string ActivityTooltip
    {
        get
        {
            var text = Character.Length == 0
                ? Activity.Tooltip
                : Character + TooltipParagraphBreak + Activity.Tooltip;

            // **THE BADGE'S OWN REASON MOVED HERE WHEN THE BADGE STOPPED TAKING
            // CLICKS.** It used to carry this tooltip itself; it is now drawn
            // over the card and hit-tested out of the way, because in the
            // layout flow it was pushing badged cards down and overhanging its
            // neighbours, and on 2026-08-25 that cost the operator the ability
            // to click `40 m` at all. A thing that cannot be hovered cannot
            // explain itself, and dropping the explanation would be hiding
            // information rather than a control (§0.5).
            return IsBestBet && BestBetTooltip.Length > 0
                ? text + TooltipParagraphBreak + BestBetTooltip
                : text;
        }
    }

    /// <summary>Blank line between the character passage and the evidence.</summary>
    private const string TooltipParagraphBreak = "\n\n";

    /// <summary>Card width, following wavelength.</summary>
    public double CardWidth => Card.Width;

    /// <summary>Sun, moon, both, or neutral.</summary>
    public Controls.DayNightIcon CardIcon => Card.Icon;

    /// <summary>The icon's color.</summary>
    public Avalonia.Media.IBrush CardIconTint => Card.IconTint;

    /// <summary>The colored bar under the label.</summary>
    public Avalonia.Media.IBrush CardBar => Card.BarBrush;

    /// <summary>Dimmed when the band is out of its element.</summary>
    public double CardOpacity => Card.Opacity;

    /// <summary>Pips in a full indicator, for the control to size itself.</summary>
    public static int ActivityPipCount => BandActivity.MaxPips;
}

/// <summary>One happening-now card: the plain-language invitation plus the
/// honesty fields — source and age — the prime directive requires.</summary>
public partial class SpotViewModel : ObservableObject
{
    [ObservableProperty]
    private string _provenance = "";

    [ObservableProperty]
    private bool _isNew;

    /// <summary>
    /// Why the ranking put this card where it is (HM-DEC-025) — shown on the
    /// card's face, never in a tooltip. A card ranked highly without a stated
    /// reason is a guess presented as a decode.
    /// </summary>
    [ObservableProperty]
    private string _reason = "";

    /// <summary>
    /// How far away and roughly which way, e.g. "480 miles northeast", or ""
    /// (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Blank whenever the grid is unknown or the source did not say where the
    /// station is — an RBN spot never carries one, because what RBN states is
    /// where a receiver is, not where the transmitter is.
    /// </remarks>
    [ObservableProperty]
    private string _distance = "";

    /// <summary>
    /// The exact age, for anybody who wants the number (HM-DEC-045).
    /// </summary>
    /// <remarks>
    /// The card speaks in words because nobody says "17 min ago" out loud, and
    /// this is the trade that makes that safe: the figure is one hover away
    /// rather than gone.
    /// </remarks>
    [ObservableProperty]
    private string _ageTooltip = "";

    /// <summary>
    /// The mode family this spot belongs to (§0.6, HM-DEC-032).
    /// </summary>
    /// <remarks>
    /// READ OFF THE DATA, LIKE EVERY OTHER SURFACE. The card was the last thing
    /// in the app still speaking in one color while the map, the field guide and
    /// the waterfall all used the mode language. The family comes from the
    /// guide's own table rather than from a list this control carries, and there
    /// is no color literal anywhere near the card.
    /// </remarks>
    [ObservableProperty]
    private ModeFamily _family = ModeFamily.Open;

    /// <summary>
    /// How strongly to draw this card, 1 down to a floor (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// AGE FADES THE DISPLAY across each source's ruled lifetime, so the eye
    /// finds what is current without anybody reading a timestamp. Never zero: a
    /// card faded to nothing is a card removed, and removing one is what this
    /// whole design exists not to do.
    /// </remarks>
    [ObservableProperty]
    private double _prominence = 1.0;

    private ActivitySpot _spot;
    private DateTime _newUntilUtc;
    private SpotLifetimeSettings _lifetimes = SpotLifetimeSettings.Defaults;

    /// <summary>Wraps an engine spot for display.</summary>
    /// <param name="spot">The engine's spot.</param>
    /// <param name="nowUtc">The reference time for the age line.</param>
    /// <param name="isNew">True when this spot was not in the previous set.</param>
    /// <param name="reason">The ranking's stated reason for this card.</param>
    /// <param name="distance">How far away, or "" when it cannot be said.</param>
    /// <param name="lifetimes">The configured source lifetimes, which decide
    /// how this card talks about its own age (HM-DEC-045).</param>
    /// <param name="prominence">How strongly to draw it (HM-DEC-057).</param>
    public SpotViewModel(
        ActivitySpot spot, DateTime nowUtc, bool isNew,
        string reason = "", string distance = "",
        SpotLifetimeSettings? lifetimes = null, double prominence = 1.0)
    {
        _spot = spot;
        _distance = distance;
        _prominence = prominence;
        _family = ModeGuide.FamilyFor(spot.Mode);
        _lifetimes = lifetimes ?? SpotLifetimeSettings.Defaults;
        Key = KeyFor(spot);
        Story = spot.Story;
        FrequencyHz = spot.FrequencyHz;
        TuneLabel = "Tune " + (spot.FrequencyHz / 1_000_000.0)
            .ToString("0.000", CultureInfo.InvariantCulture);
        _isNew = isNew;
        _reason = reason;
        _newUntilUtc = nowUtc + MainWindowViewModel.NewSpotTagLifetime;
        Reage(nowUtc);
    }

    /// <summary>The spot behind this card.</summary>
    public ActivitySpot Spot => _spot;

    /// <summary>Identity across refreshes: what was said, and where.</summary>
    /// <param name="spot">The spot to key.</param>
    /// <returns>A stable key.</returns>
    public static string KeyFor(ActivitySpot spot)
        => spot.FrequencyHz.ToString(CultureInfo.InvariantCulture) + "|" + spot.Story;

    /// <summary>Identity across refreshes.</summary>
    public string Key { get; }

    /// <summary>The invitation.</summary>
    public string Story { get; }

    /// <summary>Where the Tune button goes.</summary>
    public long FrequencyHz { get; }

    /// <summary>Button text, e.g. "Tune 7.032".</summary>
    public string TuneLabel { get; }

    /// <summary>Take the refreshed spot; a surviving card stops being new.</summary>
    /// <param name="spot">The same spot, freshly reported.</param>
    /// <param name="nowUtc">Reference time.</param>
    /// <param name="reason">The ranking's reason, recomputed with the spot.</param>
    /// <param name="distance">How far away, recomputed with the spot.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <param name="prominence">How strongly to draw it (HM-DEC-057).</param>
    public void Update(
        ActivitySpot spot, DateTime nowUtc, string reason = "", string distance = "",
        SpotLifetimeSettings? lifetimes = null, double prominence = 1.0)
    {
        _spot = spot;
        _lifetimes = lifetimes ?? _lifetimes;
        Prominence = prominence;
        Family = ModeGuide.FamilyFor(spot.Mode);
        IsNew = false;
        if (reason.Length > 0)
        {
            Reason = reason;
        }

        Distance = distance;
        Reage(nowUtc);
    }

    /// <summary>Recompute the age line, and expire the "new" tag once it has
    /// had its thirty seconds.</summary>
    /// <param name="nowUtc">Reference time.</param>
    /// <remarks>
    /// OPPORTUNITY FRESHNESS, NOT FEED FRESHNESS (HM-DEC-045). This line says
    /// how long since the spot happened and whether that person is likely
    /// still there. How long since Hamlet last talked to the network is a
    /// different fact and belongs in the panel header. A feed that reloaded
    /// four seconds ago can be full of hour-old spots, and the wording must
    /// not let those two be confused.
    /// </remarks>
    public void Reage(DateTime nowUtc)
    {
        var elapsed = nowUtc - _spot.HeardAtUtc;

        var provenance = $"{_spot.Mode} · {_spot.Source} · "
            + SpotLifetime.DescribeOpportunity(_spot, elapsed, _lifetimes)
            + (Distance.Length > 0 ? $" · {Distance}" : "");

        // COMPOSED TOGETHER, SO THE CARD CANNOT SAY A THING TWICE (HM-DEC-068).
        // Both lines ask the same function whether that person is probably still
        // there, and neither of them is wrong to. Read one after the other they
        // said it twice, which reads as two pieces of evidence when it is one.
        var lines = CardText.Compose(Reason, provenance);

        Provenance = lines[1];

        // The exact figure stays available for anybody who wants it, which is
        // the trade that lets the card speak in words (HM-DEC-045).
        AgeTooltip = $"Reported {SpotFreshness.Describe(elapsed)} by {_spot.Source}.";

        if (IsNew && nowUtc >= _newUntilUtc)
        {
            IsNew = false;
        }
    }
}
