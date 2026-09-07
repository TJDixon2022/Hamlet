using System.Diagnostics;
using Ft8Sharp.Deep;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The scene, composed by Hamlet's encoder and read back once through Hamlet's
/// decoder.** What comes out is the corpus; what went in is
/// <see cref="Ft8BandScene"/>.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS TEST WOULD HAVE CAUGHT.** A corpus written from the
/// scene script instead of from the decoder. It would look identical, it would
/// make every ledger assertion downstream pass, and it would be evidence about a
/// text file rather than about anything Hamlet can do - which is precisely what
/// step 4's criterion 6 is asking not to be handed. This test regenerates the
/// audio, decodes it, and fails if the committed file is not what came back.</para>
/// <para>**NOTHING HERE OPENS AN AUDIO DEVICE AND NOTHING PLAYS.** Samples in
/// memory, text out. A stopwatch is read to report the wall clock and decides
/// nothing.</para>
/// <para>**NO .wav IS COMMITTED.** Units 254 and 256 both dropped one for the
/// same reason: a committed binary is a second copy of what a deterministic test
/// regenerates in seconds.</para>
/// </remarks>
public sealed class TheBandSceneIsWhatHamletsDecoderReadTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the generator.</summary>
    /// <param name="output">Where the scene's counts are printed.</param>
    public TheBandSceneIsWhatHamletsDecoderReadTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>The rate the composer and the decoder share.</summary>
    private const int Rate = Ft8Composer.DefaultSampleRate;

    /// <summary>The peak the summed slot is scaled to, so nothing clips.</summary>
    /// <remarks>
    /// Eight tenths of full scale. Three FT8 signals summed can exceed 1.0 where
    /// their tones happen to align, and a clipped slot is a slot with harmonics
    /// in it that nobody put there.
    /// </remarks>
    private const float Peak = 0.8f;

    /// <summary>
    /// **THE ONE.** The scene composes, sums, decodes, and matches the committed
    /// corpus line for line.
    /// </summary>
    [Fact]
    public void TheCommittedCorpusIsWhatTheDecoderReturnedForThisScene()
    {
        var clock = Stopwatch.StartNew();
        var read = DecodeTheScene(out var lost, out var perSlot);
        clock.Stop();

        var composed = Ft8BandScene.Signals.Count;

        _output.WriteLine($"slots                : {Ft8BandScene.SlotCount}");
        _output.WriteLine($"signals composed     : {composed}");
        _output.WriteLine($"decodes returned     : {read.Count}");
        _output.WriteLine($"composed but not read: {lost.Count}");
        _output.WriteLine($"busiest slot         : {perSlot.Max()} signals "
            + $"(cap {Ft8BandScene.SignalCap})");
        _output.WriteLine($"wall clock           : {clock.Elapsed.TotalSeconds:F2} s");
        _output.WriteLine(string.Empty);

        foreach (var line in read)
        {
            _output.WriteLine($"  {line.Slot,2} | {line.FrequencyHz,5} | {line.Message}");
        }

        if (lost.Count > 0)
        {
            _output.WriteLine(string.Empty);
            _output.WriteLine("COMPOSED BUT NOT READ BACK - every one named:");
            foreach (var line in lost)
            {
                _output.WriteLine("  " + line);
            }
        }

        // The caps, which are must-pass and not suggestions.
        Assert.True(
            Ft8BandScene.SlotCount <= 12,
            $"the scene is {Ft8BandScene.SlotCount} slots and the cap is 12");

        Assert.True(
            perSlot.Max() <= Ft8BandScene.SignalCap,
            $"slot {Array.IndexOf(perSlot, perSlot.Max())} holds {perSlot.Max()} "
            + $"signals and the cap is {Ft8BandScene.SignalCap}");

        var text = Ft8SceneCorpus.Write(
            Ft8BandScene.OperatorCallsign, Ft8BandScene.SlotZeroUtc, read);

        var path = Ft8SceneCorpus.PathInTree;

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);

            Assert.Fail(
                "there was no committed scene corpus, so this run wrote one to "
                + Ft8SceneCorpus.RelativePath + ". Read it, commit it, and run "
                + "this again - from here on it is verified rather than written.");
        }

        var committed = File.ReadAllText(path)
            .Replace("\r\n", "\n", StringComparison.Ordinal);

        Assert.Equal(text, committed);
    }

    /// <summary>
    /// **The three-at-once station survived the sum.** Criterion 5's evidence is
    /// worthless if his transmissions were the ones that did not decode.
    /// </summary>
    /// <remarks>
    /// Asserted separately and by name, because the instruction's rule is that if
    /// the three-at-once station is one of the losses the slot is regenerated with
    /// more frequency separation - never that the missing line is written in.
    /// </remarks>
    [Fact]
    public void EveryTransmissionOfTheThreeAtOnceStationIsInTheCorpus()
    {
        var corpus = Ft8SceneCorpus.Read(Ft8SceneCorpus.PathInTree);

        var expected = Ft8BandScene.Signals
            .Where(signal => signal.Text.Contains("G4XYZ", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(expected);

        foreach (var signal in expected)
        {
            var found = corpus.Lines.Any(
                line => line.Slot == signal.Slot
                    && string.Equals(line.Message, signal.Text, StringComparison.Ordinal));

            _output.WriteLine(
                $"slot {signal.Slot,2}  {(found ? "in the corpus" : "MISSING     ")}  "
                + signal.Text);

            Assert.True(
                found,
                $"\"{signal.Text}\" was composed into slot {signal.Slot} and is not in "
                + "the corpus, so the station this scene exists for lost a "
                + "transmission. Regenerate that slot with more frequency "
                + "separation. Do not write the line in.");
        }
    }

    /// <summary>Composes the whole scene and decodes each slot exactly once.</summary>
    /// <param name="lost">Filled with every composed message that did not come back.</param>
    /// <param name="signalsPerSlot">Filled with how many signals each slot holds.</param>
    /// <returns>The decodes, in slot order and then in the decoder's own order.</returns>
    private static IReadOnlyList<Ft8SceneLine> DecodeTheScene(
        out IReadOnlyList<string> lost, out int[] signalsPerSlot)
    {
        // **THE DECODER HAMLET ACTUALLY RUNS** (Tim's ruling, 2026-09-05), built
        // exactly as Ft8Reader builds it - Deep with fine sync and ordered
        // statistics both on. A corpus read back by a decoder the application
        // does not use would be evidence about a decoder nobody uses.
        var decoder = new Ft8DeepSlotDecoder(
            osd: Ft8DeepOsdSettings.Default,
            fineSync: Ft8DeepFineSyncSettings.Default);

        var read = new List<Ft8SceneLine>();
        var missing = new List<string>();
        var counts = new int[Ft8BandScene.SlotCount];

        for (var slot = 0; slot < Ft8BandScene.SlotCount; slot++)
        {
            var here = Ft8BandScene.Signals.Where(s => s.Slot == slot).ToList();
            counts[slot] = here.Count;

            if (here.Count == 0)
            {
                continue;
            }

            var summed = Sum(here);
            var result = decoder.Decode(summed);

            var texts = new List<string>();

            foreach (var message in result.Messages)
            {
                var hz = (int)Math.Round(
                    message.FrequencyHz(decoder.Geometry), MidpointRounding.AwayFromZero);

                read.Add(new Ft8SceneLine(slot, hz, message.Text));
                texts.Add(message.Text);
            }

            foreach (var signal in here)
            {
                if (!texts.Contains(signal.Text, StringComparer.Ordinal))
                {
                    missing.Add(
                        $"slot {signal.Slot}, {signal.BaseFrequencyHz:F0} Hz: "
                        + $"\"{signal.Text}\"");
                }
            }
        }

        lost = missing;
        signalsPerSlot = counts;

        return read;
    }

    /// <summary>Composes one slot's signals and sums them without clipping.</summary>
    /// <param name="here">The signals in this slot.</param>
    /// <returns>One slot of audio at <see cref="Rate"/>.</returns>
    /// <exception cref="InvalidOperationException">A message would not compose.</exception>
    /// <remarks>
    /// **SCALED AFTER THE SUM AND NEVER BEFORE IT.** Dividing each signal by the
    /// count first would make a slot of three stations quieter than a slot of one
    /// for no reason on the air; scaling the sum to a fixed peak keeps every slot
    /// at the same level and keeps the relative strengths inside it as composed.
    /// </remarks>
    private static float[] Sum(IReadOnlyList<Ft8SceneSignal> here)
    {
        float[]? summed = null;

        foreach (var signal in here)
        {
            var composed = Ft8Composer.Compose(signal.Text, Rate, signal.BaseFrequencyHz);

            if (!composed.Composed)
            {
                throw new InvalidOperationException(
                    $"\"{signal.Text}\" would not compose: {composed.Refusal}. "
                    + composed.Explanation);
            }

            var samples = composed.Transmission!.Samples;

            summed ??= new float[samples.Length];

            for (var i = 0; i < summed.Length; i++)
            {
                summed[i] += samples[i];
            }
        }

        var loudest = 0.0f;

        foreach (var sample in summed!)
        {
            loudest = Math.Max(loudest, Math.Abs(sample));
        }

        if (loudest > 0.0f)
        {
            var scale = Peak / loudest;

            for (var i = 0; i < summed.Length; i++)
            {
                summed[i] *= scale;
            }
        }

        return summed;
    }
}
