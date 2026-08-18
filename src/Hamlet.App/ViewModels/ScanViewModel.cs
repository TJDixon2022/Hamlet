using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;

namespace Hamlet.App.ViewModels;

/// <summary>
/// One place the scan thought was worth a look, as the screen shows it.
/// </summary>
/// <param name="FrequencyHz">Where it is.</param>
/// <param name="Label">The frequency, written for a person.</param>
/// <param name="Why">What the waterfall saw there, in words.</param>
/// <param name="Steady">
/// True when this looks like something switched on and left on.
/// </param>
public sealed record ScanCandidateRow(
    long FrequencyHz, string Label, string Why, bool Steady);

/// <summary>
/// What a dwell came to, as the screen shows it.
/// </summary>
/// <param name="FrequencyHz">
/// The frequency it was heard on, which is what a tap tunes to.
/// </param>
/// <param name="Label">The frequency, written for a person.</param>
/// <param name="Sentence">What was heard, or what was not.</param>
/// <param name="Stopped">Whether the scan stayed here.</param>
/// <param name="Sureness">
/// How far Hamlet stands behind it, from nought to one, or null where there is
/// nothing to stand behind.
/// </param>
/// <remarks>
/// <para>**A DWELL THAT FOUND NOTHING IS IN THIS LIST TOO.** Silence about a
/// place the scan visited is the collapsed-panel failure in §0.5: hiding detail
/// is fine and hiding information is not, and the frequencies a scan passed over
/// are half of what it measured.</para>
/// <para>**THE FREQUENCY IS THE ONE THE DWELL LISTENED AT AND NEVER THE BIN IT
/// WAS RANKED IN.** A candidate is a place the waterfall saw something; a dwell
/// is a place the dial actually sat and the decoder actually listened, so tuning
/// to anything else would send the operator somewhere Hamlet never heard.</para>
/// </remarks>
public sealed record ScanDwellRow(
    long FrequencyHz, string Label, string Sentence, bool Stopped, double? Sureness)
{
    /// <summary>How sure, in words, or empty where nothing was found.</summary>
    /// <remarks>
    /// Carried from the engine's own verdict rather than re-derived, so a call
    /// assembled from dim letters cannot be drawn like a solid one (§0.0,
    /// HM-DEC-108).
    /// </remarks>
    public string SurenessText => Sureness switch
    {
        null => "",
        >= 0.8 => "sure",
        >= 0.5 => "fairly sure",
        _ => "not at all sure",
    };

    /// <summary>True when there is a sureness worth drawing.</summary>
    public bool HasSureness => Sureness is not null;

    /// <summary>True where Hamlet stands fully behind what it heard.</summary>
    /// <remarks>
    /// **A STOP ASSEMBLED FROM DIM LETTERS MAY NOT BE DRAWN LIKE A CLEAN ONE**
    /// (§0.0). The words already differ and now the colour follows them, because
    /// a row the operator is about to act on is exactly where a confident looking
    /// maybe costs him an evening. Green is solid, amber says Hamlet heard
    /// something and is telling him how little it would bet on it.
    /// </remarks>
    public bool IsSolid => Sureness >= 0.8;

    /// <summary>What tapping this row does, said plainly.</summary>
    /// <remarks>
    /// It names the frequency rather than a bare verb, so the control says where
    /// it is about to send the dial and the operator can disagree before pressing
    /// it (§0.7, §0.2.1).
    /// </remarks>
    public string TuneLabel => $"listen at {Label}";
}

/// <summary>
/// The scanner, as the operator meets it (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**THE ENGINE HAS BEEN FINISHED AND UNREACHABLE, WHICH WAS THE RIGHT
/// SAFE STATE.** §0.2.1 requires an always-visible stop and until one existed no
/// scan could be allowed to start. This is that face, and nothing here decides
/// anything the engine has not already decided: every refusal, every abort and
/// every restore is `BandScanner`'s, and this reads them out.</para>
/// <para>**IT NEVER TRANSMITS AND HAS NO PATH TO** (§0.2). The only rig calls it
/// causes are the frequency reads and writes `BandScanner` makes.</para>
/// </remarks>
public sealed partial class ScanViewModel : ObservableObject
{
    private readonly string _segmentsPath;
    private readonly string _homePath;
    private readonly ScopeBinSurvey _survey = new();
    private readonly Action<string> _say;
    private readonly Action<long>? _tune;

