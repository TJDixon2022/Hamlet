namespace Ft8Sharp.Deep;

/// <summary>
/// <b>What the subtraction stage did in the last slot decoded, beside the port's five counts.</b>
/// </summary>
/// <param name="PassesRun">
/// How many times the slot was read. <b>One with subtraction off</b>, and the divisor a reader needs
/// to make sense of the five counts, which are summed across the passes.
/// </param>
/// <param name="MessagesOffered">Decoded messages the stage was asked to subtract.</param>
/// <param name="MessagesSubtracted">Of those, how many were fitted and removed from the buffer.</param>
/// <param name="RefusedForWantOfSymbols">
/// <b>Of those, how many <c>Ft8DeepMessageSymbols.TryEncode</c> would not give up the 79 channel
/// symbols for.</b> Counted and never hidden: a silent skip is how a stage comes to report a pass it
/// did not make. Unit 251 measured 0 refusals in 510 on the ladder's population, so this reads zero
/// there and is expected to be non-zero on real air.
/// </param>
/// <param name="RefusedForWantOfFrame">
/// Of those, how many had fewer than <c>Ft8DeepSubtractionSettings.MinimumSymbols</c> of the frame
/// inside the slot. A transmission that ran off the end of what was captured is not subtracted on
/// the strength of the part that arrived.
/// </param>
/// <param name="DuplicatesAcrossPasses">
/// <b>Messages a later pass returned that an earlier pass had already returned.</b> Expected and not
/// a defect: an imperfectly subtracted transmission decodes again out of its own remnant. The
/// message is counted here and is <b>not</b> added to the result.
/// </param>
/// <param name="MessagesFromLaterPasses">
/// <b>THE NUMBER THIS STAGE IS JUDGED ON.</b> Messages in the result that the first pass did not
/// return.
/// </param>
/// <param name="DecibelsRemovedWorst">
/// The smallest <c>Ft8DeepSubtractionFit.DecibelsRemoved</c> of the slot — the fit that did least.
/// <b>Reported, never a gate</b>, for the reason on <c>Ft8DeepSubtractionFit.DecibelsRemoved</c>.
/// </param>
/// <remarks>
/// <b>A rate that moved with no visible subtraction activity behind it is not evidence</b>, which is
/// why these are kept at all. They live here rather than on <c>Ft8SlotResult</c> because that is the
/// port's own record and this phase changes no line of the port; the scoreboard's seat is
/// <c>Func&lt;float[], Ft8SlotResult&gt;</c>, so a report that wants these reads them off the
/// decoder after the call. All zero while <c>Ft8DeepSlotDecoder.Subtraction</c> is null, except
/// <see cref="PassesRun"/>, which is one.
/// </remarks>
public readonly record struct Ft8DeepSubtractionCounts(
    int PassesRun,
    int MessagesOffered,
    int MessagesSubtracted,
    int RefusedForWantOfSymbols,
    int RefusedForWantOfFrame,
    int DuplicatesAcrossPasses,
    int MessagesFromLaterPasses,
    double DecibelsRemovedWorst);
