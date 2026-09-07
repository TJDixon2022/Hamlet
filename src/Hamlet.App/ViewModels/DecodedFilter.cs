namespace Hamlet.App.ViewModels;

/// <summary>
/// Whether a decoded row is one the operator asked to see.
/// </summary>
/// <remarks>
/// <para>**TWO INDEPENDENT TOGGLES SINCE UNIT 252** (Tim's ruling, 2026-09-06),
/// superseding unit 251's three exclusive choices. `CQ` and `mine` are each on or
/// off, both can be on at once, and **`everything` is the state where neither is
/// on** rather than a third choice beside them. An enum could not express *both*,
/// which is the state he actually wants most evenings: the calls he could answer,
/// plus his own traffic.</para>
/// <para>**THE PREDICATE IS OVER THE TO-FIELD AND THE FROM-FIELD.**
/// `DigitalDecodeRow` already splits a message into addressee, sender and
/// payload, so this reads fields the row has rather than parsing the message a
/// second time. A second parser is a second answer waiting to disagree.</para>
/// <para>**A ROW WITH NO THREE FIELDS MATCHES NEITHER TOGGLE AND IS NOT AN
/// ERROR.** Free text and telemetry have no addressee, so they cannot be
/// addressed to anyone in particular and cannot be his. With neither toggle on
/// they are shown like everything else.</para>
/// <para>**AND SINCE UNIT 252 A ROW THIS SAYS NO TO IS REMOVED, NOT DIMMED**
/// (Tim's ruling, 2026-09-06). The dimming unit 251 chose left every row on the
/// list and moving, and the operator's problem was never that he could not see
/// the band's texture — it is that fourteen rows a slot, four slots a minute,
/// scrolls past faster than he can read. See
/// `MainWindowViewModel.ApplyDecodedFilter`.</para>
/// </remarks>
public static class DecodedFilterRule
{
    /// <summary>Whether a row belongs on the left-hand list.</summary>
    /// <param name="cq">Whether the `CQ` toggle is on.</param>
    /// <param name="addressee">The row's to-field, which may be "".</param>
    /// <returns>True when the left list should carry it.</returns>
    /// <remarks>
    /// <para>**OFF MEANS EVERYTHING**, which is the fresh-file state and the one
    /// this panel has always started in.</para>
    /// <para>**IT LOST ITS `mine` ARM ON 2026-09-07 AND THE ARM DID NOT MOVE, IT
    /// WAS SUPERSEDED.** `mine` was a toggle competing with `CQ` for one list;
    /// Tim's ruling makes it a side of the panel, always on, with no control that
    /// could hide it. The question *is this addressed to him* is now
    /// `Ft8MessageSplit.IsAddressedTo`, asked by the panel and by the contact
    /// column so the two cannot disagree.</para>
    /// <para>**AND THE NEW QUESTION IS NARROWER THAN THE OLD ONE.** The toggle
    /// matched either field, because Tim asked there for *his traffic* and a
    /// contact is two sides. The side is what he ruled *addressed to him*, which
    /// is the to-field alone.</para>
    /// </remarks>
    public static bool Wants(bool cq, string? addressee)
        => !cq || IsCallToAnyone(addressee);

    /// <summary>Whether the app knows a callsign to match `mine` against.</summary>
    /// <param name="callsign">The operator's callsign from settings.</param>
    /// <returns>True when there is something to compare a field with.</returns>
    public static bool HasSomethingToMatchOn(string? callsign)
        => !string.IsNullOrWhiteSpace(callsign);

    /// <summary>
    /// A to-field that is a call to anyone: `CQ`, `CQ DX`, `CQ POTA`.
    /// </summary>
    /// <param name="to">The to-field.</param>
    /// <returns>True when it is a call to anyone.</returns>
    /// <remarks>
    /// **CQ IN ANY OF ITS FORMS**, which is the ruling's own wording, and the
    /// behaviour unit 252 was told to keep. It takes two files to do it:
    /// `Ft8Vocabulary.Split` joins `CQ POTA W5LST EM33` into the one addressee
    /// `CQ POTA`, because the call and its direction are one field in two words,
    /// and this tests what that produced.
    /// </remarks>
    public static bool IsCallToAnyone(string? to)
        => Hamlet.RadioEngine.Contacts.Ft8MessageSplit.IsCallToAnyone(to);

    /// <summary>Whether a field names the same station as a callsign.</summary>
    /// <param name="field">The to-field or from-field.</param>
    /// <param name="callsign">The operator's callsign.</param>
    /// <returns>True when they are the same station.</returns>
    /// <remarks>
    /// <para>**COMPOUND AND PORTABLE FORMS ARE HIS.** If he is `W1ABC` then
    /// `W1ABC/P`, `W1ABC/QRP` and `W4/W1ABC` are all him, and a filter that hides
    /// his own portable operation while he is running it is a filter that lies
    /// about a band he is on.</para>
    /// <para>**THE RULE IS: STRIP THE SLASHES AND SEE IF HIS CALL IS ONE OF THE
    /// PIECES.** FT8 puts the prefix or suffix on the other side of a `/`, so a
    /// compound call is his base call plus one more piece and never his base call
    /// with letters welded onto it. That is why this splits rather than matching a
    /// prefix: a prefix test would make `W1ABCD` his, and `W1ABCD` is somebody
    /// else entirely.</para>
    /// <para>**AND IT WORKS BOTH WAYS ROUND.** He may type `W4/W1ABC` into
    /// settings while operating away from home, so the stored call is compared
    /// piece by piece too rather than being assumed bare.</para>
    /// <para>**AND SINCE WORK INSTRUCTION 271 THE BODY LIVES IN THE ENGINE.** The
    /// contact column needed the same question asked of the same fields, and a
    /// second copy of a callsign rule is a second answer waiting to disagree (§0).
    /// What makes two callsigns one station is a fact about amateur radio and not
    /// about a list control (§0.1), so it moved to
    /// <see cref="Hamlet.RadioEngine.Contacts.Ft8MessageSplit.IsSameStation"/> and
    /// this calls it. The behaviour, the signature and every caller are
    /// unchanged.</para>
    /// </remarks>
    public static bool IsSameStation(string? field, string? callsign)
        => Hamlet.RadioEngine.Contacts.Ft8MessageSplit.IsSameStation(field, callsign);
}
