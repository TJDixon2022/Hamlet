using System.Text;

namespace Hamlet.RadioEngine.Training;

/// <summary>
/// Morse timing and the character table, shared by the waterfall's keying
/// rhythm and the field guide's audio so the eye and the ear are taught the
/// same thing.
/// </summary>
/// <remarks>
/// Speed follows the PARIS standard: one dit is 1200/WPM milliseconds, a dah
/// is three dits, the gap inside a character is one dit, between characters
/// three, between words seven. Those ratios are what make Morse legible at
/// any speed, and getting them wrong would teach a newcomer a rhythm they
/// will never hear on the air.
/// </remarks>
public static class MorseCode
{
    /// <summary>Milliseconds in one dit at one word per minute.</summary>
    public const double DitMillisecondsAt1Wpm = 1200.0;

    /// <summary>
    /// Marks the characters after it as one symbol, up to the next space.
    /// </summary>
    /// <remarks>
    /// The IC-7300's own character for this, taken from the Full Manual's CW
    /// message contents table (p. 19-12): "^" transmits a string of characters
    /// with no inter-character space. So "^AR" is the prosign AR rather than the
    /// letters A and R, and writing fixtures in the notation the radio already
    /// uses means the transmit path will not need a second one.
    /// </remarks>
    public const char RunTogether = '^';

    private static readonly Dictionary<char, string> Table = new()
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..",
        ['E'] = ".", ['F'] = "..-.", ['G'] = "--.", ['H'] = "....",
        ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..",
        ['M'] = "--", ['N'] = "-.", ['O'] = "---", ['P'] = ".--.",
        ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-",
        ['U'] = "..-", ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-",
        ['Y'] = "-.--", ['Z'] = "--..",
        ['0'] = "-----", ['1'] = ".----", ['2'] = "..---", ['3'] = "...--",
        ['4'] = "....-", ['5'] = ".....", ['6'] = "-....", ['7'] = "--...",
        ['8'] = "---..", ['9'] = "----.",
        ['/'] = "-..-.", ['?'] = "..--..", [','] = "--..--", ['.'] = ".-.-.-",
        ['='] = "-...-", ['+'] = ".-.-.",
    };

    /// <summary>One dit's duration at a given speed.</summary>
    /// <param name="wordsPerMinute">Speed in WPM; values below 1 are treated as 1.</param>
    /// <returns>Duration of one dit.</returns>
    public static TimeSpan Dit(int wordsPerMinute)
        => TimeSpan.FromMilliseconds(
            DitMillisecondsAt1Wpm / Math.Max(1, wordsPerMinute));

    /// <summary>The dot-and-dash spelling of one character, or null.</summary>
    /// <param name="c">Character to look up; case-insensitive.</param>
    /// <returns>A string of '.' and '-', or null when unrepresentable.</returns>
    public static string? Spell(char c)
        => Table.TryGetValue(char.ToUpperInvariant(c), out var s) ? s : null;

    /// <summary>
    /// Expand text into a key-down/key-up pattern in dit units.
    /// </summary>
    /// <param name="text">Text to send.</param>
    /// <returns>
    /// Alternating durations in dits, starting with key-down. An entry is
    /// never zero, so the pattern is always alternating on/off.
    /// </returns>
    /// <remarks>
    /// Returned in dit units rather than milliseconds so the same pattern
    /// serves every speed — the caller multiplies. That is also what makes
    /// the WPM test exact: the pattern is integers, and the timing assertion
    /// is arithmetic rather than a measurement.
    /// </remarks>
    public static IReadOnlyList<int> KeyPattern(string text)
    {
        var pattern = new List<int>();
        var pendingGap = 0;
        var runTogether = false;
        var runStarted = false;

        foreach (var raw in text.Trim())
        {
            if (raw == RunTogether)
            {
                // Everything up to the next space is keyed as one symbol. This
                // is the radio's own convention rather than an invention: the
                // Full Manual lists "^" as the character that transmits a
                // string with no inter-character space (p. 19-12), which is
                // how a prosign such as AR or SK is actually sent.
                runTogether = true;
                runStarted = false;
                continue;
            }

            if (raw == ' ')
            {
                // A word gap is seven dits total, and three of them were
                // already owed as the character gap.
                pendingGap = 7;
                runTogether = false;
                continue;
            }

            var spelling = Spell(raw);
            if (spelling is null)
            {
                continue;
            }

            for (var i = 0; i < spelling.Length; i++)
            {
                if (pattern.Count > 0)
                {
                    // Gap before this element: the owed inter-character or
                    // word gap for the first element, one dit inside a
                    // character. Inside a run-together group even that first
                    // gap is one dit, which is what makes the whole group
                    // arrive as a single symbol.
                    var gap = i == 0 && !(runTogether && runStarted)
                        ? Math.Max(3, pendingGap)
                        : 1;

                    pattern.Add(gap);
                }

                pendingGap = 0;
                pattern.Add(spelling[i] == '-' ? 3 : 1);
            }

            runStarted = runTogether;
        }

        return pattern;
    }

    /// <summary>Total length of a message in dits, gaps included.</summary>
    /// <param name="text">Text to send.</param>
    /// <returns>Length in dit units.</returns>
    public static int LengthInDits(string text)
    {
        var pattern = KeyPattern(text);
        var total = 0;
        for (var i = 0; i < pattern.Count; i++)
        {
            total += pattern[i];
        }

        return total;
    }

    /// <summary>
    /// True when the key is down at a given offset into a message that
    /// repeats forever with a gap between repetitions.
    /// </summary>
    /// <param name="pattern">Key pattern from <see cref="KeyPattern"/>.</param>
    /// <param name="patternDits">Total dits in <paramref name="pattern"/>.</param>
    /// <param name="repeatGapDits">Silent dits between repetitions.</param>
    /// <param name="ditsElapsed">How many dits have passed.</param>
    /// <returns>True when the transmitter is keyed.</returns>
    public static bool IsKeyDown(
        IReadOnlyList<int> pattern, int patternDits, int repeatGapDits, double ditsElapsed)
    {
        var cycle = patternDits + Math.Max(0, repeatGapDits);
        if (cycle <= 0 || pattern.Count == 0)
        {
            return false;
        }

        var position = ditsElapsed % cycle;
        if (position < 0)
        {
            position += cycle;
        }

        var down = true;
        var cursor = 0.0;

        // Indexed rather than foreach. This runs once per CW signal per
        // frame, and iterating an IReadOnlyList<int> with foreach boxes the
        // underlying enumerator every call — a few hundred bytes a frame,
        // forever, on the one path HM-DEC-006 exists to keep allocation-free.
        for (var i = 0; i < pattern.Count; i++)
        {
            cursor += pattern[i];
            if (position < cursor)
            {
                return down;
            }

            down = !down;
        }

        // Past the end of the message: the gap before it starts again.
        return false;
    }

    /// <summary>A plausible CQ call for practice material.</summary>
    /// <param name="callsign">The calling station.</param>
    /// <returns>Text such as "CQ CQ DE KC3QIS KC3QIS K".</returns>
    public static string CqCall(string callsign)
    {
        var call = string.IsNullOrWhiteSpace(callsign) ? "W1AW" : callsign.Trim().ToUpperInvariant();
        var sb = new StringBuilder();
        sb.Append("CQ CQ DE ").Append(call).Append(' ').Append(call).Append(" K");
        return sb.ToString();
    }
}
