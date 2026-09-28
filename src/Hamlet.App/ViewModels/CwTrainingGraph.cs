using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.ViewModels;

/// <summary>One bar the training graph draws.</summary>
/// <param name="StartUtc">When it started.</param>
/// <param name="EndUtc">When it ended.</param>
/// <param name="LengthMs">Its length.</param>
/// <param name="Dah">Whether it reads as a dah beside the other bars in the window.</param>
public sealed record CwGraphBar(DateTime StartUtc, DateTime EndUtc, double LengthMs, bool Dah);

/// <summary>One settled character the training graph writes above its bars.</summary>
/// <param name="StartUtc">When its span started; its end where it carried no span.</param>
/// <param name="EndUtc">When its span ended.</param>
/// <param name="Text">What the decoder settled.</param>
/// <param name="Confidence">The decoder's class for it.</param>
/// <param name="Probability">The probability it is right, or NaN where the decoder did not measure it.</param>
/// <param name="HasSpan">Whether the decoder said how long it was; false draws it at its end.</param>
public sealed record CwGraphLetter(
    DateTime StartUtc, DateTime EndUtc, string Text, CwConfidence Confidence, double Probability, bool HasSpan);

/// <summary>What the training graph draws: the last <see cref="CwTrainingGraph.WindowSeconds"/>.</summary>
/// <param name="NowUtc">The right-hand edge.</param>
/// <param name="Bars">Every bar in the window, oldest first.</param>
/// <param name="Letters">Every settled character in the window, oldest first.</param>
public sealed record CwTrainingFrame(DateTime NowUtc, IReadOnlyList<CwGraphBar> Bars, IReadOnlyList<CwGraphLetter> Letters)
{
    /// <summary>Nothing found.</summary>
    public static CwTrainingFrame Empty { get; } =
        new(DateTime.MinValue, Array.Empty<CwGraphBar>(), Array.Empty<CwGraphLetter>());

    /// <summary>True while there is nothing to draw: no bars and no letters.</summary>
    public bool Listening => Bars.Count == 0 && Letters.Count == 0;
}

/// <summary>
/// **BARS, AND THE LETTERS OVER THEM** (work instruction 480 task 3, R95, HM-DEC-188): the last
/// eight seconds of the bars the envelope detector found and the characters the decoder settled,
/// on one clock.
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"When we start to detect bars, I want to graph those. As we start
/// to find letters, mark them in that graph and put the letter over top. This is a passive
/// training tool for learning how to read CW."*</para>
/// <para>**ONE CLOCK, THE WALL'S.** The detector's newest hop is now and each hop before it one
/// hop earlier; a settled character ends as far behind now as the decoder has heard past its
/// <see cref="CwCharacter.At"/>, and starts its span before that. The detector holds four
/// seconds and revises its last one as bars pair up, so every update replaces what those four
/// seconds hold and keeps what is older, up to eight.</para>
/// <para>**A LETTER IS KEPT ONLY WHERE THE DECODER SETTLED IT** (§0.0): nothing is inferred
/// from the bars, and a word gap is kept as nothing at all.</para>
/// <para>**DIT OR DAH IS FOR THE HOVER, AND IT IS THE AUTHOR'S - OVERRULABLE**: a bar more than
/// twice the shortest bar in the window reads as a dah, the rest as dits. It decides nothing.</para>
/// <para>**THREAD**: characters settle on the audio thread and the frame is read on the UI's, so
/// both go under one lock.</para>
/// <para>**THE TRACE CAME BACK** (work instruction 480, the letter over the bars, R94): *"I want
/// to see the flat oscilloscope shapes with the letter over top."* R95's bars-only graph drew
/// nothing where the detector marked nothing, which on the owner's tab was the whole panel. The
/// level of every hop is kept beside the bars, on the same clock and the same eight seconds, the
/// newest four replaced each update as the bars are.</para>
/// <para>**EIGHT SECONDS, NOT FOUR, AND THE AUTHOR'S - OVERRULABLE**: a character settles about a
/// second after its last element and its span reaches back two thirds of a second more at twenty
/// words a minute, so in four seconds a letter would be on screen for two.</para>
/// </remarks>
public sealed class CwTrainingGraph
{
    /// <summary>How much the graph shows, in seconds: long enough for a word.</summary>
    public const double WindowSeconds = 8;

