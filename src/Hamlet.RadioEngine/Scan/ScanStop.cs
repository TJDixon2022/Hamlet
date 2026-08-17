using System.Text;
using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Scan;

/// <summary>
/// Why a scan stopped somewhere, or why it did not (HM-DEC-107, phase 7).
/// </summary>
/// <remarks>
/// <para>**A SCANNER THAT STOPS ON "CQ" IS WORTH TEN OF ONE THAT STOPS ON
/// "THERE IS A TONE HERE."** The waterfall found the tone, and a tone is a
/// carrier, a birdie, a switching supply two houses away, or somebody calling.
/// Only the decoder can tell those apart, so the reason a scan stopped is the
/// reason it is worth having stopped, and it is carried onto the screen rather
/// than reduced to a bare halt.</para>
/// </remarks>
public enum ScanStopReason
{
    /// <summary>Nothing resolved into characters at all. Keep going.</summary>
    NothingHeard,

    /// <summary>
    /// Characters arrived and none of them formed anything a person sends.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE COMMON CASE AND IT IS NOT A FAILURE.** A tone that produces
    /// letters at random is a tone the decoder could not read, which is a fact
    /// about the signal rather than about the band, and moving on is the right
    /// answer (§0.0).
    /// </remarks>
    NothingRecognized,

    /// <summary>Somebody is calling: <c>CQ</c>.</summary>
    /// <remarks>
    /// The strongest reason there is. A CQ is an explicit invitation, so this is
    /// the one case where a scan has found not merely a signal but a person
    /// asking to be answered.
    /// </remarks>
    Calling,

    /// <summary>A handover: <c>DE</c>, somebody naming who they are.</summary>
    Handover,

    /// <summary>A token shaped like a callsign, in no particular position.</summary>
    /// <remarks>
    /// **THIS IS EVIDENCE THAT SOMEBODY IS THERE AND IT IS NOT A CALLSIGN
    /// CLAIM.** HM-DEC-073 permits a callsign to be named only after <c>DE</c>,
    /// before <c>DE</c>, or before a closing prosign, and with every character
    /// solid. Loose text that happens to fit the shape is enough to justify
    /// pausing the dial and is nowhere near enough to put a name on screen, so
    /// this reason carries no callsign with it.
    /// </remarks>
    CallsignShaped,

    /// <summary>A close: <c>K</c>, <c>KN</c>, <c>SK</c>, <c>73</c>.</summary>
    Closing,

    /// <summary>
    /// The same run of characters came round more than once.
    /// </summary>
    /// <remarks>
    /// A beacon, a station calling repeatedly, or somebody tuning up on a keyer
    /// loop. Repetition is the one structure that survives a decode too poor to
    /// read, because two bad readings of the same thing are bad in the same way.
    /// </remarks>
    Repeated,
}

/// <summary>
/// What a dwell came to.
/// </summary>
/// <param name="Stop">Whether this is somewhere to stay.</param>
/// <param name="Reason">What was found, or what was not.</param>
/// <param name="Confidence">
/// How far this stands up, from nought to one, taken from the decoder's own
/// confidence in the characters the evidence is made of.
/// </param>
/// <param name="Evidence">
/// The text the verdict rests on, so the operator can disagree with it.
/// </param>
/// <param name="Characters">How many characters the dwell produced.</param>
/// <param name="Solid">How many of those the decoder stood fully behind.</param>
/// <remarks>
/// <para>**THE CONFIDENCE TRAVELS, WHICH IS THE WHOLE REASON THIS IS A RECORD
/// AND NOT A BOOLEAN.** Stopping on a CQ every character of which was solid and
/// stopping on one assembled from dim letters are different events, and a screen
/// that draws them identically has presented a guess as a decode (§0.0). What a
/// surface does with a low number is a display question and is not settled
/// here.</para>
/// </remarks>
public readonly record struct ScanVerdict(
    bool Stop,
    ScanStopReason Reason,
    double Confidence,
    string Evidence,
    int Characters,
    int Solid)
{
    /// <summary>The verdict for a dwell that heard nothing.</summary>
    public static ScanVerdict Silent { get; }
        = new(false, ScanStopReason.NothingHeard, 0, string.Empty, 0, 0);

    /// <summary>
    /// What happened, in the app's voice.
    /// </summary>
    /// <remarks>
    /// Says what was heard and how sure Hamlet is, and never what kind of
    /// station is there or whether they would answer (§0.7, §0.0).
    /// </remarks>
    public string Sentence => Reason switch
    {
        ScanStopReason.Calling
            => $"somebody is calling CQ here, and Hamlet is {Sureness} of that",
        ScanStopReason.Handover
            => $"a station named itself here, and Hamlet is {Sureness} of that",
        ScanStopReason.CallsignShaped
            => "something callsign shaped came through here, though not in a "
               + "place where Hamlet will put a name to it",
        ScanStopReason.Closing
            => "a contact was being signed off here, so somebody was working "
               + "somebody",
        ScanStopReason.Repeated
            => "the same thing came round more than once here, which is what a "
               + "station calling over and over sounds like",
        ScanStopReason.NothingRecognized
            => $"{Characters} characters came out of this and none of them made "
               + "anything a person sends, so it is a signal Hamlet could not read",
        _ => "nothing resolved here at all",
    };

    private string Sureness => Confidence switch
    {
        >= 0.8 => "sure",
        >= 0.5 => "fairly sure",
        _ => "not at all sure",
    };
}

