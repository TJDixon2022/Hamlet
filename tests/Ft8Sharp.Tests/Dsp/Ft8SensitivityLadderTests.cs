using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>How deaf is this receiver, in decibels?</b> The whole path — samples to text — walked down a
/// ladder of known signal-to-noise ratios on signals this library synthesized itself, until it stops
/// answering.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why noise and not another look at the port.</b> Every measurement this phase has taken on the
/// receive side compares this library to <c>ft8_lib</c> or to itself. The pinned decoder cannot be
/// run on this machine — <c>HM-OPEN-065</c> — and comparing the port to itself is what units 216 and
/// 217 did thoroughly, and criterion 3's residue survived it. <b>Calibrated noise is a third oracle
/// and it answers to nobody.</b> A signal at a known ratio in a known bandwidth is a physical fact.
/// </para>
/// <para>
/// <b>THIS IS NOT STEP 6 AND IT DOES NOT CLAIM STEP 6'S CRITERIA.</b> Step 6 wants a curve generated
/// across a range of SNR and reproducible; a decode rate at -21 dB compared against the published
/// figure <em>as a verdict</em>; and behaviour below the threshold shown to degrade rather than
/// produce wrong decodes. <b>This is one session's diagnostic ladder</b>, taken so criterion 3's
/// residue can be read as either this receiver's deafness or the benchmark's reach. Step 6 will still
/// need all three of those things and this file supplies none of them.
/// </para>
/// <para>
/// <b>Aligned and on-grid here on purpose.</b> The signal starts on a block boundary and its lowest
/// tone sits exactly on a bin centre. Every impairment is
/// <see cref="Ft8ImpairedLadderTests"/>'s; mixing them in here would make the collapse point
/// unreadable, which is the whole thing this test exists to locate.
/// </para>
/// <para>
/// <b>A rung that returns nothing is a measurement, not a failure.</b> The only assertions are the
/// two that must always be true whatever the ladder says: <b>zero wrong messages at every rung</b>,
/// and the top rung — the one unit 216 already stood on — still returning everything.
/// </para>
/// </remarks>
public class Ft8SensitivityLadderTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>
    /// Exactly on a bin centre: 1000 / 6.25 is 160, a whole number of tone spacings.
    /// </summary>
    private const double OnGridHz = 1000.0;

    private readonly ITestOutputHelper _output;

    public Ft8SensitivityLadderTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>THE CENTRE OF THE NIGHT.</b> The aligned, on-grid ladder from -10 dB to -26 dB.
    /// </summary>
    [Fact]
    public void TheWholePathIsWalkedDownTheLadderUntilItStopsAnswering()
    {
        var messages = SensitivityLadder.Messages();
        var samplesPerSymbol = Ft8Waveform.SamplesPerSymbol(Rate);

        // ALIGNED: a whole number of symbol periods in, which is also a whole number of the
        // waterfall's half-symbol blocks, so the transmission begins exactly where a block does.
        var alignedOffset = samplesPerSymbol * 3;

        _output.WriteLine("THE ALIGNED LADDER. Samples in, text out, on signals this library made.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  rungs                   : {SensitivityLadder.Rungs.Length}, "
            + $"{SensitivityLadder.Rungs[0]:F0} dB down to "
            + $"{SensitivityLadder.Rungs[^1]:F0} dB, steps no larger than 2 dB, -21 among them");
        _output.WriteLine($"  messages per rung       : {messages.Count}, the corpus filtered to "
            + "!CarriesHashedCallsign and thinned to every second entry");
        _output.WriteLine($"  seeds per rung          : {SensitivityLadder.Seeds.Length}");
        _output.WriteLine($"  SLOT DECODES IN TOTAL   : {SensitivityLadder.Rungs.Length} x "
            + $"{messages.Count} x {SensitivityLadder.Seeds.Length} = "
            + $"{SensitivityLadder.Rungs.Length * messages.Count * SensitivityLadder.Seeds.Length}");
        _output.WriteLine($"  base frequency          : {OnGridHz:F2} Hz, "
            + $"{OnGridHz / SensitivityLadder.BinHz:F0} tone spacings, EXACTLY ON A BIN CENTRE");
        _output.WriteLine($"  sample offset           : {alignedOffset}, "
            + $"{alignedOffset / samplesPerSymbol} symbol periods, ON THE BLOCK GRID");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NO BOUND, NO THRESHOLD AND NO EXPECTED COLLAPSE IS WRITTEN INTO THIS");
        _output.WriteLine("  TEST. The table is printed and only then is anything asserted.");
        _output.WriteLine(string.Empty);

        var rungs = SensitivityLadder.Walk(
            messages,
            _ => OnGridHz,
            _ => alignedOffset,
            measureAgreement: true);

        _output.WriteLine(SensitivityLadder.Header);
        foreach (var rung in rungs)
        {
            _output.WriteLine(rung.Row());
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  The stage columns are MEANS PER SLOT: candidates found, of those how");
        _output.WriteLine("  many reached a valid codeword, of those how many carried their own");
        _output.WriteLine("  checksum, of those how many this library could put into words.");
        _output.WriteLine("  BINNED BY THE DELIVERED RATIO. The requested column is what was asked");
        _output.WriteLine("  for and the delivered column is what the samples actually carried.");
        _output.WriteLine(string.Empty);

        // The agreement column: what unit 217's on-air histogram has never had, a decibel value.
        _output.WriteLine("AGREEMENT WITH THE TRUE CODEWORD, out of 174 hard decisions, read at the");
        _output.WriteLine("candidate nearest where the fixture actually put the signal. THE TRUTH IS");
        _output.WriteLine("EXACT HERE because this fixture generated it.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"delivered",10} {"rate %",8} {"trials",8} {"mean agree",11} "
            + $"{"misses",8} {"mean agree, misses",20}");

        foreach (var rung in rungs)
        {
            var missMean = rung.MissAgreements.Count == 0
                ? "-"
                : rung.MissAgreements.Average().ToString("F1");

            _output.WriteLine($"{rung.DeliveredMean,10:F3} {rung.Rate,8:F1} {rung.Agreements.Count,8} "
                + $"{rung.MeanAgreement,11:F1} {rung.MissAgreements.Count,8} {missMean,20}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  UNIT 217'S ON-AIR FIGURES, for reading the column above against:");
        _output.WriteLine("    matched, mean agreement  : 167.7 of 174");
        _output.WriteLine("    missed,  mean agreement  : 122.8 of 174");
        _output.WriteLine("    chance, measured         :  84.8 of 174");
        _output.WriteLine(string.Empty);

        var top = rungs[0];
        var lastGood = rungs.LastOrDefault(r => r.Rate >= 50.0);
        var firstBad = rungs.FirstOrDefault(r => r.Rate < 50.0);
        var totalWrong = rungs.Sum(r => r.Wrong);
        var trials = rungs.Sum(r => r.Offered);

        _output.WriteLine("THE HEADLINE, IN ONE SENTENCE WITH A NUMBER IN IT:");
        if (lastGood is null)
        {
            _output.WriteLine("  this path returned MOST messages at NO rung of this ladder, which is a");
            _output.WriteLine($"  collapse at or above {top.DeliveredMean:F1} dB.");
        }
        else if (firstBad is null)
        {
            _output.WriteLine($"  this path returns most messages down to {lastGood.DeliveredMean:F1} dB");
            _output.WriteLine("  AND IS STILL ANSWERING AT THE BOTTOM RUNG, so the ladder does NOT");
            _output.WriteLine("  bracket the collapse and rungs must be added below it.");
        }
        else
        {
            _output.WriteLine($"  this path returns messages down to {lastGood.DeliveredMean:F1} dB "
                + $"({lastGood.Rate:F1} per cent)");
            _output.WriteLine($"  and stops between {lastGood.DeliveredMean:F1} dB and "
                + $"{firstBad.DeliveredMean:F1} dB ({firstBad.Rate:F1} per cent).");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  WRONG MESSAGES RETURNED, WHOLE LADDER: {totalWrong} out of {trials} trials");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOT A STEP 6 RESULT. Step 6 still requires a curve generated across a");
        _output.WriteLine("  range of SNR and shown to be reproducible; a decode rate at -21 dB");
        _output.WriteLine("  compared against the published figure AS A VERDICT; and degradation");
        _output.WriteLine("  below the threshold recorded as a criterion. This is one session's");
        _output.WriteLine("  diagnostic and it claims none of the three.");

        // ONLY WHAT MUST ALWAYS BE TRUE. Nothing here asserts where the collapse is.
        Assert.Equal(0, totalWrong);
        Assert.Equal(messages.Count * SensitivityLadder.Seeds.Length, top.Returned);
    }
}
