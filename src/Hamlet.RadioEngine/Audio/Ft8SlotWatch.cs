namespace Hamlet.RadioEngine.Audio;

/// <summary>One whole slot that has just finished, and the audio it was made of.</summary>
/// <param name="SlotStartUtc">The quarter minute it opened on, corrected.</param>
/// <param name="EndedAtPcUtc">
/// When the slot closed, **by the PC clock**, which is what
/// <see cref="Ft8Reader.Read"/> wants handed to it. It is the corrected boundary
/// with the measured offset taken back off, so the reader's own cut lands on the
/// same sample this watch cut on.
/// </param>
/// <param name="Audio">
/// Exactly fifteen seconds of it, at the rate the tap is holding. **Never short
/// and never padded** — where the ring no longer held the whole slot, no
/// <see cref="Ft8SlotReady"/> is produced at all.
/// </param>
public sealed record Ft8SlotReady(
    DateTime SlotStartUtc, DateTime EndedAtPcUtc, MonoAudio Audio);

/// <summary>What one look at the clock and the tap found.</summary>
/// <param name="Ready">
/// The slot to decode, or null when no whole slot has completed since the last
/// look.
/// </param>
/// <param name="Refusal">
/// Why nothing could be cut, in words for the operator, or "" when the look ran
/// normally — **including the normal case of a look part way through a slot**,
/// which is not a refusal and says nothing.
/// </param>
/// <param name="Skipped">
/// Whole slots that completed and were not decoded, because more than one closed
/// between two looks or because the audio had already fallen out of the ring.
/// **Counted rather than swallowed** (§0.0.1): a tab that quietly missed four
/// slots and a band with nobody on it are different facts.
/// </param>
public sealed record Ft8SlotLook(Ft8SlotReady? Ready, string Refusal, int Skipped)
{
    /// <summary>Nothing has completed, and nothing is wrong.</summary>
    public static Ft8SlotLook Nothing { get; } = new(null, "", 0);
}

/// <summary>
/// Notices when a fifteen-second FT8 slot has just closed, and hands over its
/// audio.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE PIECE THAT SEPARATES HEARING A BAND FROM SAMPLING IT.**
/// Unit 224 proved the whole path from audio to text at four sample rates, but it
/// only ran when somebody pressed a button, so the count of slots decoded without
/// a press was nought. FT8 opens a new slot every fifteen seconds, four a minute
/// and 240 an hour; an operator pressing a button for each one is not watching a
/// band. Everything hard was already built — this only says *when*.</para>
/// <para>**IT IS A FUNCTION OF ITS ARGUMENTS AND READS NO CLOCK OF ITS OWN.**
/// There is no <c>DateTime.UtcNow</c> in this file, deliberately: step 7's first
/// criterion is that slot alignment be asserted against synthesized audio *and a
/// controllable clock*, and a class that reads the wall clock cannot be driven
/// across a boundary by a test. Unit 224 already paid for the alternative — its
/// first draft read the view model's clock property and produced a test that
/// decoded on some evenings, which is not a test.</para>
/// <para>**IT NEVER RETURNS THE SAME SLOT TWICE.** The Digital tab looks four
/// times a second and a slot lasts fifteen, so sixty looks fall inside every slot
/// and exactly one of them may produce a decode. The watch remembers which slot
/// it was last in rather than which it last returned, so nothing turns on a
/// caller remembering to acknowledge anything.</para>
/// <para>**IT NEVER RETURNS A PARTIAL SLOT AND NEVER PADS ONE** (§0.0). The tap
/// keeps thirty seconds, which is two whole slots, so a caller that falls two
/// slots behind is asking for audio that is gone. A decoder handed twelve seconds
/// of a fifteen-second transmission finds nothing and cannot say why, so the
/// wrongness would arrive as silence rather than as an error.</para>
/// <para>**IT SKIPS RATHER THAN STALLS.** A busy machine or a laptop resuming can
/// put two or three boundaries between two looks. The watch takes the most
/// recently completed slot, counts the rest as skipped, and never tries to walk
/// back through audio the ring has dropped.</para>
/// <para>**AN UNMEASURED CLOCK PRODUCES THE CUTTER'S OWN REFUSAL, UNCHANGED.**
/// FT8 needs the PC within about a second of UTC or nothing decodes, and it fails
/// silently — a blank table and a wrong clock look identical, and that is the
/// commonest newcomer failure in this mode. <see cref="ClockOffset"/> already
/// carries the words and the thresholds; nothing here is a second opinion about
/// the clock.</para>
/// <para>**NO DECODING HAPPENS HERE.** <see cref="Ft8Reader.Read"/> turns audio
/// into text and is proven at four sample rates. This is a watch.</para>
/// <para>**NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2). Audio moves
/// out of a ring buffer and into an array.</para>
/// </remarks>
public sealed class Ft8SlotWatch
{
    /// <summary>What is said when the offset is too old to cut against.</summary>
    /// <remarks>
    /// **THE AGE COMES FROM <see cref="ClockOffset.IsStale"/> AND NOT FROM HERE.**
    /// This is the sentence and not the threshold, and the measurement's own words
    /// are appended so the operator is told how old it actually is.
    /// </remarks>
    public const string StaleOffset =
        "the clock offset is too old to cut slots against";

