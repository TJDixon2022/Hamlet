using System;
using System.Collections.Generic;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Dsp;

/// <summary>
/// <b>The whole path: fifteen seconds of audio in, the messages that were in it out.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Every part of this was already proved and none of them had ever met.</b> The monitor turns
/// samples into a waterfall (unit 213), the search says where the transmissions are without being
/// told (unit 214), <see cref="Ft8SoftSymbols"/> turns a place into 174 ratios (unit 216), and
/// <see cref="Ft8CodewordDecoder"/> repairs a damaged codeword or refuses it (unit 215). This type
/// is the wiring and the de-duplicator, and it re-implements none of them.
/// </para>
/// <para>
/// <b>NOTHING FAILING PARITY OR THE CHECKSUM IS RETURNED, and there is no route around the gate.</b>
/// Every candidate goes through <see cref="Ft8CodewordDecoder.Decode"/> and only a
/// <see cref="Ft8CodewordStatus.Decoded"/> reaches the list — not with a flag, not with a
/// confidence, not as a partial. <c>CLAUDE.md</c> §0.0 / HM-DEC-009.
/// </para>
/// <para>
/// <b>One callsign cache per slot, created per call, never a static singleton.</b> FT8 puts some
/// stations on the air as a 22, 12 or 10-bit hash, and the only way to read one is to have heard the
/// call in full earlier in the same slot. The cache is that memory. Unit 208 ruled that it is never
/// shared between calls, because a decode that depends on what some other slot happened to contain
/// is not reproducible.
/// </para>
/// <para>
/// <b>Deterministic.</b> The same samples give the same messages in the same order, every time. The
/// candidate list is in <see cref="Ft8Candidate.CompareTo"/>'s total order — which upstream's own
/// heapsort could not provide, divergence 19 — and the messages come back in the order they were
/// first decoded from it. Nothing here reads a clock, a random source, an environment variable or a
/// dictionary's enumeration order, and nothing runs in parallel.
/// </para>
/// <para>
/// <b>Nothing plays and nothing opens a device.</b> No audio device, no stream, no file. The caller
/// hands over samples it obtained however it likes, and this returns text. <c>CLAUDE.md</c> §0.2.
/// </para>
/// <para>
/// <b>The counts are a measurement, not a display.</b> <see cref="Ft8SlotResult"/> carries how many
/// candidates reached each stage because a report that says <em>nothing decoded</em> and stops is
/// useless to whoever has to find out why. They are five integers. They are explicitly not the
/// legibility surface the plan parks as a phase of its own: nothing here is aimed at a screen, and
/// nothing here says what a display should claim.
/// </para>
/// </remarks>
public sealed class Ft8SlotDecoder
{
    /// <summary>
    /// The most messages one slot returns, defaulting to upstream's application's own limit.
    /// </summary>
    /// <remarks>
    /// <b>A weak anchor and labelled as one.</b> <c>kMax_decoded_messages</c> is a file-scope
    /// constant in <c>demo/decode_ft8.c</c> and appears nowhere in <c>ft8/</c>, so it is one
    /// application's judgement rather than a property of FT8 —
    /// <c>UpstreamExtractionInventoryTests</c> reads it out of the pin and
    /// <c>Ft8SlotDecoderProvenanceTests</c> binds this constant to it.
    /// </remarks>
    public const int DefaultMessageLimit = 50;

    private readonly Ft8SyncSearch _search;

    /// <summary>Builds a decoder for one geometry.</summary>
    /// <param name="geometry">The extents to analyse to. Defaults to upstream's at 12 kHz.</param>
    /// <param name="search">
    /// The search to find candidates with. Defaults to one carrying upstream's own candidate limit
    /// and minimum score.
    /// </param>
    /// <param name="messageLimit">The most messages one slot returns.</param>
    /// <param name="maxIterations">
    /// How hard the correction tries per candidate, defaulting to
    /// <see cref="LdpcDecoder.DefaultMaxIterations"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The message limit is negative, or the iteration count is negative.
    /// </exception>
    public Ft8SlotDecoder(
        Ft8WaterfallGeometry? geometry = null,
        Ft8SyncSearch? search = null,
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

        Geometry = geometry ?? new Ft8WaterfallGeometry();
        _search = search ?? new Ft8SyncSearch();
        MessageLimit = messageLimit;
        MaxIterations = maxIterations;
    }

    /// <summary>The extents this decoder analyses to.</summary>
    public Ft8WaterfallGeometry Geometry { get; }

    /// <summary>The most messages one slot returns.</summary>
    public int MessageLimit { get; }

    /// <summary>How hard the correction tries per candidate.</summary>
    public int MaxIterations { get; }

    /// <summary>The candidate limit the search this decoder uses will return.</summary>
    public int CandidateLimit => _search.CandidateLimit;

    /// <summary>The minimum sync score the search this decoder uses will keep.</summary>
    public int MinimumScore => _search.MinimumScore;

