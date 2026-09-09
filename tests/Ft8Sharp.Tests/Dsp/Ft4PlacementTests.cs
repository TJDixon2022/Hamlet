using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Unit 289 task 1 — the trace. <b>Where an FT4 transmission lands in its own slot, and whether the
/// candidate sweep can reach it.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The breakage this catches</b>: an FT4 signal synthesized into the middle of its slot, decoding
/// nothing, and looking exactly like a decoder that does not work. Unit 288 spent real time
/// discovering that upstream's own FT4 decoder reads zero messages out of upstream's own FT4
/// generator, and the cause is placement rather than protocol. Nothing else in this project would
/// have told an FT4 round trip apart from a broken demodulator.
/// </para>
/// <para>
/// <b>The FT8 control runs first and in the same test.</b> A placement assertion that failed would
/// be read as a defect in the chain unless the chain is shown working on the protocol it already
/// carries, so the same encode-synthesize-decode path is walked for FT8 and its own time offset is
/// recorded before a single FT4 number is computed.
/// </para>
/// </remarks>
public class Ft4PlacementTests
{
    private readonly Unit289Report _output;

    public Ft4PlacementTests(ITestOutputHelper output) =>
        _output = new Unit289Report("task1-placement", output);

    /// <summary>How many symbols an FT4 transmission carries. Upstream's <c>FT4_NN</c>.</summary>
    private const int Ft4SymbolCount = 105;

    /// <summary>FT4's symbol period in seconds. Upstream's <c>FT4_SYMBOL_PERIOD</c>.</summary>
    private const float Ft4SymbolPeriodSeconds = 0.048f;

    /// <summary>FT4's slot in seconds. Upstream's <c>FT4_SLOT_TIME</c>.</summary>
    private const float Ft4SlotSeconds = 7.5f;

