namespace Hamlet.RadioEngine.Tests.Cw;

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
public sealed record CwScore(
    int Edits, int ScoredLength, CwKeyKind Kind, string Region, string Key,
    IReadOnlyList<CwStep> Steps)
{
    /// <summary>The three parts as a report writes them, never a bare percentage.</summary>
    public override string ToString()
        => $"{Edits} edits over {ScoredLength} characters against an "
           + (Kind == CwKeyKind.Exact ? "exact" : "inferred") + " key";
}

/// <summary>
/// Edit distance between a decode and a key over a scored region: the instrument
/// the correctness phase is measured with (work instruction 410, task 1).
/// </summary>
/// <remarks>
/// <para>**SPACES ARE CHARACTERS HERE.** Word boundaries are the fault the phase
/// is investigating, so a space missing or added costs an edit like any letter,
/// and nothing is collapsed, folded or trimmed inside the region.</para>
/// <para>**WHERE TWO ALIGNMENTS TIE, THE SCORER TAKES A CHARACTER IN PLACE, THEN
/// A KEY CHARACTER MISSING, THEN A DECODED CHARACTER ADDED**, reading back from
/// the end. The edit count does not depend on the choice; the breakdown into
/// kinds and the edges of a <see cref="Within"/> region do, and this is the rule
/// they follow.</para>
/// </remarks>
public static class CwScorer
{
    /// <summary>Scores a region already chosen against the whole key.</summary>
    /// <param name="region">The stretch of the decode the key file names.</param>
    /// <param name="key">The key for that stretch.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The score in its three parts, with its alignment.</returns>
    public static CwScore Whole(string region, string key, CwKeyKind kind)
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
        => Align(decode, key, kind, freeEnds: true);

    /// <summary>The region from the first occurrence of an opening to the last character emitted.</summary>
    /// <param name="decode">Everything the decoder settled.</param>
    /// <param name="opening">What the region starts at, the first `CQ` for a CQ call.</param>
    /// <returns>The region with the gaps at its two ends trimmed, or "" where the opening was not read.</returns>
    public static string FromFirst(string decode, string opening)
    {
        var from = decode.IndexOf(opening, StringComparison.Ordinal);

        return from < 0 ? "" : decode[from..].Trim();
    }

    private static CwScore Align(string decode, string key, CwKeyKind kind, bool freeEnds)
    {
        var m = key.Length;
        var n = decode.Length;
        var cost = new int[m + 1, n + 1];

        for (var i = 0; i <= m; i++)
        {
            cost[i, 0] = i;
        }

        for (var j = 0; j <= n; j++)
        {
            cost[0, j] = freeEnds ? 0 : j;
        }

        for (var i = 1; i <= m; i++)
        {
            for (var j = 1; j <= n; j++)
            {
                cost[i, j] = Math.Min(
                    cost[i - 1, j - 1] + (key[i - 1] == decode[j - 1] ? 0 : 1),
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1));
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
                && cost[a, b] == cost[a - 1, b - 1] + (key[a - 1] == decode[b - 1] ? 0 : 1))
            {
                steps.Add(new CwStep(
                    key[a - 1] == decode[b - 1] ? CwEdit.Same : CwEdit.Wrong,
                    key[a - 1], decode[b - 1]));
                a--;
                b--;
            }
            else if (a > 0 && cost[a, b] == cost[a - 1, b] + 1)
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

        return new CwScore(cost[m, end], m, kind, decode[b..end], key, steps);
    }
}
