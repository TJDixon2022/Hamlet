using Avalonia.Media;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.Controls;

/// <summary>One mode family's fill and ink.</summary>
/// <param name="Family">The family these colors stand for.</param>
/// <param name="Label">What the legend calls it.</param>
/// <param name="Fill">Background wash for a segment or a card.</param>
/// <param name="Ink">Text and marks drawn on that fill.</param>
public sealed record ModeColors(ModeFamily Family, string Label, Color Fill, Color Ink)
{
    /// <summary>The fill as a brush, built once.</summary>
    public IBrush FillBrush { get; } = new SolidColorBrush(Fill);

    /// <summary>The ink as a brush, built once.</summary>
    public IBrush InkBrush { get; } = new SolidColorBrush(Ink);
}

/// <summary>
/// The mode-color language: four families, fixed colors, one definition.
/// </summary>
/// <remarks>
/// <para>THE STANDING RULE (CLAUDE.md §0.6, HM-DEC-032). Mode families have
/// the same colors on every surface — the neighborhood map, the field guide,
/// the waterfall, and anything built later. Two copies of a language are two
/// languages, so both controls read from here and nothing carries a color
/// literal of its own.</para>
/// <para>The old map fills separated by lightness alone: a pale amber beside a
/// pale pink read as one wash at a glance, and pink was doing double duty as
/// both "phone segment" and, under hatching, "listen only". These separate by
/// hue and temperature as well, so the four families are distinguishable
/// without reading the labels — and none of them is pink, so the listen-only
/// hatch means one thing again.</para>
/// <para>COLOR IS NEVER THE ONLY CARRIER. Roughly one man in twelve has a
/// color vision deficiency, and this hobby's demographics make that a real
/// slice of the people who will use this. So the map labels every segment, the
/// legend names every family in words, the listen-only veil hatches as well as
/// tints, and the band cards carry an icon and a width alongside their hue.
/// Anything added later inherits that obligation.</para>
/// </remarks>
public static class ModePalette
{
    /// <summary>Morse.</summary>
    public static ModeColors Cw { get; } = new(
        ModeFamily.Cw, "Morse", Color.Parse("#EDC375"), Color.Parse("#5E3800"));

    /// <summary>RTTY, PSK31, FT8 and the rest.</summary>
    public static ModeColors Digital { get; } = new(
        ModeFamily.Digital, "Digital", Color.Parse("#BFB6E4"), Color.Parse("#2B2360"));

    /// <summary>Voice.</summary>
    public static ModeColors Phone { get; } = new(
        ModeFamily.Phone, "Voice", Color.Parse("#A3CBE8"), Color.Parse("#0B3B5C"));

    /// <summary>Open space, or a mixture.</summary>
    /// <remarks>
    /// The ink was #6E6A61 and reached only 4.09:1 against this fill, short of
    /// WCAG AA's 4.5 for normal text (HM-DEC-036). Darkening it to #5F5C53
    /// clears the bar at 5.07:1 and costs nothing: this is the least colorful
    /// of the four and its label is the one most often read at a glance. The
    /// hobby skews old enough that eyes which need the contrast are not an
    /// edge case here — they are a large share of the people who will use this.
    /// </remarks>
    public static ModeColors Open { get; } = new(
        ModeFamily.Open, "Open / mixed", Color.Parse("#E4E0D5"), Color.Parse("#5F5C53"));

    /// <summary>
    /// Not a ham band at all. Colder and darker than any of the four, on purpose.
    /// </summary>
    /// <remarks>
    /// NOT A MODE FAMILY (HM-DEC-055). It is a fifth region kind and it has to
    /// read as one at a glance: the four families are warm, light washes and
    /// this is a cold gray that stops the eye, because the thing it marks is a
    /// wall rather than a neighborhood. It is deliberately separated from
    /// <see cref="Open"/>, which is a warm neutral meaning unclaimed amateur
    /// space, and the grayscale test §0.6 sets is what forced the lightness
    /// apart as well as the hue. Ink measures 8.5:1 on the fill.
    /// </remarks>
    public static ModeColors OutsideTheBand { get; } = new(
        ModeFamily.OutsideTheBand, "Not a ham band",
        Color.Parse("#B4B8BC"), Color.Parse("#23282D"));

