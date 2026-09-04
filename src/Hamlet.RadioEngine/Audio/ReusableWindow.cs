namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// A buffer a repeating reader of <see cref="AudioTap"/> owns and reuses, so
/// reading the tap on a timer allocates nothing after the first call.
/// </summary>
/// <remarks>
/// <para>**WHY A READER'S ALLOCATION IS AN AUDIO PROBLEM AND NOT A TIDINESS
/// ONE.** Every one of these reads is a `float[]` of tens of thousands of
/// samples, and anything at or above 85,000 bytes goes on the large object
/// heap. The large object heap is collected only on a generation 2 collection,
/// and a generation 2 collection suspends **every** thread in the process,
/// including the one carrying audio out of the sound card. So a reader that
/// allocates on a timer does not merely cost itself: it periodically stops the
/// writer, which is the same fault work instruction 239 set out to fix in the
/// lock, and it is invisible in a measurement of the lock alone.</para>
/// <para>**THE TRAFFIC IT REPLACES, COUNTED FROM THE CALL SITES.** At 48 kHz
/// the keying meter reads six seconds once a second (1.15 MB/s), the decoder
/// reads eight seconds for the swing and eight for the peak at up to once a
/// second each (3.07 MB/s), the ranking reads four seconds (768 KB a time), and
/// the slot watch reads a fifteen-second slot every fifteen seconds
/// (192 KB/s). None of it survives the call that made it.</para>
/// <para>**THE ONE RULE FOR A CALLER, AND IT IS NOT OPTIONAL.** The
/// <see cref="MonoAudio"/> that comes back wraps a buffer this object owns and
/// will overwrite on the next read through it. Consume it before reading again
/// and never store it. Every caller here reads it and drops it inside the same
/// method, which is what makes this safe here and would not make it safe
/// everywhere.</para>
/// </remarks>
public sealed class ReusableWindow
{
    private float[] _buffer = [];
    private MonoAudio? _held;

    /// <summary>How many samples the buffer holds.</summary>
    public int Capacity => _buffer.Length;

    /// <summary>How many times a read had to size the buffer again.</summary>
    /// <remarks>
    /// **COUNTED, BECAUSE A NUMBER THAT KEEPS CLIMBING MEANS THIS IS NOT
    /// WORKING** (HM-DEC-093). A caller reading a fixed window sizes once and
    /// never again. One that climbs is a caller whose window is not fixed, and
    /// the test asserts on this rather than trusting the design to be what it
    /// was described as.
    /// </remarks>
    public int Sizings { get; private set; }

    /// <summary>One span out of the tap, into the buffer this owns.</summary>
    /// <param name="tap">The tap to read.</param>
    /// <param name="firstSample">The absolute index of the first sample wanted.</param>
    /// <param name="count">How many samples.</param>
    /// <returns>
    /// The audio, valid until the next read through this object, or null where
    /// the tap no longer holds that span.
    /// </returns>
    /// <exception cref="ArgumentNullException">No tap.</exception>
    public MonoAudio? From(AudioTap tap, long firstSample, int count)
    {
        ArgumentNullException.ThrowIfNull(tap);

        if (count <= 0)
        {
            return null;
        }

        Size(count);

        return tap.Window(firstSample, count, _buffer, out var rate)
            ? Wrap(rate)
            : null;
    }

    /// <summary>The newest stretch, into the buffer this owns.</summary>
    /// <param name="tap">The tap to read.</param>
    /// <param name="wanted">How much of it.</param>
    /// <returns>
    /// The audio, valid until the next read through this object, or null where
    /// the tap does not hold that much yet.
    /// </returns>
    /// <exception cref="ArgumentNullException">No tap.</exception>
    public MonoAudio? Tail(AudioTap tap, TimeSpan wanted)
    {
        ArgumentNullException.ThrowIfNull(tap);

        var rate = tap.SampleRate;

        if (rate <= 0)
        {
            return null;
        }

        var count = (int)Math.Round(wanted.TotalSeconds * rate);

        if (count <= 0)
        {
            return null;
        }

        Size(count);

        return tap.Tail(wanted, _buffer, out _, out var taken)
            ? Wrap(taken)
            : null;
    }

    /// <summary>Make the buffer exactly the length of the read.</summary>
    /// <remarks>
    /// **EXACTLY, RATHER THAN AT LEAST, AND THE DIFFERENCE IS CORRECTNESS AND
    /// NOT THRIFT.** A buffer longer than the read would have to come back as a
    /// slice of itself, and a slice is a copy: the audio handed out would be
    /// a snapshot of the buffer taken at slice time, and the next read - which
    /// fills the buffer and not the copy - would leave the caller holding the
    /// PREVIOUS window while everything about it says it is the current one.
    /// That is the torn-buffer fault of task 2 arriving by a different road,
    /// and this is the line that shuts it (§0.0).
    /// </remarks>
    private void Size(int count)
    {
        if (_buffer.Length == count)
        {
            return;
        }

        _buffer = new float[count];
        _held = null;
        Sizings++;
    }

    /// <summary>
    /// Present the buffer as audio, without allocating where nothing changed.
    /// </summary>
    /// <remarks>
    /// **A `MonoAudio` IS SMALL AND IT IS STILL NOT ALLOCATED PER CALL.** The
    /// record is a few dozen bytes, so it would never reach the large object
    /// heap on its own - but a reader running once a second for an evening is
    /// tens of thousands of them, and holding one lets the test assert
    /// **nothing allocated**, which is a proposition that cannot drift, rather
    /// than a threshold somebody has to keep re-judging.
    /// </remarks>
    private MonoAudio Wrap(int rate)
    {
        if (_held is { } held && held.SampleRate == rate)
        {
            return held;
        }

        _held = new MonoAudio(rate, _buffer);

        return _held;
    }
}
