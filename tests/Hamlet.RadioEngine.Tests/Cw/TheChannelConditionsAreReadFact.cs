using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// One reading of our decoder on the CH-* conditions, at the SNR each must
/// requirement names (HM-REQ-013, 040 and 041; work instruction 461 task 3).
/// </summary>
/// <remarks>
/// <para>**ONE POINT, NOT A SWEEP, SO NO FLOOR IS CLAIMED (V-05).** Each profile is
/// read at its requirement's SNR and at +15 dB reference, three seeds each, on
/// audio <see cref="TheChannelProfilesAreWhatTheySayTests"/> proved (V-12). The
/// decode is <see cref="SyntheticCq.Read"/>'s path, a <see cref="CwDecoder"/> fed
/// hop by hop from 600 Hz; the scoring is <see cref="TheRequirementsAreMeasuredTests"/>'
/// own: the whole decode against the whole exact key, its two ends' gaps
/// trimmed, through <see cref="CwMetrics"/>.</para>
/// <para>**THE JUDGEMENT, FIXED BEFORE THE FIRST RUN.** A requirement is met at its
/// point only if every one of the three seeds meets every clause; the three
/// pooled are printed beside. HM-REQ-013: MET-COVERAGE 1, MET-CER-SURE 0 and
/// MET-WBE 0 on CH-AWGN at +15 dB. HM-REQ-040 (CH-LM, CH-MM at -4 dB) and 041
/// (CH-HM at -1 dB): HM-REQ-010's MET-CER-SURE below 0.01, 011's MET-INVENTED 0
/// and 012's MET-COVERAGE at least 0.90.</para>
/// <para>A printer: it asserts only that every case was decoded and scored. It is
/// on neither carry-forward line.</para>
/// </remarks>
public sealed class TheChannelConditionsAreReadFact
{
    private const double Wpm = 20;

    private const double Pitch = SyntheticCq.StartingPitchHz;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheChannelConditionsAreReadFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>The points: each profile at its requirement's SNR and at +15 dB reference.</summary>
    public static IReadOnlyList<(string Profile, double SnrDb, string Requirement)> Points { get; } = new[]
    {
        ("CH-AWGN", 15.0, "HM-REQ-013"),
        ("CH-AWGN", -4.0, "-"),
        ("CH-AWGN", -7.0, "HM-REQ-020's SNR, not judged"),
        ("CH-AWGN", -10.0, "-"),
        ("CH-LM", 15.0, "-"),
        ("CH-LM", -4.0, "HM-REQ-040"),
        ("CH-MM", 15.0, "-"),
        ("CH-MM", -4.0, "HM-REQ-040"),
        ("CH-HM", 15.0, "-"),
        ("CH-HM", -1.0, "HM-REQ-041"),
    };

