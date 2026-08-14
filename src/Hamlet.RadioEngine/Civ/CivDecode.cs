using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// Turns a CI-V response payload into the values it carries.
/// </summary>
/// <remarks>
/// <para>Separate from the rig so it can be tested without a port, and so the
/// mapping from bytes to meaning sits in one file that can be checked against
/// the manual line by line (HM-DEC-049).</para>
/// <para>A PAYLOAD THAT DOES NOT MATCH ITS DOCUMENTED SHAPE PRODUCES UNKNOWN,
/// never a nearest guess. A short frame, a byte outside the documented set, a
/// nibble that is not valid BCD: each of those means Hamlet does not know what
/// the radio said, and saying so is the only honest answer (§0.0). The
/// temptation to round a stray byte into the nearest legal value is exactly how
/// a mode badge ends up confidently wrong.</para>
/// </remarks>
public static class CivDecode
{
    /// <summary>
    /// Decode one response into the field or fields it answers.
    /// </summary>
    /// <param name="read">The read that was issued.</param>
    /// <param name="payload">The data after the echoed sub-command.</param>
    /// <param name="atUtc">When the response arrived.</param>
    /// <param name="mode">
    /// The mode the radio is believed to be in, needed to turn a filter index
    /// into hertz. Null when it has not been read.
    /// </param>
    /// <param name="filterName">
    /// The selected filter designator, needed for FM whose width is fixed per
    /// slot. Null when it has not been read.
    /// </param>
    /// <param name="sourceOverride">
    /// What to record as the provenance, when it is not the read's own label.
    /// A mode the radio volunteered came from a transceive broadcast rather
    /// than from anybody asking, and the diagnostics screen has to be able to
    /// say which (§0.0.1).
    /// </param>
    /// <returns>One or more values. Never empty.</returns>
    public static IReadOnlyList<RigValue> Values(
        CivRead read,
        ReadOnlySpan<byte> payload,
        DateTime atUtc,
        CivMode? mode,
        string? filterName,
        string? sourceOverride = null)
    {
        var field = read.Field;
        var source = sourceOverride ?? read.Label;

        switch (field)
        {
            case RigField.Mode:
                return DecodeModeAndFilter(payload, atUtc, source);

            case RigField.FilterBandwidth:
                return One(DecodeFilterBandwidth(payload, atUtc, source, mode, filterName));

            case RigField.SMeter:
                return One(DecodeSMeter(payload, atUtc, source));

            case RigField.TransmitStatus:
                return One(DecodeChoice(
                    field, payload, atUtc, source, "receiving", "transmitting"));

            case RigField.Overflow:
                return One(DecodeChoice(
                    field, payload, atUtc, source, "not overloading", "overloading"));

            case RigField.SquelchStatus:
                return One(DecodeChoice(field, payload, atUtc, source, "closed", "open"));

            case RigField.Split:
            case RigField.NoiseBlanker:
            case RigField.NoiseReduction:
            case RigField.AutoNotch:
            case RigField.ManualNotch:
                return One(DecodeChoice(field, payload, atUtc, source, "off", "on"));

            case RigField.AccUsbSquelch:
                return One(DecodeChoice(
                    field, payload, atUtc, source,
                    "open regardless of squelch", "gated by the squelch"));

            case RigField.AccUsbOutputSelect:
                return One(DecodeChoice(field, payload, atUtc, source, "AF", "IF"));

            case RigField.Agc:
                return One(DecodeTable(
                    field, payload, atUtc, source,
                    new Dictionary<byte, string> { [1] = "FAST", [2] = "MID", [3] = "SLOW" }));

            case RigField.Preamp:
                return One(DecodeTable(
                    field, payload, atUtc, source,
                    new Dictionary<byte, string>
                    {
                        [0] = "off", [1] = "preamp 1", [2] = "preamp 2",
                    }));

            case RigField.BreakIn:
                return One(DecodeTable(
                    field, payload, atUtc, source,
                    new Dictionary<byte, string>
                    {
                        [0] = "off", [1] = "semi", [2] = "full",
                    }));

            case RigField.Attenuator:
                return One(DecodeAttenuator(payload, atUtc, source));

            case RigField.KeyerSpeed:
                return One(DecodeKeyerSpeed(payload, atUtc, source));

            case RigField.CwPitch:
                return One(DecodeCwPitch(payload, atUtc, source));

            case RigField.RfPower:
            case RigField.RfGain:
            case RigField.Squelch:
            case RigField.NoiseBlankerLevel:
            case RigField.NoiseReductionLevel:
            case RigField.AccUsbAfLevel:
                return One(DecodePercent(field, payload, atUtc, source));

            default:
                return One(RigValue.Unknown(field, $"{source} has no decoder"));
        }
    }

