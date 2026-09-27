namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The map from our decoder's rival margin to the probability that a character
/// is right (HM-REQ-124; work instruction 464; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// <para>**ONE FEATURE, ONE LOGISTIC, FIXED BEFORE IT WAS MEASURED** (464 task 1,
/// V-14): p = 1 / (1 + e^-(a + b.x)) with x = sign(m) ln(1 + |m|), m being
/// <see cref="CwCharacter.MarginLlr"/>. The compression is monotone, so it orders
/// characters exactly as the margin does; it is there because the margin runs
/// from under one to the hundreds and to infinity. A margin that is not finite -
/// a short letter with no rival reading on the lattice - takes one constant, the
/// share of such characters the keys say are right.</para>
/// <para>**THE CONSTANTS ARE THE FIT OVER THE WHOLE KEYED CORPUS**, the real set's
/// 23 recordings (inferred keys) and the synthetic set's 12 cases (exact keys)
/// together, by maximum likelihood, to right against wrong or added. How far they
/// hold on a recording they were not fitted on is measured held-out, per
/// condition, in `docs/phase-requirements/calibration.md`. It knows no word, no
/// callsign and no letter's frequency (R72).</para>
/// </remarks>
public static class CwCharacterProbability
{
    /// <summary>The logistic's intercept, a: the whole pool's fit, 571 characters (work instruction 464, task 2).</summary>
    public const double Intercept = 1.4046693133246466;

    /// <summary>The logistic's slope on the compressed margin, b.</summary>
    public const double Slope = 0.47470729666229566;

    /// <summary>The probability given a character whose margin is not finite: 35 right of the 38 such scored characters.</summary>
    public const double NotFinite = 0.9210526315789473;

    /// <summary>The margin as the logistic takes it.</summary>
    /// <param name="marginLlr">The rival margin, natural log.</param>
    /// <returns>sign(m) ln(1 + |m|).</returns>
    public static double Feature(double marginLlr)
        => Math.Sign(marginLlr) * Math.Log(1 + Math.Abs(marginLlr));

    /// <summary>The probability that a character with this margin is right.</summary>
    /// <param name="marginLlr">The rival margin, natural log; any value.</param>
    /// <returns>A probability from 0 to 1.</returns>
    public static double Of(double marginLlr)
        => double.IsFinite(marginLlr)
            ? 1 / (1 + Math.Exp(-(Intercept + Slope * Feature(marginLlr))))
            : NotFinite;
}
