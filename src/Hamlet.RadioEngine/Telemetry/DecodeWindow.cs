using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>Why something heard was not turned into a character.</summary>
public enum DecodeRejection
{
    /// <summary>The timings did not look like Morse at all.</summary>
    NotMorseTiming,

    /// <summary>It resolved, and not well enough to show.</summary>
    BelowConfidence,

    /// <summary>More than one signal was in the passband.</summary>
    Contested,

    /// <summary>The tone could not be tracked.</summary>
    NoTone,
}

/// <summary>
/// What the decoder has been hearing, gathered over an interval (HM-DEC-077).
/// </summary>
/// <remarks>
/// <para>THE LEAST CAPTURED THING IN THE APPLICATION, AND THE PRIME DIRECTIVE IS
/// ABOUT IT. Two hours of listening to 20 meters produced five events, every one
/// of them "decoder_started", and no record at all of what was heard, what was
/// rejected, or what the noise floor did. The decoder already computes every one
/// of these numbers; nothing was asking it for them.</para>
/// <para>COUNTS AND MEASUREMENTS, NEVER TEXT (HM-DEC-018). This has no member
/// that can hold a decoded character, a word or a message. The shape refuses
/// rather than the call site remembering, which is the same reasoning that put
/// every payload in one class.</para>
/// <para>AGGREGATED RATHER THAN PER CHARACTER, because at forty characters a
/// second a line each would bury the file and cost the hot path. Counting is
/// four increments on fields that already exist, so the audio thread allocates
/// nothing: the dictionary is built once, when the window closes.</para>
/// </remarks>
public sealed class DecodeWindow
{
    private int _high;
    private int _low;
    private int _unreadable;
    private int _notMorse;
    private int _belowConfidence;
    private int _contested;
    private int _noTone;
    private double _noiseFloorSum;
    private int _noiseFloorCount;
    private double _pitchSum;
    private int _pitchCount;
    private double _pitchMin = double.MaxValue;
    private double _pitchMax = double.MinValue;
    private int _wpmSum;
    private int _wpmCount;

    /// <summary>How often the window closes and writes, by default.</summary>
    /// <remarks>
    /// Thirty seconds. Frequent enough that a two-hour session has a couple of
    /// hundred rows describing what it heard, and rare enough that those rows
    /// are worth reading one at a time.
    /// </remarks>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(30);

    /// <summary>True when nothing happened worth writing.</summary>
    public bool IsEmpty
        => _high == 0 && _low == 0 && _unreadable == 0
           && _notMorse == 0 && _belowConfidence == 0 && _contested == 0
           && _noTone == 0 && _noiseFloorCount == 0;

    /// <summary>
    /// The level this window deserves.
    /// </summary>
    /// <remarks>
    /// A window that rejected everything it heard is worth finding by scanning,
    /// which is what levels are for (HM-DEC-077). A quiet band is not: hearing
    /// nothing is the ordinary state of a receiver.
    /// </remarks>
    public TelemetryLevel Level
    {
        get
        {
            var emitted = _high + _low;
            var rejected = _notMorse + _belowConfidence + _contested + _noTone;

            return rejected > 0 && emitted == 0
                ? TelemetryLevel.Warn
                : TelemetryLevel.Info;
        }
    }

    /// <summary>Count one character the decoder emitted.</summary>
    /// <param name="confidence">How sure it was.</param>
    /// <remarks>
    /// THE HOT PATH, and it is three comparisons and one increment. Nothing is
    /// allocated here, nothing is boxed, and no string is built: at speed this
    /// runs about forty times a second and a decoder that stutters to write its
    /// own diagnostics has traded the thing for the record of the thing.
    /// </remarks>
    public void Emitted(CwConfidence confidence)
    {
        switch (confidence)
        {
            case CwConfidence.High:
                _high++;
                break;

            case CwConfidence.Low:
                _low++;
                break;

            default:
                _unreadable++;
                break;
        }
    }

    /// <summary>Count something heard that produced no character.</summary>
    /// <param name="why">The reason.</param>
    public void Rejected(DecodeRejection why)
    {
        switch (why)
        {
            case DecodeRejection.NotMorseTiming:
                _notMorse++;
                break;

            case DecodeRejection.BelowConfidence:
                _belowConfidence++;
                break;

            case DecodeRejection.Contested:
                _contested++;
                break;

            default:
                _noTone++;
                break;
        }
    }

    /// <summary>Record the conditions the decoder is working in.</summary>
    /// <param name="noiseFloorDb">The noise floor.</param>
    /// <param name="pitchHz">The tone being tracked, or null when none is.</param>
    /// <param name="wpm">The speed, or null.</param>
    public void Observed(double noiseFloorDb, double? pitchHz, int? wpm)
    {
        _noiseFloorSum += noiseFloorDb;
        _noiseFloorCount++;

        if (pitchHz is { } pitch)
        {
            _pitchSum += pitch;
            _pitchCount++;
            _pitchMin = Math.Min(_pitchMin, pitch);
            _pitchMax = Math.Max(_pitchMax, pitch);
        }

        if (wpm is { } speed)
        {
            _wpmSum += speed;
            _wpmCount++;
        }
    }

    /// <summary>
    /// The payload, built once when the window closes.
    /// </summary>
    /// <returns>Counts and measurements. Never text.</returns>
    public IReadOnlyDictionary<string, object?> ToBag()
    {
        var bag = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["emittedHigh"] = _high,
            ["emittedLow"] = _low,
            ["emittedUnreadable"] = _unreadable,
            ["rejectedNotMorse"] = _notMorse,
            ["rejectedBelowConfidence"] = _belowConfidence,
            ["rejectedContested"] = _contested,
            ["rejectedNoTone"] = _noTone,
            ["toneTracked"] = _pitchCount > 0,
        };

        if (_noiseFloorCount > 0)
        {
            bag["noiseFloorDb"] = Math.Round(_noiseFloorSum / _noiseFloorCount, 1);
        }

        if (_pitchCount > 0)
        {
            bag["pitchHz"] = Math.Round(_pitchSum / _pitchCount, 1);

            // THE DRIFT, WHICH IS THE NUMBER THAT EXPLAINS A BAD EVENING. A
            // tracker wandering two hundred hertz across a window was chasing
            // something that was not one signal.
            bag["pitchDriftHz"] = Math.Round(_pitchMax - _pitchMin, 1);
        }

        if (_wpmCount > 0)
        {
            bag["wpm"] = _wpmSum / _wpmCount;
        }

        return bag;
    }

    /// <summary>Start again, keeping the instance.</summary>
    /// <remarks>
    /// Reset rather than a fresh object, so a session running for hours does not
    /// hand the collector one of these every thirty seconds.
    /// </remarks>
    public void Reset()
    {
        _high = _low = _unreadable = 0;
        _notMorse = _belowConfidence = _contested = _noTone = 0;
        _noiseFloorSum = 0;
        _noiseFloorCount = 0;
        _pitchSum = 0;
        _pitchCount = 0;
        _pitchMin = double.MaxValue;
        _pitchMax = double.MinValue;
        _wpmSum = 0;
        _wpmCount = 0;
    }
}
