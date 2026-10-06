using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE CHAIN AND NOTHING ELSE** (work instruction 545, HM-DEC-249): the detector's marks read through the sender's gate
/// and the lookup table into the transcript, with the tap that keeps what was heard, and the rule that Hamlet stops
/// reading while the operator is sending (HM-DEC-147).
/// </summary>
/// <remarks>
/// <para>**THE OLD DECODER CAME OUT.** The probabilistic lattice and its stream, the unit estimator, the tone tracker and
/// its survey, the keying meter, the fldigi second reader with its arbiter and vote tables, and the competitor ran on every
/// hop and decided nothing that reached the screen: every letter had come from the shape side since work instruction 490.
/// The owner held its removal until W1AW's sanity check of 2026-10-05, which read whole sentences at 10, 13 and 15 WPM.
/// The tag <c>before-old-decoder-removal</c> holds it.</para>
/// <para>**WHAT IS LEFT**: the detector (<see cref="CwEnvelopeDetector"/>) owns the hop and calls the marks;
/// <see cref="CwChain.Wire"/> hands them here through <see cref="DetectorMarks"/>; the gate
/// (<see cref="CwSenderGate"/>) holds the senders, prints the best-shaped one and labels its marks; the lookup table
/// (<see cref="CwRunReader"/>) spells the letters, which this raises as settled characters.</para>
/// </remarks>
public sealed class CwDecoder
{
    private IAudioSource? _attached;

    private int _charactersEmitted;
    private int _charactersUnsure;
    private int _elementsResolved;

    private bool _transmitting;
    private DateTime _transmitEndedUtc = DateTime.MinValue;
    private long _suspendedChunks;

    private long _samplesHeard;

    private readonly CwSenderGate _runs = new();

    // The last mark sequence number taken from the detector.
    private long _markSequence;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    public CwDecoder(int sampleRate)
        : this(sampleRate, AudioTap.SecondsKept)
    {
    }

    /// <summary>Creates the decoder with a tap that keeps a stated length of audio for Record (work instruction 548, task 3).</summary>
    /// <param name="sampleRate">The audio's sample rate.</param>
    /// <param name="tapSeconds">How many seconds the tap keeps: thirty by default, up to five minutes.</param>
    public CwDecoder(int sampleRate, int tapSeconds)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        Tap = new AudioTap(tapSeconds);

        // What the gate reads is what reaches the screen (work instruction 490), and what the decoder decoded: the app
        // times its quiet offer and the evidence that the operator is working Morse (HM-DEC-149) from it (work
        // instruction 493).
        _runs.CharacterRead += c =>
        {
            Emit(c);

            if (!c.IsWordGap)
            {
                CharacterDecoded?.Invoke(c);
            }
        };
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>A character as it is read.</summary>
    public event Action<CwCharacter>? CharacterDecoded;

    /// <summary>A character that is final and will not be revised.</summary>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>
    /// The last half minute of exactly what the decoder was fed (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// **THE TAP IS HERE RATHER THAN AT THE SOUND CARD** so that what a capture contains is what the decoder received,
    /// not what something upstream believes it sent.
    /// </remarks>
    public AudioTap Tap { get; }

    /// <summary>How much audio has been handed to the decoder, on the clock <see cref="CwCharacter.At"/> uses.</summary>
    /// <remarks>
    /// **SO A SETTLED CHARACTER CAN BE PUT OVER THE BARS THAT MADE IT** (work instruction 480 task 3): the training graph
    /// reads this when a character settles, and the character's end is that far behind now.
    /// </remarks>
    public TimeSpan Heard => TimeSpan.FromSeconds(Interlocked.Read(ref _samplesHeard) / (double)SampleRate);

    /// <summary>
    /// The detector's marks called since a sequence number, with its audio clock; null where no detector is wired, and
    /// then nothing is read.
    /// </summary>
    /// <remarks>
    /// **A CHARACTER IS A RUN OF MARKS THAT AGREE** (work instruction 490, R103, HM-DEC-195): what reaches the terminal
    /// and the scope is what <see cref="CwSenderGate"/> reads from the marks, pitch, level and length together.
    /// </remarks>
    public Func<long, CwMarkBatch>? DetectorMarks { get; set; }

    /// <summary>The gate and reader the decoder prints from, for listeners built on <see cref="CwChain"/> that need its letters, runs and senders (work instruction 542).</summary>
    internal CwSenderGate Runs => _runs;

    /// <summary>
    /// The pitch of the sender the gate is printing, or NaN when it prints nobody (work instruction 519): what the
    /// detector's reading follows and the scope draws at. Safe to read from any thread.
    /// </summary>
    public double RunsPrintingHz => _runs.StationPitchHz;

    /// <summary>
    /// The pitch of the sender the gate has qualified and is waiting out its first word gap to print, or NaN (work
    /// instruction 535): with <see cref="RunsPrintingHz"/>, all the hold-still light may claim. Safe to read from any thread.
    /// </summary>
    public double RunsWaitingHz => _runs.WaitingPitchHz;

