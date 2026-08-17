using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Training;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// The spectrum scope stream, CI-V <c>27 00</c> (HM-DEC-062, HM-DEC-005).
/// </summary>
/// <remarks>
/// Verified column-aware against `IC-7300_Full_English v6` p. 19-12. Reads only:
/// nothing in this file or the code it covers writes to or keys the radio.
/// </remarks>
public sealed class ScopeStreamTests
{
    private const byte Radio = CivConstants.DefaultRadioAddress;
    private const byte Controller = CivConstants.DefaultControllerAddress;

    /// <summary>A small number as the radio packs it: BCD, not hexadecimal.</summary>
    /// <remarks>
    /// **THE FIXTURES USED TO BE BUILT THE WAY THE PARSER READ**, which is why
    /// they passed while the radio's own frames were discarded (HM-DEC-094).
    /// </remarks>
    private static byte Packed(int value)
        => (byte)(((value / 10) << 4) | (value % 10));

    /// <summary>A first part: fixed 00, order, maximum, mode, center, span, in-range.</summary>
    private static byte[] Header(
        int total = 11, bool fixedMode = false, bool outOfRange = false,
        long centerHz = 7_100_000, long spanHz = 200_000)
    {
        var bytes = new List<byte>
        {
            0x00, Packed(1), Packed(total), (byte)(fixedMode ? 1 : 0),
        };

        bytes.AddRange(Bcd.EncodeFrequencyHz(fixedMode ? centerHz - (spanHz / 2) : centerHz));
        bytes.AddRange(Bcd.EncodeFrequencyHz(fixedMode ? centerHz + (spanHz / 2) : spanHz));
        bytes.Add((byte)(outOfRange ? 1 : 0));

        return bytes.ToArray();
    }

    /// <summary>A continuation part carrying amplitudes.</summary>
    private static byte[] Part(int sequence, int total, params byte[] waveform)
    {
        var bytes = new List<byte> { 0x00, Packed(sequence), Packed(total) };
        bytes.AddRange(waveform);
        return bytes.ToArray();
    }

    private static async Task<(Ic7300Rig Rig, FakeSerialPort Port)> ConnectAsync()
    {
        var port = new FakeSerialPort();
        var rig = new Ic7300Rig(port);

        var connect = rig.ConnectAsync();
        port.EnqueueIncoming(new CivFrame(
            Controller, Radio, CivConstants.CmdReadFrequency,
            new byte[] { 0x00, 0x30, 0x07, 0x07, 0x00 }).ToWireBytes());

        Assert.True(await connect);
        return (rig, port);
    }

    // ---- The header ----------------------------------------------------

    /// <remarks>
    /// THE SPAN COMES OFF THE WIRE, NOT OUT OF THE BAND PLAN. In center mode the
    /// radio sends a center frequency and a span; in fixed mode it sends the two
    /// edges (p. 19-12), and the mode flag is the only thing that says which
    /// reading is which.
    /// </remarks>
    [Fact]
    public void TheSpanIsReadFromWhicheverShapeTheRadioSent()
    {
        var center = CivScope.ReadHeader(Header(centerHz: 7_100_000, spanHz: 200_000));

        Assert.NotNull(center);
        Assert.False(center!.IsFixedMode);
        Assert.Equal(7_000_000, center.LowHz);
        Assert.Equal(7_200_000, center.HighHz);

        var fixedMode = CivScope.ReadHeader(
            Header(fixedMode: true, centerHz: 7_100_000, spanHz: 200_000));

        Assert.NotNull(fixedMode);
        Assert.True(fixedMode!.IsFixedMode);
        Assert.Equal(7_000_000, fixedMode.LowHz);
        Assert.Equal(7_200_000, fixedMode.HighHz);
    }

    /// <remarks>
    /// A FRAME THAT DOES NOT MATCH ITS DOCUMENTED SHAPE PRODUCES NOTHING, never
    /// a nearest guess. Falling back to the band's own edges would draw a
    /// waterfall whose frequencies are Hamlet's invention rather than the
    /// radio's measurement, on the one surface built to show what is actually
    /// there (§0.0).
    /// </remarks>
    [Fact]
    public void AHeaderThatWillNotParseProducesNothingRatherThanAGuess()
    {
        Assert.Null(CivScope.ReadHeader(Array.Empty<byte>()));
        Assert.Null(CivScope.ReadHeader(new byte[] { 0x01, 0x0B }));

        // A part whose leading fixed field is not zero.
        Assert.Null(CivScope.ReadHeader(new byte[] { 0x09, 0x01, 0x11 }));

        // A division maximum that is not BCD at all.
        var notBcd = Header();
        notBcd[2] = 0xAF;
        Assert.Null(CivScope.ReadHeader(notBcd));

        // A mode byte outside the two documented values, which now sits after
        // the three bytes every part carries (HM-DEC-094).
        var odd = Header();
        odd[CivScope.PartHeaderLength] = 0x07;
        Assert.Null(CivScope.ReadHeader(odd));

        // A span of nothing, which would make every bin the same frequency.
        Assert.Null(CivScope.ReadHeader(Header(spanHz: 0)));
    }

