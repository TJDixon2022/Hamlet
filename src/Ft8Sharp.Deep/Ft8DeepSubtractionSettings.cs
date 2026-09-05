using System;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How many times a slot is read, and how hard the fit looks for the place before it subtracts.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS NOT A GATE, A THRESHOLD OR AN ACCEPTANCE RULE.</b> Nothing here decides that a message
/// is real. Every codeword produced under these settings goes through <c>Ft8CodewordDecoder.Decode</c>
/// and past the port's own parity gate and its CRC-14, exactly as a first-pass codeword does, and
/// <b>the number of codewords put to those gates per candidate per pass is unchanged at one</b>. A
/// pass is an ordinary decode of a different buffer.
/// </para>
/// <para>
/// <b>WHAT THE MULTI-PASS STRATEGY IS AND WHERE IT COMES FROM.</b> Subtracting a decoded
/// transmission from the received slot and decoding the residual is described in Franke, Somerville
/// and Taylor, <em>The FT4 and FT8 Communication Protocols</em>, QEX, July/August 2020 — the same
/// paper the port's own <c>NOTICE</c> cites for the waveform. <b>No route to any of it goes through
/// WSJT-X's source or <c>ft4_ft8_public/</c>.</b> The least-squares fit itself is textbook
/// arithmetic and is written out in <c>docs/unit253-subtraction.md</c> §2.
/// </para>
/// <para>
/// <b>THE STOPPING RULE, AND IT IS A RULE RATHER THAN A SETTING THAT HAPPENED TO BE USED.</b> A pass
/// is the last pass when any one of these is true:
/// </para>
/// <list type="number">
/// <item><description>
/// the pass count has reached <see cref="MaxPasses"/> — a hard bound, so the worst-case cost of a
/// slot is a number that can be measured rather than a policy that has to be believed;
/// </description></item>
/// <item><description>
/// the pass returned no message that had not already been returned — there is nothing new to
/// subtract, so the next residual would be this residual;
/// </description></item>
/// <item><description>
/// no message in the pass could be subtracted — every one was either refused for want of symbols or
/// already subtracted, so the buffer cannot change and neither can the answer.
/// </description></item>
/// </list>
/// <para>
/// <b>AND OFF IS THE DEFAULT.</b> <c>Ft8DeepSlotDecoder.Subtraction</c> is <see langword="null"/>
/// unless it is asked for, for the same reason ordered statistics is: every row of units 246, 248,
/// 251 and 252 is a measurement of the default path, and a default that moved would invalidate all
/// of them at once.
/// </para>
/// </remarks>
public sealed class Ft8DeepSubtractionSettings
{
    /// <summary>The most passes over one slot, unless a caller asks for another number.</summary>
    /// <remarks>
    /// <b>Two, which is one subtraction.</b> Unit 253's task 4a priced what each further pass buys
    /// over the one before it at 51 trials and this default is read off that table, not chosen.
    /// </remarks>
    public const int DefaultMaxPasses = 2;

    /// <summary>The most passes this type will accept.</summary>
    /// <remarks>
    /// <b>Eight, and it is a bound on the false-accept budget rather than on the arithmetic.</b>
    /// Every pass is a whole extra decode, so it puts another slot's worth of codewords to the
    /// CRC-14 at about one in 16 384 each — <c>docs/unit253-subtraction.md</c> §3.1 tabulates it. A
    /// caller who wants more than eight passes is asking for a different measurement and should say
    /// so out loud.
    /// </remarks>
    public const int MaximumPasses = 8;

    /// <summary>How far either way in samples the fit looks for the transmission's start.</summary>
    /// <remarks>
    /// <b>Twelve, because <c>Ft8DeepSignalToNoise.Estimate</c> leaves at most half of its final
    /// 0.00125 s step — 7.5 samples at 12 kHz — and twelve covers that with margin.</b> The
    /// remainder after this search is under one sample, which is 83 microseconds, 0.043 per cent of
    /// a symbol, and a pure carrier phase shift that the quadrature coefficient absorbs exactly.
    /// </remarks>
    public const int DefaultTimeSearchSamples = 12;

    /// <summary>How far either way in hertz the fit looks for the transmission's frequency.</summary>
    /// <remarks>
    /// <b>Half a hertz, because <c>Ft8DeepSignalToNoise.Estimate</c>'s frequency search steps 0.40
    /// Hz and so leaves up to ±0.20 Hz.</b> That residue is not small: over a 12.64 s frame 0.20 Hz
    /// is <b>2.53 whole cycles</b> of accumulated phase, and a coherent fit against a reference off
    /// by that much correlates to approximately zero and removes nothing. <b>The estimator gets the
    /// fit into the basin; it does not finish the job</b>, and this is the search that does.
    /// </remarks>
    public const double DefaultFrequencySearchHz = 0.5;

