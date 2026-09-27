using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-050 measured: on each of TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT, at 15 dB
/// in the 2500 Hz reference on CH-AWGN, whether the decoder meets HM-REQ-013 - every
/// sent character emitted sure and correct (work instruction 468, task 2;
/// PHASE_PLAN.md 7.2). TX-BUG's cases are printed beside them under HM-REQ-052.
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE NUMBERS.** HM-REQ-013 is pass/fail
/// and not a ratchet (V-08); a case not met is a finding for the decoder steps, and
/// 7.2 asks for the measurement. It asserts only that every case was generated and
/// read.</para>
/// <para>**OURS ALONE IS THE NUMBER**, read as <see cref="TheRequirementsAreMeasuredTests"/>
/// reads the synthetic set - <see cref="CwDecoder"/> from 600 Hz hop by hop, the
/// whole decode against the whole key, ends trimmed - and scored through the same
/// <see cref="TheRequirementsAreMeasuredTests.Measure"/> and <see cref="CwMetrics"/>
/// calls. Beside it, for reading only: the transcript the live path emits with the
/// second reader under <see cref="CwSwitchTable.Live"/> as it stands, and the port
/// alone. No case is a switch row and nothing is switched (work instruction 468 DECIDED (6)).</para>
/// <para>**EVERY KEY IS EXACT BY CONSTRUCTION** (R61, V-13): the case is
/// <see cref="SyntheticCq.Text"/> keyed by <see cref="CwSender"/> at the seed task 1
/// fixed before any decode. The key is never a decoder input (R72).</para>
/// </remarks>
public sealed class TheMustFistsAtFifteenDecibelsFact
{
    private const double Snr = 15;

    private const string Channel = "CH-AWGN";

    private static readonly string[] Must = { "TX-ITU", "TX-KEYER-W", "TX-FARNS", "TX-TIGHT" };

    private static readonly string[] Should = { "TX-BUG" };

    private static readonly string[] Outputs = { "ours", "emitted (live, switch as it stands)", "port alone" };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the table is printed.</param>
    public TheMustFistsAtFifteenDecibelsFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>One output of one case, scored.</summary>
    private sealed record Scored(
        CwCoverage Coverage, CwSureErrors Sure, CwBoundaryErrors Wbe, CwInvented Invented, string Text)
    {
        public bool Met => Coverage.SureRight == Coverage.Sent && Coverage.Sent > 0 && Sure.Errors == 0 && Wbe.Errors == 0;
    }

    /// <remarks>
    /// Proves 7.2's fourth condition: HM-REQ-050 measured per case and per must-tier
    /// profile, HM-REQ-013's three numbers (MET-COVERAGE 1.00, MET-CER-SURE 0, MET-WBE
    /// 0) with met or not met, a profile met only if all three of its cases are; with
    /// MET-INVENTED and the decoded text beside the key, and the emitted and port-alone
    /// outputs beside ours. Asserts only that all fifteen cases were read.
    /// </remarks>
    [Fact]
    public void EachFistAtFifteenDecibelsOnChAwgn()
    {
        var i = CultureInfo.InvariantCulture;
        var read = 0;

        _output.WriteLine("HM-REQ-050: on each of TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT, the decoder shall meet HM-REQ-013.");
        _output.WriteLine("HM-REQ-013: every must-tier sender at 15 dB reference on CH-AWGN, every sent character sure and correct - MET-COVERAGE 1.00, MET-CER-SURE 0, MET-WBE 0.");
        _output.WriteLine($"key | {SyntheticCq.Text} | synthetic, exact");

        foreach (var (requirement, profiles) in new[] { ("HM-REQ-050", Must), ("HM-REQ-052", Should) })
        {
            foreach (var id in profiles)
            {
                var met = new int[Outputs.Length];

                for (var s = 0; s < CwSender.Speeds.Count; s++)
                {
                    var wpm = CwSender.Speeds[s];
                    var seed = CwSender.Seed(id, s);
                    var generated = CwSender.Generate(id, Channel, Snr, SyntheticCq.Text, wpm, seed);
                    var name = string.Create(i, $"{id.ToLowerInvariant()}-{wpm:0}wpm-15db-ref-awgn");
                    var scored = Read(name, generated.Audio);

                    Assert.Equal(SyntheticCq.Text, generated.Key);
                    read++;

                    for (var o = 0; o < Outputs.Length; o++)
                    {
                        var x = scored[o];
                        met[o] += x.Met ? 1 : 0;

                        _output.WriteLine(string.Create(i,
                            $"case | {requirement} | {id} | {wpm:0} wpm | seed {seed} | {Outputs[o]} | "
                            + $"MET-COVERAGE {x.Coverage.SureRight}/{x.Coverage.Sent} {Share(x.Coverage.SureRight, x.Coverage.Sent)} | "
                            + $"MET-CER-SURE {x.Sure.Errors}/{x.Sure.SureEmitted} {Share(x.Sure.Errors, x.Sure.SureEmitted)} ({x.Sure.SureWrong} substituted, {x.Sure.SureAdded} added) | "
                            + $"MET-WBE {x.Wbe.Errors}/{x.Wbe.WordsSent} {Share(x.Wbe.Errors, x.Wbe.WordsSent)} ({x.Wbe.Inserted} inserted, {x.Wbe.Deleted} deleted) | "
                            + $"MET-INVENTED {x.Invented.Count}/{x.Invented.Sent} | "
                            + $"{(x.Met ? "met" : "NOT MET")} | read `{x.Text}` | synthetic, exact"));
                    }
                }

                for (var o = 0; o < Outputs.Length; o++)
                {
                    var all = CwSender.Speeds.Count;
                    _output.WriteLine($"profile | {requirement} | {id} | {Outputs[o]} | {(met[o] == all ? "met" : "NOT MET")} | {met[o]} of {all} cases met; met only if all are | synthetic, exact");
                }
            }
        }

        Assert.Equal((Must.Length + Should.Length) * CwSender.Speeds.Count, read);
    }

