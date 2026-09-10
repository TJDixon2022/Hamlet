using System.Collections.Concurrent;
using System.Globalization;

namespace Hamlet.RadioEngine.Licensing;

/// <summary>
/// **Who a US station is and what town he is in, asked once per callsign.**
/// </summary>
/// <remarks>
/// <para>**THREE UNITS HAVE WANTED THIS AND CALLOOK IS THE FIRST THING THAT CAN GIVE
/// IT** (work instruction 302, and the ask units 297, 298 and 299 each raised). The
/// DXCC entity for all fifty states is *United States of America*, a call area is
/// historical rather than a residence, and a four-character grid is a box seventy
/// miles across that straddles state lines. **Nothing Hamlet held could say
/// Arizona.**</para>
/// <para>**THE EXPOSURE IS A NAME ON A CARD** (§0.0). A wrong operator's name beside
/// a callsign is a claim he will repeat to the person he is working, so every field
/// here is taken verbatim from what the service returned or is left empty. **Nothing
/// is inferred, nothing is hedged, and there is no *unknown*** - Tim's rule of
/// 2026-09-08: say nothing if we do not know.</para>
/// <para>**IT IS US-ONLY AND THAT IS CORRECT BEHAVIOUR RATHER THAN A SHORTFALL.**
/// Measured against the live service on 2026-09-10: `VP2MAA` returns
/// `{"status": "INVALID"}` and nothing else. Most of what makes FT8 interesting
/// resolves to nothing here, and those cards read exactly as they always did.</para>
/// <para>**ONE LOOKUP PER CALLSIGN, NEVER PER DECODE.** Fourteen messages a slot and
/// four slots a minute is a great many requests for a station he sees repeatedly, so
/// a callsign is asked about once and the answer is kept - **including the answer
/// that there is nothing to know**, which is what stops a non-US callsign being
/// asked about again every slot.</para>
/// <para>**IT FAILS QUIETLY.** Offline, timed out or refused, this hands back
/// nothing and the card reads as it always did. A transport failure is not cached,
/// because *the network was down* is not an answer about a callsign.</para>
/// <para>**NOTHING HERE IS WRITTEN TO TELEMETRY OR TO DISK** (§2.1, HM-DEC-018). A
/// callsign is personal data; the only thing that leaves the machine is the lookup
/// itself, which is the same request Settings has made since 2026-08-14.</para>
/// </remarks>
public sealed class StationDirectory
{
    private readonly ICallsignLookup _lookup;