    private static IReadOnlyList<RigValue> One(RigValue value) => new[] { value };

    /// <summary>
    /// Mode and filter arrive together, so one command answers two fields
    /// (p. 19-9).
    /// </summary>
    private static IReadOnlyList<RigValue> DecodeModeAndFilter(
        ReadOnlySpan<byte> payload, DateTime atUtc, string source)
    {
        if (payload.Length < 1 || CivValues.Mode(payload[0]) is not { } mode)
        {
            return new[]
            {
                RigValue.Unknown(RigField.Mode, $"{source} gave an undocumented mode byte"),
                RigValue.Unknown(RigField.FilterSelection, $"{source} gave an unreadable reply"),
            };
        }

        var modeValue = RigValue.Known(
            RigField.Mode, (int)mode, CivValues.Name(mode), atUtc, source);

        // The manual allows the filter byte to be omitted (p. 19-9), so a
        // one-byte reply is a valid mode reading with no filter in it.
        if (payload.Length < 2 || CivValues.FilterName(payload[1]) is not { } filter)
        {
            return new[]
            {
                modeValue,
                RigValue.Unknown(RigField.FilterSelection, $"{source} carried no filter byte"),
            };
        }

        return new[]
        {
            modeValue,
            RigValue.Known(RigField.FilterSelection, payload[1], filter, atUtc, source),
        };
    }

    /// <summary>
    /// The filter index turned into hertz, which needs the mode to know which
    /// scale applies.
    /// </summary>
    private static RigValue DecodeFilterBandwidth(
        ReadOnlySpan<byte> payload,
        DateTime atUtc,
        string source,
        CivMode? mode,
        string? filterName)
    {
        // FM's passband is fixed per filter slot and this command says nothing
        // about it, so the width comes from the designator instead (p. 4-6).
        if (mode == CivMode.Fm)
        {
            return CivFilterWidth.FixedFmHertz(filterName) is { } fmHertz
                ? RigValue.Known(
                    RigField.FilterBandwidth, fmHertz,
                    CivFilterWidth.Describe(fmHertz), atUtc,
                    "Full Manual p. 4-6, fixed in FM")
                : RigValue.Unknown(
                    RigField.FilterBandwidth,
                    "FM widths are fixed per filter and the filter is not known yet");
        }

        if (mode is null)
        {
            // The index is meaningless without knowing which scale it is on,
            // and guessing the scale would report 2.4 kHz as 600 Hz.
            return RigValue.Unknown(
                RigField.FilterBandwidth,
                $"{source} answered, but the width scale depends on the mode and that is not known yet");
        }

        if (payload.Length < 1 || CivValues.Level(0x00, payload[0]) is not { } index)
        {
            return RigValue.Unknown(RigField.FilterBandwidth, $"{source} gave an unreadable reply");
        }

        return CivFilterWidth.Hertz(index, mode.Value) is { } hertz
            ? RigValue.Known(
                RigField.FilterBandwidth, hertz, CivFilterWidth.Describe(hertz), atUtc, source)
            : RigValue.Unknown(
                RigField.FilterBandwidth, $"{source} gave index {index}, outside the documented scale");
    }

    private static RigValue DecodeSMeter(
        ReadOnlySpan<byte> payload, DateTime atUtc, string source)
        => payload.Length >= 2 && CivValues.Level(payload[0], payload[1]) is { } reading
            ? RigValue.Known(
                RigField.SMeter, reading, CivSMeter.Describe(reading), atUtc, source)
            : RigValue.Unknown(RigField.SMeter, $"{source} gave an unreadable reply");

