using System;
using Ft8Sharp.Dsp;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>How far a candidate may be moved, and in what steps.</b> The extent is set by what the coarse
/// grid leaves undetermined; the step is set by what the distance instrument can distinguish.
/// </summary>
/// <remarks>
/// <para>
/// <b>The extent must cover the whole cell and nothing here chose it.</b> At 12 kHz the waterfall
/// advances in 960-sample sub-blocks and its transform bins are 3.125 Hz apart, so a candidate is
/// placed to within <b>+/- 480 samples (+/- 0.04 s, a quarter of a symbol)</b> in time and <b>+/-
/// 1.5625 Hz (a quarter of a tone)</b> in frequency. An extent narrower than that would leave part
/// of the cell unreachable and the search would report its own edge instead of a peak, which is why
/// <c>Ft8DeepFineSync</c> counts edge hits and says the number.
/// </para>
/// <para>
/// <b>The step is measured rather than chosen, and the measurement is the sinc loss.</b> The symbol
/// correlation is over exactly one 0.160 s window, so a residual frequency error of <c>df</c> costs
/// <c>sinc(df x 0.160)</c> of the tone's amplitude. At the default step the residual is at most half
/// a step, <b>0.26 Hz, which is 0.04 dB</b> - far below anything the distance instrument can
/// distinguish over 51 trials, and a finer step would be tuning. In time, a residual of <c>dt</c>
/// admits <c>dt / 0.160</c> of the neighbouring symbol; at the default step the residual is at most
/// <b>2.5 ms, 1.6 per cent of a symbol</b>. <b>Unit 248 measured both against the hard-decision
/// distance rather than assuming them</b>, and the table is in
/// <c>docs/unit248-baseband-resync.md</c>.
/// </para>
/// <para>
/// <b>17 time positions by 7 frequency positions is 119 correlations a candidate</b>, and what that
/// costs in milliseconds is measured and reported rather than estimated - the port sits at about
/// 64 ms a slot and ordered statistics at about 72.
/// </para>
/// </remarks>
public sealed class Ft8DeepFineSyncSettings
{
    /// <summary>
    /// Half of what one waterfall sub-block leaves undetermined in time, in seconds: 0.04 at 12 kHz.
    /// </summary>
    public const double CellTimeSeconds = 0.04;

    /// <summary>
    /// Half of what one waterfall transform bin leaves undetermined in frequency, in hertz.
    /// </summary>
    public const double CellFrequencyHz = 1.5625;

    /// <summary>
    /// <b>The settings this library uses when nobody names any</b>, and the ones every figure
    /// unit 248 recorded was taken at: the whole cell in both axes, in 5 ms and 0.52 Hz steps.
    /// </summary>
    public static Ft8DeepFineSyncSettings Default { get; } =
        new(CellTimeSeconds, 0.005, CellFrequencyHz, CellFrequencyHz / 3.0);

    /// <summary>Builds settings, or refuses them.</summary>
    /// <param name="timeExtentSeconds">How far either way in time the search may move a candidate.</param>
    /// <param name="timeStepSeconds">The time step.</param>
    /// <param name="frequencyExtentHz">How far either way in frequency it may move a candidate.</param>
    /// <param name="frequencyStepHz">The frequency step.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// An extent or a step is not positive, or a step is larger than its extent.
    /// </exception>
    public Ft8DeepFineSyncSettings(
        double timeExtentSeconds,
        double timeStepSeconds,
        double frequencyExtentHz,
        double frequencyStepHz)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeExtentSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeStepSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyExtentHz);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyStepHz);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeStepSeconds, timeExtentSeconds);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(frequencyStepHz, frequencyExtentHz);

        TimeExtentSeconds = timeExtentSeconds;
        TimeStepSeconds = timeStepSeconds;
        FrequencyExtentHz = frequencyExtentHz;
        FrequencyStepHz = frequencyStepHz;
    }

    /// <summary>How far either way in time the search may move a candidate, in seconds.</summary>
    public double TimeExtentSeconds { get; }

    /// <summary>The time step, in seconds.</summary>
    public double TimeStepSeconds { get; }

    /// <summary>How far either way in frequency it may move a candidate, in hertz.</summary>
    public double FrequencyExtentHz { get; }

    /// <summary>The frequency step, in hertz.</summary>
    public double FrequencyStepHz { get; }

    /// <summary>Time positions on either side of the candidate's own.</summary>
    public int TimeStepCount => (int)Math.Floor((TimeExtentSeconds / TimeStepSeconds) + 1e-9);

    /// <summary>Frequency positions on either side of the candidate's own.</summary>
    public int FrequencyStepCount => (int)Math.Floor((FrequencyExtentHz / FrequencyStepHz) + 1e-9);

    /// <summary>How many positions one candidate's search visits.</summary>
    public int PositionCount =>
        ((2 * TimeStepCount) + 1) * ((2 * FrequencyStepCount) + 1);

    /// <summary>
    /// <b>Whether this extent covers the whole cell the coarse grid leaves undetermined</b>, which is
    /// what step 4's first exit asks for.
    /// </summary>
    /// <remarks>
    /// Read from the geometry rather than from the constants above, so a geometry at another rate
    /// answers for itself. A search narrower than the cell can only report its own edge.
    /// </remarks>
    public bool CoversTheCell(Ft8WaterfallGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var timeCell = geometry.SubblockSize / (2.0 * geometry.SampleRate);
        var frequencyCell = geometry.TransformBinSpacingHz / 2.0;

        return TimeExtentSeconds >= timeCell - 1e-9
            && FrequencyExtentHz >= frequencyCell - 1e-9;
    }
}
