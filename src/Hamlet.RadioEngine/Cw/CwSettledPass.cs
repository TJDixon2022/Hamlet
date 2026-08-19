using System.Text;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One character as the settled pass read it, with the span it covers.
/// </summary>
/// <param name="Text">What it read.</param>
/// <param name="Pattern">The dots and dashes it measured.</param>
/// <param name="Score">How far it stands behind it, from 0 to 1.</param>
/// <param name="FirstSample">Where the character began.</param>
/// <param name="LastSample">Where it ended.</param>
/// <param name="WordsPerMinute">The speed the settled clock was running at.</param>
/// <param name="Truncated">
/// True when the operator's own transmission cut part of it away, so it may
/// never be rendered as a letter (HM-DEC-095).
/// </param>
public readonly record struct SettledCharacter(
    string Text,
    string Pattern,
    double Score,
    long FirstSample,
    long LastSample,
    int WordsPerMinute,
    bool Truncated);

/// <summary>
/// Why the settled pass declined to read a stretch of audio.
/// </summary>
public enum SettledRefusal
{
    /// <summary>It read it.</summary>
    None,

    /// <summary>Not enough audio has arrived behind the cursor yet.</summary>
    NotYet,

    /// <summary>The two levels in the window were less than six decibels apart.</summary>
    Contrast,

    /// <summary>The mark lengths did not cluster into a clock anybody could send.</summary>
    Clock,

    /// <summary>
    /// The marks arriving stopped fitting the clock that was running, and no new
    /// one describes them either (HM-DEC-096, phase 2).
    /// </summary>
    ClockLost,
}

/// <summary>
/// What one settling attempt produced.
/// </summary>
/// <param name="Refusal">Why nothing was read, or <see cref="SettledRefusal.None"/>.</param>
/// <param name="WindowSeconds">How much audio the threshold was fitted over.</param>
/// <param name="WindowWasCapped">
/// True when the window wanted to be longer and hit the ceiling, so the fit is
/// weaker than the speed asked for.
/// </param>
/// <param name="ContrastDb">How far the two levels sat apart.</param>
/// <param name="DitMilliseconds">The settled clock's dit, or zero.</param>
/// <param name="SpeedChanged">
/// True when this window's clock is a different speed from the last one, which
/// on the air usually means a different station started sending.
/// </param>
public readonly record struct SettledOutcome(
    SettledRefusal Refusal,
    double WindowSeconds,
    bool WindowWasCapped,
    double ContrastDb,
    double DitMilliseconds,
    bool SpeedChanged)
{
    /// <summary>True when the pass read something.</summary>
    public bool Read => Refusal == SettledRefusal.None;
}

/// <summary>
/// Reads the audio a second time, a few seconds behind, with a threshold fitted
/// to the stretch it is reading (HM-DEC-096, amendment phase 1).
/// </summary>
/// <remarks>
/// <para>**HAMLET DECODES TWICE AND THE TWO PASSES ARE NOT RIVALS.** The
/// streaming gate answers as each element ends, which is what the operator needs
/// while somebody is calling him, and it has to decide where the threshold is
/// before it has heard the stretch the threshold describes. This pass runs
/// behind it and has the whole stretch, so it can fit the threshold to what
/// actually arrived.</para>
/// <para>The validated reference chain works this way and could not simply be
/// ported: it fits a threshold to a block and applies it to that same block,
/// which a decoder that must answer at the leading edge cannot do. Grafting its
/// gate onto the streaming chain made a real recording measurably worse. Running
/// both, and letting the settled reading firm up behind the provisional one, is
/// what keeps the leading edge live without pretending it is final.</para>
/// <para>**A TRAILING WINDOW PER CHARACTER, NOT BLOCKS.** Blocks have seams, and
/// an element straddling a seam gets one threshold applied to its start and
/// another to its end. A window that ends where the reading ends has no seam to
/// reason about. Nothing is cached: "the audio has not changed materially" needs
/// a definition, and a wrong one silently reintroduces a stale threshold, which
/// is the shape of the faults this repository keeps finding.</para>
/// <para>**THE WINDOW IS THE LONGER OF ABOUT TWO AND A HALF SECONDS AND ABOUT
/// THIRTY ELEMENTS**, because both constraints are real and they bind at
/// opposite ends of the speed range: the time is what spans a fade, and the
/// element count is what makes fitting two clusters stable. Past four seconds it
/// stops, whatever the speed asks for. A settled line exists to catch a callsign
/// in a live contact, and a six-second lag makes it useless for the one thing it
/// is for, so a weaker fit that arrives is worth more than a better one that
/// does not. **When the ceiling binds, the outcome says so**, because a degraded
/// measurement announced is honest and the same measurement concealed is not
/// (§0.0).</para>
/// </remarks>
public sealed class CwSettledPass
{
    /// <summary>The shortest the fitting window may be, in seconds.</summary>
    public const double ShortestWindowSeconds = 2.5;

    /// <summary>The longest it may be, however slow the sending is.</summary>
    public const double LongestWindowSeconds = 4.0;

