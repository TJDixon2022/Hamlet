namespace Hamlet.RadioEngine.Cw;

/// <summary>What the decoder has noticed about the signal it is working on.</summary>
public enum CwNote
{
    /// <summary>Nothing worth saying. The decode is going fine.</summary>
    None,

    /// <summary>No tone has stood above the noise for a while.</summary>
    NothingHeard,

    /// <summary>The signal keeps rising and falling.</summary>
    Fading,

    /// <summary>The sending is faster than the decoder is managing to follow.</summary>
    TooFast,

    /// <summary>The tone is only just above the noise.</summary>
    Weak,
}

/// <summary>
/// The plain words that go with each thing the decoder noticed.
/// </summary>
/// <remarks>
/// <para>THE HONESTY CONSTRAINT, and it is narrower than it looks. These notes
/// describe what the decoder measured, and nothing else. They may not diagnose
/// the band, the antenna, the other operator's equipment or propagation, and a
/// test sweeps every one of them for the phrases that would (HM-DEC-048).
/// "The tone is fading in and out" is a measurement. "Conditions are poor
/// tonight" is a claim Hamlet has no instrument for.</para>
/// <para>The one sentence that comes close is deliberate. Telling somebody that
/// a fading signal is not their fault declines to blame them rather than
/// asserting anything about the ionosphere, and the person this is written for
/// has spent six years assuming every problem is theirs. Warmth never buys a
/// claim (§0.7), and this buys none: it makes no statement about whether
/// conditions are good, bad, opening or closing.</para>
/// <para>Written as connected speech, because the reader is sitting at a radio
/// being told something by a friend rather than reading a status code.</para>
/// </remarks>
public static class CwNotes
{
    /// <summary>The words for a note, or empty when there is nothing to say.</summary>
    /// <param name="note">What the decoder noticed.</param>
    /// <returns>One short passage, or an empty string.</returns>
    public static string Text(CwNote note) => note switch
    {
        CwNote.Fading =>
            "Their signal keeps rising and falling, so letters will come and go. "
            + "That is the band doing it, not you and not your radio.",

        CwNote.TooFast =>
            "They are sending faster than Hamlet is following, so this will come "
            + "out patchy. Asking somebody to slow down is completely normal and "
            + "every operator has done it. The way you ask is QRS, and most people "
            + "will drop their speed happily.",

        CwNote.Weak =>
            "The tone is only just above the noise, so some of it will not resolve "
            + "and Hamlet will leave those places marked rather than filling them "
            + "in. A faint signal is the most ordinary thing there is on the bands.",

        CwNote.NothingHeard =>
            "Nothing is coming through just now. Hamlet is listening for a Morse "
            + "note somewhere between 300 and 900 Hz, so if the dial is not sitting "
            + "on a signal there is nothing yet for it to hear. Try moving slowly "
            + "across the band and watch this space.",

        _ => "",
    };
}

/// <summary>
/// Watches how the signal has been behaving, so the terminal can say why a
/// decode is going badly.
/// </summary>
/// <remarks>
/// <para>Everything here is a measurement taken from the decoder's own chain:
/// how far each character stood above the noise, how much that moved, how many
/// characters failed to resolve, and how long it has been since anything was
/// heard at all. Nothing is inferred about the world outside the audio.</para>
/// <para>The window is short on purpose. A note that described the last ten
/// minutes would still be apologizing for a fade that ended a minute ago, and
/// the operator would learn to ignore it.</para>
/// </remarks>
public sealed class CwSignalWatch
{
    /// <summary>How many recent characters the struggle count rests on.</summary>
    public const int WindowSize = 12;

    /// <summary>
    /// How many measurements taken while the key was down the strength history
    /// keeps.
    /// </summary>
    /// <remarks>
    /// Two hundred and forty of them at five milliseconds each is a bit under
    /// half a minute of keying, which spans a slow fade without still
    /// apologizing for one that ended a minute ago.
    /// </remarks>
    private const int StrengthWindow = 240;

    /// <summary>Below this many characters there is not enough to judge from.</summary>
    private const int MinimumSample = 6;

    /// <summary>Below this many strength readings there is nothing to compare.</summary>
    private const int MinimumStrengthSample = 40;

    /// <summary>Nothing decoded for this long means nothing is coming through.</summary>
    private const double NothingHeardSeconds = 6.0;

    /// <summary>A swing this wide in signal strength is a fade.</summary>
    private const double FadeSwingDb = 10.0;

    /// <summary>Above this speed the decoder starts losing timing resolution.</summary>
    private const int FastWpm = 35;

    /// <summary>The share of recent characters that must be failing to matter.</summary>
    private const double StruggleShare = 0.25;

    /// <summary>Median strength below this counts as weak.</summary>
    private const double WeakMedianDb = 11.0;

    /// <summary>How often the verdict is recomputed, in measurements.</summary>
    private const int RecomputeEveryReadings = 40;

    private readonly double[] _strength = new double[StrengthWindow];
    private readonly double[] _sorted = new double[StrengthWindow];
    private readonly bool[] _struggled = new bool[WindowSize];

    private int _strengthCount;
    private int _strengthWrite;
    private int _count;
    private int _write;
    private int _wordsPerMinute;
    private int _readingsSinceRecompute;
    private double _secondsSinceCharacter;

