using System.Globalization;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The three parts of a standard FT8 message, and the closed list of payloads
/// Hamlet is willing to translate.
/// </summary>
/// <remarks>
/// <para>**HOVER TEXT ON COMMON RESPONSES ONLY, AND SILENCE EVERYWHERE ELSE**
/// (Tim's ruling, 2026-09-04, settling §12.1 for this surface). The vocabulary
/// fixed by the FT8 standard is a translation and Hamlet may show it. Anything
/// not on the list gets **no tooltip at all** - not a guess, not a partial
/// reading, not the word "unrecognised". Silence is the correct answer here in
/// the same way the `snr` dash is the correct answer there.</para>
/// <para>**IT INFERS NOTHING THE MESSAGE DOES NOT CONTAIN.** Not where a grid
/// is, not what a callsign prefix implies, not why somebody is calling. `EM66`
/// is "grid square: where he is" and never "Kentucky" - naming the place would
/// be Hamlet asserting a fact about a station from four characters, which is a
/// guess dressed as a decode (§0.0).</para>
/// <para>**ONE PLACE, BECAUSE TWO COPIES OF A VOCABULARY DRIFT.** The tooltip
/// reads this, and anything later that needs to know what `RR73` means reads
/// this.</para>
/// </remarks>
public static class Ft8Vocabulary
{
    /// <summary>What each field of a message is, or null where it has no parts.</summary>
    /// <param name="message">The text exactly as it was sent.</param>
    /// <returns>The three fields, or null where the message is not a standard one.</returns>
    /// <remarks>
    /// <para>**THE SPLIT IS STRUCTURE AND NOT MEANING.** A standard FT8 message
    /// is three fields with spaces between them, and saying which of them is the
    /// addressee is a fact about the format rather than an interpretation of the
    /// contents.</para>
    /// <para>**ANYTHING THAT IS NOT PLAINLY THREE FIELDS GETS NONE.** Free text,
    /// telemetry, contest exchanges and non-standard callsign forms come back
    /// null and are drawn as plain text with no colouring and no field tooltips.
    /// Guessing at the shape of a message this does not recognise would put a
    /// label on the wrong half of it.</para>
    /// <para>**THIS DERIVES WHAT THE DECODER ALREADY KNEW AND DID NOT PASS ON.**
    /// `Ft8Sharp.Ft8StandardMessage.TryUnpack` produces the three fields
    /// separately, and `Ft8Decode` carries only the joined string. Re-splitting
    /// it here is the view doing work the engine could hand over, and it is done
    /// this way because unit 241 may not change the engine. It is reported
    /// rather than worked around quietly.</para>
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

