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
        "Green means Hamlet has the shape of a station here. Hold the frequency; the first letters print after one word gap.";
}