/// <summary>
/// Decides whether what a dwell heard is worth stopping for (HM-DEC-107,
/// phase 7).
/// </summary>
/// <remarks>
/// <para>**IT ANSWERS A NARROWER QUESTION THAN IT LOOKS.** Not "is anybody
/// there", which no amount of decoded text settles, but "did this window
/// contain something only a person sending Morse produces". <c>CQ</c>,
/// <c>DE</c>, a closing prosign and a repeat are that; a tone is not, and a
/// scattering of letters is not (§0.0).</para>
/// <para>Pure: characters in, a verdict out. No radio, no clock, no allocation
/// per character beyond the tokens themselves (§5).</para>
/// </remarks>
public static class ScanStopClassifier
{
    /// <summary>How many characters a repeat has to run to count.</summary>
    /// <remarks>
    /// Four. Shorter than that and ordinary English text repeats by accident,
    /// and a scanner that stops on a coincidence is a scanner nobody trusts.
    /// </remarks>
    public const int LeastRepeatLength = 4;

    private static readonly HashSet<string> ClosingWords = new(StringComparer.Ordinal)
    {
        "K", "KN", "SK", "<KN>", "<SK>", "<AR>", "AR", "73",
    };

    private static readonly Regex CallsignShape = new(
        @"^(?:[A-Z]{1,2}|[A-Z][0-9]|[0-9][A-Z])[0-9][A-Z]{1,4}(?:/[A-Z0-9]{1,3})?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> NotAStation = new(StringComparer.Ordinal)
    {
        "CQ", "DE", "K", "KN", "SK", "AR", "TU", "UR", "RST", "QRZ", "TEST",
        "73", "88", "5NN", "599", "R",
    };

    /// <summary>
    /// Judge one dwell's worth of decoded characters.
    /// </summary>
    /// <param name="heard">What the decoder produced, in order.</param>
    /// <returns>
    /// The verdict. Never throws, and an empty window is
    /// <see cref="ScanVerdict.Silent"/> rather than an error.
    /// </returns>
    /// <remarks>
    /// <para>**THE ORDER THE REASONS ARE TESTED IN IS THE ORDER OF THEIR
    /// WORTH.** A window holding both a CQ and a callsign-shaped token reports
    /// the CQ, because that is what makes the frequency worth the operator's
    /// evening.</para>
    /// <para>**HOW SOLID THE CHARACTERS WERE IS REPORTED AND NEVER A VETO, AND
    /// IT WAS THE OTHER WAY ROUND FIRST.** A gate on it refused a <c>CQ</c>
    /// assembled entirely from dim letters, which is the right answer for a
    /// transcript and the wrong one for a scanner. Stopping the dial is not a
    /// claim about what was sent: it costs the operator fifteen seconds and he
    /// can hear the frequency for himself, and the cost of not stopping is that
    /// Hamlet drives past the one station on the band it was meant to find. So
    /// the confidence carries it, all the way into the sentence, which says
    /// "not at all sure" rather than pretending (§0.0).</para>
    /// </remarks>
    public static ScanVerdict Judge(IReadOnlyList<CwCharacter>? heard)
    {
        if (heard is null || heard.Count == 0)
        {
            return ScanVerdict.Silent;
        }

        var tokens = Tokenize(heard);

        if (tokens.Count == 0)
        {
            return ScanVerdict.Silent;
        }

        var characters = heard.Count(c => !c.IsWordGap);
        var solid = heard.Count(c => !c.IsWordGap && c.Confidence == CwConfidence.High);

        ScanVerdict Found(ScanStopReason reason, Token token)
            => new(true, reason, token.Confidence, token.Text, characters, solid);

        foreach (var token in tokens)
        {
            if (string.Equals(token.Text, "CQ", StringComparison.Ordinal))
            {
                return Found(ScanStopReason.Calling, token);
            }
        }

        foreach (var token in tokens)
        {
            if (string.Equals(token.Text, "DE", StringComparison.Ordinal))
            {
                return Found(ScanStopReason.Handover, token);
            }
        }

        var repeat = LongestRepeat(heard);

        if (repeat is { } run)
        {
            return new ScanVerdict(
                true, ScanStopReason.Repeated, run.Confidence, run.Text,
                characters, solid);
        }

        foreach (var token in tokens)
        {
            if (!NotAStation.Contains(token.Text)
                && CallsignShape.IsMatch(token.Text))
            {
                return Found(ScanStopReason.CallsignShaped, token);
            }
        }

        foreach (var token in tokens)
        {
            if (ClosingWords.Contains(token.Text))
            {
                return Found(ScanStopReason.Closing, token);
            }
        }

        return new ScanVerdict(
            false, ScanStopReason.NothingRecognized, 0, string.Empty,
            characters, solid);
    }

    /// <summary>
    /// The longest run of characters that appears more than once.
    /// </summary>
    /// <remarks>
    /// Naive, and deliberately so: a dwell is twenty seconds of Morse, which is
    /// a few dozen characters, and a suffix structure to search it would be more
    /// machinery than the problem has.
    /// </remarks>
    private static Token? LongestRepeat(IReadOnlyList<CwCharacter> heard)
    {
        var readable = heard
            .Where(c => !c.IsWordGap && !c.IsUnreadable)
            .ToList();

        if (readable.Count < LeastRepeatLength * 2)
        {
            return null;
        }

        var text = string.Concat(readable.Select(c => c.Text));

        for (var length = Math.Min(text.Length / 2, 24); length >= LeastRepeatLength; length--)
        {
            for (var start = 0; start + (length * 2) <= text.Length; start++)
            {
                var run = text.Substring(start, length);

                if (text.IndexOf(run, start + length, StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                // The confidence is the run's own characters, not the window's:
                // a repeat made of dim letters is still a repeat and is still
                // worth less than one made of solid ones.
                var scores = readable
                    .Skip(start)
                    .Take(length)
                    .Select(c => c.Score)
                    .ToList();

                return new Token(run, scores.Average(), SolidShare: 1);
            }
        }

        return null;
    }

    private readonly record struct Token(string Text, double Confidence, double SolidShare);

    private static List<Token> Tokenize(IReadOnlyList<CwCharacter> heard)
    {
        var tokens = new List<Token>();
        var text = new StringBuilder();
        var scores = new List<double>();
        var solid = 0;

        void Flush()
        {
            if (text.Length > 0)
            {
                tokens.Add(new Token(
                    text.ToString(),
                    scores.Count == 0 ? 0 : scores.Average(),
                    scores.Count == 0 ? 0 : (double)solid / scores.Count));
            }

            text.Clear();
            scores.Clear();
            solid = 0;
        }

        foreach (var character in heard)
        {
            if (character.IsWordGap)
            {
                Flush();
                continue;
            }

            if (character.IsUnreadable)
            {
                // A hole in a word is not a word. Splitting here keeps a
                // placeholder from silently welding two tokens into one that
                // nobody sent (§0.0).
                Flush();
                continue;
            }

            text.Append(character.Text);
            scores.Add(character.Score);

            if (character.Confidence == CwConfidence.High)
            {
                solid++;
            }
        }

        Flush();

        return tokens;
    }
}
