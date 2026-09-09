using Ft8Sharp;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Unit 289 task 6 — <b>step 1's exit.</b> A hundred and more messages, each one becoming FT4
/// symbols, becoming audio, and being read back as the same message.
/// </summary>
/// <remarks>
/// <para>
/// <b>What this is and is not evidence of, said plainly because the report must say it.</b> This is
/// Hamlet's own encoder into Hamlet's own decoder, and on its own that is self-consistent by
/// construction: a wrong Gray map, a wrong Costas row, a wrong exclusive-OR byte or a wrong ramp
/// position would be applied at one end and undone at the other and every message would come back
/// perfect. What makes it evidence is that it runs <em>after</em>
/// <c>Ft4SymbolBitIdentityTests.EverySymbolOfEveryMessageIsIdenticalToUpstreamsFt4</c> and
/// <c>Ft4WaveformComparisonTests.EverySampleOfTheFt4SlotIsUpstreamsOwn</c>, which hold the symbols
/// and then the samples against <c>gen_ft8 -ft4</c>. The tones this decoder is reading have been
/// shown to be upstream's tones.
/// </para>
/// <para>
/// <b>A wrong decode is counted separately from a missed one, and both are reported even when both
/// are zero.</b> The standing ruling is explicit and applies everywhere. They are different faults:
/// a miss is a receiver that could not hear, and a wrong decode is §0.0's own failure — a message on
/// the screen that nobody sent.
/// </para>
/// <para>
/// <b>Every message is sent at its own frequency</b>, cycling across the passband, so a hundred
/// agreements are a hundred different places in the waterfall rather than one place a hundred times.
/// </para>
/// </remarks>
public class Ft4RoundTripTests
{
    private readonly Unit289Report _output;

    public Ft4RoundTripTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task6-round-trip", output);

    /// <summary>
    /// The frequencies the corpus is sent at, cycled. <b>Every one is an exact multiple of the tone
    /// spacing</b> — 20.8333 Hz, the reciprocal of the symbol period — so each sits on a waterfall
    /// bin centre. Off-grid placement is a different measurement and this unit did not take it.
    /// </summary>
    private static readonly float[] BaseFrequencies =
    [
        500.0f, 750.0f, 1000.0f, 1250.0f, 1500.0f, 1750.0f, 2000.0f, 2500.0f,
    ];

