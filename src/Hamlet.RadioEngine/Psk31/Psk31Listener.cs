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
        "a channel is retired when its carrier is, and its carrier is retired when the "
        + "search stops finding the signal: the place it sits fails the candidate test on "
        + nameof(Psk31CarrierSearch.RetireAfterPasses) + " passes of the newest spectrum "
        + "window in a row, so a channel is gone within " + nameof(RetiredWithinSeconds)
        + " of its carrier stopping. **It is never retired for saying nothing.** Separately, "
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
    /// <para>**2.5, AND HERE IS WHERE IT COMES FROM, RESTATED FOR UNIT 324'S RULE.** The
    /// spectrum window is 2 048 samples, 0.26 s at 8 kHz, so a window stops holding a
    /// carrier that has stopped within that; the search then wants
    /// <see cref="Psk31CarrierSearch.RetireAfterPasses"/> passes in a row, which is 1.02 s;
    /// the verdict lands on the next pass, 0.13 s; and the shell hands audio over in lumps
    /// of a quarter of a second. That is about 1.7 s, and 2.5 is kept as the bound with
    /// margin rather than tightened to flatter it.</para>
    /// <para>**AFTER THE CARRIER STOPS, WHICH IS NOT ALWAYS AFTER THE LAST CHARACTER.** A
    /// PSK31 operator's transmitter idles after the last character until he unkeys - the
    /// fixtures idle 40 bits, 1.28 s - and a station on idle is still a station.</para>
    /// </remarks>
    public const double RetiredWithinSeconds = 2.5;

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
                pair.Value.Text.Length))
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

            _search.Add(slice);

            samples = samples[count..];
        }

        Reconcile();
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
                channel = new Channel(_sampleRate, carrier.OffsetHz);
                Replay(channel);
                _channels[carrier.Id] = channel;
            }

            channel.OffsetHz = carrier.OffsetHz;
            channel.StrengthDb = carrier.StrengthDb;
            channel.Commit(carrier.KeyedUntilSample - _letGo);
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

        public void Hear(ReadOnlySpan<float> samples, long endsAt)
        {
            foreach (var character in _demodulator.Add(samples))
            {
                _pending.Enqueue((character, endsAt));
            }
        }

        public void Commit(long upTo)
        {
            while (_pending.Count > 0 && _pending.Peek().At <= upTo)
            {
                _text.Append(_pending.Dequeue().Character);
            }
        }
    }
}
