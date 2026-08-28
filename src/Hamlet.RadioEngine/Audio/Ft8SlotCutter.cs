namespace Hamlet.RadioEngine.Audio;

/// <summary>One slot of audio, cut on a UTC quarter minute.</summary>
/// <param name="StartUtc">The boundary it opens on, corrected.</param>
/// <param name="FirstSample">Where it starts in the recording.</param>
/// <param name="Audio">The samples.</param>
public sealed record AudioSlot(DateTime StartUtc, int FirstSample, MonoAudio Audio);

/// <summary>Why a cut produced what it produced.</summary>
/// <param name="Slots">The whole slots, oldest first.</param>
/// <param name="Reason">
/// Why nothing was cut, or "" where the cut ran normally.
/// </param>
/// <param name="ShortAtStart">
/// Samples before the first boundary, discarded because the slot they belong to
/// began before the recording did.
/// </param>
/// <param name="ShortAtEnd">
/// Samples after the last whole slot, discarded because the recording stopped
/// part way through one.
/// </param>
/// <remarks>
/// **THE DISCARDS ARE COUNTED AND REPORTED, NOT SWALLOWED** (§0.0.1). A capture
/// pressed at an arbitrary moment almost always begins and ends mid-slot, so
/// most of these will be non-zero and that is correct. What must never happen is
/// a partial slot going through as though it were whole: a decoder handed twelve
/// seconds of a fifteen-second transmission finds nothing, and a reader with no
/// discard count cannot tell that from a band with nobody on it.
/// </remarks>
public sealed record SlotCut(
    IReadOnlyList<AudioSlot> Slots,
    string Reason,
    int ShortAtStart,
    int ShortAtEnd);

/// <summary>
/// Cuts a recording into the fifteen-second slots FT8 transmits in.
/// </summary>
/// <remarks>
/// <para>**FT8 IS SYNCHRONOUS AND THAT IS THE WHOLE OF WHY THIS EXISTS.** Every
/// station on the band starts transmitting on the same quarter minute and stops
/// together, so a slot is the unit a decoder works on. Audio cut anywhere else
/// holds the end of one transmission and the start of the next, and is not
/// decodable however clean it is.</para>
/// <para>**AND THE BOUNDARY IS ON UTC, NOT ON THE PC CLOCK.** A computer running
/// two seconds fast cuts every slot two seconds early, which is a seventh of a
/// transmission missing from the front of each one. So the cut takes the
/// measured clock offset, and **an offset nobody has measured means no slots at
/// all** rather than slots cut where the machine happens to think the minute
/// falls. That would be exactly the guess §0.0 forbids, arriving as an
/// alignment instead of a sentence.</para>
/// <para>**PURE, OVER SAMPLES AND AN ELAPSED TIME.** Nothing here reads a
/// clock: it is handed when the recording ended and works backwards, so the same
/// recording cuts the same way at any hour of any day.</para>
/// </remarks>
public static class Ft8SlotCutter
{
    /// <summary>What is said when there is no offset to cut against.</summary>
    public const string NoOffset =
        "the clock offset has not been measured, so where the fifteen-second "
        + "boundaries fall is not known and nothing was cut";

    /// <summary>What is said when the recording is shorter than one slot.</summary>
    public const string TooShort =
        "the recording is shorter than one whole slot, so there is nothing to cut";

    /// <summary>
    /// Cut a recording into whole slots.
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <param name="endedAtPcUtc">
    /// When the recording ended, by the PC clock. For a capture this is the
    /// press, which is the end of the window.
    /// </param>
    /// <param name="offset">The measured clock offset.</param>
    /// <returns>The slots, and why there are as many as there are.</returns>
    /// <remarks>
    /// **A SHORT SLOT IS DISCARDED RATHER THAN PADDED OR KEPT.** Padding it
    /// would put silence on the air that nobody transmitted, and keeping it
    /// would hand a decoder a fragment and let the empty result read as an empty
    /// band.
    /// </remarks>
    public static SlotCut Cut(
        MonoAudio audio, DateTime endedAtPcUtc, ClockOffset offset)
    {
        ArgumentNullException.ThrowIfNull(audio);

        if (Ft8Slots.TrueUtc(endedAtPcUtc, offset) is not { } endedAt)
        {
            return new SlotCut(Array.Empty<AudioSlot>(), NoOffset, 0, 0);
        }

        var rate = audio.SampleRate;
        var total = audio.Samples.Length;
        var perSlot = (int)Math.Round(Ft8Slots.SlotSeconds * rate);

        if (rate <= 0 || perSlot <= 0 || total < perSlot)
        {
            return new SlotCut(Array.Empty<AudioSlot>(), TooShort, total, 0);
        }

        var startedAt = endedAt.AddSeconds(-(total / (double)rate));

        var boundaries = Ft8Slots.BoundariesBetween(startedAt, endedAt);

        var slots = new List<AudioSlot>();

        foreach (var boundary in boundaries)
        {
            // Where that moment lands in the recording. Rounded rather than
            // truncated, because a half-sample bias at the front of every slot
            // is a bias in the same direction every time.
            var at = (int)Math.Round((boundary - startedAt).TotalSeconds * rate);

            if (at < 0 || at + perSlot > total)
            {
                continue;
            }

            var samples = new float[perSlot];
            Array.Copy(audio.Samples, at, samples, 0, perSlot);

            slots.Add(new AudioSlot(
                boundary, at, new MonoAudio(rate, samples)));
        }

        if (slots.Count == 0)
        {
            return new SlotCut(
                Array.Empty<AudioSlot>(),
                "no whole slot fits inside the recording", total, 0);
        }

        var first = slots[0];
        var last = slots[^1];

        return new SlotCut(
            slots,
            "",
            first.FirstSample,
            total - (last.FirstSample + perSlot));
    }
}
