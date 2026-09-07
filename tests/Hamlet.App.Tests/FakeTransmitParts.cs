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
internal sealed class FakeSink : ITransmitAudioSink, ITransmitLevelReport
{
    /// <summary>How many times something asked it to play.</summary>
    public int TimesCalled { get; private set; }

    /// <summary>How many samples it was handed, last time.</summary>
    public int SamplesHandedOver { get; private set; }

    /// <summary>
    /// The peak this sink says the endpoint was actually handed.
    /// </summary>
    /// <remarks>
    /// <para>**IT IS SET BY THE TEST AND IT IS DELIBERATELY NOT THE COMPOSED
    /// PEAK** (work instruction 269, task 3). The whole failure this unit exists
    /// to make impossible is a readout that shows the operator his own drive
    /// setting and calls it a measurement, and the only way a test can tell those
    /// two apart is for the two numbers to be different.</para>
    /// <para>**ZERO BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.**
    /// <c>WasapiTransmitSink.PeakWritten</c> is also zero until something has
    /// been played.</para>
    /// </remarks>
    public double ReportsPeakWritten { get; set; }

    /// <summary>How many samples this sink says it had to clamp on the way out.</summary>
    /// <remarks>
    /// Set by the test, for the same reason as
    /// <see cref="ReportsPeakWritten"/>: the composed array is built inside the
    /// rails and its own clip count is zero by construction, so a readout showing
    /// the sink's count and one showing the composer's are only distinguishable
    /// when they differ.
    /// </remarks>
    public long ReportsClippedSamples { get; set; }

    /// <inheritdoc/>
    public double PeakWritten => ReportsPeakWritten;

    /// <inheritdoc/>
    public long ClippedSamples => ReportsClippedSamples;

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

    /// <inheritdoc/>
    /// <remarks>
    /// **12000 WHERE NOTHING WAS DECLARED**, which is the rate every test written
    /// before unit 262 composes at, so those tests keep the meaning they had.
    /// </remarks>
    public int EndpointSampleRate => DeclaredSampleRate ?? Ft8Composer.DefaultSampleRate;

    /// <summary>True where nothing ever reached it.</summary>
    public bool WasNeverTouched => TimesCalled == 0;

    /// <summary>
    /// Wall-clock time the whole slot should take, or null for the instant play.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE BEHAVIOUR THIS FAKE LACKED** (work instruction 263, task 2),
    /// and it lacked it worse than its engine-side neighbour did: this one had no
    /// way to report a short play at all, so **it always claimed the full
    /// `samples.Length` went out in a hard-coded 12.64 seconds**, whatever
    /// happened. Since this is the fake the application's send path actually runs
    /// against, that is where a stop that stopped only half of what was on the air
    /// would have hidden - and did.</para>
    /// <para>**IT IS WALL TIME, AND WHAT IS REPORTED IS SAMPLES.**
    /// <see cref="PlayedSoFar"/> advances in proportion to elapsed wall time
    /// against this span, so a test can compress a 12.64 s slot into a second and
    /// the fraction played, and therefore the milliseconds of audio, stay exact.</para>
    /// <para>**NULL BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.**</para>
    /// </remarks>
    public TimeSpan? PlaysOver { get; set; }

    /// <summary>Set once something is actually inside the play.</summary>
    /// <remarks>
    /// How a test lands a stop mid-transmission without racing it. Set on the
    /// timed path only; the instant play has no middle to be in.
    /// </remarks>
    public ManualResetEventSlim Entered { get; } = new(false);

    /// <summary>
    /// How many samples have gone out so far, readable while the play is running.
    /// </summary>
    /// <remarks>
    /// The difference between this read at the stop and read after the run ends,
    /// over the rate, is **the milliseconds of audio that left the machine after
    /// the operator pressed stop**.
    /// </remarks>
    public int PlayedSoFar { get; private set; }

    /// <summary>True where the token ended the play before the samples ran out.</summary>
    public bool StoppedByTheToken { get; private set; }

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

        if (PlaysOver is not TimeSpan over)
        {
            PlayedSoFar = samples.Length;

            return Task.FromResult(
                new PlayedAudio(samples.Length, TimeSpan.FromSeconds(12.64)));
        }

        // NOT `async` ON THE METHOD ITSELF. The refusal above throws synchronously
        // today and unit 262's tests rely on it; an `async` keyword here would
        // wrap it in a faulted task instead.
        return PlayOverTimeAsync(samples.Length, over, cancellationToken);
    }

    /// <summary>The play that takes time and stops when it is told to.</summary>
    /// <param name="total">How many samples the whole slot is.</param>
    /// <param name="over">How long the whole slot should take in wall time.</param>
    /// <param name="cancellationToken">Stops the playing.</param>
    /// <returns>How much actually went out, and how long it actually took.</returns>
    /// <remarks>
    /// **IT POLLS, BECAUSE `WasapiTransmitSink` POLLS**
    /// (<c>WasapiTransmitSink.cs:339</c> and <c>:372</c>), and **it does not throw
    /// on cancellation**, because the real sink does not either: the contract of
    /// this method is to say how much went out and an
    /// <c>OperationCanceledException</c> cannot.
    /// </remarks>
    private async Task<PlayedAudio> PlayOverTimeAsync(
        int total, TimeSpan over, CancellationToken cancellationToken)
    {
        const int PollMilliseconds = 2;

        var clock = System.Diagnostics.Stopwatch.StartNew();

        PlayedSoFar = 0;
        StoppedByTheToken = false;
        Entered.Set();

        while (PlayedSoFar < total && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(PollMilliseconds, CancellationToken.None).ConfigureAwait(false);

            var through = over <= TimeSpan.Zero
                ? 1.0
                : clock.Elapsed.TotalMilliseconds / over.TotalMilliseconds;

            PlayedSoFar = (int)Math.Min(total, Math.Max(0, through * total));
        }

        StoppedByTheToken = PlayedSoFar < total;
        clock.Stop();

        return new PlayedAudio(PlayedSoFar, clock.Elapsed);
    }
}