    private IRig? _rig;
    private RigStateMonitor? _monitor;
    private CwDecoder? _decoder;
    private ISpectrumSource? _spectrum;
    private BandScanner? _scanner;
    private IScanHome? _home;
    private Task? _running;

    /// <summary>Creates the scanner's face.</summary>
    /// <param name="say">How to put a line in the status bar.</param>
    /// <param name="segmentsPath">
    /// The operator's own scan file. Defaults to the one beside his settings;
    /// a parameter so the refusal can be proved against a real bad file rather
    /// than argued about (§12.5).
    /// </param>
    /// <param name="homePath">Where the dial was when a scan started.</param>
    /// <param name="tune">
    /// How to send the dial somewhere the operator picked. The app hands over
    /// its own tuning path, the one a spot or a map dot uses, so a scan result
    /// arrives at the radio the same way every other destination does.
    /// </param>
    public ScanViewModel(
        Action<string> say,
        string? segmentsPath = null,
        string? homePath = null,
        Action<long>? tune = null)
    {
        _say = say;
        _tune = tune;
        _segmentsPath = segmentsPath ?? SettingsStore.ScanSegmentsPath;
        _homePath = homePath ?? SettingsStore.ScanHomePath;
    }

    /// <summary>True while a scan is moving the dial.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(Summary))]
    private bool _isScanning;

    /// <summary>
    /// The sentence that says Hamlet is moving the dial, or empty.
    /// </summary>
    /// <remarks>
    /// **THE OPERATOR MUST NEVER WONDER WHY HIS FREQUENCY CHANGED** (§0.2.1).
    /// This is shown in the pinned strip rather than inside a widget, because a
    /// widget can be taken off the canvas and the dial still moves.
    /// </remarks>
    [ObservableProperty]
    private string _movingLine = "";

    /// <summary>What happened last, in the app's voice.</summary>
    [ObservableProperty]
    private string _outcomeLine = "";

    /// <summary>Where the scan is right now, or empty.</summary>
    [ObservableProperty]
    private string _whereNow = "";

    /// <summary>Why a scan cannot be started, or empty when it can.</summary>
    /// <remarks>
    /// A refusal always prints its reason (HM-DEC-080). The engine settles
    /// every one of these and this only shows what it said.
    /// </remarks>
    [ObservableProperty]
    private string _refusal = "";

    /// <summary>Places the waterfall thought were worth a look.</summary>
    public ObservableCollection<ScanCandidateRow> Candidates { get; } = new();

    /// <summary>Everywhere the scan listened, newest first.</summary>
    public ObservableCollection<ScanDwellRow> Dwells { get; } = new();

    /// <summary>True when a scan could be started right now.</summary>
    public bool CanStart => !IsScanning && _rig is not null && _monitor is not null;

    /// <summary>True when the waterfall has proposed anywhere.</summary>
    public bool HasCandidates => Candidates.Count > 0;

    /// <summary>True when the scan has listened anywhere.</summary>
    public bool HasDwells => Dwells.Count > 0;

    /// <summary>Collapsed-header line for the scanner widget (§0.5).</summary>
    public string Summary
    {
        get
        {
            if (IsScanning)
            {
                return WhereNow.Length > 0
                    ? $"scanning · {WhereNow}"
                    : "scanning · moving your dial";
            }

            if (Dwells.Count > 0)
            {
                var stopped = Dwells.Count(d => d.Stopped);

                return stopped > 0
                    ? $"{Dwells.Count} places listened to, stopped at {stopped}"
                    : $"{Dwells.Count} places listened to, nobody found";
            }

            return "not scanning";
        }
    }

