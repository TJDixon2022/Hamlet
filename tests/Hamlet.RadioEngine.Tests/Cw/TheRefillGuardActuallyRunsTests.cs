using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A stream reads nothing until its window has refilled, on the first fill as
/// well as after an emptying.
/// </summary>
/// <remarks>
/// <para>**THE GUARD WAS DOCUMENTED AT LENGTH AND HAD NEVER RUN.**
/// <c>_refillHops</c> was assigned in <c>Restart()</c> and nowhere else, and
/// <c>Restart()</c> is reachable only behind
/// <see cref="CwDecoder.ClearOnAStationChange"/>, which is <c>const false</c>.
/// A fresh stream therefore carried nought, the comparison it guards is never
/// true against nought, and every session's first fill was read from whatever
/// audio had arrived so far.</para>
/// <para>**THIS IS A DEFECT FIX AGAINST THE CODE'S OWN STATED INTENT, NOT A
/// BEHAVIOUR CHOICE.** <see cref="CwProbabilisticStream.RefillSeconds"/>'s
/// remarks already say why: on two seconds of audio the noise scale and the
/// signal amplitude rest on a handful of elements, so a short window reads
/// confidently and incorrectly rather than merely reading less. Less evidence
/// has to mean silence rather than guesses (HM-DEC-120).</para>
/// </remarks>
public sealed class TheRefillGuardActuallyRunsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the hop counts are printed.</param>
    public TheRefillGuardActuallyRunsTests(ITestOutputHelper output)
        => _output = output;

    private static MonoAudioSlice Signal(double seconds)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE W1AW W1AW K",
            WordsPerMinute: 18,
            ToneHz: 600,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            LeadInSeconds: 0,
            Seed: 4));

        var wanted = Math.Min(
            audio.Samples.Length, (int)(seconds * audio.SampleRate));

        return new MonoAudioSlice(audio.SampleRate, audio.Samples[..wanted]);
    }

    private sealed record MonoAudioSlice(int SampleRate, float[] Samples);

    /// <remarks>
    /// Proves the guard fires on the first fill, which is the fill it had never
    /// run on. Less than <see cref="CwProbabilisticStream.RefillSeconds"/> of
    /// audio in and nothing comes out, however much of a station is in it.
    /// </remarks>
    [Fact]
    public void AStreamFedLessThanARefillReadsNothingAtAll()
    {
        var audio = Signal(CwProbabilisticStream.RefillSeconds - 0.4);

        var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        var settled = new List<string>();
        var edges = new List<int>();

        stream.CharacterSettled += c => settled.Add(c.Text);
        stream.LeadingEdgeChanged += e => edges.Add(e.Count);

        stream.Process(audio.Samples);

        _output.WriteLine(
            $"{audio.Samples.Length / (double)audio.SampleRate:0.0} s in, "
            + $"{stream.EnvelopeHops} hops held, "
            + $"'{stream.Last.Text}' read, {settled.Count} settled");

        Assert.Equal("", stream.Last.Text);
        Assert.Empty(settled);
        Assert.Empty(edges);
    }

    /// <remarks>
    /// Proves the no-op half, which is what makes this a guard and not a mute:
    /// past the refill the stream behaves exactly as it always has and reads the
    /// message.
    /// </remarks>
    [Fact]
    public void AStreamFedMoreThanARefillReadsAsItAlwaysDid()
    {
        var audio = Signal(30);

        var stream = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        var settled = new List<string>();

        stream.CharacterSettled += c => settled.Add(c.Text);

        stream.Process(audio.Samples);
        stream.Flush();

        var text = string.Concat(settled);

        _output.WriteLine(
            $"{audio.Samples.Length / (double)audio.SampleRate:0.0} s in: '{text}'");

        Assert.NotEmpty(text);
    }

    /// <remarks>
    /// Proves the constructor and <see cref="CwProbabilisticStream.Restart"/>
    /// now agree, which is the whole of the fix: the two paths into an empty
    /// window arrive at the same guard rather than one of them at nought.
    /// </remarks>
    [Fact]
    public void AFreshStreamAndAnEmptiedOneHoldBackForTheSameLength()
    {
        var audio = Signal(CwProbabilisticStream.RefillSeconds - 0.4);

        var fresh = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        fresh.Process(audio.Samples);

        var emptied = new CwProbabilisticStream(audio.SampleRate) { ToneHz = 600 };
        emptied.Process(Signal(30).Samples);
        emptied.Restart();
        emptied.Process(audio.Samples);

        _output.WriteLine($"fresh:   '{fresh.Last.Text}'");
        _output.WriteLine($"emptied: '{emptied.Last.Text}'");

        Assert.Equal("", fresh.Last.Text);
        Assert.Equal("", emptied.Last.Text);
    }
}
