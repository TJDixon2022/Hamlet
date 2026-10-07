using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Capture;

/// <summary>What the app knows at a tick, for the automatic capture (work instruction 549).</summary>
/// <param name="Listening">Whether Hamlet is listening.</param>
/// <param name="InCw">Whether the radio is read in CW or CW-R.</param>
/// <param name="Scanning">Whether the scan is moving the dial.</param>
/// <param name="FrequencyHz">Where the dial is, or null where unread.</param>
/// <param name="FilterWidthHz">The radio's filter width, or null where unread.</param>
/// <param name="SendersHeld">How many senders the gate holds.</param>
/// <param name="AudioLostMilliseconds">Audio lost before the decode since listening began.</param>
public sealed record AutoCaptureConditions(
    bool Listening,
    bool InCw,
    bool Scanning,
    long? FrequencyHz,
    double? FilterWidthHz,
    int SendersHeld = 0,
    double AudioLostMilliseconds = 0);

/// <summary>
/// **HAMLET CAPTURES FOR ITSELF** (work instruction 549, HM-DEC-253): every W1AW session the radio sits on, whole.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-07**: W1AW, a strong clear signal *"as it is every day"*, printed stray letters in bursts, and
/// *"I think you can figure something out to allow this unattended. Perhaps auto capture on your side."* A replay of a W1AW
/// recording held one sender where the live path held four to nine; a long capture taken at the time is how to see why.</para>
/// <para>**IT ONLY LISTENS.** Nothing here tunes, changes mode, keys or writes to the radio: it reads what the app already
/// knows, once a second, and writes files under <see cref="AutoFolder"/>.</para>
/// <para>**A W1AW SESSION** (task 1): while Hamlet listens in CW with the dial on one of W1AW's Morse frequencies, from a
/// minute before a scheduled run to two minutes after it (<see cref="W1awSessionWindow"/>), the audio is written as it
/// arrives in back-to-back five-minute pieces (<see cref="ContinuousCapture"/>) into <c>w1aw-&lt;date&gt;-&lt;time&gt;</c>.
/// Leaving the frequency, leaving CW or stopping listening ends it there, and its last sheet says which.</para>
/// </remarks>
public sealed class CwAutoCapture : IDisposable
{
    private readonly W1awMorseFrequencies _table;
    private readonly Func<DateTime> _clock;
    private readonly bool _threaded;
    private readonly object _gate = new();

    private IAudioSource? _source;
    private ContinuousCapture? _w1aw;
    private W1awScheduleState? _session;
    private W1awMorseRow? _row;

    // **ITS OWN FIVE-MINUTE RING, WHATEVER RECORD IS SET TO** (task 2): a trouble capture holds the five minutes before it.
    private readonly AudioTap _ring = new(AudioTap.MaximumSecondsKept);
    private readonly TroubleWatch _watch = new();
    private readonly List<string> _troubleFolders = new();

    /// <summary>The automatic capture.</summary>
    /// <param name="autoFolder">Where automatic captures go: <c>captures\auto</c> under Hamlet's data folder.</param>
    /// <param name="table">W1AW's schedule and frequencies.</param>
    /// <param name="clock">The clock, in UTC.</param>
    /// <param name="threaded">
    /// True to write on a thread of its own behind a queue, as the app does; false to write on the caller's thread, for
    /// tests that feed audio and read the files straight after.
    /// </param>
    public CwAutoCapture(string autoFolder, W1awMorseFrequencies table, Func<DateTime> clock, bool threaded = true)
    {
        AutoFolder = autoFolder ?? throw new ArgumentNullException(nameof(autoFolder));
        _table = table ?? throw new ArgumentNullException(nameof(table));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _threaded = threaded;
    }

    /// <summary>Where automatic captures go.</summary>
    public string AutoFolder { get; }

    /// <summary>
    /// The app's lines for a sheet - the radio's frequency, band, the shape side's pitch and senders, the audio line and the
    /// rig state - as it last composed them. Read on the capture's thread, so it returns a string the app made on its own.
    /// </summary>
    public Func<string>? SheetExtra { get; set; }

    /// <summary>The W1AW capture under way, or null.</summary>
    public ContinuousCapture? W1awCapture
    {
        get { lock (_gate) { return _w1aw; } }
    }