    /// <summary>What is said when the ring no longer holds the whole slot.</summary>
    /// <remarks>
    /// <para>**A SHORT BUFFER IS NOT AN ANSWER** (§0.0). Where the ring does not
    /// hold all fifteen seconds, the honest result is that the slot was missed,
    /// said in words, rather than fourteen seconds of audio decoded as though they
    /// were fifteen.</para>
    /// <para>**AND THIRTY SECONDS TURNS OUT TO BE EXACTLY ENOUGH, WHICH IS WORTH
    /// WRITING DOWN.** The slot the watch asks for ended at most fifteen seconds
    /// ago — a boundary is never further back than that — so the audio wanted
    /// spans at most thirty seconds back from now, which is
    /// <see cref="AudioTap.SecondsKept"/> to the second. A full ring and a stream
    /// that is keeping up therefore always hold the slot, and this sentence is
    /// reachable only while the ring is still filling after start-up. **If the tap
    /// is ever shortened, this stops being true and the watch starts missing
    /// slots.**</para>
    /// </remarks>
    public const string AudioAgedOut =
        "the slot finished longer ago than the audio that is kept, so it could "
        + "not be decoded";

    /// <summary>What is said when nothing is arriving at all.</summary>
    public const string NoAudio =
        "no audio is arriving, so no slot can be cut";

    /// <summary>
    /// The least of a slot's audio that must actually have arrived.
    /// </summary>
    /// <remarks>
    /// <para>**0.98, AND THE TWO PER CENT IS FOR BUFFER-EDGE JITTER AND NOTHING
    /// ELSE.** A slot boundary does not land on a device buffer boundary, so the
    /// first and last buffers of a slot are counted at whichever side of the
    /// edge they fell; at 48 kHz in 20 ms buffers that is under half a per cent
    /// either way. Two per cent is that with room, and it is deliberately not
    /// enough to admit a slot that is actually short.</para>
    /// <para>**IT IS NOT A QUALITY THRESHOLD.** FT8 needs 12.64 s of continuous,
    /// phase-coherent audio. A slot at 0.9 is not a slightly worse slot, it is
    /// a collage of fragments with gaps in it, and no amount of decoder
    /// sensitivity recovers a signal whose phase was cut apart.</para>
    /// </remarks>
    public const double LeastArrival = 0.98;

    /// <summary>What the operator reads when the sound card fell behind.</summary>
    /// <remarks>
    /// **THIS IS THE SENTENCE THAT SHOULD HAVE BEEN ON SCREEN ON 2026-09-03.**
    /// What was there instead was `nothing decoded yet`, which reads as an empty
    /// band and sent three units looking at the decoder. The percentage is
    /// filled in by the caller, because a refusal that cannot say how short it
    /// was is the same unfalsifiable shrug in politer words.
    /// </remarks>
    public const string AudioShort =
        "the sound card delivered {0} of the last fifteen seconds, so this slot "
        + "is fragments and cannot be decoded";

    /// <summary>What is said when the samples have stopped keeping up.</summary>
    /// <remarks>
    /// <para>**THIS IS THE ONE FAULT THE MAPPING BELOW CAN COMMIT, AND IT IS THE
    /// §0.0 FAULT.** The tap holds samples and no timestamps, so the watch's only
    /// anchor is that the newest sample it holds arrived at about this moment. If
    /// the stream stalls, that stops being true: `SamplesSeen` stands still while
    /// the clock runs on, and the last fifteen seconds in the ring would be handed
    /// over labelled with a slot they are not from. **That is a wrong decode
    /// arriving as a right one** — HM-DEC-090 caught the same shape once already,
    /// where a capture went on handing over the same thirty seconds and the
    /// decoder went on reporting what it made of them.</para>
    /// <para>So the watch checks that the audio has kept up with the clock since
    /// its anchor, to within one slot, and refuses rather than guessing. A sound
    /// card whose clock differs from the PC's by a hundred parts per million takes
    /// over forty hours to drift that far, and every decoded slot re-anchors.</para>
    /// </remarks>
    public const string AudioStalled =
        "the audio stopped keeping up with the clock, so which samples belong to "
        + "this slot is not known and nothing was decoded";