    /// <summary>The step of the frequency search, in hertz.</summary>
    /// <remarks>
    /// <b>0.02 Hz, which is 0.25 cycles of phase over the frame</b> — the residual after the search
    /// is half a step, 0.01 Hz, or 0.13 cycles, costing under 0.4 dB of cancellation. Finer would
    /// cost nothing measurable and buy nothing measurable: the block-summed evaluation in
    /// <see cref="Ft8DeepMessageSubtractor"/> makes fifty-one of these cost about sixteen thousand
    /// multiply-adds in total.
    /// </remarks>
    public const double DefaultFrequencyStepHz = 0.02;

    /// <summary>
    /// <b>How many of the 79 symbols must lie inside the slot before anything is subtracted.</b>
    /// </summary>
    /// <remarks>
    /// The same forty as <c>Ft8DeepSignalToNoise.MinimumSymbols</c>, for the same reason and
    /// deliberately not a second number: a fit taken over twelve symbols because the transmission
    /// ran off the end of what was captured is a different quantity from one taken over
    /// seventy-nine, and subtracting on the strength of it would write a wrong waveform into the
    /// buffer the next pass reads.
    /// </remarks>
    public const int MinimumSymbols = Ft8DeepSignalToNoise.MinimumSymbols;

    /// <summary>Builds a rule for how many passes to run and how hard to look before subtracting.</summary>
    /// <param name="maxPasses">The most passes over one slot.</param>
    /// <param name="timeSearchSamples">How far either way in samples the fit looks.</param>
    /// <param name="frequencySearchHz">How far either way in hertz the fit looks.</param>
    /// <param name="frequencyStepHz">The step of the frequency search.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <b>Refused rather than clamped, in every case.</b> A clamped setting is a caller who asked
    /// for one measurement and got another without being told.
    /// </exception>
    public Ft8DeepSubtractionSettings(
        int maxPasses = DefaultMaxPasses,
        int timeSearchSamples = DefaultTimeSearchSamples,
        double frequencySearchHz = DefaultFrequencySearchHz,
        double frequencyStepHz = DefaultFrequencyStepHz)
    {
        if (maxPasses < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPasses),
                maxPasses,
                "a slot is read at least once, so the pass count is at least one. Zero passes is not "
                + "subtraction turned off - that is a null settings object on the decoder - it is a "
                + "decoder that returns nothing and says it decoded a slot.");
        }

        if (maxPasses > MaximumPasses)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxPasses),
                maxPasses,
                $"{MaximumPasses} passes is the most this type accepts. Every pass puts another "
                + "slot's worth of codewords to the port's CRC-14 at about one in 16384 each, so "
                + "the pass count is a false-accept budget and not a knob. See "
                + "docs/unit253-subtraction.md section 3.1.");
        }

        if (timeSearchSamples < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeSearchSamples),
                timeSearchSamples,
                "the time search extent is a number of samples either way and cannot be negative. "
                + "Zero means the fit trusts the place it was given exactly.");
        }

        if (!(frequencySearchHz >= 0.0) || double.IsInfinity(frequencySearchHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencySearchHz),
                frequencySearchHz,
                "the frequency search extent is a finite number of hertz either way and cannot be "
                + "negative or NaN.");
        }

        if (!(frequencyStepHz > 0.0) || double.IsInfinity(frequencyStepHz))
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencyStepHz),
                frequencyStepHz,
                "the frequency search step must be a finite number of hertz above zero; a step of "
                + "zero or below does not terminate.");
        }

        MaxPasses = maxPasses;
        TimeSearchSamples = timeSearchSamples;
        FrequencySearchHz = frequencySearchHz;
        FrequencyStepHz = frequencyStepHz;
    }

    /// <summary>The rule this library recommends, with the stopping rule at two passes.</summary>
    public static Ft8DeepSubtractionSettings Default { get; } = new();

    /// <summary>The most passes over one slot.</summary>
    public int MaxPasses { get; }

    /// <summary>How far either way in samples the fit looks for the transmission's start.</summary>
    public int TimeSearchSamples { get; }

    /// <summary>How far either way in hertz the fit looks for the transmission's frequency.</summary>
    public double FrequencySearchHz { get; }

    /// <summary>The step of the frequency search, in hertz.</summary>
    public double FrequencyStepHz { get; }
}