    /// <summary>The run the W1AW capture under way is of, or null.</summary>
    public W1awScheduleState? W1awSession
    {
        get { lock (_gate) { return _session; } }
    }

    /// <summary>The folder of the capture under way, or null: for the telemetry, the folder's name alone.</summary>
    public string? CaptureUnderWay
    {
        get { lock (_gate) { return _w1aw is null ? null : Path.GetFileName(_w1aw.Folder); } }
    }

    /// <summary>
    /// The line under the terminal header: <c>capturing W1AW · code practice · piece 3 · 12:40</c> while a session is
    /// written, the time being how long it has run; empty otherwise.
    /// </summary>
    public string Line
    {
        get
        {
            lock (_gate)
            {
                if (_w1aw is null || _session is null)
                {
                    return string.Empty;
                }

                return $"capturing W1AW · {_session.Run.Kind} · piece {_w1aw.PieceNumber} · {Elapsed(_clock() - _w1aw.StartedUtc)}";
            }
        }
    }

    /// <summary>The line's hover: where the files go.</summary>
    public string Tip
    {
        get
        {
            lock (_gate)
            {
                return _w1aw is null
                    ? $"Hamlet records every W1AW session it is listening to, and keeps it in {AutoFolder}."
                    : $"Recording W1AW's session in five-minute pieces, each a .wav with a .txt beside it saying what was scheduled "
                        + $"and what the radio was doing, into {_w1aw.Folder}. It stops on its own two minutes after the session "
                        + "is scheduled to end, or as soon as the dial leaves W1AW's frequency or Hamlet stops listening.";
            }
        }
    }

    /// <summary>Hear the audio a source delivers: on its thread, a copy into the queue and nothing more.</summary>
    /// <param name="source">The source, or null to stop.</param>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_source, source))
        {
            return;
        }

        if (_source is not null)
        {
            _source.SamplesReady -= OnSamples;
        }

        _source = source;

