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

/// <summary>Which of the three jobs a quill is doing.</summary>
/// <remarks>
/// <para>**§R16, TIM 2026-09-11: THE QUILL HAS TWO FORMS AND NO CAP.** Every
/// station on the list that would earn anything is marked, and the two kinds of
/// earning look different. Before this, every marked row drew the same lit ring
/// and the orbit ran on all of them, so *a new country* and *a whole set opens*
/// were the same picture and he could not tell which row was which.</para>
/// <para>**THE DEFAULT IS <see cref="Tray"/> AND THAT IS DELIBERATE.** The status
/// bar's mark is unit 300's and its behaviour is ruled; a new default would have
/// changed it silently from the other side of the application.</para>
/// </remarks>
public enum AchievementMarkForm
{
    /// <summary>
    /// The status bar mark: a ring while something is unseen, settling after
    /// <see cref="AchievementMarkControl.OrbitsFor"/>.
    /// </summary>
    Tray = 0,

    /// <summary>
    /// A row whose station would earn a counter - a new country, state or grid,
    /// with nothing opening behind it. **A disc filled decode green, the full
    /// height of the row, still, and no ring at all.** It was the quill at 9.9 px
    /// of hairline until unit 327; Tim's word for that was *useless*.
    /// </summary>
    Counter = 1,

    /// <summary>
    /// A row whose station would open a whole set. **A disc filled orange with the
    /// orbit ring around it, turning.** It does not settle: while the station is on
    /// the list, the door is open.
    /// </summary>
    Door = 2,
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
/// <para>**FILLED GREEN AT REST SINCE 2026-09-10** (Tim's ruling, option B of three
/// he was shown). At rest the quill was a grey outline and his word for it was *not
/// noticeable*, so the mark is now the same filled green object at all times and it
/// is about 27 px rather than 20.</para>
/// <para>**THE TWO STATES STILL DIFFER BY SHAPE AND NOT ONLY BY COLOUR** (§0.6), and
/// that survives the change because **the ring was always the carrier**: it is
/// present when something is unseen and absent when nothing is. What used to be a
/// second carrier - outlined against filled - is gone, so the ring is now doing the
/// work alone. It is a shape rather than a hue, so a greyscale printer keeps it, and
/// the orbiting bead adds motion on top while it turns.</para>
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

    /// <summary>Which of the three jobs this quill is doing (§R16).</summary>
    public static readonly StyledProperty<AchievementMarkForm> FormProperty =
        AvaloniaProperty.Register<AchievementMarkControl, AchievementMarkForm>(
            nameof(Form));

    private static readonly Lazy<IReadOnlyList<GeometryDrawing>> Quill =
        new(() => SvgMark.Shapes(AchievementQuill.Uri));

    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#6E6E66"));
    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush Edge = new SolidColorBrush(Color.Parse("#27490B"));
    private static readonly IBrush Spine = new SolidColorBrush(Color.Parse("#F3EEE1"));

    /// <summary>The door's orange: `HmAmber`, the tuning family's title ink.</summary>
    /// <remarks>
    /// <para>**IT IS THE PALETTE'S OWN ORANGE AND IT IS NAMED** (§R16, §0.6,
    /// HM-DEC-032). `#C25E00` is `App.axaml`'s `HmAmber` and
    /// <see cref="PanelPalette.Amber"/>'s `TitleBrush` - the colour the tuning
    /// family already writes its headings in - so a door quill is a hue the
    /// application already uses rather than a new one invented for one mark.</para>
    /// <para>**IT IS NOT THE MUSTARD** (work instruction 325 task 4). `#EDC375`
    /// is `ModePalette.Morse`'s fill, and it is the shade Tim ruled out for the
    /// tray: at 16 px on warm paper it is a pale smear. `#C25E00` is dark enough
    /// to read as a drawn object at that size.</para>
    /// <para>**AND THE COLOUR IS NEVER THE ONLY CARRIER** (§0.6). A door has the
    /// ring and a counter has none, which is a difference in shape that survives
    /// a greyscale print and a colour vision deficiency both.</para>
    /// </remarks>
    private static readonly IBrush DoorInk = new SolidColorBrush(Color.Parse("#C25E00"));

    /// <summary>The darker amber the door's outline is drawn in: `HmAmberDeep`.</summary>
    private static readonly IBrush DoorEdge = new SolidColorBrush(Color.Parse("#9A4A00"));

    private readonly DispatcherTimer _timer;

    private DateTime _lit = DateTime.MinValue;

    static AchievementMarkControl()
        => AffectsRender<AchievementMarkControl>(
            IsNewProperty, PhaseProperty, IsOrbitingProperty, FormProperty);

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

