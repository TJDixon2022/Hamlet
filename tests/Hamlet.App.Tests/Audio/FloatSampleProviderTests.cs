using Hamlet.App.Audio;
using Hamlet.RadioEngine.Training;
using NAudio.Wave;
using Xunit;

namespace Hamlet.App.Tests.Audio;

/// <summary>
/// The playback path, end to end and without a sound device: a generated
/// buffer read back out through the thing that plays it.
/// </summary>
/// <remarks>
/// <para>THE GAP THIS CLOSES. The engine already proved that
/// <see cref="ModeAudio"/> produces buffers of the right length that are not
/// silent. Nothing read one back through the sample provider, so every defect
/// between "the samples exist" and "NAudio received them" was outside the
/// suite entirely — which is exactly where a reported playback failure landed.
/// Length and loudness are properties of the buffer; delivery is a property of
/// the path, and it needs its own test.</para>
/// <para>Every mode and every CW speed the field guide offers, because the
/// buttons differ per mode and a defect that only bit one of them would
/// otherwise be found by the operator rather than here.</para>
/// </remarks>
public sealed class FloatSampleProviderTests
{
    /// <summary>Every sample the field guide can play.</summary>
    public static TheoryData<TrainingMode, int, bool> EverySample()
    {
        var data = new TheoryData<TrainingMode, int, bool>();

        // The three CW speeds behind the copy-speed ladder.
        foreach (var wpm in new[] { 12, 18, 25 })
        {
            data.Add(TrainingMode.Cw, wpm, false);
        }

        data.Add(TrainingMode.Ft8, 18, false);
        data.Add(TrainingMode.Rtty, 18, false);
        data.Add(TrainingMode.Psk31, 18, false);
        data.Add(TrainingMode.Ssb, 18, false);
        data.Add(TrainingMode.Ssb, 18, true);

        return data;
    }

