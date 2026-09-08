using System;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 280, task 7: a slot he transmitted in says so, and does not
/// report a search that never happened.
/// </summary>
/// <remarks>
/// <para>**OBSERVED 2026-09-08**: *the slot at 15:16:45 UTC was decoded and the
/// search found no place in it that looked like the start of an FT8 transmission,
/// so nothing reached the decoder at all, read by Ft8Sharp.Deep with fine sync and
/// ordered statistics* — while the slots either side decoded `CQ K5MGY EM12` at -22
/// and `CQ W3YNI FN00` at +2.</para>
/// <para>**HAMLET SUSPENDS DECODING WHILE THE RADIO IS TRANSMITTING** (HM-DEC-147),
/// so a slot he keyed produces no decodes **by design**. Reporting a candidate count
/// for it is a claim about the band drawn from a measurement nobody took (§0.0), and
/// it is the exact shape this project exists to prevent: a confident answer with no
/// signal behind it.</para>
/// <para>**NOTHING NEEDED MEASURING.** The transmission is already in
/// `ft8_transmission` telemetry with its `slotStartUtc`.</para>
/// </remarks>
public sealed class ASlotHeTransmittedInSaysSoTests
{
    private static readonly DateTime Slot =
        new(2026, 9, 8, 15, 16, 45, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the census lines are printed.</param>
    public ASlotHeTransmittedInSaysSoTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A slot he transmitted in says so, and makes no search claim.**
    /// </summary>
    [Fact]
    public void ASlotHeUsedSaysSoAndClaimsNoSearch()
    {
        var panel = Panel();

        panel.RememberTransmittedSlotForTests(Slot);

        var line = panel.CensusForTests(Slot, candidates: 0);

        _output.WriteLine("transmitted slot : " + line);

        Assert.Contains("was yours", line, StringComparison.Ordinal);
        Assert.Contains("did not listen", line, StringComparison.Ordinal);

        // **NO SEARCH RESULT AND NO CANDIDATE COUNT.** It was not searched, so
        // there is nothing to count and nothing to report about the band.
        Assert.DoesNotContain("candidate", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("search", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("nothing reached", line, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**A genuinely quiet slot is a number.**</summary>
    /// <remarks>
    /// Zero candidates reads as zero candidates. The sentence around it said the
    /// same thing in twenty-eight words, four times a minute, at somebody working a
    /// station.
    /// </remarks>
    [Fact]
    public void AQuietSlotIsANumber()
    {
        var panel = Panel();

        var line = panel.CensusForTests(Slot, candidates: 0);

        _output.WriteLine("quiet slot       : " + line);

        Assert.Equal("15:16:45 UTC · 0 candidates", line);

        // And it does not claim he was transmitting, which he was not.
        Assert.DoesNotContain("yours", line, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The stage counts survive; the sentence around them does not.**
    /// </summary>
    /// <remarks>
    /// **REMOVING WORDS MUST NOT REMOVE FACTS.** Every number the old line carried
    /// is still on the new one: candidates found, codewords among them, checksums
    /// among those.
    /// </remarks>
    [Fact]
    public void EveryNumberSurvivesTheSentenceGoing()
    {
        var panel = Panel();

        var noCodewords = panel.CensusForTests(Slot, candidates: 7);
        var noChecksums = panel.CensusForTests(Slot, candidates: 7, codewords: 3);
        var noneRead = panel.CensusForTests(Slot, candidates: 7, codewords: 3, checksums: 2);

        _output.WriteLine("no codewords     : " + noCodewords);
        _output.WriteLine("no checksums     : " + noChecksums);
        _output.WriteLine("none read        : " + noneRead);

        Assert.Equal("15:16:45 UTC · 7 candidates · 0 codewords", noCodewords);
        Assert.Equal("15:16:45 UTC · 7 candidates · 3 codewords · 0 checksums", noChecksums);
        Assert.Equal("15:16:45 UTC · 7 candidates · 2 checksums · 0 read", noneRead);

        // **AND THE DECODER'S NAME AND STAGE LIST ARE OFF THE LINE.** They belong
        // in the sidecar and on hover, not repeated four times a minute.
        foreach (var line in new[] { noCodewords, noChecksums, noneRead })
        {
            Assert.DoesNotContain("read by", line, StringComparison.Ordinal);
            Assert.DoesNotContain("Ft8Sharp", line, StringComparison.Ordinal);
        }
    }

    /// <summary>A panel with his callsign and nothing connected.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";

        return new MainWindowViewModel(settings, null);
    }
}
