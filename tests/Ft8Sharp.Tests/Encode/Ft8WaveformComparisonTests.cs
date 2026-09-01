using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Every sample this library synthesizes, held against every sample upstream's own generator writes
/// for the same message. <b>This is unit 212's target.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this and not the criterion as written.</b> Step 3's third exit criterion asks that the
/// reference <em>decoder</em> decode what we synthesize. That instrument is a different program from
/// the generator, it is not built on this machine, and no unit may build it — the permission scope
/// has no rule under which a unit runs a compiler, which is owner-class. So the criterion is not met
/// on its own terms and this unit does not claim it. What is taken instead is this: if our samples
/// are upstream's samples, then our audio <em>is</em> the audio every FT8 decoder in the world
/// already decodes. That is a stronger statement about the synthesizer than one successful decode on
/// every point but one — <b>nothing has demodulated this waveform</b>, not by us and not by anybody,
/// and that is steps 4 and 5.
/// </para>
/// <para>
/// <b>The comparison is not expected to be bit-identical and the bound is set from the
/// measurement.</b> Upstream computes in C single precision and this computes in .NET's, and the two
/// can round differently at the last place even where the arithmetic is the same arithmetic. So the
/// maximum is measured and reported first, and the bound is asserted afterwards against the number
/// it was chosen from. A bound chosen before the measurement is a guess.
/// </para>
/// <para>
/// <b>The alignment is read, not searched for.</b> Where the signal sits inside the file comes from
/// the pin's own timing, by way of <see cref="Ft8Waveform.PaddingSampleCount"/>, and
/// <see cref="UpstreamSynthesisInventoryTests"/> checks that against the file upstream actually
/// wrote. Nothing here cross-correlates to find an offset. If it ever has to, the report says so and
/// names the evidence as weaker.
/// </para>
/// <para>
/// <b>Nothing upstream's binary produced is committed.</b> Every WAV lives under
/// <see cref="Path.GetTempPath"/> and is deleted as soon as its message has been compared, rather
/// than at the end — fifty-one of them at roughly 350 KB apiece is some eighteen megabytes.
/// </para>
/// <para>
/// <b>It skips on every machine but this one.</b> That is the standing the plan already gives the
/// reference-WAV criterion, and what makes it worth something is that it ran here.
/// </para>
/// </remarks>
public class Ft8WaveformComparisonTests
{
    private readonly ITestOutputHelper _output;

    public Ft8WaveformComparisonTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The bound, in sixteen-bit counts, and it was written down after the measurement below was
    /// taken rather than before.
    /// </summary>
    /// <remarks>
    /// The instruction's own reading: a difference of a count or two in the last bit of a sample is
    /// the floating-point unit and not a defect, and a maximum above two counts is a finding rather
    /// than a tolerance to widen. The measured maximum is printed by every run of the test below, so
    /// this number can be checked against it by anybody, on any run, without reading a report.
    /// </remarks>
    private const int Bound = 2;

    /// <summary>What one message's comparison found.</summary>
    private sealed record Difference(int MaxAbsolute, int AtSample, int Differing, int Total);

