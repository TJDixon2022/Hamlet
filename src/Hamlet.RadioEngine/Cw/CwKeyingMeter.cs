using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What the meter is willing to say about the audio right now.</summary>
public enum KeyingVerdict
{
    /// <summary>
    /// Not enough evidence yet, or a quiet stretch that has not lasted long
    /// enough to mean anything. **The state a meter starts in and returns to**,
    /// and never a polite way of saying no.
    /// </summary>
    Listening,

    /// <summary>Somebody is keying in this audio.</summary>
    Keying,

    /// <summary>Nothing in this audio is being keyed, and that has held.</summary>
    NoKeying,
}

/// <summary>
/// One reading of the keying meter.
/// </summary>
/// <param name="Verdict">What it is willing to say.</param>
/// <param name="ToneHz">The pitch it swept to, or 0 when it has read nothing.</param>
/// <param name="MedianMs">
/// The middle of every threshold crossing it counted there, which is what the
/// verdict is calibrated against and is **not** a key-down length anybody sent.
/// </param>
/// <param name="SwingDb">How far that pitch moved between quiet and loud.</param>
/// <param name="Runs">How many key-downs it counted.</param>
/// <param name="Score">How much the pitch looked like keying, nought to one.</param>
/// <param name="ElementMedianMs">
/// The middle key-down length among those that could be an element at all, or
/// nought where none could. **This is the figure a reader can use** and
/// <see cref="KeyingReading.MedianMs"/> is not.
/// </param>
/// <param name="Held">
/// True when the verdict is older than the window under it: the last window did
/// not look like keying and the meter is holding through a gap. **The numbers
/// beside it are still the newest ones**, because the number moving is what tells
/// the operator his last turn of a knob did something.
/// </param>
public readonly record struct KeyingReading(
    KeyingVerdict Verdict,
    double ToneHz,
    double MedianMs,
    double SwingDb,
    int Runs,
    double Score,
    bool Held,
    double ElementMedianMs = 0)
{
    /// <summary>Nothing measured.</summary>
    public static KeyingReading None { get; }
        = new(KeyingVerdict.Listening, 0, 0, 0, 0, 0, false);
}

/// <summary>
/// **PROVISIONAL, AND FROM A SMALL SAMPLE.** Every number the keying meter
/// decides anything with, in one place so a later session can move them together.
/// </summary>
/// <remarks>
/// <para>They come from six recordings across two nights, four of which are in
/// this repository. The gap they sit in is wide: keyed audio measured 54 to 58
/// milliseconds with a score from 0.18 to 0.37, and noise measured 2 to 3
/// milliseconds with a score that rounds to nought. **A wide gap measured on a
/// small sample is still a small sample**, and these are where to change it
/// rather than in the meter's own reasoning.</para>
/// </remarks>
public static class CwKeyingThresholds
{
    /// <summary>How much audio one reading looks at.</summary>
    /// <remarks>
    /// Six seconds is the shortest window in which every keyed recording measured
    /// stayed in the element range. At four, two windows of genuinely keyed audio
    /// fell to five and eight milliseconds, because the operator had stopped
    /// sending between overs.
    /// </remarks>
    public static TimeSpan Window { get; } = TimeSpan.FromSeconds(6);

    /// <summary>The shortest median that counts as somebody keying, in milliseconds.</summary>
    /// <remarks>
    /// Twenty-five milliseconds is a dit at about fifty words a minute. Below it
    /// the thing being measured is a gate chattering, which is what the two
    /// unreadable captures looked like at seven.
    /// </remarks>
    public const double SlowestChatterMs = 25;

    /// <summary>The longest median that counts as somebody keying, in milliseconds.</summary>
    /// <remarks>
    /// Two hundred and fifty milliseconds is a dah at about five words a minute.
    /// Beyond it the measurement is a carrier or a fade rather than a fist.
    /// </remarks>
    public const double LongestElementMs = 250;

    /// <summary>How much of a stretch must look like elements before it counts.</summary>
    /// <remarks>
    /// The lowest keyed window measured 0.18 and the highest noise window measured
    /// under 0.01, so a tenth sits with most of the gap on the noise side of it.
    /// **Deliberately nearer the noise**: this meter exists to find a station
    /// nothing else found, and a threshold that misses one is the failure that
    /// costs an evening.
    /// </remarks>
    public const double KeyingScore = 0.10;

    /// <summary>
    /// How far a pitch must move between quiet and loud before the tracker's own
    /// figure is trusted over the operator's.
    /// </summary>
    /// <remarks>
    /// <para>**THE SWING IS THE FIGURE THAT HELD STEADY ALL EVENING AND THE
    /// TIMING IS NOT.** On the evening of 2026-08-20 the four captures holding a
    /// real station measured 20 to 24 decibels of swing and every capture with
    /// nothing in it measured 13 to 14, while this meter's own key-down timing
    /// wandered to nine milliseconds on a station sending ninety. Anything
    /// deciding whether there is a station to track has to rest on the steady
    /// figure.</para>
    /// <para>**AND IT SEPARATES ON THIS REPOSITORY'S OWN RECORDINGS TOO**, which
    /// is the check that matters because the evening's captures are not in the
    /// tree. Measured through <see cref="KeyingEnvelope.Best"/>: the seven
    /// recordings holding a station swing 21.8 to 91.5 decibels, and the two that
    /// hold no keying at any pitch swing 14.1 and 17.7. Twenty sits in the gap on
    /// both sets of evidence.</para>
    /// <para>**IT IS NOT A SECOND OPINION ABOUT WHETHER TO DECODE.** Nothing in
    /// the decoder reads it and it can silence nothing: all it decides is which
    /// of two speed estimates the decoder starts from, and the decoder's own
    /// refusals are untouched (§0.0).</para>
    /// </remarks>
    public const double ConfidentSwingDb = 20;

