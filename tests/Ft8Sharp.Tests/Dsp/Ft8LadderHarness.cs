using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Fixtures;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The ladder with a handle on it: one rung, a trial count and a seed, and three counts back —
/// decoded, missed and <em>returned wrong</em>.</b> Every unit of this phase measures with this.
/// </summary>
/// <remarks>
/// <para>
/// <b>This EXTENDS <see cref="Ft8Step6Ladder"/>; it does not replace it and it is not a second
/// instrument.</b> The message source, the synthesiser, the noise, the calibration and the seed
/// arithmetic are all that file's, called rather than copied. That is deliberate and it is the whole
/// design constraint: a rebuilt ladder is a different measurement, and the 13 of 306 at -21 dB that
/// <c>HM-OPEN-067</c> carries would stop being the thing a later unit is compared against.
/// </para>
/// <para>
/// <b>It reproduces <see cref="Ft8Step6Ladder.Walk"/> exactly, and that is arithmetic rather than a
/// hope.</b> <c>Walk</c> makes one <see cref="GaussianNoise"/> per seed at
/// <c>Seeds[s] + round(rung * 10)</c> and then draws from it once per message, in the population's
/// fixed order, so a trial's noise depends on how many trials came before it inside its block.
/// <see cref="Ft8Step6Ladder.Seeds"/> is <c>221001</c> through <c>221006</c>, consecutive, so
/// <c>Seeds[s]</c> is <c>221001 + s</c> and <see cref="DefaultSeed"/> plus the block index is the
/// same number. <b>This harness therefore walks whole blocks of the population in the same order,
/// and at 306 trials on a collapse rung it draws the same noise, in the same sequence, as the curve
/// test did.</b> Change either of those things and the reproduction is gone.
/// </para>
/// <para>
/// <b>THREE COUNTS, NEVER TWO</b> (<c>PHASE_PLAN.md</c> §0.0, and this phase's ruling that a wrong
/// decode is counted separately from a missed one everywhere). <see cref="Result.Decoded"/> and
/// <see cref="Result.Missed"/> partition the trials; <see cref="Result.Wrong"/> counts messages
/// returned that were not sent and <b>is not a partition of anything</b> — a slot can return the
/// right message and a wrong one at once, and both facts are true. Every wrong return is kept in
/// <see cref="Result.WrongReturns"/> with the message sent beside the message returned, so the count
/// always has evidence under it.
/// </para>
/// <para>
/// <b>Two decoders, side by side, on the same samples.</b> <see cref="Decoder"/> names an
/// implementation; <see cref="Available"/> returns the ones this tree has. Since unit 245 that is
/// <c>Ft8Sharp</c> and <c>Ft8Sharp.Deep</c>, and every trial runs both decoders over the <em>same</em>
/// mixed samples — which is a paired comparison and worth far more than two independent runs, since
/// the noise draw is held identical between them. <b>Adding the sibling was one entry in that method
/// and one project reference, and no caller in this tree changed.</b>
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
internal static class Ft8LadderHarness
{
    /// <summary>
    /// <b>The seed that makes this harness agree with the curve test.</b> The first of
    /// <see cref="Ft8Step6Ladder.Seeds"/>; block <c>s</c> uses <c>seed + s</c>, which is that array.
    /// </summary>
    internal const int DefaultSeed = 221_001;

    /// <summary>Exactly on a bin centre, as <see cref="Ft8Step6CurveTests"/> has it.</summary>
    internal const double DefaultFrequencyHz = 1000.0;

    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>A whole number of symbol periods in, as <see cref="Ft8Step6CurveTests"/> has it.</summary>
    internal static int DefaultOffsetSamples => Ft8Waveform.SamplesPerSymbol(Rate) * 3;

    /// <summary>One decoder the ladder can be walked with.</summary>
    /// <param name="Name">What it is called in the report.</param>
    /// <param name="Decode">Samples in, result out. Nothing else is told to it.</param>
    internal sealed record Decoder(string Name, Func<float[], Ft8SlotResult> Decode);

    /// <summary>
    /// One message returned that was not sent. <b>Reported on its own line, always.</b>
    /// </summary>
    /// <param name="Trial">Which trial of the run, zero-based, so it can be re-run alone.</param>
    /// <param name="Seed">The seed of the block it came from.</param>
    /// <param name="Sent">The message the synthesiser was given.</param>
    /// <param name="Returned">The message the decoder handed back.</param>
    internal sealed record WrongReturn(int Trial, int Seed, string Sent, string Returned)
    {
        /// <summary>The line the report prints, sent and returned side by side.</summary>
        public override string ToString() =>
            $"    trial {Trial,5}  seed {Seed}  SENT \"{Sent}\"  RETURNED \"{Returned}\"";
    }