    /// <summary>How many elements the window tries to span.</summary>
    public const int WindowElements = 30;

    /// <summary>How far behind the leading edge the settled reading runs.</summary>
    /// <remarks>
    /// Half a second past the end of the window, so a character sitting on the
    /// newest edge of the fit is not read from a window that only just contains
    /// it.
    /// </remarks>
    public const double TrailSeconds = 0.5;

    /// <summary>How far apart the two levels must sit before anything is read.</summary>
    public const double MinimumContrastDb = 6.0;

    /// <summary>How far apart the opening and closing decisions sit.</summary>
    private const double HysteresisDb = 6.0;

    /// <summary>
    /// How far below the keyed level half its amplitude sits, in decibels.
    /// </summary>
    /// <remarks>
    /// Six, because six decibels is a factor of two in amplitude and a shaped
    /// element crosses half its own height at its true edge. See
    /// <see cref="DecideAt"/> for why this is where the decision goes rather than
    /// midway between the two levels found.
    /// </remarks>
    private const double HalfAmplitudeDb = 6.0;

    /// <summary>How close to the operator's own transmission spoils a mark.</summary>
    /// <remarks>Sixty milliseconds, from the validated reference chain.</remarks>
    private const double TruncationBorderSeconds = 0.060;

    /// <summary>The shortest dit anybody sends, in milliseconds.</summary>
    public const double ShortestDitMs = 30;

    /// <summary>The longest, in milliseconds.</summary>
    public const double LongestDitMs = 350;

    /// <summary>
    /// How far marks may sit from a clock before it stops describing them.
    /// </summary>
    /// <remarks>
    /// About a third in log units, which is a mark landing roughly forty
    /// percent away from the length it was called. Real sending wanders well
    /// inside that; two stations at different speeds in one window do not.
    /// </remarks>
    private const double WorstAcceptableFit = 0.34;

    /// <summary>How much a clock has to move to count as a different speed.</summary>
    /// <remarks>
    /// A quarter. Real fists wander a few percent and a different operator is
    /// usually a different speed entirely, so this sits well above the first and
    /// well below the second.
    /// </remarks>
    public const double SpeedChangeFraction = 0.25;

    private readonly int _sampleRate;
    private readonly double _hopSeconds;
    private readonly int _capacity;

    private readonly float[] _db;
    private readonly bool[] _blocked;
    private readonly long[] _sample;

    private readonly double[] _scratch;
    private readonly bool[] _key;
    private readonly bool[] _voted;
    private readonly double[] _marks;
    private readonly double[] _gaps;
    private readonly bool[] _markTruncated;
    private readonly bool[] _markAtEdge;

    /// <summary>How much silence the window saw after its last mark, in ms.</summary>
    private double _tailSilenceMs;

    /// <summary>Every gap this signal has produced, newest overwriting oldest.</summary>
    /// <remarks>
    /// Five hundred and twelve is about two minutes of a traffic net at
    /// twenty-one words a minute, which is far more than the classes need and
    /// short enough that a station handing over to a different fist is forgotten
    /// within a couple of overs.
    /// </remarks>
    private readonly double[] _gapHistory = new double[512];

    private int _gapWrite;
    private int _gapsRemembered;
    private long _gapsRememberedThrough = -1;

    /// <summary>
    /// What this sender's spacing actually is, or null (HM-DEC-115).
    /// </summary>
    /// <remarks>
    /// Exposed so a surface can say it. A Farnsworth sender should be visible
    /// rather than merely survived, and these numbers answer "why does this look
    /// wrong" long before anybody thinks to ask the question.
    /// </remarks>
    public CwGapClasses? Classes { get; private set; }
    private readonly long[] _markStart;
    private readonly long[] _markEnd;

    private readonly StringBuilder _pattern = new();

    private int _write;
    private int _fill;
    private long _settledThrough = -1;
    private double _lastDitMs;
    private double _previousDitMs;
    private double _previousDahMs;

    /// <summary>Creates a settled pass.</summary>
    /// <param name="sampleRate">Samples per second of the audio.</param>
    /// <param name="hopSeconds">How long one envelope point covers.</param>
    /// <param name="historySeconds">How much audio to keep.</param>
    public CwSettledPass(int sampleRate, double hopSeconds, double historySeconds = 10.0)
    {
        _sampleRate = Math.Max(1_000, sampleRate);
        _hopSeconds = hopSeconds > 0 ? hopSeconds : 0.01;
        _capacity = Math.Max(256, (int)Math.Round(historySeconds / _hopSeconds));

        _db = new float[_capacity];
        _blocked = new bool[_capacity];
        _sample = new long[_capacity];

        _scratch = new double[Math.Max(_capacity, _gapHistory.Length)];
        _key = new bool[_capacity];
        _voted = new bool[_capacity];
        _marks = new double[_capacity];
        _gaps = new double[_capacity];
        _markTruncated = new bool[_capacity];
        _markAtEdge = new bool[_capacity];
        _markStart = new long[_capacity];
        _markEnd = new long[_capacity];
    }

    /// <summary>How far the settled reading has got, as a sample index.</summary>
    public long SettledThrough => _settledThrough;

