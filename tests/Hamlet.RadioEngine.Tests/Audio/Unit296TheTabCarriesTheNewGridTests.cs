using System.Diagnostics;
using Ft8Sharp;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// <b>Unit 296 task 6 — the operator's own path really carries the grid task 4 moved, and it still
/// reads through <c>Ft8Sharp</c> and never through <c>Ft8Sharp.Deep</c>.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS CATCHES, AND IT IS THE SHAPE HALF THIS PROJECT'S OPEN CARDS HAVE.</b> A
/// default that moves in the library and does not reach the application is a measurement about a
/// decoder the operator never runs. <c>Ft8Reception.ReadFt4</c> builds
/// <c>new Ft4SlotDecoder()</c> with no arguments and hands <c>decoder.Geometry</c> to its own
/// monitor, so a moved default <em>should</em> reach the tab — but *should* is what a remark says
/// and this is what a test says. It drives one FT4 slot through <see cref="Ft8Reader.Read"/> with
/// <see cref="DigitalMode.Ft4"/>, which is the reader the operator's press runs, at a placement
/// that is <b>deliberately not on the analysis grid</b>, and reads the message back out.
/// </para>
/// <para>
/// <b>AND THE SECOND THING IT CATCHES IS A LICENCE BREACH.</b> <c>Ft8Sharp</c> is MIT and
/// <c>Ft8Sharp.Deep</c> is GPL-3.0, and FT4 reads through the port alone. The census stamps
/// <see cref="Ft8DecoderIdentity.Port"/>, whose name is <c>Ft8Sharp</c>, and that is asserted here
/// rather than assumed — a slot that came back naming <c>Ft8Sharp.Deep</c> would mean FT4 had
/// quietly acquired stages nobody measured it with.
/// </para>
/// <para>
/// <b>The placement is off grid on both axes and it is arithmetic rather than a hope.</b> The
/// station sits a third of a bin above a bin centre and a third of a sub-block late, which is not
/// a whole number of either at any oversampling this project builds.
/// </para>
/// </remarks>
public sealed class Unit296TheTabCarriesTheNewGridTests(ITestOutputHelper output)
{
    /// <summary>What a sound card delivers, and the rate the port analyses at.</summary>
    private const int DeviceRate = 12_000;

    /// <summary>
    /// Where the synthesized station sits: 1240 Hz — unit 294's own passband placement — plus a
    /// third of an FT4 bin at upstream's two bins per tone.
    /// </summary>
    private const float PlacedAtHz = 1240.0f + (20.8333f / 2.0f / 3.0f);

    /// <summary>The recording ends on an FT4 boundary, so the cutter finds two whole 7.5 s slots.</summary>
    private static readonly DateTime EndedAt =
        new(2026, 9, 2, 14, 22, 45, DateTimeKind.Utc);

    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>The message the synthesized station sends.</summary>
    private const string Text = "CQ W9GAP EM12";

    /// <summary>
    /// <b>One FT4 decode driven through the tab's own path, off grid, with the geometry and the
    /// time named.</b>
    /// </summary>
    [Fact]
    public void OneFt4SlotThroughTheReaderTheTabRunsCarriesTheNewGrid()
    {
        // THE GEOMETRY THE READER WILL BUILD. Ft8Reception.ReadFt4 is one line -
        // `var decoder = new Ft4SlotDecoder();` - so the decoder built here reports what the
        // reader's does, and there is nothing for the two to disagree about.
        var asTheReaderBuildsIt = new Ft4SlotDecoder();
        var geometry = asTheReaderBuildsIt.Geometry;

        output.WriteLine("THE GEOMETRY Ft8Reception.ReadFt4 BUILDS, WITH NO ARGUMENTS");
        output.WriteLine(
            $"  time oversampling      {geometry.TimeOversampling}   (upstream's demo passes 2)");
        output.WriteLine(
            $"  frequency oversampling {geometry.FrequencyOversampling}   (upstream's, unmoved)");
        output.WriteLine(
            $"  block {geometry.BlockSize}, sub-block {geometry.SubblockSize}, transform "
            + $"{geometry.TransformLength}, bins {geometry.BinCount}, blocks {geometry.MaxBlocks}");

        Assert.Equal(Ft4WaterfallGeometry.DefaultTimeOversampling, geometry.TimeOversampling);
        Assert.Equal(4, geometry.TimeOversampling);
        Assert.Equal(2, geometry.FrequencyOversampling);

        var message = new byte[Ft8Payload.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "W9GAP", "EM12", message));

