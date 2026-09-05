using System;
using System.Collections.Generic;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>Slots in order, results out — and a transmission that was repeated in later slots is heard as
/// the sum of <em>every</em> hearing before anything decodes it.</b> Step 6 of the previous phase,
/// taken deeper than a pair by unit 254.
/// </summary>
/// <remarks>
/// <para>
/// <b>UNTIL UNIT 254 EVERY SUM WAS A PAIR, WHATEVER THE HISTORY HELD.</b> This loop called the
/// two-hearing overload of <see cref="Ft8DeepSoftCombiner"/> once per remembered slot, so a decoder
/// handed four slots computed a chain of pairs and a column headed <c>combined x4</c> measured the
/// 3.01 dB that two hearings are worth while its name claimed the 6.02 that four are. What changed
/// is <see cref="Ft8DeepCombineSettings.AccumulationDepth"/> and one call site;
/// <see cref="Ft8DeepCombineCounts.DeepestHearings"/> is what makes the difference visible from
/// outside.
/// </para>
/// <para>
/// <b>AND THE BUDGET DID NOT MOVE.</b> The rule is still one combination submitted per candidate per
/// partner rank per remembered slot. Depth is a sliding window over the history — more hearings in
/// each submission, never more submissions — because every codeword put to the port's CRC-14 is an
/// independent chance of accepting a message nobody sent, and a deeper sum bought by spending more
/// of that budget would be bought with the one thing this project will not trade.
/// </para>
/// <para>
/// <b>COMBINING ONLY EVER ADDS, AND THAT IS THE PROPERTY EVERYTHING ELSE RESTS ON.</b> Every message
/// the single-slot path returned for a slot is in this slot's result, in the same order, unchanged;
/// combined decodes are appended after them. So a run with combining on is a superset of the same run
/// with it off, trial for trial, and any rate this stage moves is attributable to it and to nothing
/// else. It is asserted rather than intended — see <c>Ft8DeepRepeatDecoderTests</c>.
/// </para>
/// <para>
/// <b>Combining is off by default</b>, and off means this type is <see cref="Ft8DeepSlotDecoder"/> and
/// nothing more — which, with <c>Osd</c> also null, is exactly what <c>Ft8SlotDecoder</c> does.
/// </para>
/// <para>
/// <b>THE MEMORY AND THE TIME, STATED.</b> The history is
/// <see cref="Ft8DeepCombineSettings.HistoryDepth"/> slots of <see cref="Ft8DeepHearing"/>, which is at
/// most the port's candidate limit of 140 hearings of 174 floats each — <b>about 97 kilobytes a slot,
/// under a megabyte at the maximum depth of eight</b>, and one slot at the default depth of one. The
/// time is one <c>Ft8SoftSymbols.Normalise</c> and one <c>Ft8CodewordDecoder.Decode</c> per submission,
/// bounded by <c>candidates × MaximumPartners × HistoryDepth</c> — <b>the pairing itself is two
/// floating-point comparisons per pair and costs nothing measurable.</b>
/// </para>
/// <para>
/// <b>Nothing here decides that a message is real.</b> Every combined codeword goes to
/// <c>Ft8CodewordDecoder.Decode</c> and is accepted or refused by the port's own parity gate and
/// CRC-14 gate. There is no checksum in this library and no acceptance rule in this library.
/// </para>
/// <para>
/// <b>And nothing here is told what was transmitted.</b> The pairing rule sees two candidates'
/// frequency and time and nothing else; it does not know the two slots carry the same message, and a
/// combiner that paired slots by knowing that would be measuring nothing.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
public sealed class Ft8DeepRepeatDecoder
{
    private readonly Ft8DeepSlotDecoder _inner;
    private readonly List<IReadOnlyList<Ft8DeepHearing>> _history = [];