    /// <summary>
    /// Record one point of the detection envelope.
    /// </summary>
    /// <param name="powerDb">Energy at the tracked pitch, in decibels.</param>
    /// <param name="blocked">True when the operator was transmitting.</param>
    /// <param name="sampleIndex">Where this point sits in the stream.</param>
    public void Observe(double powerDb, bool blocked, long sampleIndex)
    {
        _db[_write] = (float)powerDb;
        _blocked[_write] = blocked;
        _sample[_write] = sampleIndex;
        _write = (_write + 1) % _capacity;
        _fill = Math.Min(_fill + 1, _capacity);
    }

    /// <summary>Forget everything, because the tracker moved.</summary>
    public void Reset()
    {
        _write = 0;
        _fill = 0;
        _settledThrough = -1;
        _lastDitMs = 0;
        _previousDitMs = 0;
        _previousDahMs = 0;

        // **THE GAP CLASSES BELONG TO A SENDER AND GO WITH THE SENDER**
        // (HM-DEC-095, HM-DEC-115). Fitting them over a longer history is what
        // lets word gaps be seen at all, and the cost is that the history can
        // span a handover. Two operators' spacing averaged together describes
        // neither, which is the fault this ruling exists to fix wearing
        // different clothes. This runs when the tracker moves or the clock is
        // lost, which is operationally somebody else starting to transmit.
        _gapsRemembered = 0;
        _gapWrite = 0;
        _gapsRememberedThrough = -1;
        Classes = null;
    }

    /// <summary>
    /// Read whatever is now far enough behind the leading edge.
    /// </summary>
    /// <param name="ditMillisecondsHint">
    /// What the streaming pass believes a dit is, used only to size the window.
    /// The clock this pass decodes with is fitted here, not taken from there.
    /// </param>
    /// <param name="into">Characters are appended here, oldest first.</param>
    /// <returns>What happened, including why nothing was read.</returns>
    /// <param name="drain">
    /// True when the audio has ended, so there is no leading edge left to keep
    /// clear of and the last few seconds may be read.
    /// </param>
    public SettledOutcome Settle(
        double ditMillisecondsHint, List<SettledCharacter> into, bool drain = false)
    {
        ArgumentNullException.ThrowIfNull(into);

        if (_fill < 32)
        {
            return new SettledOutcome(SettledRefusal.NotYet, 0, false, 0, 0, false);
        }

        // **THE WINDOW SIZE IS A REQUIREMENT, NOT A TUNING KNOB.** Thirty elements
        // at the speed being sent, or two and a half seconds, whichever is
        // longer, and never past four.
        var elementSeconds = ditMillisecondsHint > 0
            ? WindowElements * 2 * ditMillisecondsHint / 1000.0
            : 0;

        var wanted = Math.Max(ShortestWindowSeconds, elementSeconds);
        var capped = wanted > LongestWindowSeconds;
        var windowSeconds = Math.Min(wanted, LongestWindowSeconds);

        var points = (int)Math.Round(windowSeconds / _hopSeconds);

        if (points > _fill)
        {
            return new SettledOutcome(
                SettledRefusal.NotYet, windowSeconds, capped, 0, 0, false);
        }

        // The reading stops short of the leading edge, so a character on the very
        // newest sample is not judged from a window that barely contains it.
        //
        // **EXCEPT WHEN THE AUDIO HAS ENDED.** A recording's last few seconds are
        // exactly where a station finishes its callsign, and holding them back
        // for a leading edge that will never arrive loses the part the operator
        // most needed.
        var trail = drain ? 0 : (int)Math.Round(TrailSeconds / _hopSeconds);

        if (_fill < points + trail)
        {
            return new SettledOutcome(
                SettledRefusal.NotYet, windowSeconds, capped, 0, 0, false);
        }

        var newest = _fill;
        var last = newest - trail;
        var first = last - points;

        if (first < 0)
        {
            return new SettledOutcome(
                SettledRefusal.NotYet, windowSeconds, capped, 0, 0, false);
        }

        if (!FitLevels(first, last, out var low, out var high))
        {
            return new SettledOutcome(
                SettledRefusal.Contrast, windowSeconds, capped, 0, 0, false);
        }

        var contrast = high - low;

        if (contrast < MinimumContrastDb)
        {
            return new SettledOutcome(
                SettledRefusal.Contrast, windowSeconds, capped, contrast, 0, false);
        }

        Gate(first, last, DecideAt(low, high));

        // Twenty milliseconds before there is a clock to size it from.
        Deglitch(first, last, 0.020);

        var count = Runs(first, last);
        var clock = FitClock(count);

        if (clock is not var (ditMs, dahMs) || ditMs <= 0)
        {
            return new SettledOutcome(
                SettledRefusal.Clock, windowSeconds, capped, contrast, 0, false);
        }

        // And four tenths of a dit once there is one, which is the reference
        // chain's own figure and is what removes the chatter a marginal signal
        // produces without touching a real element.
        Deglitch(first, last, 0.4 * ditMs / 1000.0);
        count = Runs(first, last);

        var refit = FitClock(count);

        // **THE CLOCK THAT WAS RUNNING GETS A HEARING** (HM-DEC-096, phase 2). A
        // fade or a burst of somebody else's keying can break a window's fit
        // without anything having changed about the station being read, and a
        // refusal that fires on every fade is worse than useless. So the fresh
        // fit and the one already running are both measured against the marks
        // that actually arrived, and whichever describes them better is used.
        var fresh = refit is var (freshDit, freshDah) && freshDit > 0
            ? ((double Dit, double Dah)?)(freshDit, freshDah)
            : null;

        var carried = _previousDitMs > 0
            ? ((double Dit, double Dah)?)(_previousDitMs, _previousDahMs)
            : null;

        var chosen = Better(count, fresh, carried);

        if (chosen is not var (ditFinal, dahFinal) || ditFinal <= 0)
        {
            // Neither the new fit nor the old one describes what arrived. That is
            // clock loss: emit nothing and re-acquire, because a two-means fit
            // over a mixture of two stations lands inside the legal ratio band
            // while describing neither of them, and that is a confident wrong
            // answer (§0.0).
            _previousDitMs = 0;
            _previousDahMs = 0;

            return new SettledOutcome(
                SettledRefusal.ClockLost, windowSeconds, capped, contrast, 0, false);
        }

        // **A GENUINE SPEED CHANGE IS A FACT ABOUT THE AIR AND IS ANNOTATED.** In
        // a contact it usually means a different station started transmitting,
        // which is the earliest evidence there is that somebody answered.
        var speedChanged = _lastDitMs > 0
            && Math.Abs(ditFinal - _lastDitMs) / _lastDitMs > SpeedChangeFraction;

        _lastDitMs = ditFinal;
        _previousDitMs = ditFinal;
        _previousDahMs = dahFinal;

        // **ONLY WHAT IS NEW SINCE LAST TIME** (HM-DEC-096, phase 1). The window
        // is four seconds long and is read twice a second, so every character
        // sits in about eight consecutive windows. Emitting the whole window each
        // time repeated the callsign eight times over and looked exactly like a
        // decoder hallucinating on noise.
        // **THE CURSOR FOLLOWS WHAT WAS ACTUALLY READ**, not the end of the
        // window. Marks touching the window's newest edge are held for the next
        // window rather than published unread, so advancing the cursor past them
        // would lose them for good.
        var readThrough = Emit(
            count, ditFinal, dahFinal, contrast, _settledThrough, drain, into);

        if (readThrough > _settledThrough)
        {
            _settledThrough = readThrough;
        }

        return new SettledOutcome(
            SettledRefusal.None, windowSeconds, capped, contrast, ditFinal, speedChanged);
    }