    /// <summary>
    /// How many windows in a row must show nothing before the meter says so.
    /// </summary>
    /// <remarks>
    /// <para>**A METER THAT DROPS TO NO KEYING BETWEEN OVERS IS WORSE THAN
    /// NONE**, because the operator stops trusting it in the first ten minutes and
    /// then it cannot help him at all.</para>
    /// <para>**FIVE WAS TRIED AND MEASURED AND IT IS NOT ENOUGH.** Played end to
    /// end with an eight second gap in the middle, the meter used its whole budget
    /// and changed its mind while the station was still in the contact. Eight
    /// seconds is not an unusual pause; it is barely long enough for the other
    /// operator to send a callsign.</para>
    /// <para>Fifteen, with a six second window recomputed each second, means the
    /// last element has to be about twenty seconds behind before the word changes:
    /// six for the window to empty of it and fifteen more for the run. A short
    /// over at this project's own reference copy speed of thirteen words a minute
    /// runs about that long, so the meter sits through one.</para>
    /// <para>**AND THE LONG HOLD COSTS THE OPERATOR ALMOST NOTHING, BECAUSE THE
    /// WORD IS NOT WHAT HE IS WATCHING.** While it holds it says it is holding,
    /// and the numbers beside it are the newest window's, so a knob he turns shows
    /// up in the figures inside six seconds whatever the word still says.</para>
    /// </remarks>
    public const int QuietWindowsBeforeNoKeying = 15;
}

/// <summary>
/// Whether Hamlet can hear keying in what the operator is listening to, said
/// continuously and independently of the decoder (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR CAN HEAR STATIONS HAMLET CANNOT, AND HE FINDS OUT THE
/// NEXT MORNING.** Two presses on the 19th, two stations he heard, and both rows
/// read nothing. Measurement said the audio contained no keyed signal at any
/// pitch, so the decoder was right and something between the antenna and the
/// recording is losing the station. Nobody knows what. This is the instrument
/// that lets him find out at the rig, by turning a knob and watching a number
/// move, instead of by reading a roster the next day.</para>
/// <para>**IT SHARES NOTHING WITH THE DECODER AND THAT IS THE POINT** (§12.5). It
/// sweeps its own pitch rather than taking the decoder's, because the decoder
/// once chose 800 Hz on a recording whose content sat at 608, and a meter that
/// inherited that choice could only ever agree with the thing it exists to
/// contradict.</para>
/// <para>**IT REPORTS AND DRIVES NOTHING.** It does not retune, does not switch
/// the decoder on or off, and does not gate a capture.</para>
/// </remarks>
public sealed class CwKeyingMeter
{
    /// <summary>The six seconds it reads, in a buffer it owns.</summary>
    /// <remarks>
    /// **ONCE A SECOND, AND IT USED TO BE 1.15 MB EACH TIME.** Six seconds at
    /// 48 kHz is 288,000 floats, which is on the large object heap, whose
    /// collection stops every thread in the process including the one carrying
    /// audio. The meter exists to tell the operator whether the audio path is
    /// delivering; it should not be one of the reasons it is not.
    /// </remarks>
    private readonly ReusableWindow _window = new();

    private int _quiet;
    private KeyingVerdict _verdict = KeyingVerdict.Listening;

    /// <summary>How many times the meter's own buffer has been sized.</summary>
    /// <remarks>
    /// **ONE, FOR THE LIFE OF THE METER, OR IT IS NOT REUSING ANYTHING**
    /// (HM-DEC-093). It is the whole of what unit 239 task 3 claims about this
    /// class, stated as a count rather than as an allocation measurement: a
    /// difference between two eighty-megabyte readings needs a precision the
    /// runtime's per-thread counter does not have on a loaded machine, and this
    /// needs none at all.
    /// </remarks>
    public int WindowSizings => _window.Sizings;

    /// <summary>The last thing it read.</summary>
    public KeyingReading Reading { get; private set; } = KeyingReading.None;

    /// <summary>
    /// Look at the newest stretch of what the decoder is being fed.
    /// </summary>
    /// <param name="tap">The decoder's own tap.</param>
    /// <returns>The reading, which is also left on <see cref="Reading"/>.</returns>
    /// <exception cref="ArgumentNullException">No tap.</exception>
    /// <remarks>
    /// **THE TAP AND NOT THE SOUND CARD**, so the meter and the decoder are
    /// looking at the same samples. A meter reading audio the decoder never saw
    /// could disagree with it for a reason that tells the operator nothing.
    /// </remarks>
    public KeyingReading Update(AudioTap tap)
    {
        ArgumentNullException.ThrowIfNull(tap);

        return Update(_window.Tail(tap, CwKeyingThresholds.Window));
    }

