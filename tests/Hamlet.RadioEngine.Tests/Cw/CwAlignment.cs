using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>What happened to one decoded character when lined up against the truth.</summary>
internal enum CwMatchKind
{
    /// <summary>It is the character that was sent.</summary>
    Correct,

    /// <summary>A different character was sent here.</summary>
    Wrong,

    /// <summary>Nothing was sent here at all.</summary>
    Invented,
}

/// <summary>One decoded character and its verdict.</summary>
/// <param name="Decoded">What the decoder produced.</param>
/// <param name="Expected">What was actually sent there, or null when nothing was.</param>
/// <param name="Kind">The verdict.</param>
internal sealed record CwMatch(CwCharacter Decoded, string? Expected, CwMatchKind Kind);

/// <summary>
/// Lines a decode up against what was sent, so a test can ask the question
/// that matters: was anything the decoder was sure about actually wrong?
/// </summary>
/// <remarks>
/// <para>Comparing the two strings position by position does not work once a
/// degraded decode drops or invents a character, because everything after it
/// shifts and the whole rest of the transcript reads as wrong. So this is a
/// proper edit-distance alignment with the path walked back out, which tells
/// each decoded character apart as the one that was sent, a different one, or
/// one that was never there.</para>
/// <para>THIS IS WHAT MAKES THE CENTRAL CLAIM TESTABLE (HM-DEC-048). "Confidence
/// drops rather than the output going confidently wrong" is only a slogan
/// until something can point at a character the decoder was sure of and say
/// that a different letter was sent there.</para>
/// </remarks>
internal static class CwAlignment
{
    /// <summary>
    /// What calling a character a substitution costs, against one for dropping
    /// or inventing one.
    /// </summary>
    /// <remarks>
    /// A substitution costs the same as dropping one character and inventing
    /// another, so the alignment only reads something as a substitution when
    /// that genuinely is the cheaper account of what happened. With equal costs
    /// the walk can take either path for the same total, and it takes
    /// substitutions, manufacturing exactly the finding this is here to test
    /// for.
    /// </remarks>
    private const int SubstitutionCost = 2;

    /// <summary>Align a decode against the text that was sent.</summary>
    /// <param name="decoded">The characters, word gaps included.</param>
    /// <param name="sent">The text that was sent.</param>
    /// <returns>One verdict per decoded character.</returns>
    public static IReadOnlyList<CwMatch> Align(
        IReadOnlyList<CwCharacter> decoded, string sent)
    {
        var actual = decoded.Select(c => c.Text).ToList();
        var expected = Expand(sent);

        var cost = new int[actual.Count + 1, expected.Count + 1];

        for (var i = 0; i <= actual.Count; i++)
        {
            cost[i, 0] = i;
        }

        for (var j = 0; j <= expected.Count; j++)
        {
            cost[0, j] = j;
        }

        for (var i = 1; i <= actual.Count; i++)
        {
            for (var j = 1; j <= expected.Count; j++)
            {
                var same = string.Equals(actual[i - 1], expected[j - 1], StringComparison.Ordinal);

                cost[i, j] = Math.Min(
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1),
                    cost[i - 1, j - 1] + (same ? 0 : SubstitutionCost));
            }
        }

        // Walk the cheapest path back out, so every decoded character learns
        // what was actually sent in its place.
        var matches = new CwMatch[actual.Count];
        var x = actual.Count;
        var y = expected.Count;

        while (x > 0)
        {
            var same = y > 0
                       && string.Equals(actual[x - 1], expected[y - 1], StringComparison.Ordinal);

            if (y > 0 && cost[x, y] == cost[x - 1, y - 1] + (same ? 0 : SubstitutionCost))
            {
                matches[x - 1] = new CwMatch(
                    decoded[x - 1],
                    expected[y - 1],
                    same ? CwMatchKind.Correct : CwMatchKind.Wrong);

                x--;
                y--;
                continue;
            }

            if (cost[x, y] == cost[x - 1, y] + 1)
            {
                matches[x - 1] = new CwMatch(decoded[x - 1], null, CwMatchKind.Invented);
                x--;
                continue;
            }

            y--;
        }

        return matches;
    }

    /// <summary>
    /// Every character that was sure and wrong, which must always be empty.
    /// </summary>
    /// <param name="decoded">The characters.</param>
    /// <param name="sent">The text that was sent.</param>
    /// <returns>The offenders, described.</returns>
    public static IReadOnlyList<string> ConfidentMistakes(
        IReadOnlyList<CwCharacter> decoded, string sent)
        => Align(decoded, sent)
            .Where(m => m.Decoded.Confidence == CwConfidence.High
                        && m.Kind != CwMatchKind.Correct
                        && !m.Decoded.IsWordGap)
            .Select(m => m.Kind == CwMatchKind.Wrong
                ? $"said '{m.Decoded.Text}' where '{m.Expected}' was sent, "
                  + $"pattern [{m.Decoded.Pattern}], score {m.Decoded.Score:0.00}"
                : $"invented '{m.Decoded.Text}', pattern [{m.Decoded.Pattern}], "
                  + $"score {m.Decoded.Score:0.00}")
            .ToList();

    /// <summary>
    /// How many symbols the text is, spaces excluded and prosigns counted as
    /// one apiece.
    /// </summary>
    /// <param name="sent">The text that was sent.</param>
    /// <returns>The count.</returns>
    public static int SymbolCount(string sent)
        => Expand(sent).Count(s => s != " ");

    /// <summary>
    /// The sent text as the decoder would render it: one entry per symbol, with
    /// run-together groups collapsed into their prosign name.
    /// </summary>
    private static List<string> Expand(string sent)
    {
        var expanded = new List<string>();
        var i = 0;

        while (i < sent.Length)
        {
            if (sent[i] == '^')
            {
                // A run-together group runs to the next space and arrives as
                // one symbol, so that is how it has to be compared.
                var end = sent.IndexOf(' ', i);
                var length = (end < 0 ? sent.Length : end) - i - 1;
                var letters = sent.Substring(i + 1, length);

                expanded.Add($"<{letters}>");
                i += length + 1;
                continue;
            }

            expanded.Add(sent[i].ToString());
            i++;
        }

        return expanded;
    }
}
