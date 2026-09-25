using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>What one emitted symbol claims, in the classes the decoder actually has.</summary>
/// <remarks>
/// **NO DIM CLASS IS INVENTED HERE** (work instruction 439, task 4). `CW_SPEC.md`
/// section 11 names three classes, sure, dim and placeholder; the decoder emits
/// only <see cref="CwConfidence.High"/> and <see cref="CwConfidence.Unreadable"/>
/// today. A named character at any other confidence is counted as
/// <see cref="NotSure"/> and never as sure, so a decoder that starts emitting
/// <see cref="CwConfidence.Low"/> is measured without anybody editing this.
/// </remarks>
public enum CwSymbolClass
{
    /// <summary>A named character at high confidence: the decoder stands behind it.</summary>
    Sure,

    /// <summary>A named character below high confidence. The decoder emits none today.</summary>
    NotSure,

    /// <summary>Something heard and not resolved: never scored wrong (`CW_SPEC.md` 5.3).</summary>
    Placeholder,

    /// <summary>A word boundary.</summary>
    WordGap,
}

/// <summary>One emitted symbol: a character, prosigns counted as one, or a word boundary.</summary>
/// <param name="Text">The character's text, `&lt;BT&gt;` for a prosign, a space for a gap.</param>
/// <param name="Class">What the decoder claimed for it.</param>
public readonly record struct CwSymbol(string Text, CwSymbolClass Class)
{
    /// <summary>A sure symbol, for a test that builds a decode by hand.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The symbol.</returns>
    public static CwSymbol Sure(string text) => new(text, CwSymbolClass.Sure);

    /// <summary>A placeholder, for a test that builds a decode by hand.</summary>
    public static CwSymbol Placeholder { get; } = new(MorseAlphabet.Unreadable, CwSymbolClass.Placeholder);

    /// <summary>A word boundary, for a test that builds a decode by hand.</summary>
    public static CwSymbol Gap { get; } = new(MorseAlphabet.WordGap, CwSymbolClass.WordGap);

    /// <summary>The symbol for one settled character.</summary>
    /// <param name="c">The character.</param>
    /// <returns>Its symbol and class.</returns>
    public static CwSymbol Of(CwCharacter c)
        => c.IsWordGap ? Gap
            : c.IsUnreadable ? new CwSymbol(c.Text, CwSymbolClass.Placeholder)
            : new CwSymbol(c.Text, c.Confidence == CwConfidence.High ? CwSymbolClass.Sure : CwSymbolClass.NotSure);
}

/// <summary>One step of the character alignment behind the metrics.</summary>
/// <param name="Key">The key's symbol, or null where the decode added one.</param>
/// <param name="Decoded">The decode's symbol, or null where the key's is missing.</param>
/// <param name="KeyBefore">Key symbols consumed before this step.</param>
/// <param name="KeyAfter">Key symbols consumed after this step.</param>
/// <param name="GapBefore">Whether the decode had a word boundary before this step's decoded symbol.</param>
public readonly record struct CwMetricStep(
    string? Key, CwSymbol? Decoded, int KeyBefore, int KeyAfter, bool GapBefore);

/// <summary>A decode aligned to its key, character by character, spaces aside.</summary>
/// <param name="Steps">The alignment, key order.</param>
/// <param name="KeyCharacters">Characters sent: the key's symbols, spaces excluded, a prosign one.</param>
/// <param name="KeyBoundaries">Where the key's word boundaries sit: the count of key characters before each.</param>
/// <param name="KeyWords">Words sent.</param>
/// <param name="Kind">Whether the key is exact or inferred (R61, V-13).</param>
public sealed record CwMetricAlignment(
    IReadOnlyList<CwMetricStep> Steps, int KeyCharacters, IReadOnlyList<int> KeyBoundaries,
    int KeyWords, CwKeyKind Kind);

/// <summary>MET-INVENTED in its parts (`CW_SPEC.md` 11): sure insertions and sure substitutions over characters sent.</summary>
/// <param name="SureAdded">Sure characters where the key has none.</param>
/// <param name="SureWrong">Sure characters where the key has a different one.</param>
/// <param name="Sent">Characters sent.</param>
/// <param name="Kind">The key's kind.</param>
public sealed record CwInvented(int SureAdded, int SureWrong, int Sent, CwKeyKind Kind)
{
    /// <summary>Sure insertions and substitutions together.</summary>
    public int Count => SureAdded + SureWrong;

    /// <summary>The share, or null where nothing was sent.</summary>
    public double? Share => Sent == 0 ? null : (double)Count / Sent;

