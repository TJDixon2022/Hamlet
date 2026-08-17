using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Naming what is in the passband, and offering the fixes (HM-DEC-096, phase 5).
/// </summary>
public sealed class CwInterferenceNotesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the copy is printed.</param>
    public CwInterferenceNotesTests(ITestOutputHelper output) => _output = output;

    private static readonly ToneInterference Carrier = new(500, 22, 0.52);

    /// <remarks>
    /// Proves HM-DEC-096 phase 5: **nothing measured means nothing said.** A
    /// panel that always has something to report about interference teaches the
    /// operator to stop reading it.
    /// </remarks>
    [Fact]
    public void NothingMeasuredMeansNothingSaid()
    {
        Assert.Equal("", CwInterferenceNotes.Describe(null));
        Assert.Equal("", CwInterferenceNotes.Summarize(null));
        Assert.Empty(CwInterferenceNotes.Fixes(null));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 5 and §0.0: **the copy states a frequency
    /// and a strength and claims nothing else.** Whose carrier it is, what is
    /// making it, and whether removing it will make the station readable are all
    /// outside what the decoder measured.</para>
    /// </remarks>
    [Fact]
    public void TheCopySaysWhatWasMeasuredAndNoMore()
    {
        var passage = CwInterferenceNotes.Describe(Carrier);

        _output.WriteLine(passage);

        Assert.Contains("500 hertz", passage, StringComparison.Ordinal);
        Assert.Contains("22 decibels", passage, StringComparison.Ordinal);

        // Nothing here diagnoses the station, the band, or anybody's equipment.
        foreach (var forbidden in new[]
        {
            "your radio", "faulty", "interference from", "someone is",
            "will fix", "should fix", "power supply", "jamming",
        })
        {
            Assert.DoesNotContain(
                forbidden, passage, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-096 phase 5: the reason it matters is attached to the fact,
    /// because "there is a carrier at 500 hertz" teaches nothing on its own and
    /// the gain control is the whole reason a steady signal in the passband is an
    /// operational problem rather than a curiosity (§0.7).
    /// </remarks>
    [Fact]
    public void TheReasonItMattersIsAttachedToTheFact()
    {
        var passage = CwInterferenceNotes.Describe(Carrier);

        Assert.Contains("gain", passage, StringComparison.OrdinalIgnoreCase);

        // And a fast gain setting is called out when the radio reports one,
        // because it makes the same carrier do more damage.
        var fast = CwInterferenceNotes.Describe(Carrier, "FAST");

        _output.WriteLine(fast);

        Assert.Contains("fast", fast, StringComparison.OrdinalIgnoreCase);
        Assert.True(fast.Length > passage.Length);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 5: **the three fixes are offered with their
    /// commands cited**, so nobody has to recall a sub-command. `14 08` is the
    /// outer passband control and is the byte this project once mistook for the
    /// CW pitch, which is `14 09` (§4, HM-DEC-050).</para>
    /// </remarks>
    [Fact]
    public void TheFixesCarryTheirCitations()
    {
        var fixes = CwInterferenceNotes.Fixes(Carrier);

        foreach (var fix in fixes)
        {
            _output.WriteLine($"{fix.Name}: {fix.Command}");
        }

        Assert.Equal(3, fixes.Count);

        var all = string.Join(" ", fixes.Select(f => f.Command));

        Assert.Contains("16 48", all, StringComparison.Ordinal);
        Assert.Contains("14 0D", all, StringComparison.Ordinal);
        Assert.Contains("14 07", all, StringComparison.Ordinal);
        Assert.Contains("14 08", all, StringComparison.Ordinal);

        // And never the CW pitch, which is the sub-command next door.
        Assert.DoesNotContain("14 09", all, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-096 phase 5: **the automatic notch is not offered as a
    /// fix.** It hunts for whatever is steadiest in the passband, and on a slow
    /// fist that is the Morse, so it removes the thing the operator is trying to
    /// read. It is named once, in the manual notch's explanation, in order to say
    /// why it is the wrong tool.</para>
    /// </remarks>
    [Fact]
    public void TheAutomaticNotchIsNotOfferedAsAFix()
    {
        var fixes = CwInterferenceNotes.Fixes(Carrier);

        Assert.DoesNotContain(
            fixes, f => f.Command.Contains("16 41", StringComparison.Ordinal));

        var manual = fixes.Single(
            f => f.Name.Contains("Manual", StringComparison.Ordinal));

        Assert.Contains("automatic", manual.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves HM-DEC-096 phase 5 and HM-DEC-084: **describing a fix is not
    /// performing one.** Every option here is a receive-side setting Hamlet could
    /// write, and none is written from this code, which holds no radio at all.
    /// </remarks>
    [Fact]
    public void NothingHereTouchesTheRadio()
    {
        var fixes = CwInterferenceNotes.Fixes(Carrier);

        Assert.All(fixes, f => Assert.False(string.IsNullOrWhiteSpace(f.Explanation)));

        // The dial shift is the one with no command at all, which is worth saying
        // out loud: it is the fix that cannot go wrong.
        Assert.Contains(
            fixes, f => f.Command.Contains("no command", StringComparison.Ordinal));
    }
}
