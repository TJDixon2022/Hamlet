using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A speck of energy too short to be an element does not corrupt every duration
/// that follows it.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE BENCH'S OWN BUG, ASKED OF HAMLET** (work instruction 050,
/// task 2). `tools/cwbench/cwbench.py` first dropped runs shorter than a minimum
/// **out of a run-length list**, without merging the neighbours those runs had
/// been separating. Dropping a ten millisecond blip from between two gaps leaves
/// two adjacent gap entries counted as two gaps, so **every duration after it is
/// wrong** — the symptom being consecutive same-state runs, and the decode
/// unreadable until it was fixed.</para>
/// <para>**HAMLET CANNOT HAVE THAT BUG, AND THE REASON IS STRUCTURAL RATHER THAN
/// CAREFUL.** The shipping decoder never builds a run-length list to filter.
/// `CwProbabilisticDecoder` is a semi-Markov lattice over five-millisecond hops:
/// a segment is a *span* the path chooses, `SpanBounds` refuses to propose one
/// shorter than `ShortestShare` of its expected length, and a span that is not
/// proposed is not a run that was dropped — the hops it would have covered are
/// still there and the neighbouring spans lengthen to cover them. **The merge is
/// the only thing that can happen**, because the spans of any path through the
/// lattice tile the whole hop axis by construction.</para>
/// <para>The other filter in the tree, `CwReferenceDecoder.Deglitch`, is safe for
/// a different reason: it rewrites the boolean key-state array rather than
/// filtering a list, so flipping a speck to its neighbours' value joins them in
/// the array itself. It also ships off (`TheReferenceDecoderStaysOffTests`).</para>
/// <para>**SO THIS ASSERTS THE PROPERTY RATHER THAN THE ABSENCE OF A LINE OF
/// CODE.** A test that grepped for a filter would go green the day somebody
/// added a safe one and red the day somebody added a safe one differently.</para>
/// </remarks>
public sealed class ABlipDoesNotShiftEverythingAfterItTests
{
    private readonly ITestOutputHelper _output;

    public ABlipDoesNotShiftEverythingAfterItTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The exact case that broke the bench: a sub-minimum blip inside a gap.
    /// </summary>
    /// <remarks>
    /// Ten milliseconds of tone is under a fifth of a dit at eighteen words a
    /// minute, dropped into the word gap. If the durations after it shifted, the
    /// text after it would change; the assertion is that it does not.
    /// </remarks>
    [Fact]
    public void ASubMinimumBlipInAGapChangesNothingAfterIt()
    {
        const string Message = "CQ DE W1AW K";

        var clean = CwSignal.Generate(new CwSignalRequest(
            Message, WordsPerMinute: 18, NoiseAmplitude: 0.02));

        var blipped = WithBlip(clean, atSeconds: SecondsOfFirstWordGap(clean));

        var before = Read(clean);
        var after = Read(blipped);

        _output.WriteLine($"  clean:   {before}");
        _output.WriteLine($"  blipped: {after}");

        Assert.Equal(before, after);
    }

    /// <summary>
    /// And three blips, so one surviving by luck cannot pass for the property.
    /// </summary>
    /// <remarks>
    /// The bench's failure compounded: each dropped run shifted everything after
    /// it, so three of them was three times as wrong. One blip that happens to
    /// land somewhere harmless proves nothing.
    /// </remarks>
    [Fact]
    public void ThreeBlipsChangeNothingEither()
    {
        const string Message = "CQ DE W1AW K";

        var clean = CwSignal.Generate(new CwSignalRequest(
            Message, WordsPerMinute: 18, NoiseAmplitude: 0.02));

        var start = SecondsOfFirstWordGap(clean);
        var blipped = clean;

        foreach (var at in new[] { start, start + 0.04, start + 0.08 })
        {
            blipped = WithBlip(blipped, at);
        }

        var before = Read(clean);
        var after = Read(blipped);

        _output.WriteLine($"  clean:      {before}");
        _output.WriteLine($"  three blips: {after}");

        Assert.Equal(before, after);
    }

    /// <summary>
    /// The control: an injection long enough to be an element does change the
    /// reading.
    /// </summary>
    /// <remarks>
    /// **WITHOUT THIS THE TWO TESTS ABOVE PROVE NOTHING** (§12.5). They would
    /// pass just as well if `WithBlip` wrote nothing, if the decoder ignored the
    /// whole region, or if `Read` returned the same string every time. A dit's
    /// worth of tone in the middle of a word gap is a real element and the reader
    /// must notice it; that it does is what makes the ten millisecond speck's
    /// being ignored a measurement rather than a coincidence.
    /// </remarks>
    [Fact]
    public void ADitLongInjectionIsNoticed()
    {
        const string Message = "CQ DE W1AW K";

        var clean = CwSignal.Generate(new CwSignalRequest(
            Message, WordsPerMinute: 18, NoiseAmplitude: 0.02));

        // Sixty-six milliseconds is a dit at eighteen words a minute.
        var loud = WithTone(clean, SecondsOfFirstWordGap(clean), 0.066);

        var before = Read(clean);
        var after = Read(loud);

        _output.WriteLine($"  clean: {before}");
        _output.WriteLine($"  dit:   {after}");

        Assert.NotEqual(before, after);
    }

    /// <summary>Where the first word gap falls, in seconds.</summary>
    /// <remarks>
    /// Taken from the message's own timing rather than measured, so the blip
    /// lands inside a gap by construction and the test is not sensitive to what
    /// the decoder made of the audio.
    /// </remarks>
    private static double SecondsOfFirstWordGap(MonoAudio audio)
        => audio.Samples.Length / (double)audio.SampleRate * 0.35;

    /// <summary>The same audio with ten milliseconds of tone dropped into it.</summary>
    private static MonoAudio WithBlip(MonoAudio audio, double atSeconds)
        => WithTone(audio, atSeconds, 0.010);

    /// <summary>The same audio with a burst of tone dropped into it.</summary>
    private static MonoAudio WithTone(
        MonoAudio audio, double atSeconds, double lengthSeconds)
    {
        var samples = (float[])audio.Samples.Clone();
        var from = (int)(atSeconds * audio.SampleRate);
        var count = (int)(lengthSeconds * audio.SampleRate);

        for (var i = from; i < Math.Min(from + count, samples.Length); i++)
        {
            samples[i] += (float)(0.5 * Math.Sin(
                2 * Math.PI * CwSignal.DefaultToneHz * i / audio.SampleRate));
        }

        return new MonoAudio(audio.SampleRate, samples);
    }

    /// <summary>What the shipping decoder reads out of it.</summary>
    /// <remarks>
    /// The same shape the corpus scorer uses: pump the whole buffer through the
    /// streaming decoder and take its settled reading.
    /// </remarks>
    private static string Read(MonoAudio audio)
    {
        var decoder = new CwDecoder(audio.SampleRate, 600);

        using var source = new BufferedAudioSource(audio);

        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return decoder.Reading.Text ?? "";
    }
}
