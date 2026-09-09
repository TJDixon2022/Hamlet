namespace Hamlet.RadioEngine.Audio;

/// <summary>Which of the two slotted digital modes a path is running.</summary>
/// <remarks>
/// <para>**ONE FACT, SO TWO HALVES CANNOT DISAGREE** (work instruction 292 task 3).
/// A grid and a decoder are the two things that have to change together when the
/// operator presses FT4, and passing them as two arguments is exactly how a path
/// comes to cut 7.5-second slots and hand them to FT8's decoder - which is the
/// state the tab was in before this type existed, and which produces an empty
/// table on a live band with nothing on screen able to say why. One value travels
/// and both are derived from it.</para>
/// <para>**IT IS THE TWO MODES THAT HAVE A PATH, AND NOT THE STRIP'S FOUR.**
/// `DigitalModeChip.Labels` carries PSK31 and WSPR as well, and neither has a
/// decoder, a grid or a frequency row anywhere in this tree. A member here for a
/// mode nothing can decode would be a type asserting a capability the application
/// does not have (§0.0), so the shell maps its four labels onto these two and says
/// what it did with the other two.</para>
/// <para>**IT IS NOT A READING OF THE RADIO.** This is what Hamlet is trying to
/// decode, which is the operator's choice; where the dial actually is, is the
/// map's answer and is a different fact (`DigitalModeChip.cs:70-78`,
/// HM-DEC-092).</para>
/// </remarks>
public enum DigitalMode
{
    /// <summary>FT8: fifteen-second slots, and the mode every path here defaults to.</summary>
    Ft8 = 0,

    /// <summary>FT4: 7.5-second slots, read by the port's own FT4 decoder.</summary>
    Ft4 = 1,
}

/// <summary>What each digital mode's slot grid is.</summary>
/// <remarks>
/// **THE MAPPING LIVES IN ONE PLACE AND IS A LOOKUP, NOT ARITHMETIC.** Both grids
/// are <see cref="SlotGrid"/>'s own statics, so FT4's two numbers still come out of
/// <c>Ft8Sharp.Ft4Timing</c> and the open 4.48-against-5.04 question still costs one
/// edit to settle.
/// </remarks>
public static class DigitalModes
{
    /// <summary>The slot grid this mode runs on.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>Its grid.</returns>
    /// <remarks>
    /// **ANYTHING THAT IS NOT FT4 IS FT8'S GRID**, which is what every caller that
    /// existed before this type got and still gets.
    /// </remarks>
    public static SlotGrid Grid(this DigitalMode mode)
        => mode == DigitalMode.Ft4 ? SlotGrid.Ft4 : SlotGrid.Ft8;

    /// <summary>How the permanent log names a contact made in this mode.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The log's entry for it, or null where it has none.</returns>
    /// <remarks>
    /// <para>**THE MODE WRITTEN INTO THE LOG IS THE MODE THE TAB WAS RUNNING**
    /// (work instruction 293 task 4). It is derived from the same one value the
    /// grid, the cutter, the slot watch and the decoder derive from - **not a second
    /// source, not a string, and not the chip's lit state**, which is a reading of
    /// where the dial is and not an instruction about what Hamlet is doing.</para>
    /// <para>**A LOG RECORD NAMING A MODE THE CONTACT WAS NOT MADE IN IS WRONG FOR
    /// AS LONG AS THE LOG EXISTS** (§0.0, §12.1). That is why this is a lookup on
    /// one enum rather than a string handed in at the call site: there is no
    /// argument to get wrong and no spelling to drift.</para>
    /// <para>**THE MAPPING ITSELF IS UNIT 291'S AND IS NOT RE-DERIVED HERE.**
    /// <see cref="Contacts.ContactModes"/> is where FT4 is `MODE=MFSK` plus
    /// `SUBMODE=FT4`, with its citation; this names an entry in that table and
    /// decides nothing about what an entry contains.</para>
    /// </remarks>
    public static Contacts.ContactMode? Contact(this DigitalMode mode)
        => Contacts.ContactModes.Named(mode == DigitalMode.Ft4 ? "FT4" : "FT8");
}