    /// <summary>The digital family's fill, for markup.</summary>
    /// <remarks>
    /// **`x:Static` NEEDS A BRUSH, AND THIS IS THE ONE `Digital` ALREADY BUILT.**
    /// It is an accessor rather than a new colour, so markup reaching for the
    /// digital family gets the same object the map and the field guide use and
    /// no surface can carry a literal of its own (§0.6, HM-DEC-032).
    /// </remarks>
    public static IBrush DigitalFill => Digital.FillBrush;

    /// <summary>The digital family's ink, for markup.</summary>
    /// <remarks>See <see cref="DigitalFill"/>; same reason, same object.</remarks>
    public static IBrush DigitalInk => Digital.InkBrush;

    /// <summary>The four families, in the order the legend shows them.</summary>
    public static IReadOnlyList<ModeColors> All { get; } = new[] { Cw, Digital, Phone, Open };

    /// <summary>
    /// Every region the map can draw, including the one that is not a family.
    /// </summary>
    /// <remarks>
    /// The legend reads this rather than <see cref="All"/>, because a wash
    /// nobody can decode is decoration, and decoration that looks like
    /// information is worse than none (§0.6).
    /// </remarks>
    public static IReadOnlyList<ModeColors> Legend { get; } =
        new[] { Cw, Digital, Phone, Open, OutsideTheBand };

    /// <summary>The colors for a family.</summary>
    /// <param name="family">The family.</param>
    /// <returns>Its fill and ink.</returns>
    public static ModeColors For(ModeFamily family) => family switch
    {
        ModeFamily.Cw => Cw,
        ModeFamily.Digital => Digital,
        ModeFamily.Phone => Phone,
        ModeFamily.OutsideTheBand => OutsideTheBand,
        _ => Open,
    };
}

/// <summary>
/// The dark instrument surfaces, and the ink a decode is written in.
/// </summary>
/// <remarks>
/// <para>Hamlet is a light app on warm paper (HM-DEC-012), with two exceptions
/// that are not exceptions to the taste so much as to the subject. The rig
/// display is the radio's own face and the waterfall is a spectrum display, and
/// both of those are dark everywhere in the world because a dark ground is what
/// makes a faint signal visible. The CW terminal joins them for the same
/// reason: it shows what the radio heard, so it belongs to the instrument
/// rather than to the paper.</para>
/// <para>CONFIDENCE IS SHOWN AS BRIGHTNESS, and brightness survives the
/// grayscale test §0.6 sets, because it is luminance and nothing else. A
/// character Hamlet is sure of is written in full decode green. One it is not
/// sure of is written dimmer, which reads as the app straining rather than as
/// a different kind of letter. And something it could not resolve is neither:
/// it is a placeholder in a color of its own, so it can never be mistaken for
/// content even by somebody who cannot see the color at all.</para>
/// </remarks>
public static class InstrumentPalette
{
    /// <summary>The dark ground a decode is written on.</summary>
    public static Color Surface { get; } = Color.Parse("#0B0F16");

    /// <summary>The surface as a brush.</summary>
    public static IBrush SurfaceBrush { get; } = new SolidColorBrush(Surface);

    /// <summary>The edge around an instrument panel.</summary>
    public static IBrush EdgeBrush { get; } = new SolidColorBrush(Color.Parse("#1E2A38"));

    /// <summary>A character the decoder stands behind.</summary>
    public static Color Confident { get; } = Color.Parse("#4ADE9B");

    /// <summary>
    /// A character the decoder is not sure of. Dimmer, deliberately.
    /// </summary>
    /// <remarks>
    /// Under half the brightness of a confident one, so the difference reads at
    /// a glance, and still clear of WCAG AA against the surface, because a
    /// character nobody can read is not a marked character, it is a missing one
    /// (HM-DEC-036). The first attempt at this was two shades darker and
    /// measured 3.8 to 1, which is exactly the carve-out §0.6 says there are
    /// none of.
    /// </remarks>
    public static Color Uncertain { get; } = Color.Parse("#3E9E74");

    /// <summary>Something heard and not resolved.</summary>
    public static Color Unreadable { get; } = Color.Parse("#B8843A");

    /// <summary>The quiet text an idle instrument shows.</summary>
    public static Color Idle { get; } = Color.Parse("#7E96AB");