    [Fact]
    public void AHundredFt4MessagesComeBackAsThemselvesWithNoWrongDecodes()
    {
        var corpus = Ft4RoundTripCorpus.Build();
        var decoder = new Ft4SlotDecoder();

        Assert.True(
            corpus.Count >= 100,
            $"step 1 asks for at least a hundred messages and the corpus holds {corpus.Count}");

        var sent = 0;
        var readBack = 0;
        var wrong = 0;
        var missed = 0;
        var wrongDetail = new List<string>();
        var missedDetail = new List<string>();
        var kinds = new SortedDictionary<string, (int Sent, int Back)>(StringComparer.Ordinal);

        for (var i = 0; i < corpus.Count; i++)
        {
            var entry = corpus[i];
            var frequency = BaseFrequencies[i % BaseFrequencies.Length];

            var symbols = Ft4SymbolEncoder.Encode(entry.Message);
            var slot = Ft4Waveform.SynthesizeSlot(symbols, Ft4Waveform.DefaultSampleRate, frequency);
            var result = decoder.Decode(slot);

            sent++;
            var counted = kinds.TryGetValue(entry.Kind, out var tally) ? tally : (Sent: 0, Back: 0);
            counted.Sent++;

            var itsOwn = false;
            foreach (var message in result.Messages)
            {
                if (message.Text == entry.Text)
                {
                    itsOwn = true;
                    continue;
                }

                // ONE transmission went into this slot, so anything else that came out of it is a
                // message nobody sent. Counted as wrong, separately from a miss, whatever else the
                // slot did.
                wrong++;
                wrongDetail.Add(
                    $"sent \"{entry.Text}\" at {frequency:F1} Hz and read \"{message.Text}\"");
            }

            if (itsOwn)
            {
                readBack++;
                counted.Back++;
            }
            else
            {
                missed++;
                missedDetail.Add(
                    $"sent \"{entry.Text}\" ({entry.Kind}) at {frequency:F1} Hz and it did not come "
                    + $"back; the slot gave {result.Messages.Count} message(s) from "
                    + $"{result.CandidateCount} candidate(s)");
            }

            kinds[entry.Kind] = counted;
        }

        // ----------------------------------------------------------------- the timing, measured
        var reference = Ft4Waveform.SynthesizeSlot(
            Ft4SymbolEncoder.Encode(corpus[0].Message));
        var signal = Ft4Waveform.Synthesize(Ft4SymbolEncoder.Encode(corpus[0].Message));
        const int rate = Ft4Waveform.DefaultSampleRate;
        var samplesPerSymbol = signal.Length / Ft4SymbolEncoder.SymbolCount;

        _output.WriteLine("THE ROUND TRIP");
        _output.WriteLine($"  messages sent                {sent}");
        _output.WriteLine($"  read back as themselves      {readBack}");
        _output.WriteLine($"  WRONG decodes                {wrong}");
        _output.WriteLine($"  missed decodes               {missed}");
        _output.WriteLine($"  frequencies used             {BaseFrequencies.Length} across the passband");
        _output.WriteLine(string.Empty);
        _output.WriteLine("BY KIND");
        foreach (var (kind, tally) in kinds)
        {
            _output.WriteLine($"  {kind,-24} {tally.Back} of {tally.Sent}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE TIMING, MEASURED FROM THE AUDIO AND NOT READ BACK OFF A CONSTANT");
        _output.WriteLine($"  sample rate                  {rate} Hz");
        _output.WriteLine($"  samples per symbol           {samplesPerSymbol}");
        _output.WriteLine($"  symbol period                {samplesPerSymbol / (double)rate:F6} s");
        _output.WriteLine($"  symbol count                 "
            + $"{signal.Length / samplesPerSymbol}");
        _output.WriteLine($"  occupancy                    {signal.Length / (double)rate:F4} s "
            + $"({signal.Length} samples)");
        _output.WriteLine($"  tone count                   {Ft4Waveform.ToneCount}");
        _output.WriteLine($"  tone spacing                 {rate / (double)samplesPerSymbol:F4} Hz");
        _output.WriteLine($"  slot length                  {reference.Length / (double)rate:F4} s "
            + $"({reference.Length} samples)");
        _output.WriteLine($"  signal starts at             "
            + $"{Ft4Waveform.PaddingSampleCount(rate) / (double)rate:F4} s");
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE 4.48 AGAINST 5.04 DISAGREEMENT");
        _output.WriteLine("  PHASE_PLAN.md step 1 calls FT4's transmission 4.48 s.");
        _output.WriteLine("  Upstream's FT4_SYMBOL_PERIOD is 0.048f, which puts 105 symbols at "
            + "5.04 s.");
        _output.WriteLine("  The string 4.48 appears nowhere in the pinned clone.");
        _output.WriteLine($"  Measured from this audio: {signal.Length / (double)rate:F4} s.");
        _output.WriteLine("  THIS UNIT BUILT ON 5.04, because the standing ruling is that Ft8Sharp "
            + "is a faithful");
        _output.WriteLine("  MIT port and a deliberate divergence from upstream is not a unit's to "
            + "make. Nothing");
        _output.WriteLine("  here settles it; the question is the owner's. The constant lives in "
            + "exactly one");
        _output.WriteLine("  place, src/Ft8Sharp/Ft4Timing.cs, so a ruling the other way is one "
            + "edit.");

        if (wrongDetail.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("WRONG DECODES");
            foreach (var line in wrongDetail)
            {
                _output.WriteLine("  " + line);
            }
        }

        if (missedDetail.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("MISSED DECODES");
            foreach (var line in missedDetail)
            {
                _output.WriteLine("  " + line);
            }
        }

        // The measured geometry, asserted rather than only printed.
        Assert.Equal(576, samplesPerSymbol);
        Assert.Equal(105, signal.Length / samplesPerSymbol);
        Assert.Equal(60480, signal.Length);
        Assert.Equal(90000, reference.Length);
        Assert.Equal(4, Ft4Waveform.ToneCount);

        // ZERO WRONG DECODES is the criterion, and it is asserted before the miss count so that a
        // failure names the worse of the two faults first.
        Assert.True(
            wrong == 0,
            $"{wrong} wrong decode(s) — a message on the screen nobody sent:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, wrongDetail));

        Assert.True(
            missed == 0,
            $"{missed} of {sent} messages did not come back as themselves:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, missedDetail));

        Assert.Equal(sent, readBack);
        Assert.True(readBack >= 100, $"only {readBack} messages round tripped and step 1 asks 100");
    }
}
