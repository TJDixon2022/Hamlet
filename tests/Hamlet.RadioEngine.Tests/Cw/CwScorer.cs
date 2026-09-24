using System.Text;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A decode with, for each character of its text, whether the decoder was unsure of
/// it (work instruction 412, task 2; PHASE_PLAN.md 2.1).
/// </summary>
/// <remarks>
/// **UNSURE IS WHAT THE DECODER'S OWN COUNTER CALLS UNSURE**: a placeholder, or a
/// letter settled at less than high confidence (`CwDecoder`'s `CharactersUnsure`).
/// A letter settled low prints as the letter, so a text alone cannot say which
/// they were; <see cref="FromText"/> is for a text with nothing else behind it, a
/// sidecar's, and sees only the placeholders.
/// </remarks>
/// <param name="Text">What was settled, spaces included.</param>
/// <param name="Unsure">One flag per character of the text.</param>
public sealed record CwReading(string Text, IReadOnlyList<bool> Unsure)
{
    /// <summary>The reading of what the decoder settled, character by character.</summary>
    /// <param name="settled">The settled characters, word gaps included.</param>
    /// <returns>The text and its flags.</returns>
    public static CwReading Of(IEnumerable<CwCharacter> settled)
    {
        var text = new StringBuilder();
        var unsure = new List<bool>();

        foreach (var c in settled)
        {
            var flag = !c.IsWordGap && (c.IsUnreadable || c.Confidence != CwConfidence.High);

            text.Append(c.Text);
            unsure.AddRange(Enumerable.Repeat(flag, c.Text.Length));
        }

        return new CwReading(text.ToString(), unsure);
    }

    /// <summary>A text with nothing behind it: only its placeholders are known unsure.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The reading.</returns>
    public static CwReading FromText(string text)
        => new(text, text.Select(ch => ch.ToString() == MorseAlphabet.Unreadable).ToList());

    /// <summary>The characters from one index up to another.</summary>
    /// <param name="from">The first index taken.</param>
    /// <param name="to">The index after the last taken.</param>
    /// <returns>The slice, flags kept with their characters.</returns>
    public CwReading Slice(int from, int to)
        => new(Text[from..to], Unsure.Skip(from).Take(to - from).ToList());

    /// <summary>Characters with a name: neither a space nor a placeholder.</summary>
    public int Named => Text.Count(ch => ch != ' ' && ch.ToString() != MorseAlphabet.Unreadable);

    /// <summary>Characters the decoder was unsure of, placeholders included.</summary>
    public int UnsureCount => Unsure.Count(u => u);
}

/// <summary>Whether a key is exact or inferred (PHASE_PLAN.md R61).</summary>
public enum CwKeyKind
{
    /// <summary>Known by construction, as the generator's are.</summary>
    Exact,

    /// <summary>Reasoned from the audio or the form of a call, never transcribed.</summary>
    Inferred,
}

/// <summary>What one step of the alignment did.</summary>
public enum CwEdit
{
    /// <summary>The decode has the key's character.</summary>
    Same,

    /// <summary>The decode has a different character in its place.</summary>
    Wrong,

    /// <summary>The key's character has nothing in the decode.</summary>
    Missing,

    /// <summary>The decode has a character the key does not.</summary>
    Added,
}

/// <summary>One step of the alignment: what the key had, what the decode had.</summary>
/// <param name="Edit">What the step did.</param>
/// <param name="Key">The key's character, or null where the decode added one.</param>
/// <param name="Decoded">The decode's character, or null where the key's is missing.</param>
public readonly record struct CwStep(CwEdit Edit, char? Key, char? Decoded);

/// <summary>A correctness number in its three parts (PHASE_PLAN.md §3.1).</summary>
/// <param name="Edits">Levenshtein edits between the scored region and the key.</param>
/// <param name="ScoredLength">The key's length over the region, spaces included.</param>
/// <param name="Kind">Whether the key is exact or inferred.</param>
/// <param name="Region">The stretch of the decode that was scored.</param>
/// <param name="Key">The key it was scored against.</param>
/// <param name="Steps">The alignment the edits were counted from, key order.</param>
/// <param name="Named">Named characters in the region: neither a space nor a placeholder (2.1).</param>
/// <param name="Unsure">Characters in the region the decoder was unsure of, placeholders included (2.1).</param>
public sealed record CwScore(
    int Edits, int ScoredLength, CwKeyKind Kind, string Region, string Key,
    IReadOnlyList<CwStep> Steps, int Named, int Unsure)
{
    /// <summary>The guard beside the number: unsure characters per named character, or null with none named.</summary>
    public double? UnsurePerNamed => Named == 0 ? null : (double)Unsure / Named;

    /// <summary>The guard as a report writes it, both counts and the ratio.</summary>
    public string Guard
        => $"{Unsure} unsure per {Named} named"
           + (UnsurePerNamed is { } r ? $" ({r:0.000})" : " (nothing named)");

    /// <summary>The three parts as a report writes them, never a bare percentage, and the guard beside them.</summary>
    public override string ToString()
        => $"{Edits} edits over {ScoredLength} characters against an "
           + (Kind == CwKeyKind.Exact ? "exact" : "inferred") + $" key, {Guard}";
}

