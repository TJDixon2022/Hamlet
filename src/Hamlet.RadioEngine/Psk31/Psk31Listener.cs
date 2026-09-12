using System.Text;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>What one PSK31 channel has read, as of now.</summary>
/// <param name="Id">The carrier's id, the same for as long as it is listed.</param>
/// <param name="OffsetHz">Where the carrier sits in the passband now, in hertz.</param>
/// <param name="StrengthDb">Its strength, or NaN where not measured.</param>
/// <param name="Text">Every character the channel is sure of, in order.</param>
/// <param name="Readable">
/// True while this channel's squelch is open - while it is reading, rather than only
/// hearing.
/// </param>
/// <remarks>
/// **HEARD AND READ ARE DIFFERENT FACTS AND UNIT 324 MADE THE LISTENER SAY BOTH** (§0.0,
/// HM-DEC-092). A carrier is held because the search is sure it is a carrier; whether
/// anything can be made of it is the demodulator's separate answer, and until unit 324 the
/// only way out was an empty <see cref="Text"/>, which reads the same as *a station that
/// has not said anything yet*.
/// </remarks>
public sealed record Psk31Channel(
    int Id, double OffsetHz, double StrengthDb, string Text, bool Readable = false);

/// <summary>
/// **Every PSK31 signal in the passband, each with its own demodulator.**
/// </summary>
/// <remarks>
/// <para>**IT KNOWS NOTHING ABOUT TABS, ROWS OR RADIOS** (§0.1). Samples go in; one
/// channel per carrier comes out, with its offset, its strength and its text so far.
/// Whoever draws them is the shell's business.</para>
/// <para>**THE SEARCH SAYS WHERE; A <see cref="Psk31Demodulator"/> READS EACH ONE.** A
/// channel is made the moment <see cref="Psk31CarrierSearch"/> lists a carrier and
/// dropped the moment it stops listing it. A new channel is given the last
/// <see cref="ReplaySeconds"/> of audio first, because the search needs about a second
/// to be sure of a carrier and a station's first words are said in that second.</para>
/// <para>**AND IT SHOWS ONLY WHAT THE SEARCH STILL VOUCHES FOR** (§R9, §0.0). Unit 314's
/// squelch averages its measure weighted by energy, so when a strong carrier stops, the
/// last seconds of the carrier outweigh the noise that follows and the squelch stays
/// open on nothing - which is exactly the garbage the author's unsquelched reference
/// produced after each carrier stopped (`manifest-step2.json`: 0.125, 0.386, 0.183). So
/// a character is held until the search has measured the carrier still keyed
/// <see cref="LetGoSeconds"/> after it, and one that never gets that far is dropped and
/// counted in <see cref="DroppedCharacters"/>. That costs about a second of delay on
/// every character, and it is the price of the panel never showing text nobody sent.</para>
/// </remarks>
public sealed class Psk31Listener
{
    /// <summary>**The retire rule, in one place.**</summary>
    /// <remarks>
    /// **IT IS THE SIGNAL GOING AND NOT THE TEXT STOPPING, SINCE UNIT 324.** A station
    /// idling between words keys a steady carrier for seconds at a time and is not
    /// retired for it; a station Hamlet can hear and cannot read is held and says so.
    /// </remarks>
    public const string RetireRule =
        "a channel is retired when its carrier is, and its carrier is retired only when the "
        + "search and the channel's own demodulator have both lost it: the place it sits "
        + "fails the candidate test on "
        + nameof(Psk31CarrierSearch.RetireAfterPasses) + " passes of the newest spectrum "
        + "window in a row, AND the demodulator has had neither its squelch open nor its "
        + "quality at " + nameof(Psk31CarrierSearch.KeepReadableQuality) + " or better "
        + "within the last " + nameof(Psk31CarrierSearch.KeepReadableSeconds)
        + ", so a channel is gone within " + nameof(RetiredWithinSeconds)
        + " of its carrier stopping. **It is never retired for saying nothing** and never "
        + "for idling, which is what a PSK31 station does between words. Separately, "
        + "any character it read after the keying measure last stood behind the carrier, "
        + "less " + nameof(LetGoSeconds) + ", is held and is dropped rather than shown if "
        + "the carrier goes before that measure catches up.";