    /// <remarks>
    /// Proves the whole buffer arrives, in order, and that the provider then
    /// reports end of stream. Read in an awkward chunk size that divides
    /// nothing evenly, because NAudio asks for whatever its latency setting
    /// implies rather than for round numbers.
    /// </remarks>
    [Theory]
    [MemberData(nameof(EverySample))]
    public void Reads_TheWholeBufferInOrder(TrainingMode mode, int wpm, bool mistuned)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode, wpm, mistuned));
        var provider = new FloatSampleProvider(samples, ModeAudio.SampleRate);

        var received = new List<float>(samples.Length);
        var buffer = new float[1237];
        int read;

        while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        {
            received.AddRange(buffer.AsSpan(0, read).ToArray());
        }

        Assert.Equal(samples.Length, received.Count);
        Assert.Equal(samples, received);

        // Exhausted, and it says so rather than looping or throwing.
        Assert.Equal(0, provider.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, provider.Remaining);
    }

    /// <remarks>
    /// Proves reads honour a non-zero offset and never write outside the
    /// window they were given. NAudio reads into the middle of a shared buffer,
    /// and a provider that ignored the offset would corrupt whatever else was
    /// in it.
    /// </remarks>
    [Theory]
    [MemberData(nameof(EverySample))]
    public void Reads_RespectOffsetAndCount(TrainingMode mode, int wpm, bool mistuned)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode, wpm, mistuned));
        var provider = new FloatSampleProvider(samples, ModeAudio.SampleRate);

        const int offset = 64;
        const int count = 512;
        var buffer = new float[offset + count + 64];
        Array.Fill(buffer, float.NaN);

        var read = provider.Read(buffer, offset, count);

        Assert.Equal(count, read);
        Assert.Equal(samples.AsSpan(0, count).ToArray(), buffer.AsSpan(offset, count).ToArray());

        // Nothing outside the window was touched.
        Assert.All(buffer[..offset], f => Assert.True(float.IsNaN(f)));
        Assert.All(buffer[(offset + count)..], f => Assert.True(float.IsNaN(f)));
    }

    /// <remarks>
    /// Proves the format handed to NAudio matches what was generated. A
    /// mismatch here plays the right samples at the wrong speed, which sounds
    /// like a bug in the synthesiser rather than in the plumbing.
    /// </remarks>
    [Theory]
    [MemberData(nameof(EverySample))]
    public void Declares_TheFormatItWasGeneratedAt(TrainingMode mode, int wpm, bool mistuned)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode, wpm, mistuned));
        var provider = new FloatSampleProvider(samples, ModeAudio.SampleRate);

        Assert.Equal(ModeAudio.SampleRate, provider.WaveFormat.SampleRate);
        Assert.Equal(1, provider.WaveFormat.Channels);
        Assert.Equal(WaveFormatEncoding.IeeeFloat, provider.WaveFormat.Encoding);
    }

    /// <remarks>
    /// Proves the samples reaching the provider really are a <c>float[]</c>.
    /// A playback failure was reported as the buffer being some other array
    /// type at run time; it is not, and this pins that down so the question
    /// does not have to be re-litigated from a stack trace.
    /// </remarks>
    [Theory]
    [MemberData(nameof(EverySample))]
    public void Generated_BuffersAreGenuineFloatArrays(
        TrainingMode mode, int wpm, bool mistuned)
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(mode, wpm, mistuned));

        Assert.IsType<float[]>(samples);
        Assert.Equal(typeof(float), samples.GetType().GetElementType());
        Assert.NotEmpty(samples);
    }

    /// <remarks>
    /// Proves a single-sample read works, which is the degenerate case NAudio
    /// can ask for as a stream drains.
    /// </remarks>
    [Fact]
    public void Reads_OneSampleAtATime()
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(TrainingMode.Rtty));
        var provider = new FloatSampleProvider(samples, ModeAudio.SampleRate);
        var buffer = new float[1];

        for (var i = 0; i < 2000; i++)
        {
            Assert.Equal(1, provider.Read(buffer, 0, 1));
            Assert.Equal(samples[i], buffer[0]);
        }
    }

    /// <remarks>
    /// Proves the tail read is short rather than over-reading. The last chunk
    /// of a sample almost never lines up with the buffer size.
    /// </remarks>
    [Fact]
    public void FinalRead_IsShortAndThenZero()
    {
        var samples = ModeAudio.Generate(new AudioSampleRequest(TrainingMode.Psk31));
        var provider = new FloatSampleProvider(samples, ModeAudio.SampleRate);
        var buffer = new float[1000];

        var whole = samples.Length / buffer.Length;
        for (var i = 0; i < whole; i++)
        {
            Assert.Equal(buffer.Length, provider.Read(buffer, 0, buffer.Length));
        }

        var tail = samples.Length % buffer.Length;
        Assert.Equal(tail, provider.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, provider.Read(buffer, 0, buffer.Length));
    }

    /// <remarks>
    /// Proves a caller asking for more than the destination holds is refused
    /// rather than corrupting memory past the end of the buffer.
    /// </remarks>
    [Fact]
    public void Refuses_AReadLargerThanTheDestination()
    {
        var provider = new FloatSampleProvider(new float[100], ModeAudio.SampleRate);
        var buffer = new float[10];

        Assert.Throws<ArgumentException>(() => provider.Read(buffer, 0, 50));
        Assert.Throws<ArgumentException>(() => provider.Read(buffer, 8, 5));
        Assert.Throws<ArgumentException>(() => provider.Read(buffer, -1, 5));
    }

    /// <remarks>
    /// Proves an empty buffer is end-of-stream immediately rather than a
    /// crash — the shape a zero-length generation would take.
    /// </remarks>
    [Fact]
    public void EmptyBuffer_IsImmediatelyExhausted()
    {
        var provider = new FloatSampleProvider(Array.Empty<float>(), ModeAudio.SampleRate);

        Assert.Equal(0, provider.Read(new float[64], 0, 64));
        Assert.Equal(0, provider.Remaining);
    }
}
