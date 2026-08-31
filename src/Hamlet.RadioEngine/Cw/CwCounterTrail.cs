namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// What the decoder's counters read at a moment on the audio clock.
/// </summary>
/// <param name="SamplesSeen">How much audio had reached the decoder by then.</param>
/// <param name="ElementsSeen">Marks and gaps measured by then.</param>
/// <param name="ElementsResolved">How many of those had become characters.</param>
/// <param name="CharactersEmitted">Characters that had reached the screen.</param>
/// <param name="CharactersUnsure">How many of those were marked or blocked.</param>
public readonly record struct CwCounterSample(
    long SamplesSeen,
    int ElementsSeen,
    int ElementsResolved,
    int CharactersEmitted,
    int CharactersUnsure);

/// <summary>
/// What the decoder did across one stretch of audio, rather than since it
/// started.
/// </summary>
/// <param name="Samples">How long the stretch was, in samples.</param>
/// <param name="ElementsSeen">Marks and gaps measured inside it.</param>
/// <param name="ElementsResolved">How many of those became characters.</param>
/// <param name="CharactersEmitted">Characters emitted inside it.</param>
/// <param name="CharactersUnsure">How many of those were marked or blocked.</param>
public readonly record struct CwCounterDelta(
    long Samples,
    int ElementsSeen,
    int ElementsResolved,
    int CharactersEmitted,
    int CharactersUnsure);

/// <summary>
/// A short history of the decoder's own counters, so a figure can be quoted for
/// a stretch of audio instead of for a whole evening (HM-DEC-091).
/// </summary>
/// <remarks>
/// <para>**THE COUNTERS ARE CUMULATIVE AND NOTHING SAID SO.** `ElementsSeen`,
/// `ElementsResolved`, `CharactersEmitted` and `CharactersUnsure` are fields on
/// the decoder that only ever go up, from the moment listening started until it
/// stops. A capture sidecar printed them beside thirty seconds of audio, and a
/// reader takes a number sitting next to a recording to be about the recording.
/// A capture written after seven hours of listening carried a character count
/// earned hours earlier on a different band.</para>
/// <para>**A LABEL WOULD HAVE BEEN ENOUGH TO STOP THE HARM AND NOT ENOUGH TO
/// ANSWER THE QUESTION.** What anybody wants to know of a recording is what
/// Hamlet made of *it*, and that is derivable: the counters are monotonic and the
/// tap counts the samples, so the figure for a stretch is the difference between
/// the readings at its two ends. This keeps those readings.</para>
/// <para>**THE CLOCK IS THE AUDIO'S AND NOT THE WALL'S**, because a stalled
/// pipeline moves one and not the other, and the window being asked about is a
/// window of audio.</para>
/// <para>Sampled from the same timer that refreshes the readouts, so the two ends
/// of a window are each accurate to one tick. The seed makes the start of
/// listening a real sample: at nought samples the decoder had done nothing, which
/// is a measurement rather than an assumption, and it is what lets a recording
/// made in the first half minute be covered at all.</para>
/// </remarks>
public sealed class CwCounterTrail
{
    private readonly List<CwCounterSample> _samples = new() { default };
    private readonly long _span;