    /// <summary>
    /// Take in a sweep, so the survey has something to rank.
    /// </summary>
    /// <param name="frame">The sweep, as the radio computed it.</param>
    /// <remarks>
    /// **THIS ADDS NO TRAFFIC TO THE BUS.** It listens to frames that are
    /// arriving for the waterfall anyway and issues no command of its own, so
    /// having the scanner attached costs the poll loop nothing (HM-DEC-062).
    /// </remarks>
    public void Observe(in SpectrumFrame frame) => _survey.Observe(frame);

    /// <summary>
    /// Frames arrive on the source's own thread and only the survey touches
    /// them, which is why nothing is marshalled here.
    /// </summary>
    private void OnFrame(in SpectrumFrame frame) => _survey.Observe(frame);

    /// <summary>
    /// Hand the scanner what it needs, or take it away again.
    /// </summary>
    /// <param name="rig">The radio, or null on disconnect.</param>
    /// <param name="monitor">What Hamlet knows about it, or null.</param>
    /// <param name="decoder">The Morse decoder, or null.</param>
    /// <param name="spectrum">Where sweeps come from, or null.</param>
    public void Attach(
        IRig? rig, RigStateMonitor? monitor, CwDecoder? decoder, ISpectrumSource? spectrum)
    {
        if (_spectrum is not null)
        {
            _spectrum.FrameReady -= OnFrame;
        }

        _rig = rig;
        _monitor = monitor;
        _decoder = decoder;
        _spectrum = spectrum;

        if (_spectrum is not null)
        {
            _spectrum.FrameReady += OnFrame;
        }

        _survey.Reset();

        _scanner = rig is not null && monitor is not null
            ? new BandScanner(rig, monitor, Home())
            : null;

        OnPropertyChanged(nameof(CanStart));
    }

    /// <summary>
    /// Put the dial back after a scan that never finished (§0.2.1).
    /// </summary>
    /// <returns>The frequency restored, or null.</returns>
    /// <remarks>
    /// **CALLED ON CONNECT, WHICH IS THE ONLY MOMENT IT CAN BE.** If Hamlet went
    /// away mid-scan the operator's radio is parked somewhere he never chose,
    /// and the note on disk is the only thing that knows where he was.
    /// </remarks>
    public async Task<long?> RestoreHomeAsync()
    {
        if (_scanner is null)
        {
            return null;
        }

        var back = await _scanner.RestoreHomeAsync().ConfigureAwait(true);

        if (back is { } hz)
        {
            _say($"A scan was running when Hamlet last closed, so the dial has "
                + $"gone back to {hz / 1_000_000.0:0.000} MHz where you left it.");
        }

        return back;
    }

    /// <summary>Stop, now (§0.2.1).</summary>
    /// <remarks>
    /// Always available while a scan runs, and it awaits nothing, so it cannot
    /// queue behind the tune it is stopping.
    /// </remarks>
    [RelayCommand]
    private void Stop() => StopNow();

    /// <summary>
    /// Stop, from anywhere, awaiting nothing (§0.2.1).
    /// </summary>
    /// <remarks>
    /// Separate from the command so a disconnect can call it without going
    /// through a control that may not be on screen. **A stop that only exists
    /// as a button is a stop that does not exist while the button is gone.**
    /// </remarks>
    public void StopNow() => _scanner?.Stop();

