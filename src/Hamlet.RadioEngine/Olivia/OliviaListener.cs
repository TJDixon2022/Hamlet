using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>What one Olivia channel has read, as of now.</summary>
/// <param name="Id">The station's id, the same for as long as it is listed.</param>
/// <param name="Variant">The variant it is read at.</param>
/// <param name="CenterHz">Where it sits now: the center it was found at, moved by the offset track.</param>
/// <param name="Found">How it was found: <see cref="OliviaListener.FoundByRsid"/> or <see cref="OliviaListener.FoundBlind"/>.</param>
/// <param name="OpenedSeconds">The audio seconds, from the listener's first sample, at which it was opened.</param>
/// <param name="Text">Every character of every block it was sure of, in order.</param>
/// <param name="BlocksDecoded">Blocks shown.</param>
/// <param name="BlocksRejected">Blocks read and not shown.</param>
/// <param name="Ended">True where the station announced a different variant at the same place, and this channel stopped reading.</param>
/// <param name="ShownCenterHz">
/// Where it sat when its last accepted block was read - the center a row shows - or NaN before any
/// block was accepted (work instruction 364 decision AG). <paramref name="CenterHz"/> keeps tracking
/// after the station stops; this does not.
/// </param>
/// <param name="Retired">
/// True where it ended because its station went quiet for longer than its variant's retire window
/// (work instruction 364 decision AJ), rather than by announcing a different variant.
/// </param>
public sealed record OliviaChannel(
    int Id,
    string Variant,
    double CenterHz,
    string Found,
    double OpenedSeconds,
    string Text,
    int BlocksDecoded,
    int BlocksRejected,
    bool Ended,
    double ShownCenterHz = double.NaN,
    bool Retired = false);

/// <summary>What one channel's reader is doing, for something outside to write down.</summary>
/// <param name="Id">The channel.</param>
/// <param name="Variant">Its variant.</param>
/// <param name="CenterHz">Where it sits now.</param>
/// <param name="Found">How it was found.</param>
/// <param name="InSync">True while its last block read was shown.</param>
/// <param name="LastBlockSnr">Its last block's signal-to-noise.</param>
/// <param name="SyncSnr">The mean of its shown blocks' signal-to-noise.</param>
/// <param name="BlocksDecoded">Blocks shown.</param>
/// <param name="BlocksRejected">Blocks read and not shown.</param>
/// <param name="Characters">How many characters it has shown - the count, never the text.</param>
public readonly record struct OliviaChannelState(
    int Id,
    string Variant,
    double CenterHz,
    string Found,
    bool InSync,
    double LastBlockSnr,
    double SyncSnr,
    int BlocksDecoded,
    int BlocksRejected,
    int Characters);

/// <summary>
/// **Every Olivia station in the passband, each with its own reader** - the engine's one Olivia
/// receiver, shaped like <see cref="Psk31.Psk31Listener"/>.
/// </summary>
/// <remarks>
/// <para>**IT KNOWS NOTHING ABOUT TABS, ROWS OR RADIOS** (§0.1, work instruction 363 decision Y).
/// Samples go in as they arrive; one channel per station comes out, with its variant, its center,
/// how it was found and its text so far.</para>
/// <para>**TWO WAYS IN** (`PHASE_PLAN.md` R27). The streaming <see cref="RsidDetector"/> hears every
/// announcement across the passband, and a station that names an Olivia variant gets a channel at
/// the variant and center it named, read from the burst's end. A carrier that never announced
/// itself is found by <see cref="OliviaSearchStream"/>, and its channel keeps the reader that
/// confirmed it. **The listener is never told where a station is.**</para>
/// <para>**A NEW CHANNEL LOSES NOTHING** (decision Y): the detector hands a burst back about a burst
/// after it ends, and the search names a carrier after two blocks at least, so the listener keeps
/// <see cref="ReplaySeconds"/> of audio - <see cref="OliviaSearchStream.ReplayBlocks"/> of the
/// slowest row's blocks, not a figure in seconds - and a new reader is handed it from the burst's
/// end, or from where the search's readers began.</para>
/// <para>**ONE STATION, ONE CHANNEL** (decision AA). A place is the same place within half the
/// narrower of the two variants' bandwidths. An announcement at a place already read at the same
/// variant opens nothing, and turns a blind-found channel into an announced one; at a different
/// variant it ends the old channel and opens a new one. The search is never let open a place
/// already read. **A channel is retired for going quiet** once its variant's retire window has
/// passed since its last accepted block, where the listener is given a timing table (work
/// instruction 364 decision AJ); without one, nothing retires.</para>
/// <para>**TEXT ONLY AS BLOCKS ARE SHOWN** (§3.3, §R9): a channel's text is its reader's, which
/// holds nothing but the characters of blocks that cleared the threshold.</para>
/// <para>**ITS EVENTS CARRY NO TEXT AND NO CALLSIGN** (decision AD, HM-DEC-018): `olivia_channel`
/// when a channel opens, is announced or ends, and `olivia_block` for every block a channel reads,
/// with counts and figures only.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2): it keys nothing, tunes nothing and sets no mode.</para>
/// </remarks>
public sealed class OliviaListener
{
    /// <summary>How a channel found by its own announcement is marked.</summary>
    public const string FoundByRsid = "rsid";

