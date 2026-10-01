namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **A LIGHT THAT SAYS HOLD STILL** (work instruction 521, HM-DEC-225): what the shape side is doing, so the owner
/// knows to stay on a frequency while it works.
/// </summary>
/// <remarks>
/// **THE OWNER, 2026-10-01**: *"I want a green light whenever the first shape is being detected so that I know to
/// hold on that frequency and not adjust, because you're not hearing it."* Five marks must stand before a sequence
/// exists and the first letter waits a word gap after that (unit 520), seconds of nothing on the screen while the
/// shape side works.
/// </remarks>
public enum CwShapeLight
{
    /// <summary>Dark: no candidate mark is building a sequence.</summary>
    Listening,

    /// <summary>Amber: candidate marks are arriving at one pitch and a sequence is building but has not stood.</summary>
    Forming,

    /// <summary>Green: a sequence stands; letters follow within a word gap.</summary>
    Found,

    /// <summary>Green: a sender is being printed.</summary>
    Reading,
}

/// <summary>The light's words (work instruction 521): shown beside it, since colour is never the only carrier (§0.6).</summary>
public static class CwShapeLights
{
    /// <summary>How recent a candidate mark must be for the light to say a shape is forming, in seconds: two.</summary>
    public const double FormingSeconds = 2;

    /// <summary>
    /// The shape score a standing sequence must pass before the light goes green: a fifth (work instruction 522, task
    /// 2). Clear of everything noise has produced - unit 520 measured noise's best at 0.173 - and under a rough fist's 0.374.
    /// </summary>
    public const double GreenScore = 0.2;

    /// <summary>Where the gauge's first stretch, the marks toward five, ends and its second, the shape score, begins.</summary>
    public const double Mark = 0.8;

    /// <summary>
    /// **THE GAUGE** (work instruction 522, task 3): how full the bar is, nought to one. Empty while listening; a fifth
    /// per mark while a shape forms, to four-fifths at four; from the fifth mark the shape score, 0.2 at the mark and
    /// 1.0 full; full while a sender is read.
    /// </summary>
    /// <param name="light">The light's state.</param>
    /// <param name="forming">How many marks the forming sequence holds.</param>
    /// <param name="shapeScore">The standing sequence's shape score, NaN while none stands.</param>
    /// <returns>The fill.</returns>
    public static double Fill(CwShapeLight light, int forming, double shapeScore) => light switch
    {
        CwShapeLight.Reading => 1,
        CwShapeLight.Found => Mark + ((1 - Mark) * Math.Clamp((shapeScore - GreenScore) / (1 - GreenScore), 0, 1)),
        CwShapeLight.Forming => Math.Min(Mark, forming / (double)CwPatternGate.MarksToStand),
        _ => 0,
    };

    /// <summary>What the light says.</summary>
    /// <param name="light">Its state.</param>
    /// <param name="forming">How many marks the forming sequence holds.</param>
    /// <returns>The words.</returns>
    public static string Words(CwShapeLight light, int forming) => light switch
    {
        CwShapeLight.Forming => $"shape forming · {forming} of {CwPatternGate.MarksToStand}",
        CwShapeLight.Found => "shape found · hold here",
        CwShapeLight.Reading => "reading",
        _ => "listening",
    };

    /// <summary>What the light means, on hover.</summary>
    public const string Tip =
        "Fills as Hamlet grows sure it has a station here. Past the mark, hold the frequency; the first letters print after one word gap. Amber at the mark is a shape that has stood but is not yet clean enough to trust.";
}
