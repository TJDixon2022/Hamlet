using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.App.Tests.Controls;

/// <summary>
/// Rung five of FACT-003: a sweep that parsed becomes pixels.
/// </summary>
/// <remarks>
/// <para>**THE LAST STEP WITH NO INSTRUMENT ON IT.** HM-DEC-093 counts frames
/// received, parsed and rejected, and stops there. Whether a parsed sweep reaches
/// the picture was covered by nothing, and **"the band is quiet" and "nothing was
/// ever drawn" paint the same picture** — which is the exact confusion that
/// ruling exists to end.</para>
/// <para>**THE FRAMES HERE ARE SYNTHESIZED AND SAY SO** (§12.4). The operator's
/// 2026-08-19 session carried 2,748 real scope frames, and **no capture of them
/// exists in this repository**: `tests/fixtures` holds `cw` and nothing else. So
/// these are built to the shape `SpectrumFrame` documents rather than taken off
/// the air, and they prove the path rather than the radio. A capture would be
/// better and is worth taking the next time one is connected (HM-DEC-091).</para>
/// <para>Nothing here asks the radio for anything. HM-DEC-062 stands, the
/// automatic `27 11` is out of the tree, and whether Hamlet may ever request the
/// spectrum is an unruled question this phase is bounded by.</para>
/// </remarks>
public sealed class WaterfallDrawsWhatArrivesTests
{
    /// <summary>A source that produces exactly the sweeps a test hands it.</summary>
    private sealed class Bench : ISpectrumSource
    {
        public bool IsSimulated => true;

        public bool IsRunning { get; private set; }

        public event SpectrumFrameHandler? FrameReady;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Sweep(byte[] bins)
            => FrameReady?.Invoke(
                new SpectrumFrame(
                    7_000_000, 7_300_000,
                    new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc),
                    bins));
    }

    private static (WaterfallControl Control, Bench Source) Wired()
    {
        var source = new Bench();
        var control = new WaterfallControl { Source = source };

        source.Start();
        return (control, source);
    }

    /// <remarks>
    /// Proves the rung: a sweep arrives, and the newest row is no longer the floor
    /// everywhere. Without this, a waterfall that parsed every frame and drew none
    /// would look exactly like a quiet band.
    /// </remarks>
    [AvaloniaFact]
    public void ASweepThatArrivesReachesThePixels()
    {
        var (control, source) = Wired();

        Assert.False(control.EverDrawn, "nothing has arrived yet");

        var bins = new byte[64];
        bins[32] = 200;

        source.Sweep(bins);

        Assert.True(control.EverDrawn);
        Assert.NotEqual(control.FloorPixel, control.NewestRowPixel(32));
    }

    /// <remarks>
    /// Proves the picture is of what was measured (§0.0): a bin with nothing in it
    /// is painted the floor, so an empty band renders as an empty band rather than
    /// as a wash that suggests activity.
    /// </remarks>
    [AvaloniaFact]
    public void AnEmptyBinStaysTheFloor()
    {
        var (control, source) = Wired();

        var bins = new byte[64];
        bins[32] = 200;

        source.Sweep(bins);

        Assert.Equal(control.FloorPixel, control.NewestRowPixel(0));
        Assert.Equal(control.FloorPixel, control.NewestRowPixel(63));
    }

    /// <remarks>
    /// Proves a sweep with no bins at all draws nothing rather than a row of
    /// floor, which would be a picture of a band nobody measured.
    /// </remarks>
    [AvaloniaFact]
    public void ASweepWithNoBinsDrawsNothing()
    {
        var (control, source) = Wired();

        source.Sweep(Array.Empty<byte>());

        Assert.False(control.EverDrawn);
    }

    /// <remarks>
    /// Proves the picture scrolls: a second sweep writes the newest row, so what
    /// is on screen is the sweep that just arrived rather than the first one that
    /// ever did.
    /// </remarks>
    [AvaloniaFact]
    public void TheNewestSweepIsTheOneOnTop()
    {
        var (control, source) = Wired();

        var quiet = new byte[64];
        var loud = new byte[64];
        loud[10] = 240;

        source.Sweep(loud);
        var wasLoud = control.NewestRowPixel(10);

        source.Sweep(quiet);

        Assert.NotEqual(wasLoud, control.NewestRowPixel(10));
        Assert.Equal(control.FloorPixel, control.NewestRowPixel(10));
    }
}
