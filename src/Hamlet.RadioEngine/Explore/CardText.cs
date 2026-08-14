using System.Text;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Assembling the lines of a card so that nothing is said twice (HM-DEC-068).
/// </summary>
/// <remarks>
/// <para>THE MECHANISM RATHER THAN THE INSTANCE. A card is written by several
/// pieces of code that do not know about one another: the ranking explains why a
/// spot is where it is, and the line under it says what mode, which source, how
/// old and how far. Both of them reach for the same sentence about whether that
/// person is probably still on frequency, because both of them ask the same
/// function for it. Neither is wrong on its own, and together they produce a card
/// that says "activators stay a while, so they are probably still there" and then
/// says it again underneath.</para>
/// <para>Fixing the one duplicate would leave the next one to be found by
/// somebody reading the screen. So the lines are composed through here instead:
/// a clause an earlier line already carried is dropped from a later one, and the
/// card cannot repeat itself whatever the pieces decide to say.</para>
/// <para>WHY IT MATTERS BEYOND TIDINESS. Saying a thing twice reads as two pieces
/// of evidence when it is one, which is a confidence the input does not justify
/// (§0.0). It also reads as a program that is not paying attention, and this one
/// is asking somebody to trust it about what is on the air.</para>
/// <para>A CLAUSE, NOT A WORD. The unit is the phrase between separators, and
/// commas count: "an hour ago, and activators stay a while, so they are probably
/// still there" is three clauses, so a second line may still carry the age while
/// losing the part that was already said. Word-level matching would gut ordinary
/// English, where "the" appears everywhere and means nothing on its own.</para>
/// <para>Pure: strings in, strings out. No clock, no radio (§5).</para>
/// </remarks>
public static class CardText
{
    /// <summary>What separates the phrases on one line of a card.</summary>
    public const string Separator = " · ";

    /// <summary>
    /// Compose a card's lines, dropping anything an earlier line already said.
    /// </summary>
    /// <param name="lines">
    /// The lines in the order they are read, most important first. The first
    /// line keeps everything it was given.
    /// </param>
    /// <returns>The same number of lines, in the same order, never null.</returns>
    /// <remarks>
    /// THE FIRST LINE ALWAYS SURVIVES WHOLE. It is the one carrying the reason
    /// the card is on screen at all, and a reason line thinned out by something
    /// written under it would be the tail wagging the dog (HM-DEC-025).
    /// </remarks>
    public static IReadOnlyList<string> Compose(params string[] lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var composed = new List<string>(lines.Length);

        foreach (var line in lines)
        {
            composed.Add(Thin(line ?? "", seen));
        }

        return composed;
    }

    /// <summary>
    /// True when a composed line says the same clause more than once.
    /// </summary>
    /// <param name="text">One line, or a whole card joined together.</param>
    /// <returns>True when something is repeated.</returns>
    /// <remarks>
    /// The test's own question, answered here rather than in the test, so every
    /// card family asks it the same way (HM-DEC-068).
    /// </remarks>
    public static bool RepeatsItself(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var clause in Clauses(text))
        {
            var key = Normalize(clause);

            if (key.Length > 0 && !seen.Add(key))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Every clause in a line, in order.</summary>
    /// <param name="text">The line.</param>
    /// <returns>The clauses, trimmed, in reading order.</returns>
    public static IEnumerable<string> Clauses(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var phrase in text.Split(Separator, StringSplitOptions.None))
        {
            foreach (var clause in phrase.Split(',', StringSplitOptions.None))
            {
                var trimmed = clause.Trim();

                if (trimmed.Length > 0)
                {
                    yield return trimmed;
                }
            }
        }
    }

    private static string Thin(string line, HashSet<string> seen)
    {
        if (line.Length == 0)
        {
            return "";
        }

        var kept = new StringBuilder();

        foreach (var phrase in line.Split(Separator, StringSplitOptions.None))
        {
            var rebuilt = ThinPhrase(phrase, seen);

            if (rebuilt.Length == 0)
            {
                continue;
            }

            if (kept.Length > 0)
            {
                kept.Append(Separator);
            }

            kept.Append(rebuilt);
        }

        return kept.ToString();
    }

    private static string ThinPhrase(string phrase, HashSet<string> seen)
    {
        var kept = new List<string>();

        foreach (var clause in phrase.Split(',', StringSplitOptions.None))
        {
            var trimmed = clause.Trim();
            var key = Normalize(trimmed);

            if (key.Length == 0)
            {
                continue;
            }

            // A clause nobody has said yet is kept and remembered. One that has
            // already been read is dropped wherever it turns up again.
            if (seen.Add(key))
            {
                kept.Add(trimmed);
            }
        }

        return string.Join(", ", kept);
    }

    /// <summary>
    /// What makes two clauses the same clause.
    /// </summary>
    /// <remarks>
    /// Case and trailing punctuation are noise here. "So they are probably still
    /// there." and "so they are probably still there" are one sentence written
    /// twice, and a comparison that missed that would miss most of what this is
    /// for. Leading conjunctions go the same way, since "and activators stay a
    /// while" is the same claim as "activators stay a while".
    /// </remarks>
    private static string Normalize(string clause)
    {
        var text = clause.Trim().ToLowerInvariant().TrimEnd('.', '!', '?', ';', ':');

        foreach (var lead in new[] { "and ", "so ", "which is ", "though ", "but " })
        {
            if (text.StartsWith(lead, StringComparison.Ordinal))
            {
                text = text[lead.Length..];
                break;
            }
        }

        return text.Trim();
    }
}
