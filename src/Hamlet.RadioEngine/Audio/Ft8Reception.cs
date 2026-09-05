using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;

namespace Hamlet.RadioEngine.Audio;

/// <summary>One message that came out of one slot.</summary>
/// <param name="SlotStartUtc">The quarter minute the slot opened on, corrected.</param>
/// <param name="OffsetSeconds">
/// How far into the slot the transmission's first symbol began — the `dt` column.
/// </param>
/// <param name="FrequencyHz">The lowest of the eight tones, in the audio passband.</param>
/// <param name="SyncScore">
/// How strongly the Costas pattern matched, as the search counted it. **Not a
/// signal-to-noise ratio and never displayed as one** — see
/// <see cref="Ft8Reception"/>.
/// </param>
/// <param name="Message">The text, exactly as it was sent.</param>
/// <remarks>
/// **THE TEXT IS NOT INTERPRETED HERE AND IS NOT INTERPRETED ANYWHERE YET**
/// (`CLAUDE.md` §12.1). What a message means in ordinary words is Tim's to word,
/// so this record carries what the decoder produced and nothing derived from it.
/// </remarks>
public sealed record Ft8Decode(
    DateTime SlotStartUtc,
    double OffsetSeconds,
    double FrequencyHz,
    int SyncScore,
    string Message);

/// <summary>How far one slot's candidates got, stage by stage.</summary>
/// <param name="SlotStartUtc">The quarter minute the slot opened on, corrected.</param>
/// <param name="CandidateCount">Places the search returned.</param>
/// <param name="ParitySatisfiedCount">Of those, how many reached a valid codeword.</param>
/// <param name="ChecksumPassedCount">Of those, how many carried their own checksum.</param>
/// <param name="BecameTextCount">Of those, how many became words.</param>
/// <param name="DuplicateCount">
/// Of those, how many repeated a message already returned from this slot. Expected
/// and not a defect: a strong transmission produces several candidates and every
/// one of them decodes.
/// </param>
/// <param name="TopSyncScores">
/// The highest Costas match counts the search saw in this slot, strongest first, at
/// most three. **NOT SIGNAL-TO-NOISE RATIOS** (`CLAUDE.md` §0.0) — they are counts
/// of how far the Costas pattern stood above the average of the eight tones, in no
/// units, calibrated against nothing.
/// </param>
/// <param name="SampleRate">
/// The rate the slot's audio arrived at, **before** the resampler put it on the
/// twelve kilohertz grid. It is here because a record that does not describe the
/// path it ran on is broken in the place nobody checks (`CLAUDE.md` §0.0.1), and
/// because a sound card at an unexpected rate is one of the things a slot that
/// found nothing could be.
/// </param>
/// <remarks>
/// <para>**THESE FOUR NUMBERS NAME THE STAGE THAT REFUSED**, which is the whole
/// reason they are carried instead of discarded. Candidates at zero is a front end,
/// an audio device, a routing, a mode or a filter, and no decoder change touches it.
/// Candidates present with parity at zero is the soft symbols or the belief
/// propagation. Parity present with checksum at zero is a codeword that is not a
/// message. Checksum present with text at zero is the message layer.</para>
/// <para>**THEY COUNT AND THEY DO NOT INTERPRET** (`CLAUDE.md` §12.1). Nothing here
/// concludes that the band was quiet, that a station was weak, or that anything was
/// said. Four integers and a slot boundary.</para>
/// </remarks>
public sealed record Ft8SlotCensus(
    DateTime SlotStartUtc,
    int CandidateCount,
    int ParitySatisfiedCount,
    int ChecksumPassedCount,
    int BecameTextCount,
    int DuplicateCount,
    IReadOnlyList<int> TopSyncScores,
    int SampleRate)
{
    /// <summary>How loud the audio this slot was cut from was (unit 236).</summary>
    /// <remarks>
    /// <para>**ADDED RATHER THAN SUBSTITUTED**, on unit 233's own precedent. Every
    /// member above keeps its meaning and every construction site above keeps
    /// compiling; this sits beside them and carries the one thing they could never
    /// say.</para>
    /// <para>**AND THE THING THEY COULD NEVER SAY IS WHETHER THERE WAS ANY AUDIO
    /// AT ALL.** Eight members describe the decode. A sound card handing over
    /// digital silence, a radio with its USB cable out, a laptop microphone in a
    /// quiet room and a twenty metre band with no decodable FT8 in it all produce
    /// the same eight numbers, and on 2026-09-03 the phase's closing line was
    /// performed at the radio and the record could not separate them.</para>
    /// <para>**IT IS A LEVEL AND NOT A SIGNAL-TO-NOISE RATIO** (`CLAUDE.md` §0.0),
    /// and <see cref="Ft8SlotLevel"/> says so at length.</para>
    /// <para>Defaults to <see cref="Ft8SlotLevel.None"/>, which is what a census
    /// line built by a caller that had no audio to measure has.</para>
    /// </remarks>
    public Ft8SlotLevel Level { get; init; } = Ft8SlotLevel.None;

    /// <summary>Which decoder read this slot, and which stages were on.</summary>
    /// <remarks>
    /// <para>**WITHOUT IT, EVERY CAPTURE FROM TONIGHT ONWARD IS
    /// UNATTRIBUTABLE.** Six sidecars from 2026-09-03 are readable today only
    /// because they recorded their own conditions, and this is the same
    /// discipline: from unit 249 there is more than one decoder this project
    /// might have used, and a count of candidates means a different thing
    /// depending on which one produced it.</para>
    /// <para>**ADDED RATHER THAN SUBSTITUTED**, on the precedent directly above.
    /// Every member keeps its meaning and every construction site keeps
    /// compiling.</para>
    /// <para>Defaults to <see cref="Ft8DecoderIdentity.Unrecorded"/>, which is
    /// what a census built by a caller that did not say has - and it says so
    /// rather than naming a decoder it is guessing at (§0.0).</para>
    /// </remarks>
    public Ft8DecoderIdentity Decoder { get; init; } = Ft8DecoderIdentity.Unrecorded;
}