    /// <summary>What the decoder has noticed.</summary>
    public CwNote Note { get; private set; } = CwNote.None;

    /// <summary>The words for it, or empty.</summary>
    public string NoteText => CwNotes.Text(Note);

    /// <summary>True once a character has been decoded at all.</summary>
    public bool HasDecodedAnything => _count > 0;

    /// <summary>
    /// Take account of one measurement of the tone.
    /// </summary>
    /// <param name="gate">What the gate decided.</param>
    /// <param name="hopSamples">How many samples this measurement advanced by.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <remarks>
    /// STRENGTH IS SAMPLED WHILE THE KEY IS DOWN and at no other time. During a
    /// gap the tracked peak decays toward the noise, so a history that included
    /// the silences would show every word break as a fade and would say so.
    /// A measurement taken during a mark is the signal, and nothing else.
    /// </remarks>
    public void Observe(GateReading gate, int hopSamples, int sampleRate)
    {
        if (sampleRate > 0)
        {
            _secondsSinceCharacter += (double)hopSamples / sampleRate;
        }

        if (gate.KeyDown && gate.HasSignal)
        {
            _strength[_strengthWrite] = gate.SignalToNoiseDb;
            _strengthWrite = (_strengthWrite + 1) % StrengthWindow;
            _strengthCount = Math.Min(_strengthCount + 1, StrengthWindow);
        }

        if (++_readingsSinceRecompute < RecomputeEveryReadings)
        {
            return;
        }

        _readingsSinceRecompute = 0;
        Recompute();
    }

    /// <summary>Take account of one decoded character.</summary>
    /// <param name="character">The character.</param>
    public void Observe(CwCharacter character)
    {
        _secondsSinceCharacter = 0;

        if (character.IsWordGap)
        {
            // A gap between words says nothing about how the signal is doing.
            return;
        }

        _struggled[_write] = character.Confidence != CwConfidence.High;
        _write = (_write + 1) % WindowSize;
        _count = Math.Min(_count + 1, WindowSize);
        _wordsPerMinute = character.WordsPerMinute;

        Recompute();
    }

    /// <summary>
    /// Decide what, if anything, is worth saying.
    /// </summary>
    /// <remarks>
    /// Ordered by what the reader can do about it. Silence first, because
    /// nothing else applies when there is nothing there. Fading next, because it
    /// explains letters appearing and disappearing and there is nothing to be
    /// done about it. Then speed, because asking somebody to slow down is the
    /// one thing that always works. Weakness last, because it is the case where
    /// the honest answer is that this is simply what a faint signal looks like.
    /// </remarks>
    private void Recompute()
    {
        // NOTHING DECODED, rather than no tone detected. Noise crosses a
        // threshold constantly, so "no tone" would almost never be true on a
        // real receiver, and the honest statement is about what came through
        // rather than about what crossed a threshold.
        if (_secondsSinceCharacter >= NothingHeardSeconds)
        {
            Note = CwNote.NothingHeard;
            return;
        }

        if (_count < MinimumSample)
        {
            Note = CwNote.None;
            return;
        }

        var struggling = 0;

        for (var i = 0; i < _count; i++)
        {
            if (_struggled[i])
            {
                struggling++;
            }
        }

        var strugglingShare = (double)struggling / _count;

        if (_strengthCount >= MinimumStrengthSample && Swing() >= FadeSwingDb)
        {
            Note = CwNote.Fading;
            return;
        }

        // Only when the speed is actually costing something. A clean decode at
        // forty words a minute needs no apology, and a note that fired on speed
        // alone would be nagging rather than explaining.
        if (_wordsPerMinute >= FastWpm && strugglingShare >= StruggleShare)
        {
            Note = CwNote.TooFast;
            return;
        }

        if (_strengthCount >= MinimumStrengthSample
            && Median() <= WeakMedianDb
            && strugglingShare >= StruggleShare)
        {
            Note = CwNote.Weak;
            return;
        }

        Note = CwNote.None;
    }

    /// <summary>
    /// The middle signal strength across the history.
    /// </summary>
    /// <remarks>
    /// The median rather than the mean, because a few measurements caught at
    /// the top of a fade would drag an average up and hide a signal that is
    /// otherwise down in the noise. Copied into a buffer allocated once and
    /// sorted there, at most five times a second.
    /// </remarks>
    private double Median()
    {
        Sort();
        return _sorted[_strengthCount / 2];
    }

    /// <summary>
    /// How far the signal strength has moved, from the tenth percentile to the
    /// ninetieth.
    /// </summary>
    /// <remarks>
    /// PERCENTILES RATHER THAN THE FULL RANGE, because the extremes are not
    /// evidence. The tracker is still settling on the first mark after silence,
    /// and one reading caught mid-settle would report every transmission as a
    /// fade. The middle eighty per cent is what the signal is actually doing:
    /// a steady one holds inside a decibel or two, and one rising and falling
    /// on the band spreads across ten or more.
    /// </remarks>
    private double Swing()
    {
        Sort();

        var low = _sorted[_strengthCount / 10];
        var high = _sorted[_strengthCount - 1 - (_strengthCount / 10)];

        return high - low;
    }

    private void Sort()
    {
        Array.Copy(_strength, _sorted, _strengthCount);
        Array.Sort(_sorted, 0, _strengthCount);
    }
}
