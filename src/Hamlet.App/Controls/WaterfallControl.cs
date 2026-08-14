using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Hamlet.RadioEngine.Training;

namespace Hamlet.App.Controls;

/// <summary>
/// The waterfall: one row of spectrum per frame, scrolling down.
/// </summary>
/// <remarks>
/// <para>Built the way HM-DEC-006 requires. The control owns a
/// <see cref="WriteableBitmap"/> and subscribes to the engine's spectrum
/// event directly; spectrum data never travels through data binding. At
/// twenty-five frames a second, five hundred bins each, routing pixels
/// through <c>INotifyPropertyChanged</c> would be allocation churn and
/// stutter. The ViewModel carries settings — the span, the marker, the
/// command — and nothing that changes per frame.</para>
/// <para>Threading. Frames arrive on the source's timer thread, which must
/// never touch Avalonia. So the frame handler does nothing but write ints
/// into a plain array under a short lock, and a UI-side timer copies that
/// array into the bitmap and invalidates. Two memory copies per frame, no
/// allocation on either path, and no cross-thread access to a control.</para>
/// <para>Scrolling is a single block move of the pixel buffer down one row,
/// then the new row written at the top. Around half a megabyte a frame, which
/// a memory move handles in well under a millisecond — cheap enough that the
/// simplicity is worth more than a ring buffer's saved copy.</para>
/// <para>The x axis is the band, computed exactly as the dial tape and the
/// neighborhood map compute theirs, so the same x means the same frequency in
/// all three. Clicking tunes there: the phase 2 click-a-signal gesture,
/// built early because the training radio makes it useful now.</para>
/// </remarks>
public sealed class WaterfallControl : Control
{
    /// <summary>Rows of history kept on screen.</summary>
    public const int HistoryRows = 240;

    /// <summary>How often the bitmap is refreshed from the pixel buffer.</summary>
    private static readonly TimeSpan BlitInterval = TimeSpan.FromMilliseconds(33);

    private static readonly IBrush MarkerBrush = new SolidColorBrush(Color.Parse("#FF9E3D"));
    private static readonly IBrush IdleBrush = new SolidColorBrush(Color.Parse("#0B0F16"));
    private static readonly IBrush IdleTextBrush = new SolidColorBrush(Color.Parse("#5C7285"));
    private static readonly Typeface Mono = new("Consolas,Menlo,monospace");

    /// <summary>The engine source to draw. Setting it re-subscribes.</summary>
    public static readonly StyledProperty<ISpectrumSource?> SourceProperty =
        AvaloniaProperty.Register<WaterfallControl, ISpectrumSource?>(nameof(Source));

    /// <summary>Current frequency in hertz; drawn as the marker.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<WaterfallControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<WaterfallControl, long>(nameof(BandLowHz), 7_000_000);

    /// <summary>Band upper edge in hertz.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<WaterfallControl, long>(nameof(BandHighHz), 7_300_000);

    /// <summary>Display gain, 0.5 to 3. A ViewModel setting, not per-frame data.</summary>
    public static readonly StyledProperty<double> GainProperty =
        AvaloniaProperty.Register<WaterfallControl, double>(nameof(Gain), 1.35);

    /// <summary>Executed with a frequency in hertz when the operator clicks.</summary>
    public static readonly StyledProperty<ICommand?> TuneCommandProperty =
        AvaloniaProperty.Register<WaterfallControl, ICommand?>(nameof(TuneCommand));

    private readonly object _pixelLock = new();
    private readonly int[] _palette = WaterfallPalette.Lookup();
    private readonly DispatcherTimer _blitTimer;

    private int[] _pixels = Array.Empty<int>();
    private WriteableBitmap? _bitmap;
    private ISpectrumSource? _subscribed;
    private int _width;
    private bool _dirty;
    private bool _everReceived;

    /// <summary>
    /// Gain, mirrored into a plain field.
    /// </summary>
    /// <remarks>
    /// The frame handler runs on the source's thread, and reading an Avalonia
    /// property off the UI thread trips its thread check. The exception would
    /// surface nowhere — the source swallows faults so one bad frame cannot
    /// kill its timer — so the waterfall would simply stay black forever with
    /// nothing logged. Mirroring the value on the UI thread keeps the hot
    /// path to plain field reads, which is what HM-DEC-006 wanted anyway.
    /// </remarks>
    private volatile float _gain = 1.35f;

