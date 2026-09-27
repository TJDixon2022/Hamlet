using Hamlet.RadioEngine.Cw.Second;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// One character the fldigi port printed, as the arbiter takes it: its text and
/// class as `parity.md` maps them, its p, and its span on the audio clock.
/// </summary>
/// <param name="Text">What it printed, or <see cref="MorseAlphabet.Unreadable"/> for fldigi's no-match output.</param>
/// <param name="Confidence">High for a character, Unreadable for the no-match output (458 DECIDED (2)); fldigi prints nothing dim.</param>
/// <param name="Pattern">The dots and dashes it looked up.</param>
/// <param name="P">Its p, <see cref="FldigiConfidence"/>'s map.</param>
/// <param name="Start">Its first mark's start, seconds on the audio clock; NaN where its key events could not be found.</param>
/// <param name="End">Its last mark's end, likewise.</param>
/// <param name="WordsPerMinute">The port's receive speed when it printed.</param>
public sealed record CwSecondReading(
    string Text, CwConfidence Confidence, string Pattern, double P, double Start, double End, int WordsPerMinute)
{
    /// <summary>True where both ends are known.</summary>
    public bool HasSpan => double.IsFinite(Start) && double.IsFinite(End) && End >= Start;

    /// <summary>
    /// True where the port printed a word space since its last character: parity.md
    /// section 1's word boundary, kept so the port alone can be emitted and scored
    /// with its own boundaries (HM-REQ-128; work instruction 466). The arbiter does
    /// not read it.
    /// </summary>
    public bool WordGapBefore { get; init; }
}

/// <summary>
/// Reads the port's characters, with span and p, from what it already exposes, a
/// batch at a time (work instruction 465, tasks 1 to 3; HM-REQ-120, 124).
/// </summary>
/// <remarks>
/// <para>**NOTHING UNDER <c>Cw/Second/</c> CHANGES FOR THIS** (HM-REQ-122, 129).
/// It reads the port's public <see cref="FldigiCwDecoder.Decisions"/>,
/// <see cref="FldigiCwDecoder.KeyEvents"/> and <see cref="FldigiCwDecoder.Emissions"/>,
/// so the port must run with <see cref="FldigiCwDecoder.TraceDecisions"/> on.</para>
/// <para>**THE SPAN** (465 task 1): each up event placed on the decision row it was
/// raised in; the character ends at its last up event and starts at its first less
/// that element's length, both less the filter's <see cref="FilterLag"/>. The up
/// events are the last <c>Representation.Length</c> before the emission, the last
/// carrying its representation, as <see cref="FldigiConfidence"/> takes them.</para>
/// <para>**THE P** is <see cref="FldigiConfidence.Of"/> on the same margins
/// <see cref="FldigiConfidence.Read"/> forms, read incrementally so a live port is
/// not re-read from its first sample each time.</para>
/// </remarks>
public sealed class CwSecondHarvester
{
    /// <summary>How far the port's filter output lags its input, in 8000 Hz samples.</summary>
    public const int FilterLag = 512;

    private readonly double _offsetSeconds;
    private List<(FldigiCwKeyEvent Event, long Row)> _ups = new();
    private int _row;
    private int _key;
    private int _emission;
    private bool _spaced;

    /// <summary>Creates a harvester for one port run.</summary>
    /// <param name="offsetSeconds">Where the port's first sample sits on the audio clock.</param>
    public CwSecondHarvester(double offsetSeconds = 0)
        => _offsetSeconds = offsetSeconds;

    /// <summary>Every reading of a whole port run.</summary>
    /// <param name="port">A port run with its decisions traced.</param>
    /// <returns>One reading per character printed, word spaces left out.</returns>
    public static IReadOnlyList<CwSecondReading> Of(FldigiCwDecoder port) => new CwSecondHarvester().Take(port);

    /// <summary>The readings the port has printed since the last call.</summary>
    /// <param name="port">The same port each call, run with its decisions traced.</param>
    /// <returns>The new readings, in order; word spaces left out, each marked on the reading after it (<see cref="CwSecondReading.WordGapBefore"/>).</returns>
    public IReadOnlyList<CwSecondReading> Take(FldigiCwDecoder port)
    {
        ArgumentNullException.ThrowIfNull(port);

        if (!port.TraceDecisions && port.Emissions.Count > _emission)
        {
            throw new InvalidOperationException("the port must run with TraceDecisions on: only its decision rows order and place its key events");
        }

        var found = new List<CwSecondReading>();
        var rows = port.Decisions;

        for (; _row < rows.Count; _row++)
        {
            var row = rows[_row];

            foreach (var token in row.Events.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (token is not ("down" or "up" or "spike" or "overflow"))
                {
                    continue;
                }

                var ev = port.KeyEvents[_key++];

                if (ev.Kind == "up")
                {
                    _ups.Add((ev, row.FilteredSample));
                }
            }

            if (row.Printed.Length == 0)
            {
                continue;
            }

            var e = port.Emissions[_emission++];

            if (e.Text != MorseAlphabet.WordGap)
            {
                found.Add(Reading(e) with { WordGapBefore = _spaced });
                _spaced = false;
            }
            else
            {
                _spaced = true;
            }

            _ups = new List<(FldigiCwKeyEvent, long)>();
        }

        return found;
    }

    private CwSecondReading Reading(FldigiCwEmission e)
    {
        var want = e.Representation.Length;
        var ups = _ups.Skip(Math.Max(0, _ups.Count - want)).ToList();
        var placeholder = e.Text == NoMatch;
        var text = placeholder ? MorseAlphabet.Unreadable : e.Text;
        var confidence = placeholder ? CwConfidence.Unreadable : CwConfidence.High;

        if (want == 0 || ups.Count != want || ups[^1].Event.Representation != e.Representation)
        {
            return new CwSecondReading(text, confidence, e.Representation, FldigiConfidence.Of(double.NaN, double.NaN),
                double.NaN, double.NaN, e.ReceiveSpeed);
        }

        var last = ups[^1].Event;
        var level = last.NoiseFloor > 0 && last.SigAvg > 0
            ? 20 * Math.Log10(last.SigAvg / last.NoiseFloor)
            : double.NaN;
        var timing = ups.Min(u => u.Event.TwoDots > 0 ? Math.Abs(u.Event.Element - u.Event.TwoDots) / (u.Event.TwoDots / 2.0) : double.NaN);
        var rate = (double)FldigiCwDecoder.CW_SAMPLERATE;
        var start = _offsetSeconds + ((ups[0].Row - ups[0].Event.Element - FilterLag) / rate);
        var end = _offsetSeconds + ((ups[^1].Row - FilterLag) / rate);

        return new CwSecondReading(text, confidence, e.Representation, FldigiConfidence.Of(level, timing), start, end, e.ReceiveSpeed);
    }

    /// <summary>fldigi's no-match output with the shipped defaults, `*` (configuration.h:249-251; parity.md section 1).</summary>
    public const string NoMatch = "*";
}