    /// <summary>How long the keying measure takes to let go, in seconds.</summary>
    /// <remarks>
    /// **ONE WHOLE MEASURE, 32 SYMBOLS, 1.024 S.** After keying stops the measure falls
    /// below <see cref="Psk31CarrierSearch.CoherenceToStay"/> about 29 symbols later; a
    /// whole measure is the margin over that.
    /// </remarks>
    public const double LetGoSeconds = Psk31CarrierSearch.MeasureSymbols / Psk31Demodulator.Baud;

    /// <summary>**How soon after a carrier stops its channel is gone, in seconds.**</summary>
    /// <remarks>
    /// <para>**9.0, AND HERE IS WHERE IT COMES FROM, RESTATED FOR UNIT 327'S RULE.** The
    /// spectrum window is 2 048 samples, 0.26 s at 8 kHz, so a window stops holding a
    /// carrier that has stopped within that; the search then wants
    /// <see cref="Psk31CarrierSearch.RetireAfterPasses"/> passes in a row, which is 1.02 s;
    /// the verdict lands on the next pass, 0.13 s; and the shell hands audio over in lumps
    /// of a quarter of a second. **That was 1.7 s and the bound was 2.5.**</para>
    /// <para>**WHAT UNIT 327 ADDED, AND WHAT IT COSTS - MEASURED, NOT ESTIMATED.** A carrier
    /// is now also kept while its demodulator vouches for it, and that verdict is a rolling
    /// mean over <see cref="Psk31Demodulator.QualityWindow"/> symbols **weighted by
    /// magnitude**: when a strong carrier stops, its own loud symbols hold the ratio up
    /// while they decay out of the window, and the louder it was the longer that takes.
    /// **On `psk31-idle-8s-1000hz.wav` the squelch shut 5.70 s after the carrier stopped and
    /// the quality fell under <see cref="Psk31CarrierSearch.KeepReadableQuality"/> at 7.20
    /// s**, and <see cref="Psk31CarrierSearch.KeepReadableSeconds"/> is waited after that -
    /// 8.22 s, so 9.0 is the bound with margin. It was 2.5.</para>
    /// <para>**THAT NUMBER IS A FINDING AND IT IS RAISED AS ONE** (§0.0, work instruction
    /// 327 section 4). Six seconds is a long time for a row to claim a station who has gone,
    /// and the cause is not this rule but the measure it leans on: `Quality` is documented
    /// as *0.637 on uniform noise phase*, and after a loud carrier stops the input **is**
    /// uniform noise phase while it goes on reporting 0.99. Normalizing that measure per
    /// symbol, or capping how long a vouch may outlive the spectrum, would both bring this
    /// back under three seconds. **Neither is done here**: the first changes what every
    /// PSK31 decode in the app is squelched on, and the second contradicts the rule this
    /// unit was told to build, which is that a carrier goes only when both have lost it.</para>
    /// <para>**AND THE TRADE IS DELIBERATE.** Six extra seconds of a row for a station who
    /// has left, against a station who pauses to think being retired and reborn under a new
    /// id - which is what the operator's 2026-09-12 record shows happening six times in two
    /// minutes to a signal rated 0.99 throughout. **The row says heard, not readable yet
    /// while it waits** (§0.0), so it is not claiming he is still talking.</para>
    /// <para>**AFTER THE CARRIER STOPS, WHICH IS NOT ALWAYS AFTER THE LAST CHARACTER.** A
    /// PSK31 operator's transmitter idles after the last character until he unkeys - the
    /// fixtures idle 40 bits, 1.28 s - and a station on idle is still a station.</para>
    /// </remarks>
    public const double RetiredWithinSeconds = 9.0;

