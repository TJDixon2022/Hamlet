using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// A recording is cut into the fifteen-second slots FT8 transmits in, aligned to
/// UTC quarter minutes.
/// </summary>
/// <remarks>
/// <para>**TASK 6 OF WORK INSTRUCTION 042.** FT8 is synchronous: every station
/// starts on the same quarter minute and stops together, so the slot is the unit
/// a decoder works on. Audio cut anywhere else holds the tail of one
/// transmission and the head of the next, and is not decodable however clean it
/// is.</para>
/// <para>**PURE, AND TESTED WITHOUT A WALL CLOCK.** Nothing here reads the time.
/// The cutter is handed when the recording ended and works backwards from it, so
/// the same recording cuts the same way at any hour of any day. A test that
/// asked what time it is would be a test that passes on some evenings.</para>
/// </remarks>
public sealed class TheSlotCutterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cuts are printed.</param>
    public TheSlotCutterTests(ITestOutputHelper output) => _output = output;

    private const int Rate = 12000;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Silence(double seconds)
        => new(Rate, new float[(int)Math.Round(seconds * Rate)]);

    private static ClockOffset Offset(double seconds)
        => new(seconds, new DateTime(2026, 8, 28, 20, 47, 0, DateTimeKind.Utc));

    private void Print(SlotCut cut)
    {
        _output.WriteLine(
            $"  {cut.Slots.Count} slot(s), {cut.ShortAtStart} samples short at the "
            + $"start, {cut.ShortAtEnd} at the end"
            + (cut.Reason.Length > 0 ? $", reason: {cut.Reason}" : ""));

        foreach (var slot in cut.Slots)
        {
            _output.WriteLine(
                $"    {slot.StartUtc:HH:mm:ss} at sample {slot.FirstSample}, "
                + $"{slot.Audio.Duration.TotalSeconds:0.0} s");
        }
    }

    /// <remarks>
    /// <para>**THIRTY SECONDS PRESSED MID-SLOT GIVES ONE WHOLE SLOT**, and that
    /// is the ordinary case rather than a corner of it. A capture is pressed when
    /// the operator notices something, which is never on a quarter minute, so it
    /// runs from part way through one slot to part way through another and only
    /// the middle is whole.</para>
    /// <para>The boundaries are the clock's, not the recording's: every slot
    /// starts on a second divisible by fifteen.</para>
    /// </remarks>
    [Fact]
    public void ThirtySecondsPressedMidSlotGivesTheWholeSlotsInTheMiddle()
    {
        // Ends at 20:47:22.4 true, so the recording opened at 20:46:52.4.
        // The boundaries inside it are 20:47:00 and 20:47:15, and only the
        // first of those has fifteen whole seconds behind it.
        var cut = Ft8SlotCutter.Cut(
            Silence(30),
            new DateTime(2026, 8, 28, 20, 47, 22, 400, DateTimeKind.Utc),
            Offset(0));

        Print(cut);

        Assert.Equal("", cut.Reason);

        var slot = Assert.Single(cut.Slots);

        Assert.Equal(
            new DateTime(2026, 8, 28, 20, 47, 0, DateTimeKind.Utc), slot.StartUtc);

        Assert.Equal(0, slot.StartUtc.Second % 15);
        Assert.Equal(15 * Rate, slot.Audio.Samples.Length);

        // 7.6 seconds before the boundary and 7.4 after the slot ends, both
        // discarded and both counted (§0.0.1).
        Assert.Equal((int)Math.Round(7.6 * Rate), cut.ShortAtStart);
        Assert.Equal((int)Math.Round(7.4 * Rate), cut.ShortAtEnd);

        // And nothing is lost: the discards and the slots account for the file.
        Assert.Equal(
            30 * Rate,
            cut.ShortAtStart + cut.ShortAtEnd + (cut.Slots.Count * 15 * Rate));
    }

    /// <remarks>
    /// <para>**A CLOCK TWO SECONDS OUT CUTS EVERY SLOT TWO SECONDS EARLY**,
    /// which is a seventh of a transmission missing from the front of each one.
    /// So the offset is applied, and the proof is that the same recording with
    /// the same end time cuts at a different sample once the offset moves.</para>
    /// <para>The boundary is always on a true quarter minute; what moves is
    /// where in the recording that lands.</para>
    /// </remarks>
    [Fact]
    public void TheClockOffsetMovesWhereTheBoundaryLandsInTheRecording()
    {
        var endedAtPc = new DateTime(2026, 8, 28, 20, 47, 22, 400, DateTimeKind.Utc);

        var straight = Ft8SlotCutter.Cut(Silence(30), endedAtPc, Offset(0));
        var fast = Ft8SlotCutter.Cut(Silence(30), endedAtPc, Offset(-2.0));

        Print(straight);
        Print(fast);

        var a = Assert.Single(straight.Slots);
        var b = Assert.Single(fast.Slots);

        // The clock said 20:47:22.4 and true time was 20:47:20.4, so the whole
        // recording sits two seconds earlier and the boundary lands two seconds
        // later within it.
        Assert.Equal(a.StartUtc, b.StartUtc);
        Assert.Equal(a.FirstSample + (2 * Rate), b.FirstSample);
    }

    /// <remarks>
    /// **AN OFFSET NOBODY HAS MEASURED MEANS NO SLOTS AT ALL**, and the reason
    /// is in the result rather than in a log. Cutting on the PC clock instead
    /// would be a guess arriving as an alignment, which is worse than one
    /// arriving as a sentence because nothing about it looks uncertain.
    /// </remarks>
    [Fact]
    public void AnUnmeasuredClockCutsNothingAndSaysWhy()
    {
        var cut = Ft8SlotCutter.Cut(
            Silence(30),
            new DateTime(2026, 8, 28, 20, 47, 22, 400, DateTimeKind.Utc),
            ClockOffset.Unknown);

        Print(cut);

        Assert.Empty(cut.Slots);
        Assert.Equal(Ft8SlotCutter.NoOffset, cut.Reason);
        Assert.Contains("not known", cut.Reason, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **A SHORT SLOT IS DISCARDED RATHER THAN PADDED OR KEPT.** Padding puts
    /// silence on the air nobody transmitted; keeping it hands a decoder a
    /// fragment, and the empty result then reads as an empty band.
    /// </remarks>
    [Fact]
    public void ARecordingShorterThanASlotProducesNothing()
    {
        var cut = Ft8SlotCutter.Cut(
            Silence(12),
            new DateTime(2026, 8, 28, 20, 47, 22, 400, DateTimeKind.Utc),
            Offset(0));

        Print(cut);

        Assert.Empty(cut.Slots);
        Assert.Equal(Ft8SlotCutter.TooShort, cut.Reason);
        Assert.Equal(12 * Rate, cut.ShortAtStart);
    }

    /// <remarks>
    /// **THE SAME RECORDING CUTS THE SAME WAY AT ANY HOUR**, which is what pure
    /// means here. Three end times a day apart, all on the same quarter-minute
    /// phase, produce identical sample offsets.
    /// </remarks>
    [Fact]
    public void TheCutDoesNotDependOnWhenItIsRun()
    {
        var cuts = new[]
        {
            new DateTime(2026, 8, 28, 20, 47, 22, 400, DateTimeKind.Utc),
            new DateTime(2026, 8, 29, 3, 2, 22, 400, DateTimeKind.Utc),
            new DateTime(2027, 1, 1, 0, 17, 22, 400, DateTimeKind.Utc),
        }
        .Select(end => Ft8SlotCutter.Cut(Silence(30), end, Offset(0)))
        .ToList();

        foreach (var cut in cuts)
        {
            Print(cut);
        }

        Assert.All(cuts, c => Assert.Single(c.Slots));
        Assert.All(cuts, c => Assert.Equal(cuts[0].Slots[0].FirstSample, c.Slots[0].FirstSample));
        Assert.All(cuts, c => Assert.Equal(cuts[0].ShortAtEnd, c.ShortAtEnd));
    }

    /// <remarks>
    /// <para>**A REAL RECORDING FROM THE OPERATOR'S OWN RADIO**, thirty seconds
    /// at 48 kHz, so the arithmetic meets a real length and a real sample rate
    /// rather than a round number chosen to suit it.</para>
    /// <para>**AND IT PROVES NOTHING ABOUT FT8.** This is a Morse capture from
    /// 40 m: it holds no fifteen-second cycle and nothing here claims it does.
    /// The order asks for one of the operator's digital captures and
    /// **there is not one in this repository**, so what is asserted is what can
    /// be asserted, which is that the cutter handles a real file. Anything more
    /// would be a claim resting on audio nobody has (§12.5).</para>
    /// </remarks>
    [Fact]
    public void TheCutterHandlesARealRecordingWithoutClaimingWhatIsInIt()
    {
        var path = Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            "cw-2026-08-28-005051.wav");

        var audio = WavAudio.Read(path);

        _output.WriteLine(
            $"  {audio.Duration.TotalSeconds:0.0} s at {audio.SampleRate} Hz, "
            + $"{audio.Samples.Length} samples");

        // The sidecar beside it says the press was 2026-08-28 00:50:51 UTC, and
        // the press is the end of the window (task 5).
        var cut = Ft8SlotCutter.Cut(
            audio,
            new DateTime(2026, 8, 28, 0, 50, 51, DateTimeKind.Utc),
            Offset(0.12));

        Print(cut);

        Assert.Equal("", cut.Reason);
        Assert.NotEmpty(cut.Slots);

        foreach (var slot in cut.Slots)
        {
            Assert.Equal(0, slot.StartUtc.Second % 15);
            Assert.Equal(0, slot.StartUtc.Millisecond);
            Assert.Equal(
                (int)Math.Round(15.0 * audio.SampleRate), slot.Audio.Samples.Length);
            Assert.Equal(audio.SampleRate, slot.Audio.SampleRate);
        }

        // Every sample is accounted for, in a real file rather than in silence.
        Assert.Equal(
            audio.Samples.Length,
            cut.ShortAtStart + cut.ShortAtEnd
            + (cut.Slots.Count * (int)Math.Round(15.0 * audio.SampleRate)));

        // **AND THE SAMPLES ARE THE FILE'S OWN**, copied rather than computed:
        // a cutter that quietly resampled would pass every assertion above.
        var first = cut.Slots[0];

        for (var i = 0; i < 500; i++)
        {
            Assert.Equal(audio.Samples[first.FirstSample + i], first.Audio.Samples[i]);
        }
    }
}