    private static RigValue DecodeChoice(
        RigField field,
        ReadOnlySpan<byte> payload,
        DateTime atUtc,
        string source,
        string whenZero,
        string whenOne)
        => payload.Length >= 1 && payload[0] <= 1
            ? RigValue.Known(
                field, payload[0], payload[0] == 0 ? whenZero : whenOne, atUtc, source)
            : RigValue.Unknown(field, $"{source} gave an unreadable reply");

    private static RigValue DecodeTable(
        RigField field,
        ReadOnlySpan<byte> payload,
        DateTime atUtc,
        string source,
        IReadOnlyDictionary<byte, string> table)
        => payload.Length >= 1 && table.TryGetValue(payload[0], out var text)
            ? RigValue.Known(field, payload[0], text, atUtc, source)
            : RigValue.Unknown(field, $"{source} gave an undocumented value");

    /// <summary>
    /// The attenuator, whose value is the attenuation in decibels written as
    /// BCD rather than as an index (p. 19-3).
    /// </summary>
    private static RigValue DecodeAttenuator(
        ReadOnlySpan<byte> payload, DateTime atUtc, string source)
    {
        if (payload.Length < 1 || CivValues.Level(0x00, payload[0]) is not { } decibels)
        {
            return RigValue.Unknown(RigField.Attenuator, $"{source} gave an unreadable reply");
        }

        return RigValue.Known(
            RigField.Attenuator, decibels,
            decibels == 0 ? "off" : $"{decibels} dB", atUtc, source);
    }

    /// <summary>
    /// The keyer speed, whose scale runs 6 to 48 words a minute across the
    /// usual 0 to 255 (p. 19-3).
    /// </summary>
    private static RigValue DecodeKeyerSpeed(
        ReadOnlySpan<byte> payload, DateTime atUtc, string source)
    {
        if (payload.Length < 2 || CivValues.Level(payload[0], payload[1]) is not { } level)
        {
            return RigValue.Unknown(RigField.KeyerSpeed, $"{source} gave an unreadable reply");
        }

        var wpm = (int)Math.Round(6 + (level * (48 - 6) / 255.0));
        return RigValue.Known(RigField.KeyerSpeed, wpm, $"{wpm} WPM", atUtc, source);
    }

    /// <summary>
    /// The CW pitch, 300 to 900 Hz in 5 Hz steps across the usual 0 to 255
    /// (p. 19-3).
    /// </summary>
    private static RigValue DecodeCwPitch(
        ReadOnlySpan<byte> payload, DateTime atUtc, string source)
    {
        if (payload.Length < 2 || CivValues.Level(payload[0], payload[1]) is not { } level)
        {
            return RigValue.Unknown(RigField.CwPitch, $"{source} gave an unreadable reply");
        }

        // Three anchors and a step size: 00 00 is 300 Hz, 01 28 is 600 Hz,
        // 02 55 is 900 Hz, in 5 Hz steps (p. 19-3). A single straight line
        // through the ends misses the middle anchor by a hertz, so the scale is
        // read as two lines that meet at it, then rounded to the documented
        // step. Getting this wrong by a few hertz would matter: the decoder is
        // told where to start listening.
        var interpolated = level <= 128
            ? 300 + (level * 300.0 / 128)
            : 600 + ((level - 128) * 300.0 / 127);

        var hertz = (int)(Math.Round(interpolated / 5) * 5);
        return RigValue.Known(RigField.CwPitch, hertz, $"{hertz} Hz", atUtc, source);
    }

    private static RigValue DecodePercent(
        RigField field, ReadOnlySpan<byte> payload, DateTime atUtc, string source)
    {
        if (payload.Length < 2 || CivValues.Level(payload[0], payload[1]) is not { } level)
        {
            return RigValue.Unknown(field, $"{source} gave an unreadable reply");
        }

        var percent = (int)Math.Round(level * 100.0 / 255);
        return RigValue.Known(field, percent, $"{percent}%", atUtc, source);
    }
}