    /// <summary>Which of the three jobs this quill is doing.</summary>
    public AchievementMarkForm Form
    {
        get => GetValue(FormProperty);
        set => SetValue(FormProperty, value);
    }

    /// <summary>True where this mark draws a ring at all.</summary>
    /// <remarks>
    /// **THE RING IS THE SHAPE THAT SEPARATES THE TWO KINDS** (§R16, §0.6). A
    /// counter never draws one; a door always does, whatever the orbit is up to;
    /// the tray draws one exactly while something is unseen, which is unit 300's
    /// ruling and is untouched.
    /// </remarks>
    public bool HasRing => Form switch
    {
        AchievementMarkForm.Counter => false,
        AchievementMarkForm.Door => true,
        _ => IsNew,
    };

    /// <summary>The brush the quill and the ring are drawn in.</summary>
    /// <remarks>
    /// **EXPOSED SO THAT A TEST ASSERTS THE VALUE RATHER THAN A SCREENSHOT.** The
    /// two forms have to be a different hue as well as a different shape (§R16),
    /// and reading the brush off the control is the only way to say which hue
    /// without rendering a bitmap and sampling it.
    /// </remarks>
    public IBrush LitBrush => Form == AchievementMarkForm.Door ? DoorInk : Green;

    /// <summary>**How big a row's mark is, in pixels: the full height of the row.**</summary>
    /// <remarks>
    /// <para>**EIGHTEEN, AND UNIT 325 SHIPPED SIXTEEN, AND THAT IS NOT THE CHANGE** (work
    /// instruction 327 task 3). Tim's words for what 325 shipped were *"ugly and useless"*
    /// and, of the tray's, *"a tiny dot lost in the sea"*. **The size was never the fault.**
    /// Inside a 16 px box the old mark drew the quill at `side * 0.62` - **9.9 px of thin
    /// strokes** - and a 7 px ring; what he was looking at was ten pixels of hairline art.
    /// </para>
    /// <para>**SO THE ROW'S MARK IS A FILLED DISC, WHICH IS WHAT SURVIVES AT ROW HEIGHT**
    /// (the work instruction allows exactly this: *if the quill shape does not survive at
    /// row height, a filled disc does*). Eighteen pixels of solid ink against 9.9 px of
    /// outline is **about thirty times the area**, and it is the row's own height so it
    /// reads as part of the line rather than as a speck beside it.</para>
    /// <para>**THE TRAY IS UNTOUCHED.** <see cref="AchievementMarkForm.Tray"/> still draws
    /// the quill at unit 300's size and unit 325's colors, because that mark's behaviour is
    /// ruled and this is a different surface with a different complaint against it.</para>
    /// </remarks>
    public const double RowSide = 18;

