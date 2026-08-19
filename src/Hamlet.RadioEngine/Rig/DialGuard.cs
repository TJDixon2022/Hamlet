namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// Whether a reading may move the frequency on screen (§0.0, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR TUNED FROM THE APP AND WATCHED THE DISPLAY SNAP BACK.**
/// The radio moved correctly and stayed moved; the picture returned to where he
/// had just been and held it. Turning the radio's own knob was perfect
/// throughout, and the dividing line was the write.</para>
/// <para>**A READING TAKEN BEFORE THE WRITE CANNOT SAY WHERE THE DIAL IS NOW.**
/// It is a true reading of a moment that has passed, and applying it puts a
/// number on screen that was current then and is being drawn as though it were
/// current now, which is the prime directive broken by a value rather than by a
/// sentence. The guard that existed covered the queue and was released the
/// instant the send began, so the whole round trip was unprotected.</para>
/// <para>Bounded by the write and never by a timer. A window would be too short
/// on a busy link and would freeze the display after it had already caught up on
/// a quiet one, and both are the app deciding it knows better than the radio.</para>
/// </remarks>
public static class DialGuard
{
    /// <summary>
    /// Whether a reading is about the world after Hamlet's own last tune.
    /// </summary>
    /// <param name="reading">What the model holds for the frequency.</param>
    /// <param name="tunedAtUtc">When Hamlet last wrote the dial, or null.</param>
    /// <returns>True when the reading may move the display.</returns>
    /// <remarks>
    /// <para>True when Hamlet has never tuned, which is every case of the
    /// operator using the radio's own knob: the path that always worked is not
    /// slowed down by any of this.</para>
    /// <para>A reading stamped at the very instant of the write crossed it on the
    /// wire, so the comparison is strict. Knife-edge cases go the way that leaves
    /// the operator's own action standing (§0.2.1).</para>
    /// </remarks>
    public static bool MayFollow(RigValue? reading, DateTime? tunedAtUtc)
    {
        if (tunedAtUtc is not { } tuned)
        {
            return true;
        }

        if (reading is null || !reading.IsKnown)
        {
            return false;
        }

        // A reading with no moment of its own cannot argue with one that has a
        // moment. Unknown is a state and never a licence (HM-DEC-050).
        return reading.AtUtc is { } taken && taken > tuned;
    }

    /// <summary>
    /// Whether a reading would take the display back where it just came from.
    /// </summary>
    /// <param name="readingHz">What the reading says.</param>
    /// <param name="tunedFromHz">Where the dial was before the tune.</param>
    /// <param name="tunedToHz">Where the tune was aimed, or null.</param>
    /// <returns>True when this is the signature of the snap-back.</returns>
    /// <remarks>
    /// **THE RADIO DOES NOT TUNE ITSELF BACKWARDS.** A frequency returning to a
    /// value it held moments before a write, when the write asked for somewhere
    /// else, is not an ordinary observation and is worth a line in the record
    /// even once the guard above stops it reaching the screen (§0.0.1).
    /// </remarks>
    public static bool WouldGoBackwards(
        long readingHz, long tunedFromHz, long? tunedToHz)
        => tunedToHz is { } asked && readingHz == tunedFromHz && asked != readingHz;
}