    private readonly object _gate = new();
    private readonly List<(DateTime Start, DateTime End)> _bars = new();
    private readonly List<CwGraphLetter> _letters = new();

    /// <summary>How many settled characters carried no span and were drawn at their end.</summary>
    public int WithoutSpan { get; private set; }

    /// <summary>Take the detector's last four seconds.</summary>
    /// <param name="hops">Its history, oldest first, the newest ending now.</param>
    /// <param name="hopMs">One hop, in milliseconds.</param>
    /// <param name="nowUtc">Now.</param>
    public void Update(IReadOnlyList<CwScopeHop> hops, double hopMs, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(hops);

        if (!(hopMs > 0))
        {
            return;
        }

        DateTime At(int index) => nowUtc - TimeSpan.FromMilliseconds((hops.Count - index) * hopMs);

        var windowStart = At(0);

        lock (_gate)
        {
            // What the detector still holds it may still revise; what is older is kept.
            _bars.RemoveAll(b => b.Start >= windowStart || b.End < nowUtc.AddSeconds(-WindowSeconds));

            var start = -1;

            for (var i = 0; i <= hops.Count; i++)
            {
                var mark = i < hops.Count && hops[i].Mark;

                if (mark && start < 0)
                {
                    start = i;
                }
                else if (!mark && start >= 0)
                {
                    var bar = (Start: At(start), End: At(i));

                    // A bar the window's old edge cut through is the kept one carrying on.
                    var carried = start == 0
                        ? _bars.FindIndex(b => b.End >= windowStart - TimeSpan.FromMilliseconds(hopMs))
                        : -1;

                    if (carried >= 0)
                    {
                        _bars[carried] = (_bars[carried].Start, bar.End);
                    }
                    else
                    {
                        _bars.Add(bar);
                    }

                    start = -1;
                }
            }

            _bars.Sort((a, b) => a.Start.CompareTo(b.Start));
            _letters.RemoveAll(l => l.EndUtc < nowUtc.AddSeconds(-WindowSeconds));
        }
    }

    /// <summary>Take a character the decoder settled.</summary>
    /// <param name="character">The character.</param>
    /// <param name="heard">How much audio the decoder had heard when it settled.</param>
    /// <param name="nowUtc">Now.</param>
    public void Settle(CwCharacter character, TimeSpan heard, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (character.IsWordGap || string.IsNullOrWhiteSpace(character.Text))
        {
            return;
        }

        var end = nowUtc - (heard > character.At ? heard - character.At : TimeSpan.Zero);
        var hasSpan = character.SpanHops > 0;
        var start = hasSpan
            ? end - TimeSpan.FromMilliseconds(character.SpanHops * CwProbabilisticDecoder.HopMilliseconds)
            : end;

        lock (_gate)
        {
            if (!hasSpan)
            {
                WithoutSpan++;
            }

            _letters.Add(new CwGraphLetter(
                start, end, character.Text, character.Confidence, character.Probability, hasSpan));
        }
    }

    /// <summary>Forget everything: the station, the band or the mode changed.</summary>
    public void Clear()
    {
        lock (_gate)
        {
            _bars.Clear();
            _letters.Clear();
        }
    }

    /// <summary>What to draw now.</summary>
    /// <param name="nowUtc">Now, the right-hand edge.</param>
    /// <returns>The frame.</returns>
    public CwTrainingFrame Frame(DateTime nowUtc)
    {
        lock (_gate)
        {
            var from = nowUtc.AddSeconds(-WindowSeconds);
            var bars = _bars.Where(b => b.End >= from).ToList();
            var shortest = bars.Count > 0 ? bars.Min(b => (b.End - b.Start).TotalMilliseconds) : 0;

            return new CwTrainingFrame(
                nowUtc,
                bars.Select(b =>
                {
                    var ms = (b.End - b.Start).TotalMilliseconds;
                    return new CwGraphBar(b.Start, b.End, ms, ms > 2 * shortest);
                }).ToList(),
                _letters.Where(l => l.EndUtc >= from).OrderBy(l => l.StartUtc).ToList());
        }
    }
}