    /// <summary>Where a logical position sits in the ring.</summary>
    private int Index(int logical)
    {
        var start = _fill < _capacity ? 0 : _write;
        return (start + logical) % _capacity;
    }

    /// <summary>
    /// Two levels in this window, seeded from its own percentiles.
    /// </summary>
    private bool FitLevels(int first, int last, out double low, out double high)
    {
        low = high = 0;

        var count = 0;

        for (var i = first; i < last; i++)
        {
            var at = Index(i);

            if (!_blocked[at])
            {
                _scratch[count++] = _db[at];
            }
        }

        if (count < 20)
        {
            return false;
        }

        Array.Sort(_scratch, 0, count);

        low = _scratch[(int)(count * 0.15)];
        high = _scratch[(int)(count * 0.85)];

        for (var pass = 0; pass < 15; pass++)
        {
            double lowSum = 0, highSum = 0;
            int lowCount = 0, highCount = 0;

            for (var i = 0; i < count; i++)
            {
                if (Math.Abs(_scratch[i] - low) <= Math.Abs(_scratch[i] - high))
                {
                    lowSum += _scratch[i];
                    lowCount++;
                }
                else
                {
                    highSum += _scratch[i];
                    highCount++;
                }
            }

            if (lowCount > 0)
            {
                low = lowSum / lowCount;
            }

            if (highCount > 0)
            {
                high = highSum / highCount;
            }
        }

        return true;
    }

