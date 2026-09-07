using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.App.Tests;

/// <summary>
/// A serial port that opens nothing and keeps every frame it was given.
/// </summary>
/// <remarks>
/// <para>**NO PORT IS EVER OPENED IN THIS PROJECT** (`SHACK_FACTS.md` FACT-004,
/// work instruction 260 *What not to do* 1). The engine's own tests have a fake
/// of this shape, but it is `internal` to that assembly, so the application's
/// tests carry their own rather than widening anybody's visibility.</para>
/// <para>It records the frames so a test can say what did and did not reach a
/// radio - which is what *nothing was armed* has to prove.</para>
/// </remarks>
internal sealed class FakePort : ISerialPort
{
    private readonly List<byte[]> _written = [];

    /// <summary>Every frame that was written, in order.</summary>
    public IReadOnlyList<byte[]> Written => _written;

    /// <summary>True where nothing was ever written to it.</summary>
    public bool WasNeverWrittenTo => _written.Count == 0;

    /// <inheritdoc/>
    public bool IsOpen { get; private set; } = true;

    /// <inheritdoc/>
    public string PortName => "FAKE1";

    /// <inheritdoc/>
    public int BaudRate => 115_200;

    /// <inheritdoc/>
    public void Open() => IsOpen = true;

    /// <inheritdoc/>
    public void Close() => IsOpen = false;

    /// <inheritdoc/>
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        => ValueTask.FromResult(0);

    /// <inheritdoc/>
    public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        _written.Add(buffer.ToArray());

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Write(ReadOnlySpan<byte> buffer) => _written.Add(buffer.ToArray());

    /// <inheritdoc/>
    public void Dispose() => IsOpen = false;
}

/// <summary>
/// A transmit sink that opens no device, makes no sound, and counts.
/// </summary>
/// <remarks>
/// <para>**NOTHING IN THIS PROJECT CONSTRUCTS A `WasapiTransmitSink`.** Every
/// transmission asserted here goes to this, through the substitutable factory
/// work instruction 260 task 3 puts on the view model.</para>
/// <para>**AND SINCE UNIT 262 IT CAN DECLARE A RATE AND REFUSE EVERY OTHER ONE.**
/// The engine's own fake gained the same behaviour in the same unit; this one
/// needs it too, because this is the fake the application's send path actually
/// runs against and the engine's is `internal` to the other assembly. See
/// <see cref="DeclaredSampleRate"/>.</para>
/// </remarks>
internal sealed class FakeSink : ITransmitAudioSink
{
    /// <summary>How many times something asked it to play.</summary>
    public int TimesCalled { get; private set; }

    /// <summary>How many samples it was handed, last time.</summary>
    public int SamplesHandedOver { get; private set; }

    /// <summary>What rate it was asked for, last time.</summary>
    /// <remarks>
    /// Zero until something has been played. **It was not read at all before unit
    /// 262**, which is the narrower half of why no test caught a send path
    /// composing at a rate no endpoint speaks.
    /// </remarks>
    public int RateAskedFor { get; private set; }

    /// <summary>
    /// The rate this endpoint declares, or null to take whatever it is handed.
    /// </summary>
    /// <remarks>
    /// <para>**A FAKE MORE PERMISSIVE THAN THE THING IT STANDS FOR IS WHY THE
    /// TESTS WERE GREEN** (work instruction 262, task 2).
    /// <c>WasapiTransmitSink</c> accepts one rate - the endpoint's shared-mode mix
    /// format - and throws on anything else rather than letting shared-mode WASAPI
    /// resample silently. Until this was here, this fake took any number at all.</para>
    /// <para>**NULL BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.** Setting it
    /// is how a test says *stand for a real endpoint*.</para>
    /// </remarks>
    public int? DeclaredSampleRate { get; set; }

    /// <summary>True where nothing ever reached it.</summary>
    public bool WasNeverTouched => TimesCalled == 0;

    /// <inheritdoc/>
    public Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
    {
        TimesCalled++;
        SamplesHandedOver = samples.Length;
        RateAskedFor = sampleRate;

        if (DeclaredSampleRate is int declared && sampleRate != declared)
        {
            // **THE SAME SHAPE OF MESSAGE AS `WasapiTransmitSink.cs:306-309`**,
            // word for word, because a fake whose refusal reads differently sends
            // whoever reads the red looking in the wrong place.
            throw new InvalidOperationException(
                $"the samples are at {sampleRate} Hz and the endpoint speaks "
                + $"{declared} Hz. Nothing is played rather than a rate being "
                + "silently changed on the way out.");
        }

        return Task.FromResult(
            new PlayedAudio(samples.Length, TimeSpan.FromSeconds(12.64)));
    }
}