        var symbols = Ft4SymbolEncoder.Encode(message);
        var signal = Ft4Waveform.Synthesize(symbols, DeviceRate, PlacedAtHz);

        // FIFTEEN SECONDS ENDING ON AN FT4 BOUNDARY: the cutter finds two whole 7.5 s slots and
        // the transmission is inside the second of them. The lead is the waveform's own padding
        // plus a third of an upstream sub-block, so the station is off grid in time as well as in
        // frequency and is on no grid at any oversampling.
        var offGridLead = 288 / 3;
        var samples = new float[DeviceRate * 15];
        var lead = (DeviceRate * 15 / 2) + Ft4Waveform.PaddingSampleCount(DeviceRate) + offGridLead;
        signal.CopyTo(samples.AsSpan(lead));

        output.WriteLine(string.Empty);
        output.WriteLine("WHERE THE STATION WAS PUT, AND IT IS NOT ON THE GRID");
        output.WriteLine(
            $"  {PlacedAtHz:F4} Hz, which is {PlacedAtHz / (DeviceRate / (double)geometry.TransformLength):F4} "
            + "bins - a third of a bin above a centre");
        output.WriteLine(
            $"  lead {lead} samples, which is {lead / (double)geometry.SubblockSize:F4} sub-blocks "
            + "at the shipping grid");

        var watch = Stopwatch.StartNew();
        var heard = Ft8Reader.Read(
            new MonoAudio(DeviceRate, samples),
            EndedAt,
            Measured,
            mode: DigitalMode.Ft4);
        var elapsed = watch.Elapsed;

        output.WriteLine(string.Empty);
        output.WriteLine("WHAT THE READER THE TAB RUNS GAVE BACK");
        output.WriteLine($"  refusal          \"{heard.Refusal}\"");
        output.WriteLine($"  slots decoded    {heard.SlotsDecoded}");
        output.WriteLine($"  messages         {heard.Decodes.Count}");

        foreach (var decode in heard.Decodes)
        {
            output.WriteLine(
                $"    \"{decode.Message}\" at {decode.FrequencyHz:F1} Hz, snr "
                + $"{(decode.SignalToNoiseDb is { } db ? $"{db:F1} dB" : "not measured")}");
        }

        output.WriteLine(
            $"  {heard.SlotsDecoded} slots in {elapsed.TotalMilliseconds:F0} ms, which is "
            + $"{elapsed.TotalMilliseconds / Math.Max(1, heard.SlotsDecoded):F0} ms a slot through "
            + "the whole reader - the cut, the resample, the waterfall, the search, the decode and "
            + "the ratio, not the decode alone");
        output.WriteLine(
            $"  a slot is {Ft4Timing.SlotSeconds:F1} s, so the reader spent "
            + $"{elapsed.TotalMilliseconds / Math.Max(1, heard.SlotsDecoded) / (Ft4Timing.SlotSeconds * 10.0):F2}% "
            + "of one");

        Assert.Equal(string.Empty, heard.Refusal);
        Assert.Contains(heard.Decodes, d => d.Message == Text);

        // THE LICENCE, ASSERTED. FT4 reads through the MIT port and never through GPL-3.0 Deep.
        foreach (var slot in heard.Slots)
        {
            output.WriteLine($"  slot at {slot.SlotStartUtc:HH:mm:ss} read by {slot.Decoder}");

            Assert.True(slot.Decoder.IsRecorded, "a slot did not name its decoder");
            Assert.Equal("Ft8Sharp", slot.Decoder.Name);
            Assert.False(slot.Decoder.FineSync, "FT4 has no fine sync and must not claim one");
            Assert.False(
                slot.Decoder.OrderedStatistics,
                "FT4 has no ordered statistics and must not claim any");
        }

        // AND THE READER FINISHED INSIDE THE SLOT IT WAS READING. A grid that decodes beautifully
        // in nine seconds is not a fix for a 7.5 second slot.
        Assert.True(
            elapsed.TotalSeconds < Ft4Timing.SlotSeconds,
            $"the reader took {elapsed.TotalSeconds:F2} s over {heard.SlotsDecoded} slots, which "
            + $"does not fit inside the {Ft4Timing.SlotSeconds:F1} s slot the tab has");
    }
}