    /// <summary>Keep a history reaching this far back.</summary>
    /// <param name="spanSamples">How much audio the trail must cover.</param>
    /// <exception cref="ArgumentOutOfRangeException">A span of nothing.</exception>
    public CwCounterTrail(long spanSamples)
    {
        if (spanSamples <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spanSamples), spanSamples, "A trail must cover some audio.");
        }

        _span = spanSamples;
    }

    /// <summary>How many readings are being kept.</summary>
    public int Count => _samples.Count;

    /// <summary>Record where the counters stand now.</summary>
    /// <param name="sample">The reading.</param>
    /// <remarks>
    /// A reading that is older than the newest one is dropped rather than
    /// inserted: the audio clock only goes forward, and a decoder that was
    /// restarted gets a fresh trail rather than a repaired one.
    /// </remarks>
    public void Note(CwCounterSample sample)
    {
        if (sample.SamplesSeen < _samples[^1].SamplesSeen)
        {
            return;
        }

        _samples.Add(sample);
        Trim();
    }

    /// <summary>
    /// Where the counters stood at or before a point on the audio clock.
    /// </summary>
    /// <param name="samplesSeen">The point.</param>
    /// <returns>The reading, or null when the trail does not reach that far back.</returns>
    public CwCounterSample? At(long samplesSeen)
    {
        CwCounterSample? found = null;

        foreach (var sample in _samples)
        {
            if (sample.SamplesSeen > samplesSeen)
            {
                break;
            }

            found = sample;
        }

        return found;
    }

    /// <summary>
    /// What the decoder did across the last stretch of audio.
    /// </summary>
    /// <param name="endSamplesSeen">Where the stretch ends on the audio clock.</param>
    /// <param name="windowSamples">How long the stretch is.</param>
    /// <returns>
    /// The difference across it, or **null when the trail cannot cover it** —
    /// which is an honest unknown and not a zero (§0.0).
    /// </returns>
    public CwCounterDelta? Over(long endSamplesSeen, long windowSamples)
    {
        if (windowSamples <= 0)
        {
            return null;
        }

        // **A WINDOW CANNOT REACH BACK PAST THE START OF LISTENING**, because
        // there was no audio there. A recording taken in the first half minute is
        // shorter than the tap's own length, and clamping here is what makes it
        // answerable rather than an unknown that only looks like caution.
        var end = At(endSamplesSeen);
        var start = At(Math.Max(0, endSamplesSeen - windowSamples));

        if (end is not { } last || start is not { } first)
        {
            return null;
        }

        // **COUNTERS THAT WENT BACKWARDS MEAN THEY WERE RESET INSIDE THIS
        // WINDOW, AND A DELTA ACROSS A RESET IS NOT A DELTA** (work instruction
        // 055, task 1). `CwDecoder.Retuned` zeroes them when the operator moves,
        // because a count earned on another frequency does not belong on this
        // sheet — and this trail keeps its samples from before that, so
        // subtracting one from the other produced a negative.
        //
        // **IT REACHED THE OPERATOR.** `cw-2026-08-31-003229`'s sidecar reads
        // `inThis -250 characters emitted, -96 unsure, -466 elements seen, -466
        // resolved`. That is the sheet he diagnoses everything with, lying about
        // arithmetic.
        //
        // **SAYING SO IS THE ANSWER RATHER THAN CLAMPING TO ZERO** (§0.0). Nought
        // characters in this recording and a window that cannot be measured are
        // different facts, and the second is the true one here.
        if (last.ElementsSeen < first.ElementsSeen
            || last.ElementsResolved < first.ElementsResolved
            || last.CharactersEmitted < first.CharactersEmitted
            || last.CharactersUnsure < first.CharactersUnsure)
        {
            return null;
        }

        return new CwCounterDelta(
            windowSamples,
            last.ElementsSeen - first.ElementsSeen,
            last.ElementsResolved - first.ElementsResolved,
            last.CharactersEmitted - first.CharactersEmitted,
            last.CharactersUnsure - first.CharactersUnsure);
    }

    /// <summary>
    /// Drop what is older than the span, keeping the one reading behind it.
    /// </summary>
    /// <remarks>
    /// **THE SAMPLE JUST BEHIND THE HORIZON IS THE ONE A WINDOW STARTS FROM.**
    /// Dropping it because it is old is how a trail that nominally covers thirty
    /// seconds stops being able to answer about thirty seconds.
    /// </remarks>
    private void Trim()
    {
        var horizon = _samples[^1].SamplesSeen - _span;
        var keepFrom = 0;

        for (var i = 0; i < _samples.Count; i++)
        {
            if (_samples[i].SamplesSeen > horizon)
            {
                break;
            }

            keepFrom = i;
        }

        if (keepFrom > 0)
        {
            _samples.RemoveRange(0, keepFrom);
        }
    }
}
