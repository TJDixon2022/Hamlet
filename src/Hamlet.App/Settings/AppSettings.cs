using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;

namespace Hamlet.App.Settings;

/// <summary>
/// Everything Hamlet remembers between runs, in one file:
/// <c>%AppData%\Hamlet\settings.json</c> (HM-DEC-018). A corrupt or
/// unreadable file yields defaults — losing preferences is a nuisance,
/// refusing to start is a bug.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Window left in device-independent pixels; null until saved.</summary>
    public double? WindowX { get; set; }

    /// <summary>Window top; null until saved.</summary>
    public double? WindowY { get; set; }

    /// <summary>Window width; null until saved.</summary>
    public double? WindowWidth { get; set; }

    /// <summary>Window height; null until saved.</summary>
    public double? WindowHeight { get; set; }

    /// <summary>Whether the window was maximized at exit.</summary>
    public bool WindowMaximized { get; set; }

    /// <summary>Last selected port or the simulated-rig entry.</summary>
    public string? LastPort { get; set; }

    /// <summary>
    /// Reconnect to <see cref="LastPort"/> when the app opens.
    /// </summary>
    /// <remarks>
    /// On by default, because clicking Connect is friction on the one action
    /// the operator performs every single time and the app already knows which
    /// port they used. It fails quietly by design: a radio that is switched off
    /// or a port that has renumbered is the normal case rather than an error,
    /// and Hamlet says so in the status line and carries on with the training
    /// radio (HM-DEC-052).
    /// </remarks>
    public bool ReconnectOnStartup { get; set; } = true;

    /// <summary>
    /// Let Hamlet set the radio's mode to match where the dial is pointing.
    /// </summary>
    /// <remarks>
    /// On by default, because the operator this is for does not yet know that
    /// 14.074 wants USB-D and the app does. It is a setting rather than a
    /// constant because it is the first thing this app does to somebody's radio
    /// without being asked, and anybody who would rather drive themselves must
    /// be able to say so and be obeyed (HM-DEC-056).
    /// </remarks>
    public bool ModeFollowsTheMap { get; set; } = true;

    /// <summary>
    /// The frequencies the operator saved, in the order they chose (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// Kept here rather than in the radio's own memory channels, which are
    /// numbered slots whose meaning you have to remember. Hamlet's carry the
    /// reason: the band, the mode and what the map said lives there.
    /// </remarks>
    public List<SavedFavorite> Favorites { get; set; } = new();

    /// <summary>
    /// Where the operator has been, most recent first (HM-DEC-072).
    /// </summary>
    /// <remarks>
    /// Persisted for the same reason favorites are, and the moment it matters
    /// most is the following evening thinking "where was that station". A list
    /// that emptied on exit would fail exactly then.
    /// </remarks>
    public List<SavedRecentStation> Recent { get; set; } = new();

    /// <summary>
    /// True once a send on this installation produced a real SWR reading
    /// (HM-DEC-081).
    /// </summary>
    /// <remarks>
    /// What retires the note about not being able to see the back of the radio.
    /// It earns its place before a first transmission and becomes furniture
    /// after one, so it goes when Hamlet has measured something about the socket
    /// and the operator has seen the number. Persisted so it does not come back
    /// on restart.
    /// </remarks>
    public bool HasMeasuredSwr { get; set; }

    /// <summary>Last selected band name, e.g. "40 m".</summary>
    public string? LastBand { get; set; }

    /// <summary>Who is operating (HM-DEC-019). Displayed in the app, written
    /// here, and never written to telemetry.</summary>
    public OperatorProfile Operator { get; set; } = new();

    /// <summary>
    /// Which of the happening-now panel's two lenses was last chosen, or null
    /// when the operator has never chosen one (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// Null is the state that lets Hamlet guess. Once it holds a value the
    /// operator has answered the question themselves, and guessing again after
    /// that is the app arguing with them.
    /// </remarks>
    public string? SpotLens { get; set; }

    /// <summary>
    /// When the operator last finished looking at "what's new", or null.
    /// </summary>
    /// <remarks>
    /// The watermark the delta is measured from. It moves when they leave the
    /// lens rather than when they arrive at it, so the list stays still while
    /// they are reading it and is a fresh delta the next time they come back.
    /// </remarks>
    public DateTime? SpotsLastLookedUtc { get; set; }

    /// <summary>
    /// Which mode families the happening-now panel is showing (HM-DEC-061).
    /// </summary>
    /// <remarks>
    /// Null means all of them, which is where a fresh profile starts. Stored as
    /// names rather than numbers so somebody reading their own settings file can
    /// see what it says.
    /// </remarks>
    public List<string>? SpotFamilies { get; set; }

    /// <summary>Minutes between happening-now refreshes; 0 is off
    /// (HM-DEC-020). Allowed values are in
    /// <see cref="SpotRefreshChoices"/>.</summary>
    public int SpotRefreshMinutes { get; set; } = DefaultSpotRefreshMinutes;

    /// <summary>Per-panel expand/collapse state, keyed by panel id. An absent
    /// key means expanded — a new panel arrives open (HM-DEC-021).</summary>
    public Dictionary<string, bool> PanelExpanded { get; set; } = new();

    /// <summary>
    /// Per-source on/off switches, keyed by source name (HM-DEC-022,
    /// HM-DEC-024). An absent key falls back to
    /// <see cref="DefaultSourceEnabled"/>.
    /// </summary>
    public Dictionary<string, bool> SourceEnabled { get; set; } = new();

    /// <summary>Telemetry category switches. Absent category means enabled —
    /// all categories default on (HM-DEC-018).</summary>
    public Dictionary<string, bool> TelemetryCategories { get; set; } = new();

    /// <summary>Telemetry folder size cap in megabytes.</summary>
    public int TelemetryMaxMegabytes { get; set; } = 50;

    /// <summary>
    /// "Only let me transmit where my license allows" (HM-DEC-029). On by
    /// default.
    /// </summary>
    /// <remarks>
    /// TRANSMIT ONLY. This never restricts tuning, receiving or anything the
    /// band map draws — listening is not regulated and a setting that implied
    /// otherwise would teach a beginner something false about their own
    /// license. It is read at one moment: before Hamlet keys a transmitter.
    /// </remarks>
    public bool RestrictTransmitToPrivileges { get; set; } = true;

    /// <summary>
    /// Whether distances are spoken in miles or kilometers (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Miles, because the operator is American, the licence is American and the
    /// regulations are American — the same reasoning as the spelling standard
    /// (HM-DEC-035). It is a setting rather than a constant because the app is
    /// headed for a public release where most of the world counts the other
    /// way, and the default is picked rather than asked (§0.4).
    /// </remarks>
    public DistanceUnits DistanceUnits { get; set; } = DistanceUnits.Miles;

    /// <summary>
    /// Which byline was shown last launch, so the next one differs
    /// (HM-DEC-039). −1 when none has been shown.
    /// </summary>
    public int LastBylineIndex { get; set; } = -1;

    /// <summary>
    /// The capture device the operator chose to decode from, or null to let
    /// Hamlet pick.
    /// </summary>
    /// <remarks>
    /// Stored as the device's own id rather than its name, because names
    /// change when a driver updates and an id does not. A device that has
    /// been unplugged falls back quietly rather than leaving the app with
    /// nothing to listen to.
    /// </remarks>
    public string? AudioInputDeviceId { get; set; }

    /// <summary>
    /// The render device the operator's transmit audio goes to, or null where
    /// none has been named.
    /// </summary>
    /// <remarks>
    /// <para>**NULL MEANS THE SEND REFUSES, NOT THAT HAMLET PICKS**, and that is
    /// the whole difference between this and <see cref="AudioInputDeviceId"/>
    /// above. Listening to the wrong device is a quiet waterfall; transmitting to
    /// the wrong device puts FT8 tones through the laptop speakers, or into
    /// whatever the machine happens to default to, while the operator believes
    /// he is on the air. <c>WasapiTransmitSink</c> takes a name and **refuses
    /// rather than falling back** for the same reason.</para>
    /// <para>**THE SAME SHAPE AND THE SAME REASONING AS THE INPUT FIELD**: the
    /// device's own id, because names change when a driver updates and an id does
    /// not.</para>
    /// <para>**THE SETTINGS SCREEN FOR IT IS BUILT** - the picker unit 260 task 4
    /// shipped, at <c>SettingsViewModel.cs</c>'s <c>TransmitEndpoint</c> and
    /// <c>SettingsWindow.axaml</c>'s Transmit box. **This remark said it was not
    /// built** until unit 265, which put the transmit drive control beside that
    /// picker and had to read the remark to do it. With no endpoint named the send
    /// still refuses and says so, which is the sentence below and is unchanged.</para>
    /// </remarks>
    public string? AudioOutputDeviceId { get; set; }

    /// <summary>
    /// The peak amplitude Hamlet builds a transmission at - **0.25, which is
    /// -12.04 dBFS.**
    /// </summary>
    /// <remarks>
    /// <para>**IT IS A STARTING POINT THE OPERATOR ADJUSTS AGAINST HIS OWN
    /// RADIO'S ALC. IT IS NOT A FIGURE THIS REPOSITORY KNOWS.** `SHACK_FACTS.md`
    /// FACT-004 rules that what the IC-7300's USB modulation input expects is not
    /// in this repository and cannot be inferred from anything measured on the
    /// machine Hamlet was written on - no radio has ever been attached to it.
    /// What this default is, is a conservative place to start from, so that a
    /// first transmission is not a heavily overdriven signal over other people's
    /// band before he has looked at his ALC meter once.</para>
    /// <para>**THE ARITHMETIC IT WAS CHOSEN BY** (unit 265, and written out at
    /// length in `docs/unit265-the-level-trace.md`): `20*log10(0.25) = -12.04
    /// dBFS`, twelve dB below full scale and twice the six dB the unit was
    /// required to leave as a minimum. The transmit path's only quantisation is
    /// the float-to-PCM16 conversion in `WasapiTransmitSink`, where each 6.02 dB
    /// of drive costs one bit of a sixteen-bit word; at -12.04 dBFS about
    /// fourteen bits are in use and the quantisation floor is near -86 dBFS,
    /// leaving some 74 dB against a decoder that works at about -21 dB SNR. **The
    /// drive is not what limits the decode**, so the number could be chosen for
    /// what is sensible to hand a radio.</para>
    /// <para>**Before unit 265 there was no such field and no other way to set a
    /// level**: Hamlet composed at unit amplitude and transmitted at 0 dBFS, and
    /// nothing between the composer and the sound card multiplied a sample by
    /// anything. The value here is the only control there is.</para>
    /// <para>**Out of range is refused with a sentence, not clamped.** Zero or
    /// below and above 1.0 come back from `Ft8Composer` as
    /// `Ft8ComposeRefusal.DriveLevelRefused` with words the operator reads,
    /// because a level quietly corrected is a level he believes he set.</para>
    /// </remarks>
    public float TransmitDrivePeak { get; set; } = Ft8Composer.DefaultDrivePeak;

    /// <summary>The highest contact badge Hamlet has already mentioned.</summary>
    /// <remarks>
    /// <para>**IT REMEMBERS WHAT WAS SAID, NOT WHAT WAS EARNED** (work instruction
    /// 278). Which badges he has is derived from the log every time it is read, so
    /// a log that shrinks shows fewer of them and Hamlet never asserts a number it
    /// cannot see. This is the other fact: whether the congratulation has already
    /// been given, which is about Hamlet rather than about the log, and saying it
    /// twice would be worse than not remembering.</para>
    /// <para>**MINUS ONE MEANS HAMLET HAS NEVER LOOKED**, which is not the same as
    /// nothing having been mentioned. On the first read it is seeded to where the
    /// log already stands and **nothing is announced**: installing Hamlet beside a
    /// log of ten thousand contacts would otherwise congratulate him for all nine
    /// badges at once, in one line, which is a wall of text rather than the quiet
    /// acknowledgement he asked for. An acknowledgement is for a milestone he has
    /// just passed. Measured at ten thousand records in work instruction 278 task
    /// 5, where it read *That is 10 and 25 and 50 and 100 and 500 and 1,000 and
    /// 2,000 and 5,000 and 10,000 contacts logged.*</para>
    /// <para>Zero means Hamlet has looked and he had passed nothing yet.</para>
    /// </remarks>
    public int ContactBadgeAnnounced { get; set; } = -1;

    /// <summary>The modes whose first contact Hamlet has already mentioned.</summary>
    /// <remarks>
    /// <para>**THE SAME FACT AS <see cref="ContactBadgeAnnounced"/>, ABOUT A
    /// DIFFERENT ACHIEVEMENT** (work instruction 287 task 3). Which firsts he has
    /// is derived from the log every time it is read, so nothing here can make one
    /// outlive its record. This remembers only whether the congratulation has
    /// already been given.</para>
    /// <para>**NULL MEANS HAMLET HAS NEVER LOOKED**, which is unit 278's rule
    /// wearing a different type. On the first read it is seeded to whatever the log
    /// already holds and **nothing is announced**, because a log with three modes in
    /// it would otherwise fire three notices at once on a fresh install, one on top
    /// of another, for contacts Hamlet was not there for.</para>
    /// <para>**AN EMPTY LIST MEANS HAMLET HAS LOOKED AND HE HAD NONE**, which is a
    /// different fact and the one a new operator is in.</para>
    /// </remarks>
    public List<string>? ContactModeFirstsAnnounced { get; set; }

    /// <summary>
    /// Which groups of the achievements screen have already been announced, or
    /// null before the first look.
    /// </summary>
    /// <remarks>
    /// <para>**NULL AND EMPTY MEAN DIFFERENT THINGS, WHICH IS THE WHOLE POINT**
    /// (unit 278's rule, and work instruction 298 task 6). **Null is *nobody has
    /// looked yet*** and seeds silently: a man who imports fourteen contacts from
    /// another logger gets no notices at all for things he did last year. **Empty is
    /// *we looked and nothing was open***, which is a fresh install and where the
    /// first real reveal comes from.</para>
    /// <para>**IT HOLDS KEYS AND NOT TITLES.** `band-40m` survives a change to how a
    /// heading is worded; `40 m` would announce every group again the day somebody
    /// improved the copy.</para>
    /// </remarks>
    public List<string>? AchievementGroupsAnnounced { get; set; }

    /// <summary>
    /// True once the operator has tuned with the scroll wheel (HM-DEC-141).
    /// </summary>
    /// <remarks>
    /// What retires the hint under the frequency readout. A line explaining how
    /// to do something the operator has already done is a line that teaches them
    /// to stop reading that part of the window.
    /// </remarks>
    public bool HasTunedByWheel { get; set; }

    /// <summary>
    /// Whether the independent keying sweep is drawn on the terminal.
    /// </summary>
    /// <remarks>
    /// <para>**OFF, BECAUSE THE INSTRUMENT IS WRONG MORE OFTEN THAN IT IS
    /// RIGHT.** The sweep was built to tell the operator when the decoder is
    /// looking in the wrong place, and measured against independent readings it
    /// disagreed with the truth on fourteen of twenty recordings. Unit 1.11.10
    /// then measured its calibration **inside an overlap** rather than in a gap:
    /// the four recordings holding nothing swing 14.1 to 17.7 decibels while
    /// `cw-2026-08-25-021825`, which holds a station, swings 12.6 — below all of
    /// them. There is no bar that separates them.</para>
    /// <para>**IT KEEPS COMPUTING AND IT KEEPS WRITING TO THE SIDECAR.** What is
    /// wrong with it is that it asserts on screen, where a second panel
    /// contradicting the first sends the operator to the radio for a decoder
    /// condition. The measurements are still worth having beside a recording, and
    /// rebuilding the instrument is its own unit rather than tonight's work.</para>
    /// <para>A setting rather than a deletion, so the person diagnosing it can
    /// still see it.</para>
    /// </remarks>
    public bool ShowKeyingSweep { get; set; }

    /// <summary>
    /// Whether the joint cutter decides where characters are cut.
    /// </summary>
    /// <remarks>
    /// <para>**SHIPS OFF, AND THE RULING SAYS WHEN IT MAY SHIP ON** (Tim,
    /// 2026-08-27): default on if every floor and every anchor is green, default
    /// off and shipped anyway with the measurement reported if they are not. They
    /// are not.</para>
    /// <para>**WHAT IT DOES AND WHAT IT COSTS, MEASURED 2026-08-27.** It repairs
    /// the cuts it was built for — `AB OV E` becomes `ABOVE`, `BR EE Z E` becomes
    /// `BREEZE`, `REV■R` becomes `REVER` — and it loses every word space. On a
    /// compressed fist at thirty words a minute the word gap runs well under one
    /// unit, so scored against three and seven it reads as a character gap every
    /// time, and `cw-2026-08-18-004507`'s anchor `N HANDLING THIS MESSAG` needs
    /// those spaces.</para>
    /// <para>That is HM-DEC-115's finding arriving a second time: gaps have to be
    /// clustered from the sender's own keying and never taken as multiples of the
    /// dit. The cutter accepts this sender's three fitted classes and the
    /// streaming path does not always have them to give.</para>
    /// </remarks>
    public bool UseJointDecoder { get; set; }

    /// <summary>
    /// Whether the ported reference decoder reads the audio instead of the
    /// shipped path.
    /// </summary>
    /// <remarks>
    /// <para>**IT SHIPS OFF AND THE RULING SAYS WHEN IT MAY SHIP ON** (Tim,
    /// 2026-08-28): default on only if the four phantoms emit no letters, all
    /// twelve adjudicated anchors still read, both silence controls stay silent,
    /// and the reference picks the reading pitch on more captures than the
    /// shipped path does.</para>
    /// <para>**IT IS A PORT OF `cwdecoder.py`**, the decoder that has read the
    /// operator's own captures since before this phase began, and it is behind a
    /// setting so the two can be compared on his own audio and a regression is
    /// one toggle away from being undone.</para>
    /// </remarks>
    public bool UseReferenceDecoder { get; set; }

    /// <summary>
    /// The pitch the operator hears a CW signal at, in hertz.
    /// </summary>
    /// <remarks>
    /// The IC-7300 sets this between 300 and 900 Hz, and CI-V command
    /// <c>14 09</c> encodes exactly that range with 600 Hz at its midpoint
    /// (Full Manual section 19, p. 19-3, and p. 4-14). So 600 is the middle of
    /// what this radio does rather than a number carried in from elsewhere.
    /// The decoder tracks the tone within a window either side of this, since
    /// nobody tunes exactly.
    /// </remarks>
    public int CwPitchHz { get; set; } = DefaultCwPitchHz;

    /// <summary>The CW pitch the app ships with, in hertz.</summary>
    public const int DefaultCwPitchHz = 600;

    /// <summary>Lowest CW pitch the radio offers, in hertz.</summary>
    public const int MinimumCwPitchHz = 300;

    /// <summary>Highest CW pitch the radio offers, in hertz.</summary>
    public const int MaximumCwPitchHz = 900;

    /// <summary>
    /// The Morse speed the operator would rather work at, in words a minute.
    /// </summary>
    /// <remarks>
    /// <para>A PREFERENCE AND NOT A MEASUREMENT (HM-DEC-066, HM-OPEN-006).
    /// Hamlet has never listened to anybody copy anything, so this number says
    /// what they would rather work at and nothing about what they can do. The
    /// difference decides what the app is allowed to say: it may put a station
    /// at 28 words a minute against the number here and call it far over, since
    /// both are stated figures, and it may never turn that into a verdict about
    /// the person reading it.</para>
    /// <para>Nothing is filtered out by it and nothing is hidden. It is a
    /// preference the ranking weighs, so a station sending at a pace somebody
    /// asked for sits higher up a list they can still scroll past.</para>
    /// </remarks>
    public int CopySpeedWpm { get; set; } = DefaultCopySpeedWpm;

    /// <summary>
    /// The copy speed a fresh install starts at, in words a minute.
    /// </summary>
    /// <remarks>
    /// Thirteen, which is where the ranking has always drawn the line between a
    /// relaxed pace and an ordinary one, so the number is read from that scale
    /// rather than typed again beside it (HM-DEC-066). It is deliberately below
    /// what most of the band runs at. Somebody new is better served by an app
    /// that starts gentle and lets them raise it than by one that starts where
    /// the contest operators live and leaves them wondering why none of this
    /// sounds like the practice files.
    /// </remarks>
    public const int DefaultCopySpeedWpm = SpotRankWeights.RelaxedWpm;

    /// <summary>Slowest copy speed the setting offers, in words a minute.</summary>
    /// <remarks>
    /// Five is where the licensing code tests once sat and where most people
    /// start, so it is the floor rather than a number chosen for roundness.
    /// </remarks>
    public const int MinimumCopySpeedWpm = 5;

    /// <summary>Fastest copy speed the setting offers, in words a minute.</summary>
    /// <remarks>
    /// Forty is comfortably past what a contest runs at, so nobody meets a
    /// ceiling that says more about the app than about them.
    /// </remarks>
    public const int MaximumCopySpeedWpm = 40;

    /// <summary>
    /// How long a park or summit activation stays a live invitation, in
    /// minutes (HM-DEC-045).
    /// </summary>
    /// <remarks>
    /// An activator hauled gear somewhere on purpose and stays put working
    /// whoever calls, often for well over an hour, so an hour is generous
    /// rather than optimistic.
    /// </remarks>
    public int ActivationLifetimeMinutes { get; set; } = 60;

    /// <summary>How long a skimmer report stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Much shorter, because a skimmer report says somebody called CQ at that
    /// moment and nothing at all about whether they are still calling.
    /// </remarks>
    public int SkimmerLifetimeMinutes { get; set; } = 20;

    /// <summary>How long contest activity stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Longest of the three: contest stations sit on one frequency for the
    /// whole event. Only applied where the source actually said it was a
    /// contest exchange, never guessed from a busy band.
    /// </remarks>
    public int ContestLifetimeMinutes { get; set; } = 180;

    /// <summary>The configured lifetimes, with absurd values refused.</summary>
    [JsonIgnore]
    public SpotLifetimeSettings Lifetimes => SpotLifetimeSettings.FromMinutes(
        ActivationLifetimeMinutes, SkimmerLifetimeMinutes, ContestLifetimeMinutes);

    /// <summary>The refresh interval the app ships with, in minutes.</summary>
    public const int DefaultSpotRefreshMinutes = 5;

    /// <summary>The offered refresh intervals in minutes; 0 is off
    /// (HM-DEC-020).</summary>
    public static IReadOnlyList<int> SpotRefreshChoices { get; } =
        new[] { 0, 1, 2, 5, 10, 15 };

    /// <summary>True when the category is on. Unknown categories are on.</summary>
    public bool IsTelemetryEnabled(TelemetryCategory category)
        => !TelemetryCategories.TryGetValue(category.ToString(), out var on) || on;

    /// <summary>Turn a category on or off.</summary>
    public void SetTelemetryEnabled(TelemetryCategory category, bool enabled)
        => TelemetryCategories[category.ToString()] = enabled;

    /// <summary>How many telemetry categories are currently on. Derived from
    /// the switches, never stored alongside them.</summary>
    [JsonIgnore]
    public int EnabledTelemetryCategoryCount
        => Enum.GetValues<TelemetryCategory>().Count(IsTelemetryEnabled);

    /// <summary>Which tab the app was last showing — CW, Digital or Voice.</summary>
    /// <remarks>
    /// <para>**THE APP OPENS IN THE MODE IT WAS LAST IN** (Tim's ruling,
    /// 2026-09-05). It sits beside the panel expand states because it is the same
    /// kind of fact: how this operator wants this window to look, remembered
    /// between evenings.</para>
    /// <para>**NULL IS THE FRESH-FILE ANSWER AND IT MEANS CW**, which is the
    /// default this application has always opened on. An unreadable or unknown
    /// value means CW too — a settings file naming a tab that no longer exists
    /// must not leave the window with no workspace showing, which is the failure
    /// `ModeTabViewModel`'s own remarks record from 2026-08-27.</para>
    /// <para>**RESTORING IT TOUCHES NO RADIO.** Selecting a tab schedules a mode
    /// follow, and that write is gated on a connected rig; at construction there
    /// is none. Starting the app has never moved the operator's dial and does not
    /// start now.</para>
    /// </remarks>
    public string? LastOperatingMode { get; set; }

    /// <summary>
    /// Which digital sub-mode the operator last chose — FT8, FT4, PSK31 or WSPR.
    /// </summary>
    /// <remarks>
    /// <para>**WHAT HE CHOSE, WHICH IS NOT WHERE THE DIAL IS.** The mode strip
    /// already lights the chip whose block the dial is actually in, and that is a
    /// measurement (`DigitalModeChip`, unit 228). This is the separate fact of
    /// which one he last pressed. The two are stored, drawn and reasoned about
    /// apart, because a remembered choice drawn as a lit chip would assert the
    /// radio is somewhere it is not (§0.0, HM-DEC-092).</para>
    /// <para>Null means he has not chosen one, which is the fresh-file answer and
    /// stays the fresh-file answer.</para>
    /// </remarks>
    public string? LastDigitalSubMode { get; set; }

    /// <summary>
    /// Unit 251's single filter choice — `Everything`, `CqOnly` or `Mine`.
    /// **Read on load and never written since unit 252.**
    /// </summary>
    /// <remarks>
    /// **KEPT SO A FILE WRITTEN YESTERDAY DOES NOT SILENTLY RESET** (§6.1's
    /// second exception). Unit 252 replaced the one exclusive choice with the two
    /// independent toggles below, on Tim's ruling of 2026-09-06. Dropping this key
    /// outright would take an operator who had left the panel on `CQ only` back to
    /// `everything` on his next launch with nothing on screen to say why, and a
    /// settings reset that looks like the app forgetting him is the exact failure
    /// §6.1 was written about.
    /// <para>`SettingsMigrations` carries it into
    /// <see cref="DecodedShowCq"/> and <see cref="DecodedShowMine"/> when those
    /// two are absent, which is what an unmigrated file looks like. Nothing writes
    /// here any more, so the key ages out of a profile the first time the toggles
    /// are saved.</para>
    /// </remarks>
    public string? DecodedFilter { get; set; }

    /// <summary>Whether the decoded table is showing calls to anyone.</summary>
    /// <remarks>
    /// **ITS OWN KEY, BECAUSE IT IS ITS OWN TOGGLE** (Tim's ruling, 2026-09-06).
    /// `CQ` and `mine` are independent and both can be on at once, so one stored
    /// value could not carry them: the state he wants most evenings — the calls he
    /// could answer plus his own traffic — has no name in an enum.
    /// <para>Both false is `everything`, which is the fresh-file state and what
    /// this panel has always started in.</para>
    /// </remarks>
    public bool DecodedShowCq { get; set; }

    /// <summary>
    /// Unit 252's `mine` toggle. **Written by the migration and read by nothing
    /// since 2026-09-07.**
    /// </summary>
    /// <remarks>
    /// **THE TOGGLE BECAME A SIDE OF THE PANEL** (Tim's ruling, 2026-09-07), so
    /// there is no longer a filter for this key to restore. It is kept rather than
    /// deleted for the same reason <see cref="DecodedFilter"/> above is: a key that
    /// disappears takes an operator's stored answer with it silently, and a reader
    /// of an existing `settings.json` should be able to see what the value used to
    /// mean rather than find an orphan. It ages out of a profile the first time
    /// anything else is saved.
    /// </remarks>
    public bool DecodedShowMine { get; set; }

    /// <summary>Whether the decoded table shows the newest slot first.</summary>
    /// <remarks>
    /// **NEWEST AT THE TOP, AND THAT IS A RULING RATHER THAN A DEFAULT THIS
    /// CODE MAY WEIGH** (Tim, 2026-09-04). The toggle exists and it opens this
    /// way round. It sits beside the panel's expand state because it is the same
    /// kind of fact: how this operator wants this panel to look, remembered
    /// between evenings.
    /// </remarks>
    public bool DecodedNewestFirst { get; set; } = true;

    /// <summary>
    /// Whether every slot is also decoded through the faithful port, so the two
    /// decoders' counts can be read side by side.
    /// </summary>
    /// <remarks>
    /// <para>**OFF, AND OFF IS THE ANSWER FOR ALMOST EVERY EVENING.** Hamlet
    /// decodes through `Ft8Sharp.Deep`, which unit 246 asserted is a whole-result
    /// superset of the port over 69 recordings and 801 messages. Running both
    /// permanently would pay for a comparison the ladder already makes over
    /// hundreds of trials.</para>
    /// <para>**AND WHEN IT IS ON, THE PANEL DOES NOT CHANGE.** The port's counts
    /// go to telemetry and the capture sidecar as evidence; the messages on
    /// screen stay Deep's alone. Two lists that disagree would hand the operator
    /// an adjudication this application exists to make for him (§0.0).</para>
    /// </remarks>
    public bool CompareWithThePort { get; set; }

    /// <summary>True when the panel is expanded. Unknown panels are expanded.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    public bool IsPanelExpanded(string panelKey)
        => !PanelExpanded.TryGetValue(panelKey, out var open) || open;

    /// <summary>
    /// Whether an activity source ships switched on.
    /// </summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source is on by default.</returns>
    /// <remarks>
    /// <para>Two ship off. SOTA is off for a reason that is not technical: its
    /// API's terms of service require the developer to have registered with
    /// the SOTA Reflector's API-consumers group and to have had AI-written
    /// software approved before it connects. Hamlet will not enter into that
    /// on the operator's behalf, so the switch starts off and the reason is
    /// printed next to it (HM-DEC-024).</para>
    /// <para>The sample feed is off because live feeds now work, and mixing
    /// invented spots into a real list is the prime directive broken for the
    /// sake of a fuller-looking panel. It stays one click away, because it is
    /// how the Explorer gets built with no network.</para>
    /// </remarks>
    public static bool DefaultSourceEnabled(string sourceName)
        => !string.Equals(
               sourceName,
               RadioEngine.Explore.SotaActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase)
           && !string.Equals(
               sourceName,
               RadioEngine.Explore.FakeActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase);

    /// <summary>True when an activity source is switched on.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source should be polled.</returns>
    public bool IsSourceEnabled(string sourceName)
        => SourceEnabled.TryGetValue(sourceName, out var on)
            ? on
            : DefaultSourceEnabled(sourceName);

    /// <summary>Switch an activity source on or off.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <param name="enabled">True to poll it.</param>
    public void SetSourceEnabled(string sourceName, bool enabled)
        => SourceEnabled[sourceName] = enabled;

    /// <summary>Record a panel's expand/collapse state.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    /// <param name="expanded">True when the panel is open.</param>
    public void SetPanelExpanded(string panelKey, bool expanded)
        => PanelExpanded[panelKey] = expanded;
}

