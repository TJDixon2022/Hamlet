using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Work instruction 411 task 1, criterion 6.1: the RF gain banner states what the
/// radio read back, and says it does not know only when it does not.
/// </summary>
/// <remarks>
/// <para>**TIM PHOTOGRAPHED THE BANNER ON 2026-09-23** reading *I asked for the RF
/// gain to be 100% and the radio did not confirm it, so I do not know where it is
/// now*, while the radio-state dialog in the same session showed RF gain 100% read
/// back over `CI-V 14 02` twenty-four seconds earlier. The app was stating as
/// unknown a fact it held, which is §0.0 in reverse (HM-DEC-170).</para>
/// <para>**WHY IT HAPPENS, AS THE TRACE BELOW PRINTS IT.** The CW row wants the RF
/// gain at 255, on the radio's own scale, and the read-back arrives as a percent,
/// 100. The two never compare equal, so every tune-in writes 255 and then files a
/// read-back of exactly what was asked for as not confirmed. **The comparison is
/// not repaired here**: repairing it changes what is sent to the radio, and step 6
/// changes only what the operator reads. It is raised in the unit's report.</para>
/// </remarks>
public sealed class TheBannerSaysWhatTheRadioReadBackTests
{
    private const byte RfGain = 0x02;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace and the sentences are printed.</param>
    public TheBannerSaysWhatTheRadioReadBackTests(ITestOutputHelper output)
        => _output = output;

    private static IReadOnlyList<ReceiverCondition> CwBlock()
        => NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .Select(ReceiverConditions.ForBlock)
            .First(c => c.Any(r => r.Field == RigField.RfGain));

    private static async Task<(ScriptedRadio Radio, Ic7300Rig Rig)> ConnectAsync(int rfGain)
    {
        var radio = new ScriptedRadio { FrequencyHz = 14_030_000 };
        radio.Levels[RfGain] = rfGain;

        var rig = new Ic7300Rig(radio);
        Assert.True(await rig.ConnectAsync());

        return (radio, rig);
    }

    /// <summary>
    /// **THE TRACE, AND IT ASSERTS NOTHING.** What the banner's code holds about
    /// the RF gain at the moment it writes the sentence, and what the read-back
    /// holds, for a radio already at full gain and for one at 42 percent.
    /// </summary>
    [Theory]
    [InlineData(255)]
    [InlineData(108)]
    public async Task TraceWhatTheBannerCanSee(int rawAtStart)
    {
        var (radio, rig) = await ConnectAsync(rawAtStart);
        using var _ = rig;

        var condition = CwBlock().Single(c => c.Field == RigField.RfGain);
        var before = (await rig.ReadAsync(RigField.RfGain, RigState.Empty)).Single();

        _output.WriteLine($"radio's RF gain at start, raw {rawAtStart}");
        _output.WriteLine(
            $"condition: control={condition.Control} wanted={condition.Wanted} "
            + $"wantedText={condition.WantedText}");
        _output.WriteLine(
            $"read before: number={before.Number} text={before.Text} "
            + $"at={before.AtUtc:HH:mm:ss.fff} source={before.Source}");

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, new[] { condition }, ReceiverSetupMemory.Empty);

        var after = (await rig.ReadAsync(RigField.RfGain, RigState.Empty)).Single();
        var result = results.Single();

        _output.WriteLine(
            $"writes to 14 02: {string.Join(", ", radio.LevelWrites.Select(w => w.Value))}");
        _output.WriteLine(
            $"read-back now: number={after.Number} text={after.Text} "
            + $"at={after.AtUtc:HH:mm:ss.fff} source={after.Source}");
        _output.WriteLine(
            $"result: outcome={result.Outcome} was={result.WasText ?? "-"} "
            + $"now={result.NowText ?? "-"} nowAt={result.NowAtUtc:HH:mm:ss.fff}");
        _output.WriteLine($"banner: {ReceiverSetupVoice.Admissions(results)}");
    }

    /// <summary>
    /// **THE CRITERION.** With a read-back held for the RF gain, the banner says
    /// the value and when it was read, and does not say the radio did not confirm.
    /// </summary>
    [Theory]
    [InlineData(255)]
    [InlineData(108)]
    public async Task AHeldReadBackIsStatedWithItsTime(int rawAtStart)
    {
        var (_, rig) = await ConnectAsync(rawAtStart);
        using var _ = rig;

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, CwBlock().Where(c => c.Field == RigField.RfGain).ToList(),
            ReceiverSetupMemory.Empty);

        var banner = ReceiverSetupVoice.Admissions(results);
        var said = ReceiverSetupVoice.Say(results);

        _output.WriteLine($"banner: {banner}");
        _output.WriteLine($"hover:  {said}");

        foreach (var sentence in new[] { banner, said })
        {
            Assert.DoesNotContain("did not confirm", sentence, StringComparison.Ordinal);
            Assert.DoesNotContain("I do not know where it is", sentence, StringComparison.Ordinal);
            Assert.Contains("read it back as 100%", sentence, StringComparison.Ordinal);
            Assert.Matches(new Regex(@"read it back as 100% at \d\d:\d\d:\d\d"), sentence);
        }
    }

    /// <summary>
    /// **AND WHERE NO READ-BACK IS HELD THE OLD SENTENCE STANDS, WORD FOR WORD.**
    /// That case is true and is not this unit's to touch.
    /// </summary>
    [Fact]
    public void WithNoReadBackTheOldSentenceStandsUnchanged()
    {
        var condition = CwBlock().Single(c => c.Field == RigField.RfGain);

        var banner = ReceiverSetupVoice.Admissions(new[]
        {
            new ConditionResult(condition, ConditionOutcome.NotConfirmed, "42%"),
        });

        _output.WriteLine($"banner: {banner}");

        Assert.Equal(
            "I asked for the RF gain to be 100% and the radio did not confirm it, "
            + "so I do not know where it is now.",
            banner);
    }
}