    /// <summary>How a channel found by the blind search is marked.</summary>
    public const string FoundBlind = "blind";

    private readonly OliviaFormat _format;
    private readonly RsidCodes _codes;
    private readonly int _rate;
    private readonly ITelemetry? _telemetry;
    private readonly OliviaTiming? _timing;
    private readonly RsidDetector _rsid;
    private readonly OliviaSearchStream _search;
    private readonly int _keep;
    private readonly List<Channel> _channels = new();
    private IReadOnlyList<OliviaChannel> _snapshot = Array.Empty<OliviaChannel>();

    private float[] _history;
    private long _historyStart;
    private int _historyCount;
    private int _nextId = 1;

    /// <summary>Opens a listener over a passband.</summary>
    /// <param name="format">The format's facts.</param>
    /// <param name="codes">The RSID codes and tone sequences.</param>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="lowestHz">The passband's low edge.</param>
    /// <param name="highestHz">The passband's high edge.</param>
    /// <param name="telemetry">Where the channel and block events are written, or null.</param>
    /// <param name="timing">The timing table the retire window is read from, or null where nothing retires.</param>
    public OliviaListener(OliviaFormat format, RsidCodes codes, int sampleRate, double lowestHz, double highestHz, ITelemetry? telemetry = null, OliviaTiming? timing = null)
    {
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(codes);

        _format = format;
        _codes = codes;
        _rate = sampleRate;
        _telemetry = telemetry;
        _timing = timing;
        _rsid = new RsidDetector(codes, sampleRate, lowestHz, highestHz);
        _search = new OliviaSearchStream(format, sampleRate, lowestHz, highestHz);
        _keep = (int)Math.Ceiling(_search.ReplaySeconds * sampleRate);
        _history = new float[_keep * 2];
    }

    /// <summary>Every channel now, lowest center first.</summary>
    public IReadOnlyList<OliviaChannel> Channels => _snapshot;

    /// <summary>What each channel's reader is doing right now.</summary>
    public IReadOnlyList<OliviaChannelState> States
        => _channels
            .Select(c => new OliviaChannelState(
                c.Id,
                c.Stream.Variant.Name,
                Math.Round(c.CenterHz, 1),
                c.Found,
                c.Stream.InSync,
                Math.Round(c.Stream.LastBlockSnr, 2),
                Math.Round(c.Stream.SyncSnr, 2),
                c.Stream.BlocksDecoded,
                c.Stream.BlocksRejected,
                c.Stream.CharactersOut))
            .OrderBy(s => s.CenterHz)
            .ToList();

    /// <summary>How much audio a new channel is handed from before it was opened, in seconds.</summary>
    public double ReplaySeconds => _search.ReplaySeconds;

    /// <summary>How many samples the listener has been fed.</summary>
    public long SamplesSeen { get; private set; }

    /// <summary>The audio rate this listener was opened at.</summary>
    public int SampleRate => _rate;

    /// <summary>Feed the listener some audio.</summary>
    /// <param name="samples">Samples in [-1, 1], at <see cref="SampleRate"/>.</param>
    public void Add(ReadOnlySpan<float> samples)
    {
        Remember(samples);
        SamplesSeen += samples.Length;

        foreach (var channel in _channels.Where(c => !c.Ended))
        {
            channel.Stream.Add(samples);
        }

        foreach (var heard in _rsid.Feed(samples))
        {
            Announced(heard);
        }

        foreach (var found in _search.Add(samples, Occupied()))
        {
            Open(found.Stream, FoundBlind, found.StartSample);
        }

        Report();

        if (RetireTheQuiet())
        {
            Report();
        }
    }