    /// <summary>**How long without a character is idling, in seconds.**</summary>
    /// <remarks>
    /// <para>**TWO, AND IT IS A SENTENCE'S WORTH OF TYPING** (work instruction 327 task 1).
    /// A PSK31 character is eight to ten bits at 31.25 baud, about a third of a second, and
    /// a fluent operator's longest pause mid-sentence is well under a second. Two seconds is
    /// past every gap inside typing and short enough that the eight-second idle in the
    /// fixture is bracketed with six seconds to spare.</para>
    /// <para>**IT IS SAID ONLY OF A CARRIER THE DEMODULATOR IS STILL HAPPY WITH.** A
    /// channel that has gone unreadable is also producing no characters, and calling that
    /// *idling* would be Hamlet claiming to know he is sitting there thinking (§0.0). Idle
    /// is asserted where the squelch is open on continuous reversals, which is the thing a
    /// PSK31 transmitter actually sends between words.</para>
    /// </remarks>
    public const double IdleAfterSeconds = 2.0;

    /// <summary>**The ceiling this unit states for first-character latency, in seconds.**</summary>
    /// <remarks>
    /// <para>**2.5, AND IT IS TAKEN FROM THE MEASUREMENT AND NOT CHOSEN** (work instruction
    /// 327 task 2). Measured on `psk31-clean-1000hz.wav` and on `psk31-idle-8s-1000hz.wav`,
    /// the gap between a carrier being listed and its first character reaching the panel is
    /// **1.50 s on both**. 2.5 is that with two thirds again on top, so a machine slower than
    /// this one still passes and a path that grew another whole second of waiting does not.
    /// </para>
    /// <para>**AND HERE IS WHAT THE 1.50 s IS MADE OF**, largest first. The demodulator's
    /// squelch will not pass a character until it has measured
    /// <see cref="Psk31Demodulator.QualityWindow"/> symbols, which is **1.024 s and is two
    /// thirds of the whole number**. The bit-clock settle is *inside* that window rather than
    /// added to it - the timing profile's memory is about eight symbols, 0.26 s, and it runs
    /// while the squelch is still counting. The varicode separator wait is the smallest term:
    /// a character is not emitted until its `00` has arrived, which is **two symbols, 0.064
    /// s**. The rest is <see cref="LetGoSeconds"/> overlapping the replay, and the
    /// quarter-second lump the audio arrives in.</para>
    /// <para>**IT IS UNDER THREE SECONDS, SO NOTHING IS BUILT HERE TO BRING IT DOWN** (work
    /// instruction 327 task 2, and §R14). The one term worth anything would be the squelch
    /// window, and shortening it is a decode-quality decision the operator has not been asked
    /// about: a shorter window opens on weaker evidence, which is the prime directive's own
    /// trade and not a latency tuning.</para>
    /// <para>**WHAT IS NOT IN THIS NUMBER, SAID OUT LOUD** (§0.0). It is measured from the
    /// carrier being **listed**, which is what the operator sees appear. The search takes
    /// about a second more to be sure of a carrier before that, and
    /// <see cref="ReplaySeconds"/> of history is fed to a new channel precisely so those
    /// words are not lost - so the time from a station **starting to transmit** to his first
    /// character on the screen is longer than this, and is the number
    /// `ThePsk31DemodulatorTests` prints beside it.</para>
    /// </remarks>
    public const double FirstCharacterCeilingSeconds = 2.5;

    /// <summary>How much audio a new channel hears from before it was made, in seconds.</summary>
    /// <remarks>
    /// **THREE, WHICH COVERS THE SECOND OR SO THE SEARCH TAKES TO BE SURE AND THE SECOND
    /// THE DEMODULATOR'S OWN SQUELCH TAKES TO OPEN**, with room for a carrier that started
    /// in the middle of a spectrum window.
    /// </remarks>
    public const double ReplaySeconds = 3.0;

    private readonly int _sampleRate;
    private readonly int _piece;
    private readonly long _letGo;
    private readonly long _idle;
    private readonly Psk31CarrierSearch _search;
    private readonly float[] _history;
    private int _historyWrite;
    private int _historyFilled;
    private readonly Dictionary<int, Channel> _channels = new();
    private IReadOnlyList<Psk31Channel> _snapshot = Array.Empty<Psk31Channel>();

