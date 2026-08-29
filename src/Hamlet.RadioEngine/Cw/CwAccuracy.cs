namespace Hamlet.RadioEngine.Cw;

/// <summary>How well a decode matched what was actually sent.</summary>
/// <param name="TruthCharacters">How many characters the truth holds.</param>
/// <param name="ScoredCharacters">
/// How many characters of the read were aligned against the truth span.
/// </param>
/// <param name="Correct">How many matched.</param>
/// <param name="Substitutions">Wrong letters — Hamlet guessed and missed.</param>
/// <param name="Insertions">Letters Hamlet added that nobody sent.</param>
/// <param name="Deletions">
/// Truth characters Hamlet did not produce, **including every block**.
/// </param>
/// <remarks>
/// **A BLOCK IS A DELETION AND NOT A SUBSTITUTION, AND THE DISTINCTION IS THE
/// WHOLE POINT** (Tim's ruling of 2026-08-27, unit 036). Refusing to guess and
/// guessing wrong are different errors: the first costs the operator a character
/// he can see is missing, the second hands him a plausible wrong letter he cannot
/// tell from a right one (§0.0). A score that folded them together could not show
/// the trade the refusal was ruled on.
/// </remarks>
public readonly record struct CwScore(
    int TruthCharacters,
    int ScoredCharacters,
    int Correct,
    int Substitutions,
    int Insertions,
    int Deletions)
{
    /// <summary>
    /// The share of what was sent that Hamlet got right.
    /// </summary>
    /// <remarks>
    /// **YIELD.** It counts every truth character, so a decoder that refuses
    /// everything scores nought here however careful it was. This is the number
    /// that answers "how much of the message did I get".
    /// </remarks>
    public double Yield
        => TruthCharacters == 0 ? 0 : (double)Correct / TruthCharacters;

    /// <summary>
    /// The share of what Hamlet actually emitted that was right.
    /// </summary>
    /// <remarks>
    /// **PRECISION.** Blocks are not counted against it, because a block is not a
    /// claim. This is the number §0.0 cares about: of the letters Hamlet asserted,
    /// how many were true. A decoder can raise it to one by refusing almost
    /// everything, which is why it is never reported without the yield beside it.
    /// </remarks>
    public double Precision
    {
        get
        {
            var asserted = Correct + Substitutions + Insertions;

            return asserted == 0 ? 0 : (double)Correct / asserted;
        }
    }

    /// <summary>Nothing was scored.</summary>
    public static CwScore None { get; } = new(0, 0, 0, 0, 0, 0);
}

/// <summary>
/// Scores a decode against what was actually sent.
/// </summary>
/// <remarks>
/// <para>**THIS PROJECT HAD NEVER MEASURED A PERCENTAGE** before unit 045. Every
/// unit counted characters emitted, characters unsure and blocks, and none of
/// those is accuracy: nothing compared what Hamlet read against what was sent.
/// The phase goal is stated as a percentage, so until this existed there was no
/// way to tell whether any change moved toward it.</para>
/// <para>**THE TRUTH IS A SPAN AND NOT THE WHOLE RECORDING.** What is adjudicated
/// is a fragment — a callsign, a clause of a bulletin — sitting somewhere inside
/// thirty seconds of audio. So the alignment is semi-global: the truth is matched
/// in full against the best-fitting stretch of the read, and what Hamlet produced
/// before and after that stretch is not scored. **Scoring it would count the rest
/// of the transmission as error**, which is a fact about the truth being partial
/// rather than about the decoder.</para>
/// <para>**IT IS PURE.** No clock, no network, no files. A decode and a truth
/// string go in and a score comes out, the same score every time (§5.4).</para>
/// </remarks>
public static class CwAccuracy
{
    /// <summary>What Hamlet writes where it will not guess.</summary>
    private const char Block = '■';

    /// <summary>Score a decode against the truth.</summary>
    /// <param name="read">What Hamlet produced.</param>
    /// <param name="truth">What was actually sent, over the span truth covers.</param>
    /// <returns>The score.</returns>
    public static CwScore Score(string? read, string? truth)
    {
        var t = Normalize(truth);
        var r = Normalize(read);

        if (t.Length == 0)
        {
            return CwScore.None;
        }

        if (r.Length == 0)
        {
            return new CwScore(t.Length, 0, 0, 0, 0, t.Length);
        }

        // **SEMI-GLOBAL: FREE SKIPS AT BOTH ENDS OF THE READ, NONE IN THE TRUTH.**
        // Every truth character must be accounted for; the read may run on before
        // and after the span that was adjudicated.
        var cost = new int[t.Length + 1, r.Length + 1];

        for (var i = 1; i <= t.Length; i++)
        {
            cost[i, 0] = i;
        }

        for (var i = 1; i <= t.Length; i++)
        {
            for (var j = 1; j <= r.Length; j++)
            {
                var same = t[i - 1] == r[j - 1];

                cost[i, j] = Math.Min(
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1),
                    cost[i - 1, j - 1] + (same ? 0 : 1));
            }
        }

        // **ON A TIE THE LONGER ALIGNMENT WINS, AND THAT IS NOT ARBITRARY.**
        // The read's tail is free, so a wrong final character can be skipped
        // instead of aligned at the same cost — and skipping it scores it as a
        // deletion, which reads as Hamlet having declined to name it. It did not
        // decline; it named it wrongly. Consuming on a tie counts a mistake as a
        // mistake, which is the direction that does not flatter the decoder
        // (§0.0).
        var end = 0;

