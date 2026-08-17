using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Scan;

/// <summary>What a dwell should do next.</summary>
public enum DwellAction
{
    /// <summary>Keep listening. Not enough has been heard yet.</summary>
    KeepListening,

    /// <summary>Stay here. Something a person sends came through.</summary>
    Stay,

    /// <summary>Move on. The window ran out without anything recognizable.</summary>
    MoveOn,
}

/// <summary>
/// One stop on a scan: what was heard there, and what to do about it
/// (HM-DEC-107, phase 7).
/// </summary>
/// <remarks>
/// <para>**THE LENGTH OF A DWELL IS SET BY WHAT A CQ SOUNDS LIKE AND NOT BY
/// TASTE.** A relaxed call is <c>CQ CQ CQ DE</c> and a callsign twice, which
/// runs eight to ten seconds at a middling speed, and the caller then listens
/// for about as long again. A dwell shorter than one cycle lands in the gap
/// between two calls and reports an empty frequency that had somebody on it,
/// which is the specific failure a beginner reads as "the band is dead"
/// (§0.0).</para>
/// <para>**IT CAN LEAVE EARLY AND IT CANNOT STAY LATE.** A <c>CQ</c> heard in
/// the third second settles the question, and going on listening after it costs
/// the scan the rest of the band. Nothing recognized inside the whole window is
/// the answer rather than grounds for another window.</para>
/// <para>Pure, and it holds no clock: the caller passes elapsed time in, so the
/// same characters and the same timings give the same answer every run (§5).</para>
/// </remarks>
public sealed class ScanDwell
{
    /// <summary>The shortest a dwell may be, in seconds.</summary>
    /// <remarks>
    /// Ten, which is about one relaxed CQ call. Anything less can sit entirely
    /// inside the silence between two of them.
    /// </remarks>
    public const double ShortestSeconds = 10;

    /// <summary>The longest a dwell may be, in seconds.</summary>
    /// <remarks>
    /// Twenty, roughly a call and the listen after it. Past that a frequency
    /// that has produced nothing recognizable is holding up the rest of the
    /// band.
    /// </remarks>
    public const double LongestSeconds = 20;

    private readonly List<CwCharacter> _heard = new();

    /// <summary>Creates a dwell on one candidate.</summary>
    /// <param name="frequencyHz">Where the radio was pointed.</param>
    /// <param name="seconds">
    /// How long to listen for, clamped to the ten to twenty second window.
    /// </param>
    public ScanDwell(long frequencyHz, double seconds = LongestSeconds)
    {
        FrequencyHz = frequencyHz;
        Seconds = Math.Clamp(seconds, ShortestSeconds, LongestSeconds);
    }

    /// <summary>Where this dwell is.</summary>
    public long FrequencyHz { get; }

    /// <summary>How long it will listen for, in seconds.</summary>
    public double Seconds { get; }

    /// <summary>Everything the decoder produced here, in order.</summary>
    public IReadOnlyList<CwCharacter> Heard => _heard;

    /// <summary>What the dwell came to, judged from everything heard so far.</summary>
    public ScanVerdict Verdict { get; private set; } = ScanVerdict.Silent;

    /// <summary>Take in one decoded character.</summary>
    /// <param name="character">What the decoder read.</param>
    /// <remarks>
    /// **THE SETTLED PASS IS THE ONE TO FEED THIS, NOT THE LEADING EDGE**
    /// (HM-DEC-096). A provisional reading is right far more often than not and
    /// a scan acting on one would stop on a <c>CQ</c> that a second reading
    /// dissolves, which is a guess presented as a decode in the one place the
    /// operator cannot check it: the dial has already moved.
    /// </remarks>
    public void Take(CwCharacter character)
    {
        _heard.Add(character);
        Verdict = ScanStopClassifier.Judge(_heard);
    }

    /// <summary>
    /// What to do, given how long this has been running.
    /// </summary>
    /// <param name="elapsedSeconds">Time since the dwell began.</param>
    /// <returns>Keep listening, stay, or move on.</returns>
    public DwellAction Decide(double elapsedSeconds)
    {
        if (Verdict.Stop)
        {
            return DwellAction.Stay;
        }

        return elapsedSeconds >= Seconds ? DwellAction.MoveOn : DwellAction.KeepListening;
    }

    /// <summary>
    /// What happened here, for the record and for the screen (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **A DWELL THAT FOUND NOTHING STILL REPORTS.** A scan whose record holds
    /// only its stops cannot be told from one that never ran, and the frequencies
    /// it passed over are half of what it measured.
    /// </remarks>
    public string Describe()
    {
        var mhz = FrequencyHz / 1_000_000.0;

        return $"{mhz:0.000} MHz after {Seconds:0} seconds: {Verdict.Sentence}";
    }
}
