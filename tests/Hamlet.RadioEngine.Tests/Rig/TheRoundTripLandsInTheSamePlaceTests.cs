using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Switching CW to a digital block and back lands in the same place every time.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE WHOLE POINT OF THE OWNED-SETTINGS CONTRACT** (Tim's ruling
/// of 2026-08-29). Two conversations are building against one radio, and each was
/// writing its own set of deltas: whichever ran last won on whatever it happened
/// to touch, and nothing said what became of a setting a mode never mentioned. A
/// row that answers for every owned setting cannot be overwritten by silence.</para>
/// <para>**A SETTING NO ROW STATES IS NOT TOUCHED BY EITHER DIRECTION**, and the
/// gap is in the coverage table rather than filled in by guesswork. That is what
/// lets the other conversation write the digital rows without collision.</para>
/// </remarks>
public sealed class TheRoundTripLandsInTheSamePlaceTests
{
    private const byte AutoNotch = 0x41;
    private const byte NoiseBlanker = 0x22;
    private const byte NoiseReduction = 0x40;
    private const byte Agc = 0x12;

    private readonly ITestOutputHelper _output;

    public TheRoundTripLandsInTheSamePlaceTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Ten round trips leave CW's settings where CW's row states.</summary>
    /// <remarks>
    /// Ten rather than one, because the failure this guards against is drift: a
    /// setting that moves a little on each crossing looks fine once.
    /// </remarks>
    [Fact]
    public async Task TenRoundTripsDoNotDrift()
    {
        var (radio, rig) = await ConnectAsync();
        using var owned = rig;

        var cw = ReceiverConditions.ForBlock(Cw());
        var digital = ReceiverConditions.ForBlock(Ft8());
        var memory = ReceiverSetupMemory.Empty;

        Assert.NotEmpty(cw);
        Assert.NotEmpty(digital);

        (_, memory) = await ReceiverSetup.ApplyAsync(rig, cw, memory);

        var settled = Snapshot(radio);

        _output.WriteLine($"after the first CW tune-in: {Describe(settled)}");

        for (var trip = 0; trip < 10; trip++)
        {
            (_, memory) = await ReceiverSetup.ApplyAsync(rig, digital, memory);
            (_, memory) = await ReceiverSetup.ApplyAsync(rig, cw, memory);
        }

        var after = Snapshot(radio);

        _output.WriteLine($"after ten round trips:      {Describe(after)}");

        Assert.Equal(settled, after);
    }

    /// <summary>
    /// The settings CW states are what CW's row says after the round trip.
    /// </summary>
    /// <remarks>
    /// **AUTO NOTCH IS THE ONE THAT MATTERS.** It hunts steady carriers and a
    /// keyed Morse signal is a steady carrier, so a digital block leaving it on
    /// would eat the thing CW is trying to read.
    /// </remarks>
    [Fact]
    public async Task ComingBackToMorseRestoresWhatMorseNeeds()
    {
        var (radio, rig) = await ConnectAsync();
        using var owned = rig;

        var memory = ReceiverSetupMemory.Empty;

        // The digital block runs first and sets what it states.
        (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Ft8()), memory);

        // Then the operator goes back to Morse.
        (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Cw()), memory);

        Assert.Equal(0, radio.Switches[AutoNotch]);
        Assert.Equal(0, radio.Switches[NoiseBlanker]);
        Assert.Equal(0, radio.Switches[NoiseReduction]);

        // **AGC IS THE ONE THE TWO MODES DISAGREE ABOUT**, which is exactly the
        // setting a partial delta would have left wherever the other mode put
        // it. FT8 wants slow and CW wants fast.
        Assert.Equal(1, radio.Switches[Agc]);
    }

    /// <summary>A setting no row states is not touched by either direction.</summary>
    /// <remarks>
    /// **ABSENT IS NOT THE SAME AS OFF.** The digital rows do not state the
    /// manual notch, so whatever the operator left it at survives a crossing —
    /// and the gap is reported in the coverage table rather than filled in by a
    /// unit that does not own that row.
    /// </remarks>
    [Fact]
    public async Task ASettingNoRowStatesIsLeftAlone()
    {
        var (radio, rig) = await ConnectAsync();
        using var owned = rig;

        // Something no row on either side mentions.
        const byte NotchPosition = 0x0D;

        radio.OperatorTurnsASwitch(NotchPosition, 1);

        var memory = ReceiverSetupMemory.Empty;

        (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Ft8()), memory);
        (_, memory) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Cw()), memory);

        Assert.DoesNotContain(radio.SwitchWrites, w => w.Sub == NotchPosition);
    }

    /// <summary>The auto notch is corrected on entering Morse.</summary>
    /// <remarks>
    /// One of the four states of 2026-08-29 that entering the mode must correct.
    /// </remarks>
    [Fact]
    public async Task AutoNotchLeftOnIsCorrectedOnEnteringMorse()
    {
        var (radio, rig) = await ConnectAsync();
        using var owned = rig;

        radio.OperatorTurnsASwitch(AutoNotch, 1);

        var (results, _) = await ReceiverSetup.ApplyAsync(
            rig, ReceiverConditions.ForBlock(Cw()), ReceiverSetupMemory.Empty);

        foreach (var r in results)
        {
            _output.WriteLine(
                $"  {r.Condition.Control,-16} {r.Outcome,-18} "
                + $"was={r.WasText ?? "-"} now={r.NowText ?? "-"}");
        }

        Assert.Equal(0, radio.Switches[AutoNotch]);
        Assert.Contains((AutoNotch, (byte)0), radio.SwitchWrites);
    }

    private static (int Notch, int Blanker, int Reduction, int Agc) Snapshot(
        ScriptedRadio radio)
        => (radio.Switches[AutoNotch], radio.Switches[NoiseBlanker],
            radio.Switches[NoiseReduction], radio.Switches[Agc]);

    private static string Describe(
        (int Notch, int Blanker, int Reduction, int Agc) s)
        => $"notch={s.Notch} blanker={s.Blanker} reduction={s.Reduction} agc={s.Agc}";

    private static Neighborhood Cw()
        => NeighborhoodPlan.ForBand(HfBands.Bands.First(b => b.Name == "40 m"))
            .First(n => n.ShortName == "CW");

    private static Neighborhood Ft8()
        => NeighborhoodPlan.ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.ShortName == "FT8");

    private static async Task<(ScriptedRadio Radio, Ic7300Rig Rig)> ConnectAsync()
    {
        var radio = new ScriptedRadio { FrequencyHz = 7_030_000 };
        var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        return (radio, rig);
    }
}
