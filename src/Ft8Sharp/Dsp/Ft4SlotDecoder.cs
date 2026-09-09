using System;
using System.Collections.Generic;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Dsp;

/// <summary>
/// <b>The whole FT4 path: seven and a half seconds of audio in, the messages that were in it out.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Beside <see cref="Ft8SlotDecoder"/> rather than a protocol flag on it.</b> Five of the six
/// things this wires together are FT4's own — the geometry, the search, the extraction, the layout
/// and the payload exclusive-OR — and the rest is shared and is not re-implemented here: the
/// monitor, the waterfall, the normalisation, the belief propagation, the checksum gate, the
/// unpacker and the callsign cache are all the same code FT8 runs.
/// </para>
/// <para>
/// <b>NOTHING FAILING PARITY OR THE CHECKSUM IS RETURNED, and there is no route around the gate.</b>
/// Every candidate goes through <see cref="Ft8CodewordDecoder.Decode"/> and only a
/// <see cref="Ft8CodewordStatus.Decoded"/> reaches the list — not with a flag, not with a
/// confidence, not as a partial. <c>CLAUDE.md</c> §0.0 / HM-DEC-009.
/// </para>
/// <para>
/// <b>One callsign cache per slot, created per call, never a static singleton</b>, for the reason
/// unit 208 ruled: a decode that depends on what some other slot happened to contain is not
/// reproducible.
/// </para>
/// <para>
/// <b>Deterministic.</b> The same samples give the same messages in the same order, every time.
/// Nothing here reads a clock, a random source, an environment variable or a dictionary's
/// enumeration order, and nothing runs in parallel.
/// </para>
/// <para>
/// <b>Nothing plays and nothing opens a device.</b> The caller hands over samples it obtained however
/// it likes, and this returns text. <c>CLAUDE.md</c> §0.2.
/// </para>
/// </remarks>
public sealed class Ft4SlotDecoder
{
    /// <summary>The most messages one slot returns, defaulting to upstream's application's limit.</summary>
    public const int DefaultMessageLimit = Ft8SlotDecoder.DefaultMessageLimit;

    private readonly Ft4SyncSearch _search;

    /// <summary>Builds a decoder for one geometry.</summary>
    /// <param name="geometry">The extents to analyse to. Defaults to FT4's own at 12 kHz.</param>
    /// <param name="search">The search to find candidates with. Defaults to FT4's own.</param>
    /// <param name="messageLimit">The most messages one slot returns.</param>
    /// <param name="maxIterations">How hard the correction tries per candidate.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The message limit is negative, or the iteration count is negative.
    /// </exception>
    public Ft4SlotDecoder(
        Ft4WaterfallGeometry? geometry = null,
        Ft4SyncSearch? search = null,
        int messageLimit = DefaultMessageLimit,
        int maxIterations = LdpcDecoder.DefaultMaxIterations)
    {
        if (messageLimit < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageLimit),
                messageLimit,
                "A message limit is how many messages to return and cannot be negative. Zero is "
                + "allowed and means the path runs and returns nothing.");
        }

        if (maxIterations < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxIterations),
                maxIterations,
                "An iteration count cannot be negative. Zero is allowed and means the correction "
                + "judges the raw ratios without passing a single message.");
        }

        Geometry = geometry ?? new Ft4WaterfallGeometry();
        _search = search ?? new Ft4SyncSearch();
        MessageLimit = messageLimit;
        MaxIterations = maxIterations;
    }

    /// <summary>The extents this decoder analyses to.</summary>
    public Ft4WaterfallGeometry Geometry { get; }

    /// <summary>The most messages one slot returns.</summary>
    public int MessageLimit { get; }

    /// <summary>How hard the correction tries per candidate.</summary>
    public int MaxIterations { get; }

    /// <summary>The candidate limit the search this decoder uses will return.</summary>
    public int CandidateLimit => _search.CandidateLimit;

    /// <summary>The minimum sync score the search this decoder uses will keep.</summary>
    public int MinimumScore => _search.MinimumScore;

    /// <summary>The lowest block offset the search this decoder uses will sweep.</summary>
    public int FirstBlockOffset => _search.FirstBlockOffset;

    /// <summary>The highest block offset the search this decoder uses will sweep, inclusive.</summary>
    public int LastBlockOffset => _search.LastBlockOffset;

    /// <summary>Decodes one slot of audio: analyse, search, and try every candidate in rank order.</summary>
    /// <param name="samples">
    /// The slot's audio at <see cref="Ft8WaterfallGeometry.SampleRate"/>. At least one block long.
    /// </param>
    /// <exception cref="ArgumentException">The signal is shorter than one block.</exception>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples) =>
        Decode(new Ft8Monitor(Geometry).Analyse(samples));

    /// <summary>Decodes one slot from a waterfall that has already been built.</summary>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public Ft8SlotResult Decode(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var candidates = _search.Find(waterfall);

        var cache = new Ft8CallsignCache();
        var messages = new List<Ft8SlotMessage>();
        var seen = new List<byte[]>();

        var paritySatisfied = 0;
        var checksumPassed = 0;
        var becameText = 0;
        var duplicates = 0;

        var ratios = new float[Ft4SoftSymbols.RatioCount];
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var candidate in candidates)
        {
            Ft4SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(
                ratios, cache, MaxIterations, Ft8Tables.Ft4XorSequence);

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

            // THE DE-DUPLICATION KEY IS THE MESSAGE ITSELF, which is upstream's rule. The gate does
            // not hand back the bits it accepted, so they are recovered by running the same
            // deterministic decoder over the same ratios; it is not a second CRC check and it costs
            // one belief propagation per SUCCESSFUL decode only.
            //
            // The key is taken BEFORE the exclusive-OR is undone, which is what these bits are. Two
            // decodes of the same message scramble identically, so the partition is the same one
            // either way, and taking it here means the key is what was on the air.
            LdpcDecoder.Decode(ratios, codeword, MaxIterations);
            var key = codeword[..Ft8Payload.MessageBits];

            if (AlreadySeen(seen, key))
            {
                duplicates++;
                continue;
            }

            if (messages.Count >= MessageLimit)
            {
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