/// <summary>Which decoder read a slot, and what was turned on in it.</summary>
/// <param name="Name">
/// The decoder: <c>Ft8Sharp</c> for the faithful port, <c>Ft8Sharp.Deep</c> for
/// the sibling, or an empty string where nobody recorded it.
/// </param>
/// <param name="FineSync">Whether the fine sync stage was on.</param>
/// <param name="OrderedStatistics">Whether the ordered statistics stage was on.</param>
/// <remarks>
/// <para>**IT NAMES WHAT RAN AND INFERS NOTHING FROM IT.** It does not say the
/// decode was better for having a stage on, or worse for not; those are the
/// ladder's to say, over hundreds of trials, and a single slot cannot support
/// either claim (§0.0).</para>
/// <para>**AN UNRECORDED DECODER SAYS SO.** A census assembled by something that
/// did not know which decoder ran carries <see cref="Unrecorded"/>, and the
/// surfaces print that rather than defaulting to a name. Naming the port
/// because it used to be the only one would put a false attribution in the one
/// record that exists to settle attribution.</para>
/// </remarks>
public readonly record struct Ft8DecoderIdentity(
    string Name,
    bool FineSync,
    bool OrderedStatistics)
{
    /// <summary>Nobody said which decoder ran.</summary>
    public static Ft8DecoderIdentity Unrecorded { get; } = new("", false, false);

    /// <summary>The faithful port, whose stages do not exist.</summary>
    public static Ft8DecoderIdentity Port { get; } = new("Ft8Sharp", false, false);

    /// <summary>True where a decoder was actually named.</summary>
    public bool IsRecorded => Name.Length > 0;

    /// <summary>What this says on a line somebody reads.</summary>
    /// <returns>One phrase, naming the decoder and its stages.</returns>
    public override string ToString()
    {
        if (!IsRecorded)
        {
            return "not recorded";
        }

        if (!FineSync && !OrderedStatistics)
        {
            return Name;
        }

        var stages = (FineSync, OrderedStatistics) switch
        {
            (true, true) => "fine sync and ordered statistics",
            (true, false) => "fine sync",
            _ => "ordered statistics",
        };

        return Name + " with " + stages;
    }
}

