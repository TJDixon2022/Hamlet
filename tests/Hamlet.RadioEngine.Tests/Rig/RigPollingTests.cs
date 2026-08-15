using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The polling discipline: one command at a time, fast values only while
/// somebody is looking, and nothing polled that the radio volunteers
/// (HM-DEC-050).
/// </summary>
public sealed class RigPollingTests
{
    /// <remarks>
    /// Proves the frequency is never polled for. The radio broadcasts a change
    /// as the operator makes it, so asking as well would spend bus traffic on a
    /// fact already in hand and could only ever be more stale than the
    /// broadcast.
    /// </remarks>
    [Fact]
    public void WhatTheRadioVolunteersIsNotPolledFor()
    {
        Assert.Equal(RigPollRate.Never, RigPollPlan.RateFor(RigField.Frequency));
        Assert.DoesNotContain(RigField.Frequency, RigPollPlan.At(RigPollRate.Live));
        Assert.DoesNotContain(RigField.Frequency, RigPollPlan.At(RigPollRate.Session));
    }

    /// <remarks>
    /// Proves the fast things are fast and the slow things are slow. Treating
    /// them alike would either waste the bus on settings nobody changes or leave
    /// the S-meter crawling.
    /// </remarks>
    [Fact]
    public void FastMovingValuesAreSeparatedFromSettings()
    {
        Assert.Equal(RigPollRate.Live, RigPollPlan.RateFor(RigField.SMeter));
        Assert.Equal(RigPollRate.Live, RigPollPlan.RateFor(RigField.TransmitStatus));

        Assert.Equal(RigPollRate.Session, RigPollPlan.RateFor(RigField.Agc));
        Assert.Equal(RigPollRate.Session, RigPollPlan.RateFor(RigField.FilterBandwidth));
        Assert.Equal(RigPollRate.Session, RigPollPlan.RateFor(RigField.AccUsbAfLevel));

        Assert.True(RigPollPlan.LiveInterval < RigPollPlan.SessionInterval);
        Assert.True(RigPollPlan.LiveFreshFor < RigPollPlan.SessionFreshFor);
    }

    /// <remarks>
    /// Proves the staleness window is longer than the interval that refreshes
    /// it. If they matched, an ordinary missed poll would flicker the screen
    /// between fresh and stale, and the operator would learn to ignore the
    /// marking that matters.
    /// </remarks>
    [Fact]
    public void AValueStaysCurrentForLongerThanTheGapBetweenReads()
    {
        Assert.True(RigPollPlan.LiveFreshFor > RigPollPlan.LiveInterval);
        Assert.True(RigPollPlan.SessionFreshFor > RigPollPlan.SessionInterval);

        Assert.Equal(RigPollPlan.LiveFreshFor, RigPollPlan.FreshFor(RigField.SMeter));
        Assert.Equal(RigPollPlan.SessionFreshFor, RigPollPlan.FreshFor(RigField.Agc));
    }

    /// <remarks>
    /// Proves the filter selection is not asked for separately. Reading the mode
    /// answers it too, so a separate read would double the traffic for the same
    /// two bytes on a bus this slow.
    /// </remarks>
    [Fact]
    public void TheFilterSelectionRidesAlongWithTheMode()
    {
        Assert.DoesNotContain(RigField.FilterSelection, RigPollPlan.At(RigPollRate.Session));
        Assert.Contains(RigField.Mode, RigPollPlan.At(RigPollRate.Session));
    }

    /// <remarks>
    /// ONE COMMAND IN FLIGHT AT A TIME. A second read issued before the first
    /// answered would interleave two conversations on a bus that has no way to
    /// tell them apart.
    /// </remarks>
    [Fact]
    public async Task ThePollLoopNeverIssuesOverlappingReads()
    {
        var rig = new CountingRig();
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await WaitFor(() => monitor.ReadCount > 200);
        monitor.Stop();

        Assert.Equal(0, rig.MaximumConcurrent - 1);
        Assert.True(rig.Reads > 0);
    }

