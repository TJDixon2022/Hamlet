namespace Hamlet.RadioEngine.Cw;

/// <summary>One sender the gate holds, as it stood at a moment (work instruction 537).</summary>
/// <param name="PitchHz">Its pitch, from its own marks.</param>
/// <param name="ShapeScore">How much its marks and gaps sound like code, nought to one.</param>
/// <param name="Marks">How many of its marks have stood in all.</param>
/// <param name="Printed">Whether it is the sender the terminal prints.</param>
public sealed record CwSenderStanding(double PitchHz, double ShapeScore, int Marks, bool Printed);

/// <summary>
/// **WHAT THE SHAPE SIDE HELD AT A MOMENT** (work instruction 537, HM-DEC-241): the printed sender's pitch and dit, the
/// letters printed and marks that stood with when each ended, and every sender the gate holds. Published by the gate on the
/// audio thread at the end of each batch and read whole from any thread, so the capture sheet takes every figure from one
/// instant and from the path that reaches the screen.
/// </summary>
/// <param name="HeardSeconds">Where the gate's clock stood when it was published.</param>
/// <param name="PrintedPitchHz">The printed sender's pitch to the hertz, or NaN where nobody is printed.</param>
/// <param name="PrintedDitSeconds">The printed sender's dit, or NaN where nobody is printed.</param>
/// <param name="LettersPrinted">Letters printed since the gate began.</param>
/// <param name="LettersUnreadable">Of those, how many printed as the placeholder.</param>
/// <param name="MarksStood">Marks that stood and reached the gate since it began.</param>
/// <param name="LetterTimes">When each recent letter's last mark ended, on the gate's clock.</param>
/// <param name="MarkTimes">When each recent mark that stood ended, on the gate's clock.</param>
/// <param name="UnreadableTimes">When each recent letter that printed as the placeholder ended.</param>
/// <param name="Senders">Every sender the gate holds.</param>
public sealed record CwShapeSideReading(
    double HeardSeconds,
    double PrintedPitchHz,
    double PrintedDitSeconds,
    long LettersPrinted,
    long LettersUnreadable,
    long MarksStood,
    IReadOnlyList<double> LetterTimes,
    IReadOnlyList<double> MarkTimes,
    IReadOnlyList<double> UnreadableTimes,
    IReadOnlyList<CwSenderStanding> Senders)
{
    /// <summary>How far back <see cref="LetterTimes"/> and <see cref="MarkTimes"/> reach, in seconds: two minutes.</summary>
    public const double KeptSeconds = 120;

    /// <summary>Nothing heard yet.</summary>
    public static CwShapeSideReading Nothing { get; } = new(double.NaN, double.NaN, double.NaN, 0, 0, 0, [], [], [], []);

    /// <summary>The printed sender's speed, from its dit, or null where nobody is printed.</summary>
    public int? PrintedWpm => PrintedDitSeconds > 0 ? (int)Math.Round(1.2 / PrintedDitSeconds) : null;

    /// <summary>Letters printed whose last mark ended in the last so many seconds of the gate's clock.</summary>
    /// <param name="seconds">How far back.</param>
    /// <returns>The count.</returns>
    public int LettersInLast(double seconds) => LetterTimes.Count(t => t >= HeardSeconds - seconds);

    /// <summary>Marks that stood and ended in the last so many seconds of the gate's clock.</summary>
    /// <param name="seconds">How far back.</param>
    /// <returns>The count.</returns>
    public int MarksInLast(double seconds) => MarkTimes.Count(t => t >= HeardSeconds - seconds);

    /// <summary>Letters printed as the placeholder whose last mark ended in the last so many seconds.</summary>
    /// <param name="seconds">How far back.</param>
    /// <returns>The count.</returns>
    public int UnreadableInLast(double seconds) => UnreadableTimes.Count(t => t >= HeardSeconds - seconds);
}
