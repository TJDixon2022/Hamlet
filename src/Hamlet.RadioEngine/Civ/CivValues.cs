namespace Hamlet.RadioEngine.Civ;

/// <summary>The operating modes the IC-7300 reports over CI-V.</summary>
/// <remarks>
/// The byte values are the manual's (p. 19-9). Note that 06 is absent from the
/// table and so is absent here, rather than being filled in with a guess.
/// </remarks>
public enum CivMode
{
    /// <summary>Lower sideband.</summary>
    Lsb = 0x00,

    /// <summary>Upper sideband.</summary>
    Usb = 0x01,

    /// <summary>Amplitude modulation.</summary>
    Am = 0x02,

    /// <summary>Morse.</summary>
    Cw = 0x03,

    /// <summary>Radioteletype.</summary>
    Rtty = 0x04,

    /// <summary>Frequency modulation.</summary>
    Fm = 0x05,

    /// <summary>Morse, reversed sideband.</summary>
    CwReverse = 0x07,

    /// <summary>Radioteletype, reversed.</summary>
    RttyReverse = 0x08,
}

/// <summary>
/// Turning CI-V bytes into the values they stand for, and refusing when they
/// stand for nothing.
/// </summary>
/// <remarks>
/// Every conversion here cites the manual page it came from (HM-DEC-049). A
/// byte outside the documented set produces null rather than a nearest match,
/// because a mode badge showing the wrong mode is the prime directive broken on
/// the app's most-read surface (§0.0).
/// </remarks>
public static class CivValues
{
    /// <summary>A two-byte CI-V level, 00 00 to 02 55 (p. 19-3).</summary>
    /// <param name="high">The hundreds byte, BCD.</param>
    /// <param name="low">The tens and units byte, BCD.</param>
    /// <returns>0 to 255, or null when either byte is not valid BCD.</returns>
    /// <remarks>
    /// The IC-7300 writes these levels as decimal digits rather than as a plain
    /// number, so 128 arrives as 01 28 and not as 0x80. Reading it as a plain
    /// byte would put the CW pitch at 428 Hz when the radio says 600.
    /// </remarks>
    public static int? Level(byte high, byte low)
    {
        var hundreds = Digits(high);
        var rest = Digits(low);

        return hundreds is null || rest is null ? null : (hundreds * 100) + rest;
    }

    /// <summary>The two BCD digits in a byte as a number, or null.</summary>
    private static int? Digits(byte value)
    {
        var high = (value >> 4) & 0x0F;
        var low = value & 0x0F;
        return high > 9 || low > 9 ? null : (high * 10) + low;
    }

    /// <summary>The mode a byte stands for, or null when it stands for none.</summary>
    /// <param name="value">The mode byte.</param>
    /// <returns>The mode, or null.</returns>
    public static CivMode? Mode(byte value)
        => Enum.IsDefined(typeof(CivMode), (int)value) ? (CivMode)value : null;

    /// <summary>What to call a mode on screen.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The badge text, as the radio's own display shows it.</returns>
    public static string Name(CivMode mode) => mode switch
    {
        CivMode.Lsb => "LSB",
        CivMode.Usb => "USB",
        CivMode.Am => "AM",
        CivMode.Cw => "CW",
        CivMode.Rtty => "RTTY",
        CivMode.Fm => "FM",
        CivMode.CwReverse => "CW-R",
        _ => "RTTY-R",
    };

    /// <summary>True when this mode carries Morse.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>True for CW and CW reverse.</returns>
    public static bool IsCw(CivMode mode)
        => mode is CivMode.Cw or CivMode.CwReverse;

    /// <summary>The filter designator a byte stands for, or null.</summary>
    /// <param name="value">The filter byte: 01, 02 or 03 (p. 19-9).</param>
    /// <returns>"FIL1", "FIL2", "FIL3", or null.</returns>
    public static string? FilterName(byte value) => value switch
    {
        0x01 => "FIL1",
        0x02 => "FIL2",
        0x03 => "FIL3",
        _ => null,
    };
    /// <summary>A frequency in the form the diagnostics screen shows it.</summary>
    /// <param name="hz">The frequency in hertz.</param>
    /// <returns>e.g. "7.030000 MHz".</returns>
    /// <remarks>
    /// Six decimal places, which is the resolution the radio tunes at, and an
    /// invariant decimal point so a paste into a bug report reads the same on
    /// every machine.
    /// </remarks>
    public static string FrequencyText(long hz)
        => (hz / 1_000_000.0).ToString(
               "0.000000", System.Globalization.CultureInfo.InvariantCulture)
           + " MHz";
}

