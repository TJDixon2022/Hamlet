using Hamlet.RadioEngine.Contacts;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>Which of §R2's macros a card would offer for one click, or none.</summary>
public enum Psk31Macro
{
    /// <summary>Nothing is offered.</summary>
    None,

    /// <summary>`HIS de MINE MINE K`.</summary>
    Answer,

    /// <summary>The report, name, QTH and grid.</summary>
    Report,

    /// <summary>The roger, thanks and 73, signing off.</summary>
    Confirm,
}

/// <summary>§R1's strict side: a macro is offered only when Hamlet is certain whose turn it is.</summary>
/// <remarks>
/// <para>**STRINGS AND READINGS IN, A MACRO NAME OUT** (§0.1). It composes no text and arms
/// nothing; <see cref="Psk31Macros"/> composes, and nothing here calls it.</para>
/// <para>**NEVER ON A GUESS** (§R1, *strict on anything that drives a transmission*). Anything but a
/// certain *your turn* over a certain message addressed to the operator that hands over offers
/// nothing - a guessed *your turn*, *his turn*, *he is still sending* and *unknown* alike. A low
/// offer count is a number to report, not a reason to loosen this (§6).</para>
/// <para>**WHICH MACRO FOLLOWS WHICH MESSAGE IS <see cref="Table"/>**, this unit's reading of §R2's
/// order (CQ, Answer, Report, Confirm), not a ruling:</para>
/// <list type="table">
/// <listheader><term>his last message, certain, to you, handing over</term><description>offered</description></listheader>
/// <item><term>a CQ</term><description>Answer - never reached: a CQ is addressed to anyone, so it is never a certain your turn</description></item>
/// <item><term>his bare calls (Answer)</term><description>Report</description></item>
/// <item><term>a Report or Chat, before you have sent a Report</term><description>Report</description></item>
/// <item><term>a Report or Chat, after you have</term><description>Confirm</description></item>
/// <item><term>a Closing (73) or End (SK, CL)</term><description>Confirm</description></item>
/// <item><term>anything, once you have signed off</term><description>none</description></item>
/// <item><term>Ack, Garbage or Unknown</term><description>none - Ack is never produced (ask 28), and the other two are never certain</description></item>
/// </list>
/// </remarks>
public static class Psk31Offer
{
    /// <summary>Which macro follows which message, in one line for a reader and for the report.</summary>
    public const string Table =
        "his last message, certain, addressed to you and handing over -> offered: a CQ -> Answer, never "
        + "reached because a CQ is addressed to anyone; his bare calls -> Report; a Report or Chat before "
        + "you have sent a Report -> Report; a Report or Chat after you have -> Confirm; a Closing or End "
        + "-> Confirm; anything once you have signed off -> none; Ack, Garbage or Unknown -> none.";

    /// <summary>The macro to offer, or none.</summary>
    /// <param name="conversation">The conversation's messages, oldest first.</param>
    /// <param name="turn">Whose turn it is on it (`Psk31Turn`).</param>
    /// <param name="operatorCallsign">The operator's own callsign, or null.</param>
    /// <returns>The macro, or <see cref="Psk31Macro.None"/>.</returns>
    /// <exception cref="ArgumentNullException">There is no conversation or no reading.</exception>
    public static Psk31Macro For(
        IReadOnlyList<Psk31Message> conversation, Psk31TurnReading turn, string? operatorCallsign)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(turn);

        if (turn.State != Psk31TurnState.YourTurn || !turn.IsCertain || conversation.Count == 0)
        {
            return Psk31Macro.None;
        }

        var his = conversation[^1].Exchange;

        // **THE READING AND THE MESSAGE MUST AGREE**, so a reading handed in from elsewhere cannot
        // make an uncertain message offer.
        if (!his.IsCertain || !his.IsForOperator || !his.HandsOver)
        {
            return Psk31Macro.None;
        }

        var yours = conversation
            .Select(m => m.Exchange)
            .Where(e => Ft8MessageSplit.IsSameStation(e.Speaker, operatorCallsign))
            .ToList();

        if (yours.Any(e => e.Kind == Psk31LineKind.End))
        {
            return Psk31Macro.None;
        }

        return his.Kind switch
        {
            Psk31LineKind.Answer => Psk31Macro.Report,
            Psk31LineKind.Report or Psk31LineKind.Chat
                => yours.Any(e => e.Kind == Psk31LineKind.Report) ? Psk31Macro.Confirm : Psk31Macro.Report,
            Psk31LineKind.Closing or Psk31LineKind.End => Psk31Macro.Confirm,
            _ => Psk31Macro.None,
        };
    }
}