    /// <summary>
    /// Where to put the decision, given the two levels found (HM-DEC-105).
    /// </summary>
    /// <param name="low">The band between elements.</param>
    /// <param name="high">The level while the key is down.</param>
    /// <returns>The level in decibels to decide against.</returns>
    /// <remarks>
    /// <para>**SIX DECIBELS BELOW THE KEYED LEVEL IS HALF AMPLITUDE, AND HALF
    /// AMPLITUDE IS WHERE AN ELEMENT'S TRUE EDGE IS.** A keyed element is read
    /// through a window fifty milliseconds long, so its envelope rises and falls
    /// over about that; the level chosen to decide at is therefore what decides
    /// how long the mark measures. Deciding halfway up the element's own height
    /// puts the crossing at its real edge on both sides, and the mark measures
    /// what it was.</para>
    /// <para>**MIDWAY BETWEEN THE TWO CLUSTERS IS NOT THAT, AND THE ERROR GROWS
    /// WITH THE SIGNAL.** On a strong signal the midpoint sits far down the
    /// element's leading edge, so the gate opens early and shuts late and every
    /// mark reads long by a constant. Adding a constant to a dit and a dah
    /// compresses their ratio, which is how a fist sending at a true 2.9 came to
    /// be measured at 2.35 and refused by a floor of 2.5 — while the same fist at
    /// the same true timing, ten decibels weaker, measured 2.79 and was read.
    /// **A decoder that reads a signal better as it gets worse has the wrong
    /// question at the bottom of it.**</para>
    /// <para>The floor stays at 2.5 and what the ratio is computed over is what
    /// changed. Where the contrast is small the midpoint is already at or above
    /// half amplitude and nothing moves, which is why the weak tiers that were
    /// measuring correctly are untouched. This is the same reasoning
    /// <see cref="CwGate"/> has carried since HM-DEC-088 and it had never reached
    /// the passes that fit a threshold to a window.</para>
    /// </remarks>
    private static double DecideAt(double low, double high)
        => high - Math.Min((high - low) / 2, HalfAmplitudeDb);

    /// <summary>Decide the key state across the window, with hysteresis.</summary>
    private void Gate(int first, int last, double middle)
    {
        var open = middle + (HysteresisDb / 2);
        var shut = middle - (HysteresisDb / 2);
        var on = false;

        for (var i = first; i < last; i++)
        {
            var at = Index(i);

            if (_blocked[at])
            {
                on = false;
                _key[i - first] = false;
                continue;
            }

            if (on && _db[at] < shut)
            {
                on = false;
            }
            else if (!on && _db[at] > open)
            {
                on = true;
            }

            _key[i - first] = on;
        }
    }

    /// <summary>Remove anything too short to be an element.</summary>
    private void Deglitch(int first, int last, double shortestSeconds)
    {
        var span = last - first;
        var width = Math.Max(1, (int)Math.Round(shortestSeconds / _hopSeconds));

        if (width % 2 == 0)
        {
            width++;
        }

        if (width < 3)
        {
            return;
        }

        var half = width / 2;

        for (var i = 0; i < span; i++)
        {
            var down = 0;
            var seen = 0;

            for (var k = -half; k <= half; k++)
            {
                var at = i + k;

                if (at < 0 || at >= span)
                {
                    continue;
                }

                seen++;

                if (_key[at])
                {
                    down++;
                }
            }

            _voted[i] = down * 2 > seen;
        }

        Array.Copy(_voted, _key, span);
    }

    /// <summary>Measure every mark and gap in the window.</summary>
    private int Runs(int first, int last)
    {
        var span = last - first;
        var count = 0;
        var i = 0;
        var lastMarkHop = 0;
        var border = Math.Max(1, (int)Math.Round(TruncationBorderSeconds / _hopSeconds));

        while (i < span && count < _marks.Length)
        {
            if (!_key[i])
            {
                i++;
                continue;
            }

            var start = i;

            while (i < span && _key[i])
            {
                i++;
            }

            // **A MARK BORDERING THE OPERATOR'S OWN TRANSMISSION IS NOT A MARK**
            // (HM-DEC-095). What is audible between his elements is a sliver of
            // somebody else's, cut at both ends by him.
            //
            // **A MARK TOUCHING THE WINDOW'S EDGE IS A DIFFERENT THING ENTIRELY
            // AND WAS BEING TREATED AS THE SAME ONE** (HM-DEC-107 phase 4). The
            // window is a view onto a stream, not the stream: a mark at its edge
            // is complete on the air and merely not wholly inside this view. It
            // was being rendered as a placeholder, and because the window is read
            // half a second behind the leading edge, **the marks at that edge are
            // precisely the ones about to be emitted**. The settled pass was
            // marking unreadable the characters it existed to settle.
            //
            // The remedy is to hold them for the next window, where the same
            // marks sit in the interior, rather than to publish them as unread.
            var truncated = false;
            var atEdge = i >= span;

            for (var k = Math.Max(0, start - border); k < start && !truncated; k++)
            {
                truncated = _blocked[Index(first + k)];
            }

            for (var k = i; k < Math.Min(span, i + border) && !truncated; k++)
            {
                truncated = _blocked[Index(first + k)];
            }

            lastMarkHop = i;
            _marks[count] = (i - start) * _hopSeconds * 1000;
            _markTruncated[count] = truncated;
            _markAtEdge[count] = atEdge;
            _markStart[count] = _sample[Index(first + start)];
            _markEnd[count] = _sample[Index(first + i - 1)];

            _gaps[count] = 0;

            if (count > 0)
            {
                var previousEnd = _markEnd[count - 1];
                _gaps[count - 1] =
                    (double)(_markStart[count] - previousEnd) / _sampleRate * 1000;

                // **THE GAP CLASSES ARE FITTED PER SIGNAL, NOT PER WINDOW**
                // (HM-DEC-115). A settled window is a few seconds, which holds
                // plenty of element gaps, a handful of character gaps and
                // often no word gap at all, so three classes cannot be found
                // inside one however cleanly they separate over the whole
                // transmission. The measurement that produced this ruling was
                // over thirty seconds: 69 element gaps, 28 character and 11
                // word.
                //
                // Windows overlap, so a gap is remembered once, the first time
                // its own mark is seen.
                if (_markStart[count] > _gapsRememberedThrough)
                {
                    _gapsRememberedThrough = _markStart[count];
                    _gapHistory[_gapWrite] = _gaps[count - 1];
                    _gapWrite = (_gapWrite + 1) % _gapHistory.Length;
                    _gapsRemembered = Math.Min(_gapsRemembered + 1, _gapHistory.Length);
                }
            }

            count++;
        }

        // **HOW MUCH SILENCE THE WINDOW SAW AFTER ITS LAST MARK.** Without this
        // the gap after the final mark was infinity, which is a claim that the
        // character certainly ended there, and the window has no business making
        // it: the window is a view onto a stream, so silence that has not been
        // observed yet is silence nobody has measured (§0.0).
        _tailSilenceMs = (span - lastMarkHop) * _hopSeconds * 1000;

        return count;
    }

