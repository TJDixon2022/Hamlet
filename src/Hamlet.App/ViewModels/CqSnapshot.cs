using System.Globalization;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>One station calling CQ on the decoded list.</summary>
/// <param name="Callsign">Who is calling.</param>
/// <param name="Grid">The grid his CQ carried, or "" where it carried none.</param>
/// <param name="HeardUtc">When the slot opened, as the list's `hhmmss`.</param>
public sealed record CqCall(string Callsign, string Grid, string HeardUtc);

/// <summary>
/// **The CQ list as the decoded list held it, and the moment it was read** (work instruction 335
/// task 3).
/// </summary>
/// <remarks>
/// <para>**READ ONCE, WITH ITS TIME, AND NOT KEPT LIVE** - the second of the instruction's two
/// choices. The decoded list has no recency rule of its own: an FT8 row stays until the 500-row
/// cap, a large retune or Clear. So there is no *current* here beyond *on the list*, and a
/// *right now* that went on standing behind a modal dialog would assert a freshness it lacks
/// (HM-DEC-111). **The card says when the list was read instead**, and no recency number is
/// invented.</para>
/// <para>**A CQ IS WHAT THE DECODED LIST'S OWN CQ TOGGLE CALLS ONE**:
/// `Ft8MessageSplit.IsCallToAnyone` on the row's addressee, from a row with a sender, that this
/// station did not send. **The grid is the payload only where it is a grid**
/// (`Ft8MessageSplit.IsGrid`); a PSK31 row has no fields and so no grid.</para>
/// <para>**IT READS AND DOES NOT DECODE.** Nothing here touches how a row came to be on the list
/// (§10 of the instruction).</para>
/// </remarks>
public sealed class CqSnapshot
{
    private CqSnapshot(IReadOnlyList<CqCall> calls, DateTime? readUtc)
    {
        Calls = calls;
        ReadUtc = readUtc;
    }

    /// <summary>No list was handed in.</summary>
    public static CqSnapshot None { get; } = new(Array.Empty<CqCall>(), null);

    /// <summary>Each station calling CQ, once.</summary>
    public IReadOnlyList<CqCall> Calls { get; }

    /// <summary>When the list was read, or null where none was handed in.</summary>
    public DateTime? ReadUtc { get; }

    /// <summary>The time the list was read, as a person reads it: `21:41 UTC`, or "".</summary>
    public string ReadAt
        => ReadUtc is { } at ? at.ToString("HH:mm", CultureInfo.InvariantCulture) + " UTC" : "";

    /// <summary>Read the CQ calls off the decoded rows.</summary>
    /// <param name="rows">The decoded list's rows, as it holds them.</param>
    /// <param name="readUtc">When they were read.</param>
    /// <returns>Each caller once, preferring a call that carried a grid.</returns>
    public static CqSnapshot From(IEnumerable<DigitalDecodeRow> rows, DateTime readUtc)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var calls = rows
            .Where(r => !r.IsSent
                && r.Sender.Length > 0
                && Ft8MessageSplit.IsCallToAnyone(r.Addressee))
            .Select(r => new CqCall(
                r.Sender.Trim().ToUpperInvariant(),
                Ft8MessageSplit.IsGrid(r.Payload) ? r.Payload : "",
                r.Utc))
            .GroupBy(c => c.Callsign, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.FirstOrDefault(c => c.Grid.Length > 0) ?? g.First())
            .ToList();

        return new CqSnapshot(calls, readUtc);
    }
}

/// <summary>One caller on a next card: where he is, and who and how far.</summary>
/// <param name="Place">The country or the square that working him would earn.</param>
/// <param name="CallLine">`OE8DDX · 4,500 mi`, or the callsign alone where he sent no grid.</param>
/// <param name="OpensContinent">
/// True where he is on a continent the log has never reached, so the mark beside him is the
/// ringed door and not the still counter - the same two forms his row on the decoded list wears
/// (§R16).
/// </param>
public sealed record NextCaller(string Place, string CallLine, bool OpensContinent = false)
{
    /// <summary>True where the mark is the still counter.</summary>
    public bool IsCounter => !OpensContinent;
}