/// <summary>Loads and saves <see cref="AppSettings"/>, and owns the paths
/// every other component asks for.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

        // Enums as names, not numbers. Two reasons, and the second is the
        // serious one. "LicenseClass": "General" is legible to somebody
        // reading their own settings file, where "LicenseClass": 3 is not.
        // And a person who hand-edits it to "General" gets what they meant —
        // without this converter that write throws, LoadFrom catches, and
        // EVERY setting silently reverts to defaults, which is a spectacular
        // punishment for a reasonable guess (HM-DEC-028 expects the callsign
        // and class to arrive from hand-edited files).
        // Reading still accepts the numeric form, so files written by earlier
        // builds load unchanged.
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>%AppData%\Hamlet — the one folder Hamlet writes to.</summary>
    /// <remarks>
    /// **The setter is internal so a test can point the whole path at a temporary
    /// folder**, in the manner <c>MainWindowViewModel.CaptureFolder</c> already is.
    /// It defaults to the operator's real folder and nothing in the application
    /// ever assigns it; only the test assembly does, and it does so once for the
    /// whole run.
    ///
    /// Unit 235 measured why this seam has to exist. Nine tests of one class,
    /// run alone, rewrote the operator's own <c>settings.json</c> and touched his
    /// <c>spots.db</c> — because constructing a <c>MainWindowViewModel</c> opens
    /// the spot store, saves a byline index and resolves a callsign, all before a
    /// test body runs. Twenty test files construct one, thirty-nine times.
    ///
    /// The four paths below are computed on each read rather than captured once,
    /// which is the whole point: a folder fixed at static-initialisation time
    /// cannot be redirected by anything, and that is what made his folder
    /// reachable.
    /// </remarks>
    public static string DataFolder { get; internal set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hamlet");

    /// <summary>%AppData%\Hamlet\telemetry.</summary>
    public static string TelemetryFolder => Path.Combine(DataFolder, "telemetry");

    /// <summary>%AppData%\Hamlet\settings.json.</summary>
    public static string SettingsPath => Path.Combine(DataFolder, "settings.json");

    /// <summary>
    /// %AppData%\Hamlet\scan-segments.json — where a scan may move the dial.
    /// </summary>
    /// <remarks>
    /// **THE OPERATOR'S FILE, NOT HAMLET'S** (§0.2.1). Hamlet writes it once, the
    /// first time it has anywhere to put it, and never touches it again. It sits
    /// beside the settings rather than inside them because it is meant to be
    /// opened in an editor, and a stretch of band buried in a settings blob is a
    /// stretch of band nobody will edit.
    /// </remarks>
    public static string ScanSegmentsPath => Path.Combine(DataFolder, "scan-segments.json");

    /// <summary>
    /// %AppData%\Hamlet\scan-home — where the dial was when a scan started.
    /// </summary>
    /// <remarks>
    /// **A FILE RATHER THAN A SETTING, BECAUSE OF WHEN IT IS WRITTEN** (§0.2.1).
    /// It goes down in the moment before the first tune and is deleted the moment
    /// the dial is back, so it exists only while a scan is in flight. Settings
    /// are saved on a clean exit, which is the one exit this has to survive.
    /// </remarks>
    public static string ScanHomePath => Path.Combine(DataFolder, "scan-home");

    /// <summary>Load settings, or defaults if the file is missing, corrupt or
    /// unreadable. Never throws.</summary>
    public static AppSettings Load() => LoadFrom(SettingsPath);

    /// <summary>Save settings. Never throws; a failed save loses preferences,
    /// nothing more.</summary>
    public static void Save(AppSettings settings) => SaveTo(settings, SettingsPath);

    /// <summary>Load settings from an explicit path. The real load and the
    /// tested load are the same code (§5).</summary>
    /// <param name="path">Settings file path.</param>
    public static AppSettings LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, Options)
                           ?? new AppSettings();

            // Keys that have been renamed since the file was written are
            // carried forward here, so an upgrade never looks like the app
            // forgetting who the operator is (HM-DEC-035).
            SettingsMigrations.Apply(settings, json);

            return settings;
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <summary>Save settings to an explicit path. Never throws.</summary>
    /// <param name="settings">Settings to write.</param>
    /// <param name="path">Destination file path.</param>
    public static void SaveTo(AppSettings settings, string path)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception)
        {
            // Preferences are best-effort.
        }
    }

    /// <summary>Open the data folder in the OS file browser.</summary>
    public static void OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(DataFolder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DataFolder,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            // Nothing to do if the shell refuses.
        }
    }

    /// <summary>
    /// Open the operator's scan file in whatever edits text here (§0.2.1).
    /// </summary>
    /// <remarks>
    /// **THE FILE IS WRITTEN BEFORE IT IS OPENED, NEVER OVERWRITTEN.** §0.2.1
    /// requires the scanned stretch to come from a file the operator edits, and
    /// a menu entry that opens nothing is not a way to edit anything. Anything
    /// already there is his and is left exactly as it is.
    /// </remarks>
    public static void OpenScanSegments()
    {
        try
        {
            Hamlet.RadioEngine.Scan.ScanSegments.WriteDefaultIfMissing(ScanSegmentsPath);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ScanSegmentsPath,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            // Nothing to do if the shell refuses (§8).
        }
    }
}

