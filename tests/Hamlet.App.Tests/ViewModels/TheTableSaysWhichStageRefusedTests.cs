using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// A slot that was decoded and produced no text says which stage refused, in one
/// line under the decoded table (unit 233).
/// </summary>
/// <remarks>
/// <para>**2026-09-03.** The owner sat at 14.074, pressed the thing this phase was
/// built for, and got an empty table with no way to tell a quiet band from a wrong
/// audio device from a deaf decoder.</para>
/// <para>**IT DOES NOT DUPLICATE UNIT 228'S READINESS LINE.** That one is about the
/// setup before a decode; this is about a decode that happened, and the two never
/// describe the same moment.</para>
/// <para>**IT COUNTS AND IT DOES NOT INTERPRET** (`CLAUDE.md` §12.1). It names the
/// stage the numbers point at and stops. It does not conclude that the band was
/// quiet, does not say a station was weak, and does not tell anybody what to
/// change.</para>
/// </remarks>
public sealed class TheTableSaysWhichStageRefusedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the line is printed.</param>
    public TheTableSaysWhichStageRefusedTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime SlotStart =
        new(2026, 9, 3, 14, 22, 30, DateTimeKind.Utc);

    private static MainWindowViewModel Model() => new(new AppSettings(), null);

    private static Ft8Reception Reading(params Ft8SlotCensus[] slots)
        => new(Array.Empty<Ft8Decode>(), slots.Length, slots.Sum(s => s.CandidateCount), "")
        {
            Slots = slots,
            Offset = new ClockOffset(0, SlotStart),
        };

    private static Ft8SlotCensus Slot(
        int candidates, int parity, int checksum, int text, int seconds = 0)
        => new(
            SlotStart.AddSeconds(seconds),
            candidates,
            parity,
            checksum,
            text,
            0,
            // 37, not 30: a two-digit needle has to be one the slot's own
            // timestamp cannot produce, or the test fails on its own search
            // string rather than on a score reaching the screen.
            candidates > 0 ? new[] { 37 } : Array.Empty<int>(),
            48000);

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** Signals found, nothing read, and the line
    /// says which of the four stages stopped.
    /// </summary>
    [Fact]
    public void ASlotThatFoundSignalsAndReadNoneNamesTheStageThatRefused()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 17, parity: 0, checksum: 0, text: 0)));

        _output.WriteLine(model.DigitalCensusLine);

        Assert.True(model.HasDigitalCensus);
        Assert.Contains("14:22:30 UTC", model.DigitalCensusLine, StringComparison.Ordinal);
        Assert.Contains(
            "17 places that looked like the start of an FT8 transmission",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
        Assert.Contains(
            "not one of them came out as a valid codeword",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **NOTHING FOUND AT ALL IS A DIFFERENT SENTENCE**, because it is the one no
    /// decoder change touches - it is the front end, the device, the routing, the
    /// mode or the filter.
    /// </summary>
    [Fact]
    public void ASlotThatFoundNothingSaysNothingReachedTheDecoder()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 0, parity: 0, checksum: 0, text: 0)));

        _output.WriteLine(model.DigitalCensusLine);

        Assert.True(model.HasDigitalCensus);
        Assert.Contains(
            "found no place in it that looked like the start of an FT8 transmission",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
        Assert.Contains(
            "nothing reached the decoder at all",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
    }

    /// <summary>Codewords that carried no checksum are their own stage.</summary>
    [Fact]
    public void CodewordsWithoutAChecksumAreNamedAsSuch()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 9, parity: 4, checksum: 0, text: 0)));

        _output.WriteLine(model.DigitalCensusLine);

        Assert.Contains(
            "4 of them came out as valid codewords, and not one of those carried "
                + "its own checksum",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
    }

    /// <summary>Messages that could not be put into words are their own stage.</summary>
    [Fact]
    public void AChecksumThatNeverBecameWordsIsNamedAsSuch()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 6, parity: 3, checksum: 2, text: 0)));

        _output.WriteLine(model.DigitalCensusLine);

        Assert.Contains(
            "2 of them carried their own checksum, and not one of those could be "
                + "put into words",
            model.DigitalCensusLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE LINE IS ABSENT WHEN THE SLOT READ SOMETHING.** A line that always says
    /// something is one the operator stops reading.
    /// </summary>
    [Fact]
    public void ASlotThatProducedTextShowsNoCensusLineAtAll()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 8, parity: 3, checksum: 2, text: 2)));

        Assert.False(model.HasDigitalCensus);
        Assert.Equal(string.Empty, model.DigitalCensusLine);
    }

    /// <summary>
    /// A refusal has its own sentence on the strip and no slot behind it, so there
    /// is no census line to put under the table.
    /// </summary>
    [Fact]
    public void ARefusalDoesNotAlsoProduceACensusLine()
    {
        var model = Model();

        model.NoteSlot(new Ft8Reception(
            Array.Empty<Ft8Decode>(), 0, 0, Ft8SlotCutter.NoOffset));

        Assert.False(model.HasDigitalCensus);
        Assert.Equal(string.Empty, model.DigitalCensusLine);
    }

    /// <summary>
    /// Where a reading holds one slot that read something and one that did not, the
    /// one worth explaining is the one that read nothing.
    /// </summary>
    [Fact]
    public void TheSlotThatReadNothingIsTheOneExplained()
    {
        var model = Model();

        model.NoteSlot(Reading(
            Slot(candidates: 8, parity: 3, checksum: 2, text: 2),
            Slot(candidates: 11, parity: 0, checksum: 0, text: 0, seconds: 15)));

        _output.WriteLine(model.DigitalCensusLine);

        Assert.True(model.HasDigitalCensus);
        Assert.Contains("14:22:45 UTC", model.DigitalCensusLine, StringComparison.Ordinal);
        Assert.Contains(
            "11 places", model.DigitalCensusLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **NO SIGNAL-TO-NOISE RATIO REACHES THE SCREEN** (`CLAUDE.md` §0.0), and the
    /// Costas match counts are not shown here at all — a bare number beside the
    /// word *signal* is exactly how one gets read as decibels.
    /// </summary>
    [Fact]
    public void TheLineCarriesNoSignalToNoiseRatioAndNoBareScore()
    {
        var model = Model();

        model.NoteSlot(Reading(Slot(candidates: 17, parity: 0, checksum: 0, text: 0)));

        var line = model.DigitalCensusLine;

        Assert.DoesNotContain("snr", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dB", line, StringComparison.Ordinal);
        Assert.DoesNotContain("signal-to-noise", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("37", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **AND IT INTERPRETS NOTHING** (`CLAUDE.md` §12.1). It counts stages; it does
    /// not conclude anything about the band, the station or what to do next.
    /// </summary>
    [Fact]
    public void TheLineDrawsNoConclusionAboutTheBandOrWhatToDo()
    {
        var model = Model();

        foreach (var slot in new[]
        {
            Slot(candidates: 0, parity: 0, checksum: 0, text: 0),
            Slot(candidates: 17, parity: 0, checksum: 0, text: 0),
            Slot(candidates: 9, parity: 4, checksum: 0, text: 0),
            Slot(candidates: 6, parity: 3, checksum: 2, text: 0),
        })
        {
            model.NoteSlot(Reading(slot));

            var line = model.DigitalCensusLine;

            _output.WriteLine(line);

            foreach (var forbidden in new[]
            {
                "band is", "band was", "quiet", "nobody", "weak", "strong",
                "try ", "check your", "you should", "propagation", "antenna",
            })
            {
                Assert.DoesNotContain(forbidden, line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