    /// <summary>
    /// Which of two clocks describes the marks that actually arrived
    /// (HM-DEC-096, phase 2).
    /// </summary>
    /// <param name="count">How many marks were measured.</param>
    /// <param name="fresh">The clock just fitted to this window, if any.</param>
    /// <param name="carried">The clock that was already running, if any.</param>
    /// <returns>The better fit, or null when neither describes them.</returns>
    /// <remarks>
    /// Scored by how far each mark sits from the nearer of that clock's two
    /// lengths, in log units so a dit and a dah are weighed the same way. A
    /// clock nothing sits near is not a clock, whatever its ratio was.
    /// </remarks>
    private (double Dit, double Dah)? Better(
        int count, (double Dit, double Dah)? fresh, (double Dit, double Dah)? carried)
    {
        if (carried is null)
        {
            return fresh;
        }

        if (fresh is null)
        {
            return Fits(count, carried.Value) <= WorstAcceptableFit ? carried : null;
        }

        var freshError = Fits(count, fresh.Value);
        var carriedError = Fits(count, carried.Value);

        if (Math.Min(freshError, carriedError) > WorstAcceptableFit)
        {
            return null;
        }

        return freshError <= carriedError ? fresh : carried;
    }

    /// <summary>How far the marks sit from a clock's two lengths, on average.</summary>
    private double Fits(int count, (double Dit, double Dah) clock)
    {
        var total = 0.0;
        var used = 0;

        for (var i = 0; i < count; i++)
        {
            if (_markTruncated[i] || _marks[i] <= 0)
            {
                continue;
            }

            var toDit = Math.Abs(Math.Log(_marks[i] / clock.Dit));
            var toDah = Math.Abs(Math.Log(_marks[i] / clock.Dah));

            total += Math.Min(toDit, toDah);
            used++;
        }

        return used == 0 ? double.MaxValue : total / used;
    }

    /// <summary>
    /// The element clock, or nothing when the marks do not describe one.
    /// </summary>
    /// <remarks>
    /// **A CLOCK THAT DOES NOT FIT IS A REFUSAL, NOT A BEST GUESS** (§0.0). Two
    /// stations at different speeds in one window produce a two-means fit that
    /// can land inside the legal ratio band while describing neither of them,
    /// which is a confident wrong answer, and this project fears that output more
    /// than silence.
    /// </remarks>
    private (double Dit, double Dah)? FitClock(int count)
    {
        var usable = 0;

        for (var i = 0; i < count; i++)
        {
            if (!_markTruncated[i])
            {
                _scratch[usable++] = _marks[i];
            }
        }

        if (usable < 8)
        {
            return null;
        }

        Array.Sort(_scratch, 0, usable);

        var low = _scratch[(int)(usable * 0.15)];
        var high = _scratch[(int)(usable * 0.85)];

        for (var pass = 0; pass < 15; pass++)
        {
            double lowSum = 0, highSum = 0;
            int lowCount = 0, highCount = 0;

            for (var i = 0; i < usable; i++)
            {
                if (Math.Abs(_scratch[i] - low) <= Math.Abs(_scratch[i] - high))
                {
                    lowSum += _scratch[i];
                    lowCount++;
                }
                else
                {
                    highSum += _scratch[i];
                    highCount++;
                }
            }

            if (lowCount == 0 || highCount == 0)
            {
                return null;
            }

            low = lowSum / lowCount;
            high = highSum / highCount;
        }

        var ratio = high / Math.Max(low, 1e-9);

        return ratio < CwToneSurvey.MinimumRatio || ratio > CwToneSurvey.MaximumRatio
            || low < ShortestDitMs || low > LongestDitMs
                ? null
                : (low, high);
    }

