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
    /// <param name="payload">The third field of the message.</param>
    /// <returns>One sentence, or null for silence.</returns>
    public static string? Explain(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var text = payload.Trim();

        switch (text.ToUpperInvariant())
        {
            case "CQ":
                return "calling anyone";

            case "CQ DX":
                return "calling anyone, looking for distance";

            case "RRR":
                return "roger, everything received";

            case "RR73":
                return "roger, and best regards";

            case "73":
                return "best regards, contact finished";
        }

        if (IsGrid(text))
        {
            // **WHERE HE IS, AND NEVER WHERE THAT IS.** Turning a grid into a
            // place name is the one inference this table is most tempted into
            // and the one it must not make.
            return "grid square: where he is";
        }

        if (IsReport(text, out var rogered, out var decibels))
        {
            return rogered
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "roger, and hears you at {0} dB",
                    decibels)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "signal report: hears you at {0} dB",
                    decibels);
        }

        // **NOT A FALLBACK STRING.** Contest exchanges, QRZ, free text, compound
        // and non-standard callsigns: nothing appears.
        return null;
    }

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
