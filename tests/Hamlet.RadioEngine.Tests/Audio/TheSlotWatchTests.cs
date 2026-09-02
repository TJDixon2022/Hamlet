using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The watch notices when a fifteen-second slot has closed, and hands over
/// exactly that slot's audio and nothing else.
/// </summary>
/// <remarks>
/// <para>**THIS IS STEP 7'S FIRST MUST-PASS CRITERION IN ITS OWN WORDS**: audio
/// arrives in fifteen-second slots aligned to the quarter minute, *asserted
/// against synthesized audio and a controllable clock*. The clock here is a
/// variable, the audio is synthesized into an array, and neither the machine's
/// time nor a sound card is involved anywhere — so the same run happens at any
/// hour of any day.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (§0.2). The synthesizer is a test
/// oracle and its samples go into a ring buffer.</para>
/// </remarks>
public sealed class TheSlotWatchTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the slots are printed.</param>
    public TheSlotWatchTests(ITestOutputHelper output) => _output = output;

    /// <summary>The rate the band is simulated at — the library's own.</summary>
    private const int Rate = 12000;

    /// <summary>Where the transmissions are put, in the passband.</summary>
    private const float PlacedAtHz = 1240;

    /// <summary>How often the Digital tab looks, in seconds.</summary>
    private const double LookSeconds = 0.25;

    /// <summary>A clock checked at 14:20 and found to match UTC.</summary>
    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// **THE FIRST LOOK ARMS AND CLAIMS NOTHING.** A watch that opens its eyes
    /// part way through a slot did not hear the slot before it, and the ring may
    /// hold anything from all of it to none of it.
    /// </summary>
    [Fact]
    public void TheFirstLookArmsAndYieldsNothing()
    {
        var tap = TapHolding(Rate, 30);
        var watch = new Ft8SlotWatch();

        var look = watch.Look(
            tap, new DateTime(2026, 9, 2, 14, 22, 37, DateTimeKind.Utc), Measured);

        _output.WriteLine($"  ready {look.Ready is not null}, refusal [{look.Refusal}]");
        _output.WriteLine($"  armed at {watch.LastSeenSlotStart:HH:mm:ss}");

        Assert.Null(look.Ready);
        Assert.Equal(string.Empty, look.Refusal);
        Assert.True(watch.IsWatching);
        Assert.Equal(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc),
            watch.LastSeenSlotStart);
    }

    /// <summary>
    /// **A LOOK IN THE MIDDLE OF A SLOT YIELDS NOTHING**, which is the answer
    /// fifty-nine of every sixty looks must give: the tab looks four times a
    /// second and a slot lasts fifteen.
    /// </summary>
    [Fact]
    public void ALookInsideASlotYieldsNothing()
    {
        var tap = TapHolding(Rate, 30);
        var watch = new Ft8SlotWatch();

        var start = new DateTime(2026, 9, 2, 14, 22, 31, DateTimeKind.Utc);

        watch.Look(tap, start, Measured);

        var produced = 0;

        // Every quarter second from one second into the slot to fourteen.
        for (var at = 1.25; at < 15.0; at += LookSeconds)
        {
            var look = watch.Look(tap, start.AddSeconds(at - 1), Measured);

            if (look.Ready is not null)
            {
                produced++;
            }

            Assert.Equal(string.Empty, look.Refusal);
        }

        _output.WriteLine($"  looks inside the slot produced {produced} slots");

        Assert.Equal(0, produced);
    }

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** A look just past a boundary yields
    /// exactly the slot that ended — and the audio handed over decodes to the
    /// message that was transmitted in it.
    /// </summary>
    [Fact]
    public void ALookPastABoundaryYieldsExactlyTheSlotThatEnded()
    {
        var band = new Band(Rate);

        band.Transmit(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc),
            "CQ", "K1ABC", "FN42");

        var tap = new AudioTap();

        // Fill the ring right up to a quarter second past the boundary at 14:22:45.
        var now = band.FillTo(
            tap, new DateTime(2026, 9, 2, 14, 22, 45, 250, DateTimeKind.Utc));

        var watch = new Ft8SlotWatch();

        // Armed two looks earlier, inside the slot that is about to close.
        watch.Look(tap, now.AddSeconds(-2 * LookSeconds), Measured);

        var look = watch.Look(tap, now, Measured);

        Assert.Equal(string.Empty, look.Refusal);

        Assert.NotNull(look.Ready);

        var ready = look.Ready!;

        _output.WriteLine(
            $"  slot {ready.SlotStartUtc:HH:mm:ss}, ended (pc) "
            + $"{ready.EndedAtPcUtc:HH:mm:ss}, {ready.Audio.Samples.Length} samples "
            + $"= {ready.Audio.Duration.TotalSeconds:0.000} s");

        Assert.Equal(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc), ready.SlotStartUtc);

        // **WHOLE, NOT SHORT AND NOT PADDED** (§0.0).
        Assert.Equal(15 * Rate, ready.Audio.Samples.Length);

        var heard = Ft8Reader.Read(ready.Audio, ready.EndedAtPcUtc, Measured);

        foreach (var decode in heard.Decodes)
        {
            _output.WriteLine(
                $"    {decode.SlotStartUtc:HHmmss}  dt {decode.OffsetSeconds:0.00}  "
                + $"{decode.FrequencyHz:0} Hz  {decode.Message}");
        }

        Assert.Equal(1, heard.SlotsDecoded);

        var only = Assert.Single(heard.Decodes);

        Assert.Equal("CQ K1ABC FN42", only.Message);
        Assert.Equal(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc), only.SlotStartUtc);
    }

    /// <summary>
    /// **THE SAME LOOK REPEATED YIELDS NOTHING THE SECOND TIME**, so a quarter
    /// second tick over a fifteen second slot produces one decode and not sixty.
    /// </summary>
    [Fact]
    public void TheSameLookRepeatedYieldsNothingTheSecondTime()
    {
        var band = new Band(Rate);

        band.Transmit(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc),
            "CQ", "K1ABC", "FN42");

        var tap = new AudioTap();
        var now = band.FillTo(
            tap, new DateTime(2026, 9, 2, 14, 22, 45, 250, DateTimeKind.Utc));

        var watch = new Ft8SlotWatch();

        watch.Look(tap, now.AddSeconds(-2 * LookSeconds), Measured);

        var first = watch.Look(tap, now, Measured);
        var second = watch.Look(tap, now, Measured);
        var third = watch.Look(tap, now.AddSeconds(LookSeconds), Measured);

        _output.WriteLine(
            $"  first {first.Ready is not null}, second {second.Ready is not null}, "
            + $"third {third.Ready is not null}");

        Assert.NotNull(first.Ready);
        Assert.Null(second.Ready);
        Assert.Null(third.Ready);
        Assert.Equal(string.Empty, second.Refusal);
    }

    /// <summary>
    /// **A LOOK ARRIVING SEVERAL SLOTS LATE TAKES THE SLOT THAT JUST CLOSED AND
    /// COUNTS THE REST AS MISSED.** It skips rather than stalling: a busy machine
    /// or a laptop resuming must not send the watch walking back through audio the
    /// ring has already dropped.
    /// </summary>
    [Fact]
    public void ALookArrivingSeveralSlotsLateTakesTheLastAndCountsTheRestMissed()
    {
        var band = new Band(Rate);
        var tap = new AudioTap();

        var atArm = new DateTime(2026, 9, 2, 14, 22, 20, DateTimeKind.Utc);

        band.FillTo(tap, atArm);

        var watch = new Ft8SlotWatch();
        watch.Look(tap, atArm, Measured);

        // Audio kept arriving the whole time; only the looking stopped.
        var late = band.FillTo(
            tap, new DateTime(2026, 9, 2, 14, 23, 46, DateTimeKind.Utc));

        var look = watch.Look(tap, late, Measured);

        _output.WriteLine(
            $"  refusal [{look.Refusal}], skipped {look.Skipped}, "
            + $"slot {look.Ready?.SlotStartUtc:HH:mm:ss}");

        Assert.Equal(string.Empty, look.Refusal);
        Assert.NotNull(look.Ready);

        // The slot that closed at 14:23:45, and not one of the five before it.
        Assert.Equal(
            new DateTime(2026, 9, 2, 14, 23, 30, DateTimeKind.Utc),
            look.Ready!.SlotStartUtc);

        Assert.Equal(15 * Rate, look.Ready.Audio.Samples.Length);

        // Six boundaries closed between 14:22:20 and 14:23:46; five were missed
        // and are counted rather than swallowed (§0.0.1).
        Assert.Equal(5, look.Skipped);
    }

    /// <summary>
    /// **A SLOT THE RING NEVER HELD WHOLE IS REFUSED IN WORDS RATHER THAN
    /// SHORTENED** (§0.0). This is the state at start-up: the tap has only just
    /// begun filling when the first boundary arrives.
    /// </summary>
    [Fact]
    public void ASlotTheRingNeverHeldWholeIsRefusedRatherThanShortened()
    {
        var band = new Band(Rate);
        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        // Audio starts arriving five seconds before the boundary at 14:22:45, so
        // the slot that closes on it was never in the ring.
        var opened = new DateTime(2026, 9, 2, 14, 22, 40, DateTimeKind.Utc);

        band.OpenAt(opened);
        band.FillTo(tap, opened);
        watch.Look(tap, opened, Measured);

        var at = band.FillTo(
            tap, new DateTime(2026, 9, 2, 14, 22, 45, 250, DateTimeKind.Utc));

        var look = watch.Look(tap, at, Measured);

        _output.WriteLine(
            $"  tap holds {tap.Level.Seconds:0.0} s, refusal [{look.Refusal}]");

        Assert.Null(look.Ready);
        Assert.Equal(Ft8SlotWatch.AudioAgedOut, look.Refusal);
        Assert.Equal(1, look.Skipped);
    }

    /// <summary>
    /// **A STALLED STREAM REFUSES RATHER THAN DECODING OLD AUDIO AS NEW**, which
    /// is the one fault the watch's sample-to-moment mapping could commit and is
    /// the §0.0 fault in its worst form: the row would look exactly like a real
    /// decode.
    /// </summary>
    /// <remarks>
    /// HM-DEC-090 caught this shape once already, where a stalled pipeline let a
    /// capture hand over the same thirty seconds three times and the analysis
    /// beside it read as three measurements.
    /// </remarks>
    [Fact]
    public void AStalledAudioStreamRefusesRatherThanDecodingOldAudioAsNew()
    {
        var band = new Band(Rate);
        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        // Twenty seconds of audio arrive, and then the stream stops dead while
        // the clock runs on past three more boundaries.
        var flowing = new DateTime(2026, 9, 2, 14, 22, 20, DateTimeKind.Utc);

        band.FillTo(tap, flowing);
        watch.Look(tap, flowing, Measured);

        var look = watch.Look(
            tap, new DateTime(2026, 9, 2, 14, 23, 6, DateTimeKind.Utc), Measured);

        _output.WriteLine($"  refusal [{look.Refusal}], skipped {look.Skipped}");

        Assert.Null(look.Ready);
        Assert.Equal(Ft8SlotWatch.AudioStalled, look.Refusal);

        // And it re-arms, so nothing straddling the stall is claimed when the
        // audio comes back.
        Assert.False(watch.IsWatching);
    }

    /// <summary>
    /// **A CLOCK NOBODY HAS CHECKED PRODUCES THE CUTTER'S OWN SENTENCE,
    /// UNCHANGED.** The operator meets this state through several doors and must
    /// read one answer.
    /// </summary>
    [Fact]
    public void AnUnmeasuredClockRefusesInTheCuttersOwnWords()
    {
        var tap = TapHolding(Rate, 30);
        var watch = new Ft8SlotWatch();

        var look = watch.Look(
            tap,
            new DateTime(2026, 9, 2, 14, 22, 37, DateTimeKind.Utc),
            ClockOffset.Unknown);

        _output.WriteLine($"  refusal [{look.Refusal}]");

        Assert.Null(look.Ready);
        Assert.Equal(Ft8SlotCutter.NoOffset, look.Refusal);
        Assert.False(watch.IsWatching);
    }

    /// <summary>
    /// **A MEASUREMENT TOO OLD TO RELY ON IS SAID AND NOT USED.** The threshold
    /// is <see cref="ClockOffset.StaleAfterSeconds"/> and is not re-decided here.
    /// </summary>
    [Fact]
    public void AStaleOffsetRefusesAndSaysHowOldItIs()
    {
        var tap = TapHolding(Rate, 30);
        var watch = new Ft8SlotWatch();

        var atUtc = new DateTime(2026, 9, 2, 14, 22, 37, DateTimeKind.Utc);
        var old = new ClockOffset(
            0, atUtc.AddSeconds(-(ClockOffset.StaleAfterSeconds + 60)));

        var look = watch.Look(tap, atUtc, old);

        _output.WriteLine($"  refusal [{look.Refusal}]");

        Assert.Null(look.Ready);
        Assert.Contains(Ft8SlotWatch.StaleOffset, look.Refusal, StringComparison.Ordinal);

        // **THE AGE IS THE OFFSET'S OWN WORDS AND NOT A SECOND OPINION**, so the
        // operator reads the same sentence the strip carries.
        Assert.Contains(old.Describe(atUtc), look.Refusal, StringComparison.Ordinal);
    }

    /// <summary>
    /// **AN OFFSET THAT CHANGES BETWEEN LOOKS MOVES THE BOUNDARY AND PRODUCES
    /// NEITHER A DUPLICATE NOR A GAP.** The clock query runs on its own timer and
    /// a fresh measurement can land at any moment; the grid moves with it, and no
    /// slot is claimed twice on the strength of it.
    /// </summary>
    [Fact]
    public void AnOffsetChangingBetweenLooksProducesNoDuplicateAndNoGap()
    {
        var band = new Band(Rate);

        band.Transmit(
            new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc),
            "CQ", "K1ABC", "FN42");
        band.Transmit(
            new DateTime(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc),
            "CQ", "W9XYZ", "EM48");

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        var measuredAt = new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc);

        // The PC is running two seconds slow for the first half of the run, and a
        // fresh query lands part way through saying it is running one second fast.
        var early = new ClockOffset(2, measuredAt);
        var late = new ClockOffset(-1, measuredAt);

        var seen = new List<DateTime>();
        var refusals = new List<string>();

        // PC-clock instants: true UTC 14:22:20 is PC 14:22:18 under the first
        // offset, so the run is expressed in PC time throughout.
        var from = new DateTime(2026, 9, 2, 14, 22, 16, DateTimeKind.Utc);
        var to = new DateTime(2026, 9, 2, 14, 23, 6, DateTimeKind.Utc);
        var swapAt = new DateTime(2026, 9, 2, 14, 22, 40, DateTimeKind.Utc);

        for (var at = from; at <= to; at = at.AddSeconds(LookSeconds))
        {
            band.FillTo(tap, at);

            var offset = at < swapAt ? early : late;
            var look = watch.Look(tap, at, offset);

            if (look.Refusal.Length > 0)
            {
                refusals.Add(look.Refusal);
            }

            if (look.Ready is { } ready)
            {
                seen.Add(ready.SlotStartUtc);
            }
        }

        foreach (var slot in seen)
        {
            _output.WriteLine($"  yielded {slot:HH:mm:ss}");
        }

        Assert.Empty(refusals);

        // **NO SLOT TWICE**, whichever grid it was found on.
        Assert.Equal(seen.Count, seen.Distinct().Count());

        // **AND NO GAP**: every boundary that closed while the watch was running
        // came back, on whichever grid was in force when it closed.
        Assert.Equal(
            seen.OrderBy(s => s).ToList(),
            seen);

        Assert.True(seen.Count >= 3, $"only {seen.Count} slots came back");
    }

    /// <summary>
    /// **THE MEASUREMENT THIS UNIT WAS COMMISSIONED FOR.** The clock is driven
    /// forward across several boundaries over synthesized audio with nobody
    /// pressing anything, and every slot that closed comes back with its own
    /// message on it.
    /// </summary>
    [Fact]
    public void ConsecutiveSlotsDecodeWithNobodyPressingAnything()
    {
        var band = new Band(Rate);

        var messages = new (DateTime At, string To, string Call, string Grid)[]
        {
            (new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc), "CQ", "K1ABC", "FN42"),
            (new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc), "CQ", "W9XYZ", "EM48"),
            (new DateTime(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc), "CQ", "VE7AA", "CN89"),
            (new DateTime(2026, 9, 2, 14, 23, 0, DateTimeKind.Utc), "CQ", "EA3QQ", "JN11"),
        };

        foreach (var (at, to, call, grid) in messages)
        {
            band.Transmit(at, to, call, grid);
        }

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        var decoded = new List<Ft8Decode>();
        var slots = 0;

        var from = new DateTime(2026, 9, 2, 14, 22, 10, DateTimeKind.Utc);
        var until = new DateTime(2026, 9, 2, 14, 23, 20, DateTimeKind.Utc);

        for (var at = from; at <= until; at = at.AddSeconds(LookSeconds))
        {
            band.FillTo(tap, at);

            var look = watch.Look(tap, at, Measured);

            Assert.Equal(string.Empty, look.Refusal);
            Assert.Equal(0, look.Skipped);

            if (look.Ready is not { } ready)
            {
                continue;
            }

            slots++;

            var heard = Ft8Reader.Read(ready.Audio, ready.EndedAtPcUtc, Measured);

            Assert.Equal(string.Empty, heard.Refusal);
            Assert.Equal(1, heard.SlotsDecoded);

            decoded.AddRange(heard.Decodes);
        }

        _output.WriteLine($"  {slots} consecutive slots decoded, nobody pressing anything");

        foreach (var decode in decoded)
        {
            _output.WriteLine(
                $"    {decode.SlotStartUtc:HHmmss}  dt {decode.OffsetSeconds:0.0}  "
                + $"{decode.FrequencyHz:0} Hz  {decode.Message}");
        }

        // Five boundaries close between 14:22:10 and 14:23:20 — :15, :30, :45,
        // 14:23:00 and 14:23:15 — and every one of them is a slot.
        Assert.Equal(5, slots);

        Assert.Equal(
            new[]
            {
                "CQ K1ABC FN42",
                "CQ W9XYZ EM48",
                "CQ VE7AA CN89",
                "CQ EA3QQ JN11",
            },
            decoded.Select(d => d.Message).ToArray());

        // Every message came back under the quarter minute it was sent in.
        Assert.Equal(
            messages.Select(m => m.At).ToArray(),
            decoded.Select(d => d.SlotStartUtc).ToArray());
    }

    /// <summary>
    /// **A WATCH THAT STOPPED LOOKING RE-ARMS RATHER THAN CLAIMING THE SLOT IT
    /// MISSED.** This is what the Digital tab does when it goes off screen and
    /// comes back.
    /// </summary>
    [Fact]
    public void RearmingMakesTheNextLookClaimNothing()
    {
        var band = new Band(Rate);
        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();

        var at = new DateTime(2026, 9, 2, 14, 22, 20, DateTimeKind.Utc);

        band.FillTo(tap, at);
        watch.Look(tap, at, Measured);

        watch.Rearm();

        Assert.False(watch.IsWatching);

        var later = new DateTime(2026, 9, 2, 14, 22, 46, DateTimeKind.Utc);

        band.FillTo(tap, later);

        var look = watch.Look(tap, later, Measured);

        _output.WriteLine($"  after re-arming, ready {look.Ready is not null}");

        Assert.Null(look.Ready);
        Assert.True(watch.IsWatching);
    }

    /// <summary>A tap already holding a stretch of silence at a known rate.</summary>
    private static AudioTap TapHolding(int rate, int seconds)
    {
        var tap = new AudioTap();
        tap.Take(new float[rate * seconds], rate);
        return tap;
    }

    /// <summary>
    /// A band with transmissions on it, played into a tap in real time.
    /// </summary>
    /// <remarks>
    /// **THE AUDIO IS A FUNCTION OF THE CLOCK AND NOT OF THE LOOP.** Samples are
    /// addressed from a fixed origin in UTC, so the same buffer arrives however
    /// the caller chops the run up, and a look at a given instant sees exactly the
    /// audio that instant implies.
    /// </remarks>
    private sealed class Band
    {
        /// <summary>Where the simulated band begins.</summary>
        private static readonly DateTime Origin =
            new(2026, 9, 2, 14, 22, 0, DateTimeKind.Utc);

        private readonly int _rate;
        private readonly float[] _samples;
        private long _delivered;

        /// <summary>Creates a band of silence two minutes long.</summary>
        /// <param name="rate">Samples per second.</param>
        public Band(int rate)
        {
            _rate = rate;
            _samples = new float[rate * 120];
        }

        /// <summary>Put one transmission on the band, in the slot named.</summary>
        /// <param name="slotStartUtc">The quarter minute it goes out in.</param>
        /// <param name="to">Who it is addressed to.</param>
        /// <param name="call">Who is sending.</param>
        /// <param name="grid">Their grid square.</param>
        public void Transmit(DateTime slotStartUtc, string to, string call, string grid)
        {
            var packed = new byte[Ft8StandardMessage.MessageBytes];

            Assert.Equal(
                Ft8PackResult.Ok, Ft8StandardMessage.TryPack(to, call, grid, packed));

            var slot = Ft8Waveform.SynthesizeSlot(
                Ft8SymbolEncoder.Encode(packed), _rate, PlacedAtHz);

            var at = (int)Math.Round((slotStartUtc - Origin).TotalSeconds * _rate);

            slot.CopyTo(_samples.AsSpan(at));
        }

        /// <summary>
        /// Start the band at a moment, so nothing before it is ever delivered.
        /// </summary>
        /// <param name="atUtc">When the sound card opened.</param>
        /// <remarks>
        /// **THE RING IS EMPTY AT START-UP AND THAT IS A STATE WORTH TESTING.**
        /// Without this the tap would be handed every sample from the origin at
        /// once and would always be full, which is the one condition the start-up
        /// case is about.
        /// </remarks>
        public void OpenAt(DateTime atUtc)
            => _delivered = (long)Math.Round((atUtc - Origin).TotalSeconds * _rate);

        /// <summary>
        /// Deliver everything up to a moment into the tap, and return that moment.
        /// </summary>
        /// <param name="tap">Where the audio goes.</param>
        /// <param name="atUtc">How far to play, by the PC clock.</param>
        /// <returns><paramref name="atUtc"/>, so a caller can chain.</returns>
        public DateTime FillTo(AudioTap tap, DateTime atUtc)
        {
            var wanted = (long)Math.Round((atUtc - Origin).TotalSeconds * _rate);

            if (wanted > _delivered)
            {
                var count = (int)(wanted - _delivered);

                tap.Take(_samples.AsSpan((int)_delivered, count), _rate);
                _delivered = wanted;
            }

            return atUtc;
        }
    }
}