    /// <summary>What a payload means, or null where it is not on the list.</summary>
    /// <param name="fields">
    /// The message's own three fields, from <see cref="Split(string?)"/>.
    /// </param>
    /// <returns>One sentence, or null for silence.</returns>
    /// <remarks>
    /// <para>**IT TAKES THE WHOLE MESSAGE AND NOT THE PAYLOAD ALONE, SINCE UNIT
    /// 251** (Tim's ruling of 2026-09-05). The old signature could not do
    /// otherwise than word a report as being about the reader: given `R+14` and
    /// nothing else it said *roger, and hears you at 14 dB*. On
    /// `KE9COB N5CH R+14` the operator is not in the message at all — N5CH is
    /// reporting to KE9COB, two other stations, and the tooltip put the reader in
    /// the middle of somebody else's contact. **A sentence that names the wrong
    /// station is a decode presented wrongly, which §0.0 does not distinguish
    /// from a guess.**</para>
    /// <para>**THE OVERLOAD TAKING A BARE PAYLOAD IS GONE RATHER THAN KEPT
    /// BESIDE THIS ONE.** Leaving it would leave a way to get the you-worded
    /// sentence back, and a second door to a fault is how the fault returns.</para>
    /// <para>**THE TABLE IS STILL CLOSED** (Tim, 2026-09-04). Everything below is
    /// vocabulary the FT8 standard fixes; anything else gets no tooltip at all.
    /// Two shapes that are not standard FT8 are silent for that reason and are
    /// named where they are handled: a courtesy or a report addressed to `CQ`,
    /// which has no addressee to be a courtesy or a report to.</para>
    /// <para>**NO PRONOUN CHOOSES A GENDER.** The old wording said *where he is*
    /// about a callsign, which Hamlet has no way to know. Every sentence here
    /// names the stations and uses `they`.</para>
    /// </remarks>
    public static string? Explain(Ft8MessageFields? fields)
    {
        if (fields is null)
        {
            return null;
        }

        var to = fields.To.Trim();
        var from = fields.From.Trim();
        var text = fields.Payload.Trim();

        if (to.Length == 0 || from.Length == 0 || text.Length == 0)
        {
            return null;
        }

        // **A SENDER THAT IS ITSELF A CALL TO ANYONE IS NOT A STATION**, so
        // there is nobody to build a sentence around. It comes off a message
        // shape that is not standard FT8 and it gets the same silence everything
        // else off the list gets.
        if (IsCallToAnyone(from))
        {
            return null;
        }

        var callingAnyone = IsCallToAnyone(to);

        switch (text.ToUpperInvariant())
        {
            case "CQ":
                return $"{from} is calling anyone.";

            case "CQ DX":
                return $"{from} is calling anyone, and is looking for distance.";

            case "RRR":
                return callingAnyone
                    ? null
                    : $"{from} is telling {to} that everything came through.";

            case "RR73":
                return callingAnyone
                    ? null
                    : $"{from} is telling {to} that everything came through, and "
                      + "is signing off with best regards.";

            case "73":
                return callingAnyone
                    ? null
                    : $"{from} is signing off to {to} with best regards.";
        }

        if (IsGrid(text))
        {
            // **WHICH SQUARE, AND NEVER WHERE THAT IS.** Turning a grid into a
            // place name is the one inference this table is most tempted into
            // and the one it must not make. The sentence does not even repeat
            // the four characters, so there is nothing in it that could grow a
            // country on the end.
            return callingAnyone
                ? $"{from} is calling anyone, and saying which grid square they "
                  + "are transmitting from."
                : $"{from} is telling {to} which grid square they are "
                  + "transmitting from.";
        }

        if (IsReport(text, out var rogered, out var decibels))
        {
            // **A REPORT NEEDS SOMEBODY TO BE ABOUT.** `CQ` is not a station and
            // a report addressed to it is not a message this table has a reading
            // for.
            if (callingAnyone)
            {
                return null;
            }

            var signed = decibels.ToString("+0;-0;+0", CultureInfo.InvariantCulture);

            // **THE ROGER AND THE REPORT ARE BOTH SAID**, because `R+14` carries
            // both and a sentence naming only the number would drop the half of
            // it that says the previous transmission arrived.
            return rogered
                ? $"{from} has {to}'s message, and is answering with a report of "
                  + $"its own: {from} hears {to} at {signed} dB."
                : $"{from} hears {to} at {signed} dB, and is sending that back "
                  + "as the signal report.";
        }

        // **NOT A FALLBACK STRING.** Contest exchanges, QRZ, free text, compound
        // and non-standard callsigns: nothing appears.
        return null;
    }

    /// <summary>Whether an addressee field is a call to anyone rather than a station.</summary>
    /// <remarks>
    /// `CQ`, and `CQ DX` or `CQ EU` — which <see cref="Split(string?)"/> already
    /// joins into one addressee, because the call and its direction are one field
    /// in two words.
    /// </remarks>
    private static bool IsCallToAnyone(string field)
        => string.Equals(field, "CQ", StringComparison.OrdinalIgnoreCase)
           || field.StartsWith("CQ ", StringComparison.OrdinalIgnoreCase);

    /// <summary>A four-character Maidenhead field, as FT8 sends it.</summary>
    /// <remarks>
    /// Two letters A to R, then two digits. `RR73` is deliberately excluded
    /// above by being matched first: it is letters then digits and would
    /// otherwise read as a grid square.
    /// </remarks>
    private static bool IsGrid(string text)
        => text.Length == 4
            && text[0] is >= 'A' and <= 'R'
            && text[1] is >= 'A' and <= 'R'
            && char.IsAsciiDigit(text[2])
            && char.IsAsciiDigit(text[3]);

    /// <summary>A signal report, with or without its roger.</summary>
    private static bool IsReport(string text, out bool rogered, out int decibels)
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

/// <summary>The three fields of a standard message.</summary>
/// <param name="To">Who it is addressed to, which may be a call for anyone.</param>
/// <param name="From">Who sent it.</param>
/// <param name="Payload">The rest: a grid, a report, or a courtesy.</param>
public sealed record Ft8MessageFields(string To, string From, string Payload);
