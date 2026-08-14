using System.Text;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What the radio's keyer will accept, and how a longer message is broken up
/// to fit (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>THIRTY CHARACTERS IS THE LIMIT ON COMMAND 17 (Full Manual p. 19-11,
/// "Codes for CW message contents ... Up to 30 characters"), and the UI never
/// presents a message it cannot actually send. So a longer one is split here,
/// in the engine, at the spaces, and every piece is a whole word.</para>
/// <para>THE CHARACTER SET IS THE MANUAL'S OWN, not an assumption about Morse.
/// The radio accepts digits, letters in either case, and a short list of
/// punctuation, and it will not accept anything else. A character it cannot
/// send is dropped rather than substituted, because a message that quietly
/// became a different message is the prime directive broken on the way out
/// (§0.0).</para>
/// <para>Pure: text in, pieces out. No radio, no clock (§5).</para>
/// </remarks>
public static class CwMessage
{
    /// <summary>How many characters one keyer message may carry (p. 19-11).</summary>
    public const int MaximumLength = 30;

    /// <summary>The message that stops a send in progress (p. 19-11).</summary>
    public const string StopMessage = "\xFF";

    /// <summary>
    /// Every character the radio's keyer accepts (p. 19-11).
    /// </summary>
    /// <remarks>
    /// Transcribed from the manual's own table rather than assumed: digits,
    /// letters, and exactly this punctuation. Lower case is accepted and is
    /// folded to upper here, because Morse has no case and a callsign reads
    /// better in capitals.
    /// </remarks>
    public const string Allowed = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ/?.-,:'()=+\"@ ";

    /// <summary>
    /// Fold a message to what the keyer will accept.
    /// </summary>
    /// <param name="text">What the operator or the app composed.</param>
    /// <returns>The same message in sendable characters, trimmed.</returns>
    /// <remarks>
    /// Runs of spaces collapse, because the keyer treats a run as one gap and
    /// showing the operator something the radio will not reproduce is a small
    /// lie about what is going out.
    /// </remarks>
    public static string Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        var built = new StringBuilder(text.Length);
        var lastWasSpace = false;

        foreach (var raw in text.ToUpperInvariant())
        {
            if (!Allowed.Contains(raw, StringComparison.Ordinal))
            {
                continue;
            }

            if (raw == ' ')
            {
                if (!lastWasSpace && built.Length > 0)
                {
                    built.Append(' ');
                }

                lastWasSpace = true;
                continue;
            }

            built.Append(raw);
            lastWasSpace = false;
        }

        return built.ToString().TrimEnd();
    }

    /// <summary>True when every character in the text is sendable.</summary>
    /// <param name="text">The text.</param>
    /// <returns>True when nothing would be dropped.</returns>
    public static bool IsSendable(string? text)
        => !string.IsNullOrEmpty(text)
           && text.ToUpperInvariant().All(c => Allowed.Contains(c, StringComparison.Ordinal));

    /// <summary>
    /// Break a message into pieces the keyer can take, in order.
    /// </summary>
    /// <param name="text">The message.</param>
    /// <returns>One or more pieces, none longer than the limit; empty for nothing.</returns>
    /// <remarks>
    /// <para>SPLIT AT THE SPACES, so a callsign is never cut in half. A word
    /// longer than the whole limit has nowhere to break and is cut, which cannot
    /// happen with real Morse text and is handled rather than left to throw.
    /// </para>
    /// <para>The pieces are sent as separate messages, so the radio inserts its
    /// own word gap between them. That is the same gap it would have sent
    /// anyway, which is why splitting at a space and nowhere else is what keeps
    /// a long message sounding like one message.</para>
    /// </remarks>
    public static IReadOnlyList<string> Split(string? text)
    {
        var clean = Clean(text);

        if (clean.Length == 0)
        {
            return Array.Empty<string>();
        }

        if (clean.Length <= MaximumLength)
        {
            return new[] { clean };
        }

        var pieces = new List<string>();
        var current = new StringBuilder();

        foreach (var word in clean.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            // A word nobody could send in one piece. Cut it, rather than
            // dropping it or throwing on a case real Morse cannot produce.
            if (word.Length > MaximumLength)
            {
                Flush(pieces, current);

                for (var at = 0; at < word.Length; at += MaximumLength)
                {
                    pieces.Add(word.Substring(at, Math.Min(MaximumLength, word.Length - at)));
                }

                continue;
            }

            var wouldBe = current.Length == 0 ? word.Length : current.Length + 1 + word.Length;

            if (wouldBe > MaximumLength)
            {
                Flush(pieces, current);
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(word);
        }

        Flush(pieces, current);
        return pieces;
    }

    /// <summary>How many keyer messages this text will take.</summary>
    /// <param name="text">The message.</param>
    /// <returns>The count; 0 for nothing to send.</returns>
    public static int PieceCount(string? text) => Split(text).Count;

    private static void Flush(List<string> pieces, StringBuilder current)
    {
        if (current.Length > 0)
        {
            pieces.Add(current.ToString());
            current.Clear();
        }
    }
}
