using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Hamlet.App.Controls;

/// <summary>Where the achievement quill lives.</summary>
public static class AchievementQuill
{
    /// <summary>The shipped file, linked as an application resource.</summary>
    public const string Uri = "avares://Hamlet.App/Assets/achievement-quill.svg";

    /// <summary>The file's own coordinate space.</summary>
    public const double Side = 44;
}

/// <summary>
/// **The quill in the status bar: outlined at rest, green and orbited when
/// something is unseen.**
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-09**: *"I want to add an achievement icon to the
/// bottom tray. Every time a new achievement is logged... I want that icon to animate
/// like a circle around it that indicates something new has happened. And if you click
/// on that icon, the achievements pop up."*</para>
/// <para>**A FEATHER IN YOUR CAP**, chosen over medals, cups, stars and compasses. The
/// quill is already in the logo, the app icon and the tray, so this is the same object
/// in a third place rather than a new metaphor.</para>
/// <para>**THE TWO STATES DIFFER BY SHAPE AND NOT ONLY BY COLOUR** (§0.6). At rest the
/// vane is an **outline** and there is **no ring**; when something is unseen the vane
/// is **filled** and a **ring is present**. Printed in greyscale the two are still two:
/// one has a circle round it and a solid body, the other has neither. Roughly one man
/// in twelve cannot rely on the hue, and everybody can be looking somewhere else.</para>
/// <para>**THE MOTION ENDS AND THE MARK DOES NOT** (work instruction 300 task 2).
/// Motion in peripheral vision is genuinely unpleasant for some people, so the orbit
/// turns for <see cref="OrbitsFor"/> and then stops - but the fill and the ring stay
/// until the achievements screen is opened. **If the animation cleared the mark, he
/// would never learn what he earned** on any evening he happened to be looking at the
/// radio instead of the screen.</para>
/// <para>**IT IS NOT A REPLACEMENT FOR `BadgeWindow`.** The notice says **what** was
/// earned and leaves after eight seconds; this says **something is unseen** and waits.
/// </para>
/// <para>**IT IS A `Control` AND ITS PARENT IS THE HIT TARGET.** Unit 299 found that
/// `HintMarkControl` draws a ring with no fill and is a bare control, so its middle is
/// not a target; the globe that unit added is a `Border` with a transparent background
/// and is easier to hit. **The markup wraps this in the same shape.**</para>
/// </remarks>
public sealed class AchievementMarkControl : Control
{
    /// <summary>How long one turn of the orbit takes.</summary>
    /// <remarks>
    /// **ABOUT THREE SECONDS A TURN** (the instruction). Fast enough to read as
    /// deliberate motion, slow enough not to strobe at the edge of vision.
    /// </remarks>
    public static readonly TimeSpan Turn = TimeSpan.FromSeconds(3);

    /// <summary>How long the orbit turns before it settles.</summary>
    /// <remarks>
    /// <para>**THIRTY SECONDS, WHICH IS TEN TURNS**, and then it stops moving and
    /// stays lit. **The number is a judgement and it is written down rather than
    /// buried**: long enough that somebody mid-exchange with fifteen seconds to
    /// answer in finishes the slot and still catches it, short enough that it is not
    /// moving in the corner of his eye all evening.</para>
    /// <para>**STOPPING IS NOT CLEARING.** The ring and the fill remain; only the
    /// rotation ends.</para>
    /// </remarks>
    public static readonly TimeSpan OrbitsFor = TimeSpan.FromSeconds(30);

    /// <summary>True where something has been earned that he has not looked at.</summary>
    public static readonly StyledProperty<bool> IsNewProperty =
        AvaloniaProperty.Register<AchievementMarkControl, bool>(nameof(IsNew));

    /// <summary>How far round the orbit is, 0 to 1. Set by the control's own timer.</summary>
    public static readonly StyledProperty<double> PhaseProperty =
        AvaloniaProperty.Register<AchievementMarkControl, double>(nameof(Phase));

    /// <summary>True while the orbit is still turning.</summary>
    public static readonly StyledProperty<bool> IsOrbitingProperty =
        AvaloniaProperty.Register<AchievementMarkControl, bool>(nameof(IsOrbiting));

    private static readonly Lazy<IReadOnlyList<GeometryDrawing>> Quill =
        new(() => SvgMark.Shapes(AchievementQuill.Uri));

    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#6E6E66"));
    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#27490B"));
    private static readonly IBrush Spine = new SolidColorBrush(Color.Parse("#F3EEE1"));

    private readonly DispatcherTimer _timer;

    private DateTime _lit = DateTime.MinValue;

    static AchievementMarkControl()
        => AffectsRender<AchievementMarkControl>(
            IsNewProperty, PhaseProperty, IsOrbitingProperty);

