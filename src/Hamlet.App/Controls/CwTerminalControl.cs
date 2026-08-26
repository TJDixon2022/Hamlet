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

    /// <summary>What the leading edge is reading ahead of the settled pass.</summary>
    public static readonly StyledProperty<string> TipProperty =
        AvaloniaProperty.Register<CwTerminalControl, string>(nameof(Tip), "");

    /// <summary>True when nothing is coming behind the leading edge.</summary>
    public static readonly StyledProperty<bool> TipIsUnstableProperty =
        AvaloniaProperty.Register<CwTerminalControl, bool>(nameof(TipIsUnstable));

    private readonly DispatcherTimer _timer;
    private readonly List<CwCharacter> _drained = new();

    private readonly List<Run> _tipRuns = new();

    /// <summary>What each settled run is, so its ink can be chosen twice.</summary>
    /// <remarks>
    /// A run's colour is decided once when it is written and again whenever the
    /// boundary between history and current copy moves past it, so the
    /// confidence it was drawn for has to outlive the first decision.
    /// </remarks>
    private readonly List<(Run Run, CwConfidence Confidence)> _settledRuns = new();
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

    /// <summary>
    /// The leading edge, running ahead of the settled text (HM-DEC-096).
    /// </summary>
    /// <remarks>
    /// <para>**THE TIP IS DRAWN DIFFERENTLY BECAUSE IT IS A DIFFERENT CLAIM.**
    /// The settled text behind it is what a transcript keeps; this is what the
    /// streaming pass read before anything confirmed it, and it is replaced as
    /// the second pass overtakes it. Drawn the same, a provisional reading would
    /// be a guess presented as a decode (§0.0).</para>
    /// <para>It is a string rather than a stream of characters, and that is
    /// deliberate: it is a handful of characters that is rewritten wholesale
    /// several times a second, which is the opposite shape from the transcript
    /// behind it and would be silly to push through a queue.</para>
    /// </remarks>
    public string Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    /// <summary>True when nothing is coming behind the leading edge.</summary>
    public bool TipIsUnstable
    {
        get => GetValue(TipIsUnstableProperty);
        set => SetValue(TipIsUnstableProperty, value);
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
        else if (change.Property == TipProperty
                 || change.Property == TipIsUnstableProperty)
        {
            DrawTip();
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
    private void OnDrainTick(object? sender, EventArgs e) => Draw();

    /// <summary>
    /// Take whatever the decoder has produced and put it on screen, now.
    /// </summary>
    /// <remarks>
    /// **THE TIMER CALLS THIS AND SO DOES A TEST**, because what the terminal
    /// draws is the thing worth asserting and a test that has to wait for a
    /// timer to tick will either be slow or flaky. It does exactly what the tick
    /// does and nothing else.
    /// </remarks>
    internal void Draw()
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
            // The tip moves even while the settled pass says nothing, which is
            // most of the time: it runs a few seconds behind by design.
            DrawTip();
            return;
        }

        if (_showingIdle)
        {
            Inlines?.Clear();
            _settledRuns.Clear();
            _showingIdle = false;
            _run = null;
        }

        // THE TIP COMES OFF FIRST AND GOES BACK ON AT THE END, so the settled
        // characters always land behind it rather than after it.
        RemoveTip();

        foreach (var character in _drained)
        {
            AppendCharacter(character);
        }

        DrawTip();
        TrimHistory();
        RecedeHistory();
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
            _settledRuns.Add((_run, character.Confidence));
            Inlines?.Add(_run);
        }

        _run.Text += character.Text;
        _runLength += character.Text.Length;
        _characters += character.Text.Length;
    }

    private void RemoveTip()
    {
        foreach (var run in _tipRuns)
        {
            Inlines?.Remove(run);
        }

        _tipRuns.Clear();
    }

    /// <summary>
    /// Put the leading edge back on the end, drawn as what it is.
    /// </summary>
    private void DrawTip()
    {
        if (_showingIdle && Tip.Length == 0)
        {
            return;
        }

        RemoveTip();

        if (Tip.Length == 0 || Inlines is null)
        {
            return;
        }

        if (_showingIdle)
        {
            Inlines.Clear();
            _settledRuns.Clear();
            _showingIdle = false;
            _run = null;
        }

        // **UNSTABLE AND MERELY PROVISIONAL ARE DIFFERENT STATES AND ARE DRAWN
        // DIFFERENTLY.** Provisional means the second pass has not reached this
        // yet; unstable means it has refused, so nothing is coming to confirm
        // it at all. Amber is the app's own color for a reading that could not
        // be resolved, and it is the right one here (§0.6).
        var tipInk = TipIsUnstable
            ? InstrumentPalette.For(CwConfidence.Unreadable)
            : InstrumentPalette.For(CwConfidence.Low);

        // **A PLACEHOLDER KEEPS ITS OWN INK EVEN INSIDE THE TIP.** The whole tip
        // used to be one run in one color, so an unreadable mark in the leading
        // edge came out in the tip's green and read as a solid block of
        // something rather than as the one glyph that means Hamlet could not
        // tell you what was there. Amber is the app's color for that and it is
        // the only carrier of the fact (§0.6, §0.0).
        var at = 0;

        while (at < Tip.Length)
        {
            var placeholder = Tip[at] == MorseAlphabet.Unreadable[0];
            var run = at;

            while (run < Tip.Length
                   && (Tip[run] == MorseAlphabet.Unreadable[0]) == placeholder)
            {
                run++;
            }

            var piece = new Run
            {
                Text = Tip[at..run],
                FontStyle = Avalonia.Media.FontStyle.Italic,
                Foreground = placeholder
                    ? InstrumentPalette.For(CwConfidence.Unreadable)
                    : tipInk,
            };

            Inlines.Add(piece);
            _tipRuns.Add(piece);

            at = run;
        }
    }

    /// <summary>
    /// Drop the oldest runs once the transcript is longer than anybody scrolls
    /// back.
    /// </summary>
    /// <summary>
    /// Push everything but the most recent stretch back into the surface.
    /// </summary>
    /// <remarks>
    /// <para>**THE EYE HAD NOTHING TO LAND ON.** The night of 2026-08-25 ended
    /// with a transcript whose first hundred characters were soup decoded two
    /// minutes earlier, at full strength, sitting above three correctly-read
    /// callsign tokens. Everything was equally bright, so the operator could not
    /// see that Hamlet had read `WB8SC`, `SKSK` and `KE8P` for him.</para>
    /// <para>**RECENT IS THE ONE THIS TREE ALREADY HAS.**
    /// <see cref="CwTranscript.RecentCharacters"/> is two hundred and forty, and
    /// it is there because a couple of hundred characters is several overs at
    /// any speed. Inventing a second notion of recent beside it would be two
    /// answers to one question (§0).</para>
    /// <para>**NOTHING IS DELETED AND EVERYTHING STAYS SELECTABLE.** History
    /// recedes toward the surface and keeps its own hue, so a placeholder is
    /// still amber and an uncertain character still the dimmer green; what
    /// changes is how far forward the text sits. Trimming is a different thing
    /// and <see cref="TrimHistory"/> still does it at four thousand characters.</para>
    /// <para>**THE BOUNDARY IS DRAWN AT A RUN AND NOT INSIDE ONE.** A run that
    /// straddles it stays bright, so a little more than the last two hundred and
    /// forty characters are current. Splitting a run to be exact would mean
    /// rewriting text the operator may be part-way through selecting, for a
    /// boundary nobody can see anyway.</para>
    /// </remarks>
    private void RecedeHistory()
    {
        var behind = 0;

        for (var i = _settledRuns.Count - 1; i >= 0; i--)
        {
            var (run, confidence) = _settledRuns[i];
            var length = run.Text?.Length ?? 0;

            run.Foreground = behind >= CwTranscript.RecentCharacters
                ? InstrumentPalette.HistoryFor(confidence)
                : InstrumentPalette.For(confidence);

            behind += length;
        }
    }

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
                _settledRuns.RemoveAll(kept => ReferenceEquals(kept.Run, run));
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
        _tipRuns.Clear();
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
        _settledRuns.Clear();
        Inlines?.Add(new Run
        {
            Text = IdleText,
            Foreground = InstrumentPalette.IdleBrush,
        });

        _showingIdle = true;
        _run = null;
        _tipRuns.Clear();
    }
}