    /// <summary>Decode every point, print the table and each requirement's reading.</summary>
    [Fact]
    public void EveryPointIsReadAndScored()
    {
        var i = CultureInfo.InvariantCulture;
        var verdicts = new List<string>();

        _output.WriteLine("point | profile | snr (2500 Hz ref) | seed | key | sure emitted | sure wrong | sure added | MET-CER-SURE | MET-INVENTED | sent | sure right | MET-COVERAGE | words | inserted | deleted | MET-WBE | text");

        foreach (var (id, snr, requirement) in Points)
        {
            var row = CwChannel.Profiles.ToList().FindIndex(p => p.Id == id);
            var measured = new List<TheRequirementsAreMeasuredTests.Measured>();

            for (var s = 0; s < 3; s++)
            {
                var seed = 461400 + (10 * row) + s;
                var generated = CwChannel.Generate(id, snr, SyntheticCq.Text, Wpm, Pitch, seed);
                var settled = Settle(generated.Audio);
                var reading = CwReading.Of(settled);
                var score = CwScorer.Whole(SyntheticCq.Trimmed(reading), generated.Key, CwKeyKind.Exact);
                var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();
                var m = TheRequirementsAreMeasuredTests.Measure(
                    $"{id} {snr:+0;-0} dB seed {seed}", "channel", $"{id}, TX-ITU 20 wpm, {snr:+0;-0} dB reference",
                    CwKeyKind.Exact, settled, new[] { score with { Start = from } });

                measured.Add(m);

                Assert.Null(m.NotComputable);

                var (e, inv, c, w) = Figures(new[] { m });

                _output.WriteLine(string.Create(i,
                    $"point | {id} | {snr:+0;-0} dB | {seed} | {CwMetrics.KindWord(CwKeyKind.Exact)} | {e.SureEmitted} | {e.SureWrong} | {e.SureAdded} | {Share(e.Errors, e.SureEmitted)} | {Share(inv.Count, inv.Sent)} | {c.Sent} | {c.SureRight} | {Share(c.SureRight, c.Sent)} | {w.WordsSent} | {w.Inserted} | {w.Deleted} | {Share(w.Errors, w.WordsSent)} | \"{reading.Text.Trim()}\""));
            }

            var (pe, pinv, pc, pw) = Figures(measured);

            _output.WriteLine(string.Create(i,
                $"pooled | {id} | {snr:+0;-0} dB | 3 seeds | exact | MET-CER-SURE {pe.Errors} of {pe.SureEmitted}, {Share(pe.Errors, pe.SureEmitted)} | MET-INVENTED {pinv.Count} over {pinv.Sent}, {Share(pinv.Count, pinv.Sent)} | MET-COVERAGE {pc.SureRight} over {pc.Sent}, {Share(pc.SureRight, pc.Sent)} | MET-WBE {pw.Errors} ({pw.Inserted} inserted, {pw.Deleted} deleted) over {pw.WordsSent}, {Share(pw.Errors, pw.WordsSent)} | {requirement}"));

            if (requirement == "HM-REQ-013")
            {
                var met = measured.All(m =>
                {
                    var (e, _, c, w) = Figures(new[] { m });
                    return c.SureRight == c.Sent && e.Errors == 0 && w.Errors == 0;
                });

                verdicts.Add(string.Create(i, $"verdict | HM-REQ-013 | CH-AWGN +15 dB reference, TX-ITU 20 wpm | {(met ? "met" : "not met")} at this point (coverage 1, MET-CER-SURE 0, MET-WBE 0 on each of 3 seeds)"));
            }
            else if (requirement is "HM-REQ-040" or "HM-REQ-041")
            {
                var met = measured.All(m =>
                {
                    var (e, inv, c, _) = Figures(new[] { m });
                    return e.SureEmitted > 0 && e.Errors < 0.01 * e.SureEmitted && inv.Count == 0 && c.SureRight >= 0.90 * c.Sent;
                });

                verdicts.Add(string.Create(i, $"verdict | {requirement} | {id} {snr:+0;-0} dB reference, TX-ITU 20 wpm | {(met ? "met" : "not met")} at this point (MET-CER-SURE < 0.01, MET-INVENTED 0, MET-COVERAGE >= 0.90 on each of 3 seeds) - one point, no floor claimed (V-05)"));
            }
        }

        foreach (var v in verdicts)
        {
            _output.WriteLine(v);
        }
    }

