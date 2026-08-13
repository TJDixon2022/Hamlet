namespace Hamlet.RadioEngine.Training;

/// <summary>The modes the training radio can put on the air.</summary>
public enum TrainingMode
{
    /// <summary>On/off keyed carrier — Morse.</summary>
    Cw,

    /// <summary>Synchronised 15-second bursts.</summary>
    Ft8,

    /// <summary>Two tones, 170 Hz apart, alternating.</summary>
    Rtty,

    /// <summary>A 31 Hz ribbon, near-continuous.</summary>
    Psk31,

    /// <summary>A wide, irregular voice smear.</summary>
    Ssb,
}

/// <summary>
/// One signal placed on the band, with the parameters that give its mode its
/// real character.
/// </summary>
/// <param name="Mode">Which mode it is.</param>
/// <param name="CenterHz">Where it sits.</param>
/// <param name="Strength">Peak amplitude, 0 to 1, before the noise floor is
/// added.</param>
/// <param name="WordsPerMinute">CW keying speed; ignored by other modes.</param>
/// <param name="Text">What a CW station is sending.</param>
/// <param name="PhaseOffset">Fraction of a cycle, 0 to 1, so two signals of
/// the same mode are not in lockstep.</param>
/// <param name="FadePeriod">Period of the slow QSB fade; zero for none.</param>
public sealed record SyntheticSignal(
    TrainingMode Mode,
    long CenterHz,
    double Strength,
    int WordsPerMinute = 18,
    string Text = "CQ CQ DE W1AW W1AW K",
    double PhaseOffset = 0,
    TimeSpan FadePeriod = default)
{
    /// <summary>
    /// Occupied bandwidth in hertz, from each mode's real characteristics.
    /// </summary>
    /// <remarks>
    /// These are the numbers that make the waterfall teachable. A newcomer
    /// learning that PSK31 is a hair and SSB is a slab is learning something
    /// true about the air, so the widths are the real ones and not whatever
    /// drew nicely.
    /// </remarks>
    public int WidthHz => Mode switch
    {
        TrainingMode.Cw => 150,
        TrainingMode.Ft8 => 50,
        TrainingMode.Rtty => 170 + 60,
        TrainingMode.Psk31 => 31,
        TrainingMode.Ssb => 2400,
        _ => 100,
    };

    /// <summary>Lower edge of the occupied bandwidth.</summary>
    public long LowHz => CenterHz - (WidthHz / 2);

    /// <summary>Upper edge of the occupied bandwidth.</summary>
    public long HighHz => CenterHz + (WidthHz / 2);
}