    /// <summary>Every ink written on the instrument surface.</summary>
    public static IReadOnlyList<Color> Inks { get; } =
        new[] { Confident, Uncertain, Unreadable, Idle };

    /// <summary>A character the decoder stands behind, as a brush.</summary>
    public static IBrush ConfidentBrush { get; } = new SolidColorBrush(Confident);

    /// <summary>
    /// A character the decoder is not sure of. Dimmer, deliberately: the reader
    /// has to be able to see Hamlet struggling (§0.0).
    /// </summary>
    public static IBrush UncertainBrush { get; } = new SolidColorBrush(Uncertain);

    /// <summary>
    /// Something heard and not resolved. Amber, and it is the placeholder glyph
    /// carrying the meaning rather than the color.
    /// </summary>
    public static IBrush UnreadableBrush { get; } = new SolidColorBrush(Unreadable);

    /// <summary>The quiet text an idle instrument shows.</summary>
    public static IBrush IdleBrush { get; } = new SolidColorBrush(Idle);

    /// <summary>
    /// How much of an ink survives once its characters are no longer the
    /// current copy.
    /// </summary>
    /// <remarks>
    /// <para>**THE SCREEN WAS BURYING GOOD COPY UNDER OLD SOUP** — the night of
    /// 2026-08-25 ended with a transcript whose first hundred characters were
    /// decoded two minutes earlier, at full strength, sitting above three
    /// correctly-read callsign tokens. Everything on the instrument was equally
    /// bright, so the eye had nothing to land on.</para>
    /// <para>**IT IS A BLEND TOWARD THE SURFACE AND NOT AN OPACITY.** Each ink
    /// keeps its own hue, so a placeholder is still amber and an uncertain
    /// character is still the dimmer green: what changes is how far forward the
    /// text sits, and the three confidence states stay as distinguishable from
    /// each other as they were (§0.6 — colour may never be the only carrier, and
    /// history must not become a fourth confidence).</para>
    /// <para>**FORTY-FIVE PER CENT, WHICH IS FAR ENOUGH TO RECEDE AND NOT SO FAR
    /// AS TO HIDE.** Nothing is deleted and nothing becomes unreadable: the
    /// operator can still read and select every character of it, which is the
    /// whole difference between dimming history and trimming it.</para>
    /// </remarks>
    public const double HistoryShare = 0.45;

    private static readonly Dictionary<CwConfidence, IBrush> HistoryInks =
        new()
        {
            [CwConfidence.High] = Receded(Confident),
            [CwConfidence.Low] = Receded(Uncertain),
            [CwConfidence.Unreadable] = Receded(Unreadable),
        };

    private static IBrush Receded(Color ink)
        => new SolidColorBrush(Color.FromRgb(
            (byte)Math.Round((ink.R * HistoryShare) + (Surface.R * (1 - HistoryShare))),
            (byte)Math.Round((ink.G * HistoryShare) + (Surface.G * (1 - HistoryShare))),
            (byte)Math.Round((ink.B * HistoryShare) + (Surface.B * (1 - HistoryShare)))));

    /// <summary>The ink for a character that is no longer current copy.</summary>
    /// <param name="confidence">How much the decoder stands behind it.</param>
    /// <returns>The brush, receded toward the surface.</returns>
    public static IBrush HistoryFor(CwConfidence confidence)
        => HistoryInks.TryGetValue(confidence, out var ink)
            ? ink
            : HistoryInks[CwConfidence.Unreadable];

    /// <summary>The ink for a decoded character.</summary>
    /// <param name="confidence">How much the decoder stands behind it.</param>
    /// <returns>The brush.</returns>
    public static IBrush For(CwConfidence confidence) => confidence switch
    {
        CwConfidence.High => ConfidentBrush,
        CwConfidence.Low => UncertainBrush,
        _ => UnreadableBrush,
    };
}