    /// <summary>
    /// Go and listen to something the scan found (§0.2.1).
    /// </summary>
    /// <param name="row">The result the operator tapped.</param>
    /// <remarks>
    /// <para>**A LIST OF STATIONS IS A REPORT AND THE OPERATOR WANTS A
    /// DESTINATION.** This is the payoff on a scan: the frequency it was heard
    /// on, tuned to, with the scan stopped so nothing moves the dial out from
    /// under him while he is listening. A scan that carried on hunting while he
    /// read a station is §0.2.1's own practical test failing, because he could
    /// not tell where his radio had been left or why.</para>
    /// <para>**IT STOPS THE SCAN FIRST AND WAITS FOR IT TO PUT THE DIAL BACK.**
    /// Every exit route restores where the operator was before the scan started
    /// and this one is no exception. Only once that has happened does the tune go
    /// out, and it goes out through the same path a spot or a map dot uses, so
    /// what moves the dial to a result is the operator tuning rather than the
    /// scanner writing, and the crash-safe note on disk is cleared by the
    /// scanner's own restore rather than by anything here reaching around it.
    /// </para>
    /// <para>Nothing happens where there is nowhere to go, which is a guard
    /// against a row arriving without a frequency rather than an expectation that
    /// one will.</para>
    /// </remarks>
    [RelayCommand]
    private async Task TuneToDwellAsync(ScanDwellRow? row)
    {
        if (row is null || row.FrequencyHz <= 0)
        {
            return;
        }

        StopNow();

        if (_running is { } running)
        {
            try
            {
                await running.ConfigureAwait(true);
            }
            catch (Exception)
            {
                // Whatever went wrong with the scan is the scan's to report and
                // it already has. The operator asked to go somewhere, and that is
                // still worth doing.
            }
        }

        _tune?.Invoke(row.FrequencyHz);

        _say($"The scan has stopped and the dial has gone to {row.Label}, "
            + "which is where you asked to listen.");
    }