    /// <summary>Builds a repeat decoder over a sibling slot decoder.</summary>
    /// <param name="inner">
    /// The slot decoder every slot goes through first. <see langword="null"/> builds a default one —
    /// and one that remembers its hearings if <paramref name="combining"/> asks for combining, because
    /// a combiner with nothing remembered would silently do nothing.
    /// </param>
    /// <param name="combining">
    /// The pairing rule and its submission budget, or <see langword="null"/> — the default — for
    /// combining off.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Combining is on and <paramref name="inner"/> does not remember its hearings. <b>Refused rather
    /// than quietly returning zero combinations</b>: a run that reported a combining stage doing
    /// nothing, with no way to tell that from a stage that found nothing, is the exact shape of a
    /// measurement nobody can trust.
    /// </exception>
    public Ft8DeepRepeatDecoder(
        Ft8DeepSlotDecoder? inner = null,
        Ft8DeepCombineSettings? combining = null)
    {
        if (inner is not null && combining is not null && !inner.RemembersHearings)
        {
            throw new ArgumentException(
                "Combining is on and this slot decoder does not remember its hearings, so there would "
                + "be nothing to combine a later slot with and the stage would report zero pairs "
                + "offered for every slot. Build the inner decoder with rememberHearings: true, or "
                + "leave it null and this constructor will.",
                nameof(inner));
        }

        _inner = inner ?? new Ft8DeepSlotDecoder(rememberHearings: combining is not null);
        Combining = combining;
    }

    /// <summary>
    /// <b>The pairing rule and its budget, or <see langword="null"/> for off.</b> Null is the default
    /// and means this decoder is its inner slot decoder exactly.
    /// </summary>
    public Ft8DeepCombineSettings? Combining { get; }

    /// <summary>The slot decoder every slot goes through before anything is combined.</summary>
    public Ft8DeepSlotDecoder Inner => _inner;

    /// <summary>The extents this decoder analyses to. The port's.</summary>
    public Ft8WaterfallGeometry Geometry => _inner.Geometry;

    /// <summary>How many previous slots are being held right now.</summary>
    public int RememberedSlots => _history.Count;

    /// <summary>
    /// <b>What the combining stage did in the last slot decoded</b>, beside the five counts the port
    /// returns and the four the ordered statistics stage returns. All zero while
    /// <see cref="Combining"/> is null.
    /// </summary>
    public Ft8DeepCombineCounts LastCombine { get; private set; }

    /// <summary>What the ordered statistics stage did in the last slot decoded. The inner decoder's.</summary>
    public Ft8DeepOsdCounts LastOsd => _inner.LastOsd;

    /// <summary>
    /// <b>Forgets every remembered slot.</b> Called between independent runs, so that the last slot of
    /// one measurement is never combined with the first slot of the next.
    /// </summary>
    public void Reset() => _history.Clear();

    /// <summary>Decodes the next slot of audio, in order.</summary>
    /// <param name="samples">The slot's audio. At least one block long.</param>
    /// <exception cref="ArgumentException">The signal is shorter than one block.</exception>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples) =>
        Decode(new Ft8Monitor(Geometry).Analyse(samples));

    /// <summary>Decodes the next slot, in order, from a waterfall already built.</summary>
    /// <param name="waterfall">The spectrogram of one slot.</param>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public Ft8SlotResult Decode(Ft8Waterfall waterfall)
    {
        ArgumentNullException.ThrowIfNull(waterfall);

        // STAGE ONE, ALWAYS: the ordinary single-slot decode, whatever combining is set to. Its
        // messages are this slot's result and combining may only append to them.
        var single = _inner.Decode(waterfall);

        if (Combining is null)
        {
            LastCombine = default;
            return single;
        }

        var hearings = _inner.LastHearings;
        var result = Combine(single, hearings);

        Remember(hearings);
        return result;
    }

