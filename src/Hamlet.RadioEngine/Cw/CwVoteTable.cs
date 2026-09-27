namespace Hamlet.RadioEngine.Cw;

/// <summary>A decoder's held-out calibration verdict on one condition (HM-REQ-124).</summary>
public enum CwCalibrationStanding
{
    /// <summary>Calibrated held-out: it votes on this condition.</summary>
    Calibrated,

    /// <summary>Measured and not calibrated: advisory only.</summary>
    NotCalibrated,

    /// <summary>Under 30 scored characters: advisory only.</summary>
    NotMeasurable,
}

/// <summary>Which decoders vote on the condition in force.</summary>
/// <param name="Ours">Hamlet's own decoder votes.</param>
/// <param name="Second">The fldigi port votes.</param>
public readonly record struct CwVote(bool Ours, bool Second)
{
    /// <summary>Neither votes: ours is emitted as it prints today (465 DECIDED (4)).</summary>
    public static CwVote Neither => new(false, false);
}

/// <summary>
/// Who may vote where (HM-REQ-124; work instruction 465, task 2; PHASE_PLAN.md
/// 9.6): each decoder's held-out calibration verdict per condition, transcribed.
/// </summary>
/// <remarks>
/// <para>**TRANSCRIBED, NOT MEASURED HERE** (465 DECIDED (3)): the held-out
/// verdicts of `docs/phase-requirements/calibration.md` section 2, written by
/// `EachDecodersConfidenceIsMeasuredTests.HeldOutPerConditionAndTheFileWritten`
/// at unit 464's `efdd5d11` and ticked at `b30bef48`. A decoder votes on a
/// condition only where it is calibrated there; elsewhere its reading is advisory,
/// on the sheet and never on the screen. Fixed at task 1, before any arbitrated
/// text was seen, and not changed after (V-14).</para>
/// <para>**THE LIVE PRODUCT RUNS UNDER <see cref="LiveCondition"/>**, because no
/// sender and no channel profile is known live. A condition this table does not
/// name gives <see cref="CwVote.Neither"/>.</para>
/// <para>Switching arbitration off on a condition where it loses to the better
/// decoder alone is HM-REQ-128's, criterion 9.7, and is not here.</para>
/// </remarks>
public static class CwVoteTable
{
    /// <summary>The condition the live product runs under: real HF, all.</summary>
    public const string LiveCondition = "real HF, all";

    /// <summary>calibration.md section 2, per condition: ours' verdict, then the port's.</summary>
    public static IReadOnlyDictionary<string, (CwCalibrationStanding Ours, CwCalibrationStanding Second)> Verdicts { get; } =
        new Dictionary<string, (CwCalibrationStanding, CwCalibrationStanding)>(StringComparer.Ordinal)
        {
            ["real HF, all"] = (CwCalibrationStanding.Calibrated, CwCalibrationStanding.NotCalibrated),
            ["real HF, no CH-* profile, SNR_2500 not measured, sender TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net)"]
                = (CwCalibrationStanding.NotCalibrated, CwCalibrationStanding.NotCalibrated),
            ["real HF, no CH-* profile, SNR_2500 not measured, sender TX-ITU (CW_SPEC.md 10, the KD0UN capture)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
            ["real HF, no CH-* profile, SNR_2500 not measured, sender TX-TIGHT (CW_SPEC.md 10, HM-DEC-101)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
            ["real HF, no CH-* profile, SNR_2500 not measured, sender not stated in CW_SPEC.md"]
                = (CwCalibrationStanding.Calibrated, CwCalibrationStanding.NotCalibrated),
            ["synthetic, all"] = (CwCalibrationStanding.Calibrated, CwCalibrationStanding.NotCalibrated),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 0 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 15 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.Calibrated, CwCalibrationStanding.NotCalibrated),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), TX-ITU (1:3:1:3:7), 5 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.NotCalibrated, CwCalibrationStanding.NotCalibrated),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 0 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 15 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
            ["synthetic, no fading, shaped noise band (not shown to be CH-AWGN), character gap 5 units, inside TX-FARNS's 3 to 7, 5 dB in the passband (not restated in the 2500 Hz reference)"]
                = (CwCalibrationStanding.NotMeasurable, CwCalibrationStanding.NotMeasurable),
        };

    /// <summary>Who votes on a condition: a decoder calibrated there, and nobody on a condition not in the table.</summary>
    /// <param name="condition">The condition as calibration.md names it.</param>
    /// <returns>The vote.</returns>
    public static CwVote For(string condition)
        => Verdicts.TryGetValue(condition, out var v)
            ? new CwVote(v.Ours == CwCalibrationStanding.Calibrated, v.Second == CwCalibrationStanding.Calibrated)
            : CwVote.Neither;

    /// <summary>Who votes live: <see cref="For"/> at <see cref="LiveCondition"/>.</summary>
    public static CwVote Live => For(LiveCondition);
}