/// <summary>One panel family's colors.</summary>
/// <param name="Family">The family these colors stand for.</param>
/// <param name="Title">Header text on warm paper, where the panel body is white.</param>
/// <param name="Edge">The panel border.</param>
/// <param name="Fill">A tinted panel body, where one is wanted.</param>
/// <param name="HeaderInk">Header text ON that tinted fill.</param>
/// <param name="PillFill">Background for a small badge in this family.</param>
/// <param name="PillInk">Text on that badge.</param>
public sealed record PanelColors(
    PanelFamily Family,
    Color Title,
    Color Edge,
    Color Fill,
    Color HeaderInk,
    Color PillFill,
    Color PillInk)
{
    /// <summary>Header text on warm paper, as a brush.</summary>
    public IBrush TitleBrush { get; } = new SolidColorBrush(Title);

    /// <summary>The panel border, as a brush.</summary>
    public IBrush EdgeBrush { get; } = new SolidColorBrush(Edge);

    /// <summary>A tinted panel body, as a brush.</summary>
    public IBrush FillBrush { get; } = new SolidColorBrush(Fill);

    /// <summary>Header text on the tinted fill, as a brush.</summary>
    public IBrush HeaderInkBrush { get; } = new SolidColorBrush(HeaderInk);

    /// <summary>Badge background, as a brush.</summary>
    public IBrush PillFillBrush { get; } = new SolidColorBrush(PillFill);

    /// <summary>Badge text, as a brush.</summary>
    public IBrush PillInkBrush { get; } = new SolidColorBrush(PillInk);
}

/// <summary>
/// The panel-family colors: amber is tuning, blue is spectrum, green is
/// decode, slate is everything else (HM-DEC-012).
/// </summary>
/// <remarks>
/// <para>ONE DEFINITION, like the mode language above it (§0.6). These values
/// used to live as hex literals inside <see cref="CollapsiblePanel"/>, which
/// meant the Settings window could not use them without becoming a second
/// copy. They live here now and the panel reads them, so there is one place
/// to change and nothing to drift.</para>
/// <para>TWO INKS PER FAMILY, and the reason is contrast rather than taste.
/// <see cref="PanelColors.Title"/> is the header on warm paper, where the
/// panel body is white (HM-DEC-012). <see cref="PanelColors.HeaderInk"/> is
/// the header on that family's own tinted fill, and it is darker because the
/// tint lifts the background: amber #C25E00 reaches only 3.84:1 on #FDF1DE,
/// short of the 4.5 every ink in this app has to clear, while #9A4A00 gets
/// there at 5.61. Green and blue happen to be dark enough already and carry
/// the same value twice.</para>
/// <para>COLOR IS NEVER THE ONLY CARRIER here either. A tinted section is
/// still titled in words and every badge drawn in these colors says what it
/// means (HM-DEC-044).</para>
/// </remarks>
public static class PanelPalette
{
    /// <summary>Tuning, and the license section that governs it.</summary>
    public static PanelColors Amber { get; } = new(
        PanelFamily.Amber,
        Color.Parse("#C25E00"),
        Color.Parse("#E8C093"),
        Color.Parse("#FDF1DE"),
        Color.Parse("#9A4A00"),
        Color.Parse("#F7E3C3"),
        Color.Parse("#9A4A00"));

    /// <summary>Spectrum, and the feeds that fill it.</summary>
    public static PanelColors Blue { get; } = new(
        PanelFamily.Blue,
        Color.Parse("#1F5FA8"),
        Color.Parse("#AECBEA"),
        Color.Parse("#EAF2FB"),
        Color.Parse("#1F5FA8"),
        Color.Parse("#D3E4F7"),
        Color.Parse("#174A85"));

    /// <summary>Decode, and the operator doing it.</summary>
    public static PanelColors Green { get; } = new(
        PanelFamily.Green,
        Color.Parse("#0F7B4D"),
        Color.Parse("#A9D8C1"),
        Color.Parse("#EAF6EF"),
        Color.Parse("#0F7B4D"),
        Color.Parse("#CFEBDD"),
        Color.Parse("#0B5C39"));

    /// <summary>The warm paper everything sits on, from `App.axaml`.</summary>
    /// <remarks>
    /// <para>**ONE PLACE, AND IT MATCHES `HmBackground`.** The tints below are
    /// the family fill moved toward this, so a family that arrives later lands
    /// in the same visual register as the three that were chosen by eye.</para>
    /// <para>**IT IS DECLARED ABOVE EVERY FAMILY THAT USES IT AND THAT IS NOT
    /// COSMETIC.** Static initialisers run in declaration order, so with this
    /// below `Lavender` the blend ran against a default colour and produced a
    /// dark tint: the header ink scored 1.10 against its own fill instead of
    /// 12, and `EveryPanelInkClearsAaOnItsOwnSurface` caught it.</para>
    /// </remarks>
    private static readonly Color Paper = Color.FromRgb(0xF6, 0xF3, 0xEC);