    /// <summary>Look at one stretch of audio.</summary>
    /// <param name="window">The stretch, or null when there is not enough yet.</param>
    /// <returns>The reading.</returns>
    public KeyingReading Update(MonoAudio? window)
    {
        if (window is null || KeyingEnvelope.Best(window) is not { } best)
        {
            // **NOT ENOUGH AUDIO IS NOT AN ABSENCE OF KEYING** (§0.0). The run of
            // quiet windows is left alone rather than advanced, so a stall in the
            // pipeline cannot talk the meter into saying the band is dead.
            Reading = Reading with { Held = Reading.Verdict != KeyingVerdict.Listening };

            return Reading;
        }

        // **THE VERDICT RESTS ON THE ELEMENT MEDIAN NOW, AND ON THE SWING TO
        // KEEP IT HONEST.** Tim ruled the move on 2026-08-25, on the condition
        // that the silence property survives it (HM-DEC-120).
        //
        // **WHY THE OLD FIGURE HAD TO GO.** `MedianMs` is the middle of every
        // threshold crossing, and noise crosses a threshold hundreds of times,
        // so on a recording holding a real station the chatter outnumbers the
        // elements several to one and the median lands among the chatter: four
        // milliseconds beside an adjudicated `VA3VRR`, three beside an
        // adjudicated `N4L`, and this test then said there was no keying in
        // either. Measured over the twenty-three recordings in the tree, the
        // meter was right about ten of them.
        //
        // **WHY THE ELEMENT MEDIAN ALONE COULD NOT SHIP.** It takes the meter to
        // seventeen of twenty-three, and on the live path it costs the silence:
        // sliced into the six-second windows the meter actually runs on,
        // `cw-2026-08-20-014854` and `cw-2026-08-20-014935` produce **eleven
        // windows** that clear the score and land inside the element range, and
        // the meter would announce Keying on a band holding nothing.
        //
        // **AND WHY THE REQUIREMENT IS THE SWING RATHER THAN A COUNT OF
        // ELEMENTS.** A count was the obvious candidate and it is measured
        // backwards: in six seconds an empty band produces 26 to 40 element
        // length runs and a real station produces 11 to 38, median 26, so the
        // empty windows sit at the *top* of the range and any count that
        // silences them silences every real window with it — nineteen captures
        // to nought. The swing does separate, and it separates with room: those
        // eleven empty windows run 14.7 to 17.7 decibels while the real windows
        // run to 218 with a tenth percentile of 18.9.
        //
        // **THE NUMBER IS NOT NEW AND IS NOT FITTED HERE.**
        // <see cref="CwKeyingThresholds.ConfidentSwingDb"/> is already twenty,
        // already calibrated against this same question on two independent sets
        // of evidence, and until now decided only which speed estimate the
        // decoder started from. Eighteen would keep one more capture and
        // eighteen is the empty windows' own maximum rounded up, which is
        // fitting a constant to a fixture.
        //
        // **WHAT IT COSTS, NAMED.** Sixteen of twenty-three rather than
        // seventeen. The capture given up is `cw-2026-08-23-001831`, a pileup
        // with nothing adjudicated in it, swinging 19.3 decibels against a bar
        // of twenty. **What it holds**: all four recordings that emit nothing
        // produce nought Keying windows out of twenty-five each, and their
        // whole-file swings are 14.1, 15.7, 16.7 and 17.7.
        var looksKeyed = best.Profile.Score >= CwKeyingThresholds.KeyingScore
                         && best.Profile.ElementMedianMs >= CwKeyingThresholds.SlowestChatterMs
                         && best.Profile.ElementMedianMs <= CwKeyingThresholds.LongestElementMs
                         && best.Profile.SwingDb >= CwKeyingThresholds.ConfidentSwingDb;

        if (looksKeyed)
        {
            // **QUICK TO SAY YES AND SLOW TO SAY NO.** One window of real keying
            // is evidence a station is there; one window without it is evidence of
            // nothing, because he might have stopped to listen.
            _quiet = 0;
            _verdict = KeyingVerdict.Keying;
        }
        else if (++_quiet >= CwKeyingThresholds.QuietWindowsBeforeNoKeying)
        {
            _verdict = KeyingVerdict.NoKeying;
        }

        Reading = new KeyingReading(
            _verdict,
            best.ToneHz,
            best.Profile.MedianMs,
            best.Profile.SwingDb,
            best.Profile.RunsMs.Count,
            best.Profile.Score,
            Held: !looksKeyed && _verdict == KeyingVerdict.Keying,
            ElementMedianMs: best.Profile.ElementMedianMs);

        return Reading;
    }

    /// <summary>Forget everything, for a fresh decoder or a fresh device.</summary>
    public void Reset()
    {
        _quiet = 0;
        _verdict = KeyingVerdict.Listening;
        Reading = KeyingReading.None;
    }
}
