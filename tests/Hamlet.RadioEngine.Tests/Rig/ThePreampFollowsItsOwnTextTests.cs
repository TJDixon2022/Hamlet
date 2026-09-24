using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 419 task 2 item 1, criterion 7.5: the preamp's band rule is
/// carried by what is written.
/// </summary>
/// <remarks>
/// <para>**THE CONDITION'S TEXT IS THE SPECIFICATION**: since HM-DEC-177, *preamp 1
/// from 1.8 to 29.999 MHz, preamp 2 at 50 MHz, and off while the front end reads
/// overloading*. The row names the rule, `"condition": "band"`, and
/// <see cref="ReceiverSetup"/> resolves it against the frequency the radio reports,
/// so the value written is the text's at both of task 1's frequencies.</para>
/// <para>**THIS WAS NOT WATCHED FAILING.** The instruction's premise, that the
/// rule lives in prose and in no value, was checked against the tree and is not
/// so: the setup already derives the value from the frequency. These facts pin
/// that, so a later change to the row or to the resolver cannot quietly go back
/// to writing the stated constant.</para>
/// </remarks>
public sealed class ThePreampFollowsItsOwnTextTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the writes are printed.</param>
    public ThePreampFollowsItsOwnTextTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Above 40 m the preamp is written to 1, from off and from preamp 2.
    /// </summary>
    /// <param name="start">Where the preamp was.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task At14050ThePreampIsWrittenTo1(byte start)
    {
        var written = await TuneInAsync(14_050_000, start);

        Assert.Equal(new[] { 1 }, written);
    }

    /// <summary>
    /// At 40 m the preamp is written to 1 where it was off or at 2, and nothing is sent
    /// where it was already at 1.
    /// </summary>
    /// <param name="start">Where the preamp was.</param>
    /// <param name="expected">What is written, empty for nothing.</param>
    /// <remarks>
    /// **THE TEXT CHANGED UNDER HM-DEC-177** (work instruction 424). Until then the row
    /// read *preamp 1 above 40 m, off at 40 m and below* and this fact pinned off at
    /// 7.030; the row now carries the radio's manual, preamp 1 from 1.8 to 29.999 MHz,
    /// and the fact pins that instead, so it still holds the setup to the row's own text.
    /// </remarks>
    [Theory]
    [InlineData(0, new[] { 1 })]
    [InlineData(2, new[] { 1 })]
    [InlineData(1, new int[0])]
    public async Task At7030ThePreampIsWrittenTo1OrLeftAt1(byte start, int[] expected)
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
