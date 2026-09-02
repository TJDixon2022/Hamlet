using Ft8Sharp.Dsp;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Step 6's ladder: the rungs, the trial counts, the population, the published figure and the
/// verdict band — all of them written down here, in the tree, before the curve was ever run.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This file is the reason the measurement is worth anything, and it is deliberately separate from
/// the test that produces the curve.</b> A curve read first and judged afterwards is not a verdict; it
/// is a rationalisation. Unit 212 established the pattern on this project — measure the maximum, print
/// it, and only then assert a bound — and this is the same rule applied to a decode rate. Every
/// constant below was committed before <see cref="Ft8Step6CurveTests"/> executed once.
/// </para>
/// <para>
/// <b>It is not <see cref="SensitivityLadder"/> and it does not replace it.</b> That ladder says in
/// its own header that it is unit 218's diagnostic and claims none of step 6's criteria, and two other
/// test classes are calibrated against its rungs. Changing it would move measurements that are already
/// evidence. This is a second, wider ladder that claims the criteria; the instrument underneath both —
/// <see cref="SignalToNoise"/>, <see cref="GaussianNoise"/>, <see cref="SearchFixture"/> — is the same
/// one unit 214 built, because two noise conventions in one tree is how a measurement quietly stops
/// being comparable.
/// </para>
/// </remarks>
internal static class Ft8Step6Ladder
{
    /// <summary>
    /// <b>The ratio step 6's second criterion names, in decibels in the 2500 Hz reference bandwidth.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Where the figure comes from, stated because a figure quoted as published without a source is
    /// not a published figure.</b> <c>PHASE_PLAN.md</c>'s step 6 names <b>-21 dB</b> and calls it the
    /// published threshold. The convention it is quoted in is written down in
    /// <see cref="SignalToNoise"/> and is the amateur weak-signal one: signal power over noise power
    /// in a <b>2500 Hz</b> reference bandwidth. A threshold quoted in that mode conventionally means
    /// the ratio at which the decode probability is about <b>50 per cent</b>.
    /// </para>
    /// <para>
    /// <b>The provenance the tree can support, and the part it cannot.</b> The primary source for FT8's
    /// published sensitivity is the QEX paper this project already cites in <c>NOTICE</c> — Franke
    /// K9AN, Somerville G4WJS, Taylor K1JT, <i>The FT4 and FT8 Communication Protocols</i>, QEX,
    /// July/August 2020. <b>That paper is not on this machine and no copy of the WSJT-X documentation
    /// is either</b>, so unit 221 could not open either one and read the number off the page.
    /// <see cref="Ft8Step6CurveTests"/> prints that fact rather than hiding it. <b>The -21 dB and the
    /// 50 per cent are therefore taken from the plan and stated as an assumption in those words</b>,
    /// not as a citation. The licensing boundary forbids going to <c>ft4_ft8_public/</c> or WSJT-X
    /// source for it, and a figure is a published number that comes from a paper or a manual rather
    /// than from somebody's code, so no route around that was attempted.
    /// </para>
    /// </remarks>
    internal const double PublishedThresholdDecibels = -21.0;

    /// <summary>
    /// <b>The verdict band for criterion 2, fixed by unit 221's instruction before the run and not to
    /// be moved after seeing the result.</b>
    /// </summary>
    /// <remarks>
    /// <code>
    ///   decode rate at -21 dB, delivered:
    ///      >= 40 %   criterion 2 MET
    ///      25 - 40 % criterion 2 PARTIAL - the arbiter judges it, not the unit
    ///      &lt;  25 % criterion 2 NOT MET - a clear shortfall, and that is a finding
    /// </code>
    /// <b>The raw rate and its trial count are reported whatever they are</b>, so the owner can apply
    /// his own reading of <em>comparable</em> to the same number.
    /// </remarks>
    internal const double MetAtOrAbovePercent = 40.0;

    /// <inheritdoc cref="MetAtOrAbovePercent"/>
    internal const double NotMetBelowPercent = 25.0;

    /// <summary>
    /// <b>The rungs. One decibel apart through the collapse, because two cannot resolve it.</b>
    /// </summary>
    /// <remarks>
    /// Unit 218's diagnostic went 100 per cent at -18, 25 at -20, 3.8 at -21 and 0 at -22 — the whole
    /// collapse inside four decibels on a ladder whose step was two. <b>-16 to -24 at 1 dB</b> is the
    /// span that shape lives in. <b>-10 and -13 are anchors above</b>, where every message is expected
    /// back and a rate short of 100 per cent would mean the instrument rather than the receiver.
    /// <b>-26, -28 and -30 are the anchors below and are also criterion 3's population.</b>
    /// <b>-21 is on the ladder because it is the ratio the criterion names.</b>
    /// </remarks>
    internal static readonly double[] Rungs =
    {
        -10.0, -13.0,
        -16.0, -17.0, -18.0, -19.0, -20.0, -21.0, -22.0, -23.0, -24.0,
        -26.0, -28.0, -30.0,
    };

