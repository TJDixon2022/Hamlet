using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// What goes on the wire when data territory sets its mode, and what the model
/// is left believing when the radio will not take it.
/// </summary>
/// <remarks>
/// <para>**COMMAND `26` IS USED INSTEAD OF `06` FOR EXACTLY ONE REASON**
/// (HM-DEC-056): it carries the data flag, and command `04` reports USB whether
/// the radio is in USB or USB-D. A frame that reached the right mode without
/// carrying the flag would leave the operator in voice USB with the microphone
/// live where he wanted the computer's audio, and the symptom of that is silence
/// rather than an error.</para>
/// <para>**SO THE FRAME IS ASSERTED AND NOT THE OUTCOME.** A radio that happens
/// to be in the wanted mode already proves nothing about what was sent, which is
/// §12.5's own failure in miniature.</para>
/// <para>Every test here drives the real <see cref="Ic7300Rig"/> against a
/// scripted radio (HM-DEC-093); nothing in this file is evidence about the
/// operator's own set.</para>
/// </remarks>
public sealed class TheDataWriteCarriesItsFlagTests
{
    private readonly ITestOutputHelper _output;

    public TheDataWriteCarriesItsFlagTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The write is VFO selector, mode, then the data flag.</summary>
    /// <remarks>
    /// **THE SELECTOR IS `00`, THE SELECTED VFO** (p. 19-11). Hamlet has no
    /// business writing the mode of a VFO the operator is not listening to.
    /// </remarks>
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task TheFrameCarriesTheSelectorTheModeAndTheFlag(
        bool dataMode, byte flag)
    {
        var radio = new ScriptedRadio();

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        var result = await rig.SetModeAsync(CivMode.Usb, dataMode);

        _output.WriteLine(
            "26 data: "
            + string.Join(" ", radio.LastModeWrite.Select(b => b.ToString("X2"))));

        Assert.True(result.Worked, "the scripted radio refused a mode it has");

        Assert.True(
            radio.LastModeWrite.Count >= 3,
            $"the frame carried only {radio.LastModeWrite.Count} bytes");

        Assert.Equal(0x00, radio.LastModeWrite[0]);
        Assert.Equal((byte)CivMode.Usb, radio.LastModeWrite[1]);
        Assert.Equal(flag, radio.LastModeWrite[2]);
    }

    /// <summary>
    /// Asked for no filter, the frame carries none, and the radio picks its own.
    /// </summary>
    /// <remarks>
    /// **SKIPPING THE BYTE IS NOT LEAVING THE FILTER ALONE** (§4, p. 19-11): the
    /// radio then selects DATA OFF and that mode's default. The first half is why
    /// the flag is always sent; the second is the question the order marks as
    /// Tim's, and this test asserts only what the frame contains.
    /// </remarks>
    [Fact]
    public async Task NoFilterAskedForIsNoFilterByteSent()
    {
        var radio = new ScriptedRadio();

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        await rig.SetModeAsync(CivMode.Usb, dataMode: true);

        Assert.Equal(3, radio.LastModeWrite.Count);
    }

    /// <summary>Asked for one, the frame carries it in the fourth byte.</summary>
    [Fact]
    public async Task AFilterAskedForIsTheFourthByte()
    {
        var radio = new ScriptedRadio();

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        await rig.SetModeAsync(
            CivMode.Usb, dataMode: true, CivWrites.WidestFilterSlot);

        Assert.Equal(4, radio.LastModeWrite.Count);
        Assert.Equal(CivWrites.WidestFilterSlot, radio.LastModeWrite[3]);
    }

    /// <summary>A refused write leaves the mode unknown rather than asked-for.</summary>
    /// <remarks>
    /// <para>**NOTHING IS ASSUMED FROM HAVING SENT IT** (HM-DEC-056). The radio
    /// acknowledges with FB or refuses with FA, and anything else leaves the value
    /// UNKNOWN rather than set to what was asked for.</para>
    /// <para>The failure this guards is the worst shape §0.0 has: a badge reading
    /// USB-D over a radio sitting in CW, which looks exactly like a badge reading
    /// USB-D over a radio in USB-D.</para>
    /// </remarks>
    [Fact]
    public async Task ARefusedWriteLeavesTheModeUnknown()
    {
        var radio = new ScriptedRadio();

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        var seen = new List<RigValue>();

        rig.ValuesReported += (_, e) => seen.AddRange(e.Values);

        radio.RefuseModeWrites = true;

        var result = await rig.SetModeAsync(CivMode.Usb, dataMode: true);

        _output.WriteLine($"refused write: worked {result.Worked}, {result.Detail}");

        Assert.False(result.Worked, "a refused write reported success");

        // **THE FRAME STILL WENT OUT**, so this is a refusal and not a test that
        // passes because nothing was attempted.
        Assert.True(radio.LastModeWrite.Count >= 3, "no frame reached the radio");

        var mode = seen.LastOrDefault(v => v.Field == RigField.Mode);
        var flag = seen.LastOrDefault(v => v.Field == RigField.DataMode);

        Assert.NotNull(mode);
        Assert.NotNull(flag);
        Assert.False(mode!.IsKnown, $"the mode was claimed as {mode.Text}");
        Assert.False(flag!.IsKnown, $"the data flag was claimed as {flag.Text}");
    }