    /// <summary>
    /// Where this sender's gaps divide, taken from the gaps themselves.
    /// </summary>
    /// <remarks>
    /// The two widest multiplicative steps in the sorted gaps, which is the
    /// reference chain's method and handles a fist whose inter-element gaps are
    /// shorter than its own dits. Fixed multiples of a dit read such a fist as one
    /// unbroken run (HM-DEC-095).
    /// </remarks>
    /// <summary>
    /// Where this sender's own gaps divide, or null (HM-DEC-115).
    /// </summary>
    /// <returns>The classes, or null when the gaps do not form three groups.</returns>
    /// <remarks>
    /// <para>**FITTED PER SIGNAL RATHER THAN PER WINDOW**, which is what the
    /// ruling asks for and what a window cannot do. A settled window is a few
    /// seconds: plenty of element gaps, a handful of character gaps, and often
    /// no word gap at all. The measurement behind HM-DEC-115 ran over thirty
    /// seconds and found 69 element gaps, 28 character and 11 word, and three
    /// classes cannot be found inside a window holding one of the third kind
    /// however cleanly they separate over the whole transmission.</para>
    /// <para>The fit itself is <see cref="CwGapFit"/>, shared with the streaming
    /// estimator, because two copies of a classifier is two classifiers (§0).
    /// </para>
    /// </remarks>
    /// <summary>
    /// How many gaps this sender has contributed since the last reset.
    /// </summary>
    /// <remarks>
    /// **THE NUMBER THAT DECIDES WHETHER ANYTHING IS EMITTED AT ALL**, and until
    /// 2026-08-19 nothing could see it. Below `CwGapFit.LeastGaps` there are no
    /// classes, and with no classes `Emit` returns without producing a character
    /// however well the window read (HM-DEC-115). A pass that reports it read and
    /// emits nothing is indistinguishable from a broken one without this (§0.0.1).
    /// </remarks>
    public int GapsRemembered => _gapsRemembered;

    /// <summary>Temporary diagnostic.</summary>
    public IReadOnlyList<double> GapHistory
        => _gapHistory.Take(_gapsRemembered).ToArray();

    private CwGapClasses? GapCuts()
    {
        var usable = 0;

        for (var i = 0; i < _gapsRemembered; i++)
        {
            if (_gapHistory[i] > 0)
            {
                _scratch[usable++] = _gapHistory[i];
            }
        }

        return CwGapFit.Fit(_scratch, usable);
    }

    /// <summary>Turn the measured runs into characters.</summary>
    /// <returns>The sample the last emitted character ended on.</returns>
    /// <summary>
    /// How far a gap sat from the boundary it was judged against (HM-DEC-108).
    /// </summary>
    /// <param name="gapMs">The gap, in the same units as the cut.</param>
    /// <param name="elementCut">
    /// The boundary between a gap inside a character and a gap between two.
    /// </param>
    /// <returns>
    /// Nought where the gap landed on the boundary, one where it landed on
    /// either textbook spacing, and in between for everything else.
    /// </returns>
    /// <remarks>
    /// <para>**THE SAME SHAPE AS THE MARK MEASUREMENT, DELIBERATELY.** That one
    /// asks how far a mark sat from the dit-or-dah decision, on a scale where
    /// landing on the decision is nothing and landing on a textbook length is
    /// everything. This asks the same question of the gap that divided one
    /// character from the next.</para>
    /// <para>The scale is the textbook one-to-three: a gap inside a character is
    /// one dit and a gap between two is three, so their geometric midpoint sits
    /// a factor of the square root of three from each, and that factor is what
    /// full marks means here. The centre is the measured cut rather than the
    /// textbook midpoint, for the same reason the mark measurement centres on
    /// the measured dit and dah: the decision that was actually made is the one
    /// worth scoring the distance from.</para>
    /// <para>**A GAP NOBODY DECIDED ANYTHING ABOUT SCORES ONE.** The last
    /// character in a window is closed by running out of audio rather than by a
    /// judgement, and there is no evidence against it to record. Nothing here
    /// may raise a score (§0.0); it can only find one more way to lower one.
    /// </para>
    /// </remarks>
    public static double BoundaryMargin(double gapMs, double elementCut)
    {
        if (elementCut <= 0
            || gapMs <= 0
            || double.IsNaN(gapMs)
            || double.IsInfinity(gapMs)
            || gapMs >= double.MaxValue)
        {
            return 1.0;
        }

        var scale = Math.Log(Math.Sqrt(3.0));

        return Math.Min(1.0, Math.Abs(Math.Log(gapMs / elementCut)) / scale);
    }