/// <summary>What a stretch of captured audio gave up.</summary>
/// <param name="Decodes">The messages, oldest slot first.</param>
/// <param name="SlotsDecoded">How many whole slots were cut and run.</param>
/// <param name="CandidatesFound">
/// Places the search returned across those slots. **Candidates are not messages**
/// and none of them reaches the screen; the count is here so a slot that found
/// twenty signals and read none of them is distinguishable from a dead band.
/// </param>
/// <param name="Refusal">
/// Why nothing could be run, in words for the operator, or "" when the path ran.
/// </param>
public sealed record Ft8Reception(
    IReadOnlyList<Ft8Decode> Decodes,
    int SlotsDecoded,
    int CandidatesFound,
    string Refusal)
{
    /// <summary>The census, one entry per slot that was cut and run, oldest first.</summary>
    /// <remarks>
    /// <para>**ADDED RATHER THAN SUBSTITUTED** (unit 233). <see cref="CandidatesFound"/>
    /// stays exactly where it was and means exactly what it meant, because existing
    /// callers and tests read it; this sits beside it and carries the four counts the
    /// join used to throw away at the sum.</para>
    /// <para>**EMPTY WHERE NOTHING RAN**, which is the refusal case and is not the
    /// same as a slot that ran and found nothing. A slot that ran always has an entry
    /// here, all zeroes if that is what it found.</para>
    /// </remarks>
    public IReadOnlyList<Ft8SlotCensus> Slots { get; init; } = Array.Empty<Ft8SlotCensus>();

    /// <summary>The clock offset this reading was cut against.</summary>
    /// <remarks>
    /// **CARRIED SO THE RECORD CAN SAY WHICH CLOCK IT BELIEVED** (`CLAUDE.md`
    /// §0.0.1). A clock a second or two out and a receiver that hears nothing look
    /// identical from the outside, and the only thing that separates them after the
    /// fact is knowing what was applied. Defaults to
    /// <see cref="ClockOffset.Unknown"/>, which is what a reading that never cut a
    /// slot has.
    /// </remarks>
    public ClockOffset Offset { get; init; } = ClockOffset.Unknown;
}

/// <summary>
/// Takes a stretch of captured audio and returns the FT8 messages in it.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE JOIN, AND IT IS DELIBERATELY THIN.** Everything hard was
/// built and measured inside `Ft8Sharp` over five steps; everything about clocks
/// and slot boundaries was built in <see cref="Ft8SlotCutter"/> in August. This
/// file owns the three lines between them — cut on the quarter minute, put each
/// slot on the twelve kilohertz grid, hand it over — and owns no decoding of its
/// own.</para>
/// <para>**NO SIGNAL-TO-NOISE RATIO IS PRODUCED, AND NONE IS INVENTED** (§0.0).
/// The library returns a sync score, which counts how far the Costas pattern
/// stood above the average of the eight tones; it is not decibels and it is not
/// calibrated against anything. A plausible number in a column headed `snr`
/// would be read as a measurement by every reader of the screen, which is
/// exactly the fault §0.0 names.</para>
/// <para>**AN UNKNOWN CLOCK MEANS NO DECODES RATHER THAN GUESSED ONES.** The cut
/// refuses without a measured offset and says why, and that sentence is what
/// reaches the operator — a blank table and a wrong clock are the commonest
/// newcomer failure in this mode and they look identical.</para>
/// <para>**PURE, AND NOTHING HERE REACHES A TRANSMITTER** (`CLAUDE.md` §0.2).
/// Audio in, text out.</para>
/// </remarks>
public static class Ft8Reader
{
    /// <summary>What is said when the audio holds no whole slot.</summary>
    public const string NoWholeSlot =
        "there is not a whole fifteen-second slot in what was kept, so there was "
        + "nothing to decode";

    /// <summary>Decode every whole slot in a recording.</summary>
    /// <param name="audio">The recording, at whatever rate it was captured.</param>
    /// <param name="endedAtPcUtc">
    /// When the recording ended, by the PC clock. For a capture press that is the
    /// press itself, which is the end of the window.
    /// </param>
    /// <param name="offset">The measured clock offset.</param>
    /// <param name="decoder">
    /// The decoder to use, or null for Deep with both stages on.
    /// </param>
    /// <returns>The messages, and why there are as many as there are.</returns>
    /// <exception cref="ArgumentNullException">The audio is null.</exception>
    public static Ft8Reception Read(
        MonoAudio audio,
        DateTime endedAtPcUtc,
        ClockOffset offset,
        Ft8DeepSlotDecoder? decoder = null)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var cut = Ft8SlotCutter.Cut(audio, endedAtPcUtc, offset);

        if (cut.Slots.Count == 0)
        {
            return new Ft8Reception(
                Array.Empty<Ft8Decode>(),
                0,
                0,
                cut.Reason.Length > 0 ? cut.Reason : NoWholeSlot)
            {
                Offset = offset,
            };
        }

        // **HAMLET DECODES THROUGH `Ft8Sharp.Deep` SINCE UNIT 249** (Tim's
        // ruling, 2026-09-05), with fine sync and ordered statistics both on.
        //
        // **DEEP IS A PROVEN SUPERSET RATHER THAN A DIFFERENT DECODER.** Unit
        // 246 asserted whole-result identity with the port over 69 reference
        // recordings and 801 messages, and with both settings null Deep is the
        // port byte for byte. What the two stages add, measured at -21 dB over
        // 306 trials: 13 of 306 to 33 of 306 on the grid, **0 wrong either
        // way** - and at the centre of a waterfall cell, which is where a real
        // station lands because nothing on 14.074 arranges itself on Hamlet's
        // analysis grid, 0 of 306 to 3 of 306.
        //
        // **BOTH OF THE PORT'S GATES STAY IN THE PATH.** Parity and CRC-14 are
        // the port's own and every message below has passed them, whatever
        // route its codeword took to get there. Deep recovers candidates the
        // port gives up on; it does not lower the bar they have to clear.
        decoder ??= new Ft8DeepSlotDecoder(
            osd: Ft8DeepOsdSettings.Default,
            fineSync: Ft8DeepFineSyncSettings.Default);