    /// <summary>A write the radio never answers leaves it unknown too.</summary>
    /// <remarks>
    /// Silence and a refusal are different facts about the link and the same
    /// fact about the mode: nobody said what the radio is in.
    /// </remarks>
    [Fact]
    public async Task AnUnansweredWriteLeavesTheModeUnknown()
    {
        var radio = new ScriptedRadio();

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        var seen = new List<RigValue>();

        rig.ValuesReported += (_, e) => seen.AddRange(e.Values);

        radio.AnswerNothing = true;

        var result = await rig.SetModeAsync(CivMode.Usb, dataMode: true);

        Assert.False(result.Worked);

        var mode = seen.LastOrDefault(v => v.Field == RigField.Mode);

        Assert.NotNull(mode);
        Assert.False(mode!.IsKnown, $"the mode was claimed as {mode.Text}");
    }

    /// <summary>
    /// The state of 2026-08-28: tuned to 14.074 and left in Morse behind a five
    /// hundred hertz filter.
    /// </summary>
    /// <remarks>
    /// <para>**THE RADIO WAS CORRECTLY TUNED AND THE OPERATOR HEARD NOTHING FOR
    /// AN HOUR.** FT8 city is three kilohertz wide and every station in it sits
    /// as an audio tone above the dial, so a five hundred hertz window over the
    /// bottom of it passes a sixth of the block, and Morse over a digital signal
    /// resolves to nothing at all.</para>
    /// <para>**PASSING THROUGH DOES NOT FIX IT AND STOPPING THERE DOES** (work
    /// instruction 050, tasks 4 and 5). Both halves are asserted here, because a
    /// rule that never writes would satisfy the first on its own.</para>
    /// </remarks>
    [Fact]
    public async Task TheEveningOfTheTwentyEighthIsPutRightOnlyOnADwell()
    {
        const long Ft8Hz = 14_074_000;

        var radio = new ScriptedRadio { FrequencyHz = Ft8Hz };

        using var rig = new Ic7300Rig(radio);

        Assert.True(await rig.ConnectAsync());

        var hood = NeighborhoodPlan
            .ForBand(HfBands.Bands.First(b => b.Name == "20 m"))
            .First(n => n.Name == "FT8 city");

        var state = ModeFollowState.Armed(true);
        var ledger = RigState.Empty;

        rig.ValuesReported += (_, e) => ledger = ledger.With(e.Values.ToArray());

        ledger = ledger.With((await rig.ReadAsync(RigField.Mode, ledger)).ToArray());

        _output.WriteLine(
            $"  before: {radio.Mode} FIL{radio.FilterSlot} {radio.PassbandHz} Hz");

        Assert.Equal(CivMode.Cw, radio.Mode);
        Assert.Equal(ScriptedRadio.NarrowHz, radio.PassbandHz);

        var target = ModeFollowPlan.TargetFor(hood);

        Assert.True(
            ModeFollowPlan.WaitsForDwell(target),
            "FT8 city does not wait for the dial to stop");

        // **PASSING THROUGH.** The dial is inside the block and still moving, so
        // nothing is written and nothing is said.
        var at = new DateTime(2026, 8, 28, 20, 0, 0, DateTimeKind.Utc);
        var dwell = ModeDwell.Nowhere;

        for (var step = 0; step < 8; step++)
        {
            (dwell, var early) = dwell.Observe(
                hood.Name, Ft8Hz + (step * 100), at.AddMilliseconds(step * 250),
                scanning: false);

            Assert.False(early, "a moving dial matured a dwell");
        }

        Assert.Equal(CivMode.Cw, radio.Mode);
        Assert.Equal(0, radio.ModeWrites);

        // **STOPPING.** Same block, unchanged frequency, a second apart.
        (dwell, _) = dwell.Observe(hood.Name, Ft8Hz, at.AddSeconds(10), false);
        (dwell, var matured) = dwell.Observe(
            hood.Name, Ft8Hz, at.AddSeconds(11), false);

        Assert.True(matured, "a dial at rest for a second did not mature");

        var decision = ModeFollowPlan.Decide(
            state, ledger.Mode, ledger.DataVariant, target, Ft8Hz,
            workingCw: false, ledger[RigField.Mode].AtUtc,
            ledger[RigField.DataMode].AtUtc);

        Assert.True(decision.Write);

        var result = await rig.SetModeAsync(
            decision.Mode, decision.DataMode,
            hood.PassbandHz is not null ? CivWrites.WidestFilterSlot : null);

        _output.WriteLine(
            $"  after:  {radio.Mode} data={radio.DataMode} "
            + $"FIL{radio.FilterSlot} {radio.PassbandHz} Hz — {decision.Narration}");

        Assert.True(result.Worked);
        Assert.Equal(CivMode.Usb, radio.Mode);
        Assert.True(radio.DataMode);

        // **AND IT SAID SO** (HM-DEC-056). A radio that changes mode with no
        // explanation is the "is it broken" confusion relocated.
        Assert.NotEqual("", decision.Narration);
    }
}
