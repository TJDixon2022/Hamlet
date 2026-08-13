using System.Globalization;

namespace Hamlet.App.Controls;

/// <summary>
/// One activity dot on the neighborhood map, with everything needed to draw
/// it and everything needed to tell the truth about it on hover.
/// </summary>
/// <param name="FrequencyHz">Where the spot is.</param>
/// <param name="Story">The plain-language invitation.</param>
/// <param name="Mode">Mode name, e.g. "CW".</param>
/// <param name="Source">Which network reported it.</param>
/// <param name="AgeText">Human-readable age, e.g. "3 min ago".</param>
/// <param name="Reason">Why the ranking placed it where it did.</param>
/// <param name="Prominence">0 to 1; 1 is the best-ranked spot on the band.
/// Drives dot size and brightness so the map and the list agree about what
/// matters (HM-DEC-023).</param>
public sealed record ActivityDot(
    long FrequencyHz,
    string Story,
    string Mode,
    string Source,
    string AgeText,
    string Reason,
    double Prominence)
{
    /// <summary>
    /// How far away and roughly which way, e.g. "480 miles northeast", or ""
    /// when the app cannot justify a figure (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Empty whenever the operator's grid is unknown or the source did not
    /// state where the station is. It is never estimated from a callsign or a
    /// location string, and the absence is the honest answer.
    /// </remarks>
    public string Distance { get; init; } = "";

    /// <summary>Frequency in megahertz, three decimals.</summary>
    public string FrequencyLabel
        => (FrequencyHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>
    /// What the hover tooltip shows: the story, then the same honesty fields
    /// every card carries — frequency, mode, source and age (HM-DEC-009).
    /// </summary>
    public string TooltipText
    {
        get
        {
            var head = Distance.Length > 0
                ? $"{FrequencyLabel} MHz · {Mode} · {Distance}"
                : $"{FrequencyLabel} MHz · {Mode}";

            var lines = new List<string>(4) { Story, head };

            if (Reason.Length > 0)
            {
                lines.Add(Reason);
            }

            lines.Add($"{Source} · {AgeText}");
            return string.Join("\n", lines);
        }
    }
}