    private long Emit(
        int count, double ditMs, double dahMs, double contrastDb,
        long after, bool drain, List<SettledCharacter> into)
    {
        // **NO CUTS MEANS NO TRANSCRIPT, NOT A GUESSED ONE** (HM-DEC-115). The
        // window has not seen enough gaps to say where this sender puts the
        // spaces, and guessing puts them in the wrong place with full
        // confidence. The marks stay for the next window, which is the same
        // remedy the window edge already uses.
        if (GapCuts() is not { } classes)
        {
            return after;
        }

        Classes = classes;

        var elementCut = classes.ElementCutMs;
        var characterCut = classes.CharacterCutMs;
        var middle = Math.Sqrt(ditMs * dahMs);
        var wpm = (int)Math.Round(1200.0 / ditMs);
        var signal = Math.Clamp((contrastDb - 6.0) / 14.0, 0, 1);

        _pattern.Clear();

        var worstTiming = 1.0;
        var tainted = false;
        long began = 0;

        // **THE THIRD MEASUREMENT** (HM-DEC-108). How far the gap that ended a
        // character sat from the boundary it was judged against. The two
        // existing scores are both about the elements, and the fault they could
        // not see is not about the elements at all: where the pass divides
        // characters in the wrong place a lone dah comes out as T and a lone dit
        // as E, with every element clean and the timing margin of a dah that
        // really is a dah equal to one.
        //
        // **BOTH BOUNDARIES OF A CHARACTER COUNT, NOT ONLY THE ONE THAT CLOSED
        // IT**, and that is a reading of the ruling rather than its literal
        // words. One gap misjudged produces two characters: the half in front of
        // it and the half behind. Scoring only the closing gap marks the first
        // half and leaves the second at full strength, and the second half is
        // the lone dah — it is the stranger the ruling names. Measured both ways
        // and the numbers are in OUTPUT.md.
        var openingMargin = 1.0;
        var pendingOpening = 1.0;
        var closingMargin = 1.0;

        var readThrough = after;

        for (var i = 0; i < count; i++)
        {
            // A character containing a mark that runs off the newest edge of the
            // window is not finished being observed. It is left for the next
            // window, where the same marks sit in the interior, and everything
            // after it is left with it so the cursor stays in one piece.
            //
            // **UNLESS THE AUDIO HAS ENDED, WHEN THERE IS NO NEXT WINDOW.** The
            // last few seconds of a recording are where a station finishes its
            // callsign, and holding them for a window that will never arrive
            // loses exactly the part the operator most needed.
            if (_markAtEdge[i] && !drain)
            {
                break;
            }

            if (_pattern.Length == 0)
            {
                began = _markStart[i];
                tainted = false;
                worstTiming = 1.0;
                openingMargin = pendingOpening;
                closingMargin = 1.0;
            }

            if (_markTruncated[i])
            {
                tainted = true;
            }

            _pattern.Append(_marks[i] < middle ? '.' : '-');

            // How far this mark sat from the dit-or-dah boundary, on a scale
            // where landing on the boundary is nothing and landing on either
            // textbook length is everything.
            var margin = Math.Min(
                1.0,
                Math.Abs(Math.Log(_marks[i] / middle)) / Math.Log(Math.Sqrt(dahMs / ditMs)));

            worstTiming = Math.Min(worstTiming, margin);

            var gap = i < count - 1 ? _gaps[i] : _tailSilenceMs;

            if (gap <= elementCut)
            {
                continue;
            }

            // **A CHARACTER THE WINDOW DID NOT SEE THE END OF IS NOT FINISHED
            // BEING OBSERVED**, and this is the fault the boundary measurement
            // was reaching for (HM-DEC-108). The gap after the last mark used to
            // be infinity, so whatever pattern had accumulated was flushed as a
            // whole character however little of it the window held. Every
            // stranger measured on these fixtures is that: the leading dashes of
            // the character that follows, emitted at full strength, with the
            // real character arriving whole in the next window right behind it.
            //
            // The mark-at-the-edge rule already holds a mark the key was still
            // down for. This is the same rule for the silence afterwards, which
            // nothing was watching, and the remedy is phase 4's: hold it for the
            // next window where it sits in the interior, rather than publish it.
            if (i == count - 1 && !drain && gap <= characterCut)
            {
                break;
            }

            closingMargin = BoundaryMargin(gap, elementCut);
            Flush(_markEnd[i]);
            pendingOpening = closingMargin;

            if (gap > characterCut && gap < double.MaxValue && _markEnd[i] > after)
            {
                into.Add(new SettledCharacter(
                    MorseAlphabet.WordGap, string.Empty, 1.0,
                    _markEnd[i], _markEnd[i], wpm, false));
            }
        }

        void Flush(long endedAt)
        {
            if (_pattern.Length == 0)
            {
                return;
            }

            var pattern = _pattern.ToString();
            var text = MorseAlphabet.Lookup(pattern);
            // **THE WORST OF THE THREE, NEVER THE AVERAGE**, which is what the
            // existing two already do and for the same reason: a character can
            // fail any one of these on its own and passing the other two does
            // not excuse it (HM-DEC-048, HM-DEC-108).
            var boundary = Math.Min(openingMargin, closingMargin);
            var score = tainted ? 0 : Math.Min(Math.Min(worstTiming, signal), boundary);

            if (endedAt <= after)
            {
                // Already read from an earlier window. The windows overlap by
                // design; the cursor is what stops them repeating themselves.
                _pattern.Clear();
                return;
            }

            into.Add(new SettledCharacter(
                text ?? MorseAlphabet.Unreadable,
                pattern,
                text is null ? 0 : score,
                began,
                endedAt,
                wpm,
                tainted));

            _pattern.Clear();
            readThrough = endedAt;
        }

        return readThrough;
    }
}
