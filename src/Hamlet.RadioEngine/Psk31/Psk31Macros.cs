namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **The four texts of `PHASE_PLAN.md` §R2, filled from strings handed in.**
/// </summary>
/// <remarks>
/// <para>**THE WHOLE VOCABULARY, AND NOTHING CHATTY** (§R2). A call, an answer, a report and
/// a confirmation, in the standard minimum form, with §R2's spacing exactly - the double
/// spaces between the report's parts are part of the ruling's text and are kept. Free typing
/// is not in this phase.</para>
/// <para>**EVERY CALLSIGN, NAME, PLACE AND GRID IS HANDED IN** (§0.1). The engine is never
/// told Settings exist; the application reads them and passes strings. **No callsign is
/// written into this file**, and a test reads it to say so.</para>
/// <para>**THE GRID IS FOUR CHARACTERS, AS §R2 WRITES IT.** Settings may hold six; the first
/// four are sent. That is work instruction 318's reading of §R2, not a ruling of this
/// type's own.</para>
/// <para>**A BLANK IS A REFUSAL, NOT A GAP** (§0.0). A report reading `Name  ` would be a
/// transmission saying something other than what the operator's card says it says, so an
/// empty field, a line break inside one, or a character the varicode cannot carry is
/// refused with the field named, before any audio exists.</para>
/// </remarks>
public static class Psk31Macros
{
    /// <summary>How many characters of the grid square the report carries.</summary>
    public const int GridCharacters = 4;

    /// <summary>The call to anyone.</summary>
    /// <param name="mine">The operator's callsign.</param>
    /// <returns>The text, exactly as it goes on the air.</returns>
    /// <exception cref="ArgumentException">The callsign is blank or cannot be sent.</exception>
    public static string Cq(string mine)
    {
        var me = Field(mine, nameof(mine), "your callsign");

        return $"CQ CQ CQ de {me} {me} {me} pse K";
    }

    /// <summary>
    /// **PSK31's stated equivalent of one FT8 slot: how long the answer Hamlet
    /// would send back takes on the air.**
    /// </summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <returns>Seconds.</returns>
    /// <exception cref="ArgumentException">A callsign is blank or cannot be sent.</exception>
    /// <remarks>
    /// <para>**§R18 ASKS FOR A STATED EQUIVALENT AND THIS IS IT** (work
    /// instruction 325 task 3). PSK31 has no slots, so *one full slot in which he
    /// could have answered* has to be given some length, and the honest length is
    /// **the one Hamlet itself would take to say the same thing** - the `Answer`
    /// macro, encoded through <see cref="Psk31Modulator.SecondsFor"/>. It is
    /// measured off the bits that would go on the air rather than guessed from a
    /// character count, which varicode makes meaningless.</para>
    /// <para>**IT DEPENDS ON THE TWO CALLSIGNS, AND THAT IS CORRECT.** The macro
    /// carries both, so a pair of long calls genuinely takes longer to send than a
    /// pair of short ones, and a fixed figure would be wrong for one of them.</para>
    /// <para>**THE AUTHOR'S SUGGESTION, AND TIM'S TO OVERRULE.** Nothing in this
    /// repository states a PSK31 turnaround; the work instruction says the unit
    /// states it and names this shape, and the report carries the number.</para>
    /// </remarks>
    public static double AnswerSeconds(string his, string mine)
        => Psk31Modulator.SecondsFor(Answer(his, mine));

    /// <summary>The answer to a station calling.</summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <returns>The text, exactly as it goes on the air.</returns>
    /// <exception cref="ArgumentException">A callsign is blank or cannot be sent.</exception>
    public static string Answer(string his, string mine)
    {
        var other = Field(his, nameof(his), "the other station's callsign");
        var me = Field(mine, nameof(mine), "your callsign");

        return $"{other} de {me} {me} K";
    }

    /// <summary>The report: signal, name, place and grid, then the turn handed back.</summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <param name="name">The operator's name.</param>
    /// <param name="qth">Where the operator is.</param>
    /// <param name="grid">The operator's grid square, four characters or more.</param>
    /// <returns>The text, exactly as it goes on the air.</returns>
    /// <exception cref="ArgumentException">A field is blank, too short, or cannot be sent.</exception>
    public static string Report(string his, string mine, string name, string qth, string grid)
    {
        var other = Field(his, nameof(his), "the other station's callsign");
        var me = Field(mine, nameof(mine), "your callsign");
        var who = Field(name, nameof(name), "your name");
        var where = Field(qth, nameof(qth), "your location");
        var square = Field(grid, nameof(grid), "your grid square");

        if (square.Length < GridCharacters)
        {
            throw new ArgumentException(
                $"your grid square is \"{square}\", and the report sends the first {GridCharacters} "
                + "characters of one.",
                nameof(grid));
        }

        square = square[..GridCharacters];

        return $"{other} de {me}  RST 599 599  Name {who} {who}  QTH {where}  "
            + $"Grid {square} {square}  BTU {other} de {me} K";
    }

