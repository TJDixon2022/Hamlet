using System.Globalization;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>The three fields of a standard message.</summary>
/// <param name="To">Who it is addressed to, which may be a call for anyone.</param>
/// <param name="From">Who sent it.</param>
/// <param name="Payload">The rest: a grid, a report, or a courtesy.</param>
public sealed record Ft8MessageFields(string To, string From, string Payload);

/// <summary>
/// How a standard FT8 message divides into fields, and the shapes those fields
/// come in.
/// </summary>
/// <remarks>
/// <para>**THIS IS STRUCTURE AND NOT MEANING.** A standard FT8 message is three
/// fields with spaces between them, and saying which of them is the addressee is
/// a fact about the format rather than an interpretation of the contents. What a
/// payload *means* is `Ft8Vocabulary.Explain`'s closed table, it stays in the
/// app, and Tim closed it on 2026-09-04.</para>
/// <para>**IT MOVED HERE SO THERE IS ONE SET OF PARSING RULES AND NOT TWO.** It
/// lived in `Hamlet.App.ViewModels.Ft8Vocabulary` until unit 258, where it was
/// the view doing work the engine could hand over
/// (`Ft8Vocabulary.Split`'s own remark). The contact ledger needs the same
/// division and the engine may not reach the app (§0.1), so the rules came down
/// and the app's `Split` became a one-line forward. **Do not write a second
/// copy in either direction.**</para>
/// <para>**IT VALIDATES NOTHING.** It does not check that a callsign is real,
/// that a grid exists or that a report is plausible. It counts fields and
/// applies one special case. That is what a ledger needs - who addressed whom,
/// not whether the callsign was on the air.</para>
/// <para>**THE ENGINE STILL DOES NOT KNOW A TAB EXISTS** (§0.1). Nothing here
/// reads settings, names a view or touches a window; the whole file is a string
/// going in and fields coming out.</para>
/// </remarks>
public static class Ft8MessageSplit
{
    /// <summary>What each field of a message is, or null where it has no parts.</summary>
    /// <param name="message">The text exactly as it was sent.</param>
    /// <returns>The three fields, or null where the message is not a standard one.</returns>
    /// <remarks>
    /// <para>**ANYTHING THAT IS NOT PLAINLY THREE FIELDS GETS NONE.** Free text,
    /// telemetry, contest exchanges and non-standard callsign forms come back
    /// null. Guessing at the shape of a message this does not recognise would put
    /// a label on the wrong half of it.</para>
    /// <para>**A THREE-WORD FREE-TEXT MESSAGE IS ACCEPTED AND THAT IS CORRECT.**
    /// `HW CPY OM` splits into three fields and a caller will book a station
    /// called `CPY` addressing one called `HW`. Reading further to notice that
    /// those are not callsigns would be interpreting the message (§12.1). The
    /// shapes this actually refuses are one, two, and four-or-more words where
    /// the first is not `CQ`.</para>
    /// </remarks>
    public static Ft8MessageFields? Split(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var parts = message.Trim().Split(
            ' ', StringSplitOptions.RemoveEmptyEntries);

        // `CQ DX W1ABC FN42` and `CQ EU W1ABC FN42`: the call and its direction
        // are one addressee in two words.
        if (parts.Length == 4
            && string.Equals(parts[0], "CQ", StringComparison.Ordinal))
        {
            return new Ft8MessageFields(
                parts[0] + " " + parts[1], parts[2], parts[3]);
        }

        return parts.Length == 3
            ? new Ft8MessageFields(parts[0], parts[1], parts[2])
            : null;
    }