    /// <remarks>
    /// Proves the amplitude scale is the radio's own. Data range 0 to 160
    /// (p. 19-12), scaled onto the palette's byte so the top third of every
    /// palette is not left unused, and clamped rather than wrapped: a byte that
    /// wrapped would draw a strong signal as a hole.
    /// </remarks>
    [Fact]
    public void AmplitudesAreScaledFromTheRadiosOwnRangeAndNeverWrap()
    {
        Assert.Equal(0, CivScope.Scale(0));
        Assert.Equal(255, CivScope.Scale(CivScope.MaximumAmplitude));
        Assert.Equal(255, CivScope.Scale(255));
        Assert.InRange(CivScope.Scale(80), 120, 135);
    }

    // ---- Assembly ------------------------------------------------------

    /// <remarks>
    /// A SWEEP ARRIVES IN PARTS AND IS PUBLISHED ONCE. The first part carries the
    /// header without waveform data and the rest carry the waveform, divided by
    /// eleven over USB (p. 19-12).
    /// </remarks>
    [Fact]
    public async Task ASweepIsAssembledFromItsPartsAndPublishedOnce()
    {
        var (rig, port) = await ConnectAsync();
        using var owned = rig;
        using var source = new RigSpectrumSource(rig);

        var spans = new List<(long Low, long High, int Bins)>();
        source.FrameReady += (in SpectrumFrame f)
            => spans.Add((f.LowHz, f.HighHz, f.Bins.Length));

        source.Start();

        Publish(port, Header(total: 3));
        Publish(port, Part(2, 3, 10, 20, 30));
        Publish(port, Part(3, 3, 40, 50));

        await WaitFor(() => spans.Count > 0);

        var frame = Assert.Single(spans);
        Assert.Equal(7_000_000, frame.Low);
        Assert.Equal(7_200_000, frame.High);
        Assert.Equal(5, frame.Bins);
        Assert.Equal(1, source.SweepCount);
    }

    /// <remarks>
    /// A PART THAT ARRIVES OUT OF ORDER DROPS THE SWEEP RATHER THAN PATCHING IT.
    /// A waterfall row assembled from two different sweeps would draw signals
    /// that were never simultaneously there.
    /// </remarks>
    [Fact]
    public async Task AMissingPartDropsTheSweepRatherThanDrawingHalfARow()
    {
        var (rig, port) = await ConnectAsync();
        using var owned = rig;
        using var source = new RigSpectrumSource(rig);

        var frames = 0;
        source.FrameReady += (in SpectrumFrame _) => frames++;
        source.Start();

        Publish(port, Header(total: 4));
        Publish(port, Part(2, 4, 10));
        Publish(port, Part(4, 4, 30));

        await WaitFor(() => source.DroppedCount > 0);

        Assert.Equal(0, frames);
        Assert.Equal(1, source.DroppedCount);
        Assert.Equal(0, source.SweepCount);
    }

    /// <remarks>
    /// Proves an out-of-range sweep draws nothing. The radio says so and omits
    /// the waveform entirely (p. 19-12), so there is nothing honest to show and
    /// Hamlet shows nothing.
    /// </remarks>
    [Fact]
    public async Task AnOutOfRangeSweepDrawsNothing()
    {
        var (rig, port) = await ConnectAsync();
        using var owned = rig;
        using var source = new RigSpectrumSource(rig);

        var frames = 0;
        source.FrameReady += (in SpectrumFrame _) => frames++;
        source.Start();

        Publish(port, Header(total: 2, outOfRange: true));
        Publish(port, Part(2, 2, 10, 20));

        await WaitFor(() => false);

        Assert.Equal(0, frames);
    }

    // ---- Honesty and rationing ------------------------------------------