/// <summary>
/// A score's edits split by kind, counted from its alignment (work instruction 410,
/// task 3; PHASE_PLAN.md 0.3). The five counts add up to the edits.
/// </summary>
/// <param name="Wrong">A letter in the place of a different letter.</param>
/// <param name="Missing">A letter of the key with nothing in the decode.</param>
/// <param name="Added">A letter in the decode the key does not have.</param>
/// <param name="SpaceAdded">A space where none was sent: added, or in the place of a letter.</param>
/// <param name="SpaceMissing">No space where one was sent: missing, or a letter in its place.</param>
public readonly record struct CwErrorKinds(
    int Wrong, int Missing, int Added, int SpaceAdded, int SpaceMissing)
{
    /// <summary>Word boundaries misplaced, either way.</summary>
    public int Boundaries => SpaceAdded + SpaceMissing;

    /// <summary>Every edit, which is the score's.</summary>
    public int Edits => Wrong + Missing + Added + Boundaries;
}

/// <summary>
/// Edit distance between a decode and a key over a scored region: the instrument
/// the correctness phase is measured with (work instruction 410, task 1).
/// </summary>
/// <remarks>
/// <para>**SPACES ARE CHARACTERS HERE.** Word boundaries are the fault the phase
/// is investigating, so a space missing or added costs an edit like any letter,
/// and nothing is collapsed, folded or trimmed inside the region.</para>
/// <para>**THE EDIT COUNT IS LEVENSHTEIN AND DEPENDS ON NO CHOICE. THE ALIGNMENT
/// DOES.** Where several alignments reach the fewest edits, the scorer takes the
/// one with the fewest edits that touch no space: a letter is counted wrong,
/// missing or added only where no equally short alignment reads it right. Where
/// that still ties, it takes a character in place, then a key character missing,
/// then a decoded character added, reading back from the end. The breakdown into
/// kinds and the edges of a <see cref="Within"/> region follow this rule, and
/// <see cref="LettersOnly"/> is the view that follows none.</para>
/// <para>The first rule written took a character in place before anything else
/// and nothing more; it split `DEW B 6 RE D` against `DE WB6RED` into a letter
/// wrong, a letter added and three spaces, where a hand reads every letter right
/// and five spaces wrong. It was replaced before any recording was broken down.</para>
/// </remarks>
public static class CwScorer
{
    /// <summary>Scores a region already chosen against the whole key.</summary>
    /// <param name="region">The stretch of the decode the key file names.</param>
    /// <param name="key">The key for that stretch.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The score in its three parts, with its alignment.</returns>
    public static CwScore Whole(string region, string key, CwKeyKind kind)
        => Whole(CwReading.FromText(region), key, kind);

    /// <summary>Scores a region already chosen, with the decoder's own unsure flags behind it.</summary>
    /// <param name="region">The stretch of the decode the key file names.</param>
    /// <param name="key">The key for that stretch.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The score in its three parts, with its alignment and its guard.</returns>
    public static CwScore Whole(CwReading region, string key, CwKeyKind kind)
        => Align(region, key, kind, freeEnds: false);

    /// <summary>Scores the key against the stretch of the decode that fits it best.</summary>
    /// <remarks>
    /// For a key that covers a fragment of a recording and names no region of its
    /// own: the whole key is aligned, and the decode on either side of the stretch
    /// it aligns to is not scored, because nobody keyed it (R61).
    /// </remarks>
    /// <param name="decode">Everything the decoder settled.</param>
    /// <param name="key">The key for some stretch of it.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The score in its three parts, with the stretch it chose.</returns>
    public static CwScore Within(string decode, string key, CwKeyKind kind)
        => Within(CwReading.FromText(decode), key, kind);

    /// <summary>Scores the key against the best-fitting stretch, with the decoder's own unsure flags behind it.</summary>
    /// <param name="decode">Everything the decoder settled.</param>
    /// <param name="key">The key for some stretch of it.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The score in its three parts, with the stretch it chose and its guard.</returns>
    public static CwScore Within(CwReading decode, string key, CwKeyKind kind)
        => Align(decode, key, kind, freeEnds: true);

    /// <summary>The region from the first occurrence of an opening to the last character emitted.</summary>
    /// <param name="decode">Everything the decoder settled.</param>
    /// <param name="opening">What the region starts at, the first `CQ` for a CQ call.</param>
    /// <returns>The region with the gaps at its two ends trimmed, or "" where the opening was not read.</returns>
    public static string FromFirst(string decode, string opening)
        => FromFirst(CwReading.FromText(decode), opening).Text;