    /// <summary>Whether an addressee field is a call to anyone rather than a station.</summary>
    /// <param name="to">The to-field.</param>
    /// <returns>True when it is a call to anyone.</returns>
    /// <remarks>
    /// `CQ`, and `CQ DX` or `CQ POTA` - which <see cref="Split(string?)"/>
    /// already joins into one addressee, because the call and its direction are
    /// one field in two words.
    /// **`CQ` IS AN ADDRESSEE AND NOT A STATION**, so nothing may book it as one.
    /// </remarks>
    public static bool IsCallToAnyone(string? to)
    {
        var text = to?.Trim() ?? "";

        return string.Equals(text, "CQ", StringComparison.OrdinalIgnoreCase)
               || text.StartsWith("CQ ", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Whether a message field names the same station as a callsign.</summary>
    /// <param name="field">The to-field or from-field.</param>
    /// <param name="callsign">The callsign to compare it with.</param>
    /// <returns>True when they are the same station.</returns>
    /// <remarks>
    /// <para>**COMPOUND AND PORTABLE FORMS ARE THE SAME STATION.** If he is
    /// `W1ABC` then `W1ABC/P`, `W1ABC/QRP` and `W4/W1ABC` are all him, and a rule
    /// that says otherwise says his own portable operation is somebody else's.</para>
    /// <para>**THE RULE IS: STRIP THE SLASHES AND SEE IF THE CALL IS ONE OF THE
    /// PIECES.** FT8 puts the prefix or suffix on the other side of a `/`, so a
    /// compound call is a base call plus one more piece and never a base call with
    /// letters welded onto it. That is why this splits rather than matching a
    /// prefix: a prefix test would make `W1ABCD` the same station as `W1ABC`, and
    /// `W1ABCD` is somebody else entirely.</para>
    /// <para>**AND IT WORKS BOTH WAYS ROUND**, because he may type `W4/W1ABC` into
    /// Settings while operating away from home.</para>
    /// <para>**IT LIVES HERE BECAUSE IT IS A FACT ABOUT CALLSIGNS AND NOT ABOUT A
    /// LIST CONTROL** (§0.1). It was written in `DecodedFilterRule` for the decoded
    /// list's `mine` toggle; work instruction 271 task 4 needed the same question
    /// asked about the contact column, and a second copy of a callsign rule is a
    /// second answer waiting to disagree (§0). `DecodedFilterRule.IsSameStation`
    /// now calls this and there is one implementation.</para>
    /// </remarks>
    public static bool IsSameStation(string? field, string? callsign)
    {
        var them = field?.Trim() ?? "";
        var us = callsign?.Trim() ?? "";

        if (them.Length == 0 || us.Length == 0)
        {
            return false;
        }

        if (string.Equals(them, us, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(
            BaseCall(them), BaseCall(us), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The callsign inside a compound call.</summary>
    /// <param name="call">A call, compound or not.</param>
    /// <returns>The longest slash-separated piece, which is the call itself.</returns>
    /// <remarks>
    /// **THE LONGEST PIECE AND NOT THE FIRST OR THE LAST**, because FT8 puts the
    /// added piece on either side: `W4/W1ABC` is a prefix and `W1ABC/P` is a
    /// suffix, and no rule about position covers both. A prefix or a suffix is
    /// short - a region, a country, `P`, `M`, `QRP` - and the callsign is the long
    /// one. Ties keep the first piece, which is the only case this cannot settle
    /// and is not a case any real call produces.
    /// </remarks>
    private static string BaseCall(string call)
    {
        var pieces = call.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length <= 1)
        {
            return call;
        }

        var longest = pieces[0];

        foreach (var piece in pieces)
        {
            if (piece.Length > longest.Length)
            {
                longest = piece;
            }
        }

        return longest;
    }

    /// <summary>A four-character Maidenhead field, as FT8 sends it.</summary>
    /// <param name="text">The payload field.</param>
    /// <returns>True when it has a grid square's shape.</returns>
    /// <remarks>
    /// Two letters A to R, then two digits. **`RR73` is letters then digits and
    /// would otherwise read as a grid square**, so every caller tests for the
    /// courtesies first - `Ft8Vocabulary.Explain` does, and so does the ledger.
    /// </remarks>
    public static bool IsGrid(string text)
        => text.Length == 4
            && text[0] is >= 'A' and <= 'R'
            && text[1] is >= 'A' and <= 'R'
            && char.IsAsciiDigit(text[2])
            && char.IsAsciiDigit(text[3]);

    /// <summary>A signal report, with or without its roger.</summary>
    /// <param name="text">The payload field.</param>
    /// <param name="rogered">Set true where the report carries a leading `R`.</param>
    /// <param name="decibels">The reported level, signed.</param>
    /// <returns>True when the field is a report.</returns>
    public static bool IsReport(string text, out bool rogered, out int decibels)
    {
        rogered = false;
        decibels = 0;

        var body = text;

        if (body.StartsWith('R') && body.Length > 1)
        {
            rogered = true;
            body = body[1..];
        }

        // FT8 reports always carry their sign, which is what separates `-05`
        // from a serial number in a contest exchange.
        if (body.Length < 2 || (body[0] != '+' && body[0] != '-'))
        {
            return false;
        }

        return int.TryParse(
            body, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
            out decibels);
    }
}