    private static string Share(int count, int of)
        => of == 0 ? "(nothing to divide by)" : (count / (double)of).ToString("0.00", CultureInfo.InvariantCulture);

    // Ours as the synthetic set is read and scored, then the live path with the
    // second reader: its emitted transcript and the port alone.
    private static Scored[] Read(string name, MonoAudio audio)
    {
        var ours = Settle(audio);
        var (emitted, _, readings) = TheArbitrationEarnsItsPlaceFact.Drive(
            audio.Samples, audio.SampleRate, SyntheticCq.StartingPitchHz, true, CwSwitchTable.Live);
        var port = TheArbitrationEarnsItsPlaceFact.PortAlone(readings);

        return new[] { Score(name, ours), Score(name, emitted), Score(name, port) };
    }

    private static Scored Score(string name, IReadOnlyList<CwCharacter> settled)
    {
        var m = TheRequirementsAreMeasuredTests.Measure(name, "synthetic", "TX-* at 15 dB reference on CH-AWGN", CwKeyKind.Exact,
            settled, TheArbitrationEarnsItsPlaceFact.ScoresFor(name, false, settled));
        var stretches = m.NotComputable is null ? m.Stretches : Array.Empty<CwMetricAlignment>();

        var c = stretches.Select(CwMetrics.Coverage).ToList();
        var e = stretches.Select(CwMetrics.SureErrors).ToList();
        var w = stretches.Select(CwMetrics.WordBoundaries).ToList();
        var n = stretches.Select(CwMetrics.Invented).ToList();

        // Nothing scored is nothing read: every sent character missed, every word boundary with it.
        var sent = SyntheticCq.Text.Count(ch => ch != ' ');
        var words = SyntheticCq.Text.Split(' ').Length;

        return new Scored(
            c.Count == 0 ? new CwCoverage(0, 0, sent, CwKeyKind.Exact) : new CwCoverage(c.Sum(x => x.SureEmitted), c.Sum(x => x.SureRight), c.Sum(x => x.Sent), CwKeyKind.Exact),
            new CwSureErrors(e.Sum(x => x.SureWrong), e.Sum(x => x.SureAdded), e.Sum(x => x.SureEmitted), CwKeyKind.Exact),
            w.Count == 0 ? new CwBoundaryErrors(0, words - 1, words, CwKeyKind.Exact) : new CwBoundaryErrors(w.Sum(x => x.Inserted), w.Sum(x => x.Deleted), w.Sum(x => x.WordsSent), CwKeyKind.Exact),
            new CwInvented(n.Sum(x => x.SureAdded), n.Sum(x => x.SureWrong), c.Count == 0 ? sent : n.Sum(x => x.Sent), CwKeyKind.Exact),
            CwReading.Of(settled).Text.Trim());
    }

    // TheRequirementsAreMeasuredTests' own read: CwDecoder from 600 Hz, hop by hop, flushed.
    private static IReadOnlyList<CwCharacter> Settle(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
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
}