    /// <summary>The confirmation that closes the contact.</summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <returns>The text, exactly as it goes on the air.</returns>
    /// <exception cref="ArgumentException">A callsign is blank or cannot be sent.</exception>
    public static string Confirm(string his, string mine)
    {
        var other = Field(his, nameof(his), "the other station's callsign");
        var me = Field(mine, nameof(mine), "your callsign");

        return $"{other} de {me}  R R  TNX for the QSO  73 73  {other} de {me} SK";
    }

    /// <summary>What Hamlet adds around a line the operator typed.</summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <param name="text">What he typed, already cleaned by <see cref="Sendable"/>.</param>
    /// <returns>The text, exactly as it goes on the air.</returns>
    /// <exception cref="ArgumentException">A callsign is blank or cannot be sent, or the line is empty.</exception>
    /// <remarks>
    /// <para>**R29, AND THE FRAME IS THE WHOLE OF WHAT HAMLET ADDS.** Tim's words go out as
    /// he typed them, trimmed, with his callsign and the other station's in front and the
    /// hand-back behind. **Those two are the things a beginner forgets on a keyboard mode**,
    /// and they are not his to remember: a line with no callsigns identifies nobody, and a
    /// line with no turnover leaves the other operator waiting for a hand-back that never
    /// comes.</para>
    /// <para>**NOTHING ELSE IS ADDED, AND NOTHING IS REWORDED** (§0.0). What he sees on the
    /// card is what goes on the air.</para>
    /// </remarks>
    public static string Typed(string his, string mine, string text)
    {
        var other = Field(his, nameof(his), "the other station's callsign");
        var me = Field(mine, nameof(mine), "your callsign");
        var said = Field(text, nameof(text), "your line");

        return $"{other} de {me}  {said}  BTU {other} de {me} K";
    }

    /// <summary>How long a typed line's framed text takes on the air, the announcement in front not counted.</summary>
    /// <param name="his">The other station's callsign.</param>
    /// <param name="mine">The operator's callsign.</param>
    /// <param name="text">What he typed.</param>
    /// <returns>Seconds, including the idle either side and not the RSID burst.</returns>
    /// <remarks>
    /// **THE CAP MEASURES THE TEXT, SO THIS DOES TOO** (`PHASE_PLAN.md` R32 (a); work instruction
    /// 360, the arbiter's decision E): the card's *too long to send* and the sequence's cap agree.
    /// </remarks>
    public static double TypedSeconds(string his, string mine, string text)
        => Psk31Modulator.SecondsFor(Typed(his, mine, text));

    /// <summary>What of a typed line PSK31 can send as itself, and how much was dropped.</summary>
    /// <param name="text">What the operator typed.</param>
    /// <returns>The sendable text, trimmed, and how many characters were dropped.</returns>
    /// <remarks>
    /// <para>**DROPPED, NOT REFUSED, AND THE CARD SAYS HOW MANY** (work instruction 357
    /// task 2). A curly quote from a paste, an accented letter, an emoji: the varicode
    /// carries the 256 Latin-1 bytes and nothing else, so a character outside them would go
    /// out as something other than itself, which is §0.0 broken on the air rather than on
    /// the screen. Refusing the whole line over one pasted character would be worse: he
    /// would not know which one.</para>
    /// <para>**A CONTROL CHARACTER GOES TOO**, including the tab and the newline a paste
    /// brings with it, because the line is one line.</para>
    /// </remarks>
    public static (string Text, int Dropped) Sendable(string? text)
    {
        var kept = new System.Text.StringBuilder();
        var dropped = 0;

        foreach (var character in text ?? string.Empty)
        {
            if (character > (char)0xFF || char.IsControl(character))
            {
                dropped++;
                continue;
            }

            kept.Append(character);
        }

        return (kept.ToString().Trim(), dropped);
    }

    /// <summary>One field, trimmed, or a refusal naming it.</summary>
    /// <param name="value">What was handed in.</param>
    /// <param name="parameter">The parameter it came in on.</param>
    /// <param name="words">What it is, in the operator's words.</param>
    /// <returns>The trimmed value.</returns>
    private static string Field(string? value, string parameter, string words)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            throw new ArgumentException(
                $"{words} is empty, and a macro is not sent with a blank in it.", parameter);
        }

        foreach (var character in trimmed)
        {
            // THE VARICODE CARRIES THE 256 LATIN-1 BYTES AND NOTHING ELSE. A character
            // outside them would be sent as something other than itself.
            if (character > (char)0xFF || char.IsControl(character))
            {
                throw new ArgumentException(
                    $"{words} contains a character PSK31 cannot send as itself (U+{(int)character:X4}).",
                    parameter);
            }
        }

        return trimmed;
    }
}
