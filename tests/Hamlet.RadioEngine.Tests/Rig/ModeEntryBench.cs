using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// A radio set up the way an operator might have left it, and the real
/// <see cref="ReceiverSetup"/> driven against it, for work instruction 419.
/// </summary>
/// <remarks>
/// <para>**EVERY RESULT HERE IS AN INDICATION, NOT A MEASUREMENT OF THE RADIO**
/// (FACT-004, FACT-006). There is no IC-7300 on this machine; the radio is
/// <see cref="ScriptedRadio"/>, which holds what it was last told and answers
/// for itself.</para>
/// <para>**THE FIELDS ARE THE ONES THE CONDITIONS FILE STATES**, and the radio
/// speaks every one of them, so a read never times out on a field a mode
/// asks about.</para>
/// </remarks>
internal static class ModeEntryBench
{
    /// <summary>The `16` sub-commands, from §4's table.</summary>
    public const byte Preamp = 0x02;

    /// <summary>AGC.</summary>
    public const byte Agc = 0x12;

    /// <summary>Noise blanker.</summary>
    public const byte NoiseBlanker = 0x22;

    /// <summary>Noise reduction.</summary>
    public const byte NoiseReduction = 0x40;

    /// <summary>Auto notch.</summary>
    public const byte AutoNotch = 0x41;

    /// <summary>Manual notch.</summary>
    public const byte ManualNotch = 0x48;

    /// <summary>The `14` sub-commands.</summary>
    public const byte RfGain = 0x02;

    /// <summary>Squelch.</summary>
    public const byte Squelch = 0x03;

    /// <summary>The fields a tune-in can touch, in the conditions file's CW order.</summary>
    public static readonly RigField[] Fields =
    {
        RigField.AutoNotch, RigField.ManualNotch, RigField.NoiseBlanker,
        RigField.NoiseReduction, RigField.Agc, RigField.RfGain, RigField.Squelch,
        RigField.Attenuator, RigField.Preamp,
    };

    /// <summary>
    /// A radio as an operator might have left it: preamp off, AGC on mid, the
    /// noise blanker on, RF gain at full, everything else off or open.
    /// </summary>
    /// <param name="hz">Where the dial is.</param>
    /// <param name="data">Whether the radio is in USB-D rather than CW.</param>
    /// <returns>The radio.</returns>
    public static ScriptedRadio AsLeft(long hz, bool data)
    {
        var radio = new ScriptedRadio { FrequencyHz = hz };

        radio.OperatorTurnsTheModeKnob(data ? CivMode.Usb : CivMode.Cw, data, 1);

        radio.Switches[Preamp] = 0;
        radio.Switches[Agc] = 2;
        radio.Switches[NoiseBlanker] = 1;
        radio.Switches[NoiseReduction] = 0;
        radio.Switches[AutoNotch] = 0;
        radio.Switches[ManualNotch] = 0;
        radio.Levels[RfGain] = 255;
        radio.Levels[Squelch] = 0;
        radio.AttenuatorDb = 0;
        radio.Overloading = false;

        return radio;
    }

    /// <summary>
    /// A radio already at every value the CW row asks for at this frequency.
    /// </summary>
    /// <param name="hz">Where the dial is.</param>
    /// <returns>The radio.</returns>
    /// <remarks>
    /// The preamp's value is the condition's own text read at the frequency:
    /// preamp 1 above 40 m, off at 40 m and below.
    /// </remarks>
    public static ScriptedRadio AlreadyRightForCw(long hz)
    {
        var radio = AsLeft(hz, data: false);

        radio.Switches[Agc] = 1;
        radio.Switches[NoiseBlanker] = 0;
        radio.Switches[Preamp] = (byte)(hz > 10_000_000 ? 1 : 0);

        return radio;
    }

    /// <summary>Connect Hamlet's rig to a scripted radio.</summary>
    /// <param name="radio">The radio.</param>
    /// <returns>The connected rig.</returns>
    public static async Task<Ic7300Rig> ConnectAsync(ScriptedRadio radio)
    {
        var rig = new Ic7300Rig(radio);
        Assert.True(await rig.ConnectAsync());
        return rig;
    }

    /// <summary>The block the app would hand the setup at this frequency.</summary>
    /// <param name="hz">The dial.</param>
    /// <returns>The block, or null off the map.</returns>
    public static Neighborhood? BlockAt(long hz)
        => HfBands.BandFor(hz) is { } band
            ? NeighborhoodPlan.ForBand(band).FirstOrDefault(n => n.Contains(hz))
            : null;

    /// <summary>Every write the radio has taken, as field and value, in order.</summary>
    /// <param name="radio">The radio.</param>
    /// <returns>The writes.</returns>
    public static IReadOnlyList<(RigField Field, int Value)> Writes(ScriptedRadio radio)
    {
        var writes = new List<(RigField, int)>();

        foreach (var (sub, value) in radio.SwitchWrites)
        {
            writes.Add((FieldFor(0x16, sub), value));
        }

        foreach (var (sub, value) in radio.LevelWrites)
        {
            writes.Add((FieldFor(0x14, sub), value));
        }

        foreach (var db in radio.AttenuatorWrites)
        {
            writes.Add((RigField.Attenuator, db));
        }

        return writes;
    }

    /// <summary>Forget every write so far, so the next tune-in is counted alone.</summary>
    /// <param name="radio">The radio.</param>
    public static void ClearWrites(ScriptedRadio radio)
    {
        radio.SwitchWrites.Clear();
        radio.LevelWrites.Clear();
        radio.AttenuatorWrites.Clear();
    }

    /// <summary>What the radio holds now, read through Hamlet's own reads.</summary>
    /// <param name="rig">The rig.</param>
    /// <returns>The state.</returns>
    public static async Task<RigState> ReadAllAsync(Ic7300Rig rig)
    {
        var state = RigState.Empty;

        foreach (var field in Fields.Append(RigField.Mode).Append(RigField.Overflow))
        {
            state = state.With(await rig.ReadAsync(field, state));
        }

        return state;
    }

    // The field a `16` or `14` sub-command writes, from the write table rather
    // than from a second list typed here.
    private static RigField FieldFor(byte command, byte sub)
        => CivWrites.All.First(w => w.Command == command && w.Sub.Length == 1 && w.Sub[0] == sub).Field;
}
