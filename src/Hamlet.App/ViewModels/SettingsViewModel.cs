using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Settings;
using Hamlet.App.Licensing;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;

namespace Hamlet.App.ViewModels;

/// <summary>
/// Settings dialog. Category switches write straight through to the settings
/// file, so a toggle takes effect on the next event with no Apply button
/// (HM-DEC-018).
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;

    [ObservableProperty]
    private int _maxMegabytes;

    [ObservableProperty]
    private string _usageText = "";

    [ObservableProperty]
    private string _callsign = "";

    [ObservableProperty]
    private string _operatorName = "";

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private string _gridSquare = "";

    [ObservableProperty]
    private RefreshChoice _spotRefresh;

    /// <summary>How long a park or summit activation stays worth chasing.</summary>
    [ObservableProperty]
    private int _activationLifetimeMinutes;

    /// <summary>How long a skimmer report stays worth chasing.</summary>
    [ObservableProperty]
    private int _skimmerLifetimeMinutes;

    /// <summary>How long contest activity stays worth chasing.</summary>
    [ObservableProperty]
    private int _contestLifetimeMinutes;

    [ObservableProperty]
    private LicenseClass _licenseClass;

    [ObservableProperty]
    private bool _restrictTransmitToPrivileges;

    [ObservableProperty]
    private string _licenseProvenance = "";

    [ObservableProperty]
    private string _gridProvenance = "";

    [ObservableProperty]
    private ProfileFactBadge _callsignBadge = ProfileFactBadge.None;

    [ObservableProperty]
    private ProfileFactBadge _gridBadge = ProfileFactBadge.None;

    [ObservableProperty]
    private ProfileFactBadge _licenseBadge = ProfileFactBadge.None;

    /// <summary>Which capture device the decoder listens to.</summary>
    [ObservableProperty]
    private AudioDevice? _audioDevice;

    /// <summary>The pitch the operator hears CW at, in hertz.</summary>
    [ObservableProperty]
    private int _cwPitchHz;

    /// <summary>
    /// The Morse speed the operator would rather work at, in words a minute.
    /// </summary>
    /// <remarks>
    /// A preference and never a measurement (HM-DEC-066). Nothing here tests
    /// anybody and nothing is hidden from a list because of it.
    /// </remarks>
    [ObservableProperty]
    private int _copySpeedWpm;

    /// <summary>Reconnect to the last radio when the app opens.</summary>
    [ObservableProperty]
    private bool _reconnectOnStartup;

    /// <summary>Let Hamlet set the mode to match where the dial is pointing.</summary>
    [ObservableProperty]
    private bool _modeFollowsTheMap;

    /// <summary>
    /// Recompute every badge from the profile as it stands right now.
    /// </summary>
    /// <remarks>
    /// Called on every keystroke rather than on save, because that is the
    /// whole promise: a badge saying "this is what the FCC record holds" has
    /// to stop saying it the instant the field says something else
    /// (HM-DEC-044). The badges are pure functions of the profile, so calling
    /// this too often costs nothing and calling it too rarely is a lie.
    /// </remarks>
    private void RefreshBadges()
    {
        CallsignBadge = ProfileFacts.Callsign(_settings.Operator);
        GridBadge = ProfileFacts.GridSquare(_settings.Operator);
        LicenseBadge = ProfileFacts.LicenseClass(_settings.Operator);
    }

    /// <summary>
    /// The one line that says what a grid square is (HM-DEC-037).
    /// </summary>
    /// <remarks>
    /// Beside the field rather than in a help page, because the person who
    /// needs it is looking at the field right now and will not go looking.
    /// </remarks>
    public string GridExplanation => GridResolver.Explanation;

    /// <summary>Designer constructor.</summary>
    public SettingsViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="settings">Live settings, written through on every edit.</param>
    /// <param name="telemetry">The writer, or null.</param>
    /// <param name="audioDevices">
    /// Where the capture device list comes from. Null uses WASAPI, which is
    /// what the running app wants and what a test never does.
    /// </param>
    /// <param name="transmitEndpoints">
    /// Where the render endpoint list comes from. **Null enumerates WASAPI, and
    /// a test always passes one** - work instruction 260 task 4 says the
    /// enumeration happens at run time and is never called from a test.
    /// </param>
    public SettingsViewModel(
        AppSettings settings,
        JsonlTelemetry? telemetry,
        IAudioDevices? audioDevices = null,
        Func<IReadOnlyList<RenderEndpoint>>? transmitEndpoints = null)
    {
        _settings = settings;
        _telemetry = telemetry;
        _maxMegabytes = settings.TelemetryMaxMegabytes;

        AudioDevices = (audioDevices ?? new WasapiAudioDevices()).List();
        _audioDevice = AudioDeviceChoice.Choose(AudioDevices, settings.AudioInputDeviceId);

        TransmitEndpoints = ListEndpoints(transmitEndpoints);
        _transmitEndpoint = ChooseEndpoint(TransmitEndpoints, settings.AudioOutputDeviceId);

        // **WHAT THE FILE NAMED, KEPT SO AN ABSENCE CAN BE TOLD FROM AN UNCHOOSING**
        // (work instruction 303 task 4). A device the operator deselected and a
        // device that did not come back after a reboot leave the picker looking
        // identical, and they are not the same thing at all.
        _missingTransmitDevice =
            _transmitEndpoint is null
            && !string.IsNullOrWhiteSpace(settings.AudioOutputDeviceId)
                ? settings.AudioOutputDeviceId
                : null;

        // **A DEVICE THAT HAS GONE IS A STATE CHANGE AND IT GOES IN THE FILE** (work
        // instruction 304 task 3). The snapshot says what was true at startup; this
        // says the moment Hamlet noticed the transmit device it was told to use is
        // not on the machine. **It is a warning**, because a send will refuse and the
        // operator will press CQ and see nothing happen.
        if (_missingTransmitDevice is { Length: > 0 } gone)
        {
            AppEvents.StateChanged(
                telemetry,
                "transmitDevicePresent",
                from: "true",
                to: "false",
                why: "settings name " + gone + " and this machine has "
                    + TransmitEndpoints.Count.ToString(
                        System.Globalization.CultureInfo.InvariantCulture)
                    + " output devices, none of them that one",
                level: TelemetryLevel.Warn);
        }
        _transmitDrivePercent = settings.TransmitDrivePeak * 100.0;
        _cwPitchHz = settings.CwPitchHz;
        _copySpeedWpm = settings.CopySpeedWpm;
        _reconnectOnStartup = settings.ReconnectOnStartup;
        _modeFollowsTheMap = settings.ModeFollowsTheMap;

        _callsign = settings.Operator.Callsign;
        _operatorName = settings.Operator.OperatorName;
        _location = settings.Operator.Location;
        _gridSquare = settings.Operator.GridSquare;
        _licenseClass = settings.Operator.LicenseClass;
        _licenseProvenance = LicenseResolver.DescribeProvenance(settings.Operator);
        _gridProvenance = GridResolver.DescribeProvenance(settings.Operator);
        _restrictTransmitToPrivileges = settings.RestrictTransmitToPrivileges;

        _activationLifetimeMinutes = settings.ActivationLifetimeMinutes;
        _skimmerLifetimeMinutes = settings.SkimmerLifetimeMinutes;
        _contestLifetimeMinutes = settings.ContestLifetimeMinutes;

        _callsignBadge = ProfileFacts.Callsign(settings.Operator);
        _gridBadge = ProfileFacts.GridSquare(settings.Operator);
        _licenseBadge = ProfileFacts.LicenseClass(settings.Operator);

        SpotRefreshChoices = AppSettings.SpotRefreshChoices
            .Select(m => new RefreshChoice(m))
            .ToList();
        _spotRefresh = SpotRefreshChoices
            .FirstOrDefault(c => c.Minutes == settings.SpotRefreshMinutes)
            ?? SpotRefreshChoices.First(
                c => c.Minutes == AppSettings.DefaultSpotRefreshMinutes);

        Categories = new ObservableCollection<TelemetryCategoryViewModel>(
            Describe().Select(d =>
            {
                var vm = new TelemetryCategoryViewModel(
                    d.Category, d.Name, d.Description,
                    settings.IsTelemetryEnabled(d.Category));
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(TelemetryCategoryViewModel.IsEnabled))
                    {
                        _settings.SetTelemetryEnabled(vm.Category, vm.IsEnabled);
                        SettingsStore.Save(_settings);
                    }
                };
                return vm;
            }));

        Sources = new ObservableCollection<SourceToggleViewModel>(
            DescribeSources(settings).Select(d =>
            {
                var vm = new SourceToggleViewModel(
                    d.Name, d.Description, d.Note, settings.IsSourceEnabled(d.Name));
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(SourceToggleViewModel.IsEnabled))
                    {
                        _settings.SetSourceEnabled(vm.SourceName, vm.IsEnabled);
                        SettingsStore.Save(_settings);
                        Telemetry.AppEvents.SourceToggled(
                            _telemetry, vm.SourceName, vm.IsEnabled);
                    }
                };
                return vm;
            }));

        RefreshUsage();
    }

    /// <summary>The capture devices this machine offers; empty is normal.</summary>
    public IReadOnlyList<AudioDevice> AudioDevices { get; }

    /// <summary>True when there is a capture device to choose between.</summary>
    public bool HasAudioDevices => AudioDevices.Count > 0;

    /// <summary>
    /// What to say when there is nothing to listen to.
    /// </summary>
    /// <remarks>
    /// A machine with no capture device is a perfectly ordinary machine, and
    /// the training radio works without one, so this states the fact and points
    /// at the way round it rather than reading as a fault.
    /// </remarks>
    public string AudioDeviceNote
        => AudioDevices.Count == 0
            ? "Hamlet cannot see a recording device on this computer just now. "
              + "Plug the radio in and reopen this window, or carry on with the "
              + "training radio, which makes its own Morse and needs nothing plugged in."
            : "Pick the input the radio's audio arrives on. With an IC-7300 that "
              + "is its own USB codec, which Hamlet chooses for you when it "
              + "recognizes the name.";

    /// <summary>
    /// The half of <see cref="AudioDeviceNote"/> that is a fault, or empty.
    /// </summary>
    /// <remarks>
    /// **A FAULT SPEAKS UNASKED AND ADVICE DOES NOT** (Tim, 2026-09-08). One
    /// property was carrying both: no capture device is something wrong the
    /// operator needs told, and which device to pick is a tip he can hover for.
    /// Splitting them is what lets the second move behind a mark without taking
    /// the first with it.
    /// </remarks>
    public string AudioDeviceFault
        => AudioDevices.Count == 0 ? AudioDeviceNote : string.Empty;

    /// <summary>The half of <see cref="AudioDeviceNote"/> that is a tip, or empty.</summary>
    public string AudioDeviceTip
        => AudioDevices.Count == 0 ? string.Empty : AudioDeviceNote;

    /// <summary>True while there is a fault worth a sentence on the screen.</summary>
    public bool HasAudioDeviceFault => AudioDeviceFault.Length > 0;

    // ---------------------------------------------------------------------
    // THE TRANSMIT ENDPOINT. Work instruction 260, task 4.
    // ---------------------------------------------------------------------

    /// <summary>The render endpoints this machine offers; empty is normal.</summary>
    /// <remarks>
    /// **THE SAME SHAPE AS <see cref="AudioDevices"/> AND DELIBERATELY A
    /// DIFFERENT LIST.** A capture device is something to listen to and a render
    /// endpoint is something to play into; <c>RenderEndpoint</c>'s own remarks say
    /// why the two may not be the same type.
    /// </remarks>
    public IReadOnlyList<RenderEndpoint> TransmitEndpoints { get; }

    /// <summary>True when there is a render endpoint to choose between.</summary>
    public bool HasTransmitEndpoints => TransmitEndpoints.Count > 0;

    /// <summary>
    /// What to say about the device the transmission is played into.
    /// </summary>
    /// <remarks>
    /// **IT SAYS WHAT THE DEVICE IS FOR IN THE OPERATOR'S WORDS.** *The radio's
    /// own USB audio input* is the thing being named, and naming it is not
    /// optional: with none named Hamlet refuses to transmit rather than playing
    /// into whatever the computer defaults to (0.0). An empty list is an ordinary
    /// machine and reads as a fact, not a fault.
    /// </remarks>
    public string TransmitEndpointNote
        => TransmitEndpoints.Count == 0
            ? "Hamlet cannot see a playback device on this computer just now, so "
              + "there is nothing to name here yet. Plug the radio in and reopen "
              + "this window."
            : "Pick the radio's own USB audio input, which is what carries FT8 out "
              + "of the computer. Hamlet will not choose one for you: with none "
              + "named it refuses to transmit rather than playing the tones into "
              + "whatever this computer happens to default to.";

    /// <summary>
    /// The half of <see cref="TransmitEndpointNote"/> that is a fault, or empty.
    /// </summary>
    /// <remarks>
    /// Split for <see cref="AudioDeviceFault"/>'s reason. **Nothing to play a
    /// transmission into is a fault on the screen that decides whether he can
    /// transmit at all**, so it does not go behind a hover.
    /// </remarks>
    public string TransmitEndpointFault
        => TransmitEndpoints.Count == 0 ? TransmitEndpointNote : string.Empty;

    /// <summary>The tip half, or empty.</summary>
    public string TransmitEndpointTip
        => TransmitEndpoints.Count == 0 ? string.Empty : TransmitEndpointNote;

    /// <summary>True while there is a fault worth a sentence on the screen.</summary>
    public bool HasTransmitEndpointFault => TransmitEndpointFault.Length > 0;

    /// <summary>
    /// Where a transmission's audio is played, or null where none is named.
    /// </summary>
    /// <remarks>
    /// **NOTHING IS CHOSEN ON THE OPERATOR'S BEHALF.** Where the saved id names
    /// no endpoint this comes back null and the box shows nothing selected, which
    /// is the honest state - the alternative is Hamlet quietly moving his
    /// transmission to a device he did not pick.
    /// </remarks>
    [ObservableProperty]
    private RenderEndpoint? _transmitEndpoint;

    partial void OnTransmitEndpointChanged(RenderEndpoint? value)
    {
        // **THIS PATH WAS SUSPECTED OF LOSING THE OPERATOR'S DEVICE AND IT WAS
        // MEASURED INNOCENT** (work instruction 303 task 4). The theory was that the
        // two-way binding writes null back when the saved selection is not among the
        // picker's items, erasing the id. **Two measurements say otherwise**:
        // realizing `SettingsWindow` headless leaves the saved id untouched, and
        // setting the selection to null does not even reach this handler, because
        // where the device is missing the selection is *already* null and the
        // generated setter stops on equality.
        //
        // **SO THE ERASURE THE OPERATOR SAW DID NOT COME FROM HERE**, and no guard
        // was added to pretend otherwise. What this unit does about a lost device is
        // make it **visible**, below.
        // **AND A SETTING THE OPERATOR CHANGES IS A STATE CHANGE TOO.** Which sound
        // card a transmission goes to is exactly the fact that went missing across a
        // reboot, so a deliberate change to it belongs in the record beside the
        // accidental one.
        AppEvents.StateChanged(
            _telemetry,
            "transmitDeviceSelected",
            from: _settings.AudioOutputDeviceId,
            to: value?.Id ?? "(none named, so a send refuses)",
            why: "the operator picked it in Settings");

        _settings.AudioOutputDeviceId = value?.Id;

        // Choosing a real device is what clears the warning.
        _missingTransmitDevice = null;

        OnPropertyChanged(nameof(TransmitDeviceWarning));
        OnPropertyChanged(nameof(HasTransmitDeviceWarning));

        SettingsStore.Save(_settings);
    }

    /// <summary>The transmit device named in settings that this machine has not got.</summary>
    private string? _missingTransmitDevice;

    /// <summary>
    /// **What is said on screen when the transmit device named in settings is not
    /// here.**
    /// </summary>
    /// <remarks>
    /// <para>**A FAULT SPEAKS UNASKED, AND THIS ONE SAID NOTHING AT ALL.** The
    /// operator pressed CQ, nothing happened, the Send line never changed and the
    /// stop control never armed; the only record was a refusal reason inside
    /// telemetry. **A refusal the operator cannot see is a button that appears
    /// broken.**</para>
    /// <para>**IT NAMES THE DEVICE RATHER THAN SAYING SOMETHING WENT WRONG**, because
    /// the thing he has to do about it is reconnect or re-select that particular
    /// sound card.</para>
    /// </remarks>
    public string TransmitDeviceWarning
        => _missingTransmitDevice is { Length: > 0 } missing
            ? "The sound card Hamlet was told to transmit through is not on this "
              + "machine just now, so sending is refused until it comes back or you "
              + "pick another one. It was saved as " + missing + "."
            : "";

    /// <summary>True where a saved transmit device is missing.</summary>
    public bool HasTransmitDeviceWarning => TransmitDeviceWarning.Length > 0;

    /// <summary>
    /// The peak amplitude Hamlet builds a transmission at, as a percentage of
    /// full scale.
    /// </summary>
    /// <remarks>
    /// <para>**A PERCENTAGE ON THE SCREEN AND A PEAK AMPLITUDE IN THE FILE.** The
    /// stored value is the number <c>Ft8Composer</c> takes, 0 to 1; a spinner
    /// showing `0.25` is a control an operator has to be taught to read, and
    /// `25 %` is one he does not. **The dBFS the level actually works out to is
    /// on screen beside it** (<see cref="TransmitDriveNote"/>), because that is
    /// the unit the number is talked about in.</para>
    /// <para>**IT WILL NOT ACCEPT A VALUE THE COMPOSER WOULD REFUSE.** The
    /// question is asked of <c>Ft8Composer.DriveIsUsable</c> rather than answered
    /// again here, so the screen and the send path cannot come to different
    /// answers about what is a level.</para>
    /// <para>**THE ARITHMETIC MOVED TO <see cref="TransmitDrive"/> AND THE
    /// BEHAVIOUR DID NOT** (work instruction 269, task 2). There is a second
    /// control over this one setting now, under the waterfall on the Digital tab,
    /// where step D asks the operator to read it; two copies of *percent goes in,
    /// peak comes out* is the drift this unit exists to make impossible. The
    /// conversion, the composer's question and the note sentence are the same
    /// code on both screens.</para>
    /// </remarks>
    [ObservableProperty]
    private double _transmitDrivePercent;

    partial void OnTransmitDrivePercentChanged(double value)
    {
        // ASKED OF THE COMPOSER, NOT DECIDED AGAIN HERE. A value it would refuse
        // is not written to the file at all - the setting keeps the level that
        // was last usable rather than storing one the send path would reject with
        // a sentence at the moment the operator pressed send.
        TransmitDrive.Write(_settings, value);

        OnPropertyChanged(nameof(TransmitDriveNote));
    }

    /// <summary>What the drive works out to, and what it is for.</summary>
    /// <remarks>
    /// **IT SAYS ON THE SCREEN THAT THE NUMBER IS A STARTING POINT.**
    /// `SHACK_FACTS.md` FACT-004: what the IC-7300's USB modulation input expects
    /// is not in this repository and cannot be inferred from anything measured on
    /// the machine Hamlet was written on. The operator sets this against his own
    /// radio's ALC meter, and the line says so rather than letting a default look
    /// like a specification.
    ///
    /// **THE WORDS ARE <see cref="TransmitDrive.NoteFor"/>'S AND THEY ARE
    /// UNCHANGED** (work instruction 269, task 2): the Digital tab's own drive
    /// control shows the same sentence, and two copies of a sentence that has to
    /// keep saying *this is not a specification* is one copy too many.
    /// </remarks>
    public string TransmitDriveNote
        => TransmitDrive.NoteFor(TransmitDrivePercent, _settings.TransmitDrivePeak);

    /// <summary>The endpoints, or none where the machine would not say.</summary>
    /// <remarks>
    /// **ENUMERATION IS A CALL INTO WINDOWS AND IT CAN FAIL.** A machine with the
    /// audio service stopped throws here, and a Settings window that will not open
    /// because of it is worse than one with an empty box that says so.
    /// </remarks>
    private static IReadOnlyList<RenderEndpoint> ListEndpoints(
        Func<IReadOnlyList<RenderEndpoint>>? source)
    {
        try
        {
            return (source ?? WasapiTransmitSink.Endpoints)();
        }
        catch (Exception)
        {
            return Array.Empty<RenderEndpoint>();
        }
    }

    /// <summary>The endpoint a saved id names, or null.</summary>
    /// <remarks>
    /// **BY ID, AND THERE IS NO FALLBACK.** <c>AudioDeviceChoice.Choose</c> may
    /// land on a sensible capture device because listening to the wrong one is a
    /// quiet waterfall; **transmitting into the wrong one puts FT8 somewhere the
    /// operator did not send it**, so an id that matches nothing selects nothing.
    /// </remarks>
    private static RenderEndpoint? ChooseEndpoint(
        IReadOnlyList<RenderEndpoint> endpoints, string? savedId)
        => string.IsNullOrWhiteSpace(savedId)
            ? null
            : endpoints.FirstOrDefault(
                e => string.Equals(e.Id, savedId, StringComparison.OrdinalIgnoreCase));

    /// <summary>The switchable categories.</summary>
    public ObservableCollection<TelemetryCategoryViewModel> Categories { get; }

    /// <summary>The switchable activity sources (HM-DEC-022, HM-DEC-024).</summary>
    public ObservableCollection<SourceToggleViewModel> Sources { get; }

    /// <summary>The license classes the operator can pick from.</summary>
    /// <remarks>
    /// Unknown is offered on purpose. Clearing the class is a legitimate
    /// thing to do, and it makes the band map stop claiming anything rather
    /// than leaving a stale guess on screen (HM-DEC-029).
    /// </remarks>
    public IReadOnlyList<LicenseClass> LicenseClasses { get; } = new[]
    {
        LicenseClass.Unknown, LicenseClass.Technician, LicenseClass.General,
        LicenseClass.Advanced, LicenseClass.Extra, LicenseClass.Novice,
    };

    /// <summary>The offered happening-now refresh intervals (HM-DEC-020).</summary>
    public IReadOnlyList<RefreshChoice> SpotRefreshChoices { get; }

    /// <summary>Where everything is stored.</summary>
    public string DataFolderPath => SettingsStore.DataFolder;

    partial void OnModeFollowsTheMapChanged(bool value)
    {
        _settings.ModeFollowsTheMap = value;
        SettingsStore.Save(_settings);
    }

    partial void OnReconnectOnStartupChanged(bool value)
    {
        _settings.ReconnectOnStartup = value;
        SettingsStore.Save(_settings);
    }

    partial void OnAudioDeviceChanged(AudioDevice? value)
    {
        _settings.AudioInputDeviceId = value?.Id;
        SettingsStore.Save(_settings);
        Telemetry.AppEvents.AudioDeviceChosen(_telemetry, value?.LooksLikeRadio ?? false);
    }

    /// <remarks>
    /// Held to the range the radio can actually produce (Full Manual p. 4-14).
    /// A pitch outside it is not a preference Hamlet can honor, and letting one
    /// be typed would leave the decoder hunting for a tone that is not there.
    /// </remarks>
    partial void OnCwPitchHzChanged(int value)
    {
        var held = Math.Clamp(
            value, AppSettings.MinimumCwPitchHz, AppSettings.MaximumCwPitchHz);

        if (held != value)
        {
            CwPitchHz = held;
            return;
        }

        _settings.CwPitchHz = held;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// What the Morse speed setting is for, in plain words.
    /// </summary>
    /// <remarks>
    /// THE COPY DOES THE WORK THE NUMBER CANNOT (HM-DEC-066, HM-OPEN-006). A
    /// speed box in a radio program reads like a test, and somebody who has
    /// never made a contact will read it as one and put a number in that they
    /// think they ought to manage. So this says what the figure means, says
    /// nothing is being measured, and says out loud that nothing disappears
    /// from the list because of it.
    /// </remarks>
    public string CopySpeedNote =>
        $"Words a minute is how Morse speed is counted, and {CopySpeedWpm} is about "
        + "the pace of the clubs that exist for people still finding their feet. "
        + "Hamlet uses it to sort the list, so stations sending around there come "
        + "up first and faster ones say how much faster they are. Nothing is "
        + "hidden and nothing is being tested here. Hamlet has never heard you "
        + "copy anything, so it will not tell you what you can manage. Move it up "
        + "as the letters start arriving on their own.";

    /// <remarks>
    /// Held to a range that spans the beginners and the contest operators, so
    /// nobody meets a ceiling that says more about the app than about them.
    /// </remarks>
    partial void OnCopySpeedWpmChanged(int value)
    {
        var held = Math.Clamp(
            value, AppSettings.MinimumCopySpeedWpm, AppSettings.MaximumCopySpeedWpm);

        if (held != value)
        {
            CopySpeedWpm = held;
            return;
        }

        _settings.CopySpeedWpm = held;
        SettingsStore.Save(_settings);
        OnPropertyChanged(nameof(CopySpeedNote));
    }

    partial void OnCallsignChanged(string value)
    {
        _settings.Operator.Callsign = value;
        RefreshBadges();
        SaveProfile("callsign");
    }

    partial void OnOperatorNameChanged(string value)
    {
        _settings.Operator.OperatorName = value;
        SaveProfile("name");
    }

    partial void OnLocationChanged(string value)
    {
        _settings.Operator.Location = value;
        SaveProfile("location");
    }

    /// <remarks>
    /// A typed grid is stamped as hand-entered, so a later lookup shows a
    /// disagreement rather than quietly replacing it — the same rule the
    /// license class follows, and it binds harder here because the FCC holds a
    /// mailing address and not an antenna (HM-DEC-028, HM-DEC-037).
    /// Clearing the box clears the stamp too, which hands the field back to
    /// the lookup.
    /// </remarks>
    partial void OnGridSquareChanged(string value)
    {
        _settings.Operator.SetGridByHand(value, DateTime.UtcNow);
        GridProvenance = GridResolver.DescribeProvenance(_settings.Operator);
        RefreshBadges();
        SaveProfile("grid");
    }

    partial void OnLicenseClassChanged(LicenseClass value)
    {
        // Chosen by hand, and stamped as such: a later lookup will show a
        // disagreement rather than quietly overwriting this (HM-DEC-028).
        if (value == _settings.Operator.LicenseClass)
        {
            return;
        }

        _settings.Operator.SetLicenseClass(
            value,
            value == LicenseClass.Unknown
                ? LicenseClassSource.Unset
                : LicenseClassSource.EnteredByOperator,
            "",
            DateTime.UtcNow);

        SettingsStore.Save(_settings);
        LicenseProvenance = LicenseResolver.DescribeProvenance(_settings.Operator);
        RefreshBadges();
        Telemetry.AppEvents.ProfileEdited(_telemetry, "licenseClass");
    }

    partial void OnRestrictTransmitToPrivilegesChanged(bool value)
    {
        _settings.RestrictTransmitToPrivileges = value;
        SettingsStore.Save(_settings);
        Telemetry.AppEvents.TransmitGuardToggled(_telemetry, value);
    }

    partial void OnSpotRefreshChanged(RefreshChoice value)
    {
        _settings.SpotRefreshMinutes = value.Minutes;
        SettingsStore.Save(_settings);
    }

    partial void OnActivationLifetimeMinutesChanged(int value)
    {
        _settings.ActivationLifetimeMinutes = value;
        SettingsStore.Save(_settings);
    }

    partial void OnSkimmerLifetimeMinutesChanged(int value)
    {
        _settings.SkimmerLifetimeMinutes = value;
        SettingsStore.Save(_settings);
    }

    partial void OnContestLifetimeMinutesChanged(int value)
    {
        _settings.ContestLifetimeMinutes = value;
        SettingsStore.Save(_settings);
    }

    /// <summary>Persist the profile, and record only WHICH field moved — the
    /// value is the identifying part and never goes to telemetry
    /// (HM-DEC-019).</summary>
    private void SaveProfile(string field)
    {
        SettingsStore.Save(_settings);
        Telemetry.AppEvents.ProfileEdited(_telemetry, field);
    }

    [RelayCommand]
    private void OpenFolder() => SettingsStore.OpenDataFolder();

    /// <summary>
    /// Open the file that says where a scan may move the dial (§0.2.1).
    /// </summary>
    /// <remarks>
    /// **THE ONE PLACE IN THE APP WHERE THE OPERATOR IS SENT TO A TEXT FILE**,
    /// and it is deliberate. §0.2.1 requires the scanned stretch to be his,
    /// configured in something he edits, and a set of frequency boxes in this
    /// window would be Hamlet's list with his numbers in it rather than his
    /// list. The file carries the source of every stretch Hamlet generated, and
    /// a box cannot carry a citation.
    /// </remarks>
    [RelayCommand]
    private void OpenScanFile() => SettingsStore.OpenScanSegments();

    /// <summary>Where the scan file is, so the window can say so.</summary>
    public string ScanFilePath => SettingsStore.ScanSegmentsPath;

    [RelayCommand]
    private void ClearTelemetry()
    {
        var removed = _telemetry?.ClearAll() ?? 0;
        RefreshUsage();
        UsageText = removed == 0
            ? "Nothing to clear."
            : $"Cleared {removed} file{(removed == 1 ? "" : "s")}.";
    }

    partial void OnMaxMegabytesChanged(int value)
    {
        _settings.TelemetryMaxMegabytes = value;
        SettingsStore.Save(_settings);
    }

    private void RefreshUsage()
    {
        var bytes = _telemetry?.TotalBytes() ?? 0;
        UsageText = $"Currently using {bytes / 1024.0 / 1024.0:0.00} MB.";
    }

    private static IEnumerable<(TelemetryCategory Category, string Name, string Description)> Describe()
    {
        yield return (TelemetryCategory.Diagnostics, "Diagnostics",
            "App start and stop, version, and unexpected errors.");
        yield return (TelemetryCategory.Rig, "Radio",
            "Connect attempts, timeouts and CI-V errors, with the port and radio type.");
        yield return (TelemetryCategory.Tuning, "Tuning",
            "Band changes and where a tune came from: the map, the dial tape, the digits or a spot.");
        yield return (TelemetryCategory.Explore, "Explore",
            "Which neighborhoods and field-guide cards get opened, and which spots get tuned.");
        yield return (TelemetryCategory.Decode, "Decoding",
            "That a decode ran and how confident it was. Never what was said.");
        yield return (TelemetryCategory.Performance, "Performance",
            "Frame rates and render timings, for finding slowness.");
    }

    /// <summary>
    /// The activity sources, what each one is, and any caveat the operator
    /// needs before switching it on (HM-DEC-024).
    /// </summary>
    private static IEnumerable<(string Name, string Description, string Note)>
        DescribeSources(AppSettings settings)
    {
        yield return (
            PotaActivitySource.SourceName,
            "Parks on the Air. Operators calling from parks, and the friendliest "
            + "contacts on the band: they want to be found and they will slow down "
            + "for you.",
            "");

        yield return (
            SotaActivitySource.SourceName,
            "Summits on the Air. Operators calling from mountain tops, often on slow "
            + "CW, often short of contacts.",
            SotaActivitySource.DisabledReason);

        yield return (
            RbnActivitySource.SourceName,
            "Reverse Beacon Network. Automated receivers reporting every CW signal "
            + "they decode. Filtered to your band and to receivers on your continent, "
            + "or it would be thousands a minute.",
            string.IsNullOrWhiteSpace(settings.Operator.Callsign)
                ? "Needs your callsign. RBN has no anonymous login and Hamlet will "
                  + "not invent one. Set it under Operator above."
                : "Your callsign is sent to RBN as the login, and to POTA and SOTA in "
                  + "the User-Agent, because those services are owed knowing who is "
                  + "calling them. It is still never written to telemetry.");

        yield return (
            FakeActivitySource.SourceName,
            "Built-in sample spots, labeled \"sample\" on every card. For seeing how "
            + "the Explorer behaves with no network.",
            "Off by default now that the live feeds work. Leaving it on mixes made-up "
            + "spots into a real list.");
    }
}

