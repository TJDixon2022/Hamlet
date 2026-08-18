using System.Text;
using System.Text.RegularExpressions;

namespace Hamlet.RadioEngine.Cw;

/// <summary>One listening window's worth of evidence (phase 3).</summary>
/// <param name="Heard">What the decoder settled on, in order.</param>
/// <param name="StationChanged">
/// True when the tracker followed a different station during the window.
/// </param>
/// <remarks>
/// <para>**THE TRACKER'S OWN MOVE IS EVIDENCE AND IT ARRIVES SOONER THAN TEXT.**
/// A follow means the survey found keying at a pitch that is not the one it was
/// reading, which operationally means somebody else started transmitting — and it
/// is decided on three seconds of mark-length structure rather than on a
/// classifier having enough letters to work with (HM-DEC-095, HM-DEC-123).</para>
/// <para>**IT IS A FOLLOW AND NEVER A REFINEMENT.** A refining retune is the same
/// station found more precisely and says nothing about anybody arriving, which is
/// exactly the distinction HM-DEC-123 built; counting those would stop the cycle
/// every time the survey preferred its neighbouring bin.</para>
/// </remarks>
public readonly record struct AutoCallWindow(
    IReadOnlyList<CwCharacter> Heard, bool StationChanged)
{
    /// <summary>A window in which nothing happened at all.</summary>
    public static AutoCallWindow Empty { get; }
        = new(Array.Empty<CwCharacter>(), false);
}

/// <summary>What was heard after a CQ went out (phase 3).</summary>
/// <param name="Stop">Whether the cycle should stop.</param>
/// <param name="IsAnswer">
/// True only where what was heard is shaped like somebody answering.
/// </param>
/// <param name="Why">What was heard, in the app's voice.</param>
/// <param name="Evidence">The text it rests on, so the operator can disagree.</param>
/// <param name="Confidence">
/// How far the decoder stood behind the characters this is made of, nought to
/// one.
/// </param>
/// <remarks>
/// <para>**AN ANSWER AND SOMETHING-WAS-HEARD ARE DIFFERENT CLAIMS AND MUST LOOK
/// DIFFERENT** (§0.0). One says a person replied to this operator's call; the
/// other says the frequency is not empty and Hamlet will not say more than that.
/// Collapsing them into one stop would let the weaker claim borrow the stronger
/// one's authority on the one screen where the operator is about to answer
/// somebody.</para>
/// </remarks>
public sealed record AutoCallAnswer(
    bool Stop, bool IsAnswer, string Why, string Evidence, double Confidence)
{
    /// <summary>Nothing came back.</summary>
    public static AutoCallAnswer Nothing { get; } = new(false, false, "", "", 0);
}

/// <summary>
/// Deciding whether a CQ was answered (phase 3).
/// </summary>
/// <remarks>
/// <para>**MISSING A REAL ANSWER IS WORSE THAN STOPPING ON NOISE, SO THIS IS
/// BIASED TOWARD STOPPING.** The cost of a false stop is that the operator
/// presses start again. The cost of a missed answer is that Hamlet transmits a
/// CQ over the top of somebody's reply, under his callsign, on a frequency they
/// are both trying to use — and the other operator hears a station that called
/// and then talked over the answer.</para>
/// <para>**A CALLSIGN-SHAPED TOKEN IS NOT A CALLSIGN** (HM-DEC-073). Loose text
/// that fits the shape is ample reason to stop transmitting and nowhere near
/// enough to put a name on screen, so those verdicts carry no name — exactly as
/// the scanner's already do not.</para>
/// <para>Pure: characters in, a verdict out. No radio, no clock, no decoder
/// (§5).</para>
/// </remarks>
public static class AutoCallAnswers
{
    /// <summary>How sure the decoder must be before unrecognized text stops it.</summary>
    /// <remarks>
    /// **CONFIDENT TEXT ONLY, FOR THE SECOND TIER AND NOT THE FIRST.** A window
    /// of dim letters is a signal Hamlet could not read, and stopping the cycle
    /// on every one of those would make the feature useless on a busy evening.
    /// QSO-shaped text stops whatever its confidence, because the shape is the
    /// evidence; unrecognized text has only its confidence to offer.
    /// </remarks>
    public const double ConfidentEnough = 0.7;

