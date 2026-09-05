using System;
using System.Collections.Generic;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The sibling's decode surface. It runs the port's per-candidate loop itself, through the port's
/// public members, so that an ordered statistics stage has somewhere to sit.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why the loop is reproduced rather than delegated to.</b> OSD has to run at the exact point
/// where a candidate fails parity - inside <c>Ft8SlotDecoder.Decode(Ft8Waterfall)</c>'s loop, after
/// <c>Ft8CodewordDecoder.Decode</c> has returned <c>ParityNeverSatisfied</c> and before the loop moves
/// to the next candidate. There is nowhere else to put it: wrapping the port's <c>Decode</c> only sees
/// the finished <c>Ft8SlotResult</c>, by which time every refused candidate is gone. This is route A
/// of <c>docs/unit245-deep-seam.md</c> §4, which unit 245 measured working, and it needs no
/// <c>InternalsVisibleTo</c> and no change to the port.
/// </para>
/// <para>
/// <b>WITH <see cref="Osd"/> NULL THIS IS AN EXACT REPRODUCTION, AND THAT IS ENFORCED RATHER THAN
/// CLAIMED.</b> Same search, one <see cref="Ft8CallsignCache"/> for the slot, same extract, same
/// normalise, same gate, same five counts against the same statuses, same de-duplication key, same
/// message limit. <c>Ft8DeepIdentityTests</c> compares the whole <c>Ft8SlotResult</c> - all five
/// counts and every message's text, candidate, frequency and dt, in order - against
/// <see cref="Ft8SlotDecoder"/> over one whole 51-trial ladder block at -19 dB, one at -21 dB, and the
/// committed capture. <b>Without that test a difference between the scoreboard's two sibling columns
/// would no longer be attributable to OSD, and the seam would stop paying for itself.</b>
/// </para>
/// <para>
/// <b>Nothing here decides that a message is real.</b> Every codeword this type returns has been
/// through <c>Ft8CodewordDecoder.Decode</c> and past the port's own parity gate and CRC-14 gate.
/// There is no checksum in this library and there is no acceptance rule in this library.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
public sealed class Ft8DeepSlotDecoder
{
    private readonly Ft8SlotDecoder _port;
    private readonly Ft8SyncSearch _search;

    /// <summary>
    /// Builds a sibling decoder over an <see cref="Ft8SlotDecoder"/> constructed with these same
    /// parameters.
    /// </summary>
    /// <param name="geometry">The extents to analyse to. Defaults to the port's own default.</param>
    /// <param name="search">The search to find candidates with. Defaults to the port's own.</param>
    /// <param name="messageLimit">The most messages one slot returns.</param>
    /// <param name="maxIterations">How hard the correction tries per candidate.</param>
    /// <param name="osd">
    /// How hard the ordered statistics stage tries, or <see langword="null"/> - the default - for the
    /// port's behaviour exactly.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The message limit is negative, or the iteration count is negative. <b>Thrown by the port</b>,
    /// with the port's own wording, because this constructor does not check what it is about to hand
    /// over: a second copy of a refusal is a copy that drifts.
    /// </exception>
    public Ft8DeepSlotDecoder(
        Ft8WaterfallGeometry? geometry = null,
        Ft8SyncSearch? search = null,
        int messageLimit = Ft8SlotDecoder.DefaultMessageLimit,
        int maxIterations = LdpcDecoder.DefaultMaxIterations,
        Ft8DeepOsdSettings? osd = null)
    {
        var used = search ?? new Ft8SyncSearch();
        _port = new Ft8SlotDecoder(geometry, used, messageLimit, maxIterations);
        _search = used;
        Osd = osd;
    }

    /// <summary>Builds a sibling decoder over a port decoder somebody else constructed.</summary>
    /// <param name="port">The decoder whose geometry, limits and refusals this one takes.</param>
    /// <param name="search">
    /// The search to reproduce the port's loop with. <see langword="null"/> builds one from the port's
    /// own <see cref="Ft8SlotDecoder.CandidateLimit"/> and <see cref="Ft8SlotDecoder.MinimumScore"/>.
    /// </param>
    /// <param name="osd">
    /// How hard the ordered statistics stage tries, or <see langword="null"/> for the port's behaviour
    /// exactly.
    /// </param>
    /// <exception cref="ArgumentNullException">The port decoder is null.</exception>
    /// <remarks>
    /// <b>The port does not expose the search it was built with</b>, only its candidate limit and its
    /// minimum score, so a search rebuilt from those two carries this library's default block-offset
    /// sweep. A caller who gave the port a search with a non-default sweep must pass the same search
    /// here, and this is said out loud rather than left to be discovered as a quiet divergence.
    /// </remarks>
    public Ft8DeepSlotDecoder(
        Ft8SlotDecoder port,
        Ft8SyncSearch? search = null,
        Ft8DeepOsdSettings? osd = null)
    {
        ArgumentNullException.ThrowIfNull(port);
        _port = port;
        _search = search ?? new Ft8SyncSearch(port.CandidateLimit, port.MinimumScore);
        Osd = osd;
    }

