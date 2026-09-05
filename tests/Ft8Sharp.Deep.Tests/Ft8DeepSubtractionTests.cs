using System;
using System.Linq;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The subtractor, and the two things it must not do: leave the message it removed still
/// decodable, and change what the decoder does when it is off.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE WATCHED FAILURE IS <see cref="ASubtractedMessageNoLongerDecodesOutOfTheResidual"/> AND IT
/// WAS WATCHED.</b> Unit 253 ran it once with the fit stubbed to unit gain and zero phase at the
/// coarse place - the shape of the bug the whole task is about - and the red is quoted verbatim in
/// that unit's report and in <c>docs/breakage-record.md</c> B16. It is the whole reason the fit
/// solves for two coefficients rather than one.
/// </para>
/// <para>
/// <b>NOTHING HERE ASSERTS A DECIBEL THRESHOLD.</b> The energy removed is printed as a measurement
/// beside the assertion and is never compared against a bound: a number picked on the night the fit
/// was written would be a target written after the work started, which this phase's rulings forbid.
/// What is asserted is the <em>consequence</em> - the message no longer comes back - which is the
/// property a reader actually cares about and which no threshold can stand in for.
/// </para>
/// </remarks>
public class Ft8DeepSubtractionTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>A whole slot carrying one free-text transmission and nothing else.</summary>
    private static (float[] Slot, string Text) CleanSlot(string text, double baseFrequencyHz = 1000.0)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText(text, message));

        var symbols = Ft8SymbolEncoder.Encode(message);
        var slot = new float[Ft8Waveform.SlotSampleCount(Rate)];
        var signal = Ft8Waveform.Synthesize(symbols, Rate, (float)baseFrequencyHz);
        var offset = Ft8Waveform.PaddingSampleCount(Rate);

        for (var i = 0; i < signal.Length; i++)
        {
            slot[offset + i] += signal[i];
        }

        return (slot, Ft8MessageDecoder.Decode(message).Text);
    }

    /// <summary>
    /// <b>THE WATCHED FAILURE. On a synthesised slot carrying one known transmission and no noise,
    /// after subtracting that message at its measured place the slot no longer decodes it.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>No noise on purpose.</b> On clean audio, anything left in the residual is the fit's own
    /// fault and nothing else's - there is no draw to blame and no second station to hide behind.
    /// This is also why the hazard in <c>docs/unit253-subtraction.md</c> §3.2 is checked here rather
    /// than on the ladder: a half-cancelled GFSK waveform is structured, correlates with the Costas
    /// arrays, and produces candidates, and a fit that leaves one has invented signal.
    /// </para>
    /// <para>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT, and it is the one the fit was built against.</b> A
    /// subtraction that fits a single real gain removes <c>cos^2(theta)</c> of a transmission's
    /// energy for an arrival phase <c>theta</c> - one half on average and <b>nothing at all when the
    /// signal arrives in quadrature</b>, with the reported gain reading zero while the whole
    /// transmission remains in the buffer. A multi-pass decoder built on that would return the same
    /// message on every pass, count each one as a duplicate, report *n* passes run and *n-1*
    /// messages subtracted, and be indistinguishable in every count it publishes from one that
    /// works. <b>The only thing that separates the two is whether the message still decodes out of
    /// the residual</b>, which is what this asserts.
    /// </para>
    /// </remarks>
    [Fact]
    public void ASubtractedMessageNoLongerDecodesOutOfTheResidual()
    {
        var (slot, sent) = CleanSlot("HAMLET 253");
        var decoder = new Ft8DeepSlotDecoder();

        var before = decoder.Decode(slot);
        Assert.Contains(sent, before.Texts);
        output.WriteLine($"the slot decodes \"{sent}\" before anything is subtracted.");

        var message = before.Messages.Single(m => string.Equals(m.Text, sent, StringComparison.Ordinal));
        var symbols = Ft8DeepMessageSymbols.TryEncode(message.Result.Message);
        Assert.NotNull(symbols);

        // THE PLACE, IN THREE PARTS. The candidate's own cell; unit 248's bias, which is exactly
        // minus one symbol period; and the estimator's coordinate search, which moves the window
        // onto the signal rather than onto the analysis cell.
        var geometry = decoder.Geometry;
        var frequency = message.FrequencyHz(geometry);
        var start = message.TimeSeconds(geometry) + Ft8DeepSlotDecoder.CandidateTimeBiasSeconds;

        var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency, null);
        var estimate = Ft8DeepSignalToNoise.Estimate(baseband, start, 0.0, symbols!, refine: true);
        Assert.True(estimate.IsMeasured);

        start += estimate.TimeAdjustmentSeconds;
        frequency += estimate.FrequencyAdjustmentHz;

        output.WriteLine(
            $"candidate cell {message.FrequencyHz(geometry):F4} Hz at "
            + $"{message.TimeSeconds(geometry) + Ft8DeepSlotDecoder.CandidateTimeBiasSeconds:F5} s; "
            + $"estimator moved it {estimate.TimeAdjustmentSeconds * 1000.0:F3} ms and "
            + $"{estimate.FrequencyAdjustmentHz:F3} Hz to {frequency:F4} Hz at {start:F5} s.");

        var residual = (float[])slot.Clone();
        var fit = Ft8DeepMessageSubtractor.Subtract(residual, Rate, symbols!, frequency, start);

        Assert.True(fit.IsFitted, "the fit refused a whole frame in a whole slot with no noise in it.");
        output.WriteLine($"fit: {fit}");

        // THE MEASUREMENT, BESIDE THE ASSERTION AND NEVER IN PLACE OF IT. Reported, not gated.
        output.WriteLine(
            $"ENERGY REMOVED: {fit.DecibelsRemoved:F2} dB over the {fit.Symbols} symbols of the "
            + "frame that lay inside the slot. THIS IS A MEASUREMENT AND NOT A THRESHOLD - nothing "
            + "in this library compares it against a bound.");

        var after = new Ft8DeepSlotDecoder().Decode(residual);

        output.WriteLine(
            $"residual: {after.CandidateCount} candidates, {after.ParitySatisfiedCount} past parity, "
            + $"{after.ChecksumPassedCount} past the checksum, {after.Messages.Count} messages.");

        foreach (var text in after.Texts)
        {
            output.WriteLine($"  residual returned \"{text}\"");
        }

        Assert.DoesNotContain(sent, after.Texts);
    }

    /// <summary>
    /// <b>The same thing where the transmission is on neither grid: 0.475 Hz off the waterfall's
    /// frequency step and 743 samples off its time step.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>WHY THIS EXISTS BESIDE THE ONE ABOVE, AND IT IS AN HONEST LIMITATION OF THE
    /// INSTRUMENT.</b> <c>Ft8Waveform.Synthesize</c> is deterministic and starts its carrier at zero
    /// phase, so a fixture that places its output at an integer sample offset and fits a reference
    /// built by the same call at the same offset has an arrival phase of <b>exactly zero</b>. On
    /// that fixture a single real gain would have sufficed, and no test built that way can show the
    /// quadrature coefficient earning its keep. <b>Real air has an arbitrary carrier phase and this
    /// instrument does not</b>, and saying so is better than a test that pretends otherwise.
    /// </para>
    /// <para>
    /// <b>What the quadrature coefficient does earn on this instrument is the search.</b> The score
    /// the time and frequency searches maximise is <c>|C|^2</c>, the squared magnitude of a complex
    /// correlation, and it is <em>phase-invariant only because it has both basis vectors in it</em>.
    /// A real-only score oscillates through a full cycle every twelve samples at 1000 Hz, so a
    /// search over it would find a place at random. This test puts the transmission where the search
    /// has to work: off both grids, so the coarse candidate is up to 1.5625 Hz and a twentieth of a
    /// second wrong, and the estimator's own residual - up to 0.20 Hz, which is 2.53 cycles of phase
    /// over the frame - is left for the fit to remove.
    /// </para>
    /// <para>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A subtractor tested only on a transmission
    /// sitting exactly on the analysis grid at an exact multiple of the carrier period - which is
    /// what every convenient fixture in this repository does - passing, shipping, and removing
    /// nothing at all from a real capture, where no station is on anybody's grid. The symptom would
    /// be a second pass that returns exactly what the first pass returned, reported as *subtraction
    /// bought nothing* rather than as *the fit never found the signal*.
    /// </para>
    /// <para>
    /// <b>AND THIS ONE ADDS NOISE WHERE THE TEST ABOVE HAS NONE, WHICH IS NOT A WEAKENING.</b> Unit
    /// 253 found, by running this test without it, that <b>a residual with nothing else in the slot
    /// decodes at any cancellation depth whatever</b>: 42.82 dB removed and the message still came
    /// back. That is not a defect in the fit. <c>Ft8SoftSymbols.Normalise</c> normalises a
    /// candidate's ratios, so the decoder is <b>scale-invariant</b> - a clean transmission at one
    /// per cent of its original amplitude is still a clean transmission, and in a slot holding
    /// nothing else there is nothing for it to be small against. Only the exactly-on-grid case
    /// escapes, and it escapes for a reason that is an artefact of the fixture rather than a
    /// property of the fit: the reference is bit-identical to what was placed, so the cancellation
    /// runs into float precision at 286 dB and the residue is arithmetic noise with no shape in it.
    /// </para>
    /// <para>
    /// <b>THE CONSEQUENCE, AND IT IS THE REASON THE DECIBELS ARE NOT A GATE.</b> "How much energy
    /// was removed" does not answer "is the message gone". What answers it is whether the residue
    /// is below whatever else is in the slot, and on the air there is always something else. A unit
    /// that had turned the decibels removed into a threshold would have been asserting a quantity
    /// that does not decide the question.
    /// </para>
    /// </remarks>
    [Fact]
    public void AnOffGridTransmissionIsFoundAndRemovedByTheFitsOwnSearch()
    {
        // 1233.90 Hz: the waterfall's frequency step is 3.125 Hz and the nearest grid points are
        // 1231.250 and 1234.375, so this is 0.475 Hz from the closer one and on no bin centre.
        const double offGridHz = 1233.90;

        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText("OFF GRID 253", message));
        var symbols = Ft8SymbolEncoder.Encode(message);
        var sent = Ft8MessageDecoder.Decode(message).Text;

        var slot = new float[Ft8Waveform.SlotSampleCount(Rate)];
        var signal = Ft8Waveform.Synthesize(symbols, Rate, (float)offGridHz);

        // 743 samples is not a multiple of the 960-sample sub-block, of the 1920-sample symbol, or
        // of the 12-sample carrier period at 1000 Hz.
        var offset = Ft8Waveform.PaddingSampleCount(Rate) + 743;
        for (var i = 0; i < signal.Length; i++)
        {
            slot[offset + i] += signal[i];
        }

        // Deterministic noise, so there is something for the residue to be small against. See the
        // remarks: without it a residual 42.82 dB down still decodes, because the decoder
        // normalises and a clean transmission at one per cent is still a clean transmission.
        var random = new Random(253);
        for (var i = 0; i < slot.Length; i++)
        {
            slot[i] += (float)((random.NextDouble() - 0.5) * 1.2);
        }

        var decoder = new Ft8DeepSlotDecoder();
        var before = decoder.Decode(slot);
        Assert.Contains(sent, before.Texts);

        var found = before.Messages.Single(m => string.Equals(m.Text, sent, StringComparison.Ordinal));
        var recovered = Ft8DeepMessageSymbols.TryEncode(found.Result.Message);
        Assert.NotNull(recovered);

        var geometry = decoder.Geometry;
        var frequency = found.FrequencyHz(geometry);
        var start = found.TimeSeconds(geometry) + Ft8DeepSlotDecoder.CandidateTimeBiasSeconds;

        output.WriteLine(
            $"truth      {offGridHz:F4} Hz at sample {offset} ({offset / (double)Rate:F5} s)");
        output.WriteLine(
            $"candidate  {frequency:F4} Hz at {start:F5} s - out by {frequency - offGridHz:F4} Hz "
            + $"and {(start * Rate) - offset:F1} samples");

        var baseband = Ft8DeepBaseband.Build(slot, Rate, frequency, null);
        var estimate = Ft8DeepSignalToNoise.Estimate(baseband, start, 0.0, recovered!, refine: true);
        Assert.True(estimate.IsMeasured);
        start += estimate.TimeAdjustmentSeconds;
        frequency += estimate.FrequencyAdjustmentHz;

        output.WriteLine(
            $"estimator  {frequency:F4} Hz at {start:F5} s - out by {frequency - offGridHz:F4} Hz "
            + $"and {(start * Rate) - offset:F1} samples");

        var residual = (float[])slot.Clone();
        var fit = Ft8DeepMessageSubtractor.Subtract(residual, Rate, recovered!, frequency, start);

        Assert.True(fit.IsFitted);
        output.WriteLine($"fit        {fit}");
        output.WriteLine(
            $"           settled {fit.BaseFrequencyHz - offGridHz:F4} Hz and "
            + $"{(fit.StartSeconds * Rate) - offset:F1} samples from the truth");
        output.WriteLine(
            $"ENERGY REMOVED: {fit.DecibelsRemoved:F2} dB. A MEASUREMENT, NOT A THRESHOLD.");

        var after = new Ft8DeepSlotDecoder().Decode(residual);
        output.WriteLine(
            $"residual: {after.CandidateCount} candidates, {after.ParitySatisfiedCount} past parity, "
            + $"{after.Messages.Count} messages");

        foreach (var text in after.Texts)
        {
            output.WriteLine($"  residual returned \"{text}\"");
        }

        Assert.DoesNotContain(sent, after.Texts);
    }

    /// <summary>
    /// <b>WITH SUBTRACTION OFF, THE DECODER RETURNS BIT-FOR-BIT WHAT IT RETURNED BEFORE THIS UNIT
    /// EXISTED.</b> Whole-result identity, all five counts and every message.
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> Every row of units 246, 248, 251 and 252 - the
    /// port's 13 of 306 at -21 dB, the sibling's 33, unit 252's 41, the 0.26 dB signal-to-noise
    /// agreement, the 330.4 ms worst slot - is a measurement of the default path. A unit that adds a
    /// stage and lets it run by default, or that restructures the entry point and changes the order
    /// something happens in, invalidates all of them at once, and <b>nothing else in the tree would
    /// say so</b>: the rates would still be rates, the counts would still be counts, and the next
    /// unit would compare its new number against an old number taken on a different instrument.
    /// That is <c>G1</c>'s shape in <c>docs/breakage-record.md</c> and this is the net for it.
    /// </remarks>
    [Fact]
    public void WithSubtractionOffTheWholeResultIsWhatItWasBefore()
    {
        var (slot, sent) = CleanSlot("HAMLET 253");
        var random = new Random(253);
        var noisy = new float[slot.Length];
        for (var i = 0; i < slot.Length; i++)
        {
            noisy[i] = slot[i] + (float)((random.NextDouble() - 0.5) * 1.2);
        }

        var plain = new Ft8DeepSlotDecoder();
        var explicitlyOff = new Ft8DeepSlotDecoder(subtraction: null);

        Assert.Null(plain.Subtraction);
        Assert.Null(explicitlyOff.Subtraction);

        var a = plain.Decode(noisy);
        var b = explicitlyOff.Decode(noisy);

        output.WriteLine(
            $"default:      {a.CandidateCount} cand, {a.ParitySatisfiedCount} par, "
            + $"{a.ChecksumPassedCount} crc, {a.BecameTextCount} txt, {a.DuplicateCount} dup, "
            + $"{a.Messages.Count} messages");
        output.WriteLine(
            $"subtraction null: {b.CandidateCount} cand, {b.ParitySatisfiedCount} par, "
            + $"{b.ChecksumPassedCount} crc, {b.BecameTextCount} txt, {b.DuplicateCount} dup, "
            + $"{b.Messages.Count} messages");
        output.WriteLine($"the message that was sent: \"{sent}\"; returned: {a.Texts.Contains(sent)}");

        Assert.Equal(a.CandidateCount, b.CandidateCount);
        Assert.Equal(a.ParitySatisfiedCount, b.ParitySatisfiedCount);
        Assert.Equal(a.ChecksumPassedCount, b.ChecksumPassedCount);
        Assert.Equal(a.BecameTextCount, b.BecameTextCount);
        Assert.Equal(a.DuplicateCount, b.DuplicateCount);
        Assert.Equal(a.Messages.Count, b.Messages.Count);

        for (var i = 0; i < a.Messages.Count; i++)
        {
            Assert.Equal(a.Messages[i].Text, b.Messages[i].Text, StringComparer.Ordinal);
            Assert.Equal(a.Messages[i].Candidate, b.Messages[i].Candidate);
            Assert.Equal(a.Messages[i].FrequencyHz(plain.Geometry), b.Messages[i].FrequencyHz(plain.Geometry));
            Assert.Equal(a.Messages[i].TimeSeconds(plain.Geometry), b.Messages[i].TimeSeconds(plain.Geometry));
        }

        // Off reports exactly one pass and no activity at all, so a report cannot read subtraction
        // into a row that never ran it.
        Assert.Equal(new Ft8DeepSubtractionCounts(1, 0, 0, 0, 0, 0, 0, double.NaN), plain.LastSubtraction);
        Assert.Empty(plain.LastFits);

        // And the waterfall entry point is untouched while it is off.
        var waterfall = new Ft8Monitor(plain.Geometry).Analyse(noisy);
        var fromWaterfall = plain.Decode(waterfall);
        Assert.Equal(a.CandidateCount, fromWaterfall.CandidateCount);
        Assert.Equal(a.Messages.Count, fromWaterfall.Messages.Count);
    }

    /// <summary>
    /// <b>A waterfall has no samples behind it, and a decoder configured to subtract says so rather
    /// than counting a skip.</b>
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> Unit 249 found fine synchronisation refusing 42
    /// of 42 candidates because it had been handed a waterfall, counting them honestly, and nobody
    /// reading the rates noticing for two units. Fine sync can afford that - it is a per-candidate
    /// rescue and the slot still decodes without it. <b>Subtraction cannot</b>: a caller that asked
    /// for four passes and silently got one has been told a decode ran that did not run, and the
    /// row it produces is indistinguishable from a row where subtraction genuinely bought nothing.
    /// </remarks>
    [Fact]
    public void AWaterfallWithNoSamplesBehindItIsRefusedLoudly()
    {
        var (slot, _) = CleanSlot("HAMLET 253");
        var subtracting = new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default);
        var waterfall = new Ft8Monitor(subtracting.Geometry).Analyse(slot);

        var refused = Assert.Throws<InvalidOperationException>(() => subtracting.Decode(waterfall));
        output.WriteLine(refused.Message);

        Assert.Contains("no samples behind it", refused.Message, StringComparison.Ordinal);
        Assert.Contains("Nothing has been decoded", refused.Message, StringComparison.Ordinal);

        // And the settings refuse rather than clamp, in the voice Ft8DeepOsdSettings already uses.
        var none = Assert.Throws<ArgumentOutOfRangeException>(() => new Ft8DeepSubtractionSettings(0));
        var many = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSubtractionSettings(Ft8DeepSubtractionSettings.MaximumPasses + 1));
        var backwards = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSubtractionSettings(frequencyStepHz: 0.0));

        output.WriteLine(none.Message);
        output.WriteLine(many.Message);
        output.WriteLine(backwards.Message);
    }

    /// <summary>
    /// <b>A reference waveform built to be subtracted is not a transmit path, and that is asserted
    /// rather than argued.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>CLAUDE.md</c> §0.2 governs keying a transmitter. <see cref="Ft8DeepMessageSubtractor"/>
    /// builds a <c>float[]</c>, subtracts it from a copy of a received slot and drops it: no device,
    /// no stream, no file. <c>Ft8DeepBoundaryTests.NoHamletAssemblyArrivesInEitherAssembly</c>
    /// already asserts that no Hamlet assembly - and therefore no audio device - is reachable from
    /// this library, and this asserts it again with the subtractor's own type named, so a reader who
    /// arrives at that type first finds the guarantee beside it.
    /// </para>
    /// <para>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A future unit moving the reference synthesis
    /// somewhere it could reach a sound device, on the reasoning that it already has audio in a
    /// buffer. The distance between "a waveform in memory" and "a waveform on the air" is one
    /// project reference.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheSubtractorsAssemblyCanReachNoAudioDeviceAtAll()
    {
        var assembly = typeof(Ft8DeepMessageSubtractor).Assembly;
        var referenced = assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToArray();

        output.WriteLine($"{assembly.GetName().Name} references: {string.Join(", ", referenced)}");

        Assert.DoesNotContain(referenced, n => n.StartsWith("Hamlet", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, n => n.Contains("NAudio", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, n => n.Contains("PortAudio", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, n => n.Contains("OpenAL", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referenced, n => n.Contains("Avalonia", StringComparison.OrdinalIgnoreCase));

        // The subtractor's whole public surface writes into a Span<float> the caller owns. There is
        // no overload that opens anything.
        var methods = typeof(Ft8DeepMessageSubtractor)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(m => m.DeclaringType == typeof(Ft8DeepMessageSubtractor))
            .Select(m => m.Name)
            .ToArray();

        output.WriteLine($"public static methods: {string.Join(", ", methods)}");
        Assert.Equal(new[] { "Subtract" }, methods);
    }

    /// <summary>
    /// <b>A message whose symbols cannot be recovered is not subtracted, and a frame that is mostly
    /// outside the slot is not subtracted on the strength of the part that arrived.</b>
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A subtractor that clamps instead of refusing
    /// writes a waveform fitted to twelve symbols across the whole seventy-nine, which is a signal
    /// nobody transmitted, into the buffer the next pass reads. Every candidate the next pass finds
    /// there is an artefact of this stage, and one of them satisfying parity and a checksum is a
    /// message nobody sent - the hazard <c>docs/unit253-subtraction.md</c> §3.2 names as this
    /// step's own.
    /// </remarks>
    [Fact]
    public void AFrameMostlyOutsideTheSlotIsRefusedRatherThanClamped()
    {
        var (slot, _) = CleanSlot("HAMLET 253");
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8FreeText.TryPackText("HAMLET 253", message));
        var symbols = Ft8SymbolEncoder.Encode(message);

        // A start so late that fewer than MinimumSymbols of the frame are inside the slot.
        var late = (slot.Length - (20 * Ft8Waveform.SamplesPerSymbol(Rate))) / (double)Rate;
        var residual = (float[])slot.Clone();
        var refused = Ft8DeepMessageSubtractor.Subtract(residual, Rate, symbols, 1000.0, late);

        output.WriteLine($"start {late:F3} s: {refused}");
        Assert.False(refused.IsFitted);
        Assert.Equal(slot, residual);

        // NaN and never a floor: a gain of zero would read as "the transmission was not there".
        Assert.True(double.IsNaN(refused.Gain));
        Assert.True(double.IsNaN(refused.DecibelsRemoved));

        var wrongLength = Assert.Throws<ArgumentException>(
            () => Ft8DeepMessageSubtractor.Subtract(residual, Rate, symbols.AsSpan(0, 40), 1000.0, 0.5));
        output.WriteLine(wrongLength.Message);
        Assert.Contains("nothing has been written to the slot", wrongLength.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <b>The multi-pass loop stops by its rule, de-duplicates across passes, and never writes the
    /// caller's buffer.</b>
    /// </summary>
    /// <remarks>
    /// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> A decode that subtracts in place would make every
    /// paired comparison in this project depend on which decoder ran first - the ladder hands one
    /// array to every column, so a subtracting column would silently hand the next column a residual
    /// and the row after it would be a measurement of nothing anybody can name. That is a defect
    /// with no symptom except numbers that do not reproduce.
    /// </remarks>
    [Fact]
    public void TheLoopStopsByItsRuleAndNeverWritesTheCallersBuffer()
    {
        var (slot, sent) = CleanSlot("HAMLET 253");
        var untouched = (float[])slot.Clone();

        var subtracting = new Ft8DeepSlotDecoder(subtraction: new Ft8DeepSubtractionSettings(maxPasses: 4));
        var result = subtracting.Decode(slot);
        var counts = subtracting.LastSubtraction;

        Assert.Equal(untouched, slot);

        output.WriteLine(
            $"passes run {counts.PassesRun}, offered {counts.MessagesOffered}, subtracted "
            + $"{counts.MessagesSubtracted}, refused for symbols {counts.RefusedForWantOfSymbols}, "
            + $"refused for frame {counts.RefusedForWantOfFrame}, duplicates across passes "
            + $"{counts.DuplicatesAcrossPasses}, from later passes {counts.MessagesFromLaterPasses}");

        foreach (var fit in subtracting.LastFits)
        {
            output.WriteLine($"  {fit}");
        }

        foreach (var text in result.Texts)
        {
            output.WriteLine($"  returned \"{text}\"");
        }

        Assert.Contains(sent, result.Texts);

        // ONE MESSAGE IN THE SLOT, SO THE LOOP STOPS ON RULE 2 AFTER THE SECOND PASS RETURNS
        // NOTHING NEW - not on rule 1, which would have run all four.
        Assert.True(
            counts.PassesRun < 4,
            $"the loop ran all {counts.PassesRun} passes over a slot with one message in it, so "
            + "stopping rule 2 - the pass returned nothing new - did not fire.");

        Assert.Equal(1, counts.MessagesSubtracted);
        Assert.Equal(0, counts.RefusedForWantOfSymbols);

        // No message came back twice as a new message: the message the first pass returned appears
        // exactly once in the result whatever the later passes made of the remnant.
        Assert.Single(result.Texts, t => string.Equals(t, sent, StringComparison.Ordinal));
    }
}
