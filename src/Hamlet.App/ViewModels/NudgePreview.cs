using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **What the quill's popup shows: the card he would earn, and what it teaches.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12, LOOKING AT WHAT UNIT 327 SHIPPED**: *"The achievement
/// indicator popup is not pretty or interesting."* What he pressed gave him three lines of
/// gray text in a box - `LA8ENA / Norway · a new country · you have worked 3 in Europe / A
/// QSO first. The card comes with it.` Every word of that was true and none of it was worth
/// opening.</para>
/// <para>**SO THE POPUP IS A PREVIEW OF THE CARD** (work instruction 330 task 3). The
/// achievements screen draws an earned card as a face with a figure, a title and a line
/// underneath; this is that face, drawn the same way, **as it will look when he earns it and
/// not dimmed**. The words around it are the teaching (§3.5 - *the teaching is the product*),
/// one line of fact about the station, and what working him earns.</para>
/// <para>**A DOOR NAMES NOTHING AND THAT IS ABSOLUTE** (§3.1). A counter's face carries the
/// entity - `Norway` - because that card is one the screen would already show him. A door's
/// face carries the ring and the words *a new area*, and **nothing anywhere in this object
/// knows which area it is**: the continent is not passed in, not stored and not derivable
/// from anything here.</para>
/// <para>**AND NOTHING SAYS *confirmed*** (§4). Hamlet has contacts and no confirmations,
/// so every count here says **worked**.</para>
/// </remarks>
public sealed partial class NudgePreview : ObservableObject
{
    /// <summary>What a door's face says in place of a name.</summary>
    public const string DoorFaceWord = "a new area";

    /// <summary>What a counter's face says under the entity.</summary>
    public const string CounterFaceWord = "a new country";

    /// <summary>What a door's teaching line says, naming nothing.</summary>
    /// <remarks>
    /// **THE TEACHING FOR A DOOR IS THE MECHANIC AND NOT THE PRIZE** (§3.1, §3.5). It says
    /// what a first contact in a new area does, which is the thing he does not know; it does
    /// not say which area, how many are in it, or what is behind it, because naming any of
    /// those names the door.
    /// </remarks>
    public const string DoorTeaching =
        "The first contact in a new area opens every country in it.";

    /// <summary>Nothing to preview. The popup is not offered.</summary>
    public static NudgePreview None { get; } = new();

    /// <summary>True where there is a card to draw.</summary>
    public bool HasPreview { get; private init; }

    /// <summary>True where the face is a door's - the ring, and no name.</summary>
    public bool IsDoor { get; private init; }

    /// <summary>True where the face is a counter's - the green quill, and a name.</summary>
    public bool IsCounter => HasPreview && !IsDoor;

    /// <summary>The entity on the face, for a counter. Empty for a door.</summary>
    public string FaceName { get; private init; } = "";

    /// <summary>True where the face carries a name at all.</summary>
    public bool HasFaceName => FaceName.Length > 0;

    /// <summary>The words under the face's name: the kind of card it is.</summary>
    public string FaceWord { get; private init; } = "";

    /// <summary>**The big line on the face: the entity, or a door's category.**</summary>
    /// <remarks>
    /// **A DOOR'S FACE HAS ONE LINE AND IT IS THE CATEGORY** (§3.1). There is no name to
    /// put above *a new area*, so *a new area* is the face.
    /// </remarks>
    public string FaceHeadline => HasFaceName ? FaceName : FaceWord;

    /// <summary>The small line under the face's headline, or "" for a door.</summary>
    public string FaceUnder => HasFaceName ? FaceWord : "";

    /// <summary>True where the face has a second line.</summary>
    public bool HasFaceUnder => FaceUnder.Length > 0;

    /// <summary>Which quill the face draws: the green counter, or the ringed door.</summary>
    /// <remarks>
    /// **THE SAME CONTROL AND THE SAME TWO FORMS AS THE ROW'S MARK** (§R16, §0.6). The
    /// difference between the two faces is a shape - a ring or none - before it is a hue,
    /// so it survives a grayscale print and a color vision deficiency both.
    /// </remarks>
    public Controls.AchievementMarkForm FaceForm => IsDoor
        ? Controls.AchievementMarkForm.Door
        : Controls.AchievementMarkForm.Counter;

