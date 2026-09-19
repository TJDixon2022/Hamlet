using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 364 task 4: **a channel is retired when its station goes, never while it is
/// still sending, on a window that is the variant's timing table times a stated factor** (step 3
/// criterion 3.4, the engine's half; the rows' half is `TheOliviaRowsTests`).
/// </summary>
/// <remarks>
/// <para>**THE FACTOR IS 56, NOT DECISION AJ'S 24, AND DECISION AJ SAYS WHY** (`Unit364Trace` item
/// 4): on the -16 dB 16/500 file the listener goes 28.24 s - 55.2 characters - without a new
/// accepted block while its station is still sending, so 24 would retire it mid-transmission; 56 is
/// the smallest whole number that holds on every shipped file.</para>
/// <para>**RUN ALONE, BECAUSE THE FEEDS ARE THE PROCESS'S** (<see cref="CpuMeasuredAlone"/>).
/// **COMPUTED, NOT SEEN** (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class TheOliviaRetireTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each feed is printed.</param>
    public TheOliviaRetireTests(ITestOutputHelper output) => _output = output;

    private static OliviaData Data => OliviaData.Current;

    /// <summary>
    /// **No channel retires while its station is sending, on any shipped Olivia file**: each fed
    /// whole, a quarter-second at a time, with the retire window on, and no channel retired at any
    /// piece.
    /// </summary>
    /// <param name="file">The fixture.</param>
    [Theory]
    [InlineData("olivia-8-250-cq-rsid.wav")]
    [InlineData("olivia-16-500-qso-rsid.wav")]
    [InlineData("olivia-32-1000-qso-rsid.wav")]
    [InlineData("olivia-8-250-qso-norsid.wav")]
    [InlineData("olivia-16-500-qso-snr-10db.wav")]
    [InlineData("olivia-16-500-qso-snr-16db.wav")]
    [InlineData("olivia-two-signals-rsid.wav")]
    [InlineData("olivia-noise-only-30s.wav")]
    public void NoChannelRetiresWhileItsStationIsSending(string file)
    {
        var audio = WavAudio.Read(OliviaFixtures.Load(file).Path);
        var listener = new OliviaListener(
            Data.Format!, Data.Rsid!, audio.SampleRate, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz,
            timing: Data.Timing);
        var piece = audio.SampleRate / 4;
        var retired = 0;

        for (var at = 0; at < audio.Samples.Length; at += piece)
        {
            listener.Add(audio.Samples.AsSpan(at, Math.Min(piece, audio.Samples.Length - at)));

            retired = Math.Max(retired, listener.Channels.Count(c => c.Retired));
        }

        foreach (var c in listener.Channels)
        {
            _output.WriteLine(
                $"{file}: {c.Id} {c.Variant} by {c.Found}, window {Data.Timing!.RetireWindowSeconds(c.Variant):0.000} s, "
                + $"blocks {c.BlocksDecoded}, retired {c.Retired}");
        }

        Assert.Equal(0, retired);
    }

    /// <summary>
    /// **The window is derived**: for each of the three variants, the factor and the figure are read
    /// out of `timing.json` here and the window is their product - not a literal anywhere.
    /// </summary>
    [Fact]
    public void TheWindowIsTheFactorTimesTheTimingTable()
    {
        var path = Path.Combine(OliviaFixtures.Root(), "data", "olivia", "timing.json");

        using var file = JsonDocument.Parse(File.ReadAllText(path));

        var factor = file.RootElement.GetProperty("retire_after_characters").GetInt32();
        var timing = Data.Timing!;

        Assert.Equal(factor, timing.RetireAfterCharacters);

        foreach (var row in file.RootElement.GetProperty("variants").EnumerateArray())
        {
            var variant = row.GetProperty("variant").GetString()!;
            var spc = row.GetProperty("seconds_per_character").GetDouble();

            _output.WriteLine($"{variant,-8}: {spc} s a character x {factor} = {timing.RetireWindowSeconds(variant):0.000} s");

            Assert.Equal(spc * factor, timing.RetireWindowSeconds(variant), 9);
        }

        Assert.Equal(3, file.RootElement.GetProperty("variants").GetArrayLength());
    }
}