    /// <summary>
    /// What the shape side held at the end of its last batch (work instruction 537): the printed sender, its letters and
    /// marks, and every sender the gate holds. Safe to read from any thread.
    /// </summary>
    public CwShapeSideReading ShapeSide => _runs.ShapeReading;

    /// <summary>What is arriving, and what the shape side makes of it (HM-DEC-088, work instruction 545).</summary>
    public CwDecodeReport Report
    {
        get
        {
            // The printed sender, the senders the gate holds, the counts of what reached the screen; the level is the
            // tap's, which is what arrives whatever is read.
            var side = ShapeSide;

            return new CwDecodeReport(
                Tap.Level,
                side.PrintedPitchHz,
                side.Senders.Count,
                _elementsResolved,
                _charactersEmitted,
                _charactersUnsure,
                Printing: double.IsFinite(side.PrintedPitchHz),
                SpeedProof: SpeedProof,
                WordsPerMinute: WordsPerMinute);
        }
    }

    /// <summary>The slowest speed anybody would call a speed.</summary>
    public const int SlowestPlausibleWpm = 6;

    /// <summary>The fastest the radio's own keyer sends.</summary>
    public const int FastestPlausibleWpm = 48;

    /// <summary>
    /// True while the gate has a sender qualified and waiting to print, and nobody printed: the speed is being worked out
    /// (work instruction 545).
    /// </summary>
    /// <remarks>
    /// **FROM THE SHAPE SIDE.** It used to say the old decoder's window still held the station before this one; the shape
    /// side has no window, and what it can say is that a sender has shown enough to qualify and is waiting out its first
    /// word gap. A surface showing the speed says so rather than going blank (§0.0).
    /// </remarks>
    public bool SpeedIsReacquiring
        => DetectorMarks is not null && double.IsNaN(_runs.StationPitchHz) && double.IsFinite(_runs.WaitingPitchHz);

    /// <summary>
    /// The sending speed, or null when nothing has earned the right to name one.
    /// </summary>
    /// <remarks>
    /// <para>**ONE GUARDED ANSWER, READ BY EVERY SURFACE** (HM-DEC-090): the speed on the terminal's header and the speed
    /// the transmit panel offers both read this.</para>
    /// <para>**THE PRINTED SENDER'S OWN DIT** (work instruction 545): 1.2 seconds over the dit the gate measures on the
    /// sender it prints, the standard PARIS word of fifty dits. Null where nobody is printed, or the number falls outside
    /// what anybody sends.</para>
    /// </remarks>
    public int? WordsPerMinute
    {
        get
        {
            var side = ShapeSide;

            if (!double.IsFinite(side.PrintedPitchHz) || !(side.PrintedDitSeconds > 0))
            {
                return null;
            }

            var wpm = (int)Math.Round(1.2 / side.PrintedDitSeconds);

            return wpm >= SlowestPlausibleWpm && wpm <= FastestPlausibleWpm
                ? wpm
                : null;
        }
    }

    /// <summary>What can be said about the speed now (HM-REQ-034).</summary>
    /// <remarks>
    /// **PROVED WHERE <see cref="WordsPerMinute"/> NAMES A NUMBER** (work instruction 545): the gate prints a sender only
    /// while its two kinds of mark hold over its last ten and its gaps fall into kinds, and lets it go when it falls
    /// silent, so a dit it prints from is measured on keying still arriving. None otherwise.
    /// </remarks>
    public CwSpeedProof SpeedProof => WordsPerMinute is null ? CwSpeedProof.None : CwSpeedProof.Proved;

    /// <summary>
    /// How long decoding stays suspended after the radio stops transmitting.
    /// </summary>
    /// <remarks>
    /// <para>**HALF A SECOND, AND THE EVIDENCE FOR IT IS THE POLL AND NOT THE KEYING** (HM-DEC-147). Transmit status is a
    /// live field, asked for four times a second, so the state Hamlet holds can be a quarter of a second old before the
    /// reply is parsed. Full break-in switches between elements, which is tens of milliseconds: **the poll cannot see
    /// that at all**.</para>
    /// <para>**IT IS ASYMMETRIC ON PURPOSE.** Suspension is immediate, because a late suspension puts the operator's own
    /// sending on the screen as somebody else's; resumption waits, because an early one does the same with the tail of
    /// it.</para>
    /// </remarks>
    public static TimeSpan ResumeAfter { get; } = TimeSpan.FromMilliseconds(500);

    /// <summary>True while the radio is transmitting or has just stopped.</summary>
    public bool DecodingSuspended { get; private set; }