    /// <remarks>
    /// POLLING STOPS WHEN NOTHING IS ON SCREEN, the same politeness the spot
    /// feeds observe (HM-DEC-020). A minimized window has no S-meter to show, so
    /// asking four times a second for one is noise on a bus somebody else is
    /// trying to use.
    /// </remarks>
    [Fact]
    public async Task PollingStopsWhileTheWindowIsHidden()
    {
        var rig = new CountingRig();
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await WaitFor(() => monitor.ReadCount > 50);

        monitor.IsWatching = false;

        // Let the loop notice and settle.
        await WaitFor(() => true);
        await Task.Delay(50);

        var atRest = rig.Reads;
        await Task.Delay(150);

        Assert.Equal(atRest, rig.Reads);

        monitor.IsWatching = true;
        await WaitFor(() => rig.Reads > atRest);
        monitor.Stop();
    }

    /// <remarks>
    /// Proves a read that fails leaves that value unknown and the loop carries
    /// on. The failure this guards is a polling loop that dies on one bad
    /// response and takes every other reading down with it, silently.
    /// </remarks>
    [Fact]
    public async Task AFailingReadLeavesTheValueUnknownAndTheLoopRunning()
    {
        var rig = new CountingRig { ThrowOn = RigField.SMeter };
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();
        await WaitFor(() => monitor.ReadCount > 100);
        monitor.Stop();

        Assert.Equal(RigValueState.Unknown, monitor.State[RigField.SMeter].State);

        // And something else still got read.
        Assert.True(monitor.State[RigField.Agc].IsKnown);
    }

    /// <remarks>
    /// Proves what the radio volunteers lands in the model without anybody
    /// asking, and is stamped with the moment it arrived.
    /// </remarks>
    [Fact]
    public void AVolunteeredValueLandsInTheModel()
    {
        var rig = new CountingRig();
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        var seen = 0;
        monitor.StateChanged += (_, _) => seen++;

        rig.Volunteer(RigValue.Known(
            RigField.Mode, 1, "USB", DateTime.UtcNow, "transceive 01"));

        Assert.Equal("USB", monitor.State[RigField.Mode].Text);
        Assert.Equal("transceive 01", monitor.State[RigField.Mode].Source);
        Assert.Equal(1, seen);
        Assert.Equal(0, rig.Reads);
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5);
        }
    }

    /// <summary>
    /// A rig that answers instantly and records how it was asked.
    /// </summary>
    /// <remarks>
    /// Hand-rolled rather than a mocking framework (§6). What it exists to catch
    /// is overlap: it counts how many reads are in flight at once, so a
    /// scheduler that issued a second before the first answered fails loudly.
    /// </remarks>
    private sealed class CountingRig : IRig
    {
        private int _inFlight;

        public int Reads { get; private set; }

        public int MaximumConcurrent { get; private set; } = 1;

        public RigField? ThrowOn { get; init; }

        public bool IsConnected => true;

        public bool IsSimulated => false;

        public RigCapabilities Capabilities { get; } = new(
            "Counting radio", false, false, false, false, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public void Volunteer(params RigValue[] values)
            => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(7_030_000L);

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));
            return Task.CompletedTask;
        }

        /// <summary>This one exists to count reads, so it never keys.</summary>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        /// <summary>Nothing is ever keyed here, so there is nothing to stop.</summary>
        public void AbortCw()
        {
        }

        /// <summary>This one exists to count reads, so it writes nothing.</summary>
        public Task<RigWriteResult> SetSettingAsync(
                Hamlet.RadioEngine.Civ.CivWrite write, int value,
                CancellationToken cancellationToken = default)
                => Task.FromResult(RigWriteResult.NotSupported("test rig"));
        public Task<RigWriteResult> SetModeAsync(
            Hamlet.RadioEngine.Civ.CivMode mode, bool dataMode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("counting radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            var now = Interlocked.Increment(ref _inFlight);
            MaximumConcurrent = Math.Max(MaximumConcurrent, now);
            Reads++;

            try
            {
                if (field == ThrowOn)
                {
                    throw new InvalidOperationException("scripted failure");
                }

                IReadOnlyList<RigValue> values = new[]
                {
                    RigValue.Known(field, 1, "scripted", DateTime.UtcNow, "test"),
                };

                return Task.FromResult(values);
            }
            finally
            {
                Interlocked.Decrement(ref _inFlight);
            }
        }

        public void Dispose()
        {
        }
    }
}