    /// <summary>How many characters unrecognized text needs before it counts.</summary>
    /// <remarks>
    /// Four. Two or three confident letters out of a fading band is what a
    /// decoder produces all evening, and a cycle that stopped on every one would
    /// never finish a round.
    /// </remarks>
    public const int LeastUnrecognized = 4;

    /// <summary>
    /// How many it needs where the tracker also followed a new station.
    /// </summary>
    /// <remarks>
    /// **TWO, AND THE HALVING IS WHAT "EVIDENCE RATHER THAN A VERDICT" MEANS.**
    /// The tracker moving does not decide anything on its own — it stops the cycle
    /// without claiming an answer — but it makes a couple of confident letters mean
    /// what four would have meant without it. Two independent signals agreeing is
    /// worth more than either alone, which is the same reasoning that makes the
    /// survey demand two agreeing reads before it believes anything.
    /// </remarks>
    public const int LeastWithAStationChange = 2;

    /// <summary>How long a repeat has to run to count as one.</summary>
    public const int LeastRepeatLength = 4;

    private static readonly Regex CallsignShape = new(
        @"^(?:[A-Z]{1,2}|[A-Z][0-9]|[0-9][A-Z])[0-9][A-Z]{1,4}(?:/[A-Z0-9]{1,3})?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> NotAStation = new(StringComparer.Ordinal)
    {
        "CQ", "DE", "K", "KN", "SK", "AR", "TU", "UR", "RST", "QRZ", "TEST",
        "73", "88", "5NN", "599", "R",
    };

    private static readonly HashSet<string> Closing = new(StringComparer.Ordinal)
    {
        "K", "R", "73", "KN", "<KN>", "SK", "<SK>",
    };

    /// <summary>
    /// Judge one listening window.
    /// </summary>
    /// <param name="heard">What the decoder produced, in order.</param>
    /// <param name="ownMessage">
    /// What was transmitted, so the operator's own callsign can be recognized
    /// coming back.
    /// </param>
    /// <param name="stationChanged">
    /// True where the tracker followed a different station during the window.
    /// **Evidence and never a verdict**: on its own it stops the cycle without
    /// claiming an answer, and beside a couple of confident characters it means
    /// what four would have meant alone.
    /// </param>
    /// <returns>The verdict. Never throws; an empty window is not an answer.</returns>
    /// <remarks>
    /// <para>**THE ORDER IS THE ORDER OF WHAT THEY ESTABLISH.** The operator's
    /// own callsign coming back is the strongest thing this can see, because
    /// nobody else on the band has a reason to send it. `DE` and a
    /// callsign-shaped token is next. A closing word and a repeat are weaker and
    /// still stop, because all of them mean somebody is transmitting where a
    /// CQ just went out.</para>
    /// </remarks>
    public static AutoCallAnswer Judge(
        IReadOnlyList<CwCharacter>? heard,
        string? ownMessage = null,
        bool stationChanged = false)
    {
        if (heard is null || heard.Count == 0)
        {
            // **THE TRACKER MOVING WITH NOTHING READABLE STILL STOPS IT.**
            // Somebody started transmitting near enough for the survey to find
            // their keying and Hamlet could not read a word of it, which is every
            // reason to stop calling and no reason at all to claim an answer
            // (§0.0).
            return stationChanged
                ? new AutoCallAnswer(
                    Stop: true,
                    IsAnswer: false,
                    Why: "the tracker followed a different station here, so "
                        + "somebody started transmitting, and Hamlet could not "
                        + "read what they sent",
                    Evidence: "",
                    Confidence: 0)
                : AutoCallAnswer.Nothing;
        }

        var tokens = Tokenize(heard);

        var mine = OwnCallsign(ownMessage);

        if (tokens.Count == 0)
        {
            return stationChanged
                ? new AutoCallAnswer(
                    Stop: true,
                    IsAnswer: false,
                    Why: "the tracker followed a different station here, so "
                        + "somebody started transmitting, and Hamlet could not "
                        + "read what they sent",
                    Evidence: "",
                    Confidence: 0)
                : AutoCallAnswer.Nothing;
        }


        if (mine.Length > 0)
        {
            foreach (var token in tokens)
            {
                if (string.Equals(token.Text, mine, StringComparison.Ordinal))
                {
                    return new AutoCallAnswer(
                        true, true,
                        $"it heard your own callsign come back, {mine}",
                        token.Text, token.Confidence);
                }
            }
        }

        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (!string.Equals(tokens[i].Text, "DE", StringComparison.Ordinal))
            {
                continue;
            }

            var next = tokens[i + 1];

            if (!NotAStation.Contains(next.Text) && CallsignShape.IsMatch(next.Text))
            {
                // **NAMED AS A SHAPE AND NEVER AS A CALLSIGN** (HM-DEC-073). It
                // is after DE, which is one of that ruling's own positions, and
                // the other half of it is that every character be solid. Stopping
                // the transmitter does not need that; putting a name on screen
                // does, and this does not put one there.
                return new AutoCallAnswer(
                    true, true,
                    "a station named itself here, straight after your call",
                    "DE " + next.Text, next.Confidence);
            }
        }

        foreach (var token in tokens)
        {
            if (Closing.Contains(token.Text))
            {
                return new AutoCallAnswer(
                    true, true,
                    "somebody handed it back, which is what an answer to a CQ "
                    + "sounds like",
                    token.Text, token.Confidence);
            }
        }

        if (LongestRepeat(heard) is { } repeat)
        {
            return new AutoCallAnswer(
                true, true,
                "the same run of characters came round more than once, which is "
                + "what a station calling you repeatedly sounds like",
                repeat.Text, repeat.Confidence);
        }

        // **THE SECOND TIER: CONFIDENT, AND NOT ANYTHING HAMLET RECOGNIZES.**
        // Somebody is transmitting close enough to be read. That is not an
        // answer and it is every reason not to call over them.
        var solid = heard
            .Where(c => !c.IsWordGap && !c.IsUnreadable && c.Score >= ConfidentEnough)
            .ToList();

        var enough = stationChanged ? LeastWithAStationChange : LeastUnrecognized;

        if (solid.Count >= enough)
        {
            // **STOPS, AND IS NOT AN ANSWER.** Those are the two halves of this
            // tier and they are written out rather than inferred: somebody is
            // transmitting where a CQ just went out, which is every reason to
            // stop and no reason at all to say they replied to it (§0.0).
            return new AutoCallAnswer(
                Stop: true,
                IsAnswer: false,
                Why: $"it read {solid.Count} characters here it could not make "
                    + "anything of"
                    + (stationChanged
                        ? ", and the tracker followed a different station while it "
                          + "was listening"
                        : ""),
                Evidence: string.Concat(solid.Select(c => c.Text)),
                Confidence: solid.Average(c => c.Score));
        }

        return AutoCallAnswer.Nothing;
    }

