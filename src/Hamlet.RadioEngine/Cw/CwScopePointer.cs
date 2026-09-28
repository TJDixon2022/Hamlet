using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Where the radio's scope says the station is, at one frame.</summary>
/// <param name="PitchHz">The station's beat note: the peak's offset from the dial plus the CW pitch.</param>
/// <param name="PeakHz">The peak's radio frequency.</param>
/// <param name="Level">The peak's height on the radio's own 0 to 160 scope scale.</param>
/// <param name="AtUtc">When the frame arrived.</param>
public readonly record struct CwScopePeak(double PitchHz, long PeakHz, int Level, DateTime AtUtc);

/// <summary>
/// **THE RADIO POINTS** (work instruction 480 task 2, R94, HM-DEC-188): the peak the IC-7300's
/// own spectrum scope reports inside its filter, turned into the pitch the Morse detector and
/// the tracker watch.
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"You have a waterfall. Why aren't we using that? You can read any
/// settings from the radio."*</para>
/// <para>**WHAT IT DOES.** In CW the radio's filter sits on the dial and passes a station the
/// dial is on as a beat note at the CW pitch; a station 300 Hz above the dial beats at the pitch
/// plus 300. So each frame is searched for its tallest bin between the dial minus and plus half
/// the filter width, and that bin's offset from the dial, added to the CW pitch, is the pitch the
/// detector watches. No sweep, no tolerance and no sample rate: the frame's own span places every
/// bin.</para>
/// <para>**THE PEAK IS THE TALLEST BIN AND NOTHING ELSE - AUTHOR'S, OVERRULABLE.** Where two bins
/// tie the lower one wins, so the same frame always points at the same pitch. On an empty band
/// the tallest bin is noise; the detector watching it finds no bars, and the graph draws
/// nothing, which is the honest picture.</para>
/// <para>**QUIET AFTER <see cref="ScopeFlow.QuietAfter"/>.** When no frame has arrived for three
/// seconds the pointer says nothing, and the detector goes back to its own sweep.</para>
/// <para>**THE LEVEL IS THE RADIO'S SCALE, NOT DECIBELS.** The waveform arrives as 0 to 160 and
/// nothing in this tree ties that scale to decibels, so it is carried as the radio sent it.</para>
/// <para>**THREAD**: frames arrive on the radio's read thread and the pointer is read from the
/// UI's, so both go under one lock.</para>
/// </remarks>
public sealed class CwScopePointer
{
    /// <summary>How far back <see cref="FramesLast4s"/> counts, in seconds.</summary>
    public const double CountSeconds = 4;

    private readonly object _gate = new();
    private readonly Queue<DateTime> _arrivals = new();
    private CwScopePeak? _last;

    /// <summary>
    /// The tallest bin between the dial minus and plus half the filter, as a pitch.
    /// </summary>
    /// <param name="frame">The radio's frame.</param>
    /// <param name="dialHz">The dial, in hertz.</param>
    /// <param name="cwPitchHz">The radio's CW pitch, in hertz.</param>
    /// <param name="filterWidthHz">The radio's filter width, in hertz.</param>
    /// <returns>The peak, or null where the frame has no bin inside the filter.</returns>
    public static CwScopePeak? Peak(in SpectrumFrame frame, double dialHz, double cwPitchHz, double filterWidthHz)
    {
        if (frame.Bins.Length == 0 || frame.SpanHz <= 0 || !(filterWidthHz > 0) || !(cwPitchHz > 0))
        {
            return null;
        }

        var low = dialHz - (filterWidthHz / 2);
        var high = dialHz + (filterWidthHz / 2);
        var best = -1;
        var bestLevel = -1;

        for (var i = 0; i < frame.Bins.Length; i++)
        {
            var hz = frame.BinCenterHz(i);

            if (hz < low || hz > high)
            {
                continue;
            }

            if (frame.Bins[i] > bestLevel)
            {
                best = i;
                bestLevel = frame.Bins[i];
            }
        }

        if (best < 0)
        {
            return null;
        }

        var peakHz = frame.BinCenterHz(best);

        // Back from the waterfall's 0 to 255 to the radio's own 0 to 160 (CivScope.Scale).
        var level = (int)Math.Round(bestLevel * 160.0 / 255.0);

        return new CwScopePeak(cwPitchHz + (peakHz - dialHz), peakHz, level, frame.TimestampUtc);
    }

    /// <summary>Take one of the radio's frames.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="dialHz">The dial, in hertz.</param>
    /// <param name="cwPitchHz">The radio's CW pitch, in hertz.</param>
    /// <param name="filterWidthHz">The radio's filter width, in hertz.</param>
    /// <param name="nowUtc">When it arrived.</param>
    public void Observe(in SpectrumFrame frame, double dialHz, double cwPitchHz, double filterWidthHz, DateTime nowUtc)
    {
        var peak = Peak(frame, dialHz, cwPitchHz, filterWidthHz);

        lock (_gate)
        {
            _arrivals.Enqueue(nowUtc);
            Trim(nowUtc);

            if (peak is { } found)
            {
                _last = found with { AtUtc = nowUtc };
            }
        }
    }

    /// <summary>The last peak, while the scope is not quiet; null otherwise.</summary>
    /// <param name="nowUtc">The time now.</param>
    /// <returns>The peak, or null.</returns>
    public CwScopePeak? Pointing(DateTime nowUtc)
    {
        lock (_gate)
        {
            return _last is { } last && nowUtc - last.AtUtc <= ScopeFlow.QuietAfter ? last : null;
        }
    }

    /// <summary>How many frames arrived in the last <see cref="CountSeconds"/>.</summary>
    /// <param name="nowUtc">The time now.</param>
    /// <returns>The count.</returns>
    public int FramesLast4s(DateTime nowUtc)
    {
        lock (_gate)
        {
            Trim(nowUtc);
            return _arrivals.Count;
        }
    }

    /// <summary>Forget everything: the mode or the radio changed.</summary>
    public void Reset()
    {
        lock (_gate)
        {
            _arrivals.Clear();
            _last = null;
        }
    }

    private void Trim(DateTime nowUtc)
    {
        while (_arrivals.Count > 0 && (nowUtc - _arrivals.Peek()).TotalSeconds > CountSeconds)
        {
            _arrivals.Dequeue();
        }
    }
}