    /// <remarks>
    /// REAL DATA ARRIVING MUST NOT WEAKEN THE SIMULATED LABEL (HM-DEC-026). Each
    /// source answers for itself and neither has a setter, so there is no flag to
    /// forget and no path that puts synthetic frames on screen unlabeled.
    /// </remarks>
    [Fact]
    public async Task EachSourceAnswersForItselfAndNeitherHasASetter()
    {
        var (rig, _) = await ConnectAsync();
        using var owned = rig;
        using var live = new RigSpectrumSource(rig);
        using var training = new TrainingSpectrumSource(
            BandPlan.Bands.First());

        Assert.False(live.IsSimulated);
        Assert.True(training.IsSimulated);

        Assert.Null(typeof(RigSpectrumSource)
            .GetProperty(nameof(ISpectrumSource.IsSimulated))!.SetMethod);
        Assert.Null(typeof(TrainingSpectrumSource)
            .GetProperty(nameof(ISpectrumSource.IsSimulated))!.SetMethod);
    }

    /// <remarks>
    /// THE STREAM COSTS THE POLL LOOP NOTHING (HM-DEC-050). The radio pushes
    /// these frames once its own output is on, so the source asks for nothing
    /// and cannot starve anything: it is a listener, and it issues no commands
    /// at all.
    /// </remarks>
    [Fact]
    public async Task ListeningToTheScopeIssuesNoCommands()
    {
        var (rig, port) = await ConnectAsync();
        using var owned = rig;
        using var source = new RigSpectrumSource(rig);

        var sent = 0;
        rig.FrameTrace += (outgoing, _) =>
        {
            if (outgoing)
            {
                sent++;
            }
        };

        source.Start();
        Publish(port, Header(total: 2));
        Publish(port, Part(2, 2, 10, 20));

        await WaitFor(() => source.SweepCount > 0);
        source.Stop();

        Assert.Equal(0, sent);
    }

    /// <remarks>
    /// NOTHING HERE WRITES TO THE RADIO. The two settings the stream needs are
    /// read and reported, never set: turning somebody's scope on is a change to
    /// their radio, and the output setting depends on two CI-V screens that are
    /// not commands at all (p. 19-7, footnote 4).
    /// </remarks>
    [Fact]
    public void TheScopeSettingsAreReadAndReportedAndNeverSet()
    {
        Assert.Equal(0x27, CivReads.ScopeOn.Command);
        Assert.Equal(new byte[] { 0x10 }, CivReads.ScopeOn.SubCommand);
        Assert.Equal(0x27, CivReads.ScopeOutput.Command);
        Assert.Equal(new byte[] { 0x11 }, CivReads.ScopeOutput.SubCommand);

        // And no scope write exists in the write table at all.
        // **THE SCOPE OUTPUT IS A WRITE NOW** (HM-DEC-092), which reverses what
        // this line used to assert. `27 11` is send/read in the command table and
        // an ordinary tier one receive-side setting: it decides whether the
        // picture the radio is already drawing is also sent down the cable, and
        // nothing about it can put a signal on the air. Reading it, finding it
        // off, and printing advice was the application declining to use the write
        // layer it had.
        var scopeWrite = Assert.Single(
            CivWrites.All, w => w.Command == CivConstants.CmdScope);

        Assert.Equal(new byte[] { 0x11 }, scopeWrite.SubCommand);
        Assert.Equal(RigWriteTier.Receive, scopeWrite.Tier);
        Assert.Equal("19-7", scopeWrite.Page);

        // And the scope's own on/off switch is still read only, because turning
        // somebody's scope on is a change to what their radio shows them.
        Assert.DoesNotContain(
            CivWrites.All,
            w => w.Command == CivConstants.CmdScope
                && (w.SubCommand ?? Array.Empty<byte>()).SequenceEqual(
                    new byte[] { 0x10 }));
    }

