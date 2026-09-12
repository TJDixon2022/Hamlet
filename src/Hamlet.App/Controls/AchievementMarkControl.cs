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
    /// with nothing opening behind it. **The tray's vane at row scale, drawn in
    /// decode green as a hairline outline, still, and no ring at all.** It was an
    /// 18 px filled disc under unit 327; Tim's word for that was *so so so so so
    /// ugly*, and he chose this from three treatments on 2026-09-12.
    /// </summary>
    Counter = 1,

    /// <summary>
    /// A row whose station would open a whole set. **The same vane in orange,
    /// inside a hairline ring, with a bead going round it.** It does not settle:
    /// while the station is on the list, the door is open.
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

    /// <summary>**The vane's own bounds in the file's 44-unit space.**</summary>
    /// <remarks>
    /// **THE DRAWN QUILL IS MEASURED FROM THE FILE AND NOT FROM THE VIEWBOX** (work
    /// instruction 330 task 1). The vane spans 28 of the file's 44 units and sits in the
    /// middle of them, so scaling the viewBox to a fraction of the box - which is what
    /// units 300 to 303 did - draws a quill 64 per cent of the size the number in the
    /// markup says. Scaling the vane's own height is the only way the figure on the glass
    /// is the figure that was ruled, and it stays true if somebody edits the SVG.
    /// </remarks>
    private static readonly Lazy<Rect> VaneBounds = new(() => Quill.Value
        .Select(s => s.Geometry)
        .OfType<Geometry>()
        .FirstOrDefault(g => g is not LineGeometry)
        ?.Bounds
        ?? new Rect(0, 0, AchievementQuill.Side, AchievementQuill.Side));

    /// <summary>The tray's own vane, scaled to <see cref="RowVane"/> px tall.</summary>
    /// <remarks>
    /// <para>**IT IS THE SAME PATH THE TRAY DRAWS AND NOTHING TRANSCRIBES IT** (§0, and
    /// `SvgMark`'s own rule: the file is the mark). The row mark is the tray mark at another
    /// size, which is the whole of Tim's treatment A - *the quill he already knows* - and a
    /// second copy of that path in code would be a second drawing to drift.</para>
    /// <para>**THE SCALE IS COMPUTED FROM THE FILE AND NOT TYPED IN.** The vane spans 28 of
    /// the file's 44 units, so twelve pixels is 0.4286 - but that arithmetic is done here
    /// against the geometry's own bounds, so editing the SVG moves the drawing and leaves the
    /// mark 12 px tall. Typing 0.4286 would have hard-coded a fact about a file.</para>
    /// <para>**ITS OWN COPY OF THE SHAPES, NOT <see cref="Quill"/>'S.** A `Geometry`'s
    /// transform is part of the object, so scaling the instance the tray draws would shrink
    /// the tray's mark from the other side of the application - the exact class of accident
    /// §R16's *the default is Tray and that is deliberate* was written about.</para>
    /// </remarks>
    private static readonly Lazy<Geometry> RowVaneArt = new(() =>
    {
        var vane = SvgMark.Shapes(AchievementQuill.Uri)
            .Select(s => s.Geometry)
            .OfType<Geometry>()

            // **A LINE HAS NO INTERIOR AND IS NOT THE VANE.** The file's second shape is
            // the spine, which is drawn in paper white over a filled vane; over an unfilled
            // one it would be a white scratch, so a row draws the vane alone.
            .FirstOrDefault(g => g is not LineGeometry)
            ?? throw new InvalidOperationException(
                AchievementQuill.Uri + " has no vane path to draw a row's mark from");

        var tall = vane.Bounds.Height;

        if (tall <= 0)
        {
            throw new InvalidOperationException(
                AchievementQuill.Uri + " draws a vane with no height");
        }

        var scale = RowVane / tall;

        vane.Transform = new MatrixTransform(Matrix.CreateScale(scale, scale));

        return vane;
    });

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

    /// <summary>**How tall the quill this mark draws is, on the glass, right now.**</summary>
    /// <remarks>
    /// **A TEST ASSERTS THE INK AND NOT THE BOX** (work instruction 330 task 1). The two
    /// have been different since unit 300 and nothing measured the difference: three tests
    /// checked the box was 27 and the drawing was 10.6 the whole time. This is what a
    /// reader would put a ruler against, computed from the bounds the layout gave.
    /// </remarks>
    public double DrawnQuillHeight => IsRowVane
        ? RowVane
        : Math.Min(Bounds.Width, Bounds.Height) * TrayFraction;

    /// <summary>The brush the quill and the ring are drawn in.</summary>
    /// <remarks>
    /// **EXPOSED SO THAT A TEST ASSERTS THE VALUE RATHER THAN A SCREENSHOT.** The
    /// two forms have to be a different hue as well as a different shape (§R16),
    /// and reading the brush off the control is the only way to say which hue
    /// without rendering a bitmap and sampling it.
    /// </remarks>
    public IBrush LitBrush => Form == AchievementMarkForm.Door ? DoorInk : Green;

    /// <summary>**How tall the tray's drawn quill is, in pixels: the ruled 27.**</summary>
    /// <remarks>
    /// <para>**TWENTY-SEVEN IS UNITS 300 TO 303'S OWN NUMBER AND IT WAS NEVER ON THE
    /// GLASS** (work instruction 330 task 1). Tim was shown three treatments on
    /// 2026-09-10 and chose option B - *about 27 px, filled green* - and 27 went into
    /// `HmStatusMarkSize`, which is the size of the **box**. Inside that box the renderer
    /// scaled the file's whole 44-unit viewBox to 62 per cent of the box, and the vane is
    /// 28 of those 44 units, so what actually rendered was 27 x 0.62 x 28 / 44 = **10.6 px
    /// of quill, 5.3 px wide**. His word for it on 2026-09-12 was *tiny and hard to
    /// notice*, which is the same complaint he made on 2026-09-09 about the 20 px box, and
    /// it is the same complaint because neither answer moved the drawing.</para>
    /// <para>**THIS NUMBER MEASURES THE DRAWING** (unit 328 made the same correction one
    /// surface down for <see cref="RowVane"/>). It is the height of the vane on the glass,
    /// and it matches the count badge beside it - `HmStatusMarkSize`, still 27 - so the
    /// quill and the number read as two objects of one size rather than as an icon parked
    /// next to a pill.</para>
    /// </remarks>
    public const double TrayQuill = 27;

    /// <summary>**How big the tray's mark box is, in pixels: 32, to hold the ring.**</summary>
    /// <remarks>
    /// <para>**THE BOX IS BIGGER THAN THE DRAWING BECAUSE THE RING GOES ROUND IT** (§0.6:
    /// the ring is the shape that carries *something unseen*, and it has to be a ring and
    /// not a line through the quill). The orbit is drawn at `side / 2 - 1`, so a 32 px box
    /// gives it a radius of 15 against a vane whose tip is 13.5 px from the middle - a
    /// pixel and a half of air, and the bead at `side * 0.11` runs round the outside of the
    /// drawing rather than across it.</para>
    /// <para>**IT IS A SECOND NUMBER AND THAT IS THE POINT.** Units 300 to 303 had one, and
    /// one number cannot be both the size of the ink and the room the ring needs; that is
    /// how the ruled 27 became 10.6. The two are named separately and the fraction between
    /// them is computed, so the drawing scales with the box at any size the control is
    /// given.</para>
    /// </remarks>
    public const double TraySide = 32;

    /// <summary>How much of the tray's box the drawn quill takes, top to bottom.</summary>
    private const double TrayFraction = TrayQuill / TraySide;

    /// <summary>**How big a row's mark box is, in pixels: the full height of the row.**</summary>
    /// <remarks>
    /// <para>**EIGHTEEN, AND IT IS THE BOX AND NOT THE DRAWING** (work instruction 328 task
    /// 2). It is the hit target the press needs - the mark sits in a `Button` with a hand
    /// cursor - and the room the door's ring needs, and it is the row's own height so the
    /// gutter is a fixed column down the left of the list rather than a ragged one.</para>
    /// <para>**WHAT CHANGED IN 328 IS THE INK IN IT AND NOT ITS SIZE.** Unit 327 filled all
    /// eighteen pixels of it with a solid disc; this draws <see cref="RowVane"/> px of
    /// hairline vane in the middle of the same box. Nothing on the row moves, so the list
    /// does not reflow for a change of mark.</para>
    /// <para>**THE TRAY IS UNTOUCHED.** <see cref="AchievementMarkForm.Tray"/> still draws
    /// the whole quill - vane, spine and fill - at unit 300's size and unit 325's colors,
    /// because that mark's behaviour is ruled and this is a different surface with a
    /// different complaint against it.</para>
    /// </remarks>
    public const double RowSide = 18;

    /// <summary>**How tall the drawn vane is on a row, in pixels.**</summary>
    /// <remarks>
    /// <para>**TWELVE, AND THE BOX STAYS AT EIGHTEEN** (work instruction 328 task 2, Tim's
    /// treatment A of 2026-09-12). The box is the hit target and the room the door's ring
    /// needs; the vane is the ink inside it, and it is **shorter than the row on purpose** so
    /// it reads as a thin object in the gutter rather than as a bullet the height of the
    /// line.</para>
    /// <para>**IT IS THE VANE AND NOT THE BOX THAT THIS NUMBER MEASURES**, which is the
    /// mistake units 325 and 327 made in opposite directions. 325 said 16 px and drew 9.9 px
    /// of hairline inside it; 327 said 18 px and filled all of it. This says twelve and draws
    /// twelve, because the scale is computed from the vane's own height in the file rather
    /// than from the size of the box it sits in.</para>
    /// </remarks>
    public const double RowVane = 12;

    /// <summary>**How thick every stroke on a row's mark is, in pixels.**</summary>
    /// <remarks>
    /// **ONE HAIRLINE WEIGHT FOR THE VANE AND THE RING BOTH** (the work instruction: *a
    /// hairline stroke, about 12 px tall, 1.5 px wide, not filled*). Two weights on one mark
    /// this small is a difference nobody can see and a second number to keep in step.
    /// </remarks>
    public const double RowHairline = 1.5;

    /// <summary>True where this form draws the row's vane rather than the tray's whole quill.</summary>
    /// <remarks>
    /// **THE SHAPE IS PER SURFACE AND NOT PER STATE.** A row mark is always the vane and a
    /// tray mark is always the full quill at the tray's own size; what varies within a row is
    /// the color and the ring.
    /// </remarks>
    public bool IsRowVane => Form is AchievementMarkForm.Counter or AchievementMarkForm.Door;

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => IsRowVane ? new Size(RowSide, RowSide) : new Size(TraySide, TraySide);

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
    /// **A row's mark: the tray's vane, 12 px tall and hairline, in a ring for a door.**
    /// </summary>
    /// <param name="context">Where it is drawn.</param>
    /// <param name="side">The smaller of the two bounds.</param>
    /// <param name="middle">The center of the box.</param>
    /// <param name="lit">Green for a counter, orange for a door.</param>
    /// <remarks>
    /// <para>**TIM'S TREATMENT A, 2026-09-12: A THIN QUILL IN THE GUTTER.** He was drawn
    /// three treatments of the row mark and chose this one. **What it replaces is unit 327's
    /// 18 px disc filled solid**, which he saw and called *"so so so so so ugly"* - and the
    /// disc had itself been the answer to unit 325's 9.9 px of hairline art. The thing that
    /// was wrong both times was the shape, not the size: a mark the height of the line and
    /// filled is a bullet, and a mark drawn at two thirds of a 16 px box is a smudge.</para>
    /// <para>**IT IS NOT FILLED, AND THAT IS THE RULING AND NOT A PREFERENCE.** The vane is
    /// a stroked outline at <see cref="RowHairline"/> px, so the row reads as a line of text
    /// with a small drawn object in its gutter rather than as a list with a column of dots
    /// down the left of it (§0.5's *a column of filled bars reads as stripes*, one level
    /// down).</para>
    /// <para>**COLOR IS NOT THE ONLY CARRIER AND THE RING IS THE OTHER ONE** (§0.6). A
    /// counter is the bare vane; a door is the same vane inside a hairline ring, with a bead
    /// going round it. Printed in grey, or read by somebody who cannot tell green from
    /// orange, the two are still two different objects - and the ring is drawn **first**, so
    /// the vane is the thing on top rather than something the ring crosses.</para>
    /// <para>**THE VANE IS THE SAME SIZE ON BOTH.** Unit 327's door shrank its disc to 62%
    /// to make room for the ring; the vane measures 6.0 x 12.0 px, so its furthest corner is
    /// 6.7 px from the center and the ring's radius is 8 - nothing has to shrink, and the two
    /// forms carry the same object at the same size.</para>
    /// </remarks>
    private void RenderRowVane(DrawingContext context, double side, Point middle, IBrush lit)
    {
        var outer = side / 2 - 1;

        if (Form == AchievementMarkForm.Door)
        {
            context.DrawEllipse(null, new Pen(lit, RowHairline), middle, outer, outer);

            if (IsOrbiting)
            {
                // **ONE BEAD GOING ROUND**, which is motion without a spinner: a spinner
                // says *wait*, and nothing here is waiting for anything. **The turn is
                // unit 327's and is kept exactly as it built it** (the work instruction:
                // *the turn stays as 327 built it*).
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
        }

        var art = RowVaneArt.Value;
        var bounds = art.Bounds;

        // **A TRANSLATION AND NOT A SCALE**, because the scale is already in the geometry.
        // It matters to more than tidiness: a pen inside a scale transform is drawn at
        // thickness times scale, so a hairline asked for in a 0.43 transform would arrive
        // at 0.64 px and disappear on the surface it is meant to be visible on.
        using (context.PushTransform(
            Matrix.CreateTranslation(
                middle.X - bounds.Center.X, middle.Y - bounds.Center.Y)))
        {
            context.DrawGeometry(
                null,
                new Pen(lit, RowHairline, lineJoin: PenLineJoin.Round),
                art);
        }
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

        if (IsRowVane)
        {
            RenderRowVane(context, side, middle, lit);

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

        // **THE QUILL, SCALED SO THAT THE VANE IS THE RULED HEIGHT** (work instruction
        // 330 task 1). What this replaced scaled the file's whole 44-unit viewBox to 62
        // per cent of the box, and the vane is 28 of those 44 units, so a 27 px box drew
        // 10.6 px of quill and the ruled number was never on the glass. The scale is
        // taken off the vane's own bounds in the file, so editing the SVG moves the
        // drawing and leaves the mark <see cref="TrayQuill"/> px tall.
        var vane = VaneBounds.Value;
        var scale = side * TrayFraction / vane.Height;

        using (context.PushTransform(
            Matrix.CreateTranslation(-vane.Center.X, -vane.Center.Y)
            * Matrix.CreateScale(scale, scale)
            * Matrix.CreateTranslation(middle.X, middle.Y)))
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