    /// <summary>
    /// STAGE TWO: every candidate of this slot against the remembered slots, under the pairing rule
    /// and inside the budget.
    /// </summary>
    private Ft8SlotResult Combine(Ft8SlotResult single, IReadOnlyList<Ft8DeepHearing> hearings)
    {
        var settings = Combining!;
        var geometry = Geometry;

        var offered = 0;
        var submitted = 0;
        var accepted = 0;
        var added = 0;
        var deepestHearings = 0;

        // The keys already returned, so a combined decode that repeats a single-slot decode is a
        // duplicate rather than a second message. The key is the first 77 bits of the codeword, which
        // is the port's own de-duplication key.
        var seen = new List<byte[]>(single.Messages.Count);
        var texts = new List<string>(single.Messages.Count);
        var codeword = new byte[LdpcDecoder.CodewordBits];

        foreach (var message in single.Messages)
        {
            texts.Add(message.Text);

            var found = FindHearing(hearings, message.Candidate);
            if (found >= 0)
            {
                LdpcDecoder.Decode(hearings[found].Ratios, codeword, _inner.MaxIterations);
                seen.Add(codeword[..Ft8Payload.MessageBits]);
            }
        }

        var messages = new List<Ft8SlotMessage>(single.Messages);

        // ONE CACHE FOR THE SLOT, as the port has it, and a fresh one: a combined decode may resolve a
        // hashed callsign, and the port's own cache for this slot has already been consumed.
        var cache = new Ft8CallsignCache();
        var combined = new float[Ft8DeepSoftCombiner.RatioCount];

        // One ranked partner list per remembered slot, MOST RECENT FIRST, so index j is "j slots
        // back plus one". Allocated once for the slot and cleared per candidate: the pairing itself
        // is two floating-point comparisons per pair and must stay that cheap.
        var perSlot = new List<Ft8DeepHearing>[_history.Count];
        for (var j = 0; j < perSlot.Length; j++)
        {
            perSlot[j] = new List<Ft8DeepHearing>(settings.MaximumPartners);
        }

        // The hearings handed to one call of Ft8DeepSoftCombiner.Combine. A List<float[]> rather
        // than a collection expression, for the reason Ft8DeepSoftCombiner's two-hearing overload
        // gives: a synthesised read-only array type would reach the whole-type-list tripwire.
        var chain = new List<float[]>(settings.AccumulationDepth + 1);

        foreach (var hearing in hearings)
        {
            var frequency = hearing.Candidate.FrequencyHz(geometry);
            var time = hearing.Candidate.TimeSeconds(geometry);

            // THE PAIRING RULE, AND IT IS UNIT 247'S UNCHANGED. Two candidates are the same station
            // repeating itself when they sit within the tolerances in frequency and in
            // time-within-the-slot. A transmitter does not move by a tone between slots; unit 247
            // task 1 measured how far it does move. Nothing here knows what was transmitted.
            for (var j = 0; j < perSlot.Length; j++)
            {
                var earlier = _history[_history.Count - 1 - j];
                var partners = perSlot[j];
                partners.Clear();

                foreach (var candidate in earlier)
                {
                    offered++;

                    if (Math.Abs(candidate.Candidate.FrequencyHz(geometry) - frequency)
                        > settings.FrequencyToleranceHz)
                    {
                        continue;
                    }

                    if (Math.Abs(candidate.Candidate.TimeSeconds(geometry) - time)
                        > settings.TimeToleranceSeconds)
                    {
                        continue;
                    }

                    Offer(partners, candidate, settings.MaximumPartners);
                }
            }

            // THE BUDGET, SPENT HERE AND NOWHERE ELSE: at most MaximumPartners submissions per
            // candidate per remembered slot, and every one of them is an independent chance of the
            // port's CRC-14 accepting a message nobody sent.
            //
            // THE ACCUMULATION IS A SLIDING WINDOW OVER THE HISTORY AND NOT AN EXTRA SUBMISSION.
            // For each partner rank k and each remembered slot j there is exactly ONE combination,
            // as there was when every combination was a pair - what changed is what is in it. At
            // AccumulationDepth 1 the window is one slot wide and the combination is the pair
            // (this slot, slot j), which is what this library computed at every depth before unit
            // 254. At depth A it is this slot together with the rank-k partners of the last A
            // remembered slots ending at j, so the deepest combination carries A + 1 hearings.
            //
            // A rule that submitted the 2-way AND the 3-way AND the 4-way would spend three chances
            // of a false accept where the pairwise rule spent one. CLAUDE.md 0.0: a decode nobody
            // sent is worse than a decode missed, so the depth is bought out of the same budget or
            // it is not bought.
            for (var k = 0; k < settings.MaximumPartners; k++)
            {
                for (var j = 0; j < perSlot.Length; j++)
                {
                    if (k >= perSlot[j].Count)
                    {
                        // No rank-k partner in this remembered slot, so there is no combination to
                        // submit for it - exactly as when this was a pairwise loop.
                        continue;
                    }

                    chain.Clear();
                    chain.Add(hearing.Ratios);

                    var from = Math.Max(0, j - settings.AccumulationDepth + 1);
                    for (var w = from; w <= j; w++)
                    {
                        // A slot inside the window that offered no rank-k partner is simply not in
                        // the sum. The window shrinks; it never turns into a second submission.
                        if (k < perSlot[w].Count)
                        {
                            chain.Add(perSlot[w][k].Ratios);
                        }
                    }

                    // THE SUM ITSELF. Log-likelihood ratios of conditionally independent hearings of
                    // one codeword add, which is 10 log10 R decibels of processing gain for R of
                    // them - textbook, from nobody's source. What is cited is the frame the ratios
                    // sit in: 174 bits in codeword order carrying a 77-bit payload and a CRC-14,
                    // from Franke K9AN, Somerville G4WJS and Taylor K1JT, "The FT4 and FT8
                    // Communication Protocols", QEX, July/August 2020 - position i means the same
                    // codeword bit in every hearing because the protocol says so.
                    Ft8DeepSoftCombiner.Combine(chain, settings.Weighting, combined);

                    var verdict = Ft8CodewordDecoder.Decode(combined, cache, _inner.MaxIterations);
                    submitted++;

                    // HOW MANY HEARINGS THAT SUBMISSION ACTUALLY CARRIED, which before unit 254 was
                    // two however many slots the decoder remembered.
                    deepestHearings = Math.Max(deepestHearings, chain.Count);

                    if (!verdict.Decoded)
                    {
                        continue;
                    }

                    accepted++;

                    // THE KEY IS THE CODEWORD THE COMBINATION PRODUCED, its first 77 bits - recovered
                    // by re-running the same deterministic decoder over the same combined ratios, not
                    // over any slot's original ratios, which did not converge and will not.
                    LdpcDecoder.Decode(combined, codeword, _inner.MaxIterations);
                    var key = codeword[..Ft8Payload.MessageBits];

                    if (AlreadySeen(seen, key) || AlreadySaid(texts, verdict.Message.Text))
                    {
                        continue;
                    }

                    if (messages.Count >= _inner.MessageLimit)
                    {
                        // The port's own rule, reproduced: stop adding, do not stop the loop.
                        continue;
                    }

                    seen.Add(key);
                    texts.Add(verdict.Message.Text);

                    // The candidate reported is THIS slot's, because this is the slot the message is
                    // being returned for and its frequency and time are this slot's.
                    messages.Add(new Ft8SlotMessage(hearing.Candidate, verdict));
                    added++;
                }
            }
        }

        LastCombine = new Ft8DeepCombineCounts(offered, submitted, accepted, added, deepestHearings);

        // THE FIVE COUNTS STAY THE PORT'S REPORT ON THE PORT'S BELIEF PROPAGATION. Only the message
        // list grows, so a reader comparing the counts against a single-slot run sees the same five
        // numbers and a longer list - which is what "combining only ever adds" looks like from the
        // outside.
        return single with { Messages = messages };
    }