    /// <summary>What one decoder did at one rung.</summary>
    internal sealed class Result(string decoder, double requested, int trials)
    {
        private readonly List<double> _delivered = new(trials);

        /// <summary>Which decoder this row is.</summary>
        internal string Decoder { get; } = decoder;

        /// <summary>The rung asked for, in decibels in the 2500 Hz reference bandwidth.</summary>
        internal double Requested { get; } = requested;

        /// <summary>Trials asked for.</summary>
        internal int Trials { get; } = trials;

        /// <summary><b>Count one of three:</b> the message sent came back.</summary>
        internal int Decoded { get; private set; }

        /// <summary><b>Count two of three:</b> the message sent did not come back.</summary>
        internal int Missed { get; private set; }

        /// <summary>
        /// <b>Count three of three:</b> a message came back that was not sent. Not a partition —
        /// see the remarks on <see cref="Ft8LadderHarness"/>.
        /// </summary>
        internal int Wrong { get; private set; }

        /// <summary>Every wrong return, with what was sent beside it.</summary>
        internal List<WrongReturn> WrongReturns { get; } = new();

        /// <summary>How long the whole rung took on the wall clock.</summary>
        internal TimeSpan Elapsed { get; set; }

        /// <summary>The mean ratio actually put on the samples. <b>The row is read at this.</b></summary>
        internal double DeliveredMean => _delivered.Count == 0 ? double.NaN : _delivered.Average();

        /// <summary>The largest requested-versus-delivered error over the run, in decibels.</summary>
        internal double WorstDeliveryError =>
            _delivered.Count == 0 ? double.NaN : _delivered.Max(d => Math.Abs(d - Requested));

        /// <summary>Decodes as a percentage of trials.</summary>
        internal double Rate => Trials == 0 ? 0.0 : 100.0 * Decoded / Trials;

        /// <summary>The 95 per cent Wilson score interval on <see cref="Rate"/>.</summary>
        internal (double Lower, double Upper) Interval => Ft8Step6Ladder.Wilson(Decoded, Trials);

        /// <summary>Milliseconds a slot decode cost, which is what every later unit pays.</summary>
        internal double MillisecondsPerTrial =>
            Trials == 0 ? double.NaN : Elapsed.TotalMilliseconds / Trials;

        internal void Add(int trial, int seed, double delivered, bool decoded, string sent, IReadOnlyList<string> wrong)
        {
            _delivered.Add(delivered);

            if (decoded)
            {
                Decoded++;
            }
            else
            {
                Missed++;
            }

            foreach (var text in wrong)
            {
                Wrong++;
                WrongReturns.Add(new WrongReturn(trial, seed, sent, text));
            }
        }

        /// <summary>The one line this rung reports as.</summary>
        internal string AsRow()
        {
            var (lower, upper) = Interval;
            return $"{Decoder,-12} {Requested,9:F1} {DeliveredMean,10:F3} {Trials,7} "
                + $"{Decoded,8} {Missed,7} {Wrong,6} {Rate,7:F1} {lower,7:F1} {upper,7:F1} "
                + $"{Elapsed.TotalSeconds,8:F1} {MillisecondsPerTrial,8:F1}";
        }
    }

    /// <summary>The header <see cref="Result.AsRow"/> lines up under.</summary>
    internal const string Header =
        "decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95  "
        + "  wall s    ms/tr";

    /// <summary>
    /// <b>The decoders this tree has, in the order a report prints them.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Two entries since unit 245, and the second one is the whole of step 1.</b>
    /// <c>Ft8Sharp.Deep</c> now exists and takes the seat this method was written to hold open for
    /// it. Every caller in the tree started reporting the pair side by side without being changed —
    /// nothing else in this file, and nothing at any call site, knows how many decoders there are.
    /// </para>
    /// <para>
    /// <b>Tonight the two columns are identical and that is the point rather than a result.</b>
    /// <c>Ft8DeepSlotDecoder</c> delegates to an <see cref="Ft8SlotDecoder"/>, so the pair is one
    /// decoder called twice; the phase ruling is that <c>Ft8Sharp</c> stays a byte-identical port
    /// and every improvement lands in the sibling instead. From the unit that takes step 2, a
    /// difference between these two columns is attributable to exactly one named change, which is
    /// what the second seat buys and is worth far more than two independent runs, since
    /// <see cref="Run"/> holds the noise draw identical between them.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<Decoder> Available()
    {
        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();
        return new[]
        {
            new Decoder("Ft8Sharp", samples => port.Decode(samples)),
            new Decoder("Ft8Sharp.Deep", samples => deep.Decode(samples)),
        };
    }

