using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Training;

namespace Hamlet.App.Controls;

/// <summary>
/// A mode's waterfall fingerprint, alive: a small window onto the same
/// synthesiser that draws the main waterfall.
/// </summary>
/// <remarks>
/// <para>The field guide used to draw each mode's signature as a static
/// glyph. A static glyph can show that FT8 is rectangular, but not that it
/// arrives in synchronised bursts and then stops — and the timing is half of
/// what identifies a mode. Driving these from
/// <see cref="SignalSynthesizer"/> means the picture on the card and the
/// picture on the waterfall are the same picture, so recognising one is
/// recognising the other (HM-DEC-027).</para>
/// <para>Small and cheap: sixty-four bins by forty rows, refreshed twelve
/// times a second, and only while the control is on screen. Six of these on
/// the field-guide panel cost less than the main waterfall alone.</para>
/// </remarks>
public sealed class ModeFingerprintControl : Control
{
    /// <summary>Bins across.</summary>
    private const int Bins = 72;

    /// <summary>Rows of history.</summary>
    private const int Rows = 44;

    /// <summary>The span each fingerprint is drawn over.</summary>
    /// <remarks>
    /// Three kilohertz, so every mode is drawn to the same scale and the
    /// widths are comparable by eye: PSK31's hair and SSB's slab sit in the
    /// same window at the same zoom, which is the comparison the field guide
    /// is making.
    /// </remarks>
    private const long SpanHz = 3_000;

    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(83);
    private static readonly long CenterHz = 7_040_000;

    /// <summary>Which mode to animate.</summary>
    public static readonly StyledProperty<SignatureKind> KindProperty =
        AvaloniaProperty.Register<ModeFingerprintControl, SignatureKind>(nameof(Kind));

    private readonly int[] _palette = WaterfallPalette.Lookup();
    private readonly int[] _pixels = new int[Bins * Rows];
    private readonly byte[] _bins = new byte[Bins];
    private readonly DispatcherTimer _timer;

    private WriteableBitmap? _bitmap;
    private SignalSynthesizer? _synth;
    private TimeSpan _elapsed;

    static ModeFingerprintControl()
    {
        AffectsRender<ModeFingerprintControl>(KindProperty);
    }

    /// <summary>Creates the fingerprint.</summary>
    public ModeFingerprintControl()
    {
        _timer = new DispatcherTimer(Tick, DispatcherPriority.Background, OnTick);
    }

    /// <summary>Which mode to animate.</summary>
    public SignatureKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>
    /// The training mode a field-guide signature stands for.
    /// </summary>
    /// <param name="kind">The signature archetype.</param>
    /// <returns>The mode to synthesise.</returns>
    /// <remarks>
    /// JS8 shares FT8's machinery and therefore its fingerprint, which is why
    /// the field guide gives them the same archetype and this gives them the
    /// same synthesis.
    /// </remarks>
    public static TrainingMode ModeFor(SignatureKind kind) => kind switch
    {
        SignatureKind.Dots => TrainingMode.Cw,
        SignatureKind.Blocks => TrainingMode.Ft8,
        SignatureKind.Rails => TrainingMode.Rtty,
        SignatureKind.Ribbon => TrainingMode.Psk31,
        SignatureKind.Smear => TrainingMode.Ssb,
        _ => TrainingMode.Cw,
    };

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(82, 40);

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Reset();
        _timer.Start();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        if (_bitmap is null)
        {
            _bitmap = new WriteableBitmap(
                new PixelSize(Bins, Rows), new Vector(96, 96),
                PixelFormat.Bgra8888, AlphaFormat.Opaque);
        }

        using (var buffer = _bitmap.Lock())
        {
            Marshal.Copy(_pixels, 0, buffer.Address, _pixels.Length);
        }

        context.DrawImage(
            _bitmap,
            new Rect(0, 0, Bins, Rows),
            new Rect(0, 0, Bounds.Width, Bounds.Height));
    }

    private void Reset()
    {
        var mode = ModeFor(Kind);

        // One signal, centred, at full strength: the card is showing what the
        // mode looks like, not how a band looks.
        var signal = new SyntheticSignal(
            mode, CenterHz, 0.95, WordsPerMinute: 16, PhaseOffset: 0);

        _synth = new SignalSynthesizer(
            new[] { signal },
            CenterHz - (SpanHz / 2),
            CenterHz + (SpanHz / 2),
            seed: 4242);

        _elapsed = TimeSpan.Zero;

        var floor = _palette[0];
        for (var i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = floor;
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_synth is null)
        {
            Reset();
        }

        _elapsed += Tick;
        _synth!.Render(_elapsed, _bins);

        Array.Copy(_pixels, 0, _pixels, Bins, Bins * (Rows - 1));

        for (var x = 0; x < Bins; x++)
        {
            var index = (int)Math.Clamp(_bins[x] * 1.5, 0, WaterfallPalette.Size - 1);
            _pixels[x] = _palette[index];
        }

        InvalidateVisual();
    }
}