    /// <summary>True where this form draws the row's disc rather than the tray's quill.</summary>
    /// <remarks>
    /// **THE SHAPE IS PER SURFACE AND NOT PER STATE.** A row mark is always a disc and a
    /// tray mark is always a quill; what varies within a row is the color and the ring.
    /// </remarks>
    public bool IsDisc => Form is AchievementMarkForm.Counter or AchievementMarkForm.Door;

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => IsDisc ? new Size(RowSide, RowSide) : new Size(20, 20);

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != IsNewProperty && change.Property != FormProperty)
        {
            return;
        }

        // **A COUNTER NEVER STARTS A TIMER** (§R16). It is the still quill, and on
        // a busy slot there are fourteen of these on the list; fourteen background
        // timers ticking twenty-five times a second to animate nothing is a cost
        // the old *every marked row is new* binding was quietly paying.
        if (Form == AchievementMarkForm.Counter)
        {
            Stop();

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

        // **A DOOR DOES NOT SETTLE** (§R16: *the quill with the orbit ring,
        // spinning*). The tray's thirty seconds exist because that mark sits in
        // the corner of the eye all evening; a door mark lives on a row that is
        // replaced every slot and is on the surface he is reading, and the ruling
        // asks for motion while the station is there.
        if (Form == AchievementMarkForm.Door)
        {
            Phase = (DateTime.UtcNow - _lit).TotalSeconds % Turn.TotalSeconds
                / Turn.TotalSeconds;

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

    /// <summary>
    /// **A row's mark: a disc filled the full height of the row, and a ring for a door.**
    /// </summary>
    /// <param name="context">Where it is drawn.</param>
    /// <param name="side">The smaller of the two bounds.</param>
    /// <param name="middle">The center of the box.</param>
    /// <param name="lit">Green for a counter, orange for a door.</param>
    /// <remarks>
    /// <para>**A DISC FILLED GREEN OR ORANGE IS A MARK, NOT A BAR** (§0.5, HM-DEC-012, and
    /// the work instruction says so in as many words). The family-color rule that keeps
    /// panel headers to text only is about **bars** - a column of filled bars reads as
    /// stripes rather than as structure - and one small filled disc on a row is the object
    /// that rule exists to allow. **The quill is not family color and this is not either**:
    /// green `#3B6D11` is decode green and `#C25E00` is the tuning family's own ink, and
    /// neither is a mode's fill.</para>
    /// <para>**COLOR IS NOT THE ONLY CARRIER AND THE RING IS THE OTHER ONE** (§0.6). A
    /// counter is a bare disc; a door is a disc with a ring around it and a bead going
    /// round. Printed in grey, or read by somebody who cannot tell green from orange, the
    /// two are still two different objects.</para>
    /// <para>**THE DOOR'S DISC IS SMALLER SO THE RING HAS ROOM.** Both marks occupy the same
    /// box and the same height on the row; the door spends some of it on the ring, which is
    /// what makes the ring visible rather than a rim on the disc.</para>
    /// </remarks>
    private void RenderDisc(DrawingContext context, double side, Point middle, IBrush lit)
    {
        var ring = Form == AchievementMarkForm.Door;
        var outer = side / 2 - 1;

        // **THE DISC IS THE MARK AND THE RING IS THE DIFFERENCE.** A counter fills the
        // whole box; a door keeps a third of the radius back for the ring and the gap.
        var radius = ring ? outer * 0.62 : outer;

        var edge = Form == AchievementMarkForm.Door ? DoorEdge : Edge;

        context.DrawEllipse(lit, new Pen(edge, 1), middle, radius, radius);

        if (!ring)
        {
            return;
        }

        context.DrawEllipse(null, new Pen(lit, 1.6), middle, outer, outer);

        if (!IsOrbiting)
        {
            return;
        }

        // **ONE BEAD GOING ROUND**, which is motion without a spinner: a spinner says
        // *wait*, and nothing here is waiting for anything.
        var angle = (Phase * Math.PI * 2) - (Math.PI / 2);

        context.DrawEllipse(
            lit,
            null,
            new Point(
                middle.X + (Math.Cos(angle) * outer),
                middle.Y + (Math.Sin(angle) * outer)),
            side * 0.11,
            side * 0.11);
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

        // **THE TWO FORMS DIFFER IN SHAPE AS WELL AS IN COLOR** (§R16, §0.6). The
        // door has the ring and the counter has none, so the difference survives a
        // greyscale print and does not depend on telling green from orange.
        var lit = LitBrush;

        if (IsDisc)
        {
            RenderDisc(context, side, middle, lit);

            return;
        }

        // **THE RING IS PART OF WHAT SAYS *NEW*, AND IT IS A SHAPE** (§0.6). At rest
        // there is no circle at all, which is a difference a greyscale printer keeps.
        if (HasRing)
        {
            var radius = side / 2 - 1;

            context.DrawEllipse(null, new Pen(lit, 1.6), middle, radius, radius);

            if (IsOrbiting)
            {
                // **ONE BEAD GOING ROUND**, which is motion without a spinner: a
                // spinner says *wait*, and nothing here is waiting for anything.
                var angle = Phase * Math.PI * 2 - Math.PI / 2;

                context.DrawEllipse(
                    lit,
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
            // **FILLED GREEN AT REST, NOT OUTLINED** (Tim's ruling of 2026-09-10,
            // option B of three he was shown). At rest it was a grey sliver and his
            // word for it was *not noticeable*. The quill is now the same green
            // object whether or not something is new.
            var ink = Form == AchievementMarkForm.Door ? DoorEdge : Edge;

            foreach (var shape in Quill.Value)
            {
                if (shape.Geometry is null)
                {
                    continue;
                }

                // **A LINE HAS NO INTERIOR**, and SVG's default black fill on one
                // would paint nothing here and something on another backend.
                var line = shape.Geometry is LineGeometry;

                // **THE VANE IS ALWAYS FILLED NOW**, which is what option B is. A
                // line still has no interior: SVG's default black fill on one would
                // paint nothing here and something on another backend.
                var fill = line ? null : lit;

                var pen = shape.Pen is null
                    ? null
                    : new Pen(
                        line ? Spine : ink,
                        shape.Pen.Thickness,
                        lineCap: shape.Pen.LineCap,
                        lineJoin: shape.Pen.LineJoin);

                context.DrawGeometry(fill, pen, shape.Geometry);
            }
        }
    }
}
