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
/// **NOTHING IN THIS PROJECT CONSTRUCTS A `WasapiTransmitSink`.** Every
/// transmission asserted here goes to this, through the substitutable factory
/// work instruction 260 task 3 puts on the view model.
/// </remarks>
internal sealed class FakeSink : ITransmitAudioSink
{
    /// <summary>How many times something asked it to play.</summary>
    public int TimesCalled { get; private set; }

    /// <summary>How many samples it was handed, last time.</summary>
    public int SamplesHandedOver { get; private set; }

    /// <summary>True where nothing ever reached it.</summary>
    public bool WasNeverTouched => TimesCalled == 0;

    /// <inheritdoc/>
    public Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
    {
        TimesCalled++;
        SamplesHandedOver = samples.Length;

        return Task.FromResult(
            new PlayedAudio(samples.Length, TimeSpan.FromSeconds(12.64)));
    }
}
