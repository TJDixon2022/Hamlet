using Hamlet.RadioEngine.Transmit;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// An <see cref="ITransmitAudioSink"/> that plays nothing and can be told to
/// fail in each of the ways a real one can.
/// </summary>
/// <remarks>
/// <para>**IT OPENS NO DEVICE AND MAKES NO SOUND** (work instruction 255). The
/// render implementation is the next unit's; what is being proved here is the
/// sequence around the sink, and every way a sink can end - returning, throwing,
/// being cancelled, and the quiet one, returning having played less than it was
/// given.</para>
/// <para>It records what it was handed so a test can assert the sink was never
/// touched, which is what a licence refusal has to prove.</para>
/// <para>**AND SINCE UNIT 262 IT CAN DECLARE A RATE AND REFUSE EVERY OTHER ONE**,
/// which is the last way a real sink can fail that this could not - see
/// <see cref="DeclaredSampleRate"/>.</para>
/// </remarks>
internal sealed class FakeTransmitAudioSink : ITransmitAudioSink
{
    /// <summary>How many times something asked it to play.</summary>
    public int TimesCalled { get; private set; }

    /// <summary>How many samples it was handed, last time.</summary>
    public int SamplesHandedOver { get; private set; }

    /// <summary>What rate it was asked for, last time.</summary>
    public int RateAskedFor { get; private set; }

    /// <summary>True where nothing ever reached it.</summary>
    public bool WasNeverTouched => TimesCalled == 0;

    /// <summary>Throw this instead of playing.</summary>
    public Exception? Throws { get; set; }

    /// <summary>Report having played only this many samples.</summary>
    public int? PlaysOnly { get; set; }

    /// <summary>
    /// The rate this endpoint declares, or null to take whatever it is handed.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE BEHAVIOUR THIS FAKE LACKED, AND WHY THAT MATTERED** (work
    /// instruction 262, task 2). <c>WasapiTransmitSink</c> has a rate it will
    /// accept - the endpoint's shared-mode mix format - and throws on anything
    /// else, rather than letting shared-mode WASAPI resample quietly. This fake
    /// recorded the rate it was asked for and played anyway, which made it **more
    /// permissive than the thing it stands for**: every test that handed it 12000
    /// Hz passed, against a real sink that would have refused, and sixteen units
    /// of green tests sat over a send path that could not transmit on any endpoint
    /// this machine has.</para>
    /// <para>**NULL BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.** A fake that
    /// suddenly refused would be a different fixture wearing the same name.
    /// Setting this is how a test says *stand for a real endpoint*.</para>
    /// </remarks>
    public int? DeclaredSampleRate { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// **12000 WHERE NOTHING WAS DECLARED**, which is the rate every test written
    /// before unit 262 composes at, so those tests keep the meaning they had.
    /// </remarks>
    public int EndpointSampleRate => DeclaredSampleRate ?? Ft8Composer.DefaultSampleRate;

    /// <summary>How long it claims the playing took.</summary>
    public TimeSpan Took { get; set; } = TimeSpan.FromSeconds(12.64);

    /// <summary>
    /// Wall-clock time the whole slot should take, or null for the instant play.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE BEHAVIOUR THIS FAKE LACKED, AND WHY THAT MATTERED** (work
    /// instruction 263, task 2). A play that returns before the caller can reach
    /// it cannot be cancelled by anything, so **every test of the operator's stop
    /// was proved against a sink that had already finished** - and twelve of the
    /// twelve tests in <c>TheOperatorsStopFiresFromEveryStateTests</c> would still
    /// pass if the audio never stopped at all
    /// (<c>docs/unit263-stop-audio-trace.md</c> Q7). A fake more permissive than
    /// the real thing hides the defect, which is unit 262's lesson and it cost
    /// that unit a task.</para>
    /// <para>**IT IS WALL TIME, AND WHAT IS REPORTED IS SAMPLES.**
    /// <see cref="PlayedSoFar"/> advances in proportion to elapsed wall time
    /// against this span, so a test can compress a 12.64 s slot into a second and
    /// **the fraction played, and therefore the milliseconds of audio, stay
    /// exact**. A test that wants real time sets this to the slot's own duration.</para>
    /// <para>**NULL BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.** Every test
    /// written before this unit gets the instant play it has always had, with
    /// <see cref="PlaysOnly"/> and <see cref="Throws"/> untouched. Setting this is
    /// how a test says *stand for a transmission that is still going out*.</para>
    /// </remarks>
    public TimeSpan? PlaysOver { get; set; }

    /// <summary>Set once something is actually inside the play.</summary>
    /// <remarks>
    /// **HOW A TEST LANDS A STOP MID-TRANSMISSION WITHOUT RACING IT.** The radio
    /// is keyed, the samples have been handed over, and the sequence is where it
    /// spends the slot - which is the moment the operator reaches for the stop.
    /// It is set on the timed path only; the instant play has no middle to be in.
    /// </remarks>
    public ManualResetEventSlim Entered { get; } = new(false);

    /// <summary>
    /// How many samples have gone out so far, readable while the play is running.
    /// </summary>
    /// <remarks>
    /// **THE MEASURE THE WHOLE UNIT TURNS ON.** A test reads it just before it
    /// calls the stop and again after the run ends, and the difference in samples
    /// over the rate is **the milliseconds of audio that left the machine after
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

        if (Throws is not null)
        {
            throw Throws;
        }

        if (PlaysOver is not TimeSpan over)
        {
            PlayedSoFar = PlaysOnly ?? samples.Length;

            return Task.FromResult(new PlayedAudio(PlayedSoFar, Took));
        }

        // NOT `async` ON THE METHOD ITSELF. The two refusals above throw
        // synchronously today and tests written against them rely on it; an
        // `async` keyword here would wrap both in a faulted task instead.
        return PlayOverTimeAsync(samples.Length, over, cancellationToken);
    }

    /// <summary>The play that takes time and stops when it is told to.</summary>
    /// <param name="total">How many samples the whole slot is.</param>
    /// <param name="over">How long the whole slot should take in wall time.</param>
    /// <param name="cancellationToken">Stops the playing.</param>
    /// <returns>How much actually went out, and how long it actually took.</returns>
    /// <remarks>
    /// <para>**IT POLLS, BECAUSE <c>WasapiTransmitSink</c> POLLS**
    /// (<c>WasapiTransmitSink.cs:339</c> and <c>:372</c>). A fake that registered
    /// a callback instead would prove a mechanism the real sink does not have.</para>
    /// <para>**AND IT DOES NOT THROW ON CANCELLATION**, for the reason the real
    /// sink does not: the contract of this method is to say how much went out, and
    /// an <c>OperationCanceledException</c> cannot. The waits are on
    /// <see cref="CancellationToken.None"/> for the same reason - the real sink's
    /// <c>Task.Delay(WaitMilliseconds, CancellationToken.None)</c>.</para>
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
