using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The capture sheet says which audio path it ran on, where the slot boundaries
/// fell, and how far each slot's candidates got (unit 233).
/// </summary>
/// <remarks>
/// <para>**2026-09-03 IS THE FAILURE STATE THIS TEST HOLDS.** The owner sat at
/// 14.074, pressed the thing this phase was built for, and got an empty table. The
/// only artefacts were a screenshot and a spectrogram analysis, and nothing on
/// disk could say which sound card was open, whether a whole transmission was even
/// inside the audio, or which stage of the decode refused.</para>
/// <para>**THREE STATEMENTS THAT LOOK ALIKE AND ARE NOT.** *Nothing was on the
/// air*, *nothing decodable was captured* and *the decoder could not read what was
/// captured* all produce one empty table. Only the last one is about the decoder,
/// and the sheet now separates them.</para>
/// <para>**EVERY ADDED ROW OBEYS THE SHEET'S OWN RULE**: measured, or
/// <see cref="DigitalCaptureSheet.Unread"/>, and nothing defaulted.</para>
/// </remarks>
public sealed class TheSheetSaysWhichAudioPathItRanOnTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sheet is printed.</param>
    public TheSheetSaysWhichAudioPathItRanOnTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The press, at a moment chosen so exactly two whole slots fall in a
    /// thirty-second window: 14:22:30 and 14:22:45.
    /// </summary>
    private static readonly DateTime Pressed =
        new(2026, 9, 3, 14, 23, 2, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, Pressed.AddSeconds(-40));

    private static RigState AtTheRadio() => RigState.Empty.With(new[]
    {
        RigValue.Known(RigField.Mode, (int)CivMode.Usb, "USB", Pressed, "CI-V 04"),
        RigValue.Known(RigField.DataMode, 1, "on", Pressed, "CI-V 26 00"),
        RigValue.Known(
            RigField.Frequency, 14_074_000, "14.074000 MHz", Pressed, "CI-V 03"),
    });

    private static string Compose(
        AudioPath? path,
        IReadOnlyList<Ft8SlotCensus>? census,
        string refusal,
        double seconds = 30.0,
        ClockOffset? clock = null)
        => DigitalCaptureSheet.Compose(
            Pressed,
            seconds,
            48000,
            AtTheRadio(),
            clock ?? Measured,
            Pressed,
            "FT8 city",
            3000,
            path,
            census,
            refusal);

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** The sheet names the sound card, its rate,
    /// its channels and its depth, and what Windows was doing to the level.
    /// </summary>
    [Fact]
    public void TheSheetNamesTheSoundCardTheAudioCameThrough()
    {
        var sheet = Compose(
            new AudioPath(
                "USB Audio CODEC",
                48000,
                2,
                "Pcm 24-bit",
                IsSimulated: false,
                new CaptureHealth("USB Audio CODEC", 0.35, Muted: false)),
            Array.Empty<Ft8SlotCensus>(),
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains("device     USB Audio CODEC", sheet, StringComparison.Ordinal);
        Assert.Contains("deviceRate 48000 Hz", sheet, StringComparison.Ordinal);
        Assert.Contains("channels   2", sheet, StringComparison.Ordinal);
        Assert.Contains("encoding   Pcm 24-bit", sheet, StringComparison.Ordinal);
        Assert.Contains("audioIsReal yes", sheet, StringComparison.Ordinal);

        // The third gain nobody can see, which is applied before Hamlet gets a
        // look at anything (HM-DEC-088).
        Assert.Contains("windowsGain 35 percent", sheet, StringComparison.Ordinal);
        Assert.Contains("windowsMuted no", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A PATH NOBODY READ SAYS SO** and never carries a plausible default, which
    /// is the sheet's own standing rule.
    /// </summary>
    [Fact]
    public void AnUnreadAudioPathSaysUnknownRatherThanGuessing()
    {
        var sheet = Compose(null, null, string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains(
            "device     " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.Contains(
            "channels   " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.Contains(
            "encoding   " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.Contains(
            "windowsGain " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);

        // A default of "1 channel at 48 kHz" would be believed, which is worse
        // than a gap.
        Assert.DoesNotContain("channels   1\n", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **SYNTHESIZED AUDIO SAYS SO IN CAPITALS** (HM-DEC-026). A decode from the
    /// training radio must never read as something that was on the air.
    /// </summary>
    [Fact]
    public void SynthesizedAudioIsNamedAsSynthesized()
    {
        var sheet = Compose(
            new AudioPath(
                "Training radio (no hardware)",
                8000,
                null,
                "",
                IsSimulated: true,
                CaptureHealth.Unknown),
            Array.Empty<Ft8SlotCensus>(),
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains("audioIsReal NO", sheet, StringComparison.Ordinal);
        Assert.Contains("synthesized", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// The slot geometry: every boundary in the window, and whether the whole
    /// transmission after it is inside the audio.
    /// </summary>
    [Fact]
    public void EverySlotBoundaryInTheWindowIsNamedWithWhetherItsTransmissionFits()
    {
        var sheet = Compose(null, Array.Empty<Ft8SlotCensus>(), string.Empty);

        _output.WriteLine(sheet);

        // Thirty seconds back from 14:23:02 is 14:22:32, so the boundaries inside
        // the window are 14:22:45 and 14:23:00. Only the first is followed by the
        // whole 12.64 s transmission before the audio ends.
        Assert.Contains(
            "2026-09-03 14:22:45 UTC  whole transmission inside the audio",
            sheet,
            StringComparison.Ordinal);
        Assert.Contains(
            "2026-09-03 14:23:00 UTC  CUT SHORT",
            sheet,
            StringComparison.Ordinal);
        Assert.Contains("wholeSlots 1", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A CAPTURE HOLDING NO WHOLE SLOT SAYS SO IN ITS OWN FIELD**, because
    /// *nothing decoded* and *nothing decodable was captured* are different
    /// statements and only one of them is about the decoder.
    /// </summary>
    [Fact]
    public void ACaptureHoldingNoWholeTransmissionSaysSoInItsOwnField()
    {
        var sheet = Compose(null, Array.Empty<Ft8SlotCensus>(), string.Empty, seconds: 4.0);

        _output.WriteLine(sheet);

        Assert.Contains(
            "wholeSlots 0  (NO WHOLE TRANSMISSION IS INSIDE THIS AUDIO",
            sheet,
            StringComparison.Ordinal);
    }

    /// <summary>An unmeasured clock cannot place a boundary, and says that.</summary>
    [Fact]
    public void AnUnmeasuredClockLeavesTheGridUnknownRatherThanDrawnOnAGuess()
    {
        var sheet = Compose(
            null,
            Array.Empty<Ft8SlotCensus>(),
            Ft8SlotCutter.NoOffset,
            clock: ClockOffset.Unknown);

        _output.WriteLine(sheet);

        Assert.Contains(
            "slotGrid   " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.Contains(
            "wholeSlots " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);

        // The refusal is the operator's own sentence, kept word for word.
        Assert.Contains(Ft8SlotCutter.NoOffset, sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE CENSUS, PER SLOT.** The four numbers that name the stage that refused,
    /// which the join used to throw away.
    /// </summary>
    [Fact]
    public void TheCensusNamesTheStageEachSlotReached()
    {
        var sheet = Compose(
            null,
            new[]
            {
                new Ft8SlotCensus(
                    new DateTime(2026, 9, 3, 14, 22, 45, DateTimeKind.Utc),
                    17, 0, 0, 0, 0, new[] { 34, 28, 22 }, 48000),
            },
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains(
            "candidates 17  parity 0  checksum 0  text 0  duplicate 0",
            sheet,
            StringComparison.Ordinal);
        Assert.Contains("at 48000 Hz", sheet, StringComparison.Ordinal);

        // A Costas match count is not a signal-to-noise ratio (CLAUDE.md 0.0), and
        // the sheet's own snr column stayed a dash for the same reason.
        Assert.Contains(
            "top Costas match counts 34, 28, 22", sheet, StringComparison.Ordinal);
        Assert.DoesNotContain("snr", sheet, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A sheet composed by a caller that decoded nothing says the census was never
    /// handed over, rather than showing zeroes that would read as measurements.
    /// </summary>
    [Fact]
    public void ASheetWithNoDecodeBehindItSaysTheCensusWasNotRead()
    {
        var sheet = Compose(null, null, string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains(
            "census     " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.Contains("refusal    none", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **AND HOW LOUD THE AUDIO IN EACH SLOT WAS** (unit 236). Every other figure
    /// on a census row describes the decode, so a press taken while a sound card
    /// handed over silence produced a sheet identical to one taken on a quiet band.
    /// </summary>
    [Fact]
    public void TheSheetSaysHowLoudTheAudioInEachSlotWas()
    {
        var sheet = Compose(
            null,
            new[]
            {
                new Ft8SlotCensus(
                    new DateTime(2026, 9, 3, 14, 22, 45, DateTimeKind.Utc),
                    140, 44, 41, 40, 1, new[] { 51 }, 12000)
                {
                    Level = new Ft8SlotLevel(-2.0541, -14.1684, 180_000, 13),
                },
            },
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains("peak -2.05", sheet, StringComparison.Ordinal);
        Assert.Contains("rms -14.17", sheet, StringComparison.Ordinal);
        Assert.Contains("samples 180000", sheet, StringComparison.Ordinal);
        Assert.Contains("exactly zero 13", sheet, StringComparison.Ordinal);

        // A level says how loud the audio was, not how strong a signal in it was,
        // and the row says so where the operator will read it (CLAUDE.md 0.0).
        Assert.Contains(
            "dB relative to full scale, NOT a signal-to-noise ratio",
            sheet,
            StringComparison.Ordinal);
        Assert.DoesNotContain("snr", sheet, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **A SLOT OF DIGITAL SILENCE REFUSES ON PAPER TOO** (HM-DEC-009). A floor in
    /// place of a level is a plausible number, and the operator is reading this
    /// sheet at the radio to decide what to change.
    /// </summary>
    [Fact]
    public void ASlotOfDigitalSilenceSaysSoRatherThanShowingAFloor()
    {
        var sheet = Compose(
            null,
            new[]
            {
                new Ft8SlotCensus(
                    new DateTime(2026, 9, 3, 14, 22, 45, DateTimeKind.Utc),
                    0, 0, 0, 0, 0, Array.Empty<int>(), 48000)
                {
                    Level = new Ft8SlotLevel(null, null, 720_000, 720_000),
                },
            },
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains(
            "peak and rms none - every sample in this slot was exactly zero",
            sheet,
            StringComparison.Ordinal);
        Assert.Contains("exactly zero 720000", sheet, StringComparison.Ordinal);
        Assert.Contains("(1.000000 of the slot)", sheet, StringComparison.Ordinal);

        // Not minus ninety, and not any other number a reader would average.
        Assert.DoesNotContain("peak -90", sheet, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A SLOT NOBODY MEASURED AND A SLOT MEASURED AT NOUGHT ARE DIFFERENT
    /// FACTS** (`CLAUDE.md` §0.0). Collapsing them is how the record came to be
    /// unable to tell a dead sound card from a quiet band in the first place.
    /// </summary>
    [Fact]
    public void ACensusWithNoAudioBehindItSaysTheLevelWasNotRead()
    {
        var sheet = Compose(
            null,
            new[]
            {
                new Ft8SlotCensus(
                    new DateTime(2026, 9, 3, 14, 22, 45, DateTimeKind.Utc),
                    17, 0, 0, 0, 0, new[] { 34 }, 48000),
            },
            string.Empty);

        _output.WriteLine(sheet);

        Assert.Contains(
            "level " + DigitalCaptureSheet.Unread, sheet, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "every sample in this slot was exactly zero",
            sheet,
            StringComparison.Ordinal);
    }
}