    /// <summary>The number with its parts, never a bare percentage (§3.1).</summary>
    public override string ToString()
        => $"{Count} invented ({SureAdded} sure added, {SureWrong} sure wrong) over {Sent} sent, "
           + CwMetrics.KindWord(Kind) + " key" + CwMetrics.ShareText(Share);
}

/// <summary>MET-CER-SURE in its parts (`CW_SPEC.md` 11): of the characters emitted sure, those wrong.</summary>
/// <param name="SureWrong">Sure characters where the key has a different one.</param>
/// <param name="SureAdded">Sure characters where the key has none.</param>
/// <param name="SureEmitted">Sure characters emitted.</param>
/// <param name="Kind">The key's kind.</param>
public sealed record CwSureErrors(int SureWrong, int SureAdded, int SureEmitted, CwKeyKind Kind)
{
    /// <summary>Sure characters in error.</summary>
    public int Errors => SureWrong + SureAdded;

    /// <summary>The rate, or null where nothing was emitted sure.</summary>
    public double? Rate => SureEmitted == 0 ? null : (double)Errors / SureEmitted;

    /// <summary>The number with its parts.</summary>
    public override string ToString()
        => $"{Errors} wrong of {SureEmitted} sure ({SureWrong} substituted, {SureAdded} added), "
           + CwMetrics.KindWord(Kind) + " key" + CwMetrics.ShareText(Rate);
}

/// <summary>MET-COVERAGE in its parts, under R82 (work instruction 441): sure and right characters over characters sent.</summary>
/// <param name="SureEmitted">Sure characters emitted, right or not.</param>
/// <param name="SureRight">Of those, the ones the key has in place.</param>
/// <param name="Sent">Characters sent.</param>
/// <param name="Kind">The key's kind.</param>
public sealed record CwCoverage(int SureEmitted, int SureRight, int Sent, CwKeyKind Kind)
{
    /// <summary>The share, or null where nothing was sent. A wrong or added sure letter is not coverage, so it cannot exceed one.</summary>
    public double? Share => Sent == 0 ? null : (double)SureRight / Sent;

    /// <summary>The number with its parts.</summary>
    public override string ToString()
        => $"{SureRight} sure and right over {Sent} sent ({SureEmitted} sure emitted), "
           + CwMetrics.KindWord(Kind) + " key" + CwMetrics.ShareText(Share);
}

/// <summary>MET-WBE in its parts (`CW_SPEC.md` 11): word gaps inserted or deleted over words sent.</summary>
/// <param name="Inserted">Decoded boundaries with no key boundary where they sit.</param>
/// <param name="Deleted">Key boundaries with no decoded boundary where they sit.</param>
/// <param name="WordsSent">Words sent.</param>
/// <param name="Kind">The key's kind.</param>
public sealed record CwBoundaryErrors(int Inserted, int Deleted, int WordsSent, CwKeyKind Kind)
{
    /// <summary>Boundaries misplaced, either way.</summary>
    public int Errors => Inserted + Deleted;

    /// <summary>The rate, or null where no word was sent.</summary>
    public double? Rate => WordsSent == 0 ? null : (double)Errors / WordsSent;

    /// <summary>The number with its parts.</summary>
    public override string ToString()
        => $"{Errors} boundaries wrong ({Inserted} inserted, {Deleted} deleted) over {WordsSent} words, "
           + CwMetrics.KindWord(Kind) + " key" + CwMetrics.ShareText(Rate);
}

