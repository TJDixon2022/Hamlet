using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One piece of the vocabulary, explained.</summary>
/// <param name="Term">The word or initials as they appear in copy.</param>
/// <param name="Expansion">What the initials stand for, or "" when they are
/// not initials.</param>
/// <param name="Explanation">Plain language, in the project voice.</param>
public sealed record GlossaryTerm(string Term, string Expansion, string Explanation)
{
    /// <summary>True when the term has an expansion worth showing.</summary>
    public bool HasExpansion => Expansion.Length > 0;

    /// <summary>
    /// The heading a tooltip shows: the term, and what it stands for when that
    /// is not obvious.
    /// </summary>
    public string Heading => HasExpansion ? $"{Term} ({Expansion})" : Term;
}

/// <summary>A run of copy, and whether it is a glossary term.</summary>
/// <param name="Text">The text of this run.</param>
/// <param name="Term">The term when this run is one, else null.</param>
public sealed record GlossarySpan(string Text, GlossaryTerm? Term)
{
    /// <summary>True when this run should be marked.</summary>
    public bool IsTerm => Term is not null;
}

/// <summary>
/// The vocabulary, and the machinery that finds it in ordinary copy.
/// </summary>
/// <remarks>
/// <para>THE VOCABULARY IS THE GATE (HM-DEC-041). This hobby runs on shared
/// shorthand, most of it inherited from telegraph operators who died before
/// anybody reading this was born, and none of it is written down anywhere a
/// newcomer would look. Handing out the dictionary is the most direct thing
/// Hamlet can do about that.</para>
/// <para>MARKING IS AUTOMATIC. Copy is scanned at render time rather than
/// hand-tagged, so a string written next month inherits the glossary for free
/// and adding a term lights it up everywhere it already appears. Hand-tagging
/// would guarantee the opposite: the copy and the glossary would drift apart
/// the first time somebody was in a hurry.</para>
/// <para>MATCHING IS CONSERVATIVE, because a false positive is worse than a
/// miss. Whole words only, so "band" does not fire inside "bandwidth".
/// Case-insensitive, so "Activator" and "activator" both catch. And never
/// inside a callsign or a frequency, because "K3CQ" contains CQ and
/// "14.074" contains nothing anybody wants underlined.</para>
/// <para>FIRST OCCURRENCE ONLY within one block of copy. A paragraph with
/// eleven dotted words in it reads as a language exercise rather than as
/// something a person wrote, and the second dot teaches nobody anything the
/// first did not.</para>
/// <para>Pure: a string and a term set in, a list of runs out. No clock, no
/// UI, no state (§5, §0.1).</para>
/// </remarks>
public static class Glossary
{
    /// <summary>Resource name of the embedded glossary file.</summary>
    public const string ResourceName = "Hamlet.RadioEngine.Data.glossary.json";

    private static readonly Lazy<IReadOnlyList<GlossaryTerm>> Shared = new(LoadEmbedded);

    private static readonly Lazy<IReadOnlyDictionary<string, GlossaryTerm>> Index =
        new(() => BuildIndex(Shared.Value));

    /// <summary>Every term, or an empty list when the file did not load.</summary>
    public static IReadOnlyList<GlossaryTerm> All => Shared.Value;

    /// <summary>
    /// Look a term up by its word, case-insensitively.
    /// </summary>
    /// <param name="word">The word as it appeared in copy.</param>
    /// <returns>The term, or null.</returns>
    public static GlossaryTerm? Find(string? word)
        => word is not null && Index.Value.TryGetValue(word.Trim(), out var term)
            ? term
            : null;

    /// <summary>
    /// Split a passage into runs, marking the first occurrence of each term.
    /// </summary>
    /// <param name="text">The copy as the operator will read it.</param>
    /// <returns>
    /// Runs in order. Joining every <see cref="GlossarySpan.Text"/> back
    /// together reproduces the input exactly, so a renderer cannot lose or
    /// duplicate a character by using this.
    /// </returns>
    public static IReadOnlyList<GlossarySpan> Mark(string? text)
        => Mark(text, Index.Value);