    /// <summary>**The one line of teaching** (§3.5).</summary>
    /// <remarks>
    /// <para>**A COUNTER'S TEACHING IS WHERE THIS CARD SITS IN A SET HE IS COLLECTING**:
    /// *your 4th country in Europe · 40 to go*. The ordinal is his worked count plus this
    /// one, and the remainder is the continent's entity count from
    /// <see cref="DxccContinents"/> - the cited table - less what he would then hold.</para>
    /// <para>**THE REMAINDER IS DROPPED RATHER THAN GUESSED.** Where the table does not
    /// know the continent, or knows no count for it, the line stops after the ordinal
    /// (§0.0). A *to go* worked out from nothing is a claim about how much of the world is
    /// left.</para>
    /// <para>**NOTHING SHAMES** (§4). *40 to go* is what is in front of him rather than what
    /// he is short of, and it sits after a count of what he has done.</para>
    /// </remarks>
    public string TeachingLine { get; private init; } = "";

    /// <summary>True where there is a teaching line.</summary>
    public bool HasTeachingLine => TeachingLine.Length > 0;

    /// <summary>**One line of fact about the station: how far, which way, his time.**</summary>
    /// <remarks>
    /// <para>**IT IS THE CARD'S OWN COLUMN AND NOTHING RECOMPUTES IT** (work instruction 330
    /// task 3). The distance and the compass word are `Ft8ContactCard.DistanceValue` and the
    /// clock is `SolarTimeValue`, so the popup and the table under the map cannot come to
    /// disagree about one station.</para>
    /// <para>**AND IT IS ABSENT WHERE HAMLET HAS NO GRID FOR HIM** (§0.0), the same as the
    /// rows of the table it comes from. A station who has not put a grid on the air is not a
    /// station whose grid failed to read.</para>
    /// <para>**SETTABLE, BECAUSE THE ROW LEARNS IT LATER THAN THE MARK.** A decoded row is
    /// marked as it arrives and the cards are rebuilt after, so the fact is handed to the
    /// row's preview when the card that holds it exists.</para>
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFactLine))]
    private string _factLine = "";

    /// <summary>True where there is a fact line.</summary>
    public bool HasFactLine => FactLine.Length > 0;

    /// <summary>What working him earns. The same for both kinds.</summary>
    public string ClosingLine => NudgeWords.Earns;

    /// <summary>Build the preview for one station's mark.</summary>
    /// <param name="reason">What the panel's one nudge set knows about him.</param>
    /// <param name="factLine">The card's own distance and clock, or "".</param>
    /// <returns>The preview, or <see cref="None"/> where he earns nothing.</returns>
    public static NudgePreview For(NudgeReason reason, string factLine = "")
        => reason.Kind switch
        {
            NudgeKind.Door => new NudgePreview
            {
                HasPreview = true,
                IsDoor = true,
                FaceName = "",
                FaceWord = DoorFaceWord,
                TeachingLine = DoorTeaching,
                FactLine = factLine,
            },

            NudgeKind.Visible when !string.IsNullOrWhiteSpace(reason.Entity) =>
                new NudgePreview
                {
                    HasPreview = true,
                    IsDoor = false,
                    FaceName = reason.Entity.Trim(),
                    FaceWord = CounterFaceWord,
                    TeachingLine = Teaching(reason),
                    FactLine = factLine,
                },

            _ => None,
        };

    /// <summary>Where this card would sit in the set he is collecting.</summary>
    private static string Teaching(NudgeReason reason)
    {
        if (string.IsNullOrWhiteSpace(reason.Continent))
        {
            return "";
        }

        var would = reason.WorkedInContinent + 1;

        var line = "Your " + Ordinal(would) + " country in " + reason.Continent.Trim();

        // **THE COUNT COMES OFF THE CITED TABLE AND IS NOT A NUMBER IN THIS FILE**
        // (§0's generated-from-a-source-of-truth rule). `DxccContinents` reads
        // `data/callsigns/dxcc-continents.json` and carries its own source line.
        var code = DxccContinents.Of(reason.Entity);
        var onIt = DxccContinents.EntitiesOn(code);

        if (onIt <= would)
        {
            return line;
        }

        return line + " · " + (onIt - would).ToString(CultureInfo.InvariantCulture)
            + " to go";
    }

    /// <summary>`1st`, `2nd`, `3rd`, `4th` - American, and correct in the teens.</summary>
    private static string Ordinal(int n)
    {
        var suffix = (n % 100) is >= 11 and <= 13
            ? "th"
            : (n % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            };

        return n.ToString(CultureInfo.InvariantCulture) + suffix;
    }
}
