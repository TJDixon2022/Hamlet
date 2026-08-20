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
/// <param name="MedianMs">The middle key-down length it found there.</param>
/// <param name="SwingDb">How far that pitch moved between quiet and loud.</param>
/// <param name="Runs">How many key-downs it counted.</param>
/// <param name="Score">How much the pitch looked like keying, nought to one.</param>
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
    bool Held)
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
    private int _quiet;
    private KeyingVerdict _verdict = KeyingVerdict.Listening;

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

        return Update(tap.Tail(CwKeyingThresholds.Window));
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

        var looksKeyed = best.Profile.Score >= CwKeyingThresholds.KeyingScore
                         && best.Profile.MedianMs >= CwKeyingThresholds.SlowestChatterMs
                         && best.Profile.MedianMs <= CwKeyingThresholds.LongestElementMs;

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
            Held: !looksKeyed && _verdict == KeyingVerdict.Keying);

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