/// <summary>
/// The IF filter's width in hertz, from the index CI-V reports.
/// </summary>
/// <remarks>
/// <para>THE NUMBER THIS WHOLE SUBSYSTEM EXISTS FOR. Command <c>1A 03</c>
/// returns a position on a scale rather than a bandwidth, and the scale is not
/// linear: the command table on p. 19-4 gives only its endpoints and the steps
/// between them are on p. 4-6 of the Full Manual. Both pages verified against
/// publication A7292-4EX-6 (HM-DEC-071).</para>
/// <para>Two scales, and which one applies depends on the mode. SSB, CW and
/// RTTY step 50 Hz at a time from 50 Hz to 500 Hz and then 100 Hz at a time up
/// to 3.6 kHz. AM steps 200 Hz at a time from 200 Hz to 10 kHz. FM cannot be
/// adjusted at all: its three filters are fixed at 15 kHz, 10 kHz and 7 kHz, so
/// the width comes from which filter is selected rather than from
/// <c>1A 03</c>.</para>
/// <para>The command table's "31/40=2700 Hz/3600 Hz" is the two upper limits:
/// RTTY stops at 2.7 kHz and SSB and CW reach 3.6 kHz. Both fall out of the
/// same arithmetic, so there is one formula rather than a table of forty
/// entries to mistype.</para>
/// </remarks>
public static class CivFilterWidth
{
    /// <summary>The highest index the radio reports.</summary>
    public const int MaximumIndex = 49;

    /// <summary>
    /// The width in hertz for an index, or null when the mode has no adjustable
    /// width or the index is outside the documented range.
    /// </summary>
    /// <param name="index">The value from <c>1A 03</c>.</param>
    /// <param name="mode">The mode the radio is in.</param>
    /// <returns>Width in hertz, or null.</returns>
    public static int? Hertz(int index, CivMode mode)
    {
        if (index < 0 || index > MaximumIndex)
        {
            return null;
        }

        // FM's passband is not adjustable, so this index says nothing about it
        // (p. 4-6). Its width comes from FixedFmHertz instead.
        if (mode == CivMode.Fm)
        {
            return null;
        }

        if (mode == CivMode.Am)
        {
            // 200 Hz to 10 kHz in 200 Hz steps.
            return 200 + (index * 200);
        }

        // 50 Hz to 500 Hz in 50 Hz steps, then 600 Hz upward in 100 Hz steps.
        return index < 10
            ? 50 + (index * 50)
            : 600 + ((index - 10) * 100);
    }

    /// <summary>
    /// FM's fixed passband width for a filter designator, in hertz.
    /// </summary>
    /// <param name="filter">"FIL1", "FIL2" or "FIL3".</param>
    /// <returns>Width in hertz, or null for an unknown designator.</returns>
    /// <remarks>
    /// From p. 4-6's table rather than from any command: in FM the width is a
    /// property of the filter slot and there is nothing to read.
    /// </remarks>
    public static int? FixedFmHertz(string? filter) => filter switch
    {
        "FIL1" => 15_000,
        "FIL2" => 10_000,
        "FIL3" => 7_000,
        _ => null,
    };

    /// <summary>A bandwidth in the words an operator uses.</summary>
    /// <param name="hertz">The width.</param>
    /// <returns>e.g. "500 Hz" or "2.4 kHz".</returns>
    public static string Describe(int hertz)
        => hertz >= 1000
            ? $"{hertz / 1000.0:0.##} kHz"
            : $"{hertz} Hz";
}

/// <summary>
/// The S-meter, from the number CI-V reports to the number an operator says.
/// </summary>
/// <remarks>
/// <para>Three points are documented and nothing between them: 00 00 is S0,
/// 01 20 is S9, and 02 41 is S9+60 dB (p. 19-3). So the conversion is a
/// straight line through each half, which is what the scale on the radio's own
/// display does, and it is stated as an approximation rather than dressed up as
/// a calibration.</para>
/// <para>AN S-METER IS NOT A MEASUREMENT INSTRUMENT and this code does not
/// pretend otherwise. It is a relative indication whose absolute accuracy the
/// manual never claims, which is why nothing here converts it to microvolts or
/// decibels above a femtowatt. Reporting "S7" is honest; reporting "-97 dBm"
/// would be inventing precision (§0.0).</para>
/// </remarks>
public static class CivSMeter
{
    /// <summary>The reported value at S0.</summary>
    public const int ZeroReading = 0;

    /// <summary>The reported value at S9.</summary>
    public const int S9Reading = 120;

    /// <summary>The reported value at S9 plus 60 decibels.</summary>
    public const int S9Plus60Reading = 241;

    /// <summary>
    /// Where S9 sits along the scale, as a share of its width.
    /// </summary>
    /// <remarks>
    /// A radio's S-meter is not linear in its own reported units: the nine
    /// S-units take most of the scale and the decibels above S9 are compressed
    /// into what is left. The IC-7300's face puts S9 about sixty per cent along,
    /// and the display Hamlet draws follows it, so the two agree.
    /// </remarks>
    public const double S9Position = 0.6;

