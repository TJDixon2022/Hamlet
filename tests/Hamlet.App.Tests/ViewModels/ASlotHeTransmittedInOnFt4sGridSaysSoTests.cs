using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// HM-DEC-147 on a 7.5-second grid: a slot he keyed still says he keyed it, and the
/// key that decides that still matches.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 290, TASK 5 - STEP 2'S THIRD EXIT CRITERION.** Unit 282
/// built this for FT8 after 2026-09-08, when the line said *the slot at 15:16:45 UTC
/// was decoded and the search found no place in it that looked like the start of an
/// FT8 transmission* about a slot Hamlet had spent transmitting in, while the slots
/// either side decoded two stations. Hamlet suspends decoding while the radio is
/// keyed (HM-DEC-147), so that slot produced nothing **by design**, and describing a
/// deliberate suspension as a search that found nothing is a claim about the band
/// drawn from a measurement nobody took (§0.0).</para>
/// <para>**THE BREAKAGE THIS CATCHES IS THAT SAME BREACH REAPPEARING ON THE NEW
/// GRID.** <c>_transmittedSlots</c> is a <c>HashSet&lt;DateTime&gt;</c>, and on a
/// 7.5-second grid its members are no longer whole seconds. A set whose members are
/// computed by two paths that round differently misses silently: no exception, no
/// warning, and a line that reads exactly like the one HM-DEC-147 was written to
/// stop.</para>
/// <para>**NOTHING ACTS ON THE SET AND NOTHING HERE MAKES IT.** It is a reading and
/// one line of prose changes.</para>
/// </remarks>
public sealed class ASlotHeTransmittedInOnFt4sGridSaysSoTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the census lines are printed.</param>
    public ASlotHeTransmittedInOnFt4sGridSaysSoTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Minute =
        new(2026, 9, 9, 15, 16, 0, DateTimeKind.Utc);

    /// <remarks>
    /// **THE SLOT AT `:07.5`, WHICH IS THE ONE NO WHOLE-SECOND KEY COULD HOLD.** It
    /// is asked for by a moment inside it rather than typed, so the key is the one
    /// the cutter and the watch actually produce.
    /// </remarks>
    [Fact]
    public void ASlotHeUsedOnTheSevenAndAHalfSecondGridSaysSoAndClaimsNoSearch()
    {
        var slot = SlotGrid.Ft4.SlotStart(Minute.AddSeconds(9.0));

        Assert.Equal(Minute.AddSeconds(7.5), slot);
        Assert.NotEqual(0, slot.Ticks % TimeSpan.TicksPerSecond);

        var panel = Panel();

        panel.RememberTransmittedSlotForTests(slot);

        var line = panel.CensusForTests(slot, candidates: 0);

        _output.WriteLine("  transmitted FT4 slot : " + line);

        Assert.Contains("was yours", line, StringComparison.Ordinal);
        Assert.Contains("did not listen", line, StringComparison.Ordinal);

        // **NO SEARCH RESULT AND NO CANDIDATE COUNT.** It was not searched, so there
        // is nothing to count and nothing to report about the band.
        Assert.DoesNotContain("candidate", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("search", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("nothing reached", line, StringComparison.OrdinalIgnoreCase);

        // And the stamp carries the half second, so he can match the line against a
        // capture rather than against a boundary half a second from where it fell.
        Assert.Contains("15:16:07.5", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **THE FT8 CONTROL, BESIDE IT.** The same sentence, the same stamp format it
    /// has always had, on a slot at a whole second.
    /// </remarks>
    [Fact]
    public void AnFt8SlotHeUsedSaysSoWithTheStampItAlwaysHad()
    {
        var slot = SlotGrid.Ft8.SlotStart(Minute.AddSeconds(50.0));

        Assert.Equal(Minute.AddSeconds(45), slot);

        var panel = Panel();

        panel.RememberTransmittedSlotForTests(slot);

        var line = panel.CensusForTests(slot, candidates: 0);

        _output.WriteLine("  transmitted FT8 slot : " + line);

        Assert.Contains("15:16:45 UTC was yours", line, StringComparison.Ordinal);
        Assert.DoesNotContain("15:16:45.", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// **THE KEY IS THE WHOLE OF THIS CRITERION.** Two paths reach the same slot -
    /// one from the moment the transmission went out, one from the moment the slot's
    /// audio was read - and if they disagree by a tick the set misses and the §0.0
    /// breach comes straight back. This drives every tenth of a second across four
    /// FT4 slots and asserts the set answers correctly at all of them.
    /// </remarks>
    [Fact]
    public void TheKeyStillMatchesWhenTwoPathsComputeItFromDifferentMoments()
    {
        var panel = Panel();

        // The transmit side books the slot from the moment the carrier went up.
        var keyed = SlotGrid.Ft4.SlotStart(Minute.AddSeconds(7.5 + 0.5));

        panel.RememberTransmittedSlotForTests(keyed);

        var hits = new List<double>();
        var misses = new List<double>();

        // The receive side reaches the same slot from every moment inside it.
        for (var tenths = 0; tenths < 300; tenths++)
        {
            var moment = Minute.AddTicks(tenths * (TimeSpan.TicksPerSecond / 10));
            var fromAudio = SlotGrid.Ft4.SlotStart(moment);

            var line = panel.CensusForTests(fromAudio, candidates: 0);
            var saysYours = line.Contains("was yours", StringComparison.Ordinal);
            var inTheKeyedSlot = fromAudio == keyed;

            (saysYours == inTheKeyedSlot ? hits : misses)
                .Add((moment - Minute).TotalSeconds);
        }

        _output.WriteLine($"  {hits.Count} moments answered correctly, "
            + $"{misses.Count} did not");

        if (misses.Count > 0)
        {
            _output.WriteLine("  missed at: "
                + string.Join(", ", misses.Take(10).Select(m => m.ToString("0.0"))));
        }

        Assert.Empty(misses);
        Assert.Equal(300, hits.Count);

        // 75 of those 300 tenths are inside the keyed slot, and exactly those say so.
        var yours = 0;

        for (var tenths = 0; tenths < 300; tenths++)
        {
            var moment = Minute.AddTicks(tenths * (TimeSpan.TicksPerSecond / 10));

            if (panel.CensusForTests(SlotGrid.Ft4.SlotStart(moment), candidates: 0)
                .Contains("was yours", StringComparison.Ordinal))
            {
                yours++;
            }
        }

        Assert.Equal(75, yours);
    }

    /// <remarks>
    /// **A SLOT HE DID NOT KEY IS STILL A NUMBER, ON EITHER GRID.** The set is a
    /// reading and it answers no for everything nobody put in it - it does not
    /// spread to neighbouring slots because their starts are half a second apart.
    /// </remarks>
    [Fact]
    public void TheNeighbouringFt4SlotsAreNotClaimedAsHis()
    {
        var panel = Panel();
        var keyed = Minute.AddSeconds(7.5);

        panel.RememberTransmittedSlotForTests(keyed);

        foreach (var second in new[] { 0.0, 15.0, 22.5, 30.0 })
        {
            var line = panel.CensusForTests(Minute.AddSeconds(second), candidates: 0);

            _output.WriteLine($"  :{second,-5:0.0} {line}");

            Assert.DoesNotContain("yours", line, StringComparison.Ordinal);
            Assert.Contains("0 candidates", line, StringComparison.Ordinal);
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