    [RequiresWorkingOracleFact]
    public void EverySampleAgreesWithTheWavUpstreamWritesForTheSameMessage()
    {
        var corpus = EncodeCorpus.Build();
        var comparable = corpus.Where(e => e.Text is { Length: > 0 }).ToList();

        _output.WriteLine($"answering image         : {Ft8Oracle.ResolvedExecutablePath}");
        _output.WriteLine($"is a patched copy       : {Ft8Oracle.AnsweringImageIsAPatchedCopy}");
        _output.WriteLine($"corpus                  : {corpus.Count}, of which {comparable.Count} have a text form");
        _output.WriteLine("alignment               : READ from the pin's own timing, not searched for");
        _output.WriteLine(string.Empty);

        var worst = 0;
        var worstMessage = string.Empty;
        var worstSample = -1;
        var totalDiffering = 0L;
        var totalSamples = 0L;
        var identical = 0;
        var compared = 0;
        var toneMismatches = new List<string>();
        var packedMismatches = new List<string>();

        // Where in the transmission the differences fall. Task 4 asks for the SHAPE of any
        // difference and not only its size, because the shape is what says where a defect would be:
        // confined to the ends is the padding or the ramp; growing steadily through the signal is
        // an accumulated phase error, a symbol period or a sample rate; even throughout is the last
        // place of the arithmetic and nothing more. Measured in fifths of the signal.
        const int Fifths = 5;
        var byFifth = new long[Fifths];
        var perFifth = new long[Fifths];
        var inPadding = 0L;
        var paddingSamples = 0L;

        foreach (var entry in comparable)
        {
            var kept = Ft8Oracle.GenerateKeepingWav(entry.Text!);
            try
            {
                Assert.Equal(0, kept.Run.ExitCode);

                // Before a single sample is compared: are the two sides even encoding the same
                // message? Unit 211's most portable lesson — two corpus entries turned out to be
                // asking upstream a DIFFERENT QUESTION rather than getting a different answer, and
                // both were caught by checking the packed bytes and the tones first. A sample
                // comparison that skipped this would have reported that as a defect in the
                // synthesis.
                var symbols = Ft8SymbolEncoder.Encode(entry.Message);
                if (Ft8Oracle.TryReadHexAfterLabel(kept.Run.StandardOutput, "Packed data", out var packed)
                    && !packed.AsSpan().SequenceEqual(entry.Message.AsSpan(0, packed.Length)))
                {
                    packedMismatches.Add(entry.Label);
                    continue;
                }

                if (Ft8Oracle.TryReadTones(kept.Run.StandardOutput, Ft8Oracle.ToneSequenceLength, out var tones)
                    && !tones.AsSpan().SequenceEqual(symbols))
                {
                    toneMismatches.Add(entry.Label);
                    continue;
                }

                var wav = WavFile.Read(kept.WavPath);
                var ours = Ft8Waveform.SynthesizeSlotPcm16(symbols);

                var difference = Compare(ours, wav.Samples, entry.Label);

                // The distribution, taken on the same pass the comparison was.
                var lead = Ft8Waveform.PaddingSampleCount(Ft8Waveform.DefaultSampleRate);
                var signalLength = Ft8Waveform.SampleCount(Ft8Waveform.DefaultSampleRate);
                for (var i = 0; i < ours.Length; i++)
                {
                    var offset = i - lead;
                    if (offset < 0 || offset >= signalLength)
                    {
                        paddingSamples++;
                        if (ours[i] != wav.Samples[i])
                        {
                            inPadding++;
                        }

                        continue;
                    }

                    var fifth = Math.Min(Fifths - 1, offset * Fifths / signalLength);
                    perFifth[fifth]++;
                    if (ours[i] != wav.Samples[i])
                    {
                        byFifth[fifth]++;
                    }
                }

                compared++;
                totalDiffering += difference.Differing;
                totalSamples += difference.Total;
                if (difference.MaxAbsolute == 0)
                {
                    identical++;
                }

                if (difference.MaxAbsolute > worst)
                {
                    worst = difference.MaxAbsolute;
                    worstMessage = entry.Label;
                    worstSample = difference.AtSample;
                }
            }
            finally
            {
                // Per message, not at the end.
                WavFile.DeleteQuietly(kept.WavPath);
            }
        }

        // ---- Reported BEFORE anything is asserted. ----
        _output.WriteLine($"messages compared       : {compared}");
        _output.WriteLine($"messages identical in every sample : {identical}");
        _output.WriteLine($"MAXIMUM ABSOLUTE DIFFERENCE : {worst} counts");
        _output.WriteLine($"    at                  : {worstMessage}, sample {worstSample}");
        _output.WriteLine($"samples differing at all : {totalDiffering} of {totalSamples}");
        _output.WriteLine(
            $"    as a fraction       : {(totalSamples == 0 ? 0 : (double)totalDiffering / totalSamples):P6}");
        _output.WriteLine($"packed-data mismatches  : {packedMismatches.Count} "
            + $"{(packedMismatches.Count == 0 ? string.Empty : "[" + string.Join(", ", packedMismatches) + "]")}");
        _output.WriteLine($"tone mismatches         : {toneMismatches.Count} "
            + $"{(toneMismatches.Count == 0 ? string.Empty : "[" + string.Join(", ", toneMismatches) + "]")}");
        _output.WriteLine($"bound asserted          : {Bound} counts");
        _output.WriteLine(string.Empty);
        _output.WriteLine("the SHAPE of the difference, by fifth of the signal:");
        for (var f = 0; f < Fifths; f++)
        {
            _output.WriteLine(
                $"    fifth {f + 1}             : {byFifth[f]} of {perFifth[f]} differ "
                + $"({(perFifth[f] == 0 ? 0 : (double)byFifth[f] / perFifth[f]):P3})");
        }

        _output.WriteLine($"    in the silence      : {inPadding} of {paddingSamples} differ");

        // The silence is silence on both sides, so any difference there would be the padding or the
        // slot layout rather than the synthesis, and it would be a finding of its own.
        Assert.Equal(0, inPadding);

        Assert.Empty(packedMismatches);
        Assert.Empty(toneMismatches);
        Assert.Equal(comparable.Count, compared);

        Assert.True(
            worst <= Bound,
            $"the largest disagreement with upstream's own waveform is {worst} counts, at "
            + $"{worstMessage} sample {worstSample}, past the bound of {Bound}. That is a FINDING and "
            + "not a tolerance to widen: read the shape of the difference — confined to the ends is "
            + "the padding or the ramp; growing with time is the sample rate, the symbol period or "
            + "an accumulated phase error; steady inside every symbol is the pulse shape or the "
            + "smoothing parameter; at symbol boundaries only is phase continuity; and in one "
            + "message and not the others is the symbols rather than the synthesis.");
    }