    /// <summary>
    /// True while the operator is in a Digital mode, so no CW is decoded.
    /// </summary>
    /// <remarks>
    /// In Digital the CW decode is arithmetic nobody reads; the audio still reaches the tap and the marks are dropped. It
    /// is set from the mode, never inferred from the audio.
    /// </remarks>
    public bool DigitalMode { get; set; }

    /// <summary>How many chunks of audio were dropped rather than decoded.</summary>
    public long SuspendedChunks => _suspendedChunks;

    /// <summary>
    /// Listen to a source. Replaces any previous one.
    /// </summary>
    /// <param name="source">The source, or null to stop listening.</param>
    /// <exception cref="ArgumentException">The source runs at a different rate.</exception>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        if (source is not null && source.SampleRate != SampleRate)
        {
            throw new ArgumentException(
                $"the decoder was built for {SampleRate} Hz and this source runs at "
                + $"{source.SampleRate} Hz",
                nameof(source));
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Feed samples directly, without a source.</summary>
    /// <param name="chunk">The samples.</param>
    public void Process(in AudioChunk chunk) => Process(chunk, tap: true);

    /// <summary>Feed samples, tapping them or not.</summary>
    /// <param name="chunk">The samples.</param>
    /// <param name="tap">
    /// False where the tap was fed already, on the capture's own thread, and this is the decode catching up behind a queue
    /// (work instruction 548): a sample tapped twice would double what Record and FT8 read.
    /// </param>
    public void Process(in AudioChunk chunk, bool tap)
    {
        // **THE AUDIO CLOCK A SETTLED CHARACTER'S `At` IS READ ON** (work instruction 480 task 3): every sample handed
        // here, suspended or not.
        Interlocked.Add(ref _samplesHeard, chunk.Samples.Length);

        // **THE TAP STILL TAKES IT.** A capture is the raw evidence of what arrived at the sound card, and audio the
        // operator made himself is part of that (§0.0.1). What it does not do is reach the gate.
        if (tap)
        {
            Tap.Take(chunk.Samples, chunk.SampleRate);
        }

        // **THE RUNS, FROM THE DETECTOR'S MARKS** (work instruction 490). While the radio is sending or a digital mode is
        // up the marks are taken and dropped, never read, held or released later (HM-DEC-147).
        var drop = DecodingSuspended || DigitalMode;

        if (drop)
        {
            _suspendedChunks++;
        }

        PullRuns(drop);
    }

    /// <summary>
    /// Finish: end every run and print what it read, because nothing more is coming.
    /// </summary>
    public void Flush()
    {
        PullRuns(drop: DecodingSuspended || DigitalMode);
        _runs.Flush();
    }

    /// <summary>
    /// Tell the decoder what the radio says about its own transmitter.
    /// </summary>
    /// <param name="transmitting">
    /// True when the radio reports the transmitter keyed, false when it reports it not, and null when nobody knows.
    /// </param>
    /// <param name="nowUtc">The clock.</param>
    /// <remarks>
    /// <para>**THE RADIO SAYS SO AND THE AUDIO NEVER DOES** (HM-DEC-091, HM-DEC-147). Not the level, not the sidetone's
    /// pitch, not a change in the noise floor: each of those is a guess about the transmitter made from the thing the
    /// transmitter is drowning out.</para>
    /// <para>**AND NOT KNOWING IS NOT TRANSMITTING.** An unknown state leaves decoding running, because a decoder silenced
    /// by a link that has gone quiet is a band that reads as empty (§0.0).</para>
    /// </remarks>
    public void RadioIsTransmitting(bool? transmitting, DateTime nowUtc)
    {
        if (transmitting == true)
        {
            _transmitting = true;
            DecodingSuspended = true;
            return;
        }

        if (_transmitting)
        {
            _transmitting = false;
            _transmitEndedUtc = nowUtc;
        }

        DecodingSuspended = _transmitEndedUtc != DateTime.MinValue
            && nowUtc - _transmitEndedUtc < ResumeAfter;
    }

    /// <summary>Take the marks the detector called since last time; drop them while nothing may be decoded.</summary>
    private void PullRuns(bool drop)
    {
        if (DetectorMarks is not { } marks)
        {
            return;
        }

        var batch = marks(_markSequence);

        if (batch.Marks.Count > 0)
        {
            _markSequence = batch.Marks.Max(m => m.Sequence);
        }

        if (!drop)
        {
            _runs.Read(batch);
        }
    }

    // What the gate lets out, counted as it reaches the screen.
    private void Emit(CwCharacter c)
    {
        // **THE COUNTERS COUNT WHAT REACHED THE SCREEN** (HM-DEC-091).
        if (!c.IsWordGap)
        {
            _charactersEmitted++;

            if (c.IsUnreadable || c.Confidence != CwConfidence.High)
            {
                _charactersUnsure++;
            }

            _elementsResolved += Math.Max(1, c.Pattern.Length);
        }

        CharacterSettled?.Invoke(c);
    }

    private void OnSamples(in AudioChunk chunk) => Process(chunk);
}
