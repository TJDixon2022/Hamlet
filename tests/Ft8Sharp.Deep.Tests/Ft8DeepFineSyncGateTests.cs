using System;
using System.Collections.Generic;
using System.Linq;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The rules the fine synchronisation stage runs under, asserted rather than intended.</b>
/// Off by default; only where the port refused; one submission a candidate; the port's gates are the
/// only acceptance; and re-syncing only ever adds.
/// </summary>
/// <remarks>
/// <para>
/// <b>The false-accept arithmetic, in full, because it is the thing this stage could get quietly
/// wrong.</b> Every codeword put to the CRC-14 is an independent chance of a false accept at about
/// <b>one in 16 384</b>. Fine sync submits exactly one codeword per candidate the port refused, so a
/// slot at the port's candidate limit of 140 submits at most 140 and the expected wrong count is
/// about <b>0.009 a slot</b> - the same bound the ordered statistics stage runs under, and about
/// twice today's total when the port's own submissions are counted beside it.
/// <c>Ft8DeepCombineSettings.ExpectedFalseAccepts</c> is that arithmetic already written down and it
/// is used rather than restated.
/// </para>
/// <para>
/// <b>What would break the bound is submitting a search's worth.</b> The search visits 119 positions
/// a candidate; putting all of them to the gate would be 16 660 codewords a slot and about one
/// message nobody sent every slot, each carrying a valid checksum and looking exactly like a decode.
/// <b>One position is chosen - the best by Costas correlation - and that one is submitted once.</b>
/// </para>
/// </remarks>
public class Ft8DeepFineSyncGateTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const string Text = "HAMLET 248";

    /// <summary>
    /// <b>Off is the default, and off is what the scoreboard's baseline column is built on.</b>
    /// </summary>
    [Fact]
    public void FineSynchronisationIsOffUnlessItIsAskedFor()
    {
        Assert.Null(new Ft8DeepSlotDecoder().FineSync);
        Assert.Null(new Ft8DeepSlotDecoder(new Ft8SlotDecoder()).FineSync);
        Assert.Null(new Ft8DeepSlotDecoder().Baseband);

        var on = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);
        Assert.NotNull(on.FineSync);
        Assert.Equal(0.04, on.FineSync!.TimeExtentSeconds, 9);

        // AND WITH IT OFF THE COUNTS STAY ZERO, which is what makes "a rate that moved with no
        // visible re-sync behind it is not evidence" checkable.
        var off = new Ft8DeepSlotDecoder();
        off.Decode(Noisy(CleanSlot(Text), 6.0, 248_201));

        Assert.Equal(default, off.LastFineSync);
    }

    /// <summary>
    /// <b>A waterfall has no samples behind it, so the waterfall-only entry point re-syncs nothing
    /// and says so through a count.</b> It does not throw and it does not pretend.
    /// </summary>
    [Fact]
    public void TheWaterfallOnlyEntryPointResyncsNothingAndSaysSo()
    {
        var samples = Noisy(CleanSlot(Text), 6.0, 248_202);
        var geometry = new Ft8WaterfallGeometry(Rate);
        var waterfall = new Ft8Monitor(geometry).Analyse(samples);

        var decoder = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);

        var fromWaterfall = decoder.Decode(waterfall);
        var counts = decoder.LastFineSync;

        output.WriteLine($"  offered {counts.Offered}, re-synced {counts.Resynced}, "
            + $"refused for want of samples {counts.RefusedForWantOfSamples}");

        Assert.True(counts.Offered > 0, "no candidate was refused, so this test measured nothing.");
        Assert.Equal(0, counts.Resynced);
        Assert.Equal(0, counts.Accepted);
        Assert.Equal(counts.Offered, counts.RefusedForWantOfSamples);

        // AND IT IS EXACTLY WHAT THE PORT RETURNS, because nothing ran.
        var port = new Ft8SlotDecoder().Decode(waterfall);
        Assert.Equal(port.Texts, fromWaterfall.Texts);
        Assert.Equal(port.CandidateCount, fromWaterfall.CandidateCount);
        Assert.Equal(port.ParitySatisfiedCount, fromWaterfall.ParitySatisfiedCount);
        Assert.Equal(port.ChecksumPassedCount, fromWaterfall.ChecksumPassedCount);
    }

    /// <summary>
    /// <b>Re-syncing only ever adds.</b> Every message the ordinary path returned is still there,
    /// and the extra ones are extra.
    /// </summary>
    /// <remarks>
    /// <b>The relative order of the ordinary messages is preserved too</b>, and that is asserted as a
    /// subsequence rather than as a prefix: fine sync runs at the candidate the port refused, so a
    /// message it rescues is inserted at that candidate's place in the list rather than appended.
    /// </remarks>
    [Fact]
    public void EveryMessageTheOrdinaryPathReturnedIsStillThere()
    {
        var clean = CleanSlot(Text);
        var added = 0;
        var levels = 0;

        foreach (var amplitude in new[] { 2.0, 4.0, 6.0, 8.0, 10.0, 12.0 })
        {
            var samples = Noisy(clean, amplitude, 248_300 + (int)amplitude);

            var off = new Ft8DeepSlotDecoder().Decode(samples);
            var on = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default)
                .Decode(samples);

            output.WriteLine(
                $"  noise {amplitude,5:F1}   off {off.Messages.Count} messages   "
                + $"on {on.Messages.Count}");

            Assert.True(
                on.Messages.Count >= off.Messages.Count,
                $"noise {amplitude:F1}: fine sync off returned {off.Messages.Count} messages and "
                    + $"fine sync on returned {on.Messages.Count}. Re-syncing only ever adds.");

            // THE SUBSEQUENCE, walked rather than assumed.
            var next = 0;
            foreach (var text in off.Texts)
            {
                var at = on.Texts.Skip(next).ToList().IndexOf(text);
                Assert.True(
                    at >= 0,
                    $"noise {amplitude:F1}: \"{text}\" was returned with fine sync off and is not "
                        + "in the fine sync result in order. Re-syncing only ever adds.");

                next += at + 1;
            }

            added += on.Messages.Count - off.Messages.Count;
            levels++;
        }

        output.WriteLine(string.Empty);
        output.WriteLine($"  {added} messages added over {levels} noise levels, none lost.");
    }

    /// <summary>
    /// <b>Fine sync never runs where the port decoded</b>, which is what makes the gain attributable
    /// and what bounds the submissions.
    /// </summary>
    [Fact]
    public void FineSyncIsOfferedOnlyTheCandidatesThePortRefused()
    {
        foreach (var amplitude in new[] { 2.0, 6.0, 10.0 })
        {
            var samples = Noisy(CleanSlot(Text), amplitude, 248_400 + (int)amplitude);

            var decoder = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);
            var result = decoder.Decode(samples);
            var counts = decoder.LastFineSync;

            var decodedByThePort = result.BecameTextCount - counts.Accepted;

            output.WriteLine(
                $"  noise {amplitude,5:F1}   candidates {result.CandidateCount,4}   "
                + $"offered {counts.Offered,4}   re-synced {counts.Resynced,4}   "
                + $"accepted {counts.Accepted,3}   the port decoded {decodedByThePort,3}");

            // OFFERED PLUS DECODED BY THE PORT IS EVERY CANDIDATE, exactly.
            Assert.Equal(result.CandidateCount, counts.Offered + decodedByThePort);

            // ONE SUBMISSION EACH AND NEVER MORE.
            Assert.Equal(counts.Offered, counts.Resynced);
            Assert.Equal(counts.Resynced, counts.Submissions);
            Assert.True(counts.Accepted <= counts.Resynced);
        }
    }

    /// <summary>
    /// <b>The submission arithmetic, stated in full and computed with the library's own
    /// expression.</b>
    /// </summary>
    [Fact]
    public void TheSubmissionArithmeticIsBoundedAtOnePerRefusedCandidate()
    {
        var submissions = 0;
        var slots = 0;
        var worst = 0;

        foreach (var amplitude in new[] { 2.0, 4.0, 6.0, 8.0, 10.0, 12.0 })
        {
            var decoder = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);
            decoder.Decode(Noisy(CleanSlot(Text), amplitude, 248_500 + (int)amplitude));

            submissions += decoder.LastFineSync.Submissions;
            worst = Math.Max(worst, decoder.LastFineSync.Submissions);
            slots++;
        }

        var expected = Ft8DeepCombineSettings.ExpectedFalseAccepts(submissions);

        output.WriteLine($"  {slots} slots, {submissions} submissions, worst slot {worst}");
        output.WriteLine($"  expected false accepts across the whole measurement: {expected:F4}");
        output.WriteLine($"  the port's candidate limit is {new Ft8SyncSearch().CandidateLimit}, so "
            + "the worst a slot could submit is that many");
        output.WriteLine($"  which is {Ft8DeepCombineSettings.ExpectedFalseAccepts(140):F4} expected "
            + "false accepts a slot at the limit");

        Assert.True(worst <= new Ft8SyncSearch().CandidateLimit);
    }

    /// <summary>
    /// <b>The everything-off identity: OSD off, fine sync off, hearings off is the port, whole
    /// result for whole result.</b>
    /// </summary>
    /// <remarks>
    /// <b>Not optional and not to be weakened.</b> It is what keeps the sibling an instrument: every
    /// difference between the scoreboard's columns is attributable to one named change only while
    /// this holds.
    /// </remarks>
    [Fact]
    public void WithEverythingOffTheWholeResultIsThePortsWholeResult()
    {
        foreach (var amplitude in new[] { 0.0, 2.0, 6.0, 10.0 })
        {
            var samples = Noisy(CleanSlot(Text), amplitude, 248_600 + (int)amplitude);

            var port = new Ft8SlotDecoder().Decode(samples);
            var deep = new Ft8DeepSlotDecoder().Decode(samples);

            Assert.Equal(port.CandidateCount, deep.CandidateCount);
            Assert.Equal(port.ParitySatisfiedCount, deep.ParitySatisfiedCount);
            Assert.Equal(port.ChecksumPassedCount, deep.ChecksumPassedCount);
            Assert.Equal(port.BecameTextCount, deep.BecameTextCount);
            Assert.Equal(port.DuplicateCount, deep.DuplicateCount);
            Assert.Equal(port.Texts, deep.Texts);

            output.WriteLine(
                $"  noise {amplitude,5:F1}   {port.CandidateCount,4} candidates, "
                + $"{port.Messages.Count} messages, identical");
        }
    }

    private static float[] CleanSlot(string text)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        var signal = Ft8Waveform.Synthesize(symbols, Rate, 1000.0f);

        var slot = new float[Ft8Waveform.SlotSampleCount(Rate)];

        // DELIBERATELY OFF THE GRID IN BOTH AXES: half a sub-block and half a transform bin, which
        // is the placement unit 248 task 1 measured as the worst in the cell.
        const int offset = 5760 + 480;
        for (var i = 0; i < signal.Length && offset + i < slot.Length; i++)
        {
            slot[offset + i] = signal[i];
        }

        return slot;
    }

    private static float[] Noisy(float[] slot, double sigma, int seed)
    {
        if (sigma <= 0.0)
        {
            return (float[])slot.Clone();
        }

        var random = new Random(seed);
        var noisy = new float[slot.Length];

        for (var i = 0; i < slot.Length; i++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();
            var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            noisy[i] = (float)(slot[i] + (sigma * normal));
        }

        return noisy;
    }
}