    /// <summary>
    /// **Retire every channel whose station has been quiet longer than its variant's window**
    /// (step 3 criterion 3.4, work instruction 364 decision AJ).
    /// </summary>
    /// <returns>True where one was retired.</returns>
    /// <remarks>
    /// <para>**THE WINDOW IS THE TIMING TABLE'S** - the variant's seconds per character times
    /// <see cref="OliviaTiming.RetireAfterCharacters"/> - **counted from the end of the channel's last
    /// accepted block**, or from where it was read from if it has shown none. Never a figure in
    /// seconds (§3.2).</para>
    /// <para>**A RETIRED CHANNEL STAYS LISTED, ENDED, AND IS FED NOTHING MORE**, so its reader and its
    /// offset track stop and its center stays where it was. A new announcement or a blind find at the
    /// same place opens a new channel (decision AA): the place is free once the old one has ended.</para>
    /// <para>**ONLY <see cref="Add"/> RETIRES.** A recording's end is not a station going quiet.</para>
    /// </remarks>
    private bool RetireTheQuiet()
    {
        if (_timing is null)
        {
            return false;
        }

        var any = false;

        foreach (var channel in _channels.Where(c => !c.Ended))
        {
            var window = _timing.RetireWindowSeconds(channel.Stream.Variant.Name);

            if (double.IsNaN(window) || SamplesSeen - channel.LastShownEndSample <= window * _rate)
            {
                continue;
            }

            channel.Ended = true;
            channel.Retired = true;
            any = true;

            ChannelEvent(
                channel,
                "retired",
                new Dictionary<string, object?>
                {
                    ["windowSeconds"] = Math.Round(window, 3),
                    ["retireFactor"] = _timing.RetireAfterCharacters,
                    ["lastBlockEndSeconds"] = Math.Round(channel.LastShownEndSample / (double)_rate, 3),
                });
        }

        return any;
    }

    /// <summary>
    /// The audio has ended: every burst still held is heard, and every reader reads what is left.
    /// </summary>
    /// <remarks>A recording ends; the air does not, and the app never calls this.</remarks>
    public void Flush()
    {
        foreach (var heard in _rsid.Flush())
        {
            Announced(heard);
        }

        foreach (var found in _search.Flush(Occupied()))
        {
            Open(found.Stream, FoundBlind, found.StartSample);
        }

        foreach (var channel in _channels.Where(c => !c.Ended))
        {
            channel.Stream.Flush();
        }

        Report();
    }

    private IReadOnlyList<OliviaOccupied> Occupied()
        => _channels.Where(c => !c.Ended).Select(c => new OliviaOccupied(c.CenterHz, c.Stream.Variant.BandwidthHz)).ToList();

    private void Announced(RsidDetection heard)
    {
        if (heard.Mode != "OLIVIA" || _format.Variant(heard.Variant) is not { } variant)
        {
            return;
        }

        foreach (var channel in _channels.Where(c => !c.Ended))
        {
            if (Math.Abs(channel.CenterHz - heard.CenterHz) >= Math.Min(channel.Stream.Variant.BandwidthHz, variant.BandwidthHz) / 2.0)
            {
                continue;
            }

            if (channel.Stream.Variant.Name == variant.Name)
            {
                // **THE STATION ANNOUNCED WHAT IS ALREADY BEING READ**: nothing opens, and a
                // blind-found channel becomes an announced one.
                if (channel.Found != FoundByRsid)
                {
                    channel.Found = FoundByRsid;
                    ChannelEvent(channel, "announced");
                }

                return;
            }

            // **THE STATION SAID IT CHANGED**: the old reader stops and a new one opens.
            channel.Ended = true;
            channel.Stream.Flush();
            ChannelEvent(channel, "ended");
        }

        var burstEnd = heard.StartSeconds + (_codes.Symbols / _codes.SymbolRateHz);
        var start = Math.Max(_historyStart, (long)Math.Ceiling(burstEnd * _rate));
        var stream = new OliviaDemodulator(_format, variant, heard.CenterHz, _rate).Open();

        if (start < SamplesSeen)
        {
            stream.Add(_history.AsSpan((int)(start - _historyStart), (int)(SamplesSeen - start)));
        }

        Open(stream, FoundByRsid, Math.Min(start, SamplesSeen));
    }

    private void Open(OliviaStream stream, string found, long startSample)
    {
        var channel = new Channel(_nextId++, stream, found, SamplesSeen, startSample);

        _channels.Add(channel);
        ChannelEvent(channel, "opened");
    }