    [Fact]
    public void TheFt8ControlRoundTripsAndTheFt4PlacementIsOutsideTheDemoApplicationsSweep()
    {
        // ---------------------------------------------------------------- the FT8 control
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        var slot = Ft8Waveform.SynthesizeSlot(symbols);

        var decoder = new Ft8SlotDecoder();
        var result = decoder.Decode(slot);

        Assert.Single(result.Messages);
        var found = result.Messages[0];
        Assert.Equal("CQ K1ABC FN42", found.Text);

        var geometry = decoder.Geometry;
        var ft8PaddingSamples = Ft8Waveform.PaddingSampleCount(Ft8Waveform.DefaultSampleRate);
        var ft8PaddingSeconds = ft8PaddingSamples / (double)Ft8Waveform.DefaultSampleRate;
        var ft8PaddingBlocks = ft8PaddingSamples / (double)geometry.BlockSize;

        _output.WriteLine("FT8 CONTROL");
        _output.WriteLine($"  text                 {found.Text}");
        _output.WriteLine($"  slot samples         {slot.Length}");
        _output.WriteLine($"  signal starts at     {ft8PaddingSamples} samples "
            + $"= {ft8PaddingSeconds:F4} s = {ft8PaddingBlocks:F4} blocks of {geometry.BlockSize}");
        _output.WriteLine($"  candidate block      {found.Candidate.BlockOffset} "
            + $"sub {found.Candidate.TimeSubOffset} of {geometry.TimeOversampling}");
        _output.WriteLine($"  candidate time       {found.TimeSeconds(geometry):F4} s");
        _output.WriteLine($"  candidate frequency  {found.FrequencyHz(geometry):F3} Hz");
        _output.WriteLine($"  candidate score      {found.Candidate.Score}");
        _output.WriteLine($"  candidates found     {result.CandidateCount}");

        // The FT8 signal lands inside the sweep the search actually runs, which is why FT8 works.
        Assert.InRange(
            found.Candidate.BlockOffset,
            Ft8SyncSearch.DefaultFirstBlockOffset,
            Ft8SyncSearch.DefaultLastBlockOffset);

        // ---------------------------------------------------------------- the FT4 arithmetic
        // Upstream's own expressions from demo/gen_ft8.c:174-176, in upstream's own precision.
        const int sampleRate = Ft8Waveform.DefaultSampleRate;
        var ft4SignalSamples = (int)(0.5f + (Ft4SymbolCount * Ft4SymbolPeriodSeconds * sampleRate));
        var ft4PaddingSamples = (int)(((Ft4SlotSeconds * sampleRate) - ft4SignalSamples) / 2);
        var ft4BlockSize = (int)(float)(sampleRate * Ft4SymbolPeriodSeconds);
        var ft4PaddingSeconds = ft4PaddingSamples / (double)sampleRate;
        var ft4PaddingBlocks = ft4PaddingSamples / (double)ft4BlockSize;
        var ft4SlotBlocks = (int)(float)(Ft4SlotSeconds / Ft4SymbolPeriodSeconds);

        // The demo application's sweep, at FT4's block, in seconds.
        var demoEarliest = Ft8SyncSearch.DefaultFirstBlockOffset * (double)Ft4SymbolPeriodSeconds;
        var demoLatest = Ft8SyncSearch.DefaultLastBlockOffset * (double)Ft4SymbolPeriodSeconds;

        _output.WriteLine(string.Empty);
        _output.WriteLine("FT4 PLACEMENT, ON UPSTREAM'S CONSTANTS");
        _output.WriteLine($"  symbol period        {Ft4SymbolPeriodSeconds} s");
        _output.WriteLine($"  symbols              {Ft4SymbolCount}");
        _output.WriteLine($"  occupancy            {Ft4SymbolCount * (double)Ft4SymbolPeriodSeconds:F4} s "
            + $"= {ft4SignalSamples} samples");
        _output.WriteLine($"  slot                 {Ft4SlotSeconds} s = {ft4SlotBlocks} blocks");
        _output.WriteLine($"  block                {ft4BlockSize} samples");
        _output.WriteLine($"  centred padding      {ft4PaddingSamples} samples "
            + $"= {ft4PaddingSeconds:F4} s = {ft4PaddingBlocks:F4} blocks");
        _output.WriteLine($"  demo sweep           blocks {Ft8SyncSearch.DefaultFirstBlockOffset} to "
            + $"{Ft8SyncSearch.DefaultLastBlockOffset} = {demoEarliest:F4} s to {demoLatest:F4} s");
        _output.WriteLine($"  inside that sweep?   {(ft4PaddingSeconds <= demoLatest ? "yes" : "NO")}");

        Assert.Equal(60480, ft4SignalSamples);
        Assert.Equal(14760, ft4PaddingSamples);
        Assert.Equal(576, ft4BlockSize);
        Assert.Equal(156, ft4SlotBlocks);

        // THE FINDING, ASSERTED RATHER THAN WRITTEN DOWN. A centred FT4 signal starts at 1.23 s,
        // and the demo application's own sweep stops at 0.912 s. Upstream's decoder cannot reach
        // upstream's generator, and a port that inherited the sweep would read zero and look broken.
        Assert.True(
            ft4PaddingSeconds > demoLatest,
            $"a centred FT4 signal starts at {ft4PaddingSeconds:F4} s and the demo application's "
            + $"sweep reaches {demoLatest:F4} s. If this ever becomes false the reasoning behind "
            + "Ft4SyncSearch's widened sweep has changed and must be re-read.");

        // WHAT PLACEMENT WOULD BE INSIDE, AND WHAT THIS UNIT DOES INSTEAD. Task 4 keeps upstream's
        // centred padding, because a waveform that is not upstream's is not a faithful port, and
        // widens the sweep instead: the sweep bound is the demo application's judgement about how
        // much work to do and is a constructor parameter in this port, not a protocol constant.
        // A transmission wholly inside its own slot starts somewhere in blocks 0 to 51.
        var latestWhollyInside = ft4SlotBlocks - Ft4SymbolCount;
        Assert.Equal(51, latestWhollyInside);
        Assert.InRange((int)ft4PaddingBlocks, 0, latestWhollyInside);

        _output.WriteLine($"  wholly-inside range  blocks 0 to {latestWhollyInside}");
        _output.WriteLine($"  centred start block  {(int)ft4PaddingBlocks} (fraction "
            + $"{ft4PaddingBlocks - (int)ft4PaddingBlocks:F4} of a block)");
    }
}