    /// <summary>
    /// The four alterations the comparison must refuse. <b>A comparison that has never refused is
    /// not a comparison</b>, and each of these must land far outside the bound rather than merely
    /// outside it.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void TheComparisonIsWatchedRefusingEachOfItsFourNamedAlterations()
    {
        const string Message = "CQ K1ABC FN42";
        var packed = EncodeCorpus.Build().First(e => e.Text == Message).Message;
        var symbols = Ft8SymbolEncoder.Encode(packed);

        var kept = Ft8Oracle.GenerateKeepingWav(Message);
        short[] upstream;
        try
        {
            Assert.Equal(0, kept.Run.ExitCode);
            upstream = WavFile.Read(kept.WavPath).Samples;
        }
        finally
        {
            WavFile.DeleteQuietly(kept.WavPath);
        }

        // What agreement looks like, so each refusal below can be read against it.
        var faithful = Compare(Ft8Waveform.SynthesizeSlotPcm16(symbols), upstream, "unaltered");
        _output.WriteLine($"unaltered               : max {faithful.MaxAbsolute} counts, "
            + $"{faithful.Differing} of {faithful.Total} samples differ");
        _output.WriteLine(string.Empty);

        // 1 — one symbol altered.
        var altered = (byte[])symbols.Clone();
        altered[40] = (byte)((altered[40] + 1) % Ft8Waveform.ToneCount);
        Refuse("one symbol altered, at position 40",
            Compare(Ft8Waveform.SynthesizeSlotPcm16(altered), upstream, "one symbol"));

        // 2 — the base frequency moved by exactly one tone spacing.
        Refuse($"the base frequency moved by one tone spacing ({Ft8Waveform.ToneSpacingHz} Hz)",
            Compare(
                Ft8Waveform.SynthesizeSlotPcm16(
                    symbols,
                    baseFrequency: Ft8Waveform.DefaultBaseFrequency + Ft8Waveform.ToneSpacingHz),
                upstream,
                "one tone spacing"));

        // 3 — the smoothing parameter changed. The library holds it fixed at the modulation's own
        // value, so the altered waveform comes from the second opinion, which takes it.
        Refuse("the smoothing parameter halved",
            Compare(
                Ft8WaveformSecondOpinion.SynthesizeSlotPcm16(
                    symbols,
                    smoothing: Ft8WaveformSecondOpinion.Smoothing / 2),
                upstream,
                "smoothing halved"));

        // 4 — the sample rate changed. A different rate is a different number of samples, so the
        // comparison must refuse it on the length rather than on a count, and that refusal is the
        // one that would otherwise be silently skipped.
        var atAnotherRate = Ft8Waveform.SynthesizeSlotPcm16(symbols, sampleRate: 48000);
        _output.WriteLine(
            $"the sample rate changed to 48000: {atAnotherRate.Length} samples against upstream's "
            + $"{upstream.Length} — refused on the length");
        var lengthRefusal = Assert.Throws<InvalidOperationException>(
            () => Compare(atAnotherRate, upstream, "another rate"));
        Assert.Contains(atAnotherRate.Length.ToString(), lengthRefusal.Message);
        Assert.Contains(upstream.Length.ToString(), lengthRefusal.Message);
        Assert.Contains("prefix", lengthRefusal.Message);
    }

    private void Refuse(string what, Difference difference)
    {
        _output.WriteLine(
            $"{what}: max {difference.MaxAbsolute} counts, {difference.Differing} of "
            + $"{difference.Total} samples differ — {(difference.MaxAbsolute > Bound * 100 ? "REFUSED" : "NOT REFUSED")}");

        // Far outside, not merely outside. A hundred times the bound is still a hundredth of the
        // full scale, and anything this comparison could not tell apart at that distance would not
        // be telling anything apart.
        Assert.True(
            difference.MaxAbsolute > Bound * 100,
            $"{what} produced a maximum difference of only {difference.MaxAbsolute} counts against a "
            + $"bound of {Bound}. The comparison did not refuse it far enough outside the bound to "
            + "be a comparison at all.");
    }

    /// <summary>
    /// Holds two sixteen-bit signals against each other, reporting a position and a magnitude and
    /// never a verdict alone — the shape <see cref="SymbolComparison"/> established for the tones.
    /// </summary>
    /// <remarks>
    /// Different lengths are refused outright rather than compared over the shorter prefix, because
    /// agreement over a prefix reported as agreement is exactly the laundered pass this whole unit
    /// is arranged to prevent.
    /// </remarks>
    private static Difference Compare(short[] ours, short[] theirs, string what)
    {
        if (ours.Length != theirs.Length)
        {
            throw new InvalidOperationException(
                $"{what}: our waveform is {ours.Length} samples and upstream's is {theirs.Length}. "
                + "Two signals of different length are refused outright rather than compared over "
                + "the shorter of them, because agreement over a prefix reported as agreement is "
                + "exactly the laundered pass this comparison exists to prevent.");
        }

        var max = 0;
        var at = -1;
        var differing = 0;
        for (var i = 0; i < ours.Length; i++)
        {
            var delta = Math.Abs(ours[i] - theirs[i]);
            if (delta == 0)
            {
                continue;
            }

            differing++;
            if (delta > max)
            {
                max = delta;
                at = i;
            }
        }

        return new Difference(max, at, differing, ours.Length);
    }
}
