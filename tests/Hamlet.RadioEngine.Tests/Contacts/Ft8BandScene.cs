namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>One transmission the scene puts on the air, before any decoder saw it.</summary>
/// <param name="Slot">Which numbered slot it is in.</param>
/// <param name="BaseFrequencyHz">Where tone 0 sits, in the audio passband.</param>
/// <param name="Text">The message, in the operator's own words.</param>
public sealed record Ft8SceneSignal(int Slot, float BaseFrequencyHz, string Text);

/// <summary>
/// **The band scene, written down before anything was composed.** Twelve
/// consecutive slots on one frequency, five stations Hamlet ends up holding, and
/// the four things `PHASE_PLAN.md` step 4 asks to be shown about each of them.
/// </summary>
/// <remarks>
/// <para>**THIS IS WHAT WENT IN. IT IS NOT THE CORPUS.** The corpus is
/// <c>tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt</c> and it holds
/// what came back out of <c>Ft8DeepSlotDecoder</c>. If a message here did not
/// decode, it is not in the corpus, the generator counts it and names it, and
/// nobody writes it in by hand.</para>
/// <para>**THE OPERATOR TRANSMITS IN ODD SLOTS AND HEARS NOTHING IN THEM.** A
/// station is deaf while it is transmitting, so every message the operator's
/// receiver could have heard is in an even slot and every message the operator
/// sent is in an odd one. A scene that put both in one slot would be describing a
/// station with two radios.</para>
/// <para>**AND THE OPERATOR'S OWN SIX TRANSMISSIONS ARE COMPOSED AND DECODED
/// TOO**, so that every line of the corpus came out of Hamlet's decoder rather
/// than off a keyboard. What that models is the band as an ideal third-party
/// receiver would have heard it, the operator's own signal included. The test
/// routes a line whose sender is the operator to the ledger's *sent* door and
/// every other line to its *heard* door, which is exactly what a send path will
/// do in step 5 - it will not decode its own transmission, it will say what it
/// sent.</para>
/// <para>**EVERY STATION IS ON ITS OWN FREQUENCY AND STAYS THERE**, which is what
/// stations do, and it keeps the signals in any one slot at least 300 Hz apart.
/// All of them are inside the decoder's 200-3000 Hz search window; unit 256's
/// <c>ATransmissionOutsideTheDecodersSearchWindowIsNotReadBack</c> is what
/// happens otherwise.</para>
/// </remarks>
public static class Ft8BandScene
{
    /// <summary>Whose station the scene is written from.</summary>
    /// <remarks>
    /// <c>OperatorProfile.Callsign</c>'s own default, so the scene and the
    /// application are describing the same operator. The ledger takes it as a
    /// parameter and never reads it from settings.
    /// </remarks>
    public const string OperatorCallsign = "KC3QIS";

    /// <summary>The boundary slot 0 opened on.</summary>
    public static DateTime SlotZeroUtc { get; } =
        new(2026, 9, 6, 18, 0, 0, DateTimeKind.Utc);

    /// <summary>How many numbered slots the scene runs for.</summary>
    /// <remarks>Twelve, which is the cap work instruction 257 sets.</remarks>
    public const int SlotCount = 12;

    /// <summary>The most signals the scene puts in any one slot.</summary>
    /// <remarks>The cap is eight; the scene's busiest slot holds three.</remarks>
    public const int SignalCap = 8;

    /// <summary>Where each station sits in the passband.</summary>
    /// <remarks>
    /// Chosen so that the two signals closest together in any one slot are 300 Hz
    /// apart - an FT8 signal is 50 Hz wide, so that is six times its own width -
    /// and so that every one is well inside the 200-3000 Hz the decoder searches.
    /// </remarks>
    public const float G4xyzHz = 700.0f;
    public const float Vk2pqHz = 1100.0f;
    public const float K9rstHz = 1500.0f;
    public const float W1abcHz = 1900.0f;
    public const float N5ttHz = 2300.0f;
    public const float OperatorHz = 1300.0f;
    public const float FreeTextLowHz = 400.0f;
    public const float FreeTextHighHz = 2600.0f;

