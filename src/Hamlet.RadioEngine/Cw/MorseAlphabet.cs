namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Turns a measured run of dots and dashes into the character it stands for,
/// prosigns included.
/// </summary>
/// <remarks>
/// <para>The receiving half of <c>MorseCode</c>, which spells characters out.
/// Kept separate because it needs things the sending table does not: the
/// prosigns, and an honest answer when a pattern belongs to nothing.</para>
/// <para>PROSIGNS ARE NOT LETTERS AND MUST NOT BE SHOWN AS LETTERS. An operator
/// ending a message sends <c>.-.-.</c> as one run with no gap in the middle of
/// it, and the IC-7300 has a character for exactly that: the Full Manual's
/// section 19 lists <c>^</c> as "used to transmit a string of characters with
/// no inter-character space" (p. 19-12). A decoder that split that run into
/// "AR", or worse into "EN" or "RK" depending on where it guessed the gaps
/// were, would be wrong in the most confusing way available: it would look like
/// a decoding error in a sentence rather than a symbol the reader has not met
/// yet. So they come out as <c>&lt;AR&gt;</c>, which is unmistakably one thing
/// with a name.</para>
/// <para>SOME PATTERNS HAVE TWO NAMES, and this is a naming choice rather than
/// a guess about the signal (§0.0). On the air <c>-...-</c> is the same sound
/// whether the sender was thinking "BT" or thinking "=", and <c>.-.-.</c> is
/// the same sound for "AR" and for "+". Where a pattern has both a punctuation
/// name and a prosign name, the prosign wins, because that is overwhelmingly
/// what it means inside a contact and it is the reading a newcomer needs
/// explained rather than the one they can look up.</para>
/// </remarks>
public static class MorseAlphabet
{
    /// <summary>
    /// What is shown where something was clearly heard and could not be
    /// resolved.
    /// </summary>
    /// <remarks>
    /// A filled block rather than a letter, a question mark or a dot. A letter
    /// would be a guess presented as a decode, which is the one thing this
    /// project exists to prevent (§0.0). A question mark is a real Morse
    /// character and would be indistinguishable from one that was actually
    /// sent. A dot is too easy to read past. This says "something was here and
    /// Hamlet could not tell you what" and cannot be mistaken for content.
    /// </remarks>
    public const string Unreadable = "■";

    /// <summary>A word gap, as it appears in the transcript.</summary>
    public const string WordGap = " ";

    private static readonly Dictionary<string, string> Table = BuildTable();

    /// <summary>
    /// The character a pattern stands for, or null when it stands for nothing.
    /// </summary>
    /// <param name="pattern">Dots and dashes, as measured.</param>
    /// <returns>The character or prosign, or null.</returns>
    /// <remarks>
    /// Null is a real answer and the caller must keep it as one. Padding an
    /// unknown pattern out to the nearest letter is exactly the failure
    /// HM-DEC-009 forbids, and it does specific damage here: somebody learning
    /// to copy reads the garbage, believes it, and concludes the fault is
    /// theirs.
    /// </remarks>
    public static string? Lookup(string pattern)
        => Table.TryGetValue(pattern, out var text) ? text : null;

    /// <summary>Every pattern the decoder can name, for tests and tooling.</summary>
    public static IReadOnlyDictionary<string, string> All => Table;

    private static Dictionary<string, string> BuildTable()
    {
        var table = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [".-"] = "A", ["-..."] = "B", ["-.-."] = "C", ["-.."] = "D",
            ["."] = "E", ["..-."] = "F", ["--."] = "G", ["...."] = "H",
            [".."] = "I", [".---"] = "J", ["-.-"] = "K", [".-.."] = "L",
            ["--"] = "M", ["-."] = "N", ["---"] = "O", [".--."] = "P",
            ["--.-"] = "Q", [".-."] = "R", ["..."] = "S", ["-"] = "T",
            ["..-"] = "U", ["...-"] = "V", [".--"] = "W", ["-..-"] = "X",
            ["-.--"] = "Y", ["--.."] = "Z",

            ["-----"] = "0", [".----"] = "1", ["..---"] = "2", ["...--"] = "3",
            ["....-"] = "4", ["....."] = "5", ["-...."] = "6", ["--..."] = "7",
            ["---.."] = "8", ["----."] = "9",

            ["..--.."] = "?", ["-..-."] = "/", ["--..--"] = ",",
            [".-.-.-"] = ".", ["---..."] = ":", ["-....-"] = "-",
            [".----."] = "'", ["-.-.--"] = "!", [".--.-."] = "@",
            ["-.--.-"] = ")",

            // The prosigns. Each is one run with no gap inside it, which is
            // what the IC-7300's "^" sends and what an operator's fist does
            // without being asked (Full Manual p. 19-12).
            [".-.-."] = "<AR>",      // End of message. Also "+".
            ["...-.-"] = "<SK>",     // End of contact.
            ["-...-"] = "<BT>",      // A pause between thoughts. Also "=".
            ["-.--."] = "<KN>",      // Go ahead, you specifically. Also "(".
            [".-..."] = "<AS>",      // Wait.
            ["........"] = "<HH>",   // I made a mistake, here it comes again.
        };

        return table;
    }
}