    /// <summary>
    /// Decodes one slot of audio: analyse, search, and try every candidate in rank order.
    /// </summary>
    /// <param name="samples">
    /// The slot's audio at <see cref="Ft8WaterfallGeometry.SampleRate"/>. At least one block long.
    /// </param>
    /// <exception cref="ArgumentException">The signal is shorter than one block.</exception>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples) =>
        Decode(new Ft8Monitor(Geometry).Analyse(samples));

    /// <summary>Decodes one slot from a waterfall that has already been built.</summary>
    /// <param name="waterfall">The spectrogram of one slot.</param>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public Ft8SlotResult Decode(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        var candidates = _search.Find(waterfall);

        // ONE CACHE FOR THE SLOT, created here and dropped when this returns. A callsign heard in
        // full at candidate 3 can be resolved from its hash at candidate 40; nothing carries over
        // to the next slot, because a decode that depends on what some other slot contained is not
        // a decode anybody can reproduce.
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

            // THE DE-DUPLICATION KEY IS THE MESSAGE ITSELF, which is upstream's rule. Upstream
            // compares the ten packed payload bytes; the payload is the 77 message bits followed by
            // their own CRC-14, and the CRC is a function of those bits, so comparing the message
            // bits partitions the decodes exactly as comparing the payload does.
            //
            // The gate does not hand back the bits it accepted and it is closed evidence this unit
            // may not change, so they are recovered by running the same deterministic decoder over
            // the same ratios. It is not a second CRC check - there is still exactly one of those
            // in this library - and it costs one belief propagation per SUCCESSFUL decode only.
            LdpcDecoder.Decode(ratios, codeword, MaxIterations);
            var key = codeword[..Ft8Payload.MessageBits];

            if (AlreadySeen(seen, key))
            {
                duplicates++;
                continue;
            }

            if (messages.Count >= MessageLimit)
            {
                // Upstream cannot reach this: its hash table probes forward for an empty slot and
                // a full table loops forever. Stopping is the divergence and it is recorded.
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

/// <summary>One message a slot gave up, and where in the slot it was.</summary>
/// <remarks>
/// <b>Both halves are types that already existed.</b> The candidate is the search's own record of a
/// place, carrying its score and its own frequency and time helpers; the message is step 2's own
/// decode result. Nothing new is invented to carry either, and in particular <b>no diagnostic
/// surface is started here</b> — the plan parks that as a phase of its own.
/// </remarks>
/// <param name="Candidate">Where in the waterfall it was found.</param>
/// <param name="Result">What the gate made of it. Always <see cref="Ft8CodewordStatus.Decoded"/>.</param>
public readonly record struct Ft8SlotMessage(Ft8Candidate Candidate, Ft8CodewordResult Result)
{
    /// <summary>The message, as text. Never empty for a message in a result's list.</summary>
    public string Text => Result.Message.Text;

    /// <summary>The frequency of the transmission's lowest tone.</summary>
    public double FrequencyHz(Ft8WaterfallGeometry geometry) => Candidate.FrequencyHz(geometry);

    /// <summary>When the transmission's first symbol began, in seconds from the start of the slot.</summary>
    public double TimeSeconds(Ft8WaterfallGeometry geometry) => Candidate.TimeSeconds(geometry);
}

/// <summary>What one slot of audio gave up, and how far every candidate got.</summary>
/// <remarks>
/// <b>The counts exist because a report that says nothing decoded and stops is useless.</b>
/// Candidates found but nothing past parity is a different fact from no candidates at all, and
/// whoever has to find out why needs to know which. They are five integers in a result type and they
/// are not a stage log, a diagnostic framework or anything aimed at a screen.
/// </remarks>
/// <param name="CandidateCount">Places the search returned.</param>
/// <param name="ParitySatisfiedCount">Of those, how many reached a valid codeword.</param>
/// <param name="ChecksumPassedCount">Of those, how many carried their own checksum.</param>
/// <param name="BecameTextCount">Of those, how many this library could put into words.</param>
/// <param name="DuplicateCount">
/// Of those, how many were a message already returned from this slot. Expected and not a defect: a
/// strong transmission produces several candidates and every one of them decodes.
/// </param>
/// <param name="Messages">The unique messages, in the order they were first decoded.</param>
public readonly record struct Ft8SlotResult(
    int CandidateCount,
    int ParitySatisfiedCount,
    int ChecksumPassedCount,
    int BecameTextCount,
    int DuplicateCount,
    IReadOnlyList<Ft8SlotMessage> Messages)
{
    /// <summary>The message texts, in the order they were first decoded.</summary>
    public IReadOnlyList<string> Texts
    {
        get
        {
            var texts = new string[Messages.Count];
            for (var i = 0; i < texts.Length; i++)
            {
                texts[i] = Messages[i].Text;
            }

            return texts;
        }
    }
}