/// <summary>One switchable activity source.</summary>
public partial class SourceToggleViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    /// <summary>Creates the row.</summary>
    /// <param name="sourceName">Stable source name and settings key.</param>
    /// <param name="description">What the source is, in plain language.</param>
    /// <param name="note">A caveat to show under it, or "".</param>
    /// <param name="isEnabled">Current switch position.</param>
    public SourceToggleViewModel(
        string sourceName, string description, string note, bool isEnabled)
    {
        SourceName = sourceName;
        Description = description;
        Note = note;
        _isEnabled = isEnabled;
    }

    /// <summary>Stable source name, e.g. "POTA".</summary>
    public string SourceName { get; }

    /// <summary>What the source is.</summary>
    public string Description { get; }

    /// <summary>A caveat, or "".</summary>
    public string Note { get; }

    /// <summary>True when there is a caveat to show.</summary>
    public bool HasNote => Note.Length > 0;
}

/// <summary>One happening-now refresh interval offered in Settings.</summary>
public sealed class RefreshChoice
{
    /// <summary>Creates the choice.</summary>
    /// <param name="minutes">Interval in minutes; 0 is off.</param>
    public RefreshChoice(int minutes)
    {
        Minutes = minutes;
        Label = minutes switch
        {
            0 => "Off",
            1 => "Every minute",
            _ => $"Every {minutes} minutes",
        };
    }

    /// <summary>Interval in minutes; 0 is off.</summary>
    public int Minutes { get; }

    /// <summary>What the combo box shows.</summary>
    public string Label { get; }
}

/// <summary>One switchable telemetry category.</summary>
public partial class TelemetryCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    /// <summary>Creates the row.</summary>
    public TelemetryCategoryViewModel(
        TelemetryCategory category, string name, string description, bool isEnabled)
    {
        Category = category;
        Name = name;
        Description = description;
        _isEnabled = isEnabled;
    }

    /// <summary>Which category this row governs.</summary>
    public TelemetryCategory Category { get; }

    /// <summary>Display name.</summary>
    public string Name { get; }

    /// <summary>Plain-language description of what gets recorded.</summary>
    public string Description { get; }
}