    static WaterfallControl()
    {
        AffectsRender<WaterfallControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty);
    }

    /// <summary>Creates the waterfall.</summary>
    public WaterfallControl()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        ClipToBounds = true;

        _blitTimer = new DispatcherTimer(
            BlitInterval, DispatcherPriority.Render, OnBlitTick);
    }

    /// <summary>The engine source being drawn.</summary>
    public ISpectrumSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Current frequency in hertz.</summary>
    public long FrequencyHz
    {
        get => GetValue(FrequencyHzProperty);
        set => SetValue(FrequencyHzProperty, value);
    }

    /// <summary>Band lower edge in hertz.</summary>
    public long BandLowHz
    {
        get => GetValue(BandLowHzProperty);
        set => SetValue(BandLowHzProperty, value);
    }

    /// <summary>Band upper edge in hertz.</summary>
    public long BandHighHz
    {
        get => GetValue(BandHighHzProperty);
        set => SetValue(BandHighHzProperty, value);
    }

    /// <summary>Display gain.</summary>
    public double Gain
    {
        get => GetValue(GainProperty);
        set => SetValue(GainProperty, value);
    }

    /// <summary>Click-to-tune command; the parameter is a frequency in hertz.</summary>
    public ICommand? TuneCommand
    {
        get => GetValue(TuneCommandProperty);
        set => SetValue(TuneCommandProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourceProperty)
        {
            Resubscribe(change.GetOldValue<ISpectrumSource?>(), Source);
        }
        else if (change.Property == GainProperty)
        {
            _gain = (float)Math.Clamp(Gain, 0.2, 4.0);
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Resubscribe(null, Source);
        _blitTimer.Start();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _blitTimer.Stop();
        Resubscribe(_subscribed, null);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var rect = new Rect(0, 0, w, h);

        if (_bitmap is null || !_everReceived)
        {
            // Honest empty state: a dark, switched-on instrument saying it
            // has nothing yet, rather than a blank rectangle.
            context.FillRectangle(IdleBrush, rect);

            var text = new FormattedText(
                "no spectrum yet, so connect a radio or pick the training radio",
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                Mono, 12, IdleTextBrush);
            context.DrawText(text,
                new Point((w - text.Width) / 2, (h - text.Height) / 2));
            return;
        }

        context.DrawImage(_bitmap, new Rect(0, 0, _width, HistoryRows), rect);

        if (Axis.IsUsable)
        {
            context.FillRectangle(
                MarkerBrush, new Rect(Axis.XOf(FrequencyHz) - 0.5, 0, 1, h));
        }
    }

    /// <summary>
    /// The band laid across the width, from the one definition the map and the
    /// dial tape also read. Same x, same frequency, on all three.
    /// </summary>
    private FrequencyAxis Axis
        => FrequencyAxis.Across(BandLowHz, BandHighHz, Bounds.Width);

    /// <summary>
    /// Swap the subscription, keeping exactly one live at a time.
    /// </summary>
    private void Resubscribe(ISpectrumSource? old, ISpectrumSource? next)
    {
        if (ReferenceEquals(_subscribed, next))
        {
            return;
        }

        if (_subscribed is not null)
        {
            _subscribed.FrameReady -= OnFrameReady;
        }

        if (old is not null && !ReferenceEquals(old, _subscribed))
        {
            old.FrameReady -= OnFrameReady;
        }

        _subscribed = next;

        if (_subscribed is not null)
        {
            _subscribed.FrameReady += OnFrameReady;
        }
        else
        {
            _everReceived = false;
        }
    }

    /// <summary>
    /// One frame from the engine, on the source's own thread.
    /// </summary>
    /// <remarks>
    /// Deliberately does no Avalonia work and allocates nothing: it moves the
    /// buffer down a row, maps bins through the palette into the top row, and
    /// returns. Anything heavier here would land on the timer thread that
    /// feeds every other subscriber.
    /// </remarks>
    private void OnFrameReady(in SpectrumFrame frame)
    {
        var bins = frame.Bins;
        if (bins.Length == 0)
        {
            return;
        }

        lock (_pixelLock)
        {
            EnsureBuffer(bins.Length);

            // Scroll: everything moves down one row.
            Array.Copy(_pixels, 0, _pixels, _width, _width * (HistoryRows - 1));

            var gain = _gain;
            for (var x = 0; x < _width; x++)
            {
                var level = bins[x] * gain;
                var index = (int)Math.Clamp(level, 0, WaterfallPalette.Size - 1);
                _pixels[x] = _palette[index];
            }

            _dirty = true;
            _everReceived = true;
        }
    }

    private void EnsureBuffer(int width)
    {
        if (_width == width && _pixels.Length == width * HistoryRows)
        {
            return;
        }

        _width = width;
        _pixels = new int[width * HistoryRows];

        var floor = _palette[0];
        for (var i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = floor;
        }
    }

    /// <summary>
    /// Copy the pixel buffer into the bitmap and ask for a redraw. On the UI
    /// thread, at the display's pace rather than the source's.
    /// </summary>
    private void OnBlitTick(object? sender, EventArgs e)
    {
        int width;

        lock (_pixelLock)
        {
            if (!_dirty || _width == 0)
            {
                return;
            }

            width = _width;
            EnsureBitmap(width);

            using (var buffer = _bitmap!.Lock())
            {
                Marshal.Copy(_pixels, 0, buffer.Address, width * HistoryRows);
            }

            _dirty = false;
        }

        InvalidateVisual();
    }

    private void EnsureBitmap(int width)
    {
        if (_bitmap is not null && _bitmap.PixelSize.Width == width)
        {
            return;
        }

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(
            new PixelSize(width, HistoryRows),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!Axis.IsUsable)
        {
            return;
        }

        var hz = Axis.HzAtClamped(e.GetPosition(this).X) / 100 * 100;

        if (TuneCommand?.CanExecute(hz) == true)
        {
            TuneCommand.Execute(hz);
        }
        else
        {
            SetCurrentValue(FrequencyHzProperty, hz);
        }

        e.Handled = true;
    }
}