    /// <summary>
    /// The slot the watch was in when it last looked, or null before its first
    /// look.
    /// </summary>
    /// <remarks>
    /// **THE FIRST LOOK ARMS AND DECODES NOTHING**, and that is deliberate rather
    /// than a lost slot. When the watch opens its eyes part way through a slot,
    /// the slot before it finished while nothing was watching, and the audio for
    /// it may be anything from complete to absent. Reporting the slot that closes
    /// while the watch is running is a claim the watch can stand behind; reporting
    /// whatever happened to be in the ring at start-up is not.
    /// </remarks>
    private DateTime? _lastSeenSlotStart;

    /// <summary>
    /// A sample index and the moment it was current, against which the audio is
    /// checked for keeping up. Null until any audio has been seen.
    /// </summary>
    private long? _anchorSample;
    private DateTime _anchorPcUtc;

    /// <summary>True once the watch has armed and is reporting boundaries.</summary>
    public bool IsWatching => _lastSeenSlotStart is not null;

    /// <summary>The slot the watch was in at its last look, or null.</summary>
    public DateTime? LastSeenSlotStart => _lastSeenSlotStart;

    /// <summary>
    /// Forget where the watch was, so the next look arms afresh.
    /// </summary>
    /// <remarks>
    /// **CALLED WHEN THE WATCH STOPS LOOKING AND WHEN THE RADIO MOVES.** A watch
    /// that resumes after a gap must not report the slot that closed during the
    /// gap as though it had been watching, and rows from one frequency must not
    /// appear under the same heading as rows from another (§0.0.1).
    /// </remarks>
    public void Rearm()
    {
        _lastSeenSlotStart = null;
        _anchorSample = null;
    }