        // A mirror of the decoder's own search, built from the limit and minimum it
        // publishes. It exists for one thing the result type cannot give: the highest
        // Costas match counts in a slot that decoded NOTHING, where there is no
        // message to read a candidate off. The counts below are the decoder's own.
        var search = new Ft8SyncSearch(decoder.CandidateLimit, decoder.MinimumScore);
        var monitor = new Ft8Monitor(decoder.Geometry);

        // **WHAT THIS READ WAS DECODED BY, TAKEN FROM THE DECODER RATHER
        // THAN FROM WHAT THIS METHOD DEFAULTS TO.** A caller may pass its own,
        // and a record that named the default would be describing a decoder
        // that did not run.
        var identity = new Ft8DecoderIdentity(
            "Ft8Sharp.Deep",
            decoder.FineSync is not null,
            decoder.Osd is not null);

        var found = new List<Ft8Decode>();
        var census = new List<Ft8SlotCensus>();
        var candidates = 0;

        foreach (var slot in cut.Slots)
        {
            var samples = Ft8Resample.ToFt8Rate(slot.Audio).Samples;

            var waterfall = monitor.Analyse(samples);
            var places = search.Find(waterfall);

            // **THE SAMPLES ENTRY POINT, NOT THE WATERFALL ONE, AND THE
            // DIFFERENCE IS THE WHOLE UNIT.** `Decode(Ft8Waterfall)` hands
            // Deep's loop an empty span, and a waterfall has no phase in it and
            // no samples behind it, so there is nothing to re-sync from.
            // Measured in unit 249 task 1 on the example slot: through the
            // waterfall, fine sync refused all 42 candidates for want of
            // samples; through the samples it re-synced 42 and accepted 14.
            //
            // **SO A READER THAT KEPT CALLING THE WATERFALL OVERLOAD WOULD PAY
            // FOR DEEP AND GET NONE OF THE OFF-GRID REACH THE PHASE WAS FOR.**
            //
            // The waterfall above is still built and still used: `places` is
            // read from it for the top Costas scores, which the result type
            // cannot give for a slot that decoded nothing. Deep analyses its
            // own waterfall internally from the same samples, which is one
            // extra analysis a slot and is inside the budget with room to
            // spare - 210 ms of 15,000.
            var result = decoder.Decode(samples);

            candidates += result.CandidateCount;

            census.Add(new Ft8SlotCensus(
                slot.StartUtc,
                result.CandidateCount,
                result.ParitySatisfiedCount,
                result.ChecksumPassedCount,
                result.BecameTextCount,
                result.DuplicateCount,
                TopScores(places),
                slot.Audio.SampleRate)
            {
                Decoder = identity,

                // **MEASURED FROM `slot.Audio`, NOT FROM `samples`** (unit 236).
                // The resampler above is one of the things a slot that found
                // nothing could be, and a level taken downstream of a suspect
                // cannot clear it. This is the same reason `SampleRate` on the
                // line above is the rate the audio arrived at rather than the
                // twelve kilohertz grid it was put on.
                Level = Ft8SlotLevel.Of(slot.Audio),
            });

            foreach (var message in result.Messages)
            {
                found.Add(new Ft8Decode(
                    slot.StartUtc,
                    message.TimeSeconds(decoder.Geometry),
                    message.FrequencyHz(decoder.Geometry),
                    message.Candidate.Score,
                    message.Text));
            }
        }

        return new Ft8Reception(found, cut.Slots.Count, candidates, "")
        {
            Slots = census,
            Offset = offset,
        };
    }

    /// <summary>The three highest Costas match counts, strongest first.</summary>
    /// <remarks>
    /// The search returns strongest first already, so this takes the front of the list
    /// rather than sorting it again. Three, because a fourth number tells a reader
    /// nothing a third did not.
    /// </remarks>
    private static IReadOnlyList<int> TopScores(IReadOnlyList<Ft8Candidate> places)
    {
        var take = Math.Min(3, places.Count);
        var scores = new int[take];

        for (var i = 0; i < take; i++)
        {
            scores[i] = places[i].Score;
        }

        return scores;
    }
}