    /// <summary>Opens a listener over whatever passband the samples carry.</summary>
    /// <param name="sampleRate">Samples a second.</param>
    public Psk31Listener(int sampleRate)
    {
        _sampleRate = sampleRate;
        _search = new Psk31CarrierSearch(sampleRate);

        // **ONE SYMBOL AT A TIME**, so every character carries the moment it completed to
        // within a thirty-second of a second, whatever lumps the audio arrives in.
        _piece = (int)Math.Round(sampleRate / Psk31Demodulator.Baud);
        _letGo = (long)(LetGoSeconds * sampleRate);
        _idle = (long)(IdleAfterSeconds * sampleRate);
        _history = new float[(int)(ReplaySeconds * sampleRate)];
    }

    /// <summary>Every channel now, lowest offset first.</summary>
    public IReadOnlyList<Psk31Channel> Channels => _snapshot;

    /// <summary>What the search saw, for something outside to write down.</summary>
    /// <remarks>
    /// **THE SEARCH OWNS IT AND THIS HANDS IT ON** (§0.1, work instruction 322 task 2).
    /// The shell has the listener and not the search inside it, and a second watch here
    /// would be a second copy of the same facts.
    /// </remarks>
    public Psk31Watch Watch => _search.Watch;

    /// <summary>The bottom of the passband being searched, in hertz.</summary>
    public double LowestHz => _search.LowestHz;

    /// <summary>The top of it, in hertz.</summary>
    public double HighestHz => _search.HighestHz;

    /// <summary>How long a carrier may be missing before it is retired, in seconds.</summary>
    /// <remarks>
    /// **THE SEARCH OWNS THE RULE AND THIS HANDS IT ON**, the same way
    /// <see cref="Watch"/> does: the shell holds the listener and not the search, and a
    /// second copy of the number here would be a second place for it to disagree.
    /// </remarks>
    public double RetireAfterSeconds => _search.RetireAfterSeconds;

    /// <summary>What each held channel's demodulator is doing right now.</summary>
    /// <remarks>
    /// <para>**READINGS, NOT EVENTS.** The squelch opening and the AFC moving are states,
    /// and the thing that wants to write them down is the thing that knows when it last
    /// looked. Handing it the state and letting it notice the change keeps the engine
    /// free of any notion of *since when*.</para>
    /// <para>**AND A CHANNEL WITH NOTHING TO SAY IS STILL IN THE LIST**, because a
    /// squelch that has shut is exactly the fact somebody reading the record is looking
    /// for.</para>
    /// </remarks>
    public IReadOnlyList<Psk31ChannelState> States
        => _channels
            .Select(pair => new Psk31ChannelState(
                pair.Key,
                Math.Round(pair.Value.OffsetHz, 1),
                pair.Value.Open,
                Math.Round(pair.Value.Quality, 3),
                Math.Round(pair.Value.AfcHz, 1),
                pair.Value.Text.Length,
                Latency(pair.Value)))
            .OrderBy(state => state.OffsetHz)
            .ToList();

    /// <summary>How many samples the listener has been fed.</summary>
    public long SamplesSeen { get; private set; }

    /// <summary>The audio rate this listener was opened at.</summary>
    /// <remarks>
    /// **SO A CALLER CAN TURN SAMPLES INTO SECONDS WITHOUT A SECOND OPINION** (work
    /// instruction 322 task 5). Everything this listener counts is counted in samples,
    /// and a reader turning them into seconds with a rate from somewhere else is how two
    /// clocks get into one record.
    /// </remarks>
    public int SampleRate => _sampleRate;

    /// <summary>Characters a channel read that the search never vouched for, dropped.</summary>
    public long DroppedCharacters { get; private set; }

    /// <summary>Feed the listener some audio.</summary>
    /// <param name="samples">Samples in [-1, 1].</param>
    public void Add(ReadOnlySpan<float> samples)
    {
        while (samples.Length > 0)
        {
            var count = Math.Min(_piece, samples.Length);
            var slice = samples[..count];

            Remember(slice);
            SamplesSeen += count;

            foreach (var channel in _channels.Values)
            {
                channel.Hear(slice, SamplesSeen);
            }

            // **THE CHANNELS SPEAK FIRST, AND THE SEARCH JUDGES AFTER THEM** (work
            // instruction 327 task 1). The verdict handed over is the one measured on the
            // audio the search is about to look at, not the one from a symbol ago, so the
            // two halves of the retire rule are asked about the same moment.
            foreach (var pair in _channels)
            {
                _search.Vouch(pair.Key, pair.Value.Open, pair.Value.Quality);
            }

            _search.Add(slice);

            NoteIdling();

            samples = samples[count..];
        }

        Reconcile();
    }

