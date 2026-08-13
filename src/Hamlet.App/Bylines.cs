using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.App;

/// <summary>One line under the wordmark, and the play it was bent out of.</summary>
/// <param name="Text">The line itself.</param>
/// <param name="Source">The play or poem, e.g. "Macbeth".</param>
public sealed record Byline(string Text, string Source);

/// <summary>
/// Forty-five Shakespeare lines bent toward ham radio, one shown at random
/// each launch.
/// </summary>
/// <remarks>
/// <para>THE POINT IS JOY (HM-DEC-039). Ham radio is intimidating — that is
/// the whole reason this app exists — and a small daily chuckle costs nothing
/// and softens it. It is also the only thing in Hamlet that is there purely
/// to be liked, which is worth one file.</para>
/// <para>The source play is a tooltip rather than permanent text, so the joke
/// stays legible to somebody who does not know the original without the
/// wordmark turning into a citation.</para>
/// <para>Shakespeare died in 1616, so the source text is long out of copyright
/// and these alterations are the project's own — nothing here needs anybody's
/// permission (§2.1).</para>
/// <para>NEVER A PLACEHOLDER. If the file is missing, malformed or empty, the
/// byline simply is not there. A line reading "byline unavailable" under the
/// wordmark would be worse than the silence, and this is the one feature in
/// the app that must not be able to break anything (§8).</para>
/// </remarks>
public static class Bylines
{
    /// <summary>Resource name of the embedded bylines file.</summary>
    public const string ResourceName = "Hamlet.App.Data.bylines.json";

    private static readonly Lazy<IReadOnlyList<Byline>> Shared = new(LoadEmbedded);

    /// <summary>Every byline, or an empty list when the file did not load.</summary>
    public static IReadOnlyList<Byline> All => Shared.Value;

    /// <summary>
    /// Pick a byline that is not the one shown last time.
    /// </summary>
    /// <param name="lines">The available lines.</param>
    /// <param name="lastIndex">Index shown last launch, or −1.</param>
    /// <param name="next">Random source; takes an exclusive upper bound.</param>
    /// <returns>The chosen line and its index, or null when there are none.</returns>
    /// <remarks>
    /// The repeat is avoided by drawing from the other lines rather than by
    /// re-rolling until it differs: a loop would spin forever on a
    /// single-line file, and a file with one line is exactly the sort of
    /// thing somebody hand-edits.
    /// </remarks>
    public static (Byline Line, int Index)? Pick(
        IReadOnlyList<Byline> lines, int lastIndex, Func<int, int> next)
    {
        if (lines.Count == 0)
        {
            return null;
        }

        if (lines.Count == 1)
        {
            return (lines[0], 0);
        }

        if (lastIndex < 0 || lastIndex >= lines.Count)
        {
            var free = next(lines.Count);
            return (lines[free], free);
        }

        // Draw from the lines that are not the last one, then step past it.
        var pick = next(lines.Count - 1);
        var index = pick >= lastIndex ? pick + 1 : pick;

        return (lines[index], index);
    }

    /// <summary>
    /// Pick a byline from the embedded file, avoiding the last one shown.
    /// </summary>
    /// <param name="lastIndex">Index shown last launch, or −1.</param>
    /// <param name="next">Random source; takes an exclusive upper bound.</param>
    /// <returns>The chosen line and its index, or null when there are none.</returns>
    public static (Byline Line, int Index)? Pick(int lastIndex, Func<int, int>? next = null)
        => Pick(All, lastIndex, next ?? Random.Shared.Next);

    /// <summary>
    /// Read the embedded file, or return nothing at all.
    /// </summary>
    /// <remarks>
    /// Never throws. This runs while the main window is being constructed, and
    /// a decorative feature that could stop the app from opening would be a
    /// spectacularly bad trade.
    /// </remarks>
    private static IReadOnlyList<Byline> LoadEmbedded()
    {
        try
        {
            var assembly = typeof(Bylines).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);

            if (stream is null)
            {
                return Array.Empty<Byline>();
            }

            var file = JsonSerializer.Deserialize<BylineFile>(stream, Json);

            if (file?.Bylines is null)
            {
                return Array.Empty<Byline>();
            }

            var lines = new List<Byline>(file.Bylines.Count);
            foreach (var entry in file.Bylines)
            {
                if (!string.IsNullOrWhiteSpace(entry?.Text))
                {
                    lines.Add(new Byline(entry!.Text!.Trim(), (entry.Source ?? "").Trim()));
                }
            }

            return lines;
        }
        catch (Exception)
        {
            return Array.Empty<Byline>();
        }
    }

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class BylineFile
    {
        [JsonPropertyName("bylines")]
        public List<Entry?>? Bylines { get; set; }
    }

    private sealed class Entry
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }
    }
}
