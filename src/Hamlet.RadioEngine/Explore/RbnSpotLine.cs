using System.Globalization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One parsed line of the Reverse Beacon Network telnet feed.</summary>
/// <param name="Spotter">The skimmer that heard it, decoration stripped.</param>
/// <param name="FrequencyHz">Reported frequency.</param>
/// <param name="DxCall">The station heard.</param>
/// <param name="Mode">Reported mode, e.g. "CW".</param>
/// <param name="SignalDb">Reported signal-to-noise in dB.</param>
/// <param name="Wpm">Reported CW speed, when the line carries one.</param>
/// <param name="CallType">CQ, beacon, contest exchange, or unknown.</param>
/// <param name="HeardAtUtc">When the skimmer heard it.</param>
public sealed record RbnSpot(
    string Spotter, long FrequencyHz, string DxCall, string Mode,
    int? SignalDb, int? Wpm, SpotCallType CallType, DateTime HeardAtUtc);

/// <summary>
/// Reads the Reverse Beacon Network's line format.
/// </summary>
/// <remarks>
/// <para>A line looks like this, verified against the live feed on
/// 2026-08-13:</para>
/// <code>
/// DX de WE9V-#:   14047.90  NZ1J           CW    17 dB  15 WPM  CQ      1513Z
/// DX de DL8LAS-3-#: 14046.00  OE3WHU/QRP   CW     7 dB  15 WPM  CQ      1513Z
/// DX de K5TR-#:   28254.50  K4JEE/B        CW    20 dB  15 WPM  BEACON  1513Z
/// </code>
/// <para>Columns are whitespace-aligned but not fixed-width — the spotter
/// field runs long enough on some skimmers to eat its own padding — so this
/// parses by token and by landmark ("dB", "WPM", the trailing time) rather
/// than by column offset. Anything that does not fit returns null; a line
/// this parser does not understand is dropped, never half-read into a spot
/// (HM-DEC-009).</para>
/// </remarks>
public static class RbnSpotLine
{
    /// <summary>The prefix every spot line carries.</summary>
    public const string LinePrefix = "DX de ";

    /// <summary>
    /// Parse one line of the feed.
    /// </summary>
    /// <param name="line">A raw line from the telnet stream.</param>
    /// <param name="nowUtc">Current time, used to place the line's HHMM stamp
    /// on a date and to resolve the midnight wrap. Passed in, never read from
    /// a clock inside the parser (§5).</param>
    /// <returns>The spot, or null when the line is not a parseable spot.</returns>
    public static RbnSpot? Parse(string? line, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(line)
            || !line.StartsWith(LinePrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var tokens = line[LinePrefix.Length..]
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        // spotter: freq dx mode snr dB wpm WPM type time
        if (tokens.Length < 8)
        {
            return null;
        }

        // "WE9V-#:" and "DL8LAS-3-#:" — RBN marks skimmer calls with a "-#"
        // decoration that is not part of the callsign. It is stripped here so
        // everything downstream, including the text shown to the operator,
        // sees the call the station actually holds.
        var spotter = tokens[0].TrimEnd(':');
        var dash = spotter.IndexOf('-');
        if (dash >= 0)
        {
            spotter = spotter[..dash];
        }

        if (spotter.Length == 0)
        {
            return null;
        }

        if (!double.TryParse(
                tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var khz)
            || khz <= 0)
        {
            return null;
        }

        var dxCall = tokens[2];
        var mode = tokens[3].ToUpperInvariant();

        var signalDb = ValueBefore(tokens, "dB");
        var wpm = ValueBefore(tokens, "WPM");

        var timeIndex = LastTimeIndex(tokens);
        if (timeIndex < 0)
        {
            return null;
        }

        var wpmIndex = IndexOf(tokens, "WPM");
        var typeStart = wpmIndex >= 0 ? wpmIndex + 1 : timeIndex;
        var callType = ReadCallType(tokens, typeStart, timeIndex);

        return new RbnSpot(
            spotter,
            (long)Math.Round(khz * 1000.0),
            dxCall,
            mode.Length == 0 ? "CW" : mode,
            signalDb,
            wpm,
            callType,
            ResolveTime(tokens[timeIndex], nowUtc));
    }

    /// <summary>
    /// Place an HHMMZ stamp on a date.
    /// </summary>
    /// <param name="stamp">Token such as "1513Z".</param>
    /// <param name="nowUtc">Current time.</param>
    /// <returns>The instant the stamp refers to.</returns>
    /// <remarks>
    /// The feed states a time of day and no date. Just after midnight UTC a
    /// "2358Z" line belongs to yesterday, so a stamp that would otherwise sit
    /// in the future is rolled back a day rather than being shown as a spot
    /// that has not happened yet.
    /// </remarks>
    internal static DateTime ResolveTime(string stamp, DateTime nowUtc)
    {
        var digits = stamp.TrimEnd('Z', 'z');
        if (digits.Length != 4
            || !int.TryParse(digits[..2], out var hour)
            || !int.TryParse(digits[2..], out var minute)
            || hour > 23 || minute > 59)
        {
            return nowUtc;
        }

        var candidate = new DateTime(
            nowUtc.Year, nowUtc.Month, nowUtc.Day, hour, minute, 0, DateTimeKind.Utc);

        if (candidate - nowUtc > TimeSpan.FromHours(1))
        {
            candidate = candidate.AddDays(-1);
        }

        return candidate;
    }

    private static SpotCallType ReadCallType(string[] tokens, int start, int end)
    {
        for (var i = start; i < end && i < tokens.Length; i++)
        {
            switch (tokens[i].ToUpperInvariant())
            {
                case "CQ":
                    return SpotCallType.Cq;
                case "BEACON":
                case "NCDXF":
                    return SpotCallType.Beacon;
                case "DX":
                    return SpotCallType.Dx;
            }
        }

        // RBN reports contest exchanges by naming the exchange rather than
        // labelling them, so a line with a type this parser does not know is
        // treated as a contest exchange only when it named something at all.
        return end > start ? SpotCallType.Contest : SpotCallType.Unknown;
    }

    private static int LastTimeIndex(string[] tokens)
    {
        for (var i = tokens.Length - 1; i >= 0; i--)
        {
            var t = tokens[i];
            if (t.Length == 5
                && (t[4] is 'Z' or 'z')
                && t[..4].All(char.IsAsciiDigit))
            {
                return i;
            }
        }

        return -1;
    }

    private static int IndexOf(string[] tokens, string needle)
    {
        for (var i = 0; i < tokens.Length; i++)
        {
            if (string.Equals(tokens[i], needle, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static int? ValueBefore(string[] tokens, string unit)
    {
        var index = IndexOf(tokens, unit);
        if (index <= 0)
        {
            return null;
        }

        return int.TryParse(
            tokens[index - 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : null;
    }
}