    /// <summary>How long this channel took to say its first word, in seconds.</summary>
    /// <param name="channel">The held channel.</param>
    /// <returns>The gap from the carrier being listed to the first character, or NaN.</returns>
    /// <remarks>
    /// **FROM THE CHANNEL BEING MADE, WHICH IS THE MOMENT THE CARRIER WAS LISTED**, and so
    /// the moment the row appeared. It is the operator's own question - *the row has been
    /// there for a while and it is still empty* - and not a decoder's internal one.
    /// </remarks>
    private double Latency(Channel channel)
        => channel.FirstShownAt > 0
            ? Math.Round((double)(channel.FirstShownAt - channel.SinceMadeAt) / _sampleRate, 3)
            : double.NaN;

    /// <summary>
    /// **Notice a held station stopping typing, and starting again.**
    /// </summary>
    /// <remarks>
    /// <para>**SO THE RECORD SHOWS AN IDLE GAP AS AN IDLE GAP** (work instruction 327 task
    /// 1, §0.0). Characters, then no characters, then characters is what a station who
    /// paused and a station who went off the air and came back both look like in a file of
    /// decoded text. They are two different evenings, and until these two events existed
    /// the operator had no way to tell them apart in his own record.</para>
    /// <para>**IT IS SAID OF A CARRIER THAT HAS ACTUALLY SPOKEN.** A channel that has never
    /// emitted a character is *heard, not readable yet* - a state it already has a name for
    /// - and calling that idling would be a claim about a person Hamlet has not heard from
    /// (§0.0). And it is said only while the squelch is open, because open on no characters
    /// is exactly what continuous reversals are.</para>
    /// </remarks>
    private void NoteIdling()
    {
        foreach (var pair in _channels)
        {
            var channel = pair.Value;

            if (channel.LastCharacterAt <= 0)
            {
                continue;
            }

            var quiet = SamplesSeen - channel.LastCharacterAt;

            if (!channel.Idling && channel.Open && quiet >= _idle)
            {
                channel.Idling = true;

                Watch.Add(new Psk31CarrierActivity(
                    pair.Key,
                    Idling: true,
                    Math.Round(channel.OffsetHz, 1),
                    Math.Round(channel.Quality, 3),
                    Math.Round((double)(SamplesSeen - channel.SinceAt) / _sampleRate, 1)));

                channel.SinceAt = SamplesSeen;
            }
            else if (channel.Idling && quiet == 0)
            {
                channel.Idling = false;

                Watch.Add(new Psk31CarrierActivity(
                    pair.Key,
                    Idling: false,
                    Math.Round(channel.OffsetHz, 1),
                    Math.Round(channel.Quality, 3),
                    Math.Round((double)(SamplesSeen - channel.SinceAt) / _sampleRate, 1)));

                channel.SinceAt = SamplesSeen;
            }
        }
    }

    /// <summary>Make a channel for every new carrier and drop the ones that went.</summary>
    private void Reconcile()
    {
        var listed = new HashSet<int>();

        foreach (var carrier in _search.Carriers)
        {
            listed.Add(carrier.Id);

            if (!_channels.TryGetValue(carrier.Id, out var channel))
            {
                channel = new Channel(_sampleRate, carrier.OffsetHz)
                {
                    SinceAt = SamplesSeen,
                    SinceMadeAt = SamplesSeen,
                };
                Replay(channel);
                _channels[carrier.Id] = channel;
            }

            channel.OffsetHz = carrier.OffsetHz;
            channel.StrengthDb = carrier.StrengthDb;
            channel.Commit(carrier.KeyedUntilSample - _letGo, SamplesSeen);
        }

        foreach (var id in _channels.Keys.Where(id => !listed.Contains(id)).ToList())
        {
            // **WHAT IT READ AFTER THE LAST VOUCHED MOMENT GOES WITH IT.** The last commit
            // above already took everything the search stood behind.
            DroppedCharacters += _channels[id].Pending;
            _channels.Remove(id);
        }

        _snapshot = _channels
            .Select(pair => new Psk31Channel(
                pair.Key,
                pair.Value.OffsetHz,
                pair.Value.StrengthDb,
                pair.Value.Text,
                pair.Value.Open))
            .OrderBy(c => c.OffsetHz)
            .ToList();
    }