    /// <summary>Creates the mark.</summary>
    public AchievementMarkControl()
    {
        // **FORTY MILLISECONDS IS TWENTY-FIVE FRAMES A SECOND**, which is smooth
        // enough for one small arc and cheap enough to run beside a decoder.
        _timer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(40), DispatcherPriority.Background, OnTick);
    }

    /// <summary>True where something has been earned that he has not looked at.</summary>
    public bool IsNew
    {
        get => GetValue(IsNewProperty);
        set => SetValue(IsNewProperty, value);
    }

    /// <summary>How far round the orbit is, 0 to 1.</summary>
    public double Phase
    {
        get => GetValue(PhaseProperty);
        set => SetValue(PhaseProperty, value);
    }

    /// <summary>True while the orbit is still turning.</summary>
    public bool IsOrbiting
    {
        get => GetValue(IsOrbitingProperty);
        set => SetValue(IsOrbitingProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(20, 20);

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != IsNewProperty)
        {
            return;
        }

        if (IsNew)
        {
            _lit = DateTime.UtcNow;
            IsOrbiting = true;
            _timer.Start();
        }
        else
        {
            Stop();
        }
    }

    /// <summary>Move the orbit on, and settle it once its time is up.</summary>
    /// <remarks>
    /// **THE CLOCK IS WALL TIME AND NOT A FRAME COUNT**, so a session that stalls for
    /// a second does not extend the motion by a second.
    /// </remarks>
    private void OnTick(object? sender, EventArgs e)
    {
        if (!IsNew)
        {
            Stop();
            return;
        }

        if (DateTime.UtcNow - _lit >= OrbitsFor)
        {
            // **SETTLED, NOT CLEARED.** The ring and the fill stay; only the turning
            // stops, and `Phase` is parked at the top so the resting ring has a
            // deliberate gap rather than one wherever the clock happened to be.
            IsOrbiting = false;
            Phase = 0;
            _timer.Stop();

            return;
        }

        Phase = (DateTime.UtcNow - _lit).TotalSeconds % Turn.TotalSeconds
            / Turn.TotalSeconds;
    }

    /// <summary>Bring the orbit to its own end, for a test.</summary>
    /// <remarks>
    /// **IT WINDS THE CLOCK ON RATHER THAN SETTING THE ANSWER**, so what runs is
    /// <see cref="OnTick"/>'s real settling branch. A test that set `IsOrbiting`
    /// itself would prove only that a property can be assigned; this proves that the
    /// code which stops the motion leaves the mark lit.
    /// </remarks>
    internal void SettleForTests()
    {
        _lit = DateTime.UtcNow - OrbitsFor;

        OnTick(this, EventArgs.Empty);
    }

    private void Stop()
    {
        _timer.Stop();
        IsOrbiting = false;
        Phase = 0;
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var side = Math.Min(Bounds.Width, Bounds.Height);
        var middle = new Point(Bounds.Width / 2, Bounds.Height / 2);

        // **THE RING IS PART OF WHAT SAYS *NEW*, AND IT IS A SHAPE** (§0.6). At rest
        // there is no circle at all, which is a difference a greyscale printer keeps.
        if (IsNew)
        {
            var radius = side / 2 - 1;

            context.DrawEllipse(null, new Pen(Green, 1.6), middle, radius, radius);

            if (IsOrbiting)
            {
                // **ONE BEAD GOING ROUND**, which is motion without a spinner: a
                // spinner says *wait*, and nothing here is waiting for anything.
                var angle = Phase * Math.PI * 2 - Math.PI / 2;

                context.DrawEllipse(
                    Green,
                    null,
                    new Point(
                        middle.X + Math.Cos(angle) * radius,
                        middle.Y + Math.Sin(angle) * radius),
                    // **THE BEAD SCALES WITH THE BOX**, which task 5's measurement
                    // is what caught: it was a fixed 2.2 radius, so in a 16 px box
                    // it was a third of the width of the 7 px ring it runs round and
                    // read as a blob rather than as a bead going somewhere.
                    side * 0.11,
                    side * 0.11);
            }
        }

        // **THE QUILL, SCALED INTO WHATEVER ROOM THERE IS.** The vane fills when
        // something is unseen and is an outline when nothing is, which is the second
        // carrier: body or no body.
        var art = side * 0.62;
        var scale = art / AchievementQuill.Side;

        using (context.PushTransform(
            Matrix.CreateScale(scale, scale)
            * Matrix.CreateTranslation(middle.X - art / 2, middle.Y - art / 2)))
        {
            var ink = IsNew ? Edge : Muted;

            foreach (var shape in Quill.Value)
            {
                if (shape.Geometry is null)
                {
                    continue;
                }

                // **A LINE HAS NO INTERIOR**, and SVG's default black fill on one
                // would paint nothing here and something on another backend.
                var line = shape.Geometry is LineGeometry;

                var fill = line
                    ? null
                    : IsNew ? Green : null;

                var pen = shape.Pen is null
                    ? null
                    : new Pen(
                        line && IsNew ? Spine : ink,
                        shape.Pen.Thickness,
                        lineCap: shape.Pen.LineCap,
                        lineJoin: shape.Pen.LineJoin);

                context.DrawGeometry(fill, pen, shape.Geometry);
            }
        }
    }
}
