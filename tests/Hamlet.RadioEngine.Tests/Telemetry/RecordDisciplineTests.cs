using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Telemetry;

/// <summary>
/// Diagnostics that cannot hurt the thing they are diagnosing (HM-DEC-077, §8).
/// </summary>
/// <remarks>
/// This work expands what is written more than anything before it, which makes
/// §8's never-throw discipline load-bearing rather than decorative. Logging that
/// can crash the app is worse than no logging, and a decoder that stutters to
/// write its own diagnostics has traded the thing for the record of the thing.
/// </remarks>
public sealed class RecordDisciplineTests
{
    /// <summary>A sink that fails at everything, as a full disk would.</summary>
    private sealed class BrokenSink : ITelemetry
    {
        public long DroppedEventCount { get; private set; }

        public void Write(
            TelemetryCategory category, string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
        {
            // A real sink swallows and counts. This one proves the caller does
            // not depend on the write having worked.
            DroppedEventCount++;
        }
    }

    /// <remarks>
    /// Proves §8 and HM-DEC-077: a write failure is swallowed and counted, and
    /// the caller carries on. A failed write is dropped and counted, never
    /// propagated.
    /// </remarks>
    [Fact]
    public void AWriteFailureIsSwallowedAndCounted()
    {
        var sink = new BrokenSink();

        for (var i = 0; i < 5; i++)
        {
            sink.Write(TelemetryCategory.Rig, "transmit_readiness");
        }

        Assert.Equal(5, sink.DroppedEventCount);

        // And the null sink, which is what a fully switched-off profile gets,
        // takes everything without complaint.
        NullTelemetry.Instance.Write(TelemetryCategory.Decode, "decode_window");

        Assert.Equal(0, NullTelemetry.Instance.DroppedEventCount);
    }

    /// <remarks>
    /// Proves HM-DEC-077: the decoder's aggregation does not allocate on the hot
    /// path. At speed the emit counter runs about forty times a second, and a
    /// decoder that allocated per character would be paying for its own
    /// diagnostics in the one place the prime directive cares about.
    /// </remarks>
    [Fact]
    public void TheDecoderAggregationDoesNotAllocatePerCharacter()
    {
        var window = new DecodeWindow();

        // Warm the path so the measurement is about the loop and not about
        // first-call jitter.
        for (var i = 0; i < 1_000; i++)
        {
            window.Emitted(CwConfidence.High);
            window.Rejected(DecodeRejection.NotMorseTiming);
            window.Observed(-98.5, 604, 15);
        }

        window.Reset();

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 100_000; i++)
        {
            window.Emitted(CwConfidence.High);
            window.Emitted(CwConfidence.Low);
            window.Emitted(CwConfidence.Unreadable);
            window.Rejected(DecodeRejection.Contested);
            window.Observed(-98.5, 604, 15);
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(
            allocated == 0,
            $"the hot path allocated {allocated} bytes across 500,000 calls");
    }

    /// <remarks>
    /// Proves HM-DEC-018: a decode window carries counts and measurements and
    /// has nowhere to put decoded text. The shape refuses rather than the call
    /// site remembering, which is the same reasoning that put every payload in
    /// one class.
    /// </remarks>
    [Fact]
    public void ADecodeWindowCannotCarryDecodedText()
    {
        var window = new DecodeWindow();

        window.Emitted(CwConfidence.High);
        window.Emitted(CwConfidence.Low);
        window.Rejected(DecodeRejection.BelowConfidence);
        window.Observed(-101.2, 598.5, 14);

        var bag = window.ToBag();

        Assert.Equal(1, bag["emittedHigh"]);
        Assert.Equal(1, bag["emittedLow"]);
        Assert.Equal(1, bag["rejectedBelowConfidence"]);
        Assert.Equal(true, bag["toneTracked"]);

        // Every value is a number or a flag. Nothing here is text at all, so
        // there is nowhere for a character to hide.
        foreach (var pair in bag)
        {
            Assert.True(
                pair.Value is int or long or double or bool,
                $"'{pair.Key}' is a {pair.Value?.GetType().Name}, which could hold text");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-077: a window that rejected everything it heard is a
    /// warning, because that is worth finding by scanning. A quiet band is not:
    /// hearing nothing is the ordinary state of a receiver.
    /// </remarks>
    [Fact]
    public void AWindowThatRejectedEverythingIsAWarning()
    {
        var rejecting = new DecodeWindow();
        rejecting.Rejected(DecodeRejection.Contested);
        rejecting.Rejected(DecodeRejection.NotMorseTiming);

        Assert.Equal(TelemetryLevel.Warn, rejecting.Level);

        var working = new DecodeWindow();
        working.Emitted(CwConfidence.High);
        working.Rejected(DecodeRejection.Contested);

        Assert.Equal(TelemetryLevel.Info, working.Level);

        var quiet = new DecodeWindow();

        Assert.True(quiet.IsEmpty);
        Assert.Equal(TelemetryLevel.Info, quiet.Level);
    }
}