        for (var j = 1; j <= r.Length; j++)
        {
            if (cost[t.Length, j] <= cost[t.Length, end])
            {
                end = j;
            }
        }

        var correct = 0;
        var subs = 0;
        var ins = 0;
        var dels = 0;
        var scored = 0;

        var y = t.Length;
        var x = end;

        while (y > 0)
        {
            if (x > 0)
            {
                var same = t[y - 1] == r[x - 1];

                if (cost[y, x] == cost[y - 1, x - 1] + (same ? 0 : 1))
                {
                    scored++;

                    if (same)
                    {
                        correct++;
                    }
                    else if (r[x - 1] == Block)
                    {
                        // **A BLOCK IS A DELETION.** Hamlet declined to name the
                        // character; it did not name it wrongly.
                        dels++;
                    }
                    else
                    {
                        subs++;
                    }

                    y--;
                    x--;

                    continue;
                }

                if (cost[y, x] == cost[y, x - 1] + 1)
                {
                    scored++;

                    if (r[x - 1] != Block)
                    {
                        ins++;
                    }

                    x--;

                    continue;
                }
            }

            dels++;
            y--;
        }

        return new CwScore(t.Length, scored, correct, subs, ins, dels);
    }

    /// <summary>What became of one character of the read.</summary>
    public enum Outcome
    {
        /// <summary>It matched the truth.</summary>
        Correct,

        /// <summary>A letter was asserted and it was the wrong one.</summary>
        Substitution,

        /// <summary>A letter was asserted that nobody sent.</summary>
        Insertion,

        /// <summary>Hamlet declined to name it.</summary>
        Block,
    }

    /// <summary>
    /// Align a read against the truth and say what became of each read
    /// character.
    /// </summary>
    /// <param name="read">What Hamlet produced, one entry per character.</param>
    /// <param name="truth">What was sent, over the span truth covers.</param>
    /// <returns>
    /// The outcome of every read character inside the aligned span, by its index
    /// in <paramref name="read"/>. Characters outside the span are absent.
    /// </returns>
    /// <remarks>
    /// <para>**THIS IS WHAT LETS A CONFIDENCE BE TESTED AGAINST CORRECTNESS.**
    /// The aggregate score says how many characters were wrong; this says
    /// **which**, so a number the decoder attaches to a character can be
    /// correlated against whether that character was right. Without it a
    /// confidence can only be checked at recording level, which is what an
    /// earlier measurement of `MarginShareForRecord` had to settle for.</para>
    /// <para>**NO NORMALISATION HAPPENS HERE**, because the indices have to line
    /// up with the caller's own list of characters. The caller passes what it
    /// wants compared.</para>
    /// </remarks>
    public static IReadOnlyDictionary<int, Outcome> Align(string? read, string? truth)
    {
        var outcomes = new Dictionary<int, Outcome>();
        var t = truth ?? string.Empty;
        var r = read ?? string.Empty;

        if (t.Length == 0 || r.Length == 0)
        {
            return outcomes;
        }

        var cost = new int[t.Length + 1, r.Length + 1];

        for (var i = 1; i <= t.Length; i++)
        {
            cost[i, 0] = i;
        }

        for (var i = 1; i <= t.Length; i++)
        {
            for (var j = 1; j <= r.Length; j++)
            {
                var same = t[i - 1] == r[j - 1];

                cost[i, j] = Math.Min(
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1),
                    cost[i - 1, j - 1] + (same ? 0 : 1));
            }
        }

        var end = 0;

        for (var j = 1; j <= r.Length; j++)
        {
            if (cost[t.Length, j] <= cost[t.Length, end])
            {
                end = j;
            }
        }

        var y = t.Length;
        var x = end;

        while (y > 0 && x > 0)
        {
            var same = t[y - 1] == r[x - 1];

            if (cost[y, x] == cost[y - 1, x - 1] + (same ? 0 : 1))
            {
                outcomes[x - 1] = same
                    ? Outcome.Correct
                    : r[x - 1] == Block ? Outcome.Block : Outcome.Substitution;

                y--;
                x--;

                continue;
            }

            if (cost[y, x] == cost[y, x - 1] + 1)
            {
                outcomes[x - 1] = r[x - 1] == Block
                    ? Outcome.Block
                    : Outcome.Insertion;

                x--;

                continue;
            }

            y--;
        }

        return outcomes;
    }

    /// <summary>
    /// What is compared: case folded, runs of space collapsed, ends trimmed.
    /// </summary>
    /// <remarks>
    /// **WORD SPACING IS MEASURED SEPARATELY AND IS NOT THIS SCORE'S QUESTION**
    /// (HM-DEC-142). A sender who leaves no word gaps produces an unspaced
    /// transcript that a ham reads perfectly well, and counting each missing space
    /// as an error would rank that below a decoder that invented spaces. Runs of
    /// space collapse to one on both sides so the comparison is about letters.
    /// </remarks>
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var build = new System.Text.StringBuilder(text.Length);
        var space = false;

        foreach (var c in text.ToUpperInvariant())
        {
            if (char.IsWhiteSpace(c))
            {
                space = true;

                continue;
            }

            if (space && build.Length > 0)
            {
                build.Append(' ');
            }

            space = false;
            build.Append(c);
        }

        return build.ToString();
    }
}
