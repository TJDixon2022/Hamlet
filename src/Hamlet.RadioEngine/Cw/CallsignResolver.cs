using System.Text;
using System.Text.RegularExpressions;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What a resolved callsign was doing in the transmission.</summary>
public enum CallsignRole
{
    /// <summary>The station that was transmitting: the call after <c>DE</c>.</summary>
    Sender,

    /// <summary>
    /// The station being called: the call before <c>DE</c>. When this is the
    /// operator's own, somebody is answering him.
    /// </summary>
    Addressed,
}

/// <summary>A callsign Hamlet is prepared to claim it heard.</summary>
/// <param name="Callsign">The callsign, every character solid.</param>
/// <param name="Role">What it was doing.</param>
/// <param name="Marker">The ritual word that placed it, e.g. "DE" or "K".</param>
public sealed record ResolvedCallsign(string Callsign, CallsignRole Role, string Marker);

/// <summary>
/// Pulling a callsign out of decoded Morse, and refusing to nearly do it
/// (HM-DEC-073).
/// </summary>
/// <remarks>
/// <para>A WRONG CALLSIGN IS WORSE THAN NO CALLSIGN, and worse than that on the
/// day somebody uses it to decide whether anybody answered them. `KC3QIS` with
/// one uncertain character is also a plausible reading of other real callsigns
/// belonging to other people, so a call extracted from text carrying a dimmed or
/// blocked character is a guess wearing the costume of an identification
/// (§0.0).</para>
/// <para>TWO CONDITIONS, AND BOTH ARE REQUIRED.</para>
/// <para>STRUCTURE. A claim is made only where the ritual says a callsign
/// belongs: the token after <c>DE</c> is the sending station, the token before
/// <c>DE</c> is who they are calling, and the token immediately before a closing
/// prosign is the station signing. Those are positions the whole hobby agrees
/// on. A callsign-shaped string sitting in loose text is not claimed, however
/// convincing it looks, because the shape of a callsign is also the shape of a
/// signal report with a letter in it and half the abbreviations in Morse.</para>
/// <para>CLEANLINESS. Every character of the token must have come back
/// <see cref="CwConfidence.High"/>. One dimmed character or one block and
/// nothing is claimed: no partial claim, no most-likely completion, no
/// confidence-marked callsign, because a callsign shown as uncertain still gets
/// read as fact and acted on. The dimmed text stays in the terminal with its
/// existing marking, so nothing is hidden and nothing is asserted.</para>
/// <para>Pure: characters in, callsigns out. No radio, no clock (§5).</para>
/// </remarks>
public static class CallsignResolver
{
    /// <summary>The word that separates who it is for from who it is from.</summary>
    public const string From = "DE";

    /// <summary>Prosigns that close a transmission.</summary>
    /// <remarks>
    /// The station signing off names itself immediately before one of these, so
    /// the position is structural in the same way <c>DE</c> is. It is a weaker
    /// marker than <c>DE</c> and it is only read when the token in front of it
    /// is callsign-shaped and solid, which no abbreviation in ordinary use is.
    /// </remarks>
    private static readonly HashSet<string> Closing = new(StringComparer.Ordinal)
    {
        "K", "KN", "SK", "<KN>", "<SK>", "<AR>", "AR",
    };

    /// <summary>
    /// Words that are never a callsign however they are placed.
    /// </summary>
    /// <remarks>
    /// A belt beside the braces. The shape test already rejects almost all of
    /// these, and listing the ones that could squeak through costs nothing
    /// against the one day it matters.
    /// </remarks>
    private static readonly HashSet<string> NeverACall = new(StringComparer.Ordinal)
    {
        "CQ", "DE", "K", "KN", "SK", "AR", "TU", "UR", "RST", "QRS", "QRZ",
        "QTH", "QSL", "QRM", "QRN", "QSB", "PSE", "ES", "BK", "R", "FB", "AGN",
        "OP", "NEW", "DX", "TEST", "73", "88", "5NN", "599",
    };

