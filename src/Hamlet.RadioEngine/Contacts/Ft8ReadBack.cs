using Hamlet.RadioEngine.Transmit;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// Whether a composed transmission would reach anybody, and what to say when it
/// would not.
/// </summary>
/// <param name="WouldReachAnybody">
/// True where a station hearing this slot on its own decodes the words the
/// operator clicked.
/// </param>
/// <param name="Refusal">
/// Why not, in the operator's own terms, naming what the encoder made of his
/// words. Empty where there is nothing to refuse.
/// </param>
public readonly record struct Ft8ReadBackVerdict(bool WouldReachAnybody, string Refusal);

/// <summary>
/// **A message that cannot be decoded by anybody is a transmission asserting
/// something nobody receives.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE RULE, AND IT LIVES IN THE ENGINE** (0.1). A hashed
/// callsign is a fact about FT8 and not about a view, so the view model asks this
/// and states nothing of its own. It is also the only copy: nothing here decides
/// what a callsign looks like, splits a message into fields, or knows which
/// callsigns are compound. It reads the two facts <c>Ft8Composer</c> already
/// measured and hands back an answer.</para>
/// <para>**WHAT A HASHED CALLSIGN COSTS.** A callsign that will not fit a
/// message's callsign field travels as a 22-bit hash instead. A receiver can put
/// a name to that hash only if it heard the full callsign in the same slot -
/// <c>Ft8SlotDecoder.Decode</c> builds its cache per slot and drops it when the
/// call returns - so a slot carrying one hashed message decodes to **nothing at
/// all**. That is a fact about FT8 rather than about this seam, and
/// <c>Ft8Transmission.CarriesHashedCallsign</c>'s own remarks have said so since
/// it was written.</para>
/// <para>**IT HAPPENED ON A LIVE ANTENNA.** On 2026-09-07 Hamlet keyed twice on
/// 14.074 and both slots decoded to nothing. Unit 271 repaired the input that
/// caused it - a six-character grid where the format carries four - and left the
/// class open: <c>Ft8Composer</c> computed the flag on every transmission and no
/// line of <c>src/</c> read it. Work instruction 272 task 1 measured what was
/// left: **15 of the 43 messages the menu can offer against the callsigns he
/// could plausibly work would be keyed and read back as nothing**, all of them
/// compound prefixes - the DX station's, and his own <c>W4/KC3QIS</c>.</para>
/// <para>**UNLESS HE CLICKED BRACKETS** (the arbiter's ruling for unit 272). The
/// port marks a callsign it recovered from a hash by putting it in angle
/// brackets, so an operator who asked for <c>&lt;VP2M/K1ABC&gt; KC3QIS FN00</c>
/// asked for exactly what the encoder produced and gets it. That is read off the
/// two strings rather than by looking for a bracket: where what the bits say is
/// character-for-character what he typed, the encoder made no substitution and
/// there is nothing to refuse.</para>
/// <para>**IT REFUSES; IT NEVER REPAIRS.** Nothing here rewrites a callsign,
/// drops a prefix or offers a substitute - that would be presenting a guess as
/// the operator's own words (0.0). It says no and says why, and whether Hamlet
/// should later offer him a way to send one anyway is a separate question, logged
/// and not chased.</para>
/// <para>**AND IT IS NOT A CHANGE TO THE MENU.** Nothing is removed, greyed,
/// hidden or reordered; this is asked at the send path, where the licence gate
/// already refuses, and nothing on <c>Ft8SendOptions</c> or
/// <c>Ft8SendMenu</c> consults it.</para>
/// </remarks>
public static class Ft8ReadBack
{
    /// <summary>Whether this transmission would reach anybody, and why not.</summary>
    /// <param name="transmission">What the composer made of the operator's words.</param>
    /// <returns>The verdict, with a sentence where it refuses.</returns>
    /// <exception cref="ArgumentNullException">There is no transmission.</exception>
    public static Ft8ReadBackVerdict Check(Ft8Transmission transmission)
    {
        ArgumentNullException.ThrowIfNull(transmission);

        // **NOTHING TO ANSWER.** Every callsign went out in full, so the slot
        // decodes to the words he clicked and this is the ordinary case.
        if (!transmission.CarriesHashedCallsign)
        {
            return new Ft8ReadBackVerdict(true, string.Empty);
        }

        // **UNLESS THE BRACKETS ARE HIS OWN.** Where the bits read back
        // character-for-character as what he typed, the encoder substituted
        // nothing and there is nothing to tell him. Read off the two strings
        // rather than by looking for a bracket, so nothing here has to know what
        // the port's marking looks like.
        if (string.Equals(transmission.ReadsBackAs, transmission.Text, StringComparison.Ordinal))
        {
            return new Ft8ReadBackVerdict(true, string.Empty);
        }

        return new Ft8ReadBackVerdict(
            false,
            "it encodes as \"" + transmission.ReadsBackAs + "\", which puts a callsign on the "
            + "air as a 22-bit hash instead of as a callsign. A station can only put a name to "
            + "that hash if it heard the whole callsign in the same slot, so anybody hearing "
            + "this on its own decodes nothing at all. Nothing was keyed. The message is still "
            + "in the menu and everything else toward that station will go.");
    }

    /// <summary>
    /// The line the Send area carries, in the house shape the send path's two
    /// other refusals already use.
    /// </summary>
    /// <param name="wanted">The words the operator clicked, exactly as he clicked them.</param>
    /// <param name="verdict">What <see cref="Check"/> said.</param>
    /// <returns>The whole sentence.</returns>
    /// <remarks>
    /// **THE SHAPE IS `Hamlet composed "&lt;text&gt;" and sent nothing:
    /// &lt;reason&gt;`**, which is the sentence the send path already uses when a
    /// message composed and no radio would take it. It composed - that is the
    /// whole difficulty - so the other shape, *did not send*, would be saying
    /// something untrue about where it stopped.
    /// </remarks>
    public static string SentNothing(string? wanted, Ft8ReadBackVerdict verdict)
        => "Hamlet composed \"" + (wanted ?? "").Trim() + "\" and sent nothing: "
            + verdict.Refusal;
}