    /// <summary>
    /// The operator's own callsign, taken from what he asked Hamlet to send.
    /// </summary>
    /// <remarks>
    /// **READ OUT OF HIS OWN MESSAGE RATHER THAN THE PROFILE**, so the two can
    /// never disagree. What matters is the callsign that actually went on the
    /// air, and the message is the only thing that knows it. The token after
    /// `DE` is his, which is what `DE` means.
    /// </remarks>
    public static string OwnCallsign(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "";
        }

        var words = CwMessage.Clean(message)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < words.Length - 1; i++)
        {
            if (!string.Equals(words[i], "DE", StringComparison.Ordinal))
            {
                continue;
            }

            var candidate = words[i + 1];

            if (!NotAStation.Contains(candidate) && CallsignShape.IsMatch(candidate))
            {
                return candidate;
            }
        }

        return "";
    }

    private readonly record struct Token(string Text, double Confidence);

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

                var scores = readable.Skip(start).Take(length).Select(c => c.Score).ToList();

                return new Token(run, scores.Average());
            }
        }

        return null;
    }

    private static List<Token> Tokenize(IReadOnlyList<CwCharacter> heard)
    {
        var tokens = new List<Token>();
        var text = new StringBuilder();
        var scores = new List<double>();

        void Flush()
        {
            if (text.Length > 0)
            {
                tokens.Add(new Token(
                    text.ToString(), scores.Count == 0 ? 0 : scores.Average()));
            }

            text.Clear();
            scores.Clear();
        }

        foreach (var character in heard)
        {
            // A hole in a word is not a word, and a placeholder welding two
            // tokens together would manufacture a callsign nobody sent (§0.0).
            if (character.IsWordGap || character.IsUnreadable)
            {
                Flush();
                continue;
            }

            text.Append(character.Text);
            scores.Add(character.Score);
        }

        Flush();

        return tokens;
    }
}