    /// <summary>
    /// V-04's trace of the CH-AWGN +15 dB misreading against the existing clean
    /// synthetics: the same seed and text, one difference at a time between the
    /// channel's case and <see cref="CwFixtureGenerator.Generate"/>'s.
    /// </summary>
    [Fact]
    public void TheAwgnOpeningIsTracedAgainstTheCleanSynthetics()
    {
        const int Seed = 461490;
        var i = CultureInfo.InvariantCulture;
        var offset = 10 * Math.Log10(CwChannel.ReferenceHz / CwChannel.EquivalentBandwidth(600));
        var itu = SyntheticCq.Recipe(Wpm, 15 + offset, Seed);

        var variants = new (string Label, MonoAudio Audio)[]
        {
            ("channel CH-AWGN +15 dB ref, 600 Hz, band x0.5", CwChannel.Generate("CH-AWGN", 15, SyntheticCq.Text, Wpm, 600, Seed).Audio),
            ("channel CH-AWGN +15 dB ref, 615 Hz, band x0.5", CwChannel.Generate("CH-AWGN", 15, SyntheticCq.Text, Wpm, 615, Seed).Audio),
            ("channel CH-AWGN +6.3 dB ref (the set's 15 dB in-band at 615), 615 Hz", CwChannel.Generate("CH-AWGN", 15 - (10 * Math.Log10(CwChannel.ReferenceHz / CwChannel.EquivalentBandwidth(615))), SyntheticCq.Text, Wpm, 615, Seed).Audio),
            ("channel CH-AWGN +10 dB ref, 600 Hz", CwChannel.Generate("CH-AWGN", 10, SyntheticCq.Text, Wpm, 600, Seed).Audio),
            ("channel CH-AWGN +5 dB ref, 600 Hz", CwChannel.Generate("CH-AWGN", 5, SyntheticCq.Text, Wpm, 600, Seed).Audio),
            ("generator TX-ITU same in-band SNR, 600 Hz, no drift, band x1", CwFixtureGenerator.Generate(itu with { ToneHz = 600, DriftHz = 0 }).Audio),
            ("generator TX-ITU same in-band SNR, 615 Hz +/-3 drift", CwFixtureGenerator.Generate(itu).Audio),
            ("generator TX-ITU 15 dB in-band (the set's tier), 615 Hz +/-3", CwFixtureGenerator.Generate(SyntheticCq.Recipe(Wpm, 15, Seed)).Audio),
        };

        foreach (var (label, audio) in variants)
        {
            var text = CwReading.Of(Settle(audio)).Text.Trim();

            // V-04's reference: the fldigi port, read-only, as BothDecodersAreScoredAlikeTests drives it.
            var port = new FldigiCwDecoder(600);
            port.rx_process(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate));

            _output.WriteLine(string.Create(i, $"trace | {label} | peak {AudioTap.PeakOf(audio):0.0} dBFS | ours \"{text}\" | port \"{port.Text.Trim()}\""));
        }
    }

    private static (CwSureErrors Errors, (int Count, int Sent) Invented, CwCoverage Coverage, CwBoundaryErrors Words) Figures(
        IEnumerable<TheRequirementsAreMeasuredTests.Measured> measured)
    {
        var stretches = measured.SelectMany(m => m.Stretches).ToList();
        var e = stretches.Select(CwMetrics.SureErrors).ToList();
        var inv = stretches.Select(CwMetrics.Invented).ToList();
        var c = stretches.Select(CwMetrics.Coverage).ToList();
        var w = stretches.Select(CwMetrics.WordBoundaries).ToList();

        return (
            new CwSureErrors(e.Sum(p => p.SureWrong), e.Sum(p => p.SureAdded), e.Sum(p => p.SureEmitted), CwKeyKind.Exact),
            (inv.Sum(p => p.Count), inv.Sum(p => p.Sent)),
            new CwCoverage(c.Sum(p => p.SureEmitted), c.Sum(p => p.SureRight), c.Sum(p => p.Sent), CwKeyKind.Exact),
            new CwBoundaryErrors(w.Sum(p => p.Inserted), w.Sum(p => p.Deleted), w.Sum(p => p.WordsSent), CwKeyKind.Exact));
    }

    // SyntheticCq.Read's path, keeping what settled.
    private static List<CwCharacter> Settle(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, Pitch);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return settled;
    }

    private static string Share(int count, int of) => of == 0
        ? "no number"
        : (count / (double)of).ToString("0.0000", CultureInfo.InvariantCulture);
}