/// <summary>
/// The four metrics the CW requirements are written in, MET-INVENTED, MET-CER-SURE,
/// MET-COVERAGE and MET-WBE, as `CW_SPEC.md` section 11 defines them (work
/// instruction 439, tasks 3 and 4; PHASE_PLAN.md 1.1 to 1.4).
/// </summary>
/// <remarks>
/// <para>**CHARACTERS ARE SYMBOLS.** A settled character is one symbol, a prosign
/// such as `&lt;BT&gt;` included, and a key's `&lt;BT&gt;` or `^BT` is one symbol too,
/// as `CW_SPEC.md` 6.2 has one pattern be one symbol. `CwScorer` counts `&lt;BT&gt;`
/// as four characters, which is right for its edit total and wrong for "characters
/// sent".</para>
/// <para>**CHARACTERS AND BOUNDARIES ARE SCORED APART** (HM-REQ-082). The character
/// alignment has every space taken out of both sides, so no letter error can be
/// traded for a boundary error or the other way round. It is `CwScorer`'s own
/// Levenshtein on the symbols, each encoded as one character, so the two
/// instruments cannot disagree about what an alignment is. Boundaries are then
/// placed on that alignment: a decoded boundary is right where a key boundary sits
/// between the key characters its neighbors aligned to, and each key boundary is
/// claimed once.</para>
/// <para>**EACH METRIC IS COUNTED FROM THE ALIGNMENT AND NONE FROM ANOTHER'S
/// RESULT** (V-05). MET-INVENTED and MET-CER-SURE share a numerator by definition
/// and are still each counted where they are computed.</para>
/// <para>**A PLACEHOLDER OR A NOT-SURE CHARACTER IS NEVER WRONG** (`CW_SPEC.md`
/// 5.3). It still takes its place in the alignment, because it is where it is.</para>
/// <para>**MET-COVERAGE IS SURE AND RIGHT OVER SENT** (R82, the owner, 2026-09-25,
/// work instruction 441). The spec's ratio as written, sure emitted over sent,
/// counted a wrong sure letter as coverage, so removing one lowered it. Dimming
/// everything still takes it to nought. The record keeps the sure emitted count
/// beside it so a reader sees both.</para>
/// </remarks>
public static class CwMetrics
{
    /// <summary>The key's symbols: a prosign one, a space a boundary.</summary>
    /// <param name="key">The key as the key files write it, `&lt;BT&gt;` or `^BT` for a prosign.</param>
    /// <returns>One entry per symbol.</returns>
    public static IReadOnlyList<CwSymbol> KeySymbols(string key)
    {
        var symbols = new List<CwSymbol>();
        var i = 0;

        while (i < key.Length)
        {
            var ch = key[i];

            if (ch == ' ')
            {
                symbols.Add(CwSymbol.Gap);
                i++;
            }
            else if (ch == '<' && key.IndexOf('>', i) is var close and > 0)
            {
                symbols.Add(CwSymbol.Sure(key[i..(close + 1)]));
                i = close + 1;
            }
            else if (ch == '^')
            {
                var end = key.IndexOf(' ', i);
                var to = end < 0 ? key.Length : end;

                symbols.Add(CwSymbol.Sure($"<{key[(i + 1)..to]}>"));
                i = to;
            }
            else
            {
                symbols.Add(CwSymbol.Sure(ch.ToString()));
                i++;
            }
        }

        return symbols;
    }

    /// <summary>The symbols of a stretch of settled characters.</summary>
    /// <param name="settled">What settled, word gaps included.</param>
    /// <returns>One symbol per character.</returns>
    public static IReadOnlyList<CwSymbol> Symbols(IEnumerable<CwCharacter> settled)
        => settled.Select(CwSymbol.Of).ToList();

    /// <summary>Aligns a decoded stretch against its key, spaces aside.</summary>
    /// <param name="decoded">The stretch the key covers, word gaps included.</param>
    /// <param name="key">The key.</param>
    /// <param name="kind">Whether the key is exact or inferred.</param>
    /// <returns>The alignment every metric is counted from.</returns>
    public static CwMetricAlignment Align(IReadOnlyList<CwSymbol> decoded, string key, CwKeyKind kind)
    {
        var keySymbols = KeySymbols(key);
        var keyText = keySymbols.Where(s => s.Class != CwSymbolClass.WordGap).Select(s => s.Text).ToList();
        var boundaries = new List<int>();
        var before = 0;

        foreach (var s in keySymbols)
        {
            if (s.Class == CwSymbolClass.WordGap)
            {
                if (before > 0 && before < keyText.Count && (boundaries.Count == 0 || boundaries[^1] != before))
                {
                    boundaries.Add(before);
                }
            }
            else
            {
                before++;
            }
        }

        var chars = new List<CwSymbol>();
        var gapBefore = new List<bool>();
        var pendingGap = false;

        foreach (var s in decoded)
        {
            if (s.Class == CwSymbolClass.WordGap)
            {
                pendingGap = chars.Count > 0;
                continue;
            }

            chars.Add(s);
            gapBefore.Add(pendingGap);
            pendingGap = false;
        }

        // One character per symbol, the same symbol the same character on both sides.
        var code = new Dictionary<string, char>(StringComparer.Ordinal);
        var next = '';

        char Encode(string text)
        {
            if (text.Length == 1)
            {
                return text[0];
            }

            if (!code.TryGetValue(text, out var c))
            {
                code[text] = c = next++;
            }

            return c;
        }

        var keyEncoded = new string(keyText.Select(Encode).ToArray());
        var decodedEncoded = new string(chars.Select(s => Encode(s.Text)).ToArray());
        var score = CwScorer.Whole(decodedEncoded, keyEncoded, kind);

        var steps = new List<CwMetricStep>();
        int k = 0, d = 0;

        foreach (var step in score.Steps)
        {
            var from = k;
            string? keySymbol = null;
            CwSymbol? decodedSymbol = null;
            var gap = false;

            if (step.Key is not null)
            {
                keySymbol = keyText[k++];
            }

            if (step.Decoded is not null)
            {
                decodedSymbol = chars[d];
                gap = gapBefore[d];
                d++;
            }

            steps.Add(new CwMetricStep(keySymbol, decodedSymbol, from, k, gap));
        }

        return new CwMetricAlignment(steps, keyText.Count, boundaries, boundaries.Count + (keyText.Count > 0 ? 1 : 0), kind);
    }