    /// <summary>The region from the first occurrence of an opening, flags kept.</summary>
    /// <param name="decode">Everything the decoder settled.</param>
    /// <param name="opening">What the region starts at.</param>
    /// <returns>The region with the gaps at its two ends trimmed, or an empty reading.</returns>
    public static CwReading FromFirst(CwReading decode, string opening)
    {
        var from = decode.Text.IndexOf(opening, StringComparison.Ordinal);

        if (from < 0)
        {
            return decode.Slice(0, 0);
        }

        var to = decode.Text.Length;

        while (to > from && char.IsWhiteSpace(decode.Text[to - 1]))
        {
            to--;
        }

        return decode.Slice(from, to);
    }

    /// <summary>Splits a score's edits by kind, from the alignment it was counted on.</summary>
    /// <remarks>
    /// An edit touching a space on either side is a word boundary misplaced, and
    /// is counted as one of those and nothing else. A letter swapped for a space
    /// is one edit and is counted once, on the side of the space: a space where
    /// none was sent if the space is the decode's, none where one was sent if it
    /// is the key's.
    /// </remarks>
    /// <param name="score">The score.</param>
    /// <returns>The counts, adding up to the score's edits.</returns>
    public static CwErrorKinds Kinds(CwScore score)
    {
        int wrong = 0, missing = 0, added = 0, spaceAdded = 0, spaceMissing = 0;

        foreach (var step in score.Steps)
        {
            if (step.Edit == CwEdit.Same)
            {
                continue;
            }

            if (step.Decoded == ' ')
            {
                spaceAdded++;
            }
            else if (step.Key == ' ')
            {
                spaceMissing++;
            }
            else if (step.Edit == CwEdit.Wrong)
            {
                wrong++;
            }
            else if (step.Edit == CwEdit.Missing)
            {
                missing++;
            }
            else
            {
                added++;
            }
        }

        return new CwErrorKinds(wrong, missing, added, spaceAdded, spaceMissing);
    }

    /// <summary>The same pair scored with every space taken out of both.</summary>
    /// <remarks>
    /// A second view that does not rest on how the alignment breaks a tie: what
    /// is left when word boundaries cost nothing at all.
    /// </remarks>
    /// <param name="score">The score.</param>
    /// <returns>The edits between the region and the key, both without spaces.</returns>
    public static int LettersOnly(CwScore score)
        => Whole(score.Region.Replace(" ", ""), score.Key.Replace(" ", ""), score.Kind).Edits;

    private static CwScore Align(CwReading reading, string key, CwKeyKind kind, bool freeEnds)
    {
        var decode = reading.Text;
        var m = key.Length;
        var n = decode.Length;
        var cost = new int[m + 1, n + 1];

        // Each edit weighs `edit`, and one that touches no space weighs one more,
        // so the least total is the fewest edits first and the fewest letter
        // edits among those second. `edit` exceeds any count of letter edits.
        var edit = m + n + 1;

        int Step(char? k, char? d)
            => k == d ? 0 : k == ' ' || d == ' ' ? edit : edit + 1;

        for (var i = 1; i <= m; i++)
        {
            cost[i, 0] = cost[i - 1, 0] + Step(key[i - 1], null);
        }

        for (var j = 1; j <= n; j++)
        {
            cost[0, j] = freeEnds ? 0 : cost[0, j - 1] + Step(null, decode[j - 1]);
        }

        for (var i = 1; i <= m; i++)
        {
            for (var j = 1; j <= n; j++)
            {
                cost[i, j] = Math.Min(
                    cost[i - 1, j - 1] + Step(key[i - 1], decode[j - 1]),
                    Math.Min(
                        cost[i - 1, j] + Step(key[i - 1], null),
                        cost[i, j - 1] + Step(null, decode[j - 1])));
            }
        }

        // The end of the scored stretch: the whole decode, or with free ends the
        // first column where the whole key costs least.
        var end = n;

        if (freeEnds)
        {
            end = 0;

            for (var j = 1; j <= n; j++)
            {
                if (cost[m, j] < cost[m, end])
                {
                    end = j;
                }
            }
        }

        var steps = new List<CwStep>();
        var a = m;
        var b = end;

        while (a > 0 || (b > 0 && !freeEnds))
        {
            if (a > 0 && b > 0
                && cost[a, b] == cost[a - 1, b - 1] + Step(key[a - 1], decode[b - 1]))
            {
                steps.Add(new CwStep(
                    key[a - 1] == decode[b - 1] ? CwEdit.Same : CwEdit.Wrong,
                    key[a - 1], decode[b - 1]));
                a--;
                b--;
            }
            else if (a > 0 && cost[a, b] == cost[a - 1, b] + Step(key[a - 1], null))
            {
                steps.Add(new CwStep(CwEdit.Missing, key[a - 1], null));
                a--;
            }
            else
            {
                steps.Add(new CwStep(CwEdit.Added, null, decode[b - 1]));
                b--;
            }
        }

        steps.Reverse();

        // The guard is counted over the same region the edits were (2.1).
        var region = reading.Slice(b, end);

        return new CwScore(
            steps.Count(s => s.Edit != CwEdit.Same), m, kind, region.Text, key, steps,
            region.Named, region.UnsureCount);
    }
}