    /// <summary>
    /// Where a reading sits on the meter, from 0 at rest to 1 at full scale.
    /// </summary>
    /// <param name="reading">The value from <c>15 02</c>.</param>
    /// <returns>0 to 1.</returns>
    /// <remarks>
    /// Two straight lines meeting at S9 rather than one across the whole range.
    /// A single line would put S9 at the halfway mark, and every reading would
    /// then sit to the left of where the radio's own needle is, which is the
    /// sort of quiet disagreement that makes somebody distrust both.
    /// </remarks>
    public static double Fraction(int reading)
    {
        var held = Math.Clamp(reading, ZeroReading, S9Plus60Reading);

        return held <= S9Reading
            ? held * S9Position / S9Reading
            : S9Position
              + ((held - S9Reading) * (1 - S9Position) / (S9Plus60Reading - S9Reading));
    }

    /// <summary>The reading in the words an operator would use.</summary>
    /// <param name="reading">The value from <c>15 02</c>.</param>
    /// <returns>e.g. "S7" or "S9+20".</returns>
    public static string Describe(int reading)
    {
        if (reading <= ZeroReading)
        {
            return "S0";
        }

        if (reading < S9Reading)
        {
            // Nine S-units spread evenly up to S9, which is what the radio's
            // own scale shows.
            var units = (int)Math.Round(reading * 9.0 / S9Reading);
            return $"S{Math.Clamp(units, 0, 9)}";
        }

        // Above S9 the scale is in decibels over, and the radio marks it in
        // tens. Rounding to the nearest ten matches the marks rather than
        // claiming a resolution the meter does not have.
        var overDb = (reading - S9Reading) * 60.0 / (S9Plus60Reading - S9Reading);
        var rounded = (int)(Math.Round(overDb / 10) * 10);

        return rounded <= 0 ? "S9" : $"S9+{rounded}";
    }
}

/// <summary>
/// The SWR meter's own scale, from the radio (HM-DEC-081).
/// </summary>
/// <remarks>
/// <para>Four points are cited on p. 19-3 and nothing between them is: `0000` is
/// 1.0, `0048` is 1.5, `0080` is 2.0 and `0120` is 3.0. So the conversion is
/// linear between the cited points and refuses past the last one, rather than
/// extrapolating a curve nobody published (§4).</para>
/// <para>THE NUMBERS ARE THE MANUAL'S DECIMAL COLUMN, not hexadecimal. Reading
/// `0120` as hex would give 288 and a wildly wrong ratio, which is the mistake
/// §4 records against the CW pitch and the S-meter.</para>
/// </remarks>
public static class CivSwr
{
    /// <summary>The cited points: reading, and the ratio it means.</summary>
    private static readonly (int Reading, double Ratio)[] Scale =
    {
        (0, 1.0),
        (48, 1.5),
        (80, 2.0),
        (120, 3.0),
    };

    /// <summary>At or below this the antenna is matched (Full Manual p. 11-2).</summary>
    public const double MatchedBelow = 1.5;

    /// <summary>
    /// The ratio for a reading, or null when it is off the cited scale.
    /// </summary>
    /// <param name="reading">The value from <c>15 12</c>, 0 to 255.</param>
    /// <returns>The ratio, or null.</returns>
    /// <remarks>
    /// Null past the last cited point rather than a guess. "Higher than 3" is
    /// what the manual supports and it is also everything the operator needs to
    /// know, since anything up there wants the same action.
    /// </remarks>
    public static double? Ratio(int reading)
    {
        if (reading < 0)
        {
            return null;
        }

        if (reading >= Scale[^1].Reading)
        {
            return reading == Scale[^1].Reading ? Scale[^1].Ratio : null;
        }

        for (var i = 1; i < Scale.Length; i++)
        {
            if (reading > Scale[i].Reading)
            {
                continue;
            }

            var (lowReading, lowRatio) = Scale[i - 1];
            var (highReading, highRatio) = Scale[i];
            var span = highReading - lowReading;

            var ratio = lowRatio
                + ((reading - lowReading) / (double)span * (highRatio - lowRatio));

            return Math.Round(ratio, 2);
        }

        return null;
    }

    /// <summary>How to write a ratio for the operator.</summary>
    /// <param name="reading">The raw reading.</param>
    /// <returns>e.g. "1.3 to 1", or "higher than 3 to 1".</returns>
    public static string Describe(int reading)
        => Ratio(reading) is { } ratio
            ? $"{ratio.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} to 1"
            : "higher than 3 to 1";
}