    /// <summary>
    /// The noise draws. <b>Six on the rungs the verdict is read off and three on the anchors</b>, which
    /// is what turns the population into the trial counts below.
    /// </summary>
    /// <remarks>
    /// <b>Six and three are chosen from unit 221's task 1 cost measurement and not from taste.</b> One
    /// slot decode was timed at <b>64.1 ms</b> mid-collapse, so ten minutes buys about 9360 of them.
    /// The population is 51 messages, so six draws is 306 trials on each of the nine rungs from -16 to
    /// -24 and three draws is 153 on each of the five anchors — <b>3519 slot decodes for one pass of
    /// the curve, about 3.8 minutes, and about 7.5 minutes for the two passes criterion 1 needs.</b>
    /// Task 2d's floors are 200 and 100. <b>Nothing was thinned</b>; every rung carries more than its
    /// floor, and 336 trials is what lets a rate near the collapse carry the word <em>comparable</em>
    /// at all — unit 218's 2 of 52 has a 95 per cent Wilson interval running from about 1 per cent to
    /// about 13.
    /// </remarks>
    internal static readonly int[] Seeds =
    {
        221_001, 221_002, 221_003, 221_004, 221_005, 221_006,
    };

    /// <summary>The top of the 1 dB span, inclusive.</summary>
    internal const double CollapseTopDecibels = -16.0;

    /// <summary>The bottom of the 1 dB span, inclusive.</summary>
    internal const double CollapseBottomDecibels = -24.0;

    /// <summary>Whether a rung is inside the span the verdict is read from.</summary>
    internal static bool InTheCollapse(double rung) =>
        rung <= CollapseTopDecibels + 1e-9 && rung >= CollapseBottomDecibels - 1e-9;

    /// <summary>
    /// How many noise draws a rung gets. <b>Six through the collapse, three on the anchors.</b>
    /// </summary>
    internal static int SeedsFor(double rung) => InTheCollapse(rung) ? 6 : 3;

    /// <summary>
    /// <b>The population: the encode corpus less the messages that have no text to be scored
    /// against — 51 of 56, which is nearly twice unit 218's 26.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unit 218's ladder took the corpus, dropped every entry carrying a hashed callsign, and then
    /// kept every second one — 26 messages. <b>This one drops only the five that cannot be scored at
    /// all</b>, so the population is every other message kind this library builds: the standard forms
    /// with grids, reports and lettered CQs; <b>free text</b>, eight of them; telemetry; and the
    /// non-standard callsign type with its companion spelled out.
    /// </para>
    /// <para>
    /// <b>Why the hashed-callsign messages are out, measured rather than inherited, which is what
    /// task 2e asks for.</b> <see cref="Ft8Step6CurveTests"/>'s population probe offered all 56 at
    /// -10 dB — six decibels above where unit 218's diagnostic first fell below 100 per cent — and the
    /// five hashed entries came back as <b>nothing at all, against a truth-side text that is itself
    /// the empty string</b>. That is the ladder's construction forbidding it, not a defect: a 22-bit
    /// hashed callsign resolves only against a cache the receiver warms from <em>earlier</em> decodes,
    /// and a single synthesized slot has no history behind it. There is no text on either side to
    /// compare, so such an entry can never count as returned and its only effect on a curve would be
    /// to cap every rate at 51 of 56. <b>Keeping them in would have made the top of the ladder read
    /// 91 per cent for a reason that has nothing to do with sensitivity.</b>
    /// </para>
    /// <para>
    /// <b>Deterministic by construction.</b> There is no draw and no shuffle — the corpus is built in
    /// a fixed order by <see cref="EncodeCorpus.Build"/> and filtered by a fixed predicate, so trial
    /// <c>i</c> at every rung and in every process is the same message.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<EncodeCorpus.Entry> Population() =>
        EncodeCorpus.Build().Where(CanBeScored).ToArray();

    /// <summary>
    /// The corpus entries this ladder cannot score, kept as a list rather than as a silence so the
    /// exclusion is reported with its reason every time the curve runs.
    /// </summary>
    internal static IReadOnlyList<EncodeCorpus.Entry> Unscoreable() =>
        EncodeCorpus.Build().Where(e => !CanBeScored(e)).ToArray();

    /// <summary>
    /// <b>Whether there is any text to score this entry against.</b> The truth side of every trial is
    /// <c>Ft8MessageDecoder.Decode(entry.Message).Text</c>, and where that is empty there is nothing
    /// for a returned message to equal.
    /// </summary>
    internal static bool CanBeScored(EncodeCorpus.Entry entry) =>
        Ft8MessageDecoder.Decode(entry.Message).Text.Length > 0;

    /// <summary>Trials at one rung: the population once per seed.</summary>
    internal static int TrialsFor(double rung) => Population().Count * SeedsFor(rung);

    /// <summary>One rung of the curve.</summary>
    internal sealed class Row(double requested)
    {
        private readonly List<double> _delivered = new();

        internal double Requested { get; } = requested;

        internal int Trials { get; private set; }

        internal int Returned { get; private set; }

        internal int Wrong { get; private set; }

        internal long Candidates { get; private set; }

        internal long Parity { get; private set; }

        internal long Checksum { get; private set; }