    /// <summary>
    /// <b>THE ENTRY POINT. One rung, a trial count, a seed. Deterministic.</b>
    /// </summary>
    /// <param name="rungDecibels">
    /// The ratio to deliver, in decibels in the 2500 Hz reference bandwidth. <b>Requested; the row
    /// reports what was actually delivered and the two are not assumed equal.</b>
    /// </param>
    /// <param name="trials">
    /// How many trials. <b>The population is 51 messages, so 306 is six whole blocks and is the
    /// count <c>HM-OPEN-067</c>'s figure was taken at.</b> A count that is not a multiple of 51
    /// leaves a partial last block, which is still deterministic but is not comparable to a whole
    /// one.
    /// </param>
    /// <param name="seed">
    /// The base seed. Block <c>s</c> draws its noise from <c>seed + s + round(rung * 10)</c>.
    /// <see cref="DefaultSeed"/> reproduces <see cref="Ft8Step6Ladder.Walk"/> exactly.
    /// </param>
    /// <param name="decoders">Which decoders to walk. <see cref="Available"/> by default.</param>
    /// <param name="frequencyHz">Where the synthesiser puts the lowest tone.</param>
    /// <param name="offsetSamples">Where in the slot the transmission begins.</param>
    /// <param name="log">Optional, called once per completed decoder with its row.</param>
    /// <returns>One <see cref="Result"/> per decoder, in <paramref name="decoders"/>' order.</returns>
    /// <remarks>
    /// <para>
    /// <b>The audio is synthesised once per trial and every decoder is given the same array.</b> The
    /// comparison is paired: where two decoders differ, the difference is the decoder, because the
    /// noise draw was identical and not merely drawn from the same distribution.
    /// </para>
    /// <para>
    /// <b>Nothing is told to the decode path.</b> The frequency and the offset go to the synthesiser;
    /// each decoder is handed the samples and nothing else. The truth is used once, after the code
    /// has answered, to compare the text — which is <see cref="Ft8Step6Ladder.Walk"/>'s rule and
    /// stays.
    /// </para>
    /// <para>
    /// <b>A rung that returns nothing is a measurement, not a failure.</b> Nothing here throws on a
    /// poor result and nothing here asserts a bound.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<Result> Run(
        double rungDecibels,
        int trials,
        int seed = DefaultSeed,
        IReadOnlyList<Decoder>? decoders = null,
        double frequencyHz = DefaultFrequencyHz,
        int? offsetSamples = null,
        Action<string>? log = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trials);

        var used = decoders ?? Available();
        var offset = offsetSamples ?? DefaultOffsetSamples;
        var population = Ft8Step6Ladder.Population();
        var results = used.Select(d => new Result(d.Name, rungDecibels, trials)).ToArray();
        var clocks = used.Select(_ => new Stopwatch()).ToArray();
        var rungOffset = (int)Math.Round(rungDecibels * 10.0);

        var trial = 0;
        for (var block = 0; trial < trials; block++)
        {
            // The seed depends only on the rung and the block, never on iteration order, so a fresh
            // process walking the same rung draws the same noise. This is Walk's line, and the
            // arithmetic that makes seed + block equal Ft8Step6Ladder.Seeds[block] is in the remarks.
            var blockSeed = seed + block + rungOffset;
            var noise = new GaussianNoise(blockSeed);

            foreach (var entry in population)
            {
                if (trial >= trials)
                {
                    break;
                }

                var (clean, _) = SearchFixture.OneSignal(Rate, entry, frequencyHz, offset);
                var signalPower = SearchFixture.TransmissionPower(Rate, entry, frequencyHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rungDecibels, Rate);
                var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, Rate);
                var sent = Ft8MessageDecoder.Decode(entry.Message).Text;

                for (var d = 0; d < used.Count; d++)
                {
                    clocks[d].Start();
                    var result = used[d].Decode(mixed);
                    clocks[d].Stop();

                    var decoded = result.Texts.Contains(sent, StringComparer.Ordinal);
                    var wrong = result.Texts
                        .Where(t => !string.Equals(t, sent, StringComparison.Ordinal))
                        .ToArray();

                    results[d].Add(trial, blockSeed, delivered, decoded, sent, wrong);
                }

                trial++;
            }
        }

        for (var d = 0; d < results.Length; d++)
        {
            results[d].Elapsed = clocks[d].Elapsed;
            log?.Invoke(results[d].AsRow());
        }

        return results;
    }

    /// <summary>
    /// The whole report for one run, header and rows and every wrong return on its own line, ready
    /// to be written straight into a unit's output.
    /// </summary>
    /// <remarks>
    /// <b>A rate is never printed without its wrong-decode count</b>, which is why this exists rather
    /// than each caller assembling its own lines and one of them forgetting.
    /// </remarks>
    internal static IEnumerable<string> Report(IReadOnlyList<Result> results)
    {
        yield return Header;

        foreach (var result in results)
        {
            yield return result.AsRow();
        }

        yield return string.Empty;
        yield return "  DECODED + MISSED = trials. WRONG is not part of that partition: a slot can";
        yield return "  return the right message and a wrong one at once, and both are true.";
        yield return "  lo 95 / hi 95 are the WILSON score interval on the decode rate.";
        yield return string.Empty;

        foreach (var result in results)
        {
            // The rung is named as well as the decoder: a report of several rungs otherwise prints
            // the same line three times and a reader cannot tell which one is which.
            var who = $"{result.Decoder} at {result.Requested:F1} dB";

            if (result.WrongReturns.Count == 0)
            {
                yield return $"  {who}: NO WRONG DECODES. 0 messages returned that were not sent.";
                continue;
            }

            yield return $"  {who}: {result.WrongReturns.Count} WRONG, each on its own line:";

            foreach (var wrong in result.WrongReturns)
            {
                yield return wrong.ToString();
            }
        }
    }

    // =====================================================================================
    // THE OTHER HALF OF THE SCOREBOARD: REAL AIR, SCORED AGAINST A COMMITTED FIXTURE.
    //
    // Everything above this line is the ladder, which knows what it transmitted. Everything
    // below is a real capture scored against what another decoder returned for it, read out
    // of a file Tim committed from the shack. THE TWO ARE DIFFERENT MEASUREMENTS and the
    // report must never add their numbers together - see the remarks on FixtureScore.Wrong.
    // =====================================================================================

    /// <summary>What one decoder made of one committed capture, against the fixture's rows.</summary>
    /// <remarks>
    /// <para>
    /// <b>THREE COUNTS, NEVER TWO</b>, exactly as the ladder has it.
    /// <see cref="Matched"/> and <see cref="Missed"/> partition the fixture's rows;
    /// <see cref="Wrong"/> counts messages this decoder returned that the fixture does not carry, and
    /// every one of them is kept in <see cref="ReturnedWrong"/> so the count always has evidence
    /// under it.
    /// </para>
    /// <para>
    /// <b>BUT THE THIRD COUNT MEANS SOMETHING DIFFERENT HERE THAN IT DOES ON THE LADDER, AND THE TWO
    /// MUST NEVER BE MERGED.</b> On the ladder the harness <em>knows what it transmitted</em>, so a
    /// message returned that was not sent is an error, and the phase's zero-wrong criterion (§0.0)
    /// bites on it. On a real capture the fixture is <em>another decoder's list</em>, not the air's:
    /// a message WSJT-X missed and Hamlet found is a decode this phase is actively trying to produce.
    /// <b>So this count is weaker evidence than the ladder's.</b> It is printed, on its own line, with
    /// the message, and it is looked at - it is not scored as a fault and it is not compared against
    /// the ladder's zero.
    /// </para>
    /// </remarks>
    internal sealed class FixtureScore(string decoder, Ft8CaptureFixture fixture)
    {
        /// <summary>Which decoder this row is.</summary>
        internal string Decoder { get; } = decoder;

        /// <summary>The fixture scored against, so the row can name its provenance.</summary>
        internal Ft8CaptureFixture Fixture { get; } = fixture;

        /// <summary><b>Count one of three:</b> a message in the fixture that this decoder also returned.</summary>
        internal List<string> Matched { get; } = [];

        /// <summary><b>Count two of three:</b> a message in the fixture that this decoder did not return.</summary>
        internal List<string> Missed { get; } = [];

        /// <summary>
        /// <b>Count three of three:</b> a message this decoder returned that is not in the fixture.
        /// <b>Not a fault on a real capture</b> - see the remarks on this type.
        /// </summary>
        internal List<string> ReturnedWrong { get; } = [];

        /// <summary>How long this decoder took over the capture.</summary>
        internal TimeSpan Elapsed { get; set; }

        /// <summary>Rows in the fixture. <c>Matched + Missed</c> and nothing else.</summary>
        internal int Rows => Matched.Count + Missed.Count;

        /// <summary>Matched as a percentage of the fixture's rows.</summary>
        internal double Rate => Rows == 0 ? 0.0 : 100.0 * Matched.Count / Rows;

        /// <summary>The one line this decoder reports as.</summary>
        internal string AsRow() =>
            $"{Decoder,-12} {Rows,6} {Matched.Count,8} {Missed.Count,7} {ReturnedWrong.Count,6} "
            + $"{Rate,7:F1} {Elapsed.TotalSeconds,8:F1}";
    }

    /// <summary>The header <see cref="FixtureScore.AsRow"/> lines up under.</summary>
    internal const string FixtureHeader =
        "decoder        rows  MATCHED  MISSED  WRONG    rate   wall s";

    /// <summary>
    /// <b>THE ENTRY POINT FOR A CLAIM AGAINST WSJT-X.</b> Refuses anything whose provenance is not a
    /// real WSJT-X run, then scores.
    /// </summary>
    /// <remarks>
    /// This is the call a unit makes when it wants to say <em>Hamlet found n of the m messages
    /// WSJT-X found</em>. <see cref="Ft8CaptureFixture.RequireScorable"/> is what stops that sentence
    /// ever being produced from a worked example. <see cref="Compare"/> does the same arithmetic
    /// without the claim, and says so on every line it prints.
    /// </remarks>
    internal static IReadOnlyList<FixtureScore> ScoreFixture(
        Ft8CaptureFixture fixture,
        IReadOnlyList<Decoder>? decoders = null,
        Action<string>? log = null)
    {
        fixture.RequireScorable("Scoring Hamlet against this fixture");
        return Compare(fixture, decoders, log);
    }

    /// <summary>
    /// <b>Decodes the fixture's capture with every decoder <see cref="Available"/> returns and counts
    /// three things per decoder.</b> Makes no claim about what produced the fixture's rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Every decoder, never an index.</b> Nothing here knows how many decoders there are; it
    /// iterates <paramref name="decoders"/> and emits one <see cref="FixtureScore"/> per entry, in
    /// order. <b>When step 1 adds <c>Ft8Sharp.Deep</c> to <see cref="Available"/>, this reports two
    /// rows with no change here and none at any call site</b> - which is asserted rather than
    /// asserted-in-a-comment, by <c>Ft8FixtureScoringTests</c> passing a two-entry list.
    /// </para>
    /// <para>
    /// <b>The capture is read the way the decoder wants it</b>: one float per sixteen-bit count,
    /// divided by 32768, which is upstream's own <c>load_wav</c> scaling and the same scaling
    /// <see cref="ReferenceRecording.ReadSamples"/> applies. A differently-scaled signal is a
    /// different measurement.
    /// </para>
    /// <para>
    /// <b>The sample rate is checked here because nothing else checks it.</b> <c>WavFile</c> reads the
    /// rate and compares it against nothing, and every committed capture in this repository today is
    /// CW's at 48 kHz. A capture at the wrong rate decoded anyway would report a real decoder against
    /// audio it was never given, and the miss count would look like a sensitivity result.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<FixtureScore> Compare(
        Ft8CaptureFixture fixture,
        IReadOnlyList<Decoder>? decoders = null,
        Action<string>? log = null)
    {
        // Refusals 1 and 2: the capture must be there and must be the audio these rows were
        // measured on. Both throw; neither returns an empty result.
        var capturePath = fixture.RequireCapture();

        if (fixture.SampleRate != Rate)
        {
            throw new Ft8FixtureException(
                fixture.FixturePath,
                fixture.CaptureName,
                $"it declares {fixture.SampleRate} samples per second and this decode path is built on "
                + $"{Rate}. Decoding it anyway would measure a real decoder against audio it was never "
                + "given, and the misses would read as a sensitivity result.");
        }

        var contents = WavFile.Read(capturePath);
        if (contents.SampleRate != fixture.SampleRate)
        {
            throw new Ft8FixtureException(
                fixture.FixturePath,
                fixture.CaptureName,
                $"the fixture says {fixture.SampleRate} samples per second and the capture's own fmt "
                + $"chunk says {contents.SampleRate}. One of the two is about a different file.");
        }

        var samples = new float[contents.Samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = contents.Samples[i] / 32768.0f;
        }

        var used = decoders ?? Available();
        var expected = fixture.Messages;
        var scores = new List<FixtureScore>(used.Count);

        foreach (var decoder in used)
        {
            var clock = Stopwatch.StartNew();
            var result = decoder.Decode(samples);
            clock.Stop();

            // The fixture reader already normalised its side. The decoder's side goes through the
            // SAME function - ReferenceRecording.Normalise, called and not re-implemented - so both
            // sides of the comparison are laundered by exactly one rule, and that rule is the one
            // upstream's own expected lists are read with.
            var returned = result.Texts
                .Select(ReferenceRecording.Normalise)
                .Where(t => t.Length > 0)
                .ToArray();

            var score = new FixtureScore(decoder.Name, fixture) { Elapsed = clock.Elapsed };

            foreach (var message in expected)
            {
                if (returned.Contains(message, StringComparer.Ordinal))
                {
                    score.Matched.Add(message);
                }
                else
                {
                    score.Missed.Add(message);
                }
            }

            foreach (var message in returned)
            {
                if (!expected.Contains(message, StringComparer.Ordinal))
                {
                    score.ReturnedWrong.Add(message);
                }
            }

            scores.Add(score);
            log?.Invoke(score.AsRow());
        }

        return scores;
    }

    /// <summary>
    /// The whole report for one fixture: what it was, header, a row per decoder, and every
    /// returned-wrong message on its own line.
    /// </summary>
    /// <remarks>
    /// <b>A matched count is never printed without its returned-wrong count</b>, which is why this
    /// exists rather than each caller assembling its own lines and one of them forgetting. And the
    /// provenance is printed at the top every time: a reader who sees these counts must be able to
    /// tell, without leaving the report, whether the thing on the other side of the comparison was
    /// WSJT-X or a worked example.
    /// </remarks>
    internal static IEnumerable<string> FixtureReport(IReadOnlyList<FixtureScore> scores)
    {
        if (scores.Count == 0)
        {
            yield return "  NO DECODERS. Available() returned nothing, which is not a result.";
            yield break;
        }

        var fixture = scores[0].Fixture;

        yield return $"  capture     {fixture.CaptureName}  ({fixture.Utc}, {fixture.SampleRate} Hz)";
        yield return $"  sha256      {fixture.Sha256}";
        yield return fixture.IsRealWsjtxRun
            ? $"  provenance  {fixture.Provenance} - a real WSJT-X run: {fixture.Generator}"
            : $"  provenance  {fixture.Provenance} - NOT WSJT-X. These rows are \"{fixture.Generator}\" "
                + "and no claim about WSJT-X may be made from these counts.";
        yield return string.Empty;

        yield return FixtureHeader;

        foreach (var score in scores)
        {
            yield return score.AsRow();
        }

        yield return string.Empty;
        yield return "  MATCHED + MISSED = rows in the fixture. WRONG is not part of that partition.";
        yield return "  WRONG HERE IS NOT THE LADDER'S WRONG. The ladder knows what it transmitted, so";
        yield return "  a message it did not send is an error. This fixture is another decoder's list,";
        yield return "  so a message it missed and Hamlet found is a decode this phase is TRYING to";
        yield return "  produce. Printed and looked at; never scored as a fault, never added to the";
        yield return "  ladder's zero-wrong count.";
        yield return string.Empty;

        foreach (var score in scores)
        {
            if (score.ReturnedWrong.Count == 0)
            {
                yield return $"  {score.Decoder}: 0 messages returned that the fixture does not carry.";
                continue;
            }

            yield return
                $"  {score.Decoder}: {score.ReturnedWrong.Count} returned that the fixture does not "
                + "carry, each on its own line:";

            foreach (var message in score.ReturnedWrong)
            {
                yield return $"    RETURNED \"{message}\"  NOT IN FIXTURE";
            }
        }

        foreach (var score in scores)
        {
            if (score.Missed.Count == 0)
            {
                continue;
            }

            yield return $"  {score.Decoder}: {score.Missed.Count} in the fixture and not returned:";

            foreach (var message in score.Missed)
            {
                yield return $"    MISSED   \"{message}\"";
            }
        }
    }
}