    /// <summary>
    /// The port decoder this one takes its geometry, its limits and its refusals from.
    /// </summary>
    public Ft8SlotDecoder Port => _port;

    /// <summary>
    /// <b>How hard the ordered statistics stage tries, or <see langword="null"/> for off.</b> Null is
    /// the default and means this decoder reproduces the port exactly.
    /// </summary>
    public Ft8DeepOsdSettings? Osd { get; }

    /// <summary>The extents this decoder analyses to. The port's.</summary>
    public Ft8WaterfallGeometry Geometry => _port.Geometry;

    /// <summary>The most messages one slot returns. The port's.</summary>
    public int MessageLimit => _port.MessageLimit;

    /// <summary>How hard the correction tries per candidate. The port's.</summary>
    public int MaxIterations => _port.MaxIterations;

    /// <summary>The candidate limit the search this decoder uses will return.</summary>
    public int CandidateLimit => _search.CandidateLimit;

    /// <summary>The minimum sync score the search this decoder uses will keep.</summary>
    public int MinimumScore => _search.MinimumScore;

    /// <summary>
    /// Decodes one slot of audio: analyse, search, and try every candidate in rank order.
    /// </summary>
    /// <param name="samples">The slot's audio. At least one block long.</param>
    /// <exception cref="ArgumentException">The signal is shorter than one block.</exception>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples) =>
        Decode(new Ft8Monitor(Geometry).Analyse(samples));

    /// <summary>Decodes one slot from a waterfall that has already been built.</summary>
    /// <param name="waterfall">The spectrogram of one slot.</param>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    /// <remarks>
    /// <b>This is <c>Ft8SlotDecoder.Decode(Ft8Waterfall)</c>, stage for stage, through public
    /// members.</b> The stages, in order, are the search's <c>Find</c>, one
    /// <see cref="Ft8CallsignCache"/> for the slot, then per candidate
    /// <c>Ft8SoftSymbols.Extract</c>, <c>Ft8SoftSymbols.Normalise</c> and
    /// <c>Ft8CodewordDecoder.Decode</c>; the parity count takes every status that is not
    /// <c>ParityNeverSatisfied</c>, the checksum count takes <c>Decoded</c> and
    /// <c>MessageNotReadable</c>, and only <c>Decoded</c> goes on. The de-duplication key is the first
    /// 77 bits of the codeword, recovered the way the port recovers it - by re-running
    /// <c>LdpcDecoder.Decode</c> over the same ratios - and the message limit stops adding without
    /// stopping the loop.
    /// </remarks>
    public Ft8SlotResult Decode(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var candidates = _search.Find(waterfall);

        // ONE CACHE FOR THE SLOT, as the port has it: a callsign heard in full at candidate 3 can be
        // resolved from its hash at candidate 40, and nothing carries over to the next slot.
        var cache = new Ft8CallsignCache();

        var messages = new List<Ft8SlotMessage>();
        var seen = new List<byte[]>();

        var paritySatisfied = 0;
        var checksumPassed = 0;
        var becameText = 0;
        var duplicates = 0;

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios, cache, MaxIterations);

            // WHERE THE ORDERED STATISTICS STAGE GOES, and the only place it can go: belief
            // propagation has just given up on a candidate whose answer may still be reachable.
            // Task 2 leaves it empty on purpose - nothing new decodes in this version, and the
            // identity test above is what proves it.

            if (result.Status != Ft8CodewordStatus.ParityNeverSatisfied)
            {
                paritySatisfied++;
            }

            if (result.Status is Ft8CodewordStatus.Decoded or Ft8CodewordStatus.MessageNotReadable)
            {
                checksumPassed++;
            }

            if (result.Status != Ft8CodewordStatus.Decoded)
            {
                continue;
            }

            becameText++;

            // The port's de-duplication key, recovered the port's way. The gate does not hand back
            // the bits it accepted, so they are read out of a second run of the same deterministic
            // decoder over the same ratios. It is not a second checksum check.
            LdpcDecoder.Decode(ratios, codeword, MaxIterations);
            var key = codeword[..Ft8Payload.MessageBits];

            if (AlreadySeen(seen, key))
            {
                duplicates++;
                continue;
            }

            if (messages.Count >= MessageLimit)
            {
                // The port's own divergence from upstream, reproduced: stop adding, do not stop the
                // loop, and do not count it as anything.
                continue;
            }

            seen.Add(key);
            messages.Add(new Ft8SlotMessage(candidate, result));
        }

        return new Ft8SlotResult(
            candidates.Count, paritySatisfied, checksumPassed, becameText, duplicates, messages);
    }

    private static bool AlreadySeen(List<byte[]> seen, ReadOnlySpan<byte> key)
    {
        foreach (var previous in seen)
        {
            if (key.SequenceEqual(previous))
            {
                return true;
            }
        }

        return false;
    }
}
