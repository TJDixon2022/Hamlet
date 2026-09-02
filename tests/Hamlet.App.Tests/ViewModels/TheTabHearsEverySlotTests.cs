using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The Digital tab decodes every completed slot and shows what it heard, with
/// nobody pressing anything.
/// </summary>
/// <remarks>
/// <para>**THE MEASUREMENT UNIT 225 WAS COMMISSIONED FOR IS IN THIS FILE.** Unit
/// 224 put the first real FT8 message on Hamlet's own screen and it arrived
/// because somebody pressed *keep the last 30 seconds*; slots decoded without a
/// press stood at nought. One press catches one slot, sometimes two, out of a
/// thirty-second ring, and FT8 puts a new transmission on the air four times a
/// minute.</para>
/// <para>**THE WHOLE CHAIN RUNS HERE AND NOT A SLICE OF IT** — synthesized
/// transmissions into a real <see cref="AudioTap"/>, a real
/// <see cref="Ft8SlotWatch"/> driven by a clock that is a variable, the real
/// reader, and the view model's own rows read back. The only thing missing
/// against the radio is the sound card.</para>
/// <para>**NOTHING HERE OPENS A WINDOW, TOUCHES A SOUND CARD OR READS THE
/// MACHINE'S CLOCK** (§5), so the same run happens at any hour of any day. And
/// nothing reaches a transmitter (§0.2).</para>
/// </remarks>
public sealed class TheTabHearsEverySlotTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheTabHearsEverySlotTests(ITestOutputHelper output) => _output = output;

    /// <summary>The rate the band is simulated at.</summary>
    private const int Rate = 12000;

    /// <summary>Where the transmissions are put, in the passband.</summary>
    private const float PlacedAtHz = 1240;

    /// <summary>How often the Digital tab looks, in seconds.</summary>
    private const double LookSeconds = 0.25;

    /// <summary>A clock checked at 14:20 and found to match UTC.</summary>
    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>The four transmissions put on the simulated band.</summary>
    private static readonly (DateTime At, string To, string Call, string Grid)[] OnTheAir =
    {
        (new DateTime(2026, 9, 2, 14, 22, 15, DateTimeKind.Utc), "CQ", "K1ABC", "FN42"),
        (new DateTime(2026, 9, 2, 14, 22, 30, DateTimeKind.Utc), "CQ", "W9XYZ", "EM48"),
        (new DateTime(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc), "CQ", "VE7AA", "CN89"),
        (new DateTime(2026, 9, 2, 14, 23, 0, DateTimeKind.Utc), "CQ", "EA3QQ", "JN11"),
    };

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** Consecutive slots decode on their own and
    /// their messages land on the Digital tab's table, in order, with nobody
    /// pressing anything.
    /// </summary>
    [Fact]
    public void ConsecutiveSlotsReachTheTableWithNobodyPressingAnything()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var slots = RunTheBand(
            model,
            new DateTime(2026, 9, 2, 14, 22, 10, DateTimeKind.Utc),
            new DateTime(2026, 9, 2, 14, 23, 20, DateTimeKind.Utc));

        _output.WriteLine(
            $"  {slots} consecutive slots decoded, nobody pressing anything");
        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");
        _output.WriteLine($"  strip   [{model.DigitalModeStripLine}]");

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                $"    {row.Utc}  {row.Snr}  {row.Dt}  {row.Hz}  {row.Message}");
        }

        // Five boundaries close between 14:22:10 and 14:23:20.
        Assert.Equal(5, slots);

        Assert.Equal(
            new[]
            {
                "CQ K1ABC FN42",
                "CQ W9XYZ EM48",
                "CQ VE7AA CN89",
                "CQ EA3QQ JN11",
            },
            model.DigitalDecodes.Select(r => r.Message).ToArray());

        Assert.Equal(
            new[] { "142215", "142230", "142245", "142300" },
            model.DigitalDecodes.Select(r => r.Utc).ToArray());

        Assert.True(model.HasDigitalDecodes);

        // **THE SNR CELL STAYS AN EM DASH** (§0.0). HM-OPEN-068 is Tim's.
        Assert.All(
            model.DigitalDecodes,
            r => Assert.Equal(DigitalDecodeRow.NoMeasurement, r.Snr));

        // **THE SUMMARY NAMES THE MOST RECENT SLOT, NOT THE FIRST.** Reading row
        // zero would leave it reporting a slot from an hour ago.
        Assert.Equal("142300 UTC · 4 shown", model.DigitalDecodedSummary);
    }

    /// <summary>
    /// **THE TABLE SHOWS A SESSION AND NOT A MOMENT.** Rows append, so the second
    /// slot does not erase the first.
    /// </summary>
    [Fact]
    public void RowsAppendRatherThanReplacing()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var counts = new List<int>();

        RunTheBand(
            model,
            new DateTime(2026, 9, 2, 14, 22, 10, DateTimeKind.Utc),
            new DateTime(2026, 9, 2, 14, 23, 20, DateTimeKind.Utc),
            afterEachSlot: () => counts.Add(model.DigitalDecodes.Count));

        _output.WriteLine($"  rows after each slot: {string.Join(", ", counts)}");

        // The first slot to close is 14:22:00 to 14:22:15, which nobody
        // transmitted in, and the four after it each add their own row and keep
        // the ones before. **Nothing ever goes down**, which is the assertion.
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, counts.ToArray());
    }

    /// <summary>
    /// **NO TRANSMISSION APPEARS TWICE, WHATEVER ROUTE IT ARRIVED BY** (§0.0). The
    /// running watch and the capture press both reach the same slots, and a table
    /// showing a message twice says two stations sent it.
    /// </summary>
    [Fact]
    public void TheSameTransmissionArrivingTwiceIsShownOnce()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var slot = OneSlot(OnTheAir[0]);

        model.NoteSlot(slot);
        model.NoteSlot(slot);
        model.NoteSlot(slot);

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine($"    {row.Utc}  {row.Hz}  {row.Message}");
        }

        var only = Assert.Single(model.DigitalDecodes);

        Assert.Equal("CQ K1ABC FN42", only.Message);
    }

    /// <summary>
    /// **THE TABLE IS BOUNDED AND THE OLDEST ROWS FALL OFF.** A night at 14.074 is
    /// 5760 slots, and an unbounded collection bound to an `ItemsControl` is a
    /// memory leak with a scrollbar.
    /// </summary>
    [Fact]
    public void TheTableIsBoundedAndTheOldestRowsFallOff()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var opened = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);
        var over = MainWindowViewModel.MaxDigitalDecodes + 20;

        // Cheap rows rather than synthesized audio: the assertion is about the
        // collection, and decoding five hundred slots to make it would be twenty
        // minutes of signal processing to test a `RemoveAt`.
        for (var i = 0; i < over; i++)
        {
            model.NoteSlot(
                new Ft8Reception(
                    new[]
                    {
                        new Ft8Decode(
                            opened.AddSeconds(15 * i), 1.4, 1240, 12, $"CQ K{i}ABC FN42"),
                    },
                    1,
                    1,
                    ""));
        }

        _output.WriteLine(
            $"  {over} slots offered, {model.DigitalDecodes.Count} rows kept, "
            + $"bound {MainWindowViewModel.MaxDigitalDecodes}");
        _output.WriteLine($"  oldest kept [{model.DigitalDecodes[0].Message}]");
        _output.WriteLine($"  newest kept [{model.DigitalDecodes[^1].Message}]");

        Assert.Equal(MainWindowViewModel.MaxDigitalDecodes, model.DigitalDecodes.Count);

        // The first twenty went over the side, oldest first.
        Assert.Equal("CQ K20ABC FN42", model.DigitalDecodes[0].Message);
        Assert.Equal($"CQ K{over - 1}ABC FN42", model.DigitalDecodes[^1].Message);
    }

    /// <summary>
    /// **A DIAL MOVE OUT OF THE PASSBAND CLEARS THE TABLE** (§0.0.1). Rows from
    /// 7.074 under the same heading as rows from 14.074 is a picture asserting
    /// that all of those stations were heard here.
    /// </summary>
    [Fact]
    public void MovingBandsClearsTheTableAndANudgeDoesNot()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.FrequencyHz = 14_074_000;
        model.NoteSlot(OneSlot(OnTheAir[0]));

        Assert.Single(model.DigitalDecodes);

        // A nudge inside the receiver's own audio passband. The same signals are
        // still arriving through the same filter.
        model.FrequencyHz = 14_074_500;

        _output.WriteLine(
            $"  after a 500 Hz nudge: {model.DigitalDecodes.Count} rows");

        Assert.Single(model.DigitalDecodes);

        // Another band entirely.
        model.FrequencyHz = 7_074_000;

        _output.WriteLine($"  after a band change: {model.DigitalDecodes.Count} rows");
        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);

        // **AND THE PANEL SAYS SO RATHER THAN GOING BLANK** (HM-DEC-021).
        Assert.Equal(DigitalIdleText.Decoded, model.DigitalDecodedIdle);
    }

    /// <summary>
    /// **AN UNMEASURED CLOCK OUTRANKS A FULL TABLE ON THE PANEL SUMMARY** (§0.0.1).
    /// A summary still counting yesterday's rows while nothing can arrive reads as
    /// a working session, and that is the commonest newcomer failure in this mode
    /// wearing a disguise.
    /// </summary>
    [Fact]
    public void ARefusalOutranksTheRowCountOnTheSummaryAndTheStrip()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.NoteSlot(OneSlot(OnTheAir[0]));

        Assert.Equal("142215 UTC · 1 shown", model.DigitalDecodedSummary);

        model.NoteSlot(
            new Ft8Reception(
                Array.Empty<Ft8Decode>(), 0, 0, Ft8SlotCutter.NoOffset));

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");
        _output.WriteLine($"  strip   [{model.DigitalModeStripLine}]");

        // The rows are still there — they were really heard — but the reason
        // nothing new is arriving is what the collapsed header now carries.
        Assert.Single(model.DigitalDecodes);
        Assert.Equal(Ft8SlotCutter.NoOffset, model.DigitalDecodedSummary);
        Assert.Equal(Ft8SlotCutter.NoOffset, model.DigitalModeStripLine);
    }

    /// <summary>
    /// **NOTHING NEW IS SAID ABOUT WHAT A MESSAGE MEANS** (§12.1). The
    /// plain-English panel carries the line Tim wrote in August and nothing this
    /// unit invented, however full the table above it gets.
    /// </summary>
    [Fact]
    public void AFullTableStillSaysNothingAboutWhatAMessageMeans()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        RunTheBand(
            model,
            new DateTime(2026, 9, 2, 14, 22, 10, DateTimeKind.Utc),
            new DateTime(2026, 9, 2, 14, 23, 20, DateTimeKind.Utc));

        _output.WriteLine($"  saying [{model.DigitalSayingIdle}]");

        Assert.Equal(4, model.DigitalDecodes.Count);
        Assert.Equal(DigitalIdleText.Saying, model.DigitalSayingIdle);
    }

    /// <summary>
    /// Play the simulated band past the view model's own watch and reader, a
    /// quarter second at a time, and return how many slots came back.
    /// </summary>
    /// <param name="model">The tab.</param>
    /// <param name="from">When to start looking, by the PC clock.</param>
    /// <param name="until">When to stop.</param>
    /// <param name="afterEachSlot">Run after every slot that reaches the table.</param>
    /// <remarks>
    /// **THE VIEW MODEL'S OWN TIMER IS NOT USED AND CANNOT BE.** It reads the
    /// machine's clock and needs a dispatcher, so this drives the same two calls
    /// the tick makes — the watch, then <see cref="MainWindowViewModel.NoteSlot"/>
    /// — against a clock that is a variable. What is asserted is what lands on the
    /// table, in the manner the capture press is already reached.
    /// </remarks>
    private static int RunTheBand(
        MainWindowViewModel model,
        DateTime from,
        DateTime until,
        Action? afterEachSlot = null)
    {
        var band = new SimulatedBand(Rate);

        foreach (var (at, to, call, grid) in OnTheAir)
        {
            band.Transmit(at, to, call, grid);
        }

        var tap = new AudioTap();
        var watch = new Ft8SlotWatch();
        var slots = 0;

        for (var at = from; at <= until; at = at.AddSeconds(LookSeconds))
        {
            band.FillTo(tap, at);

            var look = watch.Look(tap, at, Measured);

            if (look.Ready is not { } ready)
            {
                continue;
            }

            slots++;

            model.NoteSlot(Ft8Reader.Read(ready.Audio, ready.EndedAtPcUtc, Measured));

            afterEachSlot?.Invoke();
        }

        return slots;
    }

    /// <summary>One slot's worth of reception carrying one transmission.</summary>
    private static Ft8Reception OneSlot(
        (DateTime At, string To, string Call, string Grid) sent)
        => new(
            new[]
            {
                new Ft8Decode(
                    sent.At, 1.4, PlacedAtHz, 12, $"{sent.To} {sent.Call} {sent.Grid}"),
            },
            1,
            1,
            "");

    /// <summary>A band with transmissions on it, played into a tap in real time.</summary>
    private sealed class SimulatedBand
    {
        private static readonly DateTime Origin =
            new(2026, 9, 2, 14, 22, 0, DateTimeKind.Utc);

        private readonly int _rate;
        private readonly float[] _samples;
        private long _delivered;

        /// <summary>Creates two minutes of silence at a rate.</summary>
        /// <param name="rate">Samples per second.</param>
        public SimulatedBand(int rate)
        {
            _rate = rate;
            _samples = new float[rate * 120];
        }

        /// <summary>Put one transmission on the band in the slot named.</summary>
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

            slot.CopyTo(
                _samples.AsSpan(
                    (int)Math.Round((slotStartUtc - Origin).TotalSeconds * _rate)));
        }

        /// <summary>Deliver everything up to a moment into the tap.</summary>
        /// <param name="tap">Where the audio goes.</param>
        /// <param name="atUtc">How far to play, by the PC clock.</param>
        public void FillTo(AudioTap tap, DateTime atUtc)
        {
            var wanted = (long)Math.Round((atUtc - Origin).TotalSeconds * _rate);

            if (wanted <= _delivered)
            {
                return;
            }

            tap.Take(_samples.AsSpan((int)_delivered, (int)(wanted - _delivered)), _rate);
            _delivered = wanted;
        }
    }
}
