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
    string Refusal);

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
    /// The decoder to use, or null for one at upstream's own settings.
    /// </param>
    /// <returns>The messages, and why there are as many as there are.</returns>
    /// <exception cref="ArgumentNullException">The audio is null.</exception>
    public static Ft8Reception Read(
        MonoAudio audio,
        DateTime endedAtPcUtc,
        ClockOffset offset,
        Ft8SlotDecoder? decoder = null)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var cut = Ft8SlotCutter.Cut(audio, endedAtPcUtc, offset);

        if (cut.Slots.Count == 0)
        {
            return new Ft8Reception(
                Array.Empty<Ft8Decode>(),
                0,
                0,
                cut.Reason.Length > 0 ? cut.Reason : NoWholeSlot);
        }

        decoder ??= new Ft8SlotDecoder();

        var found = new List<Ft8Decode>();
        var candidates = 0;

        foreach (var slot in cut.Slots)
        {
            var samples = Ft8Resample.ToFt8Rate(slot.Audio).Samples;

            var result = decoder.Decode(samples);

            candidates += result.CandidateCount;

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

        return new Ft8Reception(found, cut.Slots.Count, candidates, "");
    }
}