    /// <summary>
    /// Look once: has a whole slot closed since the last look, and is its audio
    /// still here.
    /// </summary>
    /// <param name="tap">What the decoder is being fed, held for thirty seconds.</param>
    /// <param name="nowPcUtc">
    /// The moment of this look, **by the PC clock**. Passed rather than read, so
    /// the whole watch is a function of its arguments.
    /// </param>
    /// <param name="offset">
    /// How far the PC clock is from UTC, as measured. Unknown or stale produces
    /// words rather than slots.
    /// </param>
    /// <returns>What this look found.</returns>
    /// <exception cref="ArgumentNullException">The tap is null.</exception>
    public Ft8SlotLook Look(AudioTap tap, DateTime nowPcUtc, ClockOffset offset)
    {
        ArgumentNullException.ThrowIfNull(tap);

        if (Ft8Slots.TrueUtc(nowPcUtc, offset) is not { } trueNow)
        {
            // The cutter's own sentence, unchanged, because the operator meets
            // this state through several doors and should read one answer.
            Rearm();
            return new Ft8SlotLook(null, Ft8SlotCutter.NoOffset, 0);
        }

        if (offset.IsStale(nowPcUtc))
        {
            Rearm();
            return new Ft8SlotLook(
                null, $"{StaleOffset} — {offset.Describe(nowPcUtc)}", 0);
        }

        // The anchor is taken the first time there is any audio to anchor to, and
        // renewed on every slot that comes back, so it never accumulates drift.
        if (_anchorSample is null && tap.HasAudio && tap.SampleRate > 0)
        {
            _anchorSample = tap.SamplesSeen;
            _anchorPcUtc = nowPcUtc;
        }

        var current = Ft8Slots.SlotStart(trueNow);

        if (_lastSeenSlotStart is not { } wasIn)
        {
            _lastSeenSlotStart = current;
            return Ft8SlotLook.Nothing;
        }

        if (current <= wasIn)
        {
            // Still inside the slot the last look was in, or the boundary has
            // moved backwards because a fresh clock measurement arrived. Either
            // way there is nothing new and there must be no duplicate: the watch
            // takes the new grid and waits for the next boundary on it.
            _lastSeenSlotStart = current;
            return Ft8SlotLook.Nothing;
        }

        _lastSeenSlotStart = current;

        // How many whole slots closed between the two looks. One of them is the
        // slot that just ended and is about to be decoded; the rest are gone.
        var closed = (int)Math.Round(
            (current - wasIn).TotalSeconds / Ft8Slots.SlotSeconds);
        var skipped = Math.Max(0, closed - 1);

        var rate = tap.SampleRate;

        if (rate <= 0 || !tap.HasAudio)
        {
            return new Ft8SlotLook(null, NoAudio, closed);
        }

        var perSlot = (int)Math.Round(Ft8Slots.SlotSeconds * rate);

        // **THE BOUNDARY, BACK ON THE PC'S OWN CLOCK.** `TrueUtc` adds the
        // offset, so undoing it subtracts. The tap counts samples and knows
        // nothing about time, and the reader wants a PC-clock instant, so the
        // arithmetic crosses here once and in one direction.
        var endedAtPcUtc = current.AddSeconds(-(offset.OffsetSeconds ?? 0));

        // **THE MAPPING FROM A MOMENT TO A SAMPLE INDEX, WHICH DID NOT EXIST
        // BEFORE THIS UNIT.** The tap holds `SamplesSeen` and a rate and no
        // timestamps, so the only anchor available is that the newest sample it
        // holds arrived at about this moment. Everything before it is counted
        // back at the sample rate. That is the same assumption `Ft8SlotCutter`
        // has made about a capture press since work instruction 042 — the end of
        // the buffer is now — stated here rather than left implicit.
        var seen = tap.SamplesSeen;

        // **AND THE ANCHOR IS WHAT KEEPS THAT ASSUMPTION HONEST.** See
        // <see cref="AudioStalled"/>: a stream that has stopped would otherwise
        // hand over old audio wearing the current slot's timestamp.
        if (_anchorSample is { } anchor)
        {
            var expected = anchor + (long)Math.Round(
                (nowPcUtc - _anchorPcUtc).TotalSeconds * rate);

            if (seen < expected - perSlot)
            {
                Rearm();
                return new Ft8SlotLook(null, AudioStalled, closed);
            }
        }

        var sinceEnd = (long)Math.Round(
            (nowPcUtc - endedAtPcUtc).TotalSeconds * rate);

        var lastSample = seen - sinceEnd;
        var firstSample = lastSample - perSlot;

        // `Window` answers null rather than short when the ring has moved past
        // what was asked for, which is what makes the refusal below honest. It
        // takes the tap's own lock, so audio arriving between reading
        // `SamplesSeen` and asking for the window can only make the answer null —
        // never wrong audio, and never a slot from the wrong place.
        var audio = firstSample < 0 ? null : tap.Window(firstSample, perSlot);

        if (audio is null)
        {
            return new Ft8SlotLook(null, AudioAgedOut, closed);
        }

        // **RE-ANCHORED ON EVERY SLOT THAT COMES BACK**, so the check above stays
        // one slot wide against a sound card whose clock is not the PC's rather
        // than widening across a night.
        _anchorSample = seen;
        _anchorPcUtc = nowPcUtc;

        // **AND THE SLOT SAYS WHETHER ITS OWN AUDIO ACTUALLY ARRIVED.** Every
        // check above asks whether the tap holds samples at the right INDEX;
        // none of them asks whether those samples arrived in the fifteen seconds
        // they claim to cover. On 2026-09-03 the tap was filling at 13% of real
        // time, so a window at the right index held about two minutes of
        // fragments wearing one slot's timestamp, and every check passed
        // (HM-DEC-093: the path was uncounted, so nothing could say so).
        var arrival = tap.ArrivalRatioBetween(
            endedAtPcUtc.AddSeconds(-Ft8Slots.SlotSeconds), endedAtPcUtc);

        // NaN is *nobody measured* and never a refusal: a watch that has just
        // started has no marks reaching back a slot, and refusing there would
        // report a fault about a device that is working (§0.0).
        if (!double.IsNaN(arrival) && arrival < LeastArrival)
        {
            Rearm();

            return new Ft8SlotLook(
                null,
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    AudioShort,
                    arrival.ToString("P0", System.Globalization.CultureInfo.InvariantCulture)),
                closed);
        }

        return new Ft8SlotLook(
            new Ft8SlotReady(
                current.AddSeconds(-Ft8Slots.SlotSeconds), endedAtPcUtc, audio),
            "",
            skipped);
    }
}