    /// <summary>Digital modes, and the tab that decodes them.</summary>
    /// <remarks>
    /// <para>**EVERY COLOUR HERE IS TAKEN FROM `ModePalette.Digital` AND NONE IS
    /// TYPED** (§0.6, HM-DEC-032). The mode-colour language already fixes what
    /// digital looks like — lavender fill, near-navy ink — and a panel family
    /// that restated those as fresh literals would be a second copy of the
    /// language, which is exactly what that ruling exists to prevent. The other
    /// three families here predate `ModePalette` and keep their own values;
    /// this one is derived, and the next family added should be too.</para>
    /// <para>**THE INK IS THE MODE PALETTE'S OWN INK**, so it carries
    /// HM-DEC-036's contrast guarantee with it: every ink in `ModePalette`
    /// clears WCAG AA against its own fill, checked when that ruling was made.
    /// Nothing here re-derives that and nothing here may weaken it.</para>
    /// <para>**THE EDGE AND THE TINT ARE THE FILL, SOFTENED TOWARD THE PAPER**,
    /// because a panel body stays white on warm paper (HM-DEC-012) and only the
    /// header text and the border carry the family. Mixing toward the background
    /// rather than toward white keeps the hue and drops the strength, which is
    /// what the amber, blue and green entries do by eye.</para>
    /// </remarks>
    public static PanelColors Lavender { get; } = new(
        PanelFamily.Lavender,
        ModePalette.Digital.Ink,
        Toward(ModePalette.Digital.Fill, Paper, 0.35),
        Toward(ModePalette.Digital.Fill, Paper, 0.80),
        ModePalette.Digital.Ink,
        Toward(ModePalette.Digital.Fill, Paper, 0.55),
        ModePalette.Digital.Ink);

    /// <summary>One colour moved a share of the way toward another.</summary>
    /// <param name="from">Where it starts.</param>
    /// <param name="to">What it moves toward.</param>
    /// <param name="share">How far, nought to one.</param>
    /// <returns>The blend.</returns>
    private static Color Toward(Color from, Color to, double share)
    {
        byte Mix(byte a, byte b) => (byte)Math.Round(a + ((b - a) * share));

        return Color.FromRgb(
            Mix(from.R, to.R), Mix(from.G, to.G), Mix(from.B, to.B));
    }

    /// <summary>Everything else. The quiet one, and it keeps a white body.</summary>
    public static PanelColors Slate { get; } = new(
        PanelFamily.Slate,
        Color.Parse("#2B2B28"),
        Color.Parse("#D5CFC0"),
        Colors.White,
        Color.Parse("#2B2B28"),
        Color.Parse("#EDEAE1"),
        Color.Parse("#4A4740"));

    /// <summary>All four.</summary>
    public static IReadOnlyList<PanelColors> All { get; } =
        new[] { Amber, Blue, Green, Lavender, Slate };

    /// <summary>The colors for a panel family.</summary>
    /// <summary>The lavender family's tint, for markup.</summary>
    /// <remarks>
    /// **ACCESSORS, NOT COLOURS.** Everything here comes from
    /// <see cref="Lavender"/>, which is itself derived from
    /// `ModePalette.Digital`. Markup that needs the digital panel family reaches
    /// these rather than restating a value (HM-DEC-032).
    /// </remarks>
    public static IBrush LavenderFill => Lavender.FillBrush;

    /// <summary>The lavender family's edge, for markup.</summary>
    public static IBrush LavenderEdge => Lavender.EdgeBrush;

    /// <summary>The lavender family's badge background, for markup.</summary>
    public static IBrush LavenderPillFill => Lavender.PillFillBrush;

    /// <param name="family">The family.</param>
    /// <returns>Its colors.</returns>
    public static PanelColors For(PanelFamily family) => family switch
    {
        PanelFamily.Amber => Amber,
        PanelFamily.Blue => Blue,
        PanelFamily.Green => Green,
        PanelFamily.Lavender => Lavender,
        _ => Slate,
    };
}
