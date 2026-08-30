using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The readings somebody has adjudicated still read. **These are the first
/// tests in this repository that fail when a repair breaks a success.**
/// </summary>
/// <remarks>
/// <para>**EVERY OTHER RATCHET IN THIS SUITE GUARDS A FAILURE GETTING LESS
/// BAD** — the settled pass reaching further into a callsign, a tier coming back
/// with fewer strangers, a floor on how many characters a recording produces.
/// Not one of them asserts that something Hamlet reads *correctly* today is
/// still read correctly tomorrow, so nothing in the suite could tell a repair
/// from a coincidence. HM-OPEN-026 named that gap and had no candidate to fill
/// it; the W1AW seven adjudicated on 2026-08-25 fill it, alongside the three
/// callsigns already ruled.</para>
/// <para>**WHAT IS ASSERTED IS A RUN, NOT A LINE, AND THAT IS THE HONEST SHAPE
/// OF IT.** Not one of the seven bulletin lines is read whole. Asserting a whole
/// line would be asserting a failure, and asserting nothing would be the gap
/// again. So each capture carries **the longest unbroken run of its own
/// adjudicated text that the decoder produced on the day the anchor was set**,
/// and the anchor only ever grows. A run of three or four characters is a weak
/// guard and is marked as one below rather than dressed up.</para>
/// <para>**THE STARTING PITCH IS 600 HERTZ BECAUSE THAT IS WHAT THE OPERATOR'S
/// RADIO WAS SET TO.** Every one of these captures records `CwPitch 600 Hz` in
/// its own sidecar, and `MainWindowViewModel` hands the decoder
/// `_settings.CwPitchHz`, so 600 is what production starts from. It matters:
/// started instead at each station's own recorded note, `cw-2026-08-22-032113`
/// reads 22 characters of its line rather than 4 and `cw-2026-08-18-003758`
/// gives back the whole of `AA4MP/4 QNIK` rather than nine twelfths of it.
/// **`ANALYSIS-cw-emit-decision-2026-08-24.md` is written at the station's note
/// and therefore reads better than the operator does**, which is worth knowing
/// before quoting it.</para>
/// <para>The drive is hop by hop through <see cref="CwDecoder.Process"/>, the
/// same path `TheCapturesThatDecodeKeepDecoding` uses, so a floor and an anchor
/// on one recording are measured through one instrument (HM-DEC-119).</para>
/// </remarks>
public sealed class TheAdjudicatedReadingsKeepReadingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheAdjudicatedReadingsKeepReadingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// What the operator's radio was set to on every capture here, and what the
    /// application hands the decoder.
    /// </summary>
    public const double RadioPitchHz = 600;

    /// <summary>
    /// One adjudicated reading, and the run of it the decoder gives back.
    /// </summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Adjudicated">The text somebody ruled the station sent.</param>
    /// <param name="Anchor">
    /// The longest unbroken run of that text the decoder produced when this was
    /// written. **A floor, never an answer key**: it may grow and may not shrink.
    /// </param>
    /// <param name="Ruling">Who says so.</param>
    /// <param name="Retired">
    /// Why this reading is no longer required of the decoder, or "" while it is
    /// still required. **A retired anchor is re-expressed and never deleted**
    /// (work instruction 051, task 6; the pattern unit 036 set): the recording,
    /// the text and the reason all stay, so what was given up is visible and the
    /// day it can be asked for again is a change to one field.
    /// </param>
    public readonly record struct Reading(
        string Name, string Adjudicated, string Anchor, string Ruling,
        string Retired = "");

    /// <summary>Why the four W1AW anchors are retired, in one place.</summary>
    /// <remarks>
    /// <para>**RE-EXPRESSED, NOT LOWERED** (Tim's ruling, 2026-08-30). Unit 051
    /// wired the squelch: nothing is asserted from a pitch the survey has not
    /// admitted a station at. On these four recordings the survey admits partway
    /// through, so the earlier part blocks and the later part reads — and **these
    /// floors were set on text that included the part it was never entitled to
    /// assert.**</para>
    /// <para>**HAMLET IS NOT READING THESE BULLETINS WORSE.** It is declining to
    /// name letters it had no business naming. Each entry above records what the
    /// capture still reads, so the re-expression carries its own evidence rather
    /// than asking anybody to take it on trust.</para>
    /// <para>**WHAT BRINGS THEM BACK** is admission, not the decoder: when the
    /// survey admits these stations for the whole recording rather than part of
    /// it, the blocked stretch reads and the anchors are restored. That is the
    /// window this unit is about.</para>
    /// </remarks>
    private const string Squelched =
        "the squelch of unit 051 blocks the stretch of this recording where the "
        + "survey has not admitted a station, and this floor was set on text "
        + "that included it (Tim, 2026-08-30)";

    /// <summary>Every reading anybody has adjudicated, with its anchor.</summary>
    /// <remarks>
    /// Anchors measured 2026-08-25 at <see cref="RadioPitchHz"/> through
    /// <see cref="CwDecoder.Process"/>. Leading and trailing spaces are trimmed
    /// off a measured run, which cannot turn a substring into a non-substring.
    /// </remarks>
    public static IReadOnlyList<Reading> All { get; } = new[]
    {
        // The three callsigns, ruled before this unit.
        new Reading(
            "cw-2026-08-17-013347", "VA3VRR", "VA3VRR", "HM-DEC-145"),
        // **RETIRED AS A READING ANCHOR BY TIM'S RULING OF 2026-08-30, AND KEPT
        // HERE ENTIRE.** The station on this recording sits at 500.09 Hz. The old
        // tone tracker's fallback bank centre was 500.0, so the callsign was read
        // because an unmeasured number happened to land within a tenth of a hertz
        // of a station — the decoder's own comment said so before unit 050
        // measured it. `CwSpectralPeak` measures 501.2 and the reading goes.
        //
        // **HM-DEC-144 IS NOT WITHDRAWN.** `N4L` is still what that station sent;
        // what is withdrawn is the requirement that Hamlet read it, because the
        // only way it ever did was by luck. It returns when the peak can find
        // that station honestly, which is task 7's question.
        new Reading(
            "cw-2026-08-17-134712", "N4L", "N4", "HM-DEC-144",
            Retired:
                "the peak measures 501.2 Hz against a station at 500.09, and the "
                + "callsign was only ever read because an unmeasured bank centre "
                + "of 500.0 landed on it (Tim, 2026-08-30)"),
        new Reading(
            "unadjudicated/cw-2026-08-18-003758",
            "AA4MP/4 QNIK", "MP/4 QNIK", "HM-DEC-126"),

        // The control, whose text unit 1.11.8's instruction states.
        new Reading(
            "unadjudicated/cw-2026-08-24-012403",
            "DE KD0UN KD0UN K", "DE KD0UN KD0UN K", "work instruction 011"),

        // The ARRL bulletin, whose text HM-DEC-115 quotes from this recording.
        new Reading(
            "cw-2026-08-18-004507",
            "AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P",
            "N HANDLING THIS MESSAG", "HM-DEC-115"),

        // The seven W1AW lines, adjudicated by Tim on 2026-08-25.
        new Reading(
            "unadjudicated/cw-2026-08-22-031838",
            "2, 2, AND 2 WITH A MEAN OF 2.9. PRE", ", AND", "Tim 2026-08-25"),
        // **RE-EXPRESSED, NOT LOWERED** (Tim, 2026-08-30). Still reads
        // `■ ■■■■ICTED 10.7 K NTIMETER FLAX IS 125, 125T` — the admitted stretch
        // comes through and the anchor `DICTED 10.7` straddles the boundary.
        new Reading(
            "unadjudicated/cw-2026-08-22-031905",
            "DICTED 10.7 CENTIMETER FLUX IS 125, 125",
            "DICTED 10.7", "Tim 2026-08-25", Retired: Squelched),
        new Reading(
            "unadjudicated/cw-2026-08-22-031948",
            "110, 110, AND 110 WITH A MEAN OF 117",
            "110, AND 110 WITH A MEAN OF 117", "Tim 2026-08-25"),
        new Reading(
            "unadjudicated/cw-2026-08-22-032012",
            "N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI",
            "R OTHER WEBSITES MENTI", "Tim 2026-08-25"),
        // Still reads `…■■CKETY AN MT TINTERE`: the last few seconds are
        // admitted and the first twenty-odd are not.
        new Reading(
            "unadjudicated/cw-2026-08-22-032050",
            "THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE",
            "ULLETIN CAN BE FO", "Tim 2026-08-25", Retired: Squelched),
        new Reading(
            "unadjudicated/cw-2026-08-22-032113",
            "ACKET, AND INTERNET VERSIONS", "INT", "Tim 2026-08-25",
            Retired: Squelched),
        // Still reads `…ON FORECAST BUAELETIN ARLP034` where it is admitted,
        // which is most of the bulletin's own name.
        new Reading(
            "unadjudicated/cw-2026-08-22-032129",
            "2026 PROPAGATION FORECAST BULLETIN ARLP034",
            "OPAGATION", "Tim 2026-08-25", Retired: Squelched),
    };

    /// <summary>The same list, as xunit wants it.</summary>
    public static TheoryData<Reading> Readings
    {
        get
        {
            var data = new TheoryData<Reading>();

            foreach (var reading in All)
            {
                data.Add(reading);
            }

            return data;
        }
    }

    /// <summary>Read one capture the way the application would.</summary>
    /// <param name="name">The recording.</param>
    /// <returns>Everything the settled pass said, in order.</returns>
    internal static string Settled(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, RadioPitchHz);
        var text = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => text.Append(c.Text);

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return text.ToString();
    }

    /// <remarks>
    /// Proves the thing HM-OPEN-026 said this suite could not prove: a reading
    /// somebody has ruled on still comes back. It goes red when a repair
    /// elsewhere quietly costs a success, which is the failure every other test
    /// here is blind to.
    /// </remarks>
    /// <param name="reading">The adjudicated reading and its anchor.</param>
    [Theory]
    [MemberData(nameof(Readings))]
    public void EachAdjudicatedReadingStillComesBack(Reading reading)
    {
        var text = Settled(reading.Name);

        if (reading.Retired.Length > 0)
        {
            // **RETIRED, AND SAID OUT LOUD RATHER THAN SKIPPED IN SILENCE.** The
            // recording still runs and what it reads is still printed, so the day
            // it comes back is a day somebody can see in the output rather than
            // one nobody noticed.
            _output.WriteLine(
                $"{reading.Name} ({reading.Ruling}): RETIRED — {reading.Retired}");
            _output.WriteLine($"  it reads: {text}");
            _output.WriteLine(
                $"  it would come back if \"{reading.Anchor}\" appeared: "
                + text.Contains(reading.Anchor, StringComparison.Ordinal));

            return;
        }

        _output.WriteLine(
            $"{reading.Name} ({reading.Ruling}): looking for \"{reading.Anchor}\" "
            + $"— {reading.Anchor.Length} of {reading.Adjudicated.Length} "
            + $"adjudicated characters");
        _output.WriteLine($"  read: {text}");

        Assert.Contains(reading.Anchor, text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves the anchors are anchors and not answer keys, and keeps the
    /// shortfall visible rather than letting a table of green ticks imply the
    /// bulletin is read (§0.0).</para>
    /// <para>**IT ASSERTS NOTHING ABOUT THE SHORTFALL AND IS NOT MEANT TO.** The
    /// only claim it makes is the one nobody can argue with: an anchor is never
    /// longer than the text it is drawn from.</para>
    /// </remarks>
    [Fact]
    public void TheShortfallIsPrintedRatherThanPapered()
    {
        var whole = 0;
        var read = 0;

        foreach (var reading in All)
        {
            Assert.True(
                reading.Anchor.Length <= reading.Adjudicated.Length,
                $"{reading.Name} has an anchor longer than its adjudicated text");

            Assert.Contains(
                reading.Anchor, reading.Adjudicated, StringComparison.Ordinal);

            whole += reading.Adjudicated.Length;
            read += reading.Anchor.Length;

            _output.WriteLine(
                $"{reading.Name}: {reading.Anchor.Length} of "
                + $"{reading.Adjudicated.Length} characters, "
                + $"{100.0 * reading.Anchor.Length / reading.Adjudicated.Length:0} %");
        }

        _output.WriteLine(
            $"across every adjudicated reading: {read} of {whole} characters, "
            + $"{100.0 * read / whole:0} %");
    }
}