    private void Report()
    {
        foreach (var channel in _channels)
        {
            foreach (var block in channel.Stream.DrainBlocks())
            {
                if (block.Accepted)
                {
                    // **WHERE IT WAS WHEN IT WAS LAST READ** (decision AG), not where the track has
                    // wandered to since, and **WHEN THAT BLOCK ENDED**, which the retire window counts
                    // from (decision AJ).
                    channel.ShownCenterHz = channel.Stream.CenterHz + block.OffsetHz;
                    channel.LastShownEndSample = channel.StartSample
                        + (long)Math.Round((block.Seconds + (_format.SymbolsPerBlock * channel.Stream.Variant.SymbolSeconds)) * _rate);
                }

                _telemetry?.Write(
                    TelemetryCategory.Psk31,
                    "olivia_block",
                    new Dictionary<string, object?>
                    {
                        ["mode"] = "olivia",
                        ["variant"] = channel.Stream.Variant.Name,
                        ["id"] = channel.Id,
                        ["found"] = channel.Found,
                        ["centerHz"] = Math.Round(channel.CenterHz, 2),
                        ["atSeconds"] = Math.Round((channel.StartSample / (double)_rate) + block.Seconds, 3),
                        ["snr"] = Math.Round(block.Snr, 2),
                        ["accepted"] = block.Accepted,
                        ["characters"] = block.Characters,
                        ["blocksDecoded"] = channel.Stream.BlocksDecoded,
                        ["blocksRejected"] = channel.Stream.BlocksRejected,
                        ["threshold"] = OliviaDemodulator.SyncThreshold,
                    });
            }
        }

        _snapshot = _channels
            .Select(c => new OliviaChannel(
                c.Id,
                c.Stream.Variant.Name,
                c.CenterHz,
                c.Found,
                c.OpenedAt / (double)_rate,
                c.Stream.Text,
                c.Stream.BlocksDecoded,
                c.Stream.BlocksRejected,
                c.Ended,
                c.ShownCenterHz,
                c.Retired))
            .OrderBy(c => c.CenterHz)
            .ToList();
    }

    private void ChannelEvent(Channel channel, string state, IReadOnlyDictionary<string, object?>? more = null)
    {
        var fields = new Dictionary<string, object?>
        {
            ["mode"] = "olivia",
            ["variant"] = channel.Stream.Variant.Name,
            ["state"] = state,
            ["id"] = channel.Id,
            ["found"] = channel.Found,
            ["centerHz"] = Math.Round(channel.CenterHz, 2),
            ["atSeconds"] = Math.Round(SamplesSeen / (double)_rate, 3),
            ["readFromSeconds"] = Math.Round(channel.StartSample / (double)_rate, 3),
            ["replaySeconds"] = Math.Round(ReplaySeconds, 3),
        };

        foreach (var (key, value) in more ?? new Dictionary<string, object?>())
        {
            fields[key] = value;
        }

        _telemetry?.Write(TelemetryCategory.Psk31, "olivia_channel", fields);
    }

    private void Remember(ReadOnlySpan<float> samples)
    {
        if (_historyCount + samples.Length > _history.Length)
        {
            var drop = Math.Min(_historyCount, Math.Max(0, _historyCount + samples.Length - _keep));

            Array.Copy(_history, drop, _history, 0, _historyCount - drop);
            _historyCount -= drop;
            _historyStart += drop;

            if (_historyCount + samples.Length > _history.Length)
            {
                Array.Resize(ref _history, _historyCount + samples.Length);
            }
        }

        samples.CopyTo(_history.AsSpan(_historyCount));
        _historyCount += samples.Length;
    }

    private sealed class Channel(int id, OliviaStream stream, string found, long openedAt, long startSample)
    {
        public int Id { get; } = id;

        public OliviaStream Stream { get; } = stream;

        public string Found { get; set; } = found;

        public long OpenedAt { get; } = openedAt;

        public long StartSample { get; } = startSample;

        public bool Ended { get; set; }

        /// <summary>The center as of its last accepted block, or NaN before one.</summary>
        public double ShownCenterHz { get; set; } = double.NaN;

        /// <summary>Where its last accepted block ended, in the listener's samples; where it was read from until one is.</summary>
        public long LastShownEndSample { get; set; } = startSample;

        /// <summary>True where it ended by going quiet past its window.</summary>
        public bool Retired { get; set; }

        /// <summary>The center the reader was made at, moved by its offset track.</summary>
        public double CenterHz => Stream.CenterHz + Stream.OffsetHz;
    }
}