    /// <summary>
    /// The shape of an amateur callsign, strictly.
    /// </summary>
    /// <remarks>
    /// A prefix of one or two letters or a letter and a digit, then the digit
    /// that separates prefix from suffix, then one to four letters, and
    /// optionally a stroke and a portable indicator. Deliberately tight: it is
    /// the second gate after structure, and a loose one would let a signal
    /// report through on a day that cannot afford it.
    /// </remarks>
    private static readonly Regex Shape = new(
        @"^(?:[A-Z]{1,2}|[A-Z][0-9]|[0-9][A-Z])[0-9][A-Z]{1,4}(?:/[A-Z0-9]{1,3})?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Every callsign Hamlet is prepared to claim from this text.
    /// </summary>
    /// <param name="characters">The decoded characters, in order.</param>
    /// <returns>The claims, in the order they were heard, possibly empty.</returns>
    /// <remarks>
    /// Empty is the ordinary answer and it is never an error. Most of what
    /// crosses a receiver is not a clean callsign in a ritual position, and
    /// saying nothing about it is the correct behavior rather than a
    /// shortcoming.
    /// </remarks>
    public static IReadOnlyList<ResolvedCallsign> Resolve(
        IReadOnlyList<CwCharacter>? characters)
    {
        if (characters is null || characters.Count == 0)
        {
            return Array.Empty<ResolvedCallsign>();
        }

        var tokens = Tokenize(characters);
        var found = new List<ResolvedCallsign>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < tokens.Count; i++)
        {
            if (!string.Equals(tokens[i].Text, From, StringComparison.Ordinal))
            {
                continue;
            }

            // WHO IT IS FROM: everything after DE up to the next ritual word.
            // Sent twice as often as once, because the first is half-missed
            // while somebody is still tuning, so the repeat is folded rather
            // than reported as a second station.
            for (var j = i + 1; j < tokens.Count && j <= i + 2; j++)
            {
                if (!Claimable(tokens[j]))
                {
                    break;
                }

                Add(found, seen, tokens[j].Text, CallsignRole.Sender, From);
            }

            // WHO IT IS FOR: the token in front of DE, which is the whole of
            // how Hamlet can tell that somebody is answering the operator
            // rather than calling anybody.
            if (i > 0 && Claimable(tokens[i - 1]))
            {
                Add(found, seen, tokens[i - 1].Text, CallsignRole.Addressed, From);
            }
        }

        // THE STATION SIGNING, where no DE was heard cleanly. A weaker marker
        // and a real one: nobody puts anything but their own call in front of K.
        for (var i = 1; i < tokens.Count; i++)
        {
            if (Closing.Contains(tokens[i].Text) && Claimable(tokens[i - 1]))
            {
                Add(found, seen, tokens[i - 1].Text, CallsignRole.Sender, tokens[i].Text);
            }
        }

        return found;
    }

    /// <summary>
    /// The station Hamlet would name, or null.
    /// </summary>
    /// <param name="characters">The decoded characters.</param>
    /// <returns>The transmitting station's callsign, or null.</returns>
    /// <remarks>
    /// The sender, because that is who is on the frequency. Who they were
    /// calling is a different fact and is not the station that was there.
    /// </remarks>
    public static string? StationHeard(IReadOnlyList<CwCharacter>? characters)
        => Resolve(characters)
            .FirstOrDefault(c => c.Role == CallsignRole.Sender)?.Callsign;

    /// <summary>
    /// Whether anybody was heard calling this station.
    /// </summary>
    /// <param name="characters">The decoded characters.</param>
    /// <param name="yourCall">The operator's own callsign.</param>
    /// <returns>Who is calling them, or null.</returns>
    /// <remarks>
    /// The one question the operator most wants answered, and the reason the
    /// addressed position is read at all. It is still a claim about a decode, so
    /// it obeys exactly the same two conditions as everything else here.
    /// </remarks>
    public static string? AnsweringYou(
        IReadOnlyList<CwCharacter>? characters, string? yourCall)
    {
        var mine = (yourCall ?? "").Trim().ToUpperInvariant();

        if (mine.Length == 0)
        {
            return null;
        }

        var claims = Resolve(characters);

        var addressed = claims.Any(
            c => c.Role == CallsignRole.Addressed
                 && string.Equals(c.Callsign, mine, StringComparison.Ordinal));

        return addressed
            ? claims.FirstOrDefault(c => c.Role == CallsignRole.Sender)?.Callsign
            : null;
    }

    private static void Add(
        List<ResolvedCallsign> into, HashSet<string> seen,
        string call, CallsignRole role, string marker)
    {
        if (seen.Add($"{call}|{role}"))
        {
            into.Add(new ResolvedCallsign(call, role, marker));
        }
    }

    /// <summary>Both gates, in one place so neither can be skipped.</summary>
    private static bool Claimable(Token token)
        => token.AllSolid
           && !NeverACall.Contains(token.Text)
           && Shape.IsMatch(token.Text);

    private static List<Token> Tokenize(IReadOnlyList<CwCharacter> characters)
    {
        var tokens = new List<Token>();
        var text = new StringBuilder();
        var solid = true;
        var any = false;

        void Flush()
        {
            if (any)
            {
                tokens.Add(new Token(text.ToString().ToUpperInvariant(), solid));
            }

            text.Clear();
            solid = true;
            any = false;
        }

        foreach (var character in characters)
        {
            if (character.IsWordGap)
            {
                Flush();
                continue;
            }

            any = true;

            // AN UNREADABLE CHARACTER IS NOT A LETTER AND IS NEVER TREATED AS
            // ONE. It taints the token it lands in, which is the whole point of
            // marking it in the first place.
            if (character.Confidence != CwConfidence.High)
            {
                solid = false;
            }

            text.Append(character.Text);
        }

        Flush();

        return tokens;
    }

    private readonly record struct Token(string Text, bool AllSolid);
}
