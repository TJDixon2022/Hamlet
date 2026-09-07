using System.Globalization;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

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
    /// <para>**THE RULES THEMSELVES LIVE IN THE ENGINE SINCE UNIT 258** and this
    /// is a one-line forward. The contact ledger divides messages the same way
    /// and the engine may not reach the app (§0.1), so the division came down to
    /// `Hamlet.RadioEngine.Contacts.Ft8MessageSplit` rather than being written
    /// twice. **Every caller here keeps calling this**; the answer is the same
    /// object it always was, and there is one set of parsing rules in this
    /// repository rather than two.</para>
    /// </remarks>
    public static Ft8MessageFields? Split(string? message)
        => Ft8MessageSplit.Split(message);

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
    /// <para>**THE STATIONS ARE NAMED, AND THE PRONOUN IS `he`** (Tim's ruling,
    /// 2026-09-07, HM-DEC-159). This file carried a rule from unit 251 that no
    /// pronoun chooses a gender, and unit 271 followed it and wrote `they`; he
    /// ruled the other way and the rule came out rather than gaining an exception,
    /// because a file that states a rule its own code breaks is worse than either
    /// answer. See `DECISIONS.md` for what that costs and why he weighed it that
    /// way. Every sentence still names the stations, which is what carries the
    /// meaning; the pronoun only stands in for a callsign already said.</para>
    /// </remarks>
    /// <param name="observerGrid">
    /// The operator's own Maidenhead locator from Settings, or "" where he has
    /// not set one. Used only to measure a distance from, never to name a place.
    /// </param>
    public static string? Explain(Ft8MessageFields? fields, string? observerGrid = null)
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
            // **NO PLACE BELOW THE COUNTRY** (Tim's rulings, 2026-09-05 and
            // 2026-09-07: a grid is roughly 70 by 100 miles, so *Texas* is fair
            // and *Houston* is a lie). Unit 252 added arithmetic on the square;
            // unit 271 adds the country, and it takes it from the CALLSIGN rather
            // than from the square, because a square straddles borders and a
            // prefix does not.
            //
            // **THE FOUR CHARACTERS ARE NOW SAID, WHERE UNIT 241 WITHHELD THEM.**
            // That was a good instinct against a sentence growing a country on the
            // end, and it is no longer the right guard, because the sentence now
            // carries a distance and the operator has to be able to see which
            // square it was measured from. The guard that replaces it is a test
            // sweeping every output for a place name.
            // **THE DISTANCE BELONGS TO THE STATION, NOT TO THE SQUARE** (Tim,
            // 2026-09-07). It read `IK4LZH is calling anyone from grid JN54,
            // which is 4,400 miles away from you` — and *which* attaches to the
            // grid, so the sentence says the square is four thousand miles away
            // and leaves open where IK4LZH is calling from. He is in it. Two
            // sentences: what he is doing, then where he is.
            var grid = text.ToUpperInvariant();

            var doing = callingAnyone
                ? $"{from} is calling anyone."
                : $"{from} is telling {to} where he is transmitting from.";

            return doing + " " + WhereTheyAre(from, grid, observerGrid);
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

    /// <summary>Where a station is: the country, the square, and how far.</summary>
    /// <param name="sender">The station's callsign, which is what names the country.</param>
    /// <param name="grid">The station's four-character locator, upper case.</param>
    /// <param name="observerGrid">The operator's own locator, or "" / null.</param>
    /// <returns>One sentence, capitalised, with its full stop.</returns>
    /// <remarks>
    /// <para>**THE COUNTRY COMES FROM THE CALLSIGN AND NEVER FROM THE GRID**
    /// (Tim, 2026-09-07). A grid square is about 70 by 100 miles and straddles
    /// borders, and `W4/YV7AXM` is in the United States whatever his grid says.
    /// **Where the two disagree the callsign wins**, and where `DxccPrefixes`
    /// declines — a shared prefix, a station at sea — no country is named at all.
    /// Tim's ruling of 2026-09-06 is untouched: certain, or silent.</para>
    /// <para>**THE GRID SUPPLIES ONLY A LATITUDE**, which places the station
    /// inside a country the callsign already settled, and that is arithmetic on a
    /// value the message itself carries rather than a lookup of any kind
    /// (§12.1).</para>
    /// <para>**`He`, BY TIM'S RULING OF 2026-09-07** (HM-DEC-159). Unit 271
    /// raised it as an ask and wrote `they` meanwhile, under a rule this file used
    /// to state; he ruled `He` and the rule came out with the same change. The
    /// callsign is always said first, so the pronoun stands in for a station the
    /// reader has already been given by name.</para>
    /// <para>**IT IS ONE SENTENCE AND IT CARRIES ONE DASH AT MOST** (§0.7,
    /// HM-DEC-040) — it carries none.</para>
    /// </remarks>
    private static string WhereTheyAre(
        string sender, string grid, string? observerGrid)
    {
        var entity = DxccPrefixes.EntityOf(sender);

        // **WITH NO COUNTRY THE COMMA GOES TOO.** Built as one clause with an
        // optional country in front, `He is, in grid QG44` came out for every
        // station the table declines, which is a stumble in the middle of the
        // sentence and reads as though a word had gone missing. It had.
        var place = entity is null
            ? $"in grid {grid}"
            : $"in {EntityQualifier.DescribeFromGrid(entity, grid)}, in grid {grid}";

        var mine = OperatorLocation.FromGrid(observerGrid);
        var theirs = OperatorLocation.FromGrid(grid);

        if (mine is not { } here || theirs is not { } there)
        {
            return $"He is {place}, and Hamlet needs your own grid square in "
                   + "Settings before it can say how far away that is.";
        }

        var miles = GridPath.DescribeMiles(GridPath.MilesBetween(here, there));
        var bearing = GridPath.DescribeBearing(GridPath.BearingDegrees(here, there));

        return $"He is {place}, {miles} away on a bearing of {bearing}.";
    }

    /// <summary>Whether an addressee field is a call to anyone rather than a station.</summary>
    /// <remarks>
    /// `CQ`, and `CQ DX` or `CQ EU` — which <see cref="Split(string?)"/> already
    /// joins into one addressee, because the call and its direction are one field
    /// in two words.
    /// </remarks>
    private static bool IsCallToAnyone(string field)
        => Ft8MessageSplit.IsCallToAnyone(field);

    /// <summary>A four-character Maidenhead field, as FT8 sends it.</summary>
    /// <remarks>
    /// Two letters A to R, then two digits. `RR73` is deliberately excluded
    /// above by being matched first: it is letters then digits and would
    /// otherwise read as a grid square.
    /// </remarks>
    private static bool IsGrid(string text)
        => Ft8MessageSplit.IsGrid(text);

    /// <summary>A signal report, with or without its roger.</summary>
    /// <remarks>
    /// **The engine's rule since unit 258**, so that the ledger and the tooltip
    /// agree about what `R-09` is without either of them owning a second copy.
    /// </remarks>
    private static bool IsReport(string text, out bool rogered, out int decibels)
        => Ft8MessageSplit.IsReport(text, out rogered, out decibels);
}
