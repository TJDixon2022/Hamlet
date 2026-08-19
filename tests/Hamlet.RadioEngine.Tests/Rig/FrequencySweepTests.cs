using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The frequency is swept, so a missed broadcast corrects itself (HM-DEC-109).
/// </summary>
/// <remarks>
/// <para>**THE RADIO BROADCASTS A CHANGE AND THAT WAS TAKEN TO BE ENOUGH**
/// (HM-DEC-050). It is not: nothing broadcasts a broadcast that went astray, so
/// a message dropped while the app was starting left the model holding a
/// frequency the radio was not on, with nothing to correct it until the dial was
/// next turned.</para>
/// <para>Mode and FilterSelection are broadcast too and have been swept for that
/// exact reason all along. The frequency was the one left out, and it is the one
/// that matters most, because the band on screen derives from it and the band
/// scopes what RBN is filtered to and what the skimmer watch listens for
/// (HM-DEC-024, HM-DEC-075).</para>
/// </remarks>
public sealed class FrequencySweepTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public FrequencySweepTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// <para>**THE WHOLE POINT, AND IT FAILS WITHOUT THE SWEEP.** The radio moves
    /// and says nothing about it, which is what a dropped broadcast looks like
    /// from Hamlet's side. Before this ruling the model would have held the old
    /// number until somebody turned the dial again.</para>
    /// </remarks>
    [Fact]
    public async Task AFrequencyChangeNobodyAnnouncedIsCaughtByTheSweep()
    {
        var rig = new SilentRig(7_030_000);
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));

        // The operator turns the dial and the message never arrives.
        rig.MoveWithoutTelling(14_030_000);

        await WaitFor(() =>
            monitor.State[RigField.Frequency] is { IsKnown: true, Number: 14_030_000 });

        monitor.Stop();

        var reading = monitor.State[RigField.Frequency];

        _output.WriteLine($"the model caught up to {reading.Number} Hz "
            + $"after {rig.FrequencyReads} reads");

        Assert.Equal(14_030_000, reading.Number);

        // AND IT WAS ASKED FOR, which is the part that used to be missing.
        Assert.True(rig.FrequencyReads > 0);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-109's second half, on the terms that ruling wanted and
    /// with the number the evening corrected. Its age used to recede without
    /// limit whenever nobody touched the dial, so a sidecar showed it stale at
    /// sixty seconds beside neighbors at twenty-seven, which reads as a link
    /// going quiet when it is a link with nothing to report.</para>
    /// <para>**IT IS A LIVE FIELD'S WINDOW NOW AND NOT A SETTING'S.** Two minutes
    /// was the ordinary window while the frequency was swept twice a minute; the
    /// operator then watched a thirty second lag on his own radio, which that
    /// window would have called current. A second and a half is what a value
    /// asked for four times a second is worth, and it is the same number the
    /// screen uses to decide whether to say the reading is old.</para>
    /// </remarks>
    [Fact]
    public void ItsFreshnessWindowIsTheOrdinaryOne()
    {
        Assert.Equal(
            RigPollPlan.FreshFor(RigField.SMeter),
            RigPollPlan.FreshFor(RigField.Frequency));

        Assert.Equal(RigPollPlan.LiveFreshFor, RigPollPlan.FreshFor(RigField.Frequency));
    }

    private static async Task WaitFor(Func<bool> until)
    {
        for (var i = 0; i < 400 && !until(); i++)
        {
            await Task.Delay(25);
        }
    }

    /// <summary>
    /// A radio that moves and never says so, which is what a dropped broadcast
    /// looks like from Hamlet's side.
    /// </summary>
    /// <remarks>Hand-rolled rather than a mocking framework (§6).</remarks>
    private sealed class SilentRig : IRig
    {
        private long _hz;

        public SilentRig(long hz) => _hz = hz;

        public int FrequencyReads { get; private set; }

        public void MoveWithoutTelling(long hz) => Interlocked.Exchange(ref _hz, hz);

        public bool IsConnected => true;

        public bool IsSimulated => true;

        public RigCapabilities Capabilities { get; } = new(
            "Silent radio", false, false, false, false, Array.Empty<string>());

        /// <summary>
        /// **DECLARED AND DELIBERATELY NEVER RAISED.** That silence is the whole
        /// fixture: this radio moves and says nothing, which is what a dropped
        /// broadcast looks like from Hamlet's side.
        /// </summary>
        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged
        {
            add { }
            remove { }
        }

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Interlocked.Read(ref _hz));

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            Interlocked.Exchange(ref _hz, frequencyHz);
            return Task.CompletedTask;
        }

        /// <summary>Nothing here keys anything (§0.2).</summary>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        /// <summary>Nothing is ever keyed, so there is nothing to stop.</summary>
        public void AbortCw()
        {
        }

        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("silent radio"));

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("silent radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            if (field == RigField.Frequency)
            {
                FrequencyReads++;

                var hz = Interlocked.Read(ref _hz);

                return Task.FromResult<IReadOnlyList<RigValue>>(new[]
                {
                    RigValue.Known(
                        field, hz, $"{hz / 1_000_000.0:0.000} MHz",
                        DateTime.UtcNow, "silent radio"),
                });
            }

            return Task.FromResult<IReadOnlyList<RigValue>>(new[]
            {
                RigValue.Known(field, 0, "0", DateTime.UtcNow, "silent radio"),
            });
        }

        /// <summary>Present because the seam requires it; never used here.</summary>
        public void Volunteer(params RigValue[] values)
            => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));
    }
}
