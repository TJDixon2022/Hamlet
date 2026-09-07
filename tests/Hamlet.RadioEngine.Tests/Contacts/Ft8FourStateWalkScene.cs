namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **One exchange, six messages, twelve slots, and a silence in the middle of
/// it** - the scene a row has to walk through all four states to describe.
/// </summary>
/// <remarks>
/// <para>**WHY A SECOND SCENE AND NOT <see cref="Ft8BandScene"/>.** The band scene
/// survives, it is reused unchanged by every other case, and it cannot do this
/// one: no station in it walks all four states, and there is no room to add one.
/// <c>W1ABC</c> goes *your move -> waiting on him -> your move -> complete* and
/// never gone quiet; <c>VK2PQ</c> goes *your move -> gone quiet* and never
/// completes; <c>G4XYZ</c> is deliberately never gone quiet at all, which is the
/// case he exists for. The band scene is also already at its twelve-slot cap.
/// </para>
/// <para>**AND GONE QUIET CANNOT FOLLOW COMPLETE, BY DESIGN.**
/// <c>Ft8ContactStates.Read</c> tests complete first, so an exchange that has what
/// a QSO needs stays complete however long the silence after it runs - which is
/// what makes a late `73`, or none, change nothing. So the only place a walk can
/// pass through gone quiet is **in the middle of the exchange**, and that is what
/// this scene is: a station answers a CQ, goes quiet for six slots, comes back and
/// finishes the contact.</para>
/// <para>**IT IS AN ORDINARY EVENING ON THE BAND AND NOT A CONTRIVANCE.** A
/// station that answers, disappears for ninety seconds and comes back is the
/// commonest thing in FT8 - QSB, a lost decode, somebody's dog. **Hamlet reports
/// it and rules nothing**: the row says *gone quiet, 4 slots* and offers
/// everything it offered before.</para>
/// <para>**THIS IS WHAT WENT IN. IT IS NOT THE CORPUS.** The corpus is
/// <c>tests/fixtures/ft8/scenes/unit266-four-state-walk.corpus.txt</c> and it
/// holds what came back out of <c>Ft8DeepSlotDecoder</c>. If a message here did
/// not decode it is not in the corpus, the generator names it, and nobody writes
/// it in by hand.</para>
/// <para>**THE OPERATOR TRANSMITS IN ODD SLOTS AND HEARS NOTHING IN THEM**, the
/// same rule <see cref="Ft8BandScene"/> keeps: a station is deaf while it is
/// transmitting, so a scene with both in one slot would be describing a station
/// with two radios.</para>
/// </remarks>
public static class Ft8FourStateWalkScene
{
    /// <summary>Whose station the scene is written from.</summary>
    public const string OperatorCallsign = "KC3QIS";

    /// <summary>The station the whole scene is about.</summary>
    public const string Station = "W9GAP";

    /// <summary>The boundary slot 0 opened on.</summary>
    public static DateTime SlotZeroUtc { get; } =
        new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>How many numbered slots the scene runs for.</summary>
    public const int SlotCount = 12;

    /// <summary>Where each station sits in the passband.</summary>
    /// <remarks>
    /// 600 Hz apart, both well inside the 200-3000 Hz the decoder searches, and
    /// never in the same slot as each other anyway.
    /// </remarks>
    public const float StationHz = 1900.0f;

    /// <summary>Where the operator sits in the passband.</summary>
    public const float OperatorHz = 1300.0f;

    /// <summary>The slot the station falls silent after.</summary>
    public const int LastSlotBeforeTheSilence = 0;

    /// <summary>The slot he comes back in.</summary>
    public const int TheSlotHeComesBackIn = 8;

    /// <summary>
    /// The whole exchange, in slot order, the operator's own transmissions
    /// included.
    /// </summary>
    /// <remarks>
    /// <para>**SIX MESSAGES, WHICH IS A WHOLE FT8 CONTACT**: a call, an answer
    /// with a grid, a report, a rogered report, an acknowledgement and a
    /// courtesy. Three each way.</para>
    /// <para>**AND THE `73` AT SLOT 11 IS DELIBERATELY LAST AND DELIBERATELY
    /// IRRELEVANT.** The contact is already complete at slot 10, on the
    /// acknowledgement, and the `73` changes nothing - which is the ruling this
    /// scene walks past on its way to the end.</para>
    /// </remarks>
    public static IReadOnlyList<Ft8SceneSignal> Signals { get; } =
    [
        // Slot 0 - he calls anybody. Nothing has gone back, so the row is his to
        // be answered: YOUR MOVE.
        new(0, StationHz, "CQ W9GAP DM79"),

        // Slot 1 - the operator answers with his grid. WAITING ON HIM.
        new(1, OperatorHz, "W9GAP KC3QIS FN00"),

        // Slots 2 to 7 - nothing. Six slots of silence, and the row crosses the
        // four-slot threshold at slot 4: GONE QUIET, with the count.

        // Slot 8 - he comes back with a report as though nothing happened, which
        // is what stations do. YOUR MOVE again.
        new(8, StationHz, "KC3QIS W9GAP -11"),

        // Slot 9 - the operator rogers and reports. WAITING ON HIM.
        new(9, OperatorHz, "W9GAP KC3QIS R-09"),

        // Slot 10 - he acknowledges. THE CONTACT IS COMPLETE HERE, on the
        // acknowledgement and not on any courtesy.
        new(10, StationHz, "KC3QIS W9GAP RRR"),

        // Slot 11 - the operator's 73. Politeness, and it changes nothing.
        new(11, OperatorHz, "W9GAP KC3QIS 73"),
    ];
}