    /// <summary>Start a scan over what the waterfall has been watching.</summary>
    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsScanning)
        {
            return;
        }

        Refusal = "";
        OutcomeLine = "";
        Dwells.Clear();
        OnPropertyChanged(nameof(HasDwells));

        // **THE FILE IS CHECKED BEFORE THE RADIO IS, AND THE ORDER IS THE
        // POINT.** Whether the operator's scan file can be read is a fact about
        // his configuration and not about whether anything is plugged in, so a
        // broken one is worth saying the moment he asks for a scan rather than
        // after he has connected a radio and pressed again (§0.2.1).
        ScanSegments segments;

        try
        {
            segments = ScanSegments.LoadOrDefault(_segmentsPath);
        }
        catch (InvalidDataException error)
        {
            // **LOUDLY, AND NEVER A QUIET FALLBACK TO THE DEFAULT** (§0.2.1).
            // Substituting Hamlet's own list would run the scan over a stretch
            // the operator did not choose, which is what the file exists to
            // prevent.
            Refusal = "Hamlet will not scan until it can read your scan file. "
                + error.Message;
            _say(Refusal);
            return;
        }

        if (_scanner is null || _decoder is null)
        {
            Refusal = "There is no radio for Hamlet to scan with, and no way to "
                + "stop a scan that has nothing behind it.";
            _say(Refusal);
            return;
        }

        RankCandidates(segments);

        if (Candidates.Count == 0)
        {
            Refusal = "There is nothing to scan yet. The waterfall has to watch "
                + "the band for a few seconds before Hamlet can say which parts "
                + "of it are worth listening to.";
            _say(Refusal);
            return;
        }

        IsScanning = true;
        MovingLine = "Hamlet is moving your dial. It is working down the band a "
            + "stretch at a time, and it will put the dial back where you left it "
            + "when it is done.";

        var wanted = Candidates.Select(c => c.FrequencyHz).ToList();

        _scanner.DwellFinished += OnDwellFinished;

        ScanOutcome outcome;

        try
        {
            // **HELD SO A TAP CAN WAIT FOR IT** (§0.2.1). Stopping the scan asks
            // it to stop; the dial does not get back to where the operator left
            // it until the loop notices and finishes its own restore. A tune
            // issued before that lands first and the restore then drags the dial
            // off it, which is the operator being taken somewhere he did not
            // choose by the one feature meant to take him where he did.
            var run = _scanner.RunAsync(wanted, segments, ListenAsync);

            _running = run;
            outcome = await run.ConfigureAwait(true);
        }
        finally
        {
            _running = null;
            _scanner.DwellFinished -= OnDwellFinished;
            IsScanning = false;
            MovingLine = "";
            WhereNow = "";
        }

        OutcomeLine = outcome.Sentence;
        _say(outcome.Sentence);

        if (outcome.Cause is ScanStopCause.RigStateNotPopulated
            or ScanStopCause.Transmitting
            or ScanStopCause.NothingConfigured
            or ScanStopCause.RigStateUnusable
            or ScanStopCause.LinkSilent)
        {
            Refusal = outcome.Sentence;
        }

        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>
    /// Listen at one place for as long as the dwell asks, or until somebody
    /// turns up.
    /// </summary>
    /// <remarks>
    /// <para>**THE SETTLED PASS FEEDS THIS AND THE LEADING EDGE DOES NOT**
    /// (HM-DEC-096). A provisional reading is right far more often than not, and
    /// a scan acting on one would stop on a <c>CQ</c> that a second reading
    /// dissolves. The dial has already moved by then, so it is the one place the
    /// operator cannot check it.</para>
    /// </remarks>
    private async Task<ScanDwell> ListenAsync(
        long frequencyHz, double seconds, CancellationToken token)
    {
        var dwell = new ScanDwell(frequencyHz, seconds);
        var decoder = _decoder;

        WhereNow = $"listening at {frequencyHz / 1_000_000.0:0.000} MHz";
        OnPropertyChanged(nameof(Summary));

        if (decoder is null)
        {
            return dwell;
        }

        void Take(CwCharacter c) => dwell.Take(c);

        decoder.CharacterSettled += Take;

        try
        {
            // A tenth of a second is far finer than a dwell needs and it is what
            // lets a call heard in the third second end the dwell there rather
            // than at the twentieth.
            var step = TimeSpan.FromMilliseconds(100);

            for (var elapsed = 0.0; elapsed < dwell.Seconds; elapsed += 0.1)
            {
                if (dwell.Decide(elapsed) == DwellAction.Stay)
                {
                    break;
                }

                await Task.Delay(step, token).ConfigureAwait(true);
            }
        }
        finally
        {
            decoder.CharacterSettled -= Take;
        }

        return dwell;
    }

    private void OnDwellFinished(object? sender, ScanDwell dwell)
    {
        var stopped = dwell.Decide(dwell.Seconds) == DwellAction.Stay;

        Dwells.Insert(0, new ScanDwellRow(
            dwell.FrequencyHz,
            $"{dwell.FrequencyHz / 1_000_000.0:0.000} MHz",
            dwell.Verdict.Sentence,
            stopped,
            stopped ? dwell.Verdict.Confidence : null));

        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(HasDwells));
    }

    private void RankCandidates(ScanSegments segments)
    {
        Candidates.Clear();

        var fence = segments.Enabled;

        foreach (var bin in _survey.Ranked())
        {
            if (!fence.Any(s => s.Contains(bin.CenterHz)))
            {
                continue;
            }

            Candidates.Add(new ScanCandidateRow(
                bin.CenterHz,
                $"{bin.CenterHz / 1_000_000.0:0.000} MHz",
                $"present {bin.Presence:P0} of the time, "
                + $"{bin.LiftCounts:0} over the band",
                bin.LooksSteady));
        }

        foreach (var bin in _survey.Steady())
        {
            if (Candidates.Any(c => c.FrequencyHz == bin.CenterHz)
                || !fence.Any(s => s.Contains(bin.CenterHz)))
            {
                continue;
            }

            Candidates.Add(new ScanCandidateRow(
                bin.CenterHz,
                $"{bin.CenterHz / 1_000_000.0:0.000} MHz",
                "on all the time and never moving, which is what a carrier "
                + "looks like rather than a person",
                Steady: true));
        }

        OnPropertyChanged(nameof(HasCandidates));
    }

    private IScanHome Home() => _home ??= new FileScanHome(_homePath);
}
