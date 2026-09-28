using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 2 item 1, criterion 7.5: the preamp's rule is carried by
/// what is written, which since R98 (work instruction 486) is off.
/// </summary>
/// <remarks>
/// <para>**THE CONDITION'S TEXT IS THE SPECIFICATION**: since R98 (HM-DEC-191,
/// 2026-09-28), *off*, the owner's ruling for Morse. Before it, under HM-DEC-177, the
/// row was a `"condition": "band"` rule, preamp 1 from 1.8 to 29.999 MHz and preamp 2
/// at 50 MHz, and these facts pinned preamp 1 at both frequencies. The row is now a
/// plain constant, so the value written is off at both of task 1's frequencies, and a
/// radio already off is left alone.</para>
/// <para>**THIS WAS NOT WATCHED FAILING.** The instruction's premise, that the
/// rule lives in prose and in no value, was checked against the tree and is not
/// so. These facts pin the setup to the row's own value, so a later change to the
/// row or to the resolver cannot quietly write something the row does not say.</para>
/// </remarks>
public sealed class ThePreampFollowsItsOwnTextTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the writes are printed.</param>
    public ThePreampFollowsItsOwnTextTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Above 40 m the preamp is written off, from preamp 1 and from preamp 2 (R98, work
    /// instruction 486; under HM-DEC-177 it was written to 1).
    /// </summary>
    /// <param name="start">Where the preamp was.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task At14050ThePreampIsWrittenOff(byte start)
    {
        var written = await TuneInAsync(14_050_000, start);

        Assert.Equal(new[] { 0 }, written);
    }

    /// <summary>
    /// At 40 m the preamp is written off where it was at 1 or 2, and nothing is sent
    /// where it was already off.
    /// </summary>
    /// <param name="start">Where the preamp was.</param>
    /// <param name="expected">What is written, empty for nothing.</param>
    /// <remarks>
    /// **THE TEXT HAS CHANGED TWICE.** Until HM-DEC-177 the row read *preamp 1 above
    /// 40 m, off at 40 m and below* and this fact pinned off at 7.030; HM-DEC-177 (work
    /// instruction 424) carried the radio's manual, preamp 1 from 1.8 to 29.999 MHz, and
    /// the fact pinned preamp 1; R98 (work instruction 486) rules it off for Morse on
    /// every band, and the fact pins off, so it still holds the setup to the row's own text.
    /// </remarks>
    [Theory]
    [InlineData(1, new[] { 0 })]
    [InlineData(2, new[] { 0 })]
    [InlineData(0, new int[0])]
    public async Task At7030ThePreampIsWrittenOffOrLeftOff(byte start, int[] expected)
    {
        var written = await TuneInAsync(7_030_000, start);

        Assert.Equal(expected, written);
    }

    private async Task<int[]> TuneInAsync(long hz, byte start)
    {
        var radio = ModeEntryBench.AsLeft(hz, data: false);
        radio.Switches[ModeEntryBench.Preamp] = start;
        using var rig = await ModeEntryBench.ConnectAsync(radio);

        var preamp = ReceiverConditions.ForMode("CW").Where(c => c.Field == RigField.Preamp).ToList();
        await ReceiverSetup.ApplyAsync(rig, preamp, ReceiverSetupMemory.Empty);

        var written = ModeEntryBench.Writes(radio)
            .Where(w => w.Field == RigField.Preamp)
            .Select(w => w.Value)
            .ToArray();

        _output.WriteLine(
            $"{hz / 1e6:0.000} MHz, preamp {start} before: wrote [{string.Join(",", written)}], "
            + $"radio now preamp {radio.Switches[ModeEntryBench.Preamp]}");

        return written;
    }
}