        internal long Text { get; private set; }

        /// <summary>Every wrong text this rung returned, so a non-zero count has evidence under it.</summary>
        internal List<string> WrongTexts { get; } = new();

        /// <summary>The mean ratio actually put on the samples. <b>The row is binned by this.</b></summary>
        internal double DeliveredMean => _delivered.Count == 0 ? double.NaN : _delivered.Average();

        /// <summary>The largest requested-versus-delivered error at this rung, in decibels.</summary>
        internal double WorstDeliveryError =>
            _delivered.Count == 0 ? double.NaN : _delivered.Max(d => Math.Abs(d - Requested));

        internal double Rate => Trials == 0 ? 0.0 : 100.0 * Returned / Trials;

        internal (double Lower, double Upper) Interval => Wilson(Returned, Trials);

        internal void Add(Ft8SlotResult result, double delivered, bool returned, IReadOnlyList<string> wrong)
        {
            Trials++;
            _delivered.Add(delivered);
            Candidates += result.CandidateCount;
            Parity += result.ParitySatisfiedCount;
            Checksum += result.ChecksumPassedCount;
            Text += result.BecameTextCount;

            if (returned)
            {
                Returned++;
            }

            Wrong += wrong.Count;
            WrongTexts.AddRange(wrong);
        }

        internal string AsRow()
        {
            var (lower, upper) = Interval;
            return $"{Requested,9:F1} {DeliveredMean,10:F3} {Trials,7} {Returned,9} {Rate,7:F1} "
                + $"{lower,7:F1} {upper,7:F1} {Wrong,6} "
                + $"{Candidates / (double)Math.Max(Trials, 1),7:F2} "
                + $"{Parity / (double)Math.Max(Trials, 1),7:F2} "
                + $"{Checksum / (double)Math.Max(Trials, 1),7:F2} "
                + $"{Text / (double)Math.Max(Trials, 1),7:F2}";
        }
    }

    /// <summary>The header the rows line up under.</summary>
    internal const string Header =
        "requested  delivered  trials  returned    rate   lo 95   hi 95  WRONG    cand     par     crc     txt";

    /// <summary>
    /// <b>The Wilson score interval at 95 per cent</b>, which is the interval a rate near zero or near
    /// one needs — the textbook normal approximation puts 2 of 52 at a lower bound below zero and is
    /// worthless exactly where this curve is most interesting.
    /// </summary>
    internal static (double Lower, double Upper) Wilson(int successes, int trials)
    {
        if (trials == 0)
        {
            return (0.0, 0.0);
        }

        const double z = 1.959963984540054;
        var n = (double)trials;
        var p = successes / n;
        var denominator = 1.0 + (z * z / n);
        var centre = p + (z * z / (2.0 * n));
        var spread = z * Math.Sqrt((p * (1.0 - p) / n) + (z * z / (4.0 * n * n)));

        return (100.0 * (centre - spread) / denominator, 100.0 * (centre + spread) / denominator);
    }

    /// <summary>
    /// <b>Walks the whole path — samples to text — down every rung and reports what came back.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nothing is told to the decode path.</b> The frequency and the offset are handed to the
    /// synthesizer; <see cref="Ft8SlotDecoder"/> is given the samples and nothing else. The truth is
    /// used once and after the code has answered, to compare the text.
    /// </para>
    /// <para>
    /// <b>A rung that returns nothing is a measurement and not a failure.</b> Nothing here throws on a
    /// poor result.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<Row> Walk(
        IReadOnlyList<EncodeCorpus.Entry> messages,
        double frequencyHz,
        int sampleOffset,
        Action<string>? log = null)
    {
        const int rate = Ft8WaterfallGeometry.DefaultSampleRate;

        var decoder = new Ft8SlotDecoder();
        var geometry = decoder.Geometry;
        var rows = new List<Row>();

        foreach (var requested in Rungs)
        {
            var row = new Row(requested);
            var seeds = SeedsFor(requested);

            for (var s = 0; s < seeds; s++)
            {
                // The seed depends only on the rung and the draw, never on iteration order, so a
                // fresh process walking the same ladder draws the same noise.
                var noise = new GaussianNoise(Seeds[s] + (int)Math.Round(requested * 10.0));

                foreach (var entry in messages)
                {
                    var (clean, _) = SearchFixture.OneSignal(rate, entry, frequencyHz, sampleOffset);
                    var signalPower = SearchFixture.TransmissionPower(rate, entry, frequencyHz);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, rate);
                    var mixed = SearchFixture.AddNoise(clean, noise, sigma, out var noisePower);
                    var delivered = SignalToNoise.DecibelsFor(signalPower, noisePower, rate);

                    var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                    var result = decoder.Decode(waterfall);

                    var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                    var returned = result.Texts.Contains(expected, StringComparer.Ordinal);
                    var wrong = result.Texts
                        .Where(t => !string.Equals(t, expected, StringComparison.Ordinal))
                        .ToArray();

                    row.Add(result, delivered, returned, wrong);
                }
            }

            log?.Invoke(row.AsRow());
            rows.Add(row);
        }

        return rows;
    }
}