    /// <remarks>
    /// Proves the waterfall says what is missing rather than sitting empty. The
    /// answer is four menu screens away on the radio, and an app that looked
    /// broken instead of saying so would send somebody hunting.
    /// </remarks>
    [Fact]
    public void TheWaterfallSaysWhichSettingIsMissing()
    {
        var radio = new RigCapabilities(
            "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
            HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

        var now = new DateTime(2026, 8, 15, 22, 0, 0, DateTimeKind.Utc);

        Assert.Equal(
            ScopeReadyState.NotRead,
            ScopeReadiness.Check(radio, RigState.Empty).State);

        var scopeOff = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.ScopeOn, 0, "off", now, "CI-V 27 10"),
            RigValue.Known(RigField.ScopeOutput, 1, "on", now, "CI-V 27 11"),
        });

        Assert.Equal(ScopeReadyState.ScopeOff, ScopeReadiness.Check(radio, scopeOff).State);

        var outputOff = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.ScopeOn, 1, "on", now, "CI-V 27 10"),
            RigValue.Known(RigField.ScopeOutput, 0, "off", now, "CI-V 27 11"),
        });

        var status = ScopeReadiness.Check(radio, outputOff);

        // **IT IS A THING TO DO, NOT A THING TO REPORT** (HM-DEC-092). This used
        // to name two menu settings as the cause. Neither was among the forty
        // fields Hamlet reads, both were already correct, and the operator walked
        // to the radio for nothing.
        Assert.Equal(ScopeReadyState.OutputOff, status.State);
        Assert.Equal("", status.WhereToLook);
        Assert.Contains("asking it to", status.Detail, StringComparison.Ordinal);
        Assert.Contains("19-7", status.Citation, StringComparison.Ordinal);

        var ready = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.ScopeOn, 1, "on", now, "CI-V 27 10"),
            RigValue.Known(RigField.ScopeOutput, 1, "on", now, "CI-V 27 11"),
        });

        Assert.True(ScopeReadiness.Check(radio, ready).IsReady);

        // And a radio with no scope says that instead (HM-DEC-030).
        Assert.Equal(
            ScopeReadyState.NoScope,
            ScopeReadiness.Check(radio with { HasSpectrumScope = false }, ready).State);
    }

    /// <summary>
    /// The settings read as on, nothing arrives, and the waterfall says so
    /// (HM-DEC-067).
    /// </summary>
    /// <remarks>
    /// The case somebody actually sits and stares at. Both switches report on,
    /// the waterfall stays blank, and until now the app said nothing at all,
    /// which reads as a broken program while the answer is a pair of menu
    /// screens away.
    /// </remarks>
    [Fact]
    public void AWaterfallThatNeverFillsSaysSoAndBlamesNothing()
    {
        var radio = new RigCapabilities(
            "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
            HasUsbAudio: true, CanTransmit: true, new[] { "40 m" });

        var now = new DateTime(2026, 8, 15, 22, 0, 0, DateTimeKind.Utc);

        var ready = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.ScopeOn, 1, "on", now, "CI-V 27 10"),
            RigValue.Known(RigField.ScopeOutput, 1, "on", now, "CI-V 27 11"),
        });

        var silent = ScopeReadiness.Check(radio, ready, sweepsSeen: 0);

        Assert.Equal(ScopeReadyState.NothingArriving, silent.State);
        Assert.False(silent.IsReady);
        Assert.Contains("none of it has", silent.Detail, StringComparison.Ordinal);

        // Named as the radio names them, because a paraphrase sends somebody
        // hunting for a screen that does not exist.
        // **AND IT NAMES NOTHING IT HAS NOT READ** (HM-DEC-092). Everything
        // Hamlet can read says the spectrum should be arriving and none of it
        // has, which is worth saying on its own and is not grounds for pointing
        // at a menu setting nobody has looked at.
        Assert.Equal("", silent.WhereToLook);
        Assert.Contains("19-7", silent.Citation, StringComparison.Ordinal);

        // No fault language anywhere in it. Nothing here is anybody's mistake.
        var said = (silent.Detail + " " + silent.WhereToLook).ToLowerInvariant();
        foreach (var blame in new[]
                 { "you forgot", "error", "failed", "wrong", "invalid", "you must" })
        {
            Assert.False(said.Contains(blame, StringComparison.Ordinal),
                $"the note says '{blame}'");
        }

        // And one sweep is enough to stop saying it.
        Assert.True(ScopeReadiness.Check(radio, ready, sweepsSeen: 1).IsReady);
    }

    /// <summary>
    /// Push one scope frame at the radio's port, as the radio would.
    /// </summary>
    /// <remarks>
    /// Through the real wire path rather than a test hook on the rig, so what is
    /// proved here is the frame handling somebody's radio will actually meet
    /// (HM-DEC-007).
    /// </remarks>
    private static void Publish(FakeSerialPort port, byte[] payload)
    {
        var data = new byte[payload.Length + 1];
        data[0] = CivConstants.ScopeWaveformSub;
        payload.CopyTo(data, 1);

        port.EnqueueIncoming(
            new CivFrame(Controller, Radio, CivConstants.CmdScope, data).ToWireBytes());
    }

    /// <summary>Wait for the read loop to catch up, without a fixed sleep.</summary>
    private static async Task WaitFor(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(5);
        }
    }
}