    private void Remember(ReadOnlySpan<float> samples)
    {
        foreach (var sample in samples)
        {
            _history[_historyWrite] = sample;
            _historyWrite = (_historyWrite + 1) % _history.Length;

            if (_historyFilled < _history.Length)
            {
                _historyFilled++;
            }
        }
    }

    /// <summary>Give a new channel the recent audio, oldest first, with its moments.</summary>
    private void Replay(Channel channel)
    {
        var start = (_historyWrite - _historyFilled + _history.Length) % _history.Length;
        var at = SamplesSeen - _historyFilled;
        var piece = new float[_piece];

        for (var done = 0; done < _historyFilled;)
        {
            var count = Math.Min(_piece, _historyFilled - done);

            for (var i = 0; i < count; i++)
            {
                piece[i] = _history[(start + done + i) % _history.Length];
            }

            done += count;
            at += count;

            channel.Hear(piece.AsSpan(0, count), at);
        }
    }

    /// <summary>One demodulator, and the characters it has read but not yet shown.</summary>
    private sealed class Channel
    {
        private readonly Psk31Demodulator _demodulator;
        private readonly Queue<(char Character, long At)> _pending = new();
        private readonly StringBuilder _text = new();

        public Channel(int sampleRate, double offsetHz)
        {
            _demodulator = new Psk31Demodulator(sampleRate, offsetHz);
            OffsetHz = offsetHz;
        }

        public double OffsetHz { get; set; }

        public double StrengthDb { get; set; } = double.NaN;

        /// <summary>True while this channel's squelch is letting characters through.</summary>
        public bool Open => _demodulator.IsOpen;

        /// <summary>How BPSK-shaped its last few symbols were.</summary>
        public double Quality => _demodulator.Quality;

        /// <summary>How far its AFC has pulled from where it started listening.</summary>
        public double AfcHz => _demodulator.TrackedHz - OffsetHz;

        public string Text => _text.ToString();

        public int Pending => _pending.Count;

        /// <summary>The sample count when this channel last emitted a character.</summary>
        /// <remarks>
        /// **ZERO MEANS IT HAS NEVER SAID ANYTHING**, which is a different fact from
        /// *it said something a long time ago* and is never reported as idling (§0.0).
        /// </remarks>
        public long LastCharacterAt { get; private set; }

        /// <summary>True while he is holding the carrier and not typing.</summary>
        public bool Idling { get; set; }

        /// <summary>The sample count when the state it is in now began.</summary>
        public long SinceAt { get; set; }

        /// <summary>The sample count when this channel was made, which is when it was listed.</summary>
        public long SinceMadeAt { get; set; }

        /// <summary>The sample count when its first character reached the panel.</summary>
        /// <remarks>
        /// **WHEN IT WAS *SHOWN*, NOT WHEN IT WAS READ.** A character is held until the
        /// search has vouched for the carrier after it, so the moment the demodulator
        /// emitted it is not the moment the operator could see it, and the second one is
        /// the one he is asking about.
        /// </remarks>
        public long FirstShownAt { get; private set; }

        public void Hear(ReadOnlySpan<float> samples, long endsAt)
        {
            foreach (var character in _demodulator.Add(samples))
            {
                _pending.Enqueue((character, endsAt));

                LastCharacterAt = endsAt;
            }
        }

        public void Commit(long upTo, long now)
        {
            while (_pending.Count > 0 && _pending.Peek().At <= upTo)
            {
                _text.Append(_pending.Dequeue().Character);

                if (FirstShownAt == 0)
                {
                    FirstShownAt = now;
                }
            }
        }
    }
}