    /// <summary>
    /// Keeps the best <paramref name="limit"/> partners by sync score, best first. <b>The budget's
    /// tie-break, and it is the score because the score is the only ranking the search offers without
    /// a truth in it.</b>
    /// </summary>
    private static void Offer(List<Ft8DeepHearing> partners, Ft8DeepHearing candidate, int limit)
    {
        var at = partners.Count;
        while (at > 0 && partners[at - 1].Candidate.Score < candidate.Candidate.Score)
        {
            at--;
        }

        if (at >= limit)
        {
            return;
        }

        partners.Insert(at, candidate);
        if (partners.Count > limit)
        {
            partners.RemoveAt(partners.Count - 1);
        }
    }

    /// <summary>Which hearing a returned message came from, or -1 if it cannot be identified.</summary>
    private static int FindHearing(IReadOnlyList<Ft8DeepHearing> hearings, Ft8Candidate candidate)
    {
        for (var i = 0; i < hearings.Count; i++)
        {
            if (hearings[i].Candidate.Equals(candidate))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// The second half of the de-duplication, and it is a belt as well as braces. The key above is the
    /// port's own; this catches the one case the key cannot — a message the ordered statistics stage
    /// rescued, whose key cannot be recovered by re-running belief propagation over its original
    /// ratios because that is exactly what did not converge on it.
    /// </summary>
    private static bool AlreadySaid(List<string> texts, string text)
    {
        foreach (var previous in texts)
        {
            if (string.Equals(previous, text, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
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

    /// <summary>
    /// Keeps this slot's hearings and drops anything older than the history depth. <b>The memory bound
    /// is enforced here rather than trusted.</b>
    /// </summary>
    private void Remember(IReadOnlyList<Ft8DeepHearing> hearings)
    {
        _history.Add(hearings);
        while (_history.Count > Combining!.HistoryDepth)
        {
            _history.RemoveAt(0);
        }
    }
}
