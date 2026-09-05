using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Ft8DeepOrderedStatistics? _statistics;
    private readonly byte[] _osdCodeword;
    private readonly float[] _osdRatios;
    private readonly Ft8DeepFineSync? _fineSync;
    private readonly float[] _fineRatios;
    private readonly double[] _fineGrid;

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
    /// <param name="rememberHearings">
    /// Whether to keep every candidate's normalised ratios in <see cref="LastHearings"/> so that a
    /// later slot can be combined with them. <b>Off by default and off costs nothing</b> - see that
    /// property.
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
        Ft8DeepOsdSettings? osd = null,
        bool rememberHearings = false,
        Ft8DeepFineSyncSettings? fineSync = null,
        Ft8DeepBasebandSettings? baseband = null,
        Ft8DeepSubtractionSettings? subtraction = null)
    {
        var used = search ?? new Ft8SyncSearch();
        _port = new Ft8SlotDecoder(geometry, used, messageLimit, maxIterations);
        _search = used;
        Osd = osd;
        RemembersHearings = rememberHearings;
        FineSync = fineSync;
        Baseband = baseband;
        Subtraction = subtraction;
        (_statistics, _osdCodeword, _osdRatios) = Prepare(osd);
        (_fineSync, _fineRatios, _fineGrid) = PrepareFineSync(fineSync);
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
        Ft8DeepOsdSettings? osd = null,
        bool rememberHearings = false,
        Ft8DeepFineSyncSettings? fineSync = null,
        Ft8DeepBasebandSettings? baseband = null,
        Ft8DeepSubtractionSettings? subtraction = null)
    {
        ArgumentNullException.ThrowIfNull(port);
        _port = port;
        _search = search ?? new Ft8SyncSearch(port.CandidateLimit, port.MinimumScore);
        Osd = osd;
        RemembersHearings = rememberHearings;
        FineSync = fineSync;
        Baseband = baseband;
        Subtraction = subtraction;
        (_statistics, _osdCodeword, _osdRatios) = Prepare(osd);
        (_fineSync, _fineRatios, _fineGrid) = PrepareFineSync(fineSync);
    }

    /// <summary>
    /// The ordered statistics stage and its scratch, or nothing at all when it is off. Built once per
    /// decoder so that a per-candidate call allocates nothing.
    /// </summary>
    private static (Ft8DeepOrderedStatistics?, byte[], float[]) Prepare(Ft8DeepOsdSettings? osd) =>
        osd is null
            ? (null, [], [])
            : (new Ft8DeepOrderedStatistics(),
                new byte[Ft8DeepOrderedStatistics.CodewordBits],
                new float[Ft8DeepOrderedStatistics.CodewordBits]);

    /// <summary>
    /// The fine synchronisation stage and its scratch, or nothing at all when it is off. Built once
    /// per decoder so that a per-candidate re-sync allocates nothing but its baseband.
    /// </summary>
    private static (Ft8DeepFineSync?, float[], double[]) PrepareFineSync(
        Ft8DeepFineSyncSettings? settings) =>
        settings is null
            ? (null, [], [])
            : (new Ft8DeepFineSync(settings),
                new float[Ft8SoftSymbols.RatioCount],
                new double[Ft8DeepBasebandExtractor.GridLength]);

    /// <summary>
    /// <b>How far a candidate's nominal time sits from the start of the signal it found, in
    /// seconds.</b> Exactly minus one symbol period, and it was measured rather than derived.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Ft8WaterfallGeometry.TimeSeconds</c> says in its own remarks that it returns <em>the
    /// block's nominal position and not the centre of the window that produced it</em> - the analysis
    /// frame is 3840 samples, is prefilled with zeros and slides, so the samples behind a block reach
    /// back before it - and that <b>the exact alignment could not be settled by reading and is not
    /// asserted there</b>.
    /// </para>
    /// <para>
    /// <b>So unit 248 swept it on the hard-decision distance instead</b>, over one whole 51-message
    /// block at -14 dB, taking the candidate closest to the transmitted codeword in each trial and
    /// running this library's extractor at that candidate's nominal time plus a bias running two
    /// symbols either way in half-symbol steps. The median distance is <b>0 of 174 at minus one
    /// symbol with all 51 trials inside the code's recovery threshold</b>, and 47 or worse at every
    /// neighbouring half-symbol step. The table is in <c>docs/unit248-baseband-resync.md</c> and the
    /// sweep is <c>Ft8Unit248ExtractorTraceTests</c>.
    /// </para>
    /// <para>
    /// <b>Getting this wrong would be a constant time error in every position this library reports</b>
    /// and it would look exactly like a fine search that does not work, so it is a named constant
    /// with its measurement beside it rather than a number inside an expression.
    /// </para>
    /// </remarks>
    public const double CandidateTimeBiasSeconds = -Ft8WaterfallGeometry.SymbolPeriodSeconds;

    /// <summary>
    /// The port decoder this one takes its geometry, its limits and its refusals from.
    /// </summary>
    public Ft8SlotDecoder Port => _port;

    /// <summary>
    /// <b>How hard the ordered statistics stage tries, or <see langword="null"/> for off.</b> Null is
    /// the default and means this decoder reproduces the port exactly.
    /// </summary>
    public Ft8DeepOsdSettings? Osd { get; }

    /// <summary>
    /// <b>What the ordered statistics stage did in the last slot decoded, beside the five counts the
    /// port already returns.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A rate that moved with no visible OSD activity behind it is not evidence</b>, which is why
    /// these are kept at all. Reset at the top of every <see cref="Decode(Ft8Waterfall)"/> and read
    /// after it; all zero while <see cref="Osd"/> is null.
    /// </para>
    /// <para>
    /// They live here rather than on <c>Ft8SlotResult</c> because <c>Ft8SlotResult</c> is the port's
    /// own record and this phase changes no line of the port. The scoreboard's seat is
    /// <c>Func&lt;float[], Ft8SlotResult&gt;</c>, so a report that wants these reads them from the
    /// decoder after the call.
    /// </para>
    /// </remarks>
    public Ft8DeepOsdCounts LastOsd { get; private set; }

    /// <summary>
    /// <b>Whether every candidate's normalised ratios are kept for a later slot to be combined
    /// with.</b> Off by default.
    /// </summary>
    public bool RemembersHearings { get; }

    /// <summary>
    /// <b>Every candidate of the last slot decoded, with the normalised ratios the gate saw</b>, or an
    /// empty list when <see cref="RemembersHearings"/> is false.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is what step 6 needs and it is the only reason it exists.</b> Combining a repeat means
    /// adding an earlier slot's ratios to a later slot's, and a finished
    /// <see cref="Ft8SlotResult"/> does not carry them - by the time it exists, every refused
    /// candidate's evidence has been overwritten by the next candidate's.
    /// </para>
    /// <para>
    /// <b>Off by default because it costs an allocation and a copy per candidate.</b> With it off this
    /// decoder does exactly what it did at unit 246, which with <see cref="Osd"/> also null is exactly
    /// what the port does - and the scoreboard's OSD-off and OSD-on columns stay comparable with the
    /// rows already recorded. <b>Turning it on changes no decision and no count</b>; it only keeps a
    /// copy of what was already computed.
    /// </para>
    /// </remarks>
    public IReadOnlyList<Ft8DeepHearing> LastHearings { get; private set; } =
        Array.Empty<Ft8DeepHearing>();

    /// <summary>
    /// <b>How far a refused candidate may be re-synced below the waterfall's grid, or
    /// <see langword="null"/> for off.</b> Null is the default and means this decoder does exactly
    /// what it did at unit 247 - which, with <see cref="Osd"/> also null, is exactly what the port
    /// does.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Fine synchronisation runs only where the port's gates refused the candidate, and it never
    /// replaces a decode the port already made.</b> Same shape as the ordered statistics stage: the
    /// candidate goes through the port's path first and only a candidate the port would have thrown
    /// away is re-synced. <b>At most one extra submission to <c>Ft8CodewordDecoder.Decode</c> per
    /// coarse candidate</b>, which is what bounds the false-accept arithmetic and what makes the
    /// superset property assertable - <b>re-syncing only ever adds.</b>
    /// </para>
    /// <para>
    /// <b>It needs the samples.</b> A waterfall holds magnitudes quantised to half a decibel on a
    /// fixed grid, with no phase and no audio behind it, so <see cref="Decode(Ft8Waterfall)"/> with
    /// this configured performs no re-sync at all. It does not throw and it does not pretend: the
    /// candidates it could not touch are counted in
    /// <c>Ft8DeepFineSyncCounts.RefusedForWantOfSamples</c>.
    /// </para>
    /// </remarks>
    public Ft8DeepFineSyncSettings? FineSync { get; }

    /// <summary>
    /// How the samples are mixed down, filtered and decimated for a re-sync, or
    /// <see langword="null"/> for <c>Ft8DeepBasebandSettings.Default</c>. Unused while
    /// <see cref="FineSync"/> is null.
    /// </summary>
    public Ft8DeepBasebandSettings? Baseband { get; }

    /// <summary>
    /// <b>What the fine synchronisation stage did in the last slot decoded.</b> Reset at the top of
    /// every decode and read after it; all zero while <see cref="FineSync"/> is null.
    /// </summary>
    public Ft8DeepFineSyncCounts LastFineSync { get; private set; }

    /// <summary>
    /// <b>How many times a slot is read and what is subtracted between the readings, or
    /// <see langword="null"/> for off.</b> Null is the default and means this decoder does exactly
    /// what it did at unit 252 - which, with <see cref="Osd"/> and <see cref="FineSync"/> also null,
    /// is exactly what the port does.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>OFF IS THE DEFAULT AND THAT IS LOAD-BEARING.</b> Every row of units 246, 248, 251 and 252
    /// is a measurement of the default path. <c>Ft8DeepSubtractionTests</c> asserts whole-result
    /// identity - all five counts and every message's text, candidate, frequency and dt - between a
    /// decoder built with this null and one built with no arguments at all, on a slot that decodes,
    /// because if the default path ever moved, every one of those rows would be invalidated at once
    /// and nothing in the tree would say so.
    /// </para>
    /// <para>
    /// <b>IT NEEDS THE SAMPLES, AND UNLIKE FINE SYNC IT REFUSES RATHER THAN COUNTS.</b> A waterfall
    /// holds magnitudes on a grid with no phase and no audio behind it: there is nothing to fit a
    /// carrier phase to, nothing to subtract a waveform from, and nothing to re-analyse afterwards.
    /// <see cref="Decode(Ft8Waterfall)"/> with this configured <b>throws</b>. Fine sync's answer to
    /// the same wall is to count the candidates it could not touch and carry on, which is right for
    /// a per-candidate rescue and wrong here: subtraction is a whole extra pass over a whole slot,
    /// and a caller who asked for four passes and silently got one has been told a decode ran that
    /// did not run. Unit 249 found fine sync refusing 42 of 42 candidates in exactly that shape.
    /// </para>
    /// <para>
    /// <b>NOTHING HERE DECIDES THAT A MESSAGE IS REAL.</b> A pass over a residual is an ordinary
    /// decode of a different buffer: same search, same extract, same normalise, same
    /// <c>Ft8CodewordDecoder.Decode</c>, <b>one codeword to the port's parity gate and CRC-14 per
    /// candidate per pass</b>. What multiplies with the pass count is the number of passes, and
    /// <c>docs/unit253-subtraction.md</c> §3.1 tabulates what that costs.
    /// </para>
    /// </remarks>
    public Ft8DeepSubtractionSettings? Subtraction { get; }

    /// <summary>
    /// <b>What the subtraction stage did in the last slot decoded.</b> Reset at the top of every
    /// decode and read after it; <c>PassesRun</c> is one and everything else is zero while
    /// <see cref="Subtraction"/> is null.
    /// </summary>
    public Ft8DeepSubtractionCounts LastSubtraction { get; private set; } =
        new(1, 0, 0, 0, 0, 0, 0, double.NaN);

    /// <summary>
    /// <b>Every fit the subtraction stage made in the last slot decoded, in the order it made
    /// them</b>, or an empty list when it is off.
    /// </summary>
    /// <remarks>
    /// The gain, the phase, the place and the decibels removed, one entry a message subtracted.
    /// <b>Diagnostic, and reported rather than gated on</b> - see
    /// <c>Ft8DeepSubtractionFit.DecibelsRemoved</c>.
    /// </remarks>
    public IReadOnlyList<Ft8DeepSubtractionFit> LastFits { get; private set; } =
        Array.Empty<Ft8DeepSubtractionFit>();

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
    /// <remarks>
    /// <b>This is the samples-carrying entry point and it is the only one a re-sync can run in.</b>
    /// Until unit 248 it discarded the audio the moment it had a waterfall; it now keeps it
    /// alongside, because a waterfall has no phase in it and no samples behind it and there is
    /// nothing in one to re-sync from. <b>Nothing else about it changed</b>: with
    /// <see cref="FineSync"/> null it is the same call it always was.
    /// </remarks>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples)
    {
        if (Subtraction is null)
        {
            // THE DEFAULT PATH, UNTOUCHED. One analyse, one pass, the same call it always was.
            LastSubtraction = new Ft8DeepSubtractionCounts(1, 0, 0, 0, 0, 0, 0, double.NaN);
            LastFits = Array.Empty<Ft8DeepSubtractionFit>();
            return Decode(new Ft8Monitor(Geometry).Analyse(samples), samples);
        }

        return DecodeWithSubtraction(samples, Subtraction);
    }

    /// <summary>
    /// <b>Read the slot, subtract what it gave up, read what is left, and stop by the rule.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>THE STOPPING RULE IS <c>Ft8DeepSubtractionSettings</c>'S AND IS WRITTEN OUT THERE.</b>
    /// Three conditions, any one of which makes a pass the last: the pass budget is spent; the pass
    /// returned nothing new; or nothing in the pass could be subtracted. The third matters because
    /// without it a slot whose only message refuses to give up its symbols would run its whole
    /// budget over a buffer that never changes.
    /// </para>
    /// <para>
    /// <b>THE DUPLICATE RULE, AND IT IS ACROSS PASSES.</b> The port's own <c>seen</c> list is local
    /// to one <c>Decode</c> call, so nothing in the tree stops a later pass returning a message an
    /// earlier pass already returned - and an imperfectly subtracted transmission decodes again out
    /// of its own remnant, which is the ordinary case rather than the exceptional one. A message
    /// whose text a previous pass returned is counted in
    /// <c>Ft8DeepSubtractionCounts.DuplicatesAcrossPasses</c> and is not added to the result.
    /// <b>By text and not by the 77-bit key</b>: the key is not handed back by the gate and the port
    /// recovers it by re-running belief propagation over the ratios that produced the decode, which
    /// is inside a pass and not reachable from here. The text is a deterministic function of the
    /// bits, so key equality implies text equality, and the only way the two rules could differ is
    /// two distinct payloads printing the same string - in which case the operator sees one line
    /// either way and merging them is the correct display.
    /// </para>
    /// <para>
    /// <b>THE CALLER'S BUFFER IS NEVER WRITTEN.</b> The residual is built in a copy. A decode that
    /// silently modified the samples it was handed would make every paired comparison in this
    /// project - where two decoders are given the same array - depend on which ran first.
    /// </para>
    /// <para>
    /// <b>THE FIVE COUNTS ARE SUMMED OVER THE PASSES</b>, and <c>LastSubtraction.PassesRun</c> is
    /// the divisor that makes them readable. This is a real change in what the census means and it
    /// is listed in <c>docs/unit253-subtraction.md</c> §6 as one of the five surfaces that must be
    /// settled before subtraction ships. <b>With subtraction off exactly one pass runs, so the sums
    /// are the single pass's own counts and the default path is bit-for-bit unchanged.</b>
    /// </para>
    /// </remarks>
    private Ft8SlotResult DecodeWithSubtraction(
        ReadOnlySpan<float> samples, Ft8DeepSubtractionSettings settings)
    {
        var working = samples.ToArray();
        var monitor = new Ft8Monitor(Geometry);

        var messages = new List<Ft8SlotMessage>();
        var seen = new List<string>();
        var subtracted = new List<string>();
        var fits = new List<Ft8DeepSubtractionFit>();

        var candidates = 0;
        var parity = 0;
        var checksum = 0;
        var text = 0;
        var duplicatesInPass = 0;

        var passes = 0;
        var offered = 0;
        var removed = 0;
        var refusedSymbols = 0;
        var refusedFrame = 0;
        var duplicatesAcrossPasses = 0;
        var fromLaterPasses = 0;
        var worstDecibels = double.NaN;

        for (var pass = 0; pass < settings.MaxPasses; pass++)
        {
            var result = Decode(monitor.Analyse(working), working);
            passes++;

            candidates += result.CandidateCount;
            parity += result.ParitySatisfiedCount;
            checksum += result.ChecksumPassedCount;
            text += result.BecameTextCount;
            duplicatesInPass += result.DuplicateCount;

            var added = 0;

            foreach (var message in result.Messages)
            {
                if (seen.Contains(message.Text, StringComparer.Ordinal))
                {
                    duplicatesAcrossPasses++;
                    continue;
                }

                seen.Add(message.Text);

                if (messages.Count >= MessageLimit)
                {
                    // The port's own divergence from upstream, reproduced across passes: stop
                    // adding, do not stop the loop, and do not count it as anything.
                    continue;
                }

                messages.Add(message);
                added++;

                if (pass > 0)
                {
                    fromLaterPasses++;
                }
            }

            // STOPPING RULE 2: nothing new came back, so the next residual would be this residual.
            if (added == 0)
            {
                break;
            }

            // STOPPING RULE 1: the budget is spent. Checked here rather than by the loop condition
            // alone so that the last pass does not pay for a subtraction nobody will read.
            if (pass + 1 >= settings.MaxPasses)
            {
                break;
            }

            var removedThisPass = 0;

            foreach (var message in result.Messages)
            {
                if (subtracted.Contains(message.Text, StringComparer.Ordinal))
                {
                    continue;
                }

                offered++;
                var symbols = Ft8DeepMessageSymbols.TryEncode(message.Result.Message);

                if (symbols is null)
                {
                    // A REFUSAL IS A CORRECT ANSWER AND IT IS COUNTED. The message stands in the
                    // result; it is simply not removed from the buffer, so whatever it masks stays
                    // masked. Hiding this is how a stage comes to report a pass it did not make.
                    refusedSymbols++;
                    continue;
                }

                var fit = Fit(working, message, symbols, settings);

                if (!fit.IsFitted)
                {
                    refusedFrame++;
                    continue;
                }

                subtracted.Add(message.Text);
                fits.Add(fit);
                removed++;
                removedThisPass++;

                if (double.IsNaN(worstDecibels) || fit.DecibelsRemoved < worstDecibels)
                {
                    worstDecibels = fit.DecibelsRemoved;
                }
            }

            // STOPPING RULE 3: the buffer did not change, so neither can the answer.
            if (removedThisPass == 0)
            {
                break;
            }
        }

        LastSubtraction = new Ft8DeepSubtractionCounts(
            passes,
            offered,
            removed,
            refusedSymbols,
            refusedFrame,
            duplicatesAcrossPasses,
            fromLaterPasses,
            worstDecibels);

        LastFits = fits;

        return new Ft8SlotResult(candidates, parity, checksum, text, duplicatesInPass, messages);
    }

    /// <summary>
    /// Refines one decoded message's place and subtracts it from the working buffer.
    /// </summary>
    /// <remarks>
    /// <b>THE PLACE ARRIVES IN THREE PARTS AND ALL THREE ARE NEEDED.</b> The candidate's own
    /// frequency and time, quantised to the waterfall's 0.080 s by 3.125 Hz cell;
    /// <see cref="CandidateTimeBiasSeconds"/>, which is exactly minus one symbol period and was
    /// measured by unit 248; and <c>Ft8DeepSignalToNoise.Estimate</c>'s coordinate search, which
    /// moves the window onto the signal rather than onto the analysis cell - unit 251 measured that
    /// not refining reads 3.50 dB out. <b>What the estimator leaves is still not good enough</b>:
    /// its 0.40 Hz frequency step leaves up to ±0.20 Hz, which is 2.53 whole cycles of phase over a
    /// 12.64 s frame, so <c>Ft8DeepMessageSubtractor</c> searches again on its own axis. The
    /// estimator gets the fit into the basin; it does not finish the job.
    /// </remarks>
    private Ft8DeepSubtractionFit Fit(
        float[] working,
        Ft8SlotMessage message,
        byte[] symbols,
        Ft8DeepSubtractionSettings settings)
    {
        var frequency = message.FrequencyHz(Geometry);
        var start = message.TimeSeconds(Geometry) + CandidateTimeBiasSeconds;

        var baseband = Ft8DeepBaseband.Build(working, Geometry.SampleRate, frequency, Baseband);
        var estimate = Ft8DeepSignalToNoise.Estimate(baseband, start, 0.0, symbols, refine: true);

        if (estimate.IsMeasured)
        {
            start += estimate.TimeAdjustmentSeconds;
            frequency += estimate.FrequencyAdjustmentHz;
        }

        return Ft8DeepMessageSubtractor.Subtract(
            working, Geometry.SampleRate, symbols, frequency, start, settings);
    }

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
        if (Subtraction is not null)
        {
            // REFUSED LOUDLY, AND DELIBERATELY NOT COUNTED-AND-CARRIED-ON. See the remarks on
            // Subtraction: a caller who asked for several passes and silently got one has been told
            // a decode ran that did not run.
            throw new InvalidOperationException(
                "This decoder is configured to subtract decoded messages and read the slot again, "
                + "and a waterfall has no samples behind it - magnitudes on a grid, quantised to "
                + "half a decibel, with no phase. There is nothing to fit a carrier phase to, "
                + "nothing to subtract a waveform from, and nothing to re-analyse afterwards. Call "
                + "Decode(ReadOnlySpan<float>), which is the entry point that has the audio, or "
                + "build this decoder with subtraction null. Nothing has been decoded.");
        }

        LastSubtraction = new Ft8DeepSubtractionCounts(1, 0, 0, 0, 0, 0, 0, double.NaN);
        LastFits = Array.Empty<Ft8DeepSubtractionFit>();
        return Decode(waterfall, ReadOnlySpan<float>.Empty);
    }

    /// <summary>
    /// The whole loop, with the audio behind the waterfall when there is any and an empty span when
    /// there is not.
    /// </summary>
    /// <remarks>
    /// <b>One body and not two.</b> The two public entry points differ in exactly one thing - whether
    /// the samples came with the waterfall - and a second copy of this loop would be a second thing
    /// to drift.
    /// </remarks>
    private Ft8SlotResult Decode(Ft8Waterfall waterfall, ReadOnlySpan<float> samples)
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

        var offered = 0;
        var produced = 0;
        var accepted = 0;
        var reencodings = 0L;

        var fineOffered = 0;
        var fineResynced = 0;
        var fineAccepted = 0;
        var fineTimeEdges = 0;
        var fineFrequencyEdges = 0;
        var fineNoSamples = 0;
        var fineTimeTotal = 0.0;
        var fineFrequencyTotal = 0.0;
        var fineTimeWorst = 0.0;
        var fineFrequencyWorst = 0.0;

        // ONE BASEBAND PER MIXING FREQUENCY, not one per candidate. The mixing and the 401-tap
        // filter are the expensive part and they depend only on where the eight tones sit, so two
        // candidates in the same bin at different times share one. Cleared with the slot.
        var basebands = _fineSync is null
            ? null
            : new Dictionary<(int Bin, int Sub), Ft8DeepBaseband>();

        var hearings = RemembersHearings
            ? new List<Ft8DeepHearing>(candidates.Count)
            : null;

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            // KEPT BEFORE ANYTHING IS DECIDED, and a copy rather than the buffer, which is re-used on
            // the next candidate. Nothing about this changes what happens below.
            hearings?.Add(new Ft8DeepHearing(candidate, (float[])ratios.Clone()));

            var result = Ft8CodewordDecoder.Decode(ratios, cache, MaxIterations);
            byte[]? osdKey = null;

            // THE ORDERED STATISTICS STAGE, at the only place it can go: belief propagation has
            // just given up on a candidate whose answer may still be reachable. Where it converged,
            // the port's answer stands untouched and OSD is never asked.
            if (_statistics is not null
                && Osd is not null
                && result.Status == Ft8CodewordStatus.ParityNeverSatisfied)
            {
                offered++;

                // THE STOPPING RULE FOR A CANDIDATE: the search is exhaustive over every subset of
                // the window up to the order, and then it stops. It always produces exactly one
                // codeword - the best by soft distance - and that one codeword is submitted once.
                // There is no retry, no second order and no second submission. THE WINDOW CANNOT
                // MOVE THAT: it narrows which patterns are ranked, never how many answers come back
                // - docs/unit252-osd-window.md section 4.
                var found = _statistics.Decode(ratios, Osd.Order, _osdCodeword, Osd.Window);
                reencodings += found.Reencodings;
                produced++;

                // ONE CODEWORD TO THE GATE, AND THE GATE IS THE PORT'S. Every codeword put to the
                // CRC-14 is an independent chance of a false accept at about one in 16384;
                // submitting a search's worth would put tens of messages nobody sent in front of
                // the operator every slot, each carrying a valid checksum.
                Ft8DeepOrderedStatistics.Saturate(_osdCodeword, _osdRatios);
                var gated = Ft8CodewordDecoder.Decode(_osdRatios, cache, MaxIterations);

                if (gated.Decoded)
                {
                    accepted++;
                    result = gated;

                    // THE KEY IS THE CODEWORD OSD ALREADY HAS. The port recovers its key by
                    // re-running belief propagation over the original ratios, which cannot work for
                    // exactly the candidates OSD rescued - it did not converge on them and will not
                    // - and would return the same message twice.
                    osdKey = _osdCodeword[..Ft8Payload.MessageBits];
                }

                // A codeword the port refused leaves the port's own verdict standing, so the five
                // counts stay a report on the port's belief propagation and OSD's three counts
                // carry OSD's story. Nothing here overrides a refusal.
            }

            // THE FINE SYNCHRONISATION STAGE, at the only place it can go and under the same rule as
            // OSD: the port has just refused this candidate, so there is nothing here to overwrite.
            // Where the port DECODED, this never runs and the port's answer stands untouched.
            byte[]? fineKey = null;

            if (_fineSync is not null && result.Status != Ft8CodewordStatus.Decoded)
            {
                fineOffered++;

                if (samples.IsEmpty)
                {
                    // A WATERFALL HAS NO SAMPLES BEHIND IT. Not an error and not a pretence: the
                    // count says so and the loop carries on doing exactly what unit 247 did.
                    fineNoSamples++;
                }
                else
                {
                    var mixedAt = (candidate.BinOffset, candidate.FrequencySubOffset);
                    if (!basebands!.TryGetValue(mixedAt, out var baseband))
                    {
                        baseband = Ft8DeepBaseband.Build(
                            samples,
                            Geometry.SampleRate,
                            Geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset),
                            Baseband);

                        basebands[mixedAt] = baseband;
                    }

                    var found = _fineSync.Search(
                        baseband,
                        Geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset)
                            + CandidateTimeBiasSeconds);

                    fineResynced++;
                    fineTimeTotal += Math.Abs(found.TimeShiftSeconds);
                    fineFrequencyTotal += Math.Abs(found.FrequencyShiftHz);
                    fineTimeWorst = Math.Max(fineTimeWorst, Math.Abs(found.TimeShiftSeconds));
                    fineFrequencyWorst =
                        Math.Max(fineFrequencyWorst, Math.Abs(found.FrequencyShiftHz));

                    if (found.OnTimeEdge)
                    {
                        fineTimeEdges++;
                    }

                    if (found.OnFrequencyEdge)
                    {
                        fineFrequencyEdges++;
                    }

                    // ONE CODEWORD TO THE GATE, AND THE GATE IS THE PORT'S. Exactly one submission
                    // per candidate re-synced, so the expected false accepts stay at about the
                    // candidate count in 16384 rather than at a search's worth.
                    Ft8DeepBasebandExtractor.Extract(
                        baseband,
                        found.StartSeconds,
                        found.FrequencyOffsetHz,
                        _fineRatios,
                        _fineGrid);

                    Ft8SoftSymbols.Normalise(_fineRatios);
                    var gated = Ft8CodewordDecoder.Decode(_fineRatios, cache, MaxIterations);

                    if (gated.Decoded)
                    {
                        fineAccepted++;
                        result = gated;

                        // THE KEY COMES FROM THE RE-SYNCED RATIOS, not from the coarse ones. The
                        // port recovers its key by re-running belief propagation over the ratios
                        // that produced the decode, and for a candidate this stage rescued those
                        // are these - the coarse ones did not converge and will not.
                        LdpcDecoder.Decode(_fineRatios, codeword, MaxIterations);
                        fineKey = codeword[..Ft8Payload.MessageBits];
                    }
                }
            }

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

            byte[] key;
            if (fineKey is not null)
            {
                key = fineKey;
            }
            else if (osdKey is not null)
            {
                key = osdKey;
            }
            else
            {
                // The port's de-duplication key, recovered the port's way. The gate does not hand
                // back the bits it accepted, so they are read out of a second run of the same
                // deterministic decoder over the same ratios. It is not a second checksum check.
                LdpcDecoder.Decode(ratios, codeword, MaxIterations);
                key = codeword[..Ft8Payload.MessageBits];
            }

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

        // THE STOPPING RULE FOR A SLOT: the candidate list runs out. There is no early exit, no time
        // budget and no cap on how many candidates OSD is offered - so the worst-case cost of a slot
        // is the search's candidate limit times the order's re-encoding count, which is a number
        // that can be measured rather than a policy that has to be believed.
        LastOsd = new Ft8DeepOsdCounts(offered, produced, accepted, reencodings);
        LastFineSync = new Ft8DeepFineSyncCounts(
            fineOffered,
            fineResynced,
            fineAccepted,
            fineTimeEdges,
            fineFrequencyEdges,
            fineNoSamples,
            fineTimeTotal,
            fineFrequencyTotal,
            fineTimeWorst,
            fineFrequencyWorst);

        LastHearings = hearings is null
            ? Array.Empty<Ft8DeepHearing>()
            : hearings;

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