    /// <summary>MET-INVENTED: sure insertions and sure substitutions over characters sent.</summary>
    /// <param name="a">The alignment.</param>
    /// <returns>The count in its parts.</returns>
    public static CwInvented Invented(CwMetricAlignment a)
    {
        int added = 0, wrong = 0;

        foreach (var s in a.Steps)
        {
            if (s.Decoded is not { Class: CwSymbolClass.Sure } d)
            {
                continue;
            }

            if (s.Key is null)
            {
                added++;
            }
            else if (!string.Equals(s.Key, d.Text, StringComparison.Ordinal))
            {
                wrong++;
            }
        }

        return new CwInvented(added, wrong, a.KeyCharacters, a.Kind);
    }

    /// <summary>MET-CER-SURE: of the characters emitted sure, the ones wrong or added.</summary>
    /// <param name="a">The alignment.</param>
    /// <returns>The rate in its parts.</returns>
    public static CwSureErrors SureErrors(CwMetricAlignment a)
    {
        int emitted = 0, wrong = 0, added = 0;

        foreach (var s in a.Steps)
        {
            if (s.Decoded is not { Class: CwSymbolClass.Sure } d)
            {
                continue;
            }

            emitted++;

            if (s.Key is null)
            {
                added++;
            }
            else if (!string.Equals(s.Key, d.Text, StringComparison.Ordinal))
            {
                wrong++;
            }
        }

        return new CwSureErrors(wrong, added, emitted, a.Kind);
    }

    /// <summary>MET-COVERAGE under R82: sure and right characters over characters sent.</summary>
    /// <param name="a">The alignment.</param>
    /// <returns>The share in its parts.</returns>
    public static CwCoverage Coverage(CwMetricAlignment a)
    {
        int emitted = 0, right = 0;

        foreach (var s in a.Steps)
        {
            if (s.Decoded is not { Class: CwSymbolClass.Sure } d)
            {
                continue;
            }

            emitted++;

            if (s.Key is not null && string.Equals(s.Key, d.Text, StringComparison.Ordinal))
            {
                right++;
            }
        }

        return new CwCoverage(emitted, right, a.KeyCharacters, a.Kind);
    }

    /// <summary>MET-WBE: word boundaries inserted or deleted over words sent, scored on boundaries alone.</summary>
    /// <remarks>
    /// A decoded boundary sits between two decoded characters; the key positions it
    /// could stand for run from the key characters consumed after the first to
    /// those consumed before the second. It is right where an unclaimed key boundary
    /// lies in that span, and inserted otherwise; every key boundary nothing claimed
    /// is deleted.
    /// </remarks>
    /// <param name="a">The alignment.</param>
    /// <returns>The rate in its parts.</returns>
    public static CwBoundaryErrors WordBoundaries(CwMetricAlignment a)
    {
        var unclaimed = new SortedSet<int>(a.KeyBoundaries);
        var inserted = 0;
        int? keyAfterPrevious = null;

        foreach (var s in a.Steps)
        {
            if (s.Decoded is null)
            {
                continue;
            }

            if (s.GapBefore && keyAfterPrevious is { } low)
            {
                var high = s.KeyBefore;
                var match = unclaimed.GetViewBetween(low, Math.Max(low, high)).Cast<int?>().FirstOrDefault();

                if (match is { } m)
                {
                    unclaimed.Remove(m);
                }
                else
                {
                    inserted++;
                }
            }

            keyAfterPrevious = s.KeyAfter;
        }

        return new CwBoundaryErrors(inserted, unclaimed.Count, a.KeyWords, a.Kind);
    }

    /// <summary>The key's kind as a report writes it.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>"exact" or "inferred".</returns>
    public static string KindWord(CwKeyKind kind) => kind == CwKeyKind.Exact ? "exact" : "inferred";

    /// <summary>A share appended to its parts, or a statement that there is none.</summary>
    /// <param name="share">The share.</param>
    /// <returns>The text.</returns>
    public static string ShareText(double? share)
        => share is { } s ? $", {s:0.0000}" : ", no number: nothing to divide by";
}
