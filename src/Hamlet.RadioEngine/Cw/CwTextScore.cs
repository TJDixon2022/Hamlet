using System.Globalization;
using System.Text;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// How much of a sent text the terminal read, in the owner's words (work instruction numbered 509,
/// run as unit 510, task 3, HM-DEC-214).
/// </summary>
/// <param name="Percent">The share of the sent text's characters read right, rounded, never below nought.</param>
/// <param name="Wrong">Characters read as a different character.</param>
/// <param name="Missing">Characters sent and not read, spaces included.</param>
/// <param name="Extra">Characters read and not sent, spaces included.</param>
/// <param name="SentLength">The sent text's length after it is normalized, spaces included.</param>
/// <param name="Start">Where the stretch the sent text aligned to starts in the normalized terminal text.</param>
/// <param name="Stretch">The stretch of the normalized terminal text it was scored over.</param>
public sealed record CwTextScore(int Percent, int Wrong, int Missing, int Extra, int SentLength, int Start, string Stretch)
{
    /// <summary>
    /// Score what the terminal holds against a text the owner pasted.
    /// </summary>
    /// <param name="terminal">Everything the terminal holds.</param>
    /// <param name="sent">What was sent, as published.</param>
    /// <returns>The score, or null where either side is empty.</returns>
    /// <remarks>
    /// <para>**THE SAME INSTRUMENT THE TESTS USE**: <see cref="CwScorer.Within(string, string, CwKeyKind)"/>,
    /// the edit distance with free ends, which aligns the whole sent text to the stretch of the
    /// terminal that fits it best and leaves what the terminal holds either side of it unscored,
    /// because nobody sent that as part of this text.</para>
    /// <para>**BOTH SIDES ARE NORMALIZED THE SAME WAY, AND THAT IS ALL**: upper case, since case
    /// does not count, and every run of spaces, tabs and line breaks made one space, since the
    /// ARRL's text breaks its lines where a page does and the terminal breaks where its box does.
    /// Punctuation and prosigns count as the characters they are.</para>
    /// <para>**SPACES ARE CHARACTERS**, as they are to the scorer: a word run into the next is a
    /// character missing, and a word split in two is one extra.</para>
    /// </remarks>
    public static CwTextScore? Of(string terminal, string sent)
    {
        var decode = Normalize(terminal);
        var key = Normalize(sent);

        if (decode.Length == 0 || key.Length == 0)
        {
            return null;
        }

        var score = CwScorer.Within(decode, key, CwKeyKind.Exact);
        var kinds = CwScorer.Kinds(score);
        var right = Math.Max(0, key.Length - score.Edits);

        return new CwTextScore(
            (int)Math.Round(100.0 * right / key.Length, MidpointRounding.AwayFromZero),
            kinds.Wrong,
            kinds.Missing + kinds.SpaceMissing,
            kinds.Added + kinds.SpaceAdded,
            key.Length,
            score.Start,
            score.Region);
    }

    /// <summary>The line the CW tab shows: "W1AW 7 PM bulletin: 94% of characters, 3 wrong, 2 missing, 1 extra".</summary>
    /// <param name="slot">What was scored, "W1AW 7 PM bulletin".</param>
    /// <returns>The line.</returns>
    public string Line(string slot)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{slot}: {Percent}% of characters, {Wrong} wrong, {Missing} missing, {Extra} extra");

    /// <summary>Upper case, with every run of whitespace one space and none at either end.</summary>
    /// <param name="text">The text.</param>
    /// <returns>It normalized.</returns>
    public static string Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var built = new StringBuilder(text.Length);
        var space = false;

        foreach (var ch in text.Trim())
        {
            if (char.IsWhiteSpace(ch))
            {
                space = true;
                continue;
            }

            if (space && built.Length > 0)
            {
                built.Append(' ');
            }

            space = false;
            built.Append(char.ToUpperInvariant(ch));
        }

        return built.ToString();
    }
}