/// <summary>One visited frequency, as settings.json holds it (HM-DEC-072).</summary>
/// <remarks>
/// A settings shape rather than the engine's record, for the same reason
/// <see cref="SavedFavorite"/> is one: anything persisted has to survive a
/// rename with a migration behind it (§6.1).
/// </remarks>
public sealed class SavedRecentStation
{
    /// <summary>Where it is.</summary>
    public long FrequencyHz { get; set; }

    /// <summary>The callsign if one was identified, or "".</summary>
    public string Station { get; set; } = "";

    /// <summary>The mode at the time.</summary>
    public string Mode { get; set; } = "";

    /// <summary>Which band.</summary>
    public string BandName { get; set; } = "";

    /// <summary>What the map said lives there.</summary>
    public string Neighborhood { get; set; } = "";

    /// <summary>When the visit was recorded.</summary>
    public DateTime VisitedUtc { get; set; }

    /// <summary>
    /// How many times the operator has settled here (HM-DEC-134).
    /// </summary>
    /// <remarks>
    /// **ABSENT IN EVERY PROFILE WRITTEN BEFORE HM-DEC-134**, and absent reads as
    /// one rather than as zero, because an entry is in this list precisely
    /// because somebody was there (§6.1). Nothing is migrated and nothing is
    /// lost: an existing list keeps every place in it and starts counting
    /// returns from the next one.
    /// </remarks>
    public int Visits { get; set; } = 1;

