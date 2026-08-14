using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.Controls;

/// <summary>
/// The decoded Morse, on the dark instrument surface, with every character
/// showing how much the decoder stands behind it.
/// </summary>
/// <remarks>
/// <para>THE SCREEN THIS WHOLE FEATURE EXISTS FOR. Somebody who has held a
/// license for six years and never made a contact is looking at this while a
/// stranger calls CQ. What they read here has to be what was on the air, and
/// where it is not, that has to be visible rather than inferred.</para>
/// <para>Confidence is brightness. Full decode green is a character the decoder
/// stands behind, dimmer is one it does not, and the placeholder is something
/// it heard and could not resolve. None of that is decoration: a beginner
/// reading a line of clean-looking garbage concludes the fault is theirs, which
/// is exactly what they have been told for years, and dimmed text is how the
/// app says otherwise (§0.0).</para>
/// <para>Built the way HM-DEC-006 built the waterfall. The ViewModel holds a
/// <see cref="CwTranscript"/> and this control subscribes to it directly rather
/// than binding a growing string, because at high speed a decode arrives at
/// about forty characters a second and rebuilding the text on each one would
/// re-measure the whole transcript forty times a second. Characters are drained
/// on a timer at the display's pace, and consecutive ones the decoder feels the
/// same way about share a single run, so an ordinary clean decode extends one
/// piece of text rather than adding a hundred.</para>
/// <para>It is a <see cref="SelectableTextBlock"/> rather than something drawn
/// by hand, which is what gives the operator selection and copy for free. A
/// callsign somebody cannot copy out of the terminal is a callsign they have to
/// transcribe by eye, and this is an app for people who find that part hard.</para>
/// </remarks>
public sealed class CwTerminalControl : SelectableTextBlock
{
    /// <summary>How often the screen catches up with the decoder.</summary>
    /// <remarks>
    /// Twenty times a second, which is faster than anybody reads and slow
    /// enough that a burst of fast CW arrives as one update rather than forty.
    /// </remarks>
    private static readonly TimeSpan DrainInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>How long one run of same-confidence text is allowed to get.</summary>
    /// <remarks>
    /// Appending to a run re-measures that run, so an unbounded one would get
    /// slower the longer a clean decode ran. Breaking every so often keeps the
    /// cost flat and costs nothing visually, since the pieces are the same
    /// color and butt up against one another.
    /// </remarks>
    private const int MaximumRunLength = 120;

    /// <summary>The transcript to show.</summary>
    public static readonly StyledProperty<CwTranscript?> TranscriptProperty =
        AvaloniaProperty.Register<CwTerminalControl, CwTranscript?>(nameof(Transcript));

    /// <summary>What to say when nothing has been decoded yet.</summary>
    public static readonly StyledProperty<string> IdleTextProperty =
        AvaloniaProperty.Register<CwTerminalControl, string>(
            nameof(IdleText), "waiting for a signal");

    private readonly DispatcherTimer _timer;
    private readonly List<CwCharacter> _drained = new();

    private Run? _run;
    private CwConfidence _runConfidence = CwConfidence.High;
    private int _runLength;
    private int _characters;
    private int _version = -1;
    private bool _showingIdle;

    /// <summary>Creates the terminal.</summary>
    public CwTerminalControl()
    {
        FontFamily = new FontFamily("Consolas,Menlo,DejaVu Sans Mono,monospace");
        Background = InstrumentPalette.SurfaceBrush;
        Foreground = InstrumentPalette.ConfidentBrush;
        TextWrapping = TextWrapping.Wrap;
        Padding = new Thickness(12, 10);

        _timer = new DispatcherTimer(
            DrainInterval, DispatcherPriority.Background, OnDrainTick);
    }

    /// <summary>The transcript to show.</summary>
    public CwTranscript? Transcript
    {
        get => GetValue(TranscriptProperty);
        set => SetValue(TranscriptProperty, value);
    }

    /// <summary>What to say when nothing has been decoded yet.</summary>
    public string IdleText
    {
        get => GetValue(IdleTextProperty);
        set => SetValue(IdleTextProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ShowIdle();
        _timer.Start();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TranscriptProperty)
        {
            Reset();
        }
        else if (change.Property == IdleTextProperty && _showingIdle)
        {
            ShowIdle();
        }
    }

    /// <summary>
    /// Take whatever the decoder has produced since the last tick and put it on
    /// screen.
    /// </summary>
    private void OnDrainTick(object? sender, EventArgs e)
    {
        var transcript = Transcript;
        if (transcript is null)
        {
            return;
        }

        // A cleared transcript starts the display again, which is how band
        // changes and reconnections avoid running two sessions together.
        if (transcript.Version != _version)
        {
            _version = transcript.Version;
            Reset();
        }

        _drained.Clear();
        if (transcript.Drain(_drained) == 0)
        {
            return;
        }

        if (_showingIdle)
        {
            Inlines?.Clear();
            _showingIdle = false;
            _run = null;
        }

        foreach (var character in _drained)
        {
            AppendCharacter(character);
        }

        TrimHistory();
        ScrollToEndIfFollowing();
    }

    /// <summary>
    /// Add one character, extending the current run when the decoder feels the
    /// same way about it.
    /// </summary>
    private void AppendCharacter(CwCharacter character)
    {
        if (_run is null
            || character.Confidence != _runConfidence
            || _runLength >= MaximumRunLength)
        {
            _run = new Run
            {
                Foreground = InstrumentPalette.For(character.Confidence),
            };

            _runConfidence = character.Confidence;
            _runLength = 0;
            Inlines?.Add(_run);
        }

        _run.Text += character.Text;
        _runLength += character.Text.Length;
        _characters += character.Text.Length;
    }

    /// <summary>
    /// Drop the oldest runs once the transcript is longer than anybody scrolls
    /// back.
    /// </summary>
    private void TrimHistory()
    {
        if (_characters <= CwTranscript.MaximumCharacters || Inlines is null)
        {
            return;
        }

        while (_characters > CwTranscript.MaximumCharacters && Inlines.Count > 1)
        {
            var oldest = Inlines[0];
            Inlines.RemoveAt(0);

            if (oldest is Run run)
            {
                _characters -= run.Text?.Length ?? 0;
            }
        }
    }

    /// <summary>
    /// Follow the decode down the page, unless the operator has scrolled up to
    /// read something.
    /// </summary>
    /// <remarks>
    /// Yanking the view back to the bottom while somebody is reading a callsign
    /// they missed would be the app fighting them, so the scroll only follows
    /// when it is already near the end.
    /// </remarks>
    private void ScrollToEndIfFollowing()
    {
        var scroller = this.FindAncestorOfType<ScrollViewer>();
        if (scroller is null)
        {
            return;
        }

        var distanceFromEnd = scroller.Extent.Height
            - scroller.Offset.Y
            - scroller.Viewport.Height;

        if (distanceFromEnd < 40)
        {
            Dispatcher.UIThread.Post(scroller.ScrollToEnd, DispatcherPriority.Background);
        }
    }

    private void Reset()
    {
        _run = null;
        _runLength = 0;
        _characters = 0;
        ShowIdle();
    }

    /// <summary>
    /// The honest empty state: a switched-on instrument saying it has nothing
    /// yet, rather than a blank rectangle that could mean anything.
    /// </summary>
    private void ShowIdle()
    {
        Inlines?.Clear();
        Inlines?.Add(new Run
        {
            Text = IdleText,
            Foreground = InstrumentPalette.IdleBrush,
        });

        _showingIdle = true;
        _run = null;
    }
}