    private readonly ConcurrentDictionary<string, StationName> _known = new(
        StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, byte> _asking = new(
        StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates the directory over a lookup.</summary>
    /// <param name="lookup">The callsign lookup, which is not created here.</param>
    /// <exception cref="ArgumentNullException">There is no lookup.</exception>
    /// <remarks>
    /// **THE CLIENT IS HANDED IN AND NOT BUILT** (the instruction: do not write a
    /// second callook client). This is a cache and a parser over the one Settings
    /// already uses.
    /// </remarks>
    public StationDirectory(ICallsignLookup lookup)
    {
        ArgumentNullException.ThrowIfNull(lookup);

        _lookup = lookup;
    }

    /// <summary>Raised when a callsign's name arrives, so a card can redraw.</summary>
    public event EventHandler<StationName>? Learned;

    /// <summary>How many callsigns have been asked about.</summary>
    /// <remarks>**FOR THE COST REPORT** and for a test to prove one ask per call.</remarks>
    public int Asked => _known.Count;

    /// <summary>What is known about a callsign right now, without asking.</summary>
    /// <param name="callsign">The callsign.</param>
    /// <returns>What is known, which may be that there is nothing to know.</returns>
    /// <remarks>
    /// **THIS NEVER WAITS AND NEVER CALLS OUT.** A card is composed on the UI thread
    /// while decodes are arriving; a lookup that blocked it would stutter the one
    /// screen the operator times his transmission by.
    /// </remarks>
    public StationName Known(string? callsign)
    {
        var call = Normalize(callsign);

        return call.Length > 0 && _known.TryGetValue(call, out var found)
            ? found
            : StationName.Nothing;
    }

    /// <summary>Ask about a callsign, at most once, and never twice at a time.</summary>
    /// <param name="callsign">The callsign.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Nothing; the answer arrives through <see cref="Learned"/>.</returns>
    public async Task AskAboutAsync(
        string? callsign, CancellationToken cancellationToken = default)
    {
        var call = Normalize(callsign);

        if (call.Length == 0 || _known.ContainsKey(call))
        {
            return;
        }

        // **ONE IN FLIGHT PER CALLSIGN.** Fourteen rows in one slot can all name the
        // same station, and without this every one of them would start a request.
        if (!_asking.TryAdd(call, 0))
        {
            return;
        }

        try
        {
            var result = await _lookup.LookupAsync(call, cancellationToken)
                .ConfigureAwait(false);

            // **A SERVICE THAT ANSWERED AND KNOWS NOTHING IS AN ANSWER**, and it is
            // cached so a non-US callsign is asked about once rather than for ever.
            var name = result is null
                ? StationName.NothingFor(call)
                : StationName.From(call, result);

            _known[call] = name;

            if (name.HasAnything)
            {
                Learned?.Invoke(this, name);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // **UNREACHABLE IS A CONDITION AND NOT AN ERROR**, and it is deliberately
            // not cached: the network being down says nothing about this callsign, so
            // asking again later is right where remembering a failure would be wrong.
            // The card shows exactly what it showed before.
        }
        finally
        {
            _asking.TryRemove(call, out _);
        }
    }

    private static string Normalize(string? callsign)
        => (callsign ?? "").Trim().ToUpperInvariant();
}

/// <summary>What is known about one station's operator.</summary>
/// <param name="Callsign">The callsign this is about.</param>
/// <param name="Name">His first name, in ordinary case, or empty.</param>
/// <param name="Town">His town and state, `Sun City AZ`, or empty.</param>
/// <param name="State">The two-letter state on its own, or empty.</param>
public sealed record StationName(
    string Callsign, string Name, string Town, string State)
{
    /// <summary>Nothing is known, about nobody.</summary>
    public static readonly StationName Nothing = new("", "", "", "");

    /// <summary>True where there is anything at all to show.</summary>
    public bool HasAnything => Name.Length > 0 || Town.Length > 0;

    /// <summary>Nothing is known about this callsign, and that is settled.</summary>
    /// <param name="callsign">The callsign asked about.</param>
    /// <returns>An empty answer that is still an answer.</returns>
    public static StationName NothingFor(string callsign)
        => new(callsign, "", "", "");

    /// <summary>What a lookup found, read conservatively.</summary>
    /// <param name="callsign">The callsign asked about.</param>
    /// <param name="result">What the service returned.</param>
    /// <returns>The name and town, or empties where they cannot be read cleanly.</returns>
    public static StationName From(string callsign, CallsignLookupResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new StationName(
            callsign,
            FirstName(result.LicenseeName),
            TownAndState(result.TownLine),
            StateOf(result.TownLine));
    }

    /// <summary>
    /// The first name, in ordinary case: `RICHARD R HALE` becomes `Richard`.
    /// </summary>
    /// <param name="full">The licensee name as the service holds it.</param>
    /// <returns>One name, or empty where it cannot be read.</returns>
    /// <remarks>
    /// <para>**A FIRST NAME AND NOT THE WHOLE RECORD.** What a card wants is what to
    /// call him, which is how the operator would greet him on the air.</para>
    /// <para>**AND NOTHING IS INVENTED WHERE THE SHAPE IS UNFAMILIAR.** A club
    /// licence holds an organisation's name rather than a person's, so a value with
    /// no space in it, or one that reads as a company, yields nothing rather than a
    /// guess at somebody's name (§0.0).</para>
    /// </remarks>
    internal static string FirstName(string? full)
    {
        var parts = (full ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);

        if (parts.Length < 2)
        {
            return "";
        }

        var first = parts[0];

        // **A SINGLE INITIAL IS NOT A NAME TO CALL SOMEBODY BY**, and a hyphen or an
        // apostrophe is part of one. Measured: a letters-only test threw away
        // `MARY-ANNE` and `O'BRIEN`, which is refusing to name real people.
        if (first.Length < 2
            || !first.All(c => char.IsLetter(c) || c == '-' || c == '\''))
        {
            return "";
        }

        return Cased(first);
    }

    /// <summary>A name in ordinary case, capitalised after a hyphen and an apostrophe.</summary>
    /// <remarks>
    /// **`ToTitleCase` CAPITALISES AFTER A HYPHEN AND NOT AFTER AN APOSTROPHE**, which
    /// is right for *o'clock* and wrong for a person: measured, it renders `O'BRIEN`
    /// as `O'brien`. A card carries a name the operator is about to say to the man
    /// himself, so it is worth the extra line.
    /// </remarks>
    private static string Cased(string word)
    {
        var letters = CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(word.ToLowerInvariant())
            .ToCharArray();

        for (var i = 1; i < letters.Length - 1; i++)
        {
            if (letters[i] == '\'')
            {
                letters[i + 1] = char.ToUpperInvariant(letters[i + 1]);
            }
        }

        return new string(letters);
    }

    /// <summary>
    /// The town and state: `SUN CITY, AZ 85373` becomes `Sun City AZ`.
    /// </summary>
    /// <param name="line">The address line the service holds.</param>
    /// <returns>The town and state, or empty where the shape is not recognised.</returns>
    /// <remarks>
    /// **THE ZIP IS DROPPED AND THE STREET IS NEVER READ AT ALL.** What is on the
    /// card is where he is, at the granularity one operator tells another. **The
    /// street address is not read out of the payload anywhere in this application**,
    /// which is the restraint `CallookCallsignLookup` wrote into its own parser and
    /// this does not undo.
    /// </remarks>
    internal static string TownAndState(string? line)
    {
        var state = StateOf(line);

        if (state.Length == 0)
        {
            return "";
        }

        var town = (line ?? "").Split(',')[0].Trim();

        if (town.Length == 0)
        {
            return "";
        }

        return CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(town.ToLowerInvariant()) + " " + state;
    }

    /// <summary>The two-letter state, or empty where the line is not that shape.</summary>
    /// <remarks>
    /// **THE SHAPE IS `TOWN, XX ZIP` AND ANYTHING ELSE YIELDS NOTHING.** An overseas
    /// address, a PO box abroad or a line the service formats differently is not
    /// forced into a state, because a station placed in the wrong state is exactly
    /// the confident wrong answer §0.0 exists to prevent.
    /// </remarks>
    internal static string StateOf(string? line)
    {
        var after = (line ?? "").Split(',');

        if (after.Length < 2)
        {
            return "";
        }

        var tail = after[^1].Trim().Split(
            ' ', StringSplitOptions.RemoveEmptyEntries);

        if (tail.Length == 0)
        {
            return "";
        }

        var state = tail[0].ToUpperInvariant();

        return state.Length == 2 && state.All(char.IsLetter) ? state : "";
    }
}