    /// <summary>
    /// Split a passage into runs against an explicit term set.
    /// </summary>
    /// <param name="text">The copy.</param>
    /// <param name="index">Terms, keyed case-insensitively by word.</param>
    /// <returns>Runs in order.</returns>
    /// <remarks>
    /// The overload tests drive, so the matching rules can be exercised
    /// against a handful of terms rather than against whatever happens to be
    /// in the shipped file.
    /// </remarks>
    public static IReadOnlyList<GlossarySpan> Mark(
        string? text, IReadOnlyDictionary<string, GlossaryTerm> index)
    {
        if (string.IsNullOrEmpty(text) || index.Count == 0)
        {
            return text is null or "" ? Array.Empty<GlossarySpan>() : new[] { new GlossarySpan(text, null) };
        }

        var longest = index.Keys.Max(k => k.Count(c => c == ' ')) + 1;
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var spans = new List<GlossarySpan>();

        var plainFrom = 0;
        var i = 0;

        while (i < text.Length)
        {
            if (!IsWordStart(text, i))
            {
                i++;
                continue;
            }

            var match = LongestMatchAt(text, i, index, used, longest);

            if (match is null)
            {
                // Step past the whole word, so a term can never be found
                // starting in the middle of a longer one.
                i = EndOfWord(text, i);
                continue;
            }

            var (term, length) = match.Value;

            if (i > plainFrom)
            {
                spans.Add(new GlossarySpan(text[plainFrom..i], null));
            }

            spans.Add(new GlossarySpan(text.Substring(i, length), term));
            used.Add(term.Term);

            i += length;
            plainFrom = i;
        }

        if (plainFrom < text.Length)
        {
            spans.Add(new GlossarySpan(text[plainFrom..], null));
        }

        return spans;
    }

    /// <summary>
    /// The longest unused term starting exactly at this position, or null.
    /// </summary>
    /// <remarks>
    /// Longest first, so "grid square" wins over "grid" and "rag chew" is not
    /// left half-marked.
    /// </remarks>
    private static (GlossaryTerm Term, int Length)? LongestMatchAt(
        string text,
        int start,
        IReadOnlyDictionary<string, GlossaryTerm> index,
        HashSet<string> used,
        int longestInWords)
    {
        var end = start;
        var words = 0;
        (GlossaryTerm Term, int Length)? best = null;

        while (words < longestInWords && end < text.Length)
        {
            end = EndOfWord(text, end);
            words++;

            var candidate = text[start..end];

            if (index.TryGetValue(candidate, out var term)
                && !used.Contains(term.Term)
                && IsWordEnd(text, end)
                && !SitsInsideACallsignOrFrequency(text, start, end))
            {
                best = (term, end - start);
            }

            // Extend across exactly one space to try a multi-word term.
            if (end < text.Length && text[end] == ' ')
            {
                end++;
            }
            else
            {
                break;
            }
        }

        return best;
    }

    private static int EndOfWord(string text, int from)
    {
        var i = from;
        while (i < text.Length && IsWordChar(text[i]))
        {
            i++;
        }

        return i == from ? from + 1 : i;
    }

    private static bool IsWordStart(string text, int i)
        => IsWordChar(text[i]) && (i == 0 || !IsWordChar(text[i - 1]));

    private static bool IsWordEnd(string text, int end)
        => end >= text.Length || !IsWordChar(text[end]);

    /// <summary>Letters, digits and the hyphen inside "break-in".</summary>
    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '-';

