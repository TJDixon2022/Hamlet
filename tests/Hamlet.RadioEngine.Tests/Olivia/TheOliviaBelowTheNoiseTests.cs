using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 363 task 2: **Olivia reads below the noise - an 8/250 QSO at -14 dB in 2500 Hz,
/// RSID in front, read at CER 0.10 or under with the variant from its RSID** (step 3 criterion 3.0).
/// </summary>
/// <remarks>
/// <para>**THE AUDIO IS MADE BY DECISION W'S RECIPE AND WRITTEN NOWHERE** (<see cref="OliviaBelowTheNoise"/>):
/// the fldigi-port burst from the front of the 8/250 CQ file in front of the whole of the mode
/// author's no-RSID 8/250 QSO, seeded Gaussian noise at -14 dB in 2500 Hz, the SNR measured back.</para>
/// <para>**THE VARIANT AND CENTER COME FROM THE MADE AUDIO'S RSID** (decisions I and X); the manifest
/// only checks them. **The 0.10 is never loosened, the SNR never changed and the seed never chosen
/// after its result** (§10): the asserted seed is <see cref="OliviaBelowTheNoise.StatedSeed"/>, and
/// four more are printed and not asserted, so the report shows whether 0.10 was the mode's or
/// luck's.</para>
/// <para>**RUN ALONE, BECAUSE THE CPU IS THE PROCESS'S** (<see cref="CpuMeasuredAlone"/>).
/// **COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaBelowTheNoiseTests
{
    private const double CpuCeilingSeconds = 20.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public TheOliviaBelowTheNoiseTests(ITestOutputHelper output) => _output = output;

    private static RsidCodes Codes => OliviaData.Current.Rsid!;

    private static OliviaFormat Format => OliviaData.Current.Format!;

    /// <summary>
    /// **3.0: on the stated seed, the RSID names 8/250 within 5 Hz of 1000, and the demodulator built
    /// from it reads the QSO at CER 0.10 or under, every character from an accepted block, inside 2.5's
    /// twenty seconds of CPU.**
    /// </summary>
    [Fact]
    public void TheQsoIsReadBelowTheNoise()
    {
        var (made, heard, decoding, variant, cpu) = Read(OliviaBelowTheNoise.StatedSeed);
        var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, made.Qso.Text);

        // **THE MANIFEST CHECKS THE DETECTION; IT DOES NOT FEED THE DEMODULATOR** (decision I).
        Assert.Equal(made.Qso.Variant, heard.Variant);
        Assert.InRange(heard.CenterHz, made.Qso.CenterHz - 5, made.Qso.CenterHz + 5);

        Assert.True(cer <= 0.10, $"seed {made.Seed}: CER {cer:0.0000} over 0.10");
        Assert.True(cpu < CpuCeilingSeconds, $"seed {made.Seed}: demodulator cpu {cpu:0.000} s");

        // **A BLOCK OR NOTHING** (§3.3, §R9): every block that gave characters cleared the
        // threshold, and no more characters are shown than those blocks could carry.
        Assert.Equal(decoding.BlocksDecoded, decoding.BlockSnrs.Count(s => s >= OliviaDemodulator.SyncThreshold));
        Assert.Equal(decoding.Text.Length, decoding.CharactersOut);
        Assert.InRange(decoding.CharactersOut, 0, decoding.BlocksDecoded * variant.BitsPerSymbol);
    }

    /// <summary>**The four further seeds, printed and not asserted**: one realization of noise is one draw.</summary>
    [Fact]
    public void TheFurtherSeedsArePrinted()
    {
        foreach (var seed in OliviaBelowTheNoise.FurtherSeeds)
        {
            Read(seed);
        }
    }

    private (BelowTheNoiseAudio Made, RsidDetection Heard, OliviaDecoding Decoding, OliviaVariant Variant, double Cpu) Read(int seed)
    {
        var made = OliviaBelowTheNoise.Make(seed);
        var burstEnd = made.BurstStartSeconds + (Codes.Symbols / Codes.SymbolRateHz);
        var measured = OliviaBelowTheNoise.MeasureSnrDb(made.Audio, burstEnd);
        var heard = Assert.Single(
            RsidDetector.Detect(Codes, made.Audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz),
            d => d.Mode == "OLIVIA");
        var variant = Format.Variant(heard.Variant)!;
        var demodulator = new OliviaDemodulator(Format, variant, heard.CenterHz, made.Audio.SampleRate);
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var decoding = demodulator.Decode(made.Audio, heard.StartSeconds + (Codes.Symbols / Codes.SymbolRateHz));
        var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;
        var cer = OliviaFixtures.CharacterErrorRate(decoding.Text, made.Qso.Text);

        _output.WriteLine(
            $"seed {seed}{(seed == OliviaBelowTheNoise.StatedSeed ? " (asserted)" : " (printed, not asserted)")}: SNR set {OliviaBelowTheNoise.SnrDb:0.00} dB "
            + $"in {OliviaBelowTheNoise.ReferenceHz} Hz, measured back {measured:0.00} dB; RSID {heard.Name} at {heard.CenterHz:0.00} Hz, "
            + $"{heard.TonesRight} tones right; CER {cer:0.0000} (ceiling 0.10); characters {decoding.CharactersOut} of {made.Qso.Text.Length}; "
            + $"blocks decoded {decoding.BlocksDecoded}, rejected {decoding.BlocksRejected}; sync snr {decoding.SyncSnr:0.00}; "
            + $"cpu {cpu:0.000} s (2.5's ceiling {CpuCeilingSeconds} s)");

        if (cer > 0)
        {
            _output.WriteLine("decoded : " + JsonSerializer.Serialize(decoding.Text));
            _output.WriteLine("manifest: " + JsonSerializer.Serialize(made.Qso.Text));
        }

        return (made, heard, decoding, variant, cpu);
    }
}
