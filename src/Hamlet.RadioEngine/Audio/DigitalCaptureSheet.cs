using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// The sheet written beside a digital capture, so a later reader can tell
/// whether a fault was in the signal, the radio, or Hamlet.
/// </summary>
/// <remarks>
/// <para>**THIS IS §0.0.1 AND NOTHING ELSE** (work instruction 041, task 1). The
/// operator photographed a waterfall on 2026-08-28 and the picture was read as a
/// signal problem twice, wrongly, because **the radio's mode and filter existed
/// in no file Hamlet wrote.** He was in CW at 500 Hz under a three-kilohertz
/// block, and no amount of looking at the screenshot could have said so.</para>
/// <para>**MODE AND THE DATA FLAG ARE SEPARATE LINES ON PURPOSE.** `USB` and
/// `USB-D` are different radios to an operator, and folding them into one line is
/// exactly the ambiguity that cost an hour. A sheet that says `USB` when the flag
/// was never read is the guess §0.0 forbids.</para>
/// <para>**EVERY ROW SAYS MEASURED OR UNKNOWN AND NOTHING IS DEFAULTED.** A value
/// nobody read says so, the way the "What the radio is doing" window already
/// does; a plausible number in its place is worse than a gap, because it will be
/// believed.</para>
/// <para>**AND IT IS NOT `CwCaseRoster`.** That roster scores the CW decoder and
/// every row of it asserts the operator heard a station Hamlet failed to read
/// (Tim's ruling of 2026-08-28). A digital press is not a CW case.</para>
/// </remarks>
public static class DigitalCaptureSheet
{
    /// <summary>How the sheet reports a value that was never read.</summary>
    public const string Unread = "unknown (not read)";

    /// <summary>Compose the sheet.</summary>
    /// <param name="capturedUtc">When the press happened.</param>
    /// <param name="seconds">How much audio was kept.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="state">The radio as Hamlet believed it at the press.</param>
    /// <param name="clock">The clock offset and its age.</param>
    /// <param name="nowUtc">The moment, for the clock's age.</param>
    /// <param name="neighborhood">Where the dial is, in words, or "".</param>
    /// <param name="needsHz">
    /// How much passband the block needs, or null where none is stated.
    /// </param>
    /// <returns>The sheet, ready to write.</returns>
    public static string Compose(
        DateTime capturedUtc,
        double seconds,
        int sampleRate,
        RigState state,
        ClockOffset clock,
        DateTime nowUtc,
        string neighborhood,
        long? needsHz)
    {
        ArgumentNullException.ThrowIfNull(state);

        var sheet = new StringBuilder();

        void Line(string key, string value)
            => sheet.Append(key.PadRight(11)).Append(value).Append('\n');

        Line("captured", capturedUtc.ToString(
            "yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture));
        Line("seconds", seconds.ToString("0.0", CultureInfo.InvariantCulture));
        Line("sampleRate", sampleRate.ToString(CultureInfo.InvariantCulture));

        // **THE FILE IS NOT TRIMMED AND SAYS SO** (Tim's ruling of 2026-08-28).
        // A thirty-second grab starting mid-slot leaves WSJT-X two partial slots
        // it cannot score, so these are diagnostic material rather than corpus,
        // and a later scoring run has to be able to tell without opening the
        // audio.
        Line("trimmed", "no  (raw ring, not aligned to slot boundaries; this is "
            + "diagnostic material and not scoring corpus)");

        sheet.Append('\n');

        // **THE THREE FIELDS WHOSE ABSENCE COST TWO HOURS**, first and apart.
        Line("mode", Describe(state[RigField.Mode]));
        Line("dataMode", DescribeDataMode(state));
        Line("filterSlot", Describe(state[RigField.FilterSelection]));
        Line("filterHz", DescribeWidth(state, needsHz));

        sheet.Append('\n');

        Line("frequency", Describe(state[RigField.Frequency]));
        Line("block", neighborhood.Length == 0 ? "not on the map" : neighborhood);
        Line("needsHz", needsHz is { } n
            ? n.ToString(CultureInfo.InvariantCulture)
              + "  (every signal here is an audio tone above the dial)"
            : "no passband requirement stated for this block");

        sheet.Append('\n');

        Line("clock", clock.Describe(nowUtc));

        sheet.Append('\n');

        // The rest of what the "What the radio is doing" window reads. That
        // window is §0.0.1 working; the sheet holds the same set so a capture is
        // as diagnosable as the screen was at the moment of the press.
        foreach (var field in new[]
        {
            RigField.SMeter, RigField.Overflow, RigField.Preamp,
            RigField.Attenuator, RigField.Agc, RigField.NoiseBlanker,
            RigField.NoiseReduction, RigField.RfGain, RigField.Squelch,
            RigField.TransmitStatus,
        })
        {
            Line(Name(field), Describe(state[field]));
        }

        return sheet.ToString();
    }

    /// <summary>The data variant, on its own line, never folded into the mode.</summary>
    private static string DescribeDataMode(RigState state)
    {
        var value = state[RigField.DataMode];

        if (!value.IsKnown)
        {
            return Unread
                + "  (so whether this is USB or USB-D is NOT established here)";
        }

        return value.Number is 1
            ? "on   (this is the -D variant: the computer's audio is routed)"
            : "off  (this is the plain voice or Morse variant)";
    }

    /// <summary>The passband in hertz, against what the block needs.</summary>
    private static string DescribeWidth(RigState state, long? needsHz)
    {
        var value = state[RigField.FilterBandwidth];

        if (!value.IsKnown || value.Number is not { } hertz)
        {
            return Unread + "  (so the passband is not established here)";
        }

        var width = ((int)hertz).ToString(CultureInfo.InvariantCulture) + " Hz";

        if (needsHz is not { } needed)
        {
            return $"{width}  (measured, {Age(value)})";
        }

        return hertz >= needed
            ? $"{width}  (measured, {Age(value)}; wide enough for the {needed} Hz "
              + "this block occupies)"
            : $"{width}  (measured, {Age(value)}; TOO NARROW for the {needed} Hz "
              + "this block occupies, so most of it cannot be heard)";
    }

    private static string Describe(RigValue value)
        => value.IsKnown
            ? $"{value.Text}  (measured, {Age(value)})"
            : $"{Unread}  ({value.Source})";

    private static string Age(RigValue value)
        => value.AtUtc is { } at
            ? $"read {at:HH:mm:ss} UTC via {value.Source}"
            : $"via {value.Source}";

    private static string Name(RigField field) => field switch
    {
        RigField.SMeter => "sMeter",
        RigField.Overflow => "overflow",
        RigField.Preamp => "preamp",
        RigField.Attenuator => "attenuator",
        RigField.Agc => "agc",
        RigField.NoiseBlanker => "noiseBlank",
        RigField.NoiseReduction => "noiseRed",
        RigField.RfGain => "rfGain",
        RigField.Squelch => "squelch",
        RigField.TransmitStatus => "transmit",
        _ => field.ToString(),
    };
}
