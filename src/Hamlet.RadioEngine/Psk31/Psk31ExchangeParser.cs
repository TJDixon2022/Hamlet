using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>What kind of line a PSK31 message is, in the transcript corpus's nine words.</summary>
/// <remarks>
/// <para>**THE NINE ARE THE CORPUS'S `kinds` ARRAY AND NOTHING MORE**
/// (`assets/fixtures/psk31-transcripts/corpus.json`). A tenth would be a state the corpus
/// cannot check.</para>
/// <para>**<see cref="Ack"/> IS NEVER PRODUCED, AND THAT IS §R3 AND NOT AN OMISSION.** A roger
/// is `R`, `RR` or `QSL`, and none of them is in the vocabulary the parser is allowed: callsigns,
/// `de`, `CQ`, RST, a grid, the turnover words and the closers. Anything else is text. A line
/// that only rogers therefore reads as <see cref="Chat"/>.</para>
/// </remarks>
public enum Psk31LineKind
{
    /// <summary>A call to anyone: `CQ ... de CALL`, or `CQ DX`.</summary>
    Cq,

    /// <summary>A reply to a named station that says nothing but the calls and the turnover.</summary>
    Answer,

    /// <summary>A message carrying a signal report.</summary>
    Report,

    /// <summary>A roger with no report and no goodbye. Never produced; see the remarks.</summary>
    Ack,

    /// <summary>Text that is none of the others.</summary>
    Chat,

    /// <summary>A goodbye (`73`) that still hands the turn over rather than signing off.</summary>
    Closing,

    /// <summary>A message that signs off: its last words include `SK` or `CL`.</summary>
    End,

    /// <summary>More damaged tokens than clean ones.</summary>
    Garbage,

    /// <summary>Nothing to read: no words at all, or nothing but turnover words.</summary>
    Unknown,
}

/// <summary>What one PSK31 message says about the exchange, and whether Hamlet is sure.</summary>
/// <param name="Speaker">The callsign that sent it, or null where the text does not say.</param>
/// <param name="Addressee">
/// A callsign, <see cref="Psk31ExchangeParser.Anyone"/>, <see cref="Psk31ExchangeParser.Dx"/>,
/// or null where the text does not say.
/// </param>
/// <param name="Kind">What kind of line it is.</param>
/// <param name="HandsOver">True where its last word is a turnover or closing word.</param>
/// <param name="IsCertain">
/// True only where the text supports every field without a guess. **False is a guess or an
/// unknown, and a reader must show it as one** (`PHASE_PLAN.md` §R1).
/// </param>
/// <param name="Rst">The report, three characters with `N` read as `9`, or null.</param>
/// <param name="Grid">A Maidenhead locator, upper case, or null.</param>
/// <param name="IsForOperator">True where the addressee is the operator's own station.</param>
/// <remarks>
/// **THERE IS NO MEMBER FOR NAME OR QTH, AND THAT IS THE RULING** (§R3). They are for
/// reading, not for state, and a wrong parse of them changes nothing Hamlet does.
/// </remarks>
public sealed record Psk31Exchange(
    string? Speaker,
    string? Addressee,
    Psk31LineKind Kind,
    bool HandsOver,
    bool IsCertain,
    string? Rst,
    string? Grid,
    bool IsForOperator);

/// <summary>Reads one PSK31 message into who, to whom, what kind, whose turn, and how sure.</summary>
/// <remarks>
/// <para>**TEXT AND A CALLSIGN IN, A PARSE OUT** (§0.1). It is not told about tabs, rows,
/// settings or radios.</para>
/// <para>**THE CORPUS'S `rules` ARRAY, EACH ONCE, WITH WHERE IT SAYS IT**
/// (`assets/fixtures/psk31-transcripts/corpus.json`):</para>
/// <list type="number">
/// <item>`rules[0]` - a line's speaker is the callsign after the last `de`.</item>
/// <item>`rules[1]` - a line's addressee is the callsign before that `de`, or ANY for a CQ,
/// or DX for CQ DX.</item>
/// <item>`rules[2]` - turnover words: `K`, `KN`, `BTU`, `OVER`; closers: `73`, `SK`, `CL`.</item>
/// <item>`rules[3]` - RST is 3 digits, with `N` standing for 9.</item>
/// <item>`rules[4]` - name and QTH are never parsed into state.</item>
/// <item>`rules[5]` - certain=false means the parser must expose the state as a guess or as
/// unknown, never as fact.</item>
/// </list>
/// <para>**THE CERTAINTY RULE, IN ONE SENTENCE:** a parse is certain only where no token is
/// damaged, the speaker and the addressee are both named, the message ends on a turnover or
/// closing word, it opens on its own sign (`CQ`, or `CALL de CALL`) and every `de` in it names
/// the same pair, and no report or grid in it contradicts another.</para>
/// <para>**A DAMAGED TOKEN IS ONE WITH A SYMBOL WELDED INTO IT** - anything that is not a letter,
/// a digit, or one of <c>/ - ' . , : ; ! ? " ( )</c> - such as `5#9` or `K3A~C`. **It is never
/// read as its nearest clean neighbour** (§R1, §0.0): `5#9` is not `599` and `K3A~C` is not
/// `K3ABC`. It makes the field it would have filled unknown - a damaged three-character report
/// makes the RST unknown even beside a clean `599`, because the two copies no longer agree - and
/// it makes the whole message uncertain. **A clean closing sign still names its speaker**, so
/// an uncertain message can say who sent it and say that it is a guess.</para>
/// <para>**TEXT BEFORE THE OPENING SIGN HAS NO OWNER.** A station brackets its over with the
/// calls. Words that arrive before the first `CQ` or `CALL de CALL` may be the tail of somebody
/// else's transmission - `05-garbled` line 4, which has no turnover, is exactly that when one
/// frequency carries it straight into line 5 - so a message that does not open on its sign is
/// never certain, whatever its closing sign says.</para>
/// <para>**THE CALLSIGN SHAPE IS THE TREE'S OWN** (§R3: *the same rule the decoded list uses
/// today*): <see cref="CallsignPattern"/> is character for character the pattern
/// `CallsignResolver`, `AutoCallAnswers` and `ScanStop` each hold privately, and a test holds
/// the four together.</para>
/// <para>**ONE CALLSIGN QUESTION IS ASKED OF THE CONTACTS LAYER AND NO TEXT IS.** Whether the
/// addressee is the operator's own station is
/// <see cref="Ft8MessageSplit.IsSameStation(string?, string?)"/>, handed two callsigns this
/// parser has already read - a second copy of what makes `W1ABC/P` the same station as `W1ABC`
/// would be a second answer waiting to disagree (§0). **No PSK31 text ever reaches
/// `Ft8MessageSplit.Split`**, which reads any three words as an FT8 message.</para>
/// </remarks>
public static class Psk31ExchangeParser
{
    /// <summary>The addressee of a call to anyone.</summary>
    public const string Anyone = "ANY";