        if (_source is not null)
        {
            _source.SamplesReady += OnSamples;
        }
    }

    /// <summary>Hear one chunk, as the source's callback does.</summary>
    /// <param name="firstSampleIndex">Its place on the audio clock.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="samples">The samples.</param>
    public void Hear(long firstSampleIndex, int sampleRate, ReadOnlySpan<float> samples)
    {
        // **ONE REFERENCE, READ WITHOUT A LOCK**: the callback never waits on a tick, and a tick that is stopping a capture
        // takes the reference away before it waits for the queue to drain.
        _ring.Take(samples, sampleRate);

        var writing = Volatile.Read(ref _writing);

        if (writing?.Feed is not null)
        {
            writing.Feed.Offer(firstSampleIndex, sampleRate, samples);
        }
        else
        {
            writing?.Capture.Append(firstSampleIndex, sampleRate, samples);
        }
    }

    // What the callback writes into: the capture and its queue, swapped whole.
    private Writing? _writing;

    private sealed record Writing(ContinuousCapture Capture, CaptureFeed? Feed);

    /// <summary>Wait until the capture under way has written everything handed to it: for the tests.</summary>
    /// <param name="timeout">How long to wait.</param>
    /// <returns>Whether it was written in time.</returns>
    internal bool WaitUntilWritten(TimeSpan timeout) => Volatile.Read(ref _writing)?.Feed?.WaitUntilWritten(timeout) ?? true;

    /// <summary>Decide, once a second, whether a W1AW capture starts or ends.</summary>
    /// <param name="now">What the app knows.</param>
    public void Tick(AutoCaptureConditions now)
    {
        ArgumentNullException.ThrowIfNull(now);

        var at = _clock();
        var session = W1awSessionWindow.At(_table, at);
        var row = now.FrequencyHz is { } hz ? W1awSessionWindow.RowAt(_table, hz, now.FilterWidthHz) : null;
        (ContinuousCapture Capture, CaptureFeed? Feed, string Why)? ending = null;

        lock (_gate)
        {
            if (_w1aw is not null)
            {
                var why = !now.Listening ? "listening stopped"
                    : !now.InCw ? "the radio left CW"
                    : row is null || row != _row ? "the dial left W1AW's frequency"
                    : session is null ? "the session's window closed, two minutes after its scheduled end"
                    : session.StartUtc != _session!.StartUtc ? "the next scheduled session began"
                    : _w1aw.EndReason;

                if (why is not null)
                {
                    ending = TakeLocked(why);
                }
            }

            if (_w1aw is null && now.Listening && now.InCw && !now.Scanning && session is not null && row is not null)
            {
                StartLocked(session, row, W1awSessionWindow.ToleranceHz(now.FilterWidthHz), at);
            }
        }

        Finish(ending);
        Trouble(now, at);
    }

    /// <summary>A character the terminal printed, from the chain's thread: what the stray-letter trigger reads.</summary>
    /// <param name="character">The character, or a word gap.</param>
    public void Character(CwCharacter character) => _watch.Character(character, _clock());

    /// <summary>The trouble captures saved so far, by folder name, oldest first.</summary>
    public IReadOnlyList<string> TroubleFolders
    {
        get { lock (_gate) { return _troubleFolders.ToList(); } }
    }

    /// <summary>The last trigger that fired, or null: for the line and the telemetry.</summary>
    public TroubleFired? LastTrouble { get; private set; }

    // **ANYWHERE ELSE, WHILE LISTENING IN CW AND NOT SCANNING** (task 2): a trigger saves the ring's last five minutes; during
    // a W1AW capture it is noted on that capture's sheet instead, since the capture already holds the audio.
    private void Trouble(AutoCaptureConditions now, DateTime at)
    {
        if (!now.Listening || !now.InCw || now.Scanning)
        {
            _watch.Pause(now.AudioLostMilliseconds);
            return;
        }

        foreach (var fired in _watch.Observe(at, now.SendersHeld, now.AudioLostMilliseconds))
        {
            LastTrouble = fired;

            var capture = W1awCapture;

            if (capture is not null)
            {
                capture.Note($"trigger    {fired.AtUtc:HH:mm:ss} UTC  {fired.Detail}  (noted here instead of saving again)");
                continue;
            }

            SaveTrouble(fired);
        }
    }

    private void SaveTrouble(TroubleFired fired)
    {
        // Everything the ring holds: five minutes once it has heard five, and less, never padded, before then.
        var audio = _ring.Snapshot();

        if (audio is null || audio.Samples.Length == 0)
        {
            return;
        }

        var name = $"trouble-{fired.AtUtc:yyyy-MM-dd-HHmmss}-{fired.Token}";
        var folder = Path.Combine(AutoFolder, name);
        var seen = _ring.SamplesSeen;
        var extra = SheetExtra?.Invoke() ?? string.Empty;

        lock (_gate)
        {
            _troubleFolders.Add(name);
        }

        void Write()
        {
            try
            {
                WavAudio.Write(Path.Combine(folder, "capture.wav"), audio);
                File.WriteAllText(Path.Combine(folder, "capture.txt"), TroubleSheet(fired, audio, seen, extra));
            }
            catch (Exception)
            {
                // Never-throw (§8): a trouble capture that cannot be written is lost, and listening goes on.
            }
        }

        if (_threaded)
        {
            _ = Task.Run(Write);
        }
        else
        {
            Write();
        }
    }

    private static string TroubleSheet(TroubleFired fired, MonoAudio audio, long seen, string extra)
    {
        var lines = new List<string>
        {
            "captured   automatically, trouble (work instruction 549)",
            $"trigger    {fired.Detail}",
            $"fired      {fired.AtUtc:yyyy-MM-dd HH:mm:ss} UTC",
            string.Format(
                CultureInfo.InvariantCulture,
                "seconds    {0:0.0}  (the audio before the trigger, from Hamlet's own five-minute ring, whatever Record is set to; less where it had not yet heard five minutes)",
                audio.Duration.TotalSeconds),
            $"sampleRate {audio.SampleRate}",
            $"audioSeen  {seen} samples  (the last of them is the moment it fired)",
            "cooldown   this trigger waits ten minutes before it saves again",
        };

        if (extra.Length > 0)
        {
            lines.Add(string.Empty);
            lines.Add(extra);
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    /// <summary>End any capture under way, as listening stopping does.</summary>
    public void Dispose()
    {
        Listen(null);

        (ContinuousCapture Capture, CaptureFeed? Feed, string Why)? ending = null;

        lock (_gate)
        {
            if (_w1aw is not null)
            {
                ending = TakeLocked("listening stopped");
            }
        }

        Finish(ending);
    }

    /// <summary>A length of time as the line shows it: 12:40, or 1:02:05 past an hour.</summary>
    /// <param name="elapsed">The length.</param>
    /// <returns>The text.</returns>
    public static string Elapsed(TimeSpan elapsed)
    {
        var whole = TimeSpan.FromSeconds(Math.Max(0, Math.Floor(elapsed.TotalSeconds)));

        return whole.TotalHours >= 1
            ? whole.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture)
            : whole.ToString(@"m\:ss", CultureInfo.InvariantCulture);
    }

    private void StartLocked(W1awScheduleState session, W1awMorseRow row, double toleranceHz, DateTime at)
    {
        var folder = Path.Combine(AutoFolder, $"w1aw-{at:yyyy-MM-dd-HHmmss}");
        var capture = new ContinuousCapture(folder, _clock, piece => W1awSheet(piece, session, row, toleranceHz));

        _w1aw = capture;
        _session = session;
        _row = row;
        Volatile.Write(ref _writing, new Writing(capture, _threaded ? new CaptureFeed(capture, _source?.SampleRate ?? 48_000) : null));
    }

    // Takes the capture away from the callback first, so nothing more is offered to it, and hands it back to be finished.
    private (ContinuousCapture Capture, CaptureFeed? Feed, string Why) TakeLocked(string why)
    {
        var writing = _writing;

        Volatile.Write(ref _writing, null);
        _w1aw = null;
        _session = null;
        _row = null;

        return (writing!.Capture, writing.Feed, why);
    }

    // **THE QUEUE IS WRITTEN OUT BEFORE THE LAST PIECE IS CLOSED**, so nothing heard before the end is lost; on the app's
    // own thread that wait would hold the window, so it is done off it.
    private static void Finish((ContinuousCapture Capture, CaptureFeed? Feed, string Why)? ending)
    {
        if (ending is not { } e)
        {
            return;
        }

        if (e.Feed is null)
        {
            e.Capture.End(e.Why);
            return;
        }

        _ = Task.Run(() =>
        {
            e.Feed.Stop();
            e.Capture.End(e.Why);
        });
    }

    private void OnSamples(in AudioChunk chunk) => Hear(chunk.FirstSampleIndex, chunk.SampleRate, chunk.Samples);

    private string W1awSheet(CapturePiece piece, W1awScheduleState session, W1awMorseRow row, double toleranceHz)
    {
        var lines = new List<string>
        {
            "captured   automatically, a W1AW session (work instruction 549)",
            $"session    {W1awSessionWindow.Entry(_table, session)}",
            $"speeds     {_table.Speeds}  (the schedule's own words)",
            string.Format(
                CultureInfo.InvariantCulture,
                "w1aw       {0} Hz on {1}  (the dial within {2:0} Hz of it: half the radio's filter, or 250 Hz where unread)",
                row.FrequencyHz,
                row.Band,
                toleranceHz),
            string.Format(
                CultureInfo.InvariantCulture,
                "window     {0:HH:mm:ss} to {1:HH:mm:ss} UTC  (a minute before the scheduled start to two minutes after the scheduled end)",
                session.StartUtc - W1awSessionWindow.Before,
                session.EndUtc + W1awSessionWindow.After),
            $"piece      {piece.Number}",
            $"started    {piece.StartUtc:yyyy-MM-dd HH:mm:ss} UTC",
            $"closed     {piece.EndUtc:yyyy-MM-dd HH:mm:ss} UTC",
            string.Format(CultureInfo.InvariantCulture, "seconds    {0:0.0}", piece.Seconds),
            $"sampleRate {piece.SampleRate}",
            string.Format(
                CultureInfo.InvariantCulture,
                "samples    {0} to {1} on the audio clock  (the next piece begins at {1}, so nothing falls between them)",
                piece.FirstSampleIndex,
                piece.FirstSampleIndex + piece.Samples + piece.MissingSamples),
            $"missing    {piece.MissingSamples} samples  (audio the capture never received inside this piece; not filled)",
        };

        lines.AddRange(piece.Notes);
        lines.Add(piece.EndReason is null
            ? $"continues  piece-{piece.Number + 1:00}.wav"
            : $"ended      {piece.EndReason}");

        if (SheetExtra?.Invoke() is { Length: > 0 } extra)
        {
            lines.Add(string.Empty);
            lines.Add(extra);
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}
