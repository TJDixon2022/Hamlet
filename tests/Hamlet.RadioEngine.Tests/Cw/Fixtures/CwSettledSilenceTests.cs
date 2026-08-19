using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Why the settled pass is silent, named rather than measured as a share.
/// </summary>
/// <remarks>
/// <para>**IT IS NOT READING WORSE. ON TWO OF THREE PROVED FIXTURES IT IS BARELY
/// READING AT ALL.** `DECODER_AND_SCANNER_BRIEF.md` phase 4 records the settled
/// pass at 15 characters with 73% unresolved on `exchange-easy`. Measured on
/// 2026-08-19 it emits **three**, against the leading edge's twenty-eight, and on
/// `coverage-easy` it emits **none** against twenty-eight. The leading edge reads
/// both perfectly.</para>
/// <para>**AND THE TWO FIXTURES FAIL DIFFERENTLY**, which is the finding this
/// file exists to keep. On `exchange-easy` the pass refuses with `Clock` and the
/// tracker's keying verdict is false, so it never gets as far as reading. On
/// `coverage-easy` it refuses nothing, fits a hundred millisecond dit — twelve
/// words a minute, which is right — measures 23.6 dB of contrast, passes the
/// keying gate, **and hands back an empty window.** A pass that reports it read
/// and produces nothing is not a sensitivity problem; it is a contradiction, and
/// nothing in the suite could see it because everything measured shares of what
/// was emitted.</para>
/// <para>Not closed. The lead is the one the brief names: the reference
/// de-glitches a second time at 0.4 of a dit and re-reads every run, and this
/// pass reads once. That is its own phase and it is not this one (HM-OPEN-048).
/// **What is here is the instrument**, so the next attempt starts from a reason
/// rather than from a percentage.</para>
/// </remarks>
public sealed class CwSettledSilenceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reasons are printed.</param>
    public CwSettledSilenceTests(ITestOutputHelper output) => _output = output;

    private sealed record Reading(
        int Tip, int Settled, int Follows, SettledRefusal Refusal,
        double DitMs, double ContrastDb, bool Keying, int Gaps, bool Classes);

    private Reading Measure(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);

        var tip = 0;
        var settled = 0;

        decoder.CharacterDecoded += _ => tip++;
        decoder.CharacterSettled += _ => settled++;

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var reading = new Reading(
            tip, settled, decoder.Tracker.Follows,
            decoder.SettledState.Refusal,
            decoder.SettledState.DitMilliseconds,
            decoder.SettledState.ContrastDb,
            decoder.Tracker.KeyingRecently,
            decoder.SettledGapsRemembered,
            decoder.GapClasses is not null);

        _output.WriteLine(
            $"{name}: tip {reading.Tip}, settled {reading.Settled}, "
            + $"follows {reading.Follows}, last refusal {reading.Refusal}, "
            + $"dit {reading.DitMs:F0} ms, contrast {reading.ContrastDb:F1} dB, "
            + $"keying {reading.Keying}, gaps {reading.Gaps}, "
            + $"classes {(reading.Classes ? "fitted" : "none")}");

        return reading;
    }

    /// <remarks>
    /// **THE CONTRADICTION, AND THE ONE ASSERTION HERE THAT BITES.** A pass that
    /// refused nothing, fitted a clock and passed the keying gate has read a
    /// window; if it then emits nothing, the fault is in extracting characters
    /// from a window it says it read, and that is a different repair from making
    /// it more sensitive. Failing on it is what keeps the next session from
    /// chasing a percentage.
    /// </remarks>
    [Fact]
    public void APassThatReadSomethingEmitsSomething()
    {
        var reading = Measure("coverage-easy");

        Assert.True(
            reading.Tip > 0,
            "the leading edge read nothing, so this fixture proves nothing today");

        if (reading.Refusal != SettledRefusal.None || !reading.Keying)
        {
            // It refused, and a refusal is an answer. The other test carries it.
            return;
        }

        // **THE MECHANISM, FOUND 2026-08-19 AND NOT REPAIRED HERE.** The window
        // reads: two hundred and fifty-eight of them returned `None` on this
        // fixture. `Emit` then asks for the sender's gap classes and returns
        // without a character when there are none (HM-DEC-115: no cuts means no
        // transcript, not a guessed one). There are eighty gaps to cluster, far
        // past the ten `CwGapFit` needs, **and the fit refuses because three
        // heaps cannot be found** — this message leaves almost no word gaps, so
        // the top class comes back empty and the fit returns null.
        //
        // What to do about a sender who leaves too few word gaps to form a third
        // heap is not a session's to decide: it changes what a transcript
        // asserts about where the words are (§12.1, HM-OPEN-048).
        Assert.True(
            reading.Settled > 0,
            $"the settled pass refused nothing, fitted a {reading.DitMs:F0} ms dit "
            + $"at {reading.ContrastDb:F1} dB and passed the keying gate, had "
            + $"{reading.Gaps} gaps to cluster and "
            + $"{(reading.Classes ? "fitted classes" : "fitted no classes")}, and "
            + "then emitted no characters at all. A window it says it read has to "
            + "produce something or say why (§0.0.1)");
    }

    /// <remarks>
    /// Records the other shape without asserting an improvement: on
    /// `exchange-easy` the pass never gets as far as reading, because the clock is
    /// refused and the keying verdict is false at the end of the recording. The
    /// numbers are printed so a session that changes this can see which of the two
    /// it moved.
    /// </remarks>
    [Fact]
    public void TheRefusalIsNamedRatherThanCountedAsAShare()
    {
        var reading = Measure("exchange-easy");

        Assert.True(reading.Tip > 0);

        Assert.True(
            reading.Refusal != SettledRefusal.None
            || !reading.Keying
            || reading.Settled > 0,
            "either the pass says why it refused, or it produces characters");
    }
}