    /// <summary>The addressee of a `CQ DX`.</summary>
    public const string Dx = "DX";

    /// <summary>The shape of a callsign, the one the tree already uses.</summary>
    public const string CallsignPattern =
        @"^(?:[A-Z]{1,2}|[A-Z][0-9]|[0-9][A-Z])[0-9][A-Z]{1,4}(?:/[A-Z0-9]{1,3})?$";

    /// <summary>The words that hand the turn over (`rules[2]`).</summary>
    public static readonly IReadOnlySet<string> TurnoverWords =
        new HashSet<string>(StringComparer.Ordinal) { "K", "KN", "BTU", "OVER" };

    /// <summary>The words that close (`rules[2]`).</summary>
    public static readonly IReadOnlySet<string> ClosingWords =
        new HashSet<string>(StringComparer.Ordinal) { "73", "SK", "CL" };

    private static readonly Regex CallsignShape = new(
        CallsignPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RstShape = new(
        "^[1-5][1-9N][1-9N]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex GridShape = new(
        "^[A-R]{2}[0-9]{2}(?:[A-X]{2})?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Characters trimmed off either end of a word.</summary>
    private static readonly char[] Edges = ".,;:!?'\"()".ToCharArray();

    /// <summary>Characters that may sit inside a word without damaging it.</summary>
    private const string Joiners = "/-'.,:;!?\"()";

    /// <summary>Read one message.</summary>
    /// <param name="message">What arrived between two turnovers.</param>
    /// <param name="operatorCallsign">The operator's own callsign, or null.</param>
    /// <returns>The parse.</returns>
    public static Psk31Exchange Read(string? message, string? operatorCallsign)
    {
        var words = Words(message);

        if (words.Count == 0)
        {
            return new Psk31Exchange(null, null, Psk31LineKind.Unknown, false, false, null, null, false);
        }

        // rules[0] and rules[1]: the pair either side of the last `de`.
        var lastDe = words.FindLastIndex(w => IsWord(w, "DE"));
        string? speaker = null;
        string? addressee = null;

        if (lastDe >= 0)
        {
            if (lastDe + 1 < words.Count && IsCallsign(words[lastDe + 1]))
            {
                speaker = words[lastDe + 1].Text;
            }

            addressee = AddresseeBefore(words, lastDe);
        }
        else if (IsWord(words[0], "CQ"))
        {
            // **A CQ WITH NO `de` IS STILL A CALL TO ANYONE**, from nobody the text names.
            addressee = words.Count > 1 && IsWord(words[1], "DX") ? Dx : Anyone;
        }

        var last = words[^1];
        var handsOver = !last.Damaged
            && (TurnoverWords.Contains(last.Text) || ClosingWords.Contains(last.Text));

        // rules[3]: every clean report, and whether a damaged one sits among them.
        var reports = words
            .Where(w => !w.Damaged && RstShape.IsMatch(w.Text))
            .Select(w => w.Text.Replace('N', '9'))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var damagedReport = words.Any(IsDamagedReport);
        var hasReport = reports.Count > 0 || damagedReport;
        var rst = !damagedReport && reports.Count == 1 ? reports[0] : null;

        var grids = words
            .Where(w => !w.Damaged && GridShape.IsMatch(w.Text) && w.Text != "RR73")
            .Select(w => w.Text)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var grid = grids.Count == 1 ? grids[0] : null;

        var kind = KindOf(words, addressee, hasReport);
        var damaged = words.Count(w => w.Damaged);

        var certain = damaged == 0
            && speaker is not null
            && addressee is not null
            && handsOver
            && kind is not (Psk31LineKind.Garbage or Psk31LineKind.Unknown)
            && (!hasReport || rst is not null)
            && grids.Count <= 1
            && OpensOnItsSign(words, speaker, addressee)
            && EveryDeNamesThePair(words, speaker, addressee);

        var forOperator = IsStation(addressee)
            && Ft8MessageSplit.IsSameStation(addressee, operatorCallsign);

        return new Psk31Exchange(speaker, addressee, kind, handsOver, certain, rst, grid, forOperator);
    }

    /// <summary>One word of a message, upper case, and whether a symbol is welded into it.</summary>
    private readonly record struct Word(string Text, bool Damaged);

    private static List<Word> Words(string? message)
    {
        var words = new List<Word>();

        if (string.IsNullOrEmpty(message))
        {
            return words;
        }

        foreach (var raw in message.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var text = raw.Trim(Edges).ToUpperInvariant();

            if (text.Length == 0)
            {
                continue;
            }

            var hasLetterOrDigit = text.Any(char.IsLetterOrDigit);
            var damaged = text.Any(c => !char.IsLetterOrDigit(c) && !Joiners.Contains(c));

            // **PUNCTUATION ON ITS OWN IS NOT A WORD**, and a lone symbol that is not
            // punctuation is damage.
            if (!hasLetterOrDigit && !damaged)
            {
                continue;
            }

            words.Add(new Word(text, damaged));
        }

        return words;
    }

    private static Psk31LineKind KindOf(List<Word> words, string? addressee, bool hasReport)
    {
        var damaged = words.Count(w => w.Damaged);

        if (damaged > words.Count - damaged)
        {
            return Psk31LineKind.Garbage;
        }

        if (words.All(w => !w.Damaged && TurnoverWords.Contains(w.Text)))
        {
            return Psk31LineKind.Unknown;
        }

        if (addressee is Anyone or Dx)
        {
            return Psk31LineKind.Cq;
        }

        if (EndsOnSignOff(words))
        {
            return Psk31LineKind.End;
        }

        if (words.Any(w => IsWord(w, "73")))
        {
            return Psk31LineKind.Closing;
        }

        if (hasReport)
        {
            return Psk31LineKind.Report;
        }

        if (IsStation(addressee)
            && words.All(w => IsWord(w, "DE") || IsCallsign(w) || (!w.Damaged && TurnoverWords.Contains(w.Text))))
        {
            return Psk31LineKind.Answer;
        }

        return Psk31LineKind.Chat;
    }

    /// <summary>True where the run of turnover and closing words the message ends on holds `SK` or `CL`.</summary>
    private static bool EndsOnSignOff(List<Word> words)
    {
        for (var at = words.Count - 1; at >= 0; at--)
        {
            var word = words[at];

            if (word.Damaged || !(TurnoverWords.Contains(word.Text) || ClosingWords.Contains(word.Text)))
            {
                return false;
            }

            if (word.Text is "SK" or "CL")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The addressee named before the `de` at an index, or null.</summary>
    private static string? AddresseeBefore(List<Word> words, int de)
    {
        if (de == 0)
        {
            return null;
        }

        var before = words[de - 1];

        if (IsCallsign(before))
        {
            return before.Text;
        }

        if (IsWord(before, "CQ"))
        {
            return Anyone;
        }

        return IsWord(before, "DX") && words.Take(de - 1).Any(w => IsWord(w, "CQ")) ? Dx : null;
    }

    private static bool OpensOnItsSign(List<Word> words, string speaker, string addressee)
    {
        if (IsWord(words[0], "CQ"))
        {
            return addressee is Anyone or Dx;
        }

        return words.Count >= 3
            && IsCallsign(words[0]) && IsWord(words[1], "DE") && IsCallsign(words[2])
            && words[0].Text == addressee
            && words[2].Text == speaker;
    }

    private static bool EveryDeNamesThePair(List<Word> words, string speaker, string addressee)
    {
        for (var at = 0; at < words.Count; at++)
        {
            if (!IsWord(words[at], "DE"))
            {
                continue;
            }

            if (at + 1 >= words.Count || words[at + 1].Damaged || words[at + 1].Text != speaker)
            {
                return false;
            }

            if (AddresseeBefore(words, at) != addressee)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>A damaged word that would have been a three-character report.</summary>
    private static bool IsDamagedReport(Word word)
        => word.Damaged
            && word.Text.Length == 3
            && word.Text.Any(char.IsDigit)
            && word.Text.All(c => char.IsDigit(c) || c == 'N' || !char.IsLetterOrDigit(c));

    private static bool IsCallsign(Word word) => !word.Damaged && CallsignShape.IsMatch(word.Text);

    private static bool IsWord(Word word, string text) => !word.Damaged && word.Text == text;

    private static bool IsStation(string? addressee) => addressee is not null and not Anyone and not Dx;
}