    /// <summary>
    /// True when this match is really part of a callsign or a frequency.
    /// </summary>
    /// <remarks>
    /// <para>The two cases that would otherwise embarrass the feature. "K3CQ"
    /// ends in CQ, and "7.030" is full of digits; underlining either would
    /// look like the app had misread something the operator can plainly see.
    /// </para>
    /// <para>The test is positional rather than semantic: if the characters
    /// touching the match are the ones that make up a callsign or a decimal
    /// number, the match is inside one. That is cheap, and it errs toward
    /// leaving text alone.</para>
    /// </remarks>
    private static bool SitsInsideACallsignOrFrequency(string text, int start, int end)
    {
        // A digit immediately either side means a number ran into the term.
        if (start > 0 && char.IsDigit(text[start - 1]))
        {
            return true;
        }

        if (end < text.Length && char.IsDigit(text[end]))
        {
            return true;
        }

        // A decimal point counts only when a digit sits on its far side. The
        // period ending a sentence is the commonest character in the app's
        // copy, and reading "on the band." as a frequency would silently stop
        // marking any term that happened to end a sentence.
        if (start > 1 && text[start - 1] == '.' && char.IsDigit(text[start - 2]))
        {
            return true;
        }

        if (end + 1 < text.Length && text[end] == '.' && char.IsDigit(text[end + 1]))
        {
            return true;
        }

        // Slash-suffixed calls: "JO4MJO/4" and "W1ABC/P".
        if (start > 0 && text[start - 1] == '/')
        {
            return true;
        }

        // A term made only of letters, sitting in a run that also holds a
        // digit, is part of a callsign: K3CQ, N0DX, VE3QRP.
        var runStart = start;
        while (runStart > 0 && IsWordChar(text[runStart - 1]))
        {
            runStart--;
        }

        var runEnd = end;
        while (runEnd < text.Length && IsWordChar(text[runEnd]))
        {
            runEnd++;
        }

        if (runStart == start && runEnd == end)
        {
            return false;
        }

        return text[runStart..runEnd].Any(char.IsDigit);
    }

    /// <summary>Build the lookup, keyed by the term itself.</summary>
    /// <param name="terms">The terms.</param>
    /// <returns>A case-insensitive index.</returns>
    public static IReadOnlyDictionary<string, GlossaryTerm> BuildIndex(
        IEnumerable<GlossaryTerm> terms)
    {
        var index = new Dictionary<string, GlossaryTerm>(StringComparer.OrdinalIgnoreCase);

        foreach (var term in terms)
        {
            index[term.Term] = term;
        }

        return index;
    }

    /// <summary>
    /// Read the embedded file, or return nothing at all.
    /// </summary>
    /// <remarks>
    /// Never throws. A glossary that failed to load means unmarked copy, which
    /// is exactly what the app looked like before this existed. A decorative
    /// layer that could stop the window opening would be a bad trade (§8).
    /// </remarks>
    private static IReadOnlyList<GlossaryTerm> LoadEmbedded()
    {
        try
        {
            var assembly = typeof(Glossary).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);

            if (stream is null)
            {
                return Array.Empty<GlossaryTerm>();
            }

            return Parse(stream);
        }
        catch (Exception)
        {
            return Array.Empty<GlossaryTerm>();
        }
    }

    /// <summary>
    /// Parse a glossary file, skipping anything malformed.
    /// </summary>
    /// <param name="stream">The file.</param>
    /// <returns>The usable terms, which may be none.</returns>
    public static IReadOnlyList<GlossaryTerm> Parse(Stream stream)
    {
        try
        {
            var file = JsonSerializer.Deserialize<GlossaryFile>(stream, Json);

            if (file?.Terms is null)
            {
                return Array.Empty<GlossaryTerm>();
            }

            var terms = new List<GlossaryTerm>(file.Terms.Count);

            foreach (var entry in file.Terms)
            {
                // A term with no explanation is worse than no term: it would
                // mark a word and then say nothing about it.
                if (entry is null
                    || string.IsNullOrWhiteSpace(entry.Term)
                    || string.IsNullOrWhiteSpace(entry.Explanation))
                {
                    continue;
                }

                terms.Add(new GlossaryTerm(
                    entry.Term!.Trim(),
                    (entry.Expansion ?? "").Trim(),
                    entry.Explanation!.Trim()));
            }

            return terms;
        }
        catch (JsonException)
        {
            return Array.Empty<GlossaryTerm>();
        }
    }

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class GlossaryFile
    {
        [JsonPropertyName("terms")]
        public List<Entry?>? Terms { get; set; }
    }

    private sealed class Entry
    {
        [JsonPropertyName("term")]
        public string? Term { get; set; }

        [JsonPropertyName("expansion")]
        public string? Expansion { get; set; }

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }
    }
}