    /// <summary>
    /// Every transmission in the scene, in slot order, the operator's own included.
    /// </summary>
    /// <remarks>
    /// <para>**WHAT EACH STATION IS HERE TO PROVE**, one line each. The five are
    /// what the ledger ends up holding; <c>DL1QQ</c> and <c>JA1ZZ</c> are
    /// addressees of somebody else's traffic and never transmit in an even slot,
    /// so the ledger never books them - which is itself correct and is asserted.
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>W1ABC</b> - a CQ the operator answers, run to a
    /// complete exchange with <b>no 73 anywhere in it</b>. Step 4's criterion 3.
    /// </description></item>
    /// <item><description><b>G4XYZ</b> - <b>one station working three others at
    /// once</b>: the operator, DL1QQ and JA1ZZ, interleaved across the same
    /// slots, so from the operator's side his replies have gaps in them. He
    /// transmits in slot 10 and must never read as gone quiet. Step 4's criterion
    /// 5, and the case this scene exists for.</description></item>
    /// <item><description><b>VK2PQ</b> - answers once, at slot 2, and is
    /// <b>never heard again</b>: nine silent slots after it.</description></item>
    /// <item><description><b>K9RST</b> - an exchange that is <b>already complete
    /// at slot 5</b>, who then sends <c>73</c> at slot 6 anyway. Complete did not
    /// need it and its arrival changes nothing.</description></item>
    /// <item><description><b>N5TT</b> - answers the operator's CQ late, at slot 8,
    /// and the operator has not come back to him. The plain <i>your move</i> case.
    /// </description></item>
    /// </list>
    /// <para>**AND TWO MESSAGES THE SPLITTER REFUSES**, at slots 4 and 10, so the
    /// ledger is watched not crashing on what it cannot split. Both are four-word
    /// or one-word free text: <c>Ft8MessageSplit</c> takes three fields, or four
    /// where the first is <c>CQ</c>, and refuses everything else. A three-word
    /// free-text message such as <c>HW CPY OM</c> would be <i>accepted</i> as
    /// three fields, which is the format's answer and not a defect, so it is
    /// deliberately not the case used here.</para>
    /// </remarks>
    public static IReadOnlyList<Ft8SceneSignal> Signals { get; } =
    [
        // Slot 0 - G4XYZ is already working JA1ZZ when the scene opens.
        new(0, G4xyzHz, "JA1ZZ G4XYZ -08"),

        // Slot 1 - the operator calls CQ. One transmission, several answers.
        new(1, OperatorHz, "CQ KC3QIS FN00"),

        // Slot 2 - three stations answer the same CQ. The operator can take one.
        new(2, Vk2pqHz, "KC3QIS VK2PQ QF56"),
        new(2, K9rstHz, "KC3QIS K9RST EM12"),
        new(2, G4xyzHz, "KC3QIS G4XYZ IO91"),

        // Slot 3 - he takes K9RST. VK2PQ and G4XYZ are left where they are.
        new(3, OperatorHz, "K9RST KC3QIS -13"),

        // Slot 4 - K9RST rogers and reports; G4XYZ has gone to DL1QQ; free text.
        new(4, K9rstHz, "KC3QIS K9RST R-09"),
        new(4, G4xyzHz, "DL1QQ G4XYZ -12"),
        new(4, FreeTextHighHz, "TNX BOB 73 GL"),

        // Slot 5 - the operator acknowledges. THE K9RST EXCHANGE IS COMPLETE HERE.
        new(5, OperatorHz, "K9RST KC3QIS RRR"),

        // Slot 6 - K9RST's 73 arrives AFTER complete; G4XYZ is on JA1ZZ; W1ABC calls.
        new(6, K9rstHz, "KC3QIS K9RST 73"),
        new(6, G4xyzHz, "JA1ZZ G4XYZ RR73"),
        new(6, W1abcHz, "CQ W1ABC FN42"),

        // Slot 7 - the operator answers W1ABC with a report.
        new(7, OperatorHz, "W1ABC KC3QIS -12"),

        // Slot 8 - W1ABC rogers and reports; G4XYZ is on DL1QQ; N5TT answers late.
        new(8, W1abcHz, "KC3QIS W1ABC R-15"),
        new(8, G4xyzHz, "DL1QQ G4XYZ RRR"),
        new(8, N5ttHz, "KC3QIS N5TT EM10"),

        // Slot 9 - the operator acknowledges. W1ABC IS COMPLETE, AND NO 73 IS IN IT.
        new(9, OperatorHz, "W1ABC KC3QIS RRR"),

        // Slot 10 - G4XYZ is calling again, still not to us; more free text.
        new(10, G4xyzHz, "CQ G4XYZ IO91"),
        new(10, FreeTextLowHz, "ABCDEFGHIJKLM"),

        // Slot 11 - the operator finally comes back to G4XYZ.
        new(11, OperatorHz, "G4XYZ KC3QIS -14"),
    ];
}
