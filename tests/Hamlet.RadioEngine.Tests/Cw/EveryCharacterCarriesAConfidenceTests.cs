using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Tests.Cw.Instruments;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-124's first half: every character either decoder emits carries a
/// confidence p between 0 and 1, and the calibration measure gives the answer a
/// hand count gives (work instruction 464, task 2; PHASE_PLAN.md 9.5).
/// </summary>
/// <remarks>
/// Not on either carry-forward line. Whether the confidences are calibrated on the
/// keyed corpus is measured, not asserted, in `docs/phase-requirements/calibration.md`.
/// </remarks>
public sealed class EveryCharacterCarriesAConfidenceTests
{
    private const string Case = "cq-18wpm-15db";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each character's p is printed.</param>
    public EveryCharacterCarriesAConfidenceTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// HM-REQ-124: on a synthetic case of exact key, every character of ours and
    /// every character of the port has a p in [0, 1] and none is NaN. Ours is fed
    /// hop by hop as the metrics feed it; the port is given the file at 8000 Hz and
    /// the pitch instrument's median, as the parity harness gives it.
    /// </remarks>
    [Fact]
    public void OnASyntheticCaseEveryCharacterOfBothHasAProbability()
    {
        var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, Case + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var ours = new List<CwCharacter>();

        decoder.CharacterSettled += ours.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var windows = CwPitchInstrument.Measure(audio.Samples, audio.SampleRate);
        var given = windows.Select(w => w.Hz).OrderBy(h => h).ElementAt(windows.Count / 2);
        var port = new FldigiCwDecoder(given) { TraceDecisions = true };

        port.rx_process(FldigiRateAdapter.ToFldigiRate(audio.Samples, audio.SampleRate));

        var portP = FldigiConfidence.For(port);
        var bad = new List<string>();

        foreach (var c in ours.Where(c => !c.IsWordGap))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"ours | {c.Text} | margin {c.MarginLlr:0.###} | p {c.Probability:0.####}"));

            if (!(c.Probability >= 0 && c.Probability <= 1))
            {
                bad.Add(string.Create(CultureInfo.InvariantCulture, $"ours `{c.Text}` at {c.At.TotalSeconds:0.000} s: p {c.Probability}"));
            }
        }

        for (var i = 0; i < port.Emissions.Count; i++)
        {
            var e = port.Emissions[i];

            if (e.Text == MorseAlphabet.WordGap)
            {
                continue;
            }

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"port | {e.Text} | {e.Representation} | p {portP[i]:0.####}"));

            if (!(portP[i] >= 0 && portP[i] <= 1))
            {
                bad.Add(string.Create(CultureInfo.InvariantCulture, $"port `{e.Text}` at sample {e.InputSample}: p {portP[i]}"));
            }
        }

        Assert.Contains(ours, c => !c.IsWordGap);
        Assert.Contains(port.Emissions, e => e.Text != MorseAlphabet.WordGap);
        Assert.True(bad.Count == 0, $"{bad.Count} characters without a probability:{Environment.NewLine}{string.Join(Environment.NewLine, bad)}");
    }

    /// <remarks>
    /// HM-REQ-124, the drop candidate: the calibration measure on hand-built lists
    /// whose answer is known by construction. 100 characters at p 0.9 with 90 right
    /// are calibrated; with 80 right they are not; 29 are not measurable.
    /// </remarks>
    [Fact]
    public void TheMeasureGivesTheAnswerAHandCountGives()
    {
        static List<CwCalibrationPoint> At(double p, int right, int of)
            => Enumerable.Range(0, of).Select(i => new CwCalibrationPoint(p, i < right)).ToList();

        Assert.Equal(CwCalibrationVerdict.Calibrated, CwCalibration.Judge(At(0.9, 90, 100)));
        Assert.Equal(CwCalibrationVerdict.NotCalibrated, CwCalibration.Judge(At(0.9, 80, 100)));
        Assert.Equal(CwCalibrationVerdict.NotMeasurable, CwCalibration.Judge(At(0.9, 29, 29)));
        Assert.Equal(9, CwCalibration.BinOf(0.9));
        Assert.Equal(9, CwCalibration.BinOf(1.0));
        Assert.Equal(-10.0, CwCalibration.Table(At(0.9, 80, 100))[9].DifferencePoints, 9);
    }
}