    /// <summary>
    /// How the station came to be known (HM-DEC-073), as its name.
    /// </summary>
    /// <remarks>
    /// Stored as a name rather than a number so somebody reading their own
    /// settings file can see what it says. Absent in a profile written before
    /// provenance existed, and those are read back as a spot feed, because that
    /// was the only way a name could get in there at the time. That is a fact
    /// about the history of the file rather than a guess about the entry.
    /// </remarks>
    public string StationSource { get; set; } = "";
}

/// <summary>One saved frequency, as settings.json holds it (HM-DEC-060).</summary>
/// <remarks>
/// A settings shape rather than the engine's record, because it is persisted and
/// anything persisted has to survive a rename with a migration behind it
/// (§6.1). It converts to and from
/// <see cref="Hamlet.RadioEngine.Explore.Favorite"/> at the edge.
/// </remarks>
public sealed class SavedFavorite
{
    /// <summary>Where it is.</summary>
    public long FrequencyHz { get; set; }

    /// <summary>What the operator calls it.</summary>
    public string Name { get; set; } = "";

    /// <summary>The mode it was saved in.</summary>
    public string Mode { get; set; } = "";

    /// <summary>Which band.</summary>
    public string BandName { get; set; } = "";

    /// <summary>What the map said lives there when it was saved.</summary>
    public string Neighborhood { get; set; } = "";

    /// <summary>When it was saved.</summary>
    public DateTime SavedUtc { get; set; }

    /// <summary>Why this one, in the operator's own words, or "".</summary>
    /// <remarks>
    /// Defaulted rather than required, so every favorite saved before notes
    /// existed still loads and simply has none. That is what an empty note means
    /// and there is nothing to migrate (§6.1).
    /// </remarks>
    public string Note { get; set; } = "";
}
